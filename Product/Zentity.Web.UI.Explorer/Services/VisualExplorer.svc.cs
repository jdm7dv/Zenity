// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Zentity.Core;
    using Zentity.Platform;
    using Zentity.Services.Web;
    using Zentity.Web.UI.Explorer.DataModelingServiceReference;
    using Zentity.Web.UI.Explorer.ResourceTypeServiceReference;

    /// <summary>
    /// VisualExplorer service used by the Silverlight client
    /// </summary>
    public class VisualExplorer : IVisualExplorerService
    {
        #region IVisualExplorer Members
        /// <summary>
        /// Zentity.Security.Authorization model namespace
        /// </summary>
        private const string ZentitySecurityAuthorization = "Zentity.Security.Authorization";

        /// <summary>
        /// Gets VisualExplorer graph by ResourceId
        /// </summary>
        /// <param name="resourceId">ResourceId from Zentity store</param>
        /// <returns>VisualExplorerGraph object</returns>
        public VisualExplorerGraph GetVisualExplorerGraphByResourceId(string resourceId)
        {
            try
            {
                VisualExplorerHelper helper = new VisualExplorerHelper();
                return helper.GetVisualExplorerGraphByResourceId(resourceId);
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
            }

            return null;
        }

        /// <summary>
        /// Get metadata for the specified ResourceId
        /// </summary>
        /// <param name="resourceId">ResourceId from Zentity store</param>
        /// <returns>Resource metadata</returns>
        public string GetResourceMetadataByResourceId(string resourceId)
        {
            try
            {
                VisualExplorerHelper helper = new VisualExplorerHelper();
                Dictionary<string, string> metadata = helper.GetResourceMetadataByResourceId(resourceId);

                if (metadata != null)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
                    MemoryStream stream = new MemoryStream();
                    serializer.WriteObject(stream, metadata);

                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
            }

            return string.Empty;
        }

        /// <summary>
        /// Get Resource relation by ResourceId
        /// </summary>
        /// <param name="subjectResourceId">Identifier of the subject resource</param>
        /// <param name="objectResourceId">Identifier of the object resource</param>
        /// <returns>Resource relation metadata</returns>
        public string GetResourceRelationByResourceId(string subjectResourceId, string objectResourceId)
        {
            try
            {
                VisualExplorerHelper helper = new VisualExplorerHelper();
                List<string> result = helper.GetResourceRelationByResourceId(subjectResourceId, objectResourceId);

                if (result != null)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<string>));
                    MemoryStream stream = new MemoryStream();
                    serializer.WriteObject(stream, result);

                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets VisualExplorer graph by the searched keyword
        /// </summary>
        /// <param name="keyword">Searched keyword</param>
        /// <returns>VisualExplorerGraph object</returns>
        public VisualExplorerGraph GetVisualExplorerGraphBySearchKeyword(string keyword)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                try
                {
                    using (ZentityContext context = Zentity.Services.Web.Utilities.CreateZentityContext())
                    {
                        SearchEngine search = new SearchEngine(1, false, null);
                        int totalRecords;
                        SortProperty sortProperty = new SortProperty("Title", SortDirection.Ascending);
                        IEnumerable<Resource> resources = search.SearchResources(keyword, context, sortProperty, 0, out totalRecords);

                        if (resources != null && resources.Count() > 0)
                        {
                            return this.GetVisualExplorerGraphByResourceId(resources.First().Id.ToString());
                        }
                    }
                }
                catch (Exception exception)
                {
                    Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// Get resources by search keyword
        /// </summary>
        /// <param name="keyword">search keyword</param>
        /// <returns>List of resources</returns>
        public string GetResourcesByKeyword(string keyword)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                try
                {
                    using (ZentityContext context = Zentity.Services.Web.Utilities.CreateZentityContext())
                    {
                        SearchEngine search = new SearchEngine(10, false, null);
                        int totalRecords;
                        SortProperty sortProperty = new SortProperty("Title", SortDirection.Ascending);
                        IEnumerable<Resource> resources = search.SearchResources(string.Format(@"Title: {0}", keyword), context, sortProperty, 0, out totalRecords);
                        if (resources != null && resources.Count() > 0)
                        {
                            List<KeyValuePair<Guid, string>> results = new List<KeyValuePair<Guid, string>>(10);
                            Parallel.ForEach(
                                    resources, 
                                    resource =>
                                    {
                                        results.Add(new KeyValuePair<Guid, string>(resource.Id, resource.Title));
                                    });

                            results.Insert(0, new KeyValuePair<Guid, string>(Guid.Empty, keyword));
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<KeyValuePair<Guid, string>>));
                            MemoryStream stream = new MemoryStream();
                            serializer.WriteObject(stream, results);

                            return Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
                catch (Exception exception)
                {
                    Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get List of ResourceTypes and RelationTypes in Zentity store
        /// </summary>
        /// <returns>VisualExplorerFilterList object</returns>
        public VisualExplorerFilterList GetVisualExplorerFilterList()
        {
            VisualExplorerFilterList filterList = new VisualExplorerFilterList();
            try
            {
                List<string> dataModels;
                using (DataModelServiceClient dataModelService = new DataModelServiceClient())
                {
                    dataModels = dataModelService.GetAllDataModels().ToList();
                    if (dataModels.Contains(ZentitySecurityAuthorization))
                    {
                        dataModels.Remove(ZentitySecurityAuthorization);
                    }
                }

                List<string> resourceTypes = new List<string>();
                List<string> relationshipTypes = new List<string>();

                using (ResourceTypeServiceClient resourceTypeService = new ResourceTypeServiceClient())
                {
                    foreach (var dataModel in dataModels)
                    {
                        resourceTypes.AddRange(resourceTypeService.GetAllResourceTypesByNamespace(dataModel).Select(rt => string.Format("{0}.{1}", dataModel, rt.Name)));
                    }
                }

                using (ZentityContext context = Utilities.CreateZentityContext())
                {
                    relationshipTypes.AddRange(
                        context.Predicates.Where(predicateFilter =>
                            (!predicateFilter.Uri.Contains("urn:zentity/module/zentity-authorization/predicate")))
                            .AsEnumerable().Select(predicate => predicate.Name));
                }

                resourceTypes.Sort();
                relationshipTypes.Sort();
                filterList.ResourceTypes = resourceTypes;
                filterList.RelationShipTypes = relationshipTypes;
                return filterList;
            }
            catch (Exception exception)
            {
                Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
            }

            return null;
        }
        #endregion
    }

    /// <summary>
    /// VisualExplorer filter list: Represents the list of ResourceTypes and RelationShipTypes which can be filtered on
    /// </summary>
    public class VisualExplorerFilterList
    {
        /// <summary>
        /// Gets or sets list of ResourceTypes
        /// </summary>
        public IList<string> ResourceTypes { get; set; }

        /// <summary>
        /// Gets or sets list of RelationShipTypes
        /// </summary>
        public IList<string> RelationShipTypes { get; set; }
    }
}
