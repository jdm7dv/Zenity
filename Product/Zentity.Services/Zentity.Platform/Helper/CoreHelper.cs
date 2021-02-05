// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Objects;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.ScholarlyWorks;
    using Zentity.Security.Authentication;
    using Zentity.Security.Authorization;
    using Zentity.Security.AuthorizationHelper;

    #region MetaDataProvider class

    /// <summary>
    /// Provide Core APIs support required by OAI-PMH
    /// </summary>
    internal class CoreHelper
    {
        #region Memeber Variables

        private static List<ResourceType> resourecTypes;
        private static Dictionary<string, List<string>> resourceTypeHierarchy = new Dictionary<string, List<string>>();
        int totalResourceCount;
        int actualRecordCount;
        int actualHarvestedCount;
        private readonly string entityConnectionString;
        private static Type[] systemResourceTypes;

        #endregion

        #region Constants

        #region Private

        #region Compile-LINQ Queries
        private static Func<ZentityContext, DateTime?> getEarliestDateStamp =
           CompiledQuery.Compile((ZentityContext _context) =>
           (_context.Resources.Select(tuple => tuple.DateAdded).Min()));

        #endregion

        #endregion

        #endregion

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHelper"/> class.
        /// </summary>
        public CoreHelper()
            : this(CoreHelper.DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHelper"/> class.
        /// </summary>
        /// <param name="entityConnectionString">The entity connection string.</param>
        public CoreHelper(string entityConnectionString)
        {
            this.entityConnectionString = entityConnectionString;
        }

        #endregion

        #region Internal properties

        /// <summary>
        /// Gets the system resource types.
        /// </summary>
        /// <value>The system resource types.</value>
        private static Type[] SystemResourceTypes
        {
            get
            {
                if(null == systemResourceTypes)
                {
                    LoadSystemResourceTypes();
                }

                return systemResourceTypes;
            }
        }

        /// <summary>
        /// Gets the default connection string.
        /// </summary>
        /// <value>The default connection string.</value>
        internal static string DefaultConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["CoreConnectionString"];
            }
        }

        /// <summary>
        /// Gets the total resource count.
        /// </summary>
        internal int ResourceCount
        {
            get
            {
                return this.totalResourceCount;
            }
        }

        /// <summary>
        /// Gets the total resource count also containing the count for OfType(Contact).
        /// </summary>
        internal int ActualResourceCount
        {
            get
            {
                return this.actualRecordCount;
            }
        }

        /// <summary>
        /// Gets the total resource count which have been harvested.
        /// </summary>
        internal int ActualHarvestedCount
        {
            get
            {
                return this.actualHarvestedCount;
            }
        }


        /// <summary>
        /// Gets or sets the list of all Resource Types.
        /// </summary>
        internal static List<ResourceType> ResourceTypes
        {
            get
            {
                if(null == CoreHelper.resourecTypes)
                {
                    using(ZentityContext context = new ZentityContext(CoreHelper.DefaultConnectionString))
                    {
                        List<ResourceType> resourceTypes = CoreHelper.GetResourceTypes(context);
                        CoreHelper.ResourceTypes = resourceTypes;
                    }
                }

                return CoreHelper.resourecTypes;
            }
            set
            {
                CoreHelper.resourecTypes = value;
            }
        }

        #endregion

        /// <summary>
        /// Loads the system resource types.
        /// </summary>
        private static void LoadSystemResourceTypes()
        {
            Type resourceType = typeof(Resource);
            systemResourceTypes = new Type[1];
            systemResourceTypes[0] = resourceType;
            string customAssemblyNames = ConfigurationManager.AppSettings["CustomResourceTypeAssemblies"];
            customAssemblyNames += ";Zentity.Core";

            if(string.IsNullOrEmpty(customAssemblyNames))
            {
                return;
            }

            string[] customAssemblies = customAssemblyNames.Split(new char[] { ';' });

            foreach(string assemblyName in customAssemblies)
            {
                Assembly customAssembly;
                customAssembly = Assembly.Load(assemblyName);
                IEnumerable<Type> customTypes = customAssembly.GetTypes()
                                                        .Where(type => resourceType.IsAssignableFrom(type));
                systemResourceTypes = systemResourceTypes.Union(customTypes).ToArray();
            }
        }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The <see cref="ResourceType"/>.</returns>
        internal ResourceType GetResourceType(Core.Resource resource)
        {
            if(null == resource)
            {
                return null;
            }

            return GetResourceType(resource.GetType().FullName);
        }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>The <see cref="ResourceType"/>.</returns>
        internal ResourceType GetResourceType(string typeName)
        {
            if(string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            ResourceType typeInfo = null;

            using(Core.ZentityContext context = CoreHelper.CreateZentityContext(entityConnectionString))
            {
                var resourceTypes = context.DataModel.Modules
                                    .SelectMany(tuple => tuple.ResourceTypes);

                if(typeName.Contains("."))
                {
                    typeInfo = resourceTypes.FirstOrDefault(type => type.FullName.ToUpperInvariant() == typeName.ToUpperInvariant());
                }
                else
                {
                    typeInfo = resourceTypes.FirstOrDefault(type => type.Name.ToUpperInvariant() == typeName.ToUpperInvariant());
                }
            }

            return typeInfo;
        }

        /// <summary>
        /// Gets the type of the system resource.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>The <see cref="Type"/>.</returns>
        internal static Type GetSystemResourceType(string typeName)
        {
            if(string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            if(typeName.Contains('.'))
            {
                return CoreHelper.SystemResourceTypes
                        .FirstOrDefault(type => type.FullName.ToUpperInvariant() == typeName.ToUpperInvariant());
            }
            else
            {
                return CoreHelper.SystemResourceTypes.FirstOrDefault(type => type.Name.ToUpperInvariant() == typeName.ToUpperInvariant());
            }
        }

        /// <summary>
        /// Gets the resource type hierarchy.
        /// </summary>
        /// <param name="resourceTypeInfo">The resource type info.</param>
        /// <returns>List of resource type by heirarchy.</returns>
        internal List<string> GetResourceTypeHierarchy(ResourceType resourceTypeInfo)
        {
            List<string> resourceTypeList = new List<string>();
            if(null == resourceTypeInfo)
            {
                return resourceTypeList;
            }

            if(CoreHelper.resourceTypeHierarchy.ContainsKey(resourceTypeInfo.Name))
            {
                return CoreHelper.resourceTypeHierarchy[resourceTypeInfo.Name];
            }

            ResourceType orginalResourceType = resourceTypeInfo;
            resourceTypeList.Add(resourceTypeInfo.Name);

            using(Core.ZentityContext context = CoreHelper.CreateZentityContext(entityConnectionString))
            {
                while(resourceTypeInfo.BaseType != null)
                {
                    ResourceType resourceType = null;
                    foreach(ResourceType resType in CoreHelper.GetResourceTypes(context))
                    {
                        if((resourceTypeInfo.BaseType.FullName.EndsWith(resType.Name, StringComparison.Ordinal) &&
                            resourceTypeInfo.BaseType.FullName.StartsWith(resType.Parent.NameSpace, StringComparison.Ordinal)))
                        {
                            resourceType = resType;
                            break;
                        }
                    }

                    resourceTypeInfo = resourceType;
                    resourceTypeList.Add(resourceTypeInfo.Name);
                }
            }
            CoreHelper.resourceTypeHierarchy.Add(orginalResourceType.Name, resourceTypeList);
            return resourceTypeList;
        }

        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        /// <returns>The <see cref="AuthenticatedToken"/>.</returns>
        internal static AuthenticatedToken GetAuthenticationToken()
        {
            AuthenticatedToken authenticatedToken = null;
            if(HttpContext.Current.Items.Contains("AuthenticatedToken")
                && HttpContext.Current.Items["AuthenticatedToken"] != null)
            {
                authenticatedToken = (AuthenticatedToken)HttpContext.Current.Items["AuthenticatedToken"];

                if(authenticatedToken == null)
                {
                    throw new AuthenticationException(Resources.INVALID_AUTHENTICATION_TOKEN);
                }
            }
            else
            {
                throw new AuthenticationException(Resources.INVALID_AUTHENTICATION_TOKEN);
            }

            return authenticatedToken;
        }

        #region Internal Static Utility functions

        /// <summary>
        /// Retrieves the name by combining the first middle and the last name
        /// </summary>
        /// <param name="firstName">
        /// first name 
        /// </param>
        /// <param name="middleName">
        /// middle name
        /// </param>
        /// <param name="lastName">
        /// last name
        /// </param>
        /// <returns>returns the complete name</returns>
        internal static string GetCompleteName(string firstName, string middleName, string lastName)
        {
            StringBuilder completeName = new StringBuilder();
            if(!string.IsNullOrEmpty(firstName))
            {
                completeName.Append(firstName);
                completeName.Append(" ");
            }
            if(!string.IsNullOrEmpty(middleName))
            {
                completeName.Append(middleName);
                completeName.Append(" ");
            }
            if(!string.IsNullOrEmpty(lastName))
            {
                completeName.Append(lastName);
            }
            return completeName.ToString();
        }

        /// <summary>
        /// Checks if incoming object contains valid date
        /// </summary>
        /// <param name="parseDate"> object to be parsed </param>
        /// <param name="isUntil">are we parsing until date time?</param>
        /// <returns> valid datetime else throws error </returns>
        internal static DateTime ValidateDate(object parseDate, bool isUntil)
        {
            DateTime date = DateTime.MinValue;
            if(!DateTime.TryParseExact(System.Convert.ToString(parseDate, CultureInfo.CurrentCulture),
                MetadataProviderHelper.DateTimeGranularity, CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out date))
            {

                if(!DateTime.TryParseExact(System.Convert.ToString(parseDate, CultureInfo.CurrentCulture),
                    MetadataProviderHelper.DateTimeFormat, CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out date))
                {
                    throw new ArgumentException(string.Empty, "parseDate");
                }
                if(isUntil)
                {
                    //Add a complete day's time for until datetime
                    date = date.Add(new TimeSpan(23, 59, 59));
                }
            }
            return date;
        }

        /// <summary>
        /// Determines whether [is well known type] [the specified res type].
        /// </summary>
        /// <param name="resType">Type of the res.</param>
        /// <returns>
        /// 	<c>true</c> if [is well known type] [the specified res type]; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsWellKnownType(ResourceType resType)
        {
            return resType.Parent.IsMsShipped;
        }

        /// <summary>
        /// Gets the resource types.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>List of <see cref="ResourceType"/>.</returns>
        internal static List<ResourceType> GetResourceTypes(Zentity.Core.ZentityContext context)
        {
            return context.DataModel.Modules
                          .SelectMany(tuple => tuple.ResourceTypes)
                          .ToList();
        }

        #endregion

        #region OAI-PMH Internal functions

        /// <summary>
        ///     retrieves the when was first resource added to core
        /// </summary>
        /// <returns>datestamp of first resource added</returns>
        internal string GetEarliestdateStamp()
        {
            using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext())
            {
                DateTime? earliestScholarlyWorkDateStamp = getEarliestDateStamp(zentityContext);

                if(earliestScholarlyWorkDateStamp.HasValue)
                {
                    return earliestScholarlyWorkDateStamp.Value.ToUniversalTime().
                            ToString(MetadataProviderHelper.DateTimeGranularity, CultureInfo.InvariantCulture);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the instance of a ZentityContext.
        /// </summary>
        /// <returns>Instance of ZentityContext.</returns>
        internal static ZentityContext CreateZentityContext()
        {
            return CreateZentityContext(CoreHelper.DefaultConnectionString);
        }

        /// <summary>
        /// Creates the zentity context.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="ZentityContext"/>.</returns>
        internal static ZentityContext CreateZentityContext(string connectionString)
        {
            ZentityContext context = null;

            if(string.IsNullOrEmpty(connectionString))
            {
                context = new ZentityContext();
            }
            else
            {
                context = new ZentityContext(connectionString);
            }

            if(null != context)
            {
                context.CommandTimeout = PlatformSettings.ConnectionTimeout;
            }

            context.MetadataWorkspace.LoadFromAssembly(Assembly.GetAssembly(typeof(ScholarlyWorkItem)));
            context.MetadataWorkspace.LoadFromAssembly(Assembly.GetAssembly(typeof(Identity)));

            return context;
        }

        /// <summary>
        /// retrieves resource object specific to resource id
        /// </summary>
        /// <param name="resourceId">id for retrieving specific object</param>
        /// <returns>resource object</returns>
        internal Core.Resource GetResource(Guid resourceId)
        {
            Core.Resource coreResource = null;

            using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext())
            {
                var returnedResource = zentityContext.Resources
                                .Where(resource => resource.Id == resourceId)
                                .FirstOrDefault();

                if(returnedResource != null
                    && returnedResource.Authorize("Read", zentityContext, GetAuthenticationToken()))
                {
                    coreResource = LoadResource(returnedResource);
                }
            }

            return coreResource;

        }

        /// <summary>
        /// Checks if specified resource exist
        /// </summary>
        /// <param name="identifier"> resource identifier </param>
        /// <returns> boolean indicating whether resource exist or not </returns>
        internal bool CheckIfResourceExists(Guid identifier)
        {
            bool exists = false;
            using(Core.ZentityContext zentityContext = CoreHelper.CreateZentityContext())
            {
                var returnedResource = zentityContext.Resources
                                .Where(resource => resource.Id == identifier)
                                .FirstOrDefault();

                if(returnedResource != null
                    && returnedResource.Authorize("Read", zentityContext, GetAuthenticationToken()))
                {
                    exists = true;
                }
            }

            return exists;
        }

        /// <summary>
        /// retrieves list of resources according to query parameters
        /// </summary>
        /// <param name="queryParams"> hashtable containing query parameters </param>
        /// <param name="initialQueryExecutionTime"> required for idempotency of resumption token </param>
        /// <param name="isListRecords"> if isListRecords then load resources with authors and tags</param>
        /// <returns> list of resources </returns>
        internal List<Core.Resource> GetResources(Hashtable queryParams, DateTime initialQueryExecutionTime, bool isListRecords)
        {
            // Algorithm:
            List<Core.Resource> listOfResources = new List<Core.Resource>();
            try
            {
                Type set = null;
                if(queryParams.ContainsKey("set"))
                {
                    set = CoreHelper.GetSystemResourceType(queryParams["set"] as string);
                    if(null == set)
                    {
                        throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_QueryParams, "queryParams");
                    }
                }

                if(null == set)
                {
                    set = typeof(Core.Resource);
                }

                //make generic call
                MethodInfo getResourceListMethod = getResourceListMethod = this.GetType().GetMethod("GetResourceList",
                                    BindingFlags.NonPublic | BindingFlags.Instance);
                if(null != getResourceListMethod)
                {
                    listOfResources = (List<Core.Resource>)getResourceListMethod.MakeGenericMethod(set).Invoke(this, new object[] { queryParams, initialQueryExecutionTime, isListRecords, false, null });
                }
            }
            catch(TargetInvocationException ex)
            {
                if(null != ex.InnerException)
                {
                    throw ex.InnerException;
                }
                throw;
            }
            catch(Exception)
            {
                throw;
            }

            return listOfResources;
        }

        /// <summary>
        /// retrieves list of resources according to query parameters
        /// </summary>
        /// <param name="queryParams"> hashtable containing query parameters </param>        
        /// <param name="token"> resumption token details </param>
        /// <param name="isListRecords"> if isListRecords then load resources with authors and tags</param>
        /// <returns> list of resources </returns>
        internal List<Core.Resource> GetResources(Hashtable queryParams, MetadataProviderHelper.TokenDetails token, bool isListRecords)
        {
            // Algorithm:
            List<Core.Resource> listOfResources = new List<Core.Resource>();
            try
            {
                Type set = null;
                if(queryParams.ContainsKey("set"))
                {
                    set = CoreHelper.GetSystemResourceType(queryParams["set"] as string);
                    if(null == set)
                    {
                        throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_QueryParams, "queryParams");
                    }
                }

                if(null == set)
                {
                    set = typeof(Core.Resource);
                }

                //make generic call
                MethodInfo getResourceListMethod = getResourceListMethod = this.GetType().GetMethod("GetResourceList",
                                    BindingFlags.NonPublic | BindingFlags.Instance);
                if(null != getResourceListMethod)
                {
                    listOfResources = (List<Core.Resource>)getResourceListMethod.MakeGenericMethod(set).Invoke(this, new object[] { queryParams, token.QueryExecutionDateTime, isListRecords, true, token });
                }
            }
            catch(TargetInvocationException ex)
            {
                if(null != ex.InnerException)
                {
                    throw ex.InnerException;
                }
                throw;
            }
            catch(Exception)
            {
                throw;
            }

            return listOfResources;
        }

        //internal static Core.ResourceTypeInfo GetResourceType(Core.Resource resource)
        //{
        //    if (null == resource)
        //    {
        //        return null;
        //    }

        //    Core.ResourceTypeInfo resourceType = null;
        //    using (Core.ZentityContext context = CoreHelper.GetZentityContext())
        //    {
        //      resourceType = CoreHelper.GetResourceType(context, resource);
        //    }

        //    return resourceType;
        //}

        #endregion

        #region Private Member functions

        /// <summary>
        /// Filters the query.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><see cref="IQueryable"/> of <see cref="Resource"/> types.</returns>
        private IQueryable<Resource> FilterQuery(ZentityContext context)
        {
            return context.Resources.
                Where(resource => !(resource is Resource &&
                    (((Resource)resource) is Contact
                        || ((Resource)resource) is Group
                        || ((Resource)resource) is Identity)));
        }

        /// <summary>
        /// Gets the resource list.
        /// </summary>
        /// <typeparam name="T">Type of resource.</typeparam>
        /// <param name="queryParams">The query params.</param>
        /// <param name="initialQueryExecutionTime">The initial query execution time.</param>
        /// <param name="isListRecords">if set to <c>true</c> [is list records].</param>
        /// <param name="isResumptionTokenSpecified">if set to <c>true</c> [is resumption token specified].</param>
        /// <param name="token">The token.</param>
        /// <returns>List of <see cref="Resource"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private List<Core.Resource> GetResourceList<T>(
                            Hashtable queryParams,
                            DateTime initialQueryExecutionTime,
                            bool isListRecords,
                            bool isResumptionTokenSpecified,
                            MetadataProviderHelper.TokenDetails token) where T : Core.Resource
        {
            List<Core.Resource> listOfResources = new List<Core.Resource>();
            IQueryable<T> resourceQuery = null;

            // 1) Build the query
            DateTime from = DateTime.MinValue;
            DateTime until = DateTime.MinValue;

            if(queryParams.ContainsKey("from"))
            {
                from = ValidateDate(queryParams["from"], false);
            }
            if(queryParams.ContainsKey("until"))
            {
                until = ValidateDate(queryParams["until"], true);
            }

            using(Core.ZentityContext context = CoreHelper.CreateZentityContext())
            {
                AuthenticatedToken authenticatedToken = GetAuthenticationToken();

                if(from != DateTime.MinValue)
                {
                    if((until != DateTime.MinValue))
                    {
                        if(from > until)
                        {
                            throw new ArgumentException(MetadataProviderHelper.Error_Message_Invalid_QueryParams, "queryParams");
                        }
                        resourceQuery = FilterQuery(context).OfType<T>().
                            Where(resource =>
                                resource.DateModified >= from
                                && resource.DateModified <= until
                                && resource.DateModified < initialQueryExecutionTime)
                                .Authorize("Read", context, authenticatedToken)
                                .OrderByDescending(resource => resource.DateModified);
                    }
                    else
                    {
                        resourceQuery = FilterQuery(context).OfType<T>().
                            Where(resource =>
                                resource.DateModified >= from
                                && resource.DateModified < initialQueryExecutionTime)
                                .Authorize("Read", context, authenticatedToken)
                                .OrderByDescending(resource => resource.DateModified);
                    }
                }
                else
                {
                    resourceQuery = FilterQuery(context).OfType<T>().
                            Where(resource =>
                                resource.DateModified < initialQueryExecutionTime)
                                .Authorize("Read", context, authenticatedToken)
                                .OrderByDescending(resource => resource.DateModified);
                }

                // 2) Retrieve resources count 
                this.totalResourceCount = this.actualRecordCount = resourceQuery.Count();

                if(this.totalResourceCount <= 0)
                {
                    throw new ArgumentException(MetadataProviderHelper.Error_Message_NoRecords,
                        "queryParams");
                }
                if(!isResumptionTokenSpecified)
                {
                    // 3) If count greater than maxharvestcount then
                    //       retrieve resources according to harvest count
                    //    else
                    //       retrieve resources according to original count
                    if(MetadataProviderHelper.IsResumptionRequired(this.totalResourceCount))
                    {
                        listOfResources = CoreHelper.FilterResources<T>(
                                            resourceQuery.Take(PlatformSettings.MaximumHarvestCount),
                                            isListRecords);
                        this.actualHarvestedCount = listOfResources.Count;
                    }
                    else
                    {
                        listOfResources = CoreHelper.FilterResources<T>(resourceQuery, isListRecords);
                    }
                }
                else
                {
                    // 4) retrieve pending resources i.e : according to maximum harvest count
                    int noOfRecordsToSkip = token.ActualHarvestedRecords;

                    listOfResources = CoreHelper.FilterResources<T>(resourceQuery.
                                            Skip(noOfRecordsToSkip).
                                            Take(PlatformSettings.MaximumHarvestCount), isListRecords);

                    this.actualHarvestedCount = noOfRecordsToSkip + listOfResources.Count;
                }
            }
            return listOfResources;
        }

        /// <summary>
        /// Filters the resources.
        /// </summary>
        /// <typeparam name="T">Type of resource.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="isListRecords">if set to <c>true</c> its a list of resources.</param>
        /// <returns>List of <see cref="Resource"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static List<Core.Resource> FilterResources<T>(
                                                        IQueryable<T> query, 
                                                        bool isListRecords) where T : Core.Resource
        {
            List<Core.Resource> listOfResources = new List<Core.Resource>();
            foreach(T resource in query)
            {
                if(resource is ScholarlyWorks.Contact || resource is Identity || resource is Group)
                    continue;
                Core.Resource resourceToAdd = resource;
                listOfResources.Add(resourceToAdd);
            }
            if(isListRecords)
            {
                listOfResources = CoreHelper.LoadResources(listOfResources);
            }
            return listOfResources;
        }

        /// <summary>
        /// Loads all the dependent collection set for the given resource 
        /// </summary>
        /// <param name="resources">List of resource</param>
        /// <returns>List of all loaded resources</returns>
        private static List<Core.Resource> LoadResources(IEnumerable<Core.Resource> resources)
        {
            List<Core.Resource> listOfResources = new List<Core.Resource>();
            if(null != resources)
            {
                foreach(Core.Resource resource in resources)
                {
                    listOfResources.Add(LoadResource(resource));
                }
            }
            return listOfResources;
        }

        /// <summary>
        /// Loads all the dependent collection set for the given resource 
        /// </summary>
        /// <param name="resource"> an resource object</param>
        /// <returns> loaded resource </returns>
        private static Core.Resource LoadResource(Core.Resource resource)
        {
            if(null == resource)
            {
                return null;
            }

            ScholarlyWorks.ScholarlyWork scholarlyWork = resource as ScholarlyWorks.ScholarlyWork;
            if(null != scholarlyWork)
            {
                if(!scholarlyWork.Authors.IsLoaded)
                {
                    scholarlyWork.Authors.Load();
                }
                if(!scholarlyWork.Contributors.IsLoaded)
                {
                    scholarlyWork.Contributors.Load();
                }
                if(!scholarlyWork.Tags.IsLoaded)
                {
                    scholarlyWork.Tags.Load();
                }

            }

            return resource;
        }

        #endregion
    }

    #endregion
}
