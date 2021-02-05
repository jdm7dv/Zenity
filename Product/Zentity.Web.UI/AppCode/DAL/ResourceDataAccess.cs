// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using System.Data;
using System.Collections;
using System.Data.Objects;
using System.Globalization;
using Zentity.Platform;
using Zentity.Web.UI.ToolKit;
using System.Configuration;
using System.IO;
using System.Data.Common;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Web.Caching;
using System.IdentityModel.Tokens;
using Zentity.Security.Authentication;
using Zentity.Security.Authorization;
using Zentity.Security.AuthorizationHelper;
using Zentity.Security.AuthenticationProvider;
using System.Text;
using Resources;

namespace Zentity.Web.UI
{
    /// <summary>
    /// Summary description for ResourceDAL
    /// </summary>
    public sealed class ResourceDataAccess : IDisposable
    {
        #region Member Variables

        #region Private

        private ZentityContext _context;
        private bool isDisposed;

        #endregion

        #endregion

        #region Constants

        #region Private

        private const string _resourceInfo = "resourceInfo";
        private const string _ID = "ID";
        private const string _Title = "Title";

        #region Compiled LINQ Queries

        static Func<ZentityContext, Guid, IQueryable<Resource>> _getResourceByIdCompiledQuery =
           CompiledQuery.Compile((ZentityContext context, Guid id) =>
           (context.Resources.Where(tuple => tuple.Id == id)));

        static Func<ZentityContext, Guid, IQueryable<Resource>> _deleteResourceByIdCompiledQuery =
          CompiledQuery.Compile((ZentityContext context, Guid id) =>
          (context.Resources.Include(Resources.Resources.PredicateRelationshipsAsSubject).Include(Resources.Resources.PredicateRelationshipsAsObject)
          .Where(tuple => tuple.Id == id)));

        static Func<ZentityContext, Guid, IQueryable<ScholarlyWork>> _getCitedScholarlyWorksCompiledQuery =
          CompiledQuery.Compile((ZentityContext context, Guid id) =>
          (context.Resources.OfType<ScholarlyWork>().Include(Resources.Resources.ZentityCites).Where(tuple => tuple.Id == id)));

        static Func<ZentityContext, Guid, IQueryable<ScholarlyWorkItem>> _getRefrenceTagsCompiledQuery =
            CompiledQuery.Compile((ZentityContext context, Guid id) =>
                context.Resources.OfType<ScholarlyWorkItem>().Include(Resources.Resources.ZentityTags)
                .Where(res => res.Id == id));

        static Func<ZentityContext, Guid, IQueryable<ScholarlyWorkItem>> _getRefrenceCategoryNodesCompiledQuery =
           CompiledQuery.Compile((ZentityContext context, Guid id) =>
               context.Resources.OfType<ScholarlyWorkItem>().Include(Resources.Resources.ZentityCategoryNodes)
               .Where(res => res.Id == id));

        private static Func<ZentityContext, Guid, IQueryable<Zentity.Core.File>> _getFileById =
         CompiledQuery.Compile((ZentityContext _context, Guid id) =>
         (_context.Resources.OfType<Zentity.Core.File>().Where(tuple => tuple.Id == id)));

        private static Func<ZentityContext, Guid, Guid, IOrderedQueryable<Relationship>> _getRelationsBySubjectResourceAndObjectContactQuery =
            CompiledQuery.Compile((ZentityContext _context, Guid predicateId, Guid SubjectId) =>
            (_context.Relationships
            .Where(r => (r.Predicate.Id == predicateId && r.Subject.Id == SubjectId))
            .OrderBy(r => r.OrdinalPosition)));


        private static Func<ZentityContext, Guid, IOrderedQueryable<Relationship>> _getRelationsAsSubjecByResourceIdQuery =
            CompiledQuery.Compile((ZentityContext _context, Guid SubjectId) =>
                (_context.Relationships
                .Where(r => r.Subject.Id == SubjectId).OrderBy(r => r.Object.Title)));


        private static Func<ZentityContext, Guid, IOrderedQueryable<Relationship>> _getRelationsAsObjectByResourceIdQuery =
            CompiledQuery.Compile((ZentityContext _context, Guid ObjectId) =>
                (_context.Relationships
                .Where(r => r.Object.Id == ObjectId).OrderBy(r => r.Subject.Title)));

        private static Func<ZentityContext, Guid, IOrderedQueryable<Relationship>> _getOrderedRelationsAsSubjectByResourceIdQuery =
            CompiledQuery.Compile((ZentityContext context, Guid SubjectId) =>
            (context.Relationships
            .Where(r => r.Subject.Id == SubjectId).OrderBy(r => r.OrdinalPosition)));

        #endregion

        #endregion

        #endregion

        #region Properties
        #region Public

        /// <summary>
        /// Get all Resource types
        /// </summary>
        /// <returns></returns>
        public List<ResourceType> ResourceTypes
        {
            get
            {
                return _context.DataModel.Modules.
                    SelectMany(tuple => tuple.ResourceTypes).ToList();
            }
        }

        #endregion
        #endregion

        #region Constructors & finalizers

        #region public

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ResourceDataAccess()
        {
            if (this._context == null)
            {
                this._context = Utility.CreateContext();
            }
        }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        /// <param name="context">Zentity Context object</param>
        public ResourceDataAccess(ZentityContext context)
        {
            this._context = context;
        }

        #endregion

        #endregion

        #region Methods

        #region public


        /// <summary>
        /// Gets root category nodes.
        /// </summary>
        /// <returns>Root category nodes.</returns>
        public IEnumerable<CategoryNode> GetRootCategoryNodes()
        {
            return _context.CategoryNodes().Where(cat => cat.Parent == null);
        }

        /// <summary>
        /// Loads the hierarchy of a category node.
        /// </summary>
        /// <returns>Loaded category node.</returns>
        public void LoadCategoryNodeHierarchy(CategoryNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(Resources.Resources.ParameterNode);
            }

            LoadCatgoryNodeChildren(node);
        }

        private void LoadCatgoryNodeChildren(CategoryNode node)
        {
            node.Children.Load();
            IEnumerable<CategoryNode> children = node.Children;
            foreach (CategoryNode child in children)
            {
                LoadCatgoryNodeChildren(child);
            }
        }

        /// <summary>
        /// Fetches resource object based on resource Id.
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <returns>Resource object</returns>
        public Resource GetResource(Guid resourceId)
        {
            return _getResourceByIdCompiledQuery.Invoke(_context, resourceId).FirstOrDefault();
        }

        /// <summary>
        /// Deletes Resource from Database
        /// </summary>
        /// <param name="resourceId">Resource Id</param>        
        /// <returns>True if operation completed successfully</returns>
        public bool DeleteResource(Guid resourceId)
        {
            bool success = false;

            Resource resource = _deleteResourceByIdCompiledQuery.Invoke(_context, resourceId).FirstOrDefault();

            if (resource != null)
            {
                //Delete relationships as a subject              
                List<Relationship> relationships = resource.RelationshipsAsSubject.ToList();
                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                //Delete relationships as a object               
                relationships = resource.RelationshipsAsObject.ToList();
                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                // Delete associated Resource propertes
                resource.ResourceProperties.Load();
                List<ResourceProperty> resProperties = resource.ResourceProperties.ToList();
                foreach (ResourceProperty property in resProperties)
                {
                    resource.ResourceProperties.Remove(property);
                    _context.DeleteObject(property);
                }

                //Delete resource object
                _context.DeleteObject(resource);

                try
                {
                    _context.SaveChanges();
                    success = true;
                }
                catch (OptimisticConcurrencyException exception)
                {
                    throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
                }
            }
            return success;
        }

        /// <summary>
        /// Deletes input Resources from Database
        /// </summary>
        /// <param name="resourceIds">List of resource Guids to be deleted</param>
        /// <exception cref="ConcurrentAccessException">Thrown when an exception is raised due to concurrency
        /// issues.</exception>
        public void DeleteReseources(Collection<Guid> resourceIds)
        {
            foreach (Guid resourceId in resourceIds)
            {
                DeleteResourceLogically(resourceId);
            }

            try
            {
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }
        }

        /// <summary>
        /// Deletes Resource from Database
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <param name="context">FamulusConttext object</param>
        /// <returns>True if operation completed successfully</returns>
        private bool DeleteResourceLogically(Guid resourceId)
        {
            bool success = false;

            Resource resource = _context.Resources.Where(res => res.Id == resourceId).FirstOrDefault();

            if (resource != null)
            {
                //Delete relationships as a subject
                resource.RelationshipsAsSubject.Load();
                List<Relationship> relationships = resource.RelationshipsAsSubject.ToList();

                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                //Delete relationships as a object
                resource.RelationshipsAsObject.Load();
                relationships = resource.RelationshipsAsObject.ToList();

                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                //Delete resource object
                _context.DeleteObject(resource);

                success = true;
            }
            return success;
        }

        public string DeleteUsers(Collection<Guid> resourceIds, AuthenticatedToken userToken)
        {
            List<string> identities = GetIdentities(string.Empty)
                .Where(identity => resourceIds.Contains<Guid>(identity.Id))
                .Select(identity => identity.IdentityName)
                .ToList();

            StringBuilder identitiesNotDeleted = new StringBuilder();
            string separator = Constants.Comma + Constants.Space;
            if (identities.Remove(UserManager.AdminUserName))
            {
                identitiesNotDeleted.Append(UserManager.AdminUserName);
                identitiesNotDeleted.Append(separator);
            }
            if (identities.Remove(UserManager.GuestUserName))
            {
                identitiesNotDeleted.Append(UserManager.GuestUserName);
                identitiesNotDeleted.Append(separator);
            }

            foreach (string identity in identities)
            {
                ZentityUserAdmin adminObject = new ZentityUserAdmin(userToken);
                ZentityUserProfile currentUserProfile = adminObject.GetUserProfile(identity);
                ZentityUser currentUser = new ZentityUser(identity, userToken, currentUserProfile);

                if (!UserManager.DeleteUser(currentUser, userToken))
                {
                    identitiesNotDeleted.Append(identity);
                    identitiesNotDeleted.Append(Constants.Comma);
                    identitiesNotDeleted.Append(Constants.Space);
                }
            }
            string message = identitiesNotDeleted.ToString();
            message = message.TrimEnd(Constants.Comma[0], Constants.Space[0]);
            if (!string.IsNullOrEmpty(message))
            {
                message = string.Format(CultureInfo.CurrentCulture, Resources.Resources.ErrorDeletingUsers, message);
            }
            return message;
        }

        public string DeleteGroups(Collection<Guid> resourceIds, AuthenticatedToken token)
        {
            List<Group> groups = GetGroups(string.Empty)
               .Where(group => resourceIds.Contains<Guid>(group.Id))
               .ToList();

            StringBuilder groupsNotDeleted = new StringBuilder();
            string separator = Constants.Comma + Constants.Space;
            if (groups.RemoveAll(group => group.GroupName == UserManager.AdminGroupName) > 0)
            {
                groupsNotDeleted.Append(UserManager.AdminGroupName);
                groupsNotDeleted.Append(separator);
            }
            if (groups.RemoveAll(group => group.GroupName == UserManager.AllUsersGroupName) > 0)
            {
                groupsNotDeleted.Append(UserManager.AllUsersGroupName);
                groupsNotDeleted.Append(separator);
            }

            foreach (Group group in groups)
            {
                if (!UserManager.DeleteGroup(group, token))
                {
                    groupsNotDeleted.Append(group);
                    groupsNotDeleted.Append(Constants.Comma);
                    groupsNotDeleted.Append(Constants.Space);
                }
            }
            string message = groupsNotDeleted.ToString();
            message = message.TrimEnd(Constants.Comma[0], Constants.Space[0]);
            if (!string.IsNullOrEmpty(message))
            {
                message = string.Format(CultureInfo.CurrentCulture, Resources.Resources.ErrorDeletingGroups, message);
            }
            return message;
        }

        private class AccessResources<T> where T : Zentity.Core.Resource
        {
            private delegate IEnumerable<T> FilterOnTitle(IEnumerable<T> resources, string strtitle);
            private static Dictionary<ResourceStringComparison, FilterOnTitle> _dictFilterOnTitle =
                new Dictionary<ResourceStringComparison, FilterOnTitle>();
            static AccessResources()
            {
                _dictFilterOnTitle.Clear();
                _dictFilterOnTitle.Add(ResourceStringComparison.All, (resources, title) => resources);
                _dictFilterOnTitle.Add(ResourceStringComparison.Equals, (resources, title) =>
                    resources.Where(res => res.Title == title));
                _dictFilterOnTitle.Add(ResourceStringComparison.Contains, (resources, title) =>
                    resources.Where(res => res.Title.Contains(title)));
                _dictFilterOnTitle.Add(ResourceStringComparison.NotEqual, (resources, title) =>
                    resources.Where(res => res.Title != title));
            }

            static public IEnumerable<T> FilterResources(ZentityContext context, ResourceStringComparison filterBy,
                string filterValue)
            {
                try
                {
                    return _dictFilterOnTitle[filterBy](context.Resources.OfType<T>(), filterValue);
                }
                catch (KeyNotFoundException)
                {
                    return new List<T>();
                }
            }
        }
        /// <summary>
        /// Filters resources based on filter type and filter value.
        /// </summary>
        /// <param name="filterType">Filter type (All, Equals, Contains, NotEqual)</param>
        /// <param name="filterValue">Filter value</param>
        /// <returns>List of filtered resources</returns>
        public IEnumerable<T> GetResources<T>(ResourceStringComparison filterType, string filterValue) where T : Resource
        {
            return AccessResources<T>.FilterResources(_context, filterType, filterValue);
        }

        /// <summary>
        /// Load all object scholarlywork related to subject scholarlywork through predicate CitedBy.
        /// </summary>
        /// <param name="scholarlyWorkId">Subject scholarlywork Id</param>
        /// <returns>Subject scholarlywork object</returns>
        public ScholarlyWork GetScholarlyWorkWithCitedScholarlyWorks(Guid scholarlyWorkId)
        {
            ScholarlyWork scholarlyWork = _getCitedScholarlyWorksCompiledQuery
                .Invoke(_context, scholarlyWorkId).FirstOrDefault();
            return scholarlyWork;
        }

        /// <summary>
        /// Fetch File oject for given file Id.
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <returns>File object</returns>
        public Zentity.Core.File GetFile(Guid fileId)
        {
            IQueryable<Zentity.Core.File> myFile = _getFileById(_context, fileId);

            if (myFile != null)
            {
                return myFile.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns list of latest added resources
        /// </summary>
        /// <param name="pageSize">Maximum No. of records to be fetched</param>
        /// <param name="token">Authenticated token</param>
        /// <returns></returns>
        public List<Contact> GetTopAuthors(AuthenticatedToken token, int pageSize)
        {
            List<Contact> topAuthors = new List<Contact>();
            if (token != null)
            {
                topAuthors = _context.Resources.OfType<Contact>()
                    .Authorize<Contact>(UserResourcePermissions.Read, _context, token)
                    .Include(Resources.Resources.IncludeAuthoredWorks)
                    .Where(tuple => (tuple.AuthoredWorks.Count > 0))
                    .OrderByDescending(author => author.AuthoredWorks.Count)
                    .Take(10)
                    .ToList();
            }
            return topAuthors;
        }


        /// <summary>
        /// Returns child resource types count.
        /// </summary>
        /// <param name="resourceType">resource type</param>
        /// <returns>int type</returns>
        public int GetChildResourceTypesCount(string resourceType)
        {
            int count = 0;
            List<ResourceType> resTypes = ResourceTypes;
            resTypes = resTypes.Where(tuple => tuple.BaseType != null).ToList();
            if (resourceType.Contains("."))
            {
                count = resTypes.Where(tuple => tuple.BaseType.FullName == resourceType).Count();
            }
            else
            {
                count = resTypes.Where(tuple => tuple.BaseType.Name == resourceType).Count();
            }
            return count;
        }

        /// <summary>
        /// Return object of ResourceTypeInfo based on resource Id
        /// </summary>
        /// <param name="resourceId">resource Id</param>
        /// <returns>Object of ResourceTypeInfo</returns>
        public ResourceType GetResourceType(Guid resourceId)
        {
            ResourceType resType = null;
            if (resourceId != Guid.Empty)
            {
                Resource res = GetResource(resourceId);
                if (res != null)
                {
                    resType = GetResourceType(res.GetType().FullName);
                }
            }

            return resType;
        }

        /// <summary>
        /// Return object of ResourceTypeInfo based on resource type
        /// </summary>
        /// <param name="resourceType">resource type</param>
        /// <returns>Object of ResourceTypeInfo</returns>
        public ResourceType GetResourceType(string resourceType)
        {
            ResourceType resType = null;
            if (resourceType.Contains("."))
            {
                resType = _context.DataModel.Modules.
                      SelectMany(tuple => tuple.ResourceTypes).
                      Where(tuple => tuple.FullName.Equals(resourceType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
            else
            {
                resType = _context.DataModel.Modules.
                      SelectMany(tuple => tuple.ResourceTypes).
                      Where(tuple => tuple.Name.Equals(resourceType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
            return resType;
        }

        /// <summary>
        /// Fetches scalar properties based on resource type
        /// </summary>
        /// <param name="resourceType">Resource type</param>
        /// <returns>List of scalar properties.</returns>
        public IEnumerable<ScalarProperty> GetScalarPropertyCollection(string resourceType)
        {
            IEnumerable<ScalarProperty> properties = new ScalarProperty[0];

            ResourceType type = _context.DataModel.Modules
                .SelectMany(module => module.ResourceTypes)
                .Where(rt => rt.FullName == resourceType)
                .FirstOrDefault();

            if (type != null)
            {
                properties = GetScalarProperties(type);
                while (type.BaseType != null)
                {
                    properties = properties.Concat(GetScalarProperties(type.BaseType));
                    type = type.BaseType;
                }
            }

            return properties;
        }

        public IEnumerable<ScalarProperty> GetScalarProperties(ResourceType resourceType)
        {
            return resourceType.ScalarProperties;
        }

        public static Collection<NavigationProperty> GetNavigationalProperties(Cache pageCache, ZentityContext context,
           string typeName)
        {
            ResourceType type = null;
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(context))
            {
                type = dataAccess.GetResourceType(typeName);
            }
            Collection<NavigationProperty> navigationalProperties = null;
            if (type != null)
            {
                navigationalProperties = GetNavigationalProperties(pageCache, type);
            }

            return navigationalProperties;
        }

        public static Collection<NavigationProperty> GetNavigationalProperties(Cache pageCache,
           ResourceType type)
        {
            Collection<NavigationProperty> navigationalProperties = null;
            if (type != null)
            {
                if (pageCache[type.FullName + Resources.Resources.ZentityNavigationalProperties] == null)
                {
                    navigationalProperties = GetNavigationalProperties(navigationalProperties, type);
                }
                else
                {
                    navigationalProperties = pageCache[type.FullName + Resources.Resources.ZentityNavigationalProperties] as Collection<NavigationProperty>;
                }
            }
            return navigationalProperties;
        }

        private static Collection<NavigationProperty> GetNavigationalProperties(Collection<NavigationProperty>
            navigationalPropertyCollection, ResourceType type)
        {
            if (type != null)
            {
                if (navigationalPropertyCollection == null)
                    navigationalPropertyCollection = new Collection<NavigationProperty>();
                foreach (NavigationProperty property in type.NavigationProperties)
                {
                    navigationalPropertyCollection.Add((property));
                }
                //If base type is not null then fetch navigational properties of Base class recursively
                if (type.BaseType != null)
                {
                    GetNavigationalProperties(navigationalPropertyCollection, type.BaseType);
                }
            }

            return navigationalPropertyCollection;
        }

        /// <summary>
        /// Fetches scalar property based on type and property name. IF object is present in cache 
        /// then it is fetched from cache else from Database.
        /// </summary>
        /// <param name="cache">Cache object</param>
        /// <param name="context"><see cref="ZentityContext" /> object.</param>
        ///<param name="typeName"> type name </param>
        /// <param name="propertyName">property name</param>
        /// <returns>ScalarProperty object is successful else null value</returns>
        public static NavigationProperty GetNavigationProperty(Cache cache, ZentityContext context,
            string typeName, string propertyName)
        {
            NavigationProperty property = null;

            //GetAll ScalarProperties
            Collection<NavigationProperty> navigationalPropertyCollection = GetNavigationalProperties(cache, context, typeName);

            if (navigationalPropertyCollection != null)
            {
                //Get scalar property
                property = navigationalPropertyCollection.Where(prop => prop.Name == propertyName).FirstOrDefault();
            }

            return property;
        }

        /// <summary>
        /// Fetches navigation properties based on resource type
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <returns>Navigation properties collection</returns>
        public IEnumerable<NavigationProperty> GetNavigationProperties(Cache cache, Guid resourceId)
        {
            IEnumerable<NavigationProperty> collection = new List<NavigationProperty>();
            if (resourceId != Guid.Empty)
            {
                Resource res = GetResource(resourceId);
                if (res != null)
                {
                    collection = GetNavigationalProperties(cache, _context, res.GetType().FullName);
                }
            }

            return collection;
        }

        /// <summary>
        /// Returns file string to the file.
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <returns>File stream</returns>
        public Stream GetContentStream(Guid fileId)
        {
            Zentity.Core.File fileObj = GetFile(fileId);

            if (fileObj != null)
            {
                return _context.GetContentStream(fileObj);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns file stream to the file to be download
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <param name="stream">File stream</param>
        /// <returns>File stream</returns>
        public Stream DownloadFileContent(Guid fileId, Stream stream)
        {
            Zentity.Core.File fileObj = GetFile(fileId);
            if (fileObj != null && stream != null)
            {
                _context.DownloadFileContent((Zentity.Core.File)fileObj, stream);
            }

            return stream;
        }

        /// <summary>
        /// Adds new resource
        /// </summary>
        /// <param name="resource">Resource object to be added</param>
        public void AddResource(Resource resource)
        {
            if (resource != null)
            {
                if (resource.EntityState == EntityState.Detached)
                {
                    _context.AddToResources(resource);
                }

                try
                {
                    _context.SaveChanges();
                }
                catch (OptimisticConcurrencyException exception)
                {
                    throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
                }
            }
        }

        public bool SaveResource(bool isEditMode, Resource resourceObj)
        {
            if (isEditMode)
            {
                if (resourceObj.EntityKey == null)
                {
                    throw new EntityNotFoundException(EntityType.Resource, resourceObj.Id);
                }

                object original;
                if (_context.TryGetObjectByKey(resourceObj.EntityKey, out original))
                {
                    resourceObj.DateModified = DateTime.Now;
                    _context.ApplyCurrentValues(resourceObj.EntityKey.EntitySetName, resourceObj);
                }
                else
                {
                    throw new EntityNotFoundException(EntityType.Resource, resourceObj.Id);
                }
            }
            else
            {
                _context.AddToResources(resourceObj);
            }
            try
            {
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataSet ImportBibTeX(AuthenticatedToken token, Guid parentResourceId, ICollection<ScholarlyWork> resourcesToImport,
            ICollection<Guid> resourcesToBeCitedId)
        {
            DataSet resourceInfo = null;
            ICollection<ScholarlyWork> resourcesToBeImported = null;

            resourcesToBeImported = resourcesToImport;
            ICollection<Guid> resourcesToBeCitesOnly = resourcesToBeCitedId;

            resourceInfo = new DataSet(_resourceInfo);
            resourceInfo.Locale = System.Globalization.CultureInfo.CurrentCulture;
            DataTable resourseInfoTable = resourceInfo.Tables.Add(Constants.ResourceTypeInfo);

            resourseInfoTable.Columns.Add(_ID).DataType = typeof(Guid);
            resourseInfoTable.Columns.Add(_Title).DataType = typeof(String);

            ScholarlyWork scholarlyWorkObj = (ScholarlyWork)GetResource(parentResourceId);

            DateTime dateAdded = DateTime.Now;

            if (resourcesToBeImported.Count > 0)
            {
                if (HasCreatePermission(token))
                {
                    //Resource subjectResoruce = scholarlyWorkObj;
                    foreach (ScholarlyWork resource in resourcesToBeImported)
                    {
                        // Check current resource is availble in the Zentity context or not
                        if (resource.EntityState == EntityState.Detached)
                        {
                            _context.AddToResources(resource);
                        }

                        // Update citation 
                        scholarlyWorkObj.Cites.Add(resource);

                        //Save for later use. To be displayed in grid
                        resourseInfoTable.Rows.Add(resource.Id, resource.Title);
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException(Resources.Resources.MsgUnAuthorizeAccessCreate);
                }
            }

            foreach (Guid id in resourcesToBeCitesOnly)
            {
                if (AuthorizeUser(token, Constants.PermissionRequiredForAssociation, id))
                {
                    //get resource
                    var resourceQuery = _context.Resources.OfType<ScholarlyWork>().Where(res => res.Id == id);
                    ScholarlyWork resource = resourceQuery.FirstOrDefault();

                    // Add to citations 
                    if (resource != null)
                        scholarlyWorkObj.Cites.Add(resource);
                }
                else
                {
                    throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                        Resources.Resources.MsgUnAuthorizeAccess, Constants.PermissionRequiredForAssociation));
                }
            }

            IEnumerable<ObjectStateEntry> objs = _context.ObjectStateManager.GetObjectStateEntries(EntityState.Added);
            foreach (ObjectStateEntry obj in objs)
            {
                Resource resource = obj.Entity as Resource;
                if (resource != null)
                {
                    resource.DateAdded = dateAdded;
                }
            }

            try
            {
                CorrectDuplicateBibTexEntries(token);
                GrantOwnerPermissionsForImportedResources(token);
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }

            UpdateRelationOrdinalPosition(resourcesToBeImported);

            try
            {
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }

            return resourceInfo;
        }

        /// <summary>
        /// ReplaCe duplicate entries with the entries already exist in the DB 
        /// and update the relationships.
        /// </summary>
        private void CorrectDuplicateBibTexEntries(AuthenticatedToken token)
        {

            List<ObjectStateEntry> entriesToDelete = new List<ObjectStateEntry>();

            //Find all scholarlyWork objects with EntityState as Added.
            IEnumerable<ObjectStateEntry> resourcesToImport = _context.ObjectStateManager.
                GetObjectStateEntries(EntityState.Added).Where(tuple => tuple.Entity is ScholarlyWork);


            foreach (ObjectStateEntry entry in resourcesToImport)
            {
                ScholarlyWork currentRes = (ScholarlyWork)entry.Entity;
                ScholarlyWork resExist = null;

                foreach (ScholarlyWork scholWork in _context.Resources.OfType<ScholarlyWork>().Where(
                         tuple => tuple.Title.Equals(currentRes.Title, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (scholWork.GetType() == currentRes.GetType())
                    {
                        resExist = scholWork;
                        break;
                    }
                }

                //Check if resource already exist in the DB or not.
                if (resExist != null)
                {
                    //If exist then Delete the new resource entry.
                    if (!entriesToDelete.Contains(entry))
                        entriesToDelete.Add(entry);
                }

                List<Contact> authorList = currentRes.Authors.ToList();
                foreach (Contact author in authorList)
                {
                    Contact authorExist = _context.Resources.OfType<Contact>().Where(
                        tuple => tuple.Title.Equals(author.Title, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    //Check if the author is aleardy present in the DB.
                    if (authorExist != null && author.EntityState == EntityState.Added)
                    {
                        if (AuthorizeUser(token, Constants.PermissionRequiredForAssociation, authorExist.Id))
                        {
                            //If exist then remove newly created athor from AuthorList and 
                            //add existing author.
                            currentRes.Authors.Remove(author);
                            currentRes.Authors.Add(authorExist);

                            //Delete newly creted author.
                            ObjectStateEntry contactEntry = _context.ObjectStateManager.GetObjectStateEntry(author.EntityKey);
                            if (!entriesToDelete.Contains(contactEntry))
                                entriesToDelete.Add(contactEntry);
                        }
                        else
                        {
                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                                Resources.Resources.MsgUnAuthorizeAccess, Constants.PermissionRequiredForAssociation));
                        }

                    }
                }
                List<Contact> editorList = currentRes.Editors.ToList();
                foreach (Contact editor in editorList)
                {
                    Contact editorExist = _context.Resources.OfType<Contact>().Where(
                        tuple => tuple.Title.Equals(editor.Title, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                    //Check if the editor is aleardy present in the DB.
                    if (editorExist != null && entry.State == EntityState.Added)
                    {
                        if (AuthorizeUser(token, Constants.PermissionRequiredForAssociation, editorExist.Id))
                        {
                            //If exist then remove newly created editor from 
                            //EditorList and add existing editor.
                            currentRes.Editors.Remove(editor);
                            currentRes.Editors.Add(editorExist);

                            //Delete newly creted author.
                            ObjectStateEntry contactEntry = _context.ObjectStateManager.GetObjectStateEntry(editor.EntityKey);
                            if (!entriesToDelete.Contains(contactEntry))
                                entriesToDelete.Add(contactEntry);
                        }
                        else
                        {
                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                                Resources.Resources.MsgUnAuthorizeAccess, Constants.PermissionRequiredForAssociation));
                        }
                    }
                }
            }
            //Delete newly added resources.
            foreach (ObjectStateEntry entry in entriesToDelete)
            {
                entry.Delete();
            }
        }

        private void GrantOwnerPermissionsForImportedResources(AuthenticatedToken token)
        {
            //Find all scholarlyWork objects with EntityState as Added.
            IEnumerable<ObjectStateEntry> resourcesToBeGranted = _context.ObjectStateManager.
                GetObjectStateEntries(EntityState.Added).Where(tuple => tuple.Entity is ScholarlyWork || tuple.Entity is Contact);

            foreach (ObjectStateEntry entry in resourcesToBeGranted)
            {
                Resource resource = entry.Entity as Resource;
                resource.GrantDefaultPermissions<Resource>(_context, token);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourcesToBeImported"></param>
        /// <returns></returns>
        private bool UpdateRelationOrdinalPosition(ICollection<ScholarlyWork> resourcesToBeImported)
        {
            bool result = false;

            // Update the ordinal positions of the relationship.
            // For authors
            //   1. Retrieve realationShip object
            //   2. set the ordinalt positon 
            //   3. Save context.
            foreach (ScholarlyWork resource in resourcesToBeImported)
            {
                ScholarlyWork scholary = resource as ScholarlyWork;
                if (scholary != null)
                {
                    // Update ordinal position for authors
                    int posCount = 1;
                    foreach (Contact contact in scholary.Authors)
                    {
                        List<Relationship> myRelation = _getRelationsBySubjectResourceAndObjectContactQuery(_context, contact.Id, resource.Id).ToList();

                        Relationship resPersonRelation = myRelation.FirstOrDefault();

                        if (resPersonRelation != null)
                        {
                            resPersonRelation.OrdinalPosition = posCount++;
                        }
                    }

                    // Update ordinal position for editors
                    posCount = 1;
                    foreach (Contact contact in scholary.Editors)
                    {
                        List<Relationship> myRelation = _getRelationsBySubjectResourceAndObjectContactQuery(_context, contact.Id, resource.Id).ToList();

                        Relationship resPersonRelation = myRelation.FirstOrDefault();

                        if (resPersonRelation != null)
                        {
                            resPersonRelation.OrdinalPosition = posCount++;
                        }
                    }
                }
            }

            result = true;

            return result;
        }

        /// <summary>
        /// Updates resource scope
        /// </summary>
        /// <param name="resourceIdList">List of resource Ids</param>
        /// <returns>True if operation is successfully completed</returns>
        public bool UpdateImportedResourceScope(Hashtable resourceIdList)
        {
            bool result = false;

            foreach (DictionaryEntry de in resourceIdList)
            {
                Guid resourceID = new Guid(Convert.ToString(de.Key, CultureInfo.InvariantCulture));

                ScholarlyWorkItem scholarlyWokItemObj = GetResource(resourceID) as ScholarlyWorkItem;

                if (scholarlyWokItemObj != null)
                {
                    scholarlyWokItemObj.Scope = Convert.ToString(de.Value, CultureInfo.InvariantCulture);
                }
            }

            try
            {
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }

            result = true;

            return result;
        }

        /// <summary>
        /// Detaches resource
        /// </summary>
        /// <param name="resourceObj"></param>
        public void DetachResource(Resource resourceObj)
        {
            _context.Detach(resourceObj);
        }

        /// <summary>
        /// Authenticate User.
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public AuthenticatedToken Authenticate(string userName, string password)
        {
            SecurityToken credentialToken = new UserNameSecurityToken(userName, password);
            IAuthenticationProvider authenticationProvider = AuthenticationProviderFactory.CreateAuthenticationProvider(Resources.Resources.ZentityAuthenticationProvider);

            //Authenticate User
            AuthenticatedToken authenticatedToken = authenticationProvider.Authenticate(credentialToken);

            return authenticatedToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupList"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CreateIdentityForUser(List<String> groupList, ZentityUser user)
        {
            try
            {
                Identity identity = new Identity();
                identity.IdentityName = user.LogOnName;
                identity.Title = user.Profile.FirstName + " " + user.Profile.LastName;
                _context.AddToResources(identity);

                foreach (String eachResourceId in groupList)
                {
                    Resource group = GetResource(new Guid(eachResourceId));

                    identity.Groups.Add((Group)group);
                }

                _context.SaveChanges();

                Relationship identityCanCreateResource = new Relationship();
                identityCanCreateResource.Subject = identity;
                identityCanCreateResource.Predicate =
                    _context.Predicates.Where(pred => pred.Uri == AuthorizingPredicates.HasCreateAccess).First();
                identityCanCreateResource.Object = identity;
                _context.AddToRelationships(identityCanCreateResource);
                _context.SaveChanges();
                return true;
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }
        }

        public bool AddUserInGroups(List<String> groupList, ZentityUser user)
        {
            try
            {
                IQueryable<Identity> identityType = _context.Resources.OfType<Identity>().Where(tuple => tuple.IdentityName == user.LogOnName);
                Identity identity = identityType.FirstOrDefault();

                foreach (String eachResourceId in groupList)
                {
                    Resource group = GetResource(new Guid(eachResourceId));

                    identity.Groups.Add((Group)group);
                }

                _context.SaveChanges();
                return true;
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }
        }

        public IEnumerable<Identity> GetIdentities(string filterCriteria)
        {
            IEnumerable<Identity> identites = UserManager.GetAllIdentities(_context);

            if (!string.IsNullOrEmpty(filterCriteria))
            {
                filterCriteria = filterCriteria.ToLowerInvariant();
                identites = identites.Where(tuple => tuple.IdentityName.ToLowerInvariant().Contains(filterCriteria));
            }
            return identites;
        }

        public IEnumerable<Group> GetGroups(string filterCriteria)
        {
            IEnumerable<Group> groups = UserManager.GetAllGroups(_context);

            if (!string.IsNullOrEmpty(filterCriteria))
            {
                filterCriteria = filterCriteria.ToLowerInvariant();
                groups = groups.Where(tuple => tuple.GroupName.ToLowerInvariant().Contains(filterCriteria));
            }
            return groups;
        }

        public IEnumerable<Resource> GetResources(string filterCriteria)
        {
            IEnumerable<Resource> resources = null;
            if (!string.IsNullOrEmpty(filterCriteria.Trim()))
            {
                resources = _context.Resources.OfType<Resource>()
                    .Where(tuple => tuple.Title.Contains(filterCriteria) &&
                        !((tuple is Group) || (tuple is Identity))).ToList();
            }
            else
            {
                resources = _context.Resources.OfType<Resource>()
                    .Where(tuple => !((tuple is Group) || (tuple is Identity)));
            }
            return resources;
        }

        public IEnumerable<Identity> GetIdentitiesWithExplicitPermissions(Guid resourceId,
            string filterCriteria, AuthenticatedToken userToken)
        {
            Resource res = GetResource(resourceId);
            IEnumerable<Identity> identities = res.GetUsersWithExplicitPermissions(_context, userToken);
            if (!string.IsNullOrEmpty(filterCriteria))
            {
                filterCriteria = filterCriteria.ToLowerInvariant();
                identities = identities.Where(tuple => tuple.IdentityName.ToLowerInvariant().Contains(filterCriteria));
            }
            return identities;
        }

        public IEnumerable<Group> GetGroupsWithExplicitPermissions(Guid resourceId,
            string filterCriteria, AuthenticatedToken userToken)
        {
            Resource resource = GetResource(resourceId);
            IEnumerable<Group> groups = resource.GetGroupsWithExplicitPermissions(_context, userToken);
            if (!string.IsNullOrEmpty(filterCriteria))
            {
                filterCriteria = filterCriteria.ToLowerInvariant();
                groups = groups.Where(tuple => tuple.GroupName.ToLowerInvariant().Contains(filterCriteria));
            }

            return groups;
        }

        public IEnumerable<Resource> GetResourcesWithExplicitPermissions(Guid UserOrGroupId,
            string filterCriteria, AuthenticatedToken userToken)
        {
            IEnumerable<Resource> resources = null;
            Resource res = GetResource(UserOrGroupId);
            Type resourceType = res.GetType();
            if (resourceType.Name == "Group")
            {
                Group group = (Group)res;
                resources = group.GetResourcesWithExplicitPermissions(_context, userToken);
            }
            else
            {
                Identity identity = (Identity)res;
                resources = identity.GetResourcesWithExplicitPermissions(_context, userToken);
            }


            if (!string.IsNullOrEmpty(filterCriteria))
            {
                filterCriteria = filterCriteria.ToLowerInvariant();
                resources = resources.Where(tuple => tuple.Title.ToLowerInvariant().Contains(filterCriteria));
            }
            return resources;
        }

        public bool UpdateIdentityForUser(List<String> groupList, ZentityUser user, Guid identityId)
        {
            try
            {
                Identity identity = (Identity)GetResource(identityId);

                identity.RelationshipsAsSubject.Load();
                List<Relationship> relationships = identity.RelationshipsAsSubject.ToList();

                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                identity.RelationshipsAsObject.Load();
                relationships = identity.RelationshipsAsObject.ToList();

                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                identity.IdentityName = user.LogOnName;
                identity.Title = user.Profile.FirstName + " " + user.Profile.LastName;

                foreach (String eachResourceId in groupList)
                {
                    Resource group = GetResource(new Guid(eachResourceId));

                    identity.Groups.Add((Group)group);
                }

                _context.SaveChanges();

                return true;
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(Resources.Resources.ConcurrentAccess, exception);
            }
        }


        public bool SetPermissionToResource(string resourceId, List<PermissionMap> permissionList, string identityOrGroupNameId, AuthenticatedToken userToken)
        {
            bool result = false;
            Resource resource = GetResource(new Guid(resourceId));
            Resource securityObject = GetResource(new Guid(identityOrGroupNameId));
            if (securityObject is Identity)
            {
                Identity identity = (Identity)securityObject;
                result = resource.SetPermissionMap(permissionList, identity, _context, userToken);

            }
            else if (securityObject is Group)
            {
                Group group = (Group)GetResource(new Guid(identityOrGroupNameId));
                result = resource.SetPermissionMap(permissionList, group, _context, userToken);

            }
            _context.SaveChanges();
            return result;
        }


        /// <summary>
        /// Searchs for resources based on given filter ciretiria.
        /// </summary>
        /// <param name="token">Authenticated token</param>
        /// <param name="searchText">Search text</param>
        /// <param name="pageSize">Paze size</param>
        /// <param name="sortProperty">Sorting properties</param>
        /// <param name="parsedRecordCount">Parsed record count</param>
        /// <param name="searchContents">Search contents</param>
        /// <param name="totalRecords">Total records</param>
        /// <returns>list of resources found</returns>
        public IEnumerable<Resource> SearchResources(AuthenticatedToken token, string searchText, int pageSize,
            SortProperty sortProperty, int parsedRecordCount, bool searchContents, out int totalRecords)
        {
            totalRecords = 0;
            IEnumerable<Resource> resources = null;
            SearchEngine search = new SearchEngine(pageSize, true, token);
            if (searchContents)
            {
                var files = search.SearchContent(searchText, _context, sortProperty, parsedRecordCount, out totalRecords);
                if (files != null)
                    resources = files.Select(tuple => tuple as Resource).ToList();
            }
            else
            {
                resources = search.SearchResources(searchText, _context, sortProperty, parsedRecordCount, out totalRecords).ToList();
            }

            if (resources != null && resources.Count() > 0)
            {
                foreach (Resource res in resources)
                {
                    res.Files.Load();
                    ScholarlyWork scholWork = res as ScholarlyWork;
                    if (scholWork != null)
                        scholWork.Authors.Load();
                }
            }

            return resources;
        }

        /// <summary>
        /// Gets user permission for the resources in the specified resource list.
        /// </summary>
        /// <param name="token">Authentication token</param>
        /// <param name="resources">List of resources</param>
        /// <returns>List of resources mapped with user permissions.</returns>
        public IEnumerable<ResourcePermissions<T>> GetResourcePermissions<T>(AuthenticatedToken token, IList<T> resources) where T : Resource
        {
            if (token != null && resources != null)
            {
                if (resources != null && resources.Count > 0 && token != null)
                    return resources.GetPermissions<T>(_context, token);
            }

            return null;
        }

        /// <summary>
        /// Fetches resource object and permissions based on resource Id.
        /// </summary>
        /// <param name="token">authenticated Token.</param>
        /// <param name="resourceId">Resource Id.</param>
        /// <returns>Resource object and permissions.</returns>
        public ResourcePermissions<Resource> GetResourcePermissions(AuthenticatedToken token, Guid resourceId)
        {
            ResourcePermissions<Resource> resPermissons = null;

            if (resourceId != Guid.Empty && token != null)
            {
                Resource resource = this.GetResource(resourceId);
                if (resource != null)
                {
                    resPermissons = resource.GetPermissions(_context, token);
                }
            }

            return resPermissons;
        }

        /// <summary>
        /// Authorize user for for given permission on the specified resource.
        /// </summary>
        /// <param name="token">Authenticated Token</param>
        /// <param name="permission">User Permission</param>
        /// <param name="resourceId">Resource Id</param>
        /// <returns>True, if user is Authorized, else false.</returns>
        public bool AuthorizeUser(AuthenticatedToken token, string permission, Guid resourceId)
        {
            if (token != null && resourceId != Guid.Empty)
            {
                Resource res = GetResource(resourceId);
                if (res != null)
                {
                    return res.Authorize(permission, _context, token);
                }
            }
            return false;
        }

        /// <summary>
        ///Checks whether user is having create permission or not.
        /// </summary>
        /// <param name="token">Authenticated Token.</param>
        /// <returns>True, if user is having create permission, else false.</returns>
        public bool HasCreatePermission(AuthenticatedToken token)
        {
            if (token != null)
                return AuthorizationManager.HasCreatePermission(token, _context);

            return false;
        }

        /// <summary>
        /// Authorizes user for delete permission on Category object and it hierarchy.
        /// </summary>
        /// <param name="token">Authenticated token.</param>
        /// <param name="categoryId">Category Id.</param>
        /// <returns>True if user is authorized, else false.</returns>
        public bool AuthorizeUserForDeletePermissionOnCategory(AuthenticatedToken token, Guid categoryId)
        {
            bool success = false;

            if (categoryId != Guid.Empty)
            {
                CategoryNode parentCat = GetResource(categoryId) as CategoryNode;
                if (parentCat != null)
                    success = CheckForDeletePermissionOnCategory(token, parentCat);
            }

            return success;
        }

        private bool CheckForDeletePermissionOnCategory(AuthenticatedToken token, CategoryNode parentCategory)
        {
            bool success = true;

            if (AuthorizeUser(token, UserResourcePermissions.Delete, parentCategory.Id))
            {
                parentCategory.Children.Load();
                foreach (CategoryNode childNode in parentCategory.Children)
                {
                    success = CheckForDeletePermissionOnCategory(token, childNode);
                    if (!success)
                        break;
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Fetches CategoryNode with its hierarchy loaded.
        /// </summary>
        /// <param name="categoryNodeId">Root CategoryNode.</param>
        /// <returns>CategoryNode with its hierarchy loaded.</returns>
        public CategoryNode GetCategoryNodeWithHierarchy(Guid categoryNodeId)
        {
            CategoryNode categoryNode = GetResource(categoryNodeId) as CategoryNode;

            if (categoryNode != null)
            {
                categoryNode.Children.Load();
                categoryNode = LoadChildrenNodes(categoryNode);
            }

            return categoryNode;
        }

        /// <summary>
        /// Load Child nodes.
        /// </summary>
        /// <param name="rootNode">Node for which load childs.</param>
        /// <returns>Returns CategoryNode Root</returns>
        private CategoryNode LoadChildrenNodes(CategoryNode rootNode)
        {
            rootNode.Children.Load();

            foreach (CategoryNode current in rootNode.Children)
            {
                LoadChildrenNodes(current);
            }
            return rootNode;
        }

        /// <summary>
        /// Deletes Hierarchy of specified CategoryNode.
        /// </summary>
        /// <param name="categoryNodeId">ID of CategoryNode to be deleted.</param>
        /// <returns></returns>
        public bool DeleteCategoryNodeWithHierarchy(Guid categoryNodeId)
        {
            CategoryNode categoryNode = GetCategoryNodeWithHierarchy(categoryNodeId);
            if (categoryNode == null)
            {
                throw new EntityNotFoundException(EntityType.Resource, categoryNodeId);
            }
            else
            {
                DeleteCategoryNode(categoryNode);
            }

            if (_context.SaveChanges() > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Deletes CategoryNodes recursively
        /// </summary>
        /// <param name="categoryNode">Root node of CatagoryNode hierarchy to be deleted.</param>
        private void DeleteCategoryNode(CategoryNode categoryNode)
        {
            if (categoryNode != null)
            {
                categoryNode.RelationshipsAsSubject.Load();
                categoryNode.RelationshipsAsObject.Load();

                foreach (CategoryNode childNode in categoryNode.Children.ToList())
                {
                    DeleteCategoryNode(childNode);
                }

                foreach (Relationship relationship in
                    categoryNode.RelationshipsAsSubject.Union(categoryNode.RelationshipsAsObject).ToList())
                {
                    _context.DeleteObject(relationship);
                }

                // Delete associated Resource propertes
                categoryNode.ResourceProperties.Load();
                List<ResourceProperty> resProperties = categoryNode.ResourceProperties.ToList();
                foreach (ResourceProperty property in resProperties)
                {
                    categoryNode.ResourceProperties.Remove(property);
                    _context.DeleteObject(property);
                }

                _context.DeleteObject(categoryNode);
            }
        }


        public IQueryable<T> GetAllSecurityObjects<T>(Guid resourceId) where T : Resource
        {
            List<string> predicateUri = PermissionManager.GetSecurityPredicates().ToList();
            return this._context.Relationships.Where(ContainsExpression<Relationship, string>(tuple => tuple.Predicate.Uri, predicateUri))
                       .Where(tuple => tuple.Object.Id == resourceId)
                       .Select(tuple => tuple.Subject).OfType<T>();
        }
        public IEnumerable GetUsersOrGroupPremissions(string resourceId, string strUserOrGroupId, AuthenticatedToken userToken)
        {
            Resource resource = GetResource(new Guid(resourceId));
            Resource securityObject = GetResource(new Guid(strUserOrGroupId));
            IEnumerable<PermissionMap> permissionMapping = null;
            if (securityObject is Identity)
            {
                Identity identity = (Identity)securityObject;
                permissionMapping = resource.GetPermissionMap(identity, _context, userToken);
            }
            else if (securityObject is Group)
            {
                Group group = (Group)securityObject;
                permissionMapping = resource.GetPermissionMap(group, _context, userToken);
            }

            if (permissionMapping != null)
            {
                Dictionary<string, int> hierarchy = PermissionManager.GetPermissionsHierarchy();
                return (from pm in permissionMapping
                        join ph in hierarchy on pm.Permission equals ph.Key
                        select new { pm.Permission, pm.Allow, pm.Deny, Priority = ph.Value });
            }
            return null;
        }

        public List<Resource> GetResourcesForUserOrGroup(Guid userOrGroupId)
        {
            List<string> predicateUri = PermissionManager.GetSecurityPredicates().ToList();
            return this._context.Relationships.Where(
                        ContainsExpression<Relationship, string>(tuple => tuple.Predicate.Uri, predicateUri))
                       .Where(tuple => tuple.Subject.Id == userOrGroupId)
                       .Select(tuple => tuple.Object).ToList();
        }

        public IEnumerable<PermissionMap> GetCreatePremissions(string resourceId, AuthenticatedToken userToken)
        {
            List<PermissionMap> maps = new List<PermissionMap>();
            PermissionMap map = null;
            Resource resource = GetResource(new Guid(resourceId));
            if (resource is Identity)
            {
                Identity identity = (Identity)resource;
                map = identity.GetCreatePermissionMap(_context, userToken);
            }
            else if (resource is Group)
            {
                Group group = (Group)resource;
                map = group.GetCreatePermissionMap(_context, userToken);
            }

            if (map != null)
            {
                maps.Add(map);
            }

            return maps;
        }

        public bool SetCreatePremissions(PermissionMap permissionMap, string resourceId, AuthenticatedToken userToken)
        {
            bool result = false;
            Resource resource = GetResource(new Guid(resourceId));

            if (resource is Identity)
            {
                Identity identity = (Identity)resource;
                result = identity.SetCreatePermissionMap(permissionMap, _context, userToken);
            }
            else if (resource is Group)
            {
                Group group = (Group)resource;
                result = group.SetCreatePermissionMap(permissionMap, _context, userToken);
            }

            _context.SaveChanges();
            return result;
        }

        public List<Resource> GetResourcesRelatedToUser(Identity user)
        {
            return GetResourcesForUserOrGroup(user.Id);
        }
        public List<Resource> GetResourcesRelatedToUser(Group groupId)
        {
            return GetResourcesForUserOrGroup(groupId.Id);
        }

        private Expression<Func<T, bool>> ContainsExpression<T, V>(
                Expression<Func<T, V>> valueSelector,
                IEnumerable<V> values)
        {
            ParameterExpression p = valueSelector.Parameters.Single();
            if (!values.Any())
                return e => false;

            var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body,
                                        Expression.Constant(value, typeof(V))));

            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));

            return Expression.Lambda<Func<T, bool>>(body, p);
        }


        public bool CreateUser(ZentityUser user, AuthenticatedToken token)
        {
            return Zentity.Security.AuthorizationHelper.UserManager.CreateUser(user, token);
        }

        public bool IsAdmin(AuthenticatedToken token)
        {
            return token.IsAdmin(_context);
        }

        public bool IsOwner(AuthenticatedToken token, Resource resource)
        {
            return token.IsOwner(resource, _context);
        }

        public bool IsOwner(AuthenticatedToken token, Guid resourceId)
        {
            bool isOwner = false;

            Resource res = GetResource(resourceId);

            if (res != null)
            {
                isOwner = token.IsOwner(res, _context);
            }

            return isOwner;

        }

        public bool CreateGroup(Group group, List<String> seletcedList, AuthenticatedToken token)
        {
            return UserManager.CreateGroup(group, token);
        }

        public bool AddIdentityToGroup(Group group, Identity identity, AuthenticatedToken token)
        {
            return UserManager.AddIdentityToGroup(identity, group, token);
        }

        public bool RemoveIdentityFromGroup(Identity identity, Group group, AuthenticatedToken token)
        {
            if (!(UserManager.AdminUserName == identity.IdentityName || UserManager.GuestUserName == identity.IdentityName))
                return UserManager.RemoveIdentityFromGroup(identity, group, token);

            return true;
        }

        public bool RemoveGroupFromIdentity(Guid Id)
        {
            try
            {
                Identity resource = (Identity)GetResource(Id);

                resource.RelationshipsAsSubject.Load();
                List<Relationship> relationships = resource.RelationshipsAsSubject.ToList();

                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                //Delete relationships as a object
                resource.RelationshipsAsObject.Load();
                relationships = resource.RelationshipsAsObject.ToList();

                foreach (Relationship relation in relationships)
                {
                    _context.DeleteObject(relation);
                }

                _context.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Group GetGroup(string groupName)
        {
            Group group = UserManager.GetGroup(groupName, _context);
            return group;
        }

        public bool UpdateGroup(Group group, AuthenticatedToken token)
        {
            return UserManager.UpdateGroup(group, token);
        }

        public bool GrantDefaultOwnership(AuthenticatedToken token, Guid resourceId)
        {
            bool success = false;
            Resource resource = GetResource(resourceId);
            if (resource != null)
            {
                if (resource.GrantDefaultPermissions<Resource>(_context, token))
                {
                    _context.SaveChanges();
                    success = true;
                }
            }

            return success;
        }

        public IEnumerable<SimilarRecord> SearchSimilarResource(string resourceType, IEnumerable<PropertyValuePair> searchCriteria, AuthenticatedToken token, int totalParsedRecords, out int totalRecords, int pageSize)
        {
            SearchEngine search = null;
            if (token == null)
            {
                search = new SearchEngine();
            }
            else
            {
                search = new SearchEngine(true, token);
            }

            search.MaximumResultCount = pageSize;
            return search.SearchSimilarResources(resourceType, searchCriteria,
                _context, totalParsedRecords, out totalRecords);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!isDisposed && this._context != null)
            {
                this._context.Dispose();
                isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
