// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using System.IO;
using System.Data;
using System.Collections;
using System.Data.Objects;
using System.Globalization;
using System.Collections.ObjectModel;
using Zentity.Platform;

using Zentity.Web.UI.ToolKit.Resources;

using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Zentity.Security.Authentication;
using Zentity.Security.AuthorizationHelper;
using Zentity.Security.Authorization;
using System.Web.Caching;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Summary description for ResourceDAL.
    /// </summary>
    internal sealed class ResourceDataAccess : IDisposable
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
        private const string _namespaceZentityCore = "Zentity.Core.";
        private const string _relationshipAsObject = "RelationshipsAsObject.Predicate";
        private const string _group = "Group";
        private const string _identity = "Identity";
        private const string _tag = "Tag";
        private const string _categoryNode = "CategoryNode";

        #region Complied LINQ Queries

        const string _getResourcesByResourceTypeQuery = @"Select value r from ofType 
                            (ZentityContext.Resources,{0})
                            as r Order By r.Title Skip({1}) Limit({2})";

        const string _authorsRangeQuery =
            @"(select value contact from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Contact) as contact 
            where contact.Title Like('[{0}]%')) 
            union 
            (select value person from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Person) as person 
            where person.FirstName Like('[{0}]%'))";

        const string _authorsSubStringQuery =
            @"(select value contact from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Contact) as contact 
            where contact.Title Like('%{0}%')) 
            union 
            (select value person from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Person) as person 
            where  person.FirstName Like('%{0}%'))";


        string _authorsRangeAndSubstringQuery =
            @"(select value contact from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Contact) as contact 
            where contact.Title Like('[{0}]%') && contact.Title Like('%{1}%')) 
            union 
            (select value person from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Person) as person 
            where person.FirstName Like('[{0}]%') && person.FirstName Like('%{1}%'))";

        string _otherAuthorsQuery =
            @"(select value contact from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Contact) as contact 
            where contact.Title not Like('[A-Z]%')) 
            union
            (select value person from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Person) as person 
            where person.FirstName not Like('[A-Z]%'))";

        string _otherAuthorsWithSubStringQuery =
            @"(select value contact from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Contact) as contact 
            where contact.Title not Like('[A-Z]%') && contact.Title Like('%{0}%')) 
            union
            (select value person from OFTYPE(ZentityContext.Resources, Zentity.ScholarlyWorks.Person) as person 
            where person.FirstName not Like('[A-Z]%') && person.FirstName Like('%{0}%'))";


        internal const string AuthoredByPredicateUri = "urn:zentity/module/zentity-scholarly-works/association/scholarly-work-is-authored-by";

        private static Func<ZentityContext, Guid, IQueryable<Resource>> _getResourceById =
            CompiledQuery.Compile((ZentityContext _context, Guid id) =>
            (_context.Resources.Where(tuple => tuple.Id == id)));

        private static Func<ZentityContext, Guid, IQueryable<ScholarlyWorkItem>> _getScholarlyWorkItemById =
            CompiledQuery.Compile((ZentityContext _context, Guid id) =>
            (_context.Resources.OfType<ScholarlyWorkItem>().Where(tuple => tuple.Id == id)));

        private static Func<ZentityContext, Guid, Guid, IOrderedQueryable<Relationship>> _getRelationshipsAsSubject =
          CompiledQuery.Compile((ZentityContext _context, Guid predicateId, Guid SubjectId) =>
          (_context.Relationships
                    .Where(r => (r.Predicate.Id == predicateId && r.Subject.Id == SubjectId))
                    .OrderBy(r => r.OrdinalPosition)));

        private static Func<ZentityContext, Guid, Guid, IOrderedQueryable<Relationship>> _getRelationshipsAsObject =
          CompiledQuery.Compile((ZentityContext _context, Guid predicateId, Guid objectId) =>
          (_context.Relationships
                    .Where(r => (r.Predicate.Id == predicateId && r.Object.Id == objectId))
                    .OrderBy(r => r.OrdinalPosition)));

        private static Func<ZentityContext, Guid, IQueryable<CategoryNode>> _getCategoryNodeById =
            CompiledQuery.Compile((ZentityContext _context, Guid id) =>
            (_context.Resources.OfType<CategoryNode>().Where(tuple => tuple.Id == id)));

        private static Func<ZentityContext, ObjectQuery<CategoryNode>> _getCategoryRootNode =
            CompiledQuery.Compile((ZentityContext _context) =>
            (_context.Resources.OfType<CategoryNode>()));

        private static Func<ZentityContext, Guid, IQueryable<Zentity.Core.File>> _getFileById =
            CompiledQuery.Compile((ZentityContext _context, Guid id) =>
            (_context.Resources.OfType<Zentity.Core.File>().Where(tuple => tuple.Id == id)));

        private static Func<ZentityContext, IQueryable<Resource>> _allResources =
            CompiledQuery.Compile((ZentityContext _context) =>
            (_context.Resources.Where(tuple => !(tuple is Identity) && !(tuple is Zentity.Security.Authorization.Group))));

        private static Func<ZentityContext, IOrderedQueryable<CategoryNode>> _getCategoryNodesQuery =
            CompiledQuery.Compile((ZentityContext _context) =>
            (_context.Resources.OfType<CategoryNode>().Include("Children").OrderBy(tuple => tuple.Title)));

        private static Func<ZentityContext, IQueryable<Contact>> _getContactsQuery =
            CompiledQuery.Compile((ZentityContext _context) =>
            (_context.Resources.OfType<Contact>().Where(tuple => tuple.AuthoredWorks.Count() > 0)));

        private static Func<ZentityContext, int, int, IQueryable<Resource>> _getResourcesByMonthAndYear =
            CompiledQuery.Compile((ZentityContext _context, int selectedMonth, int selectedYear) =>
                (_context.Resources.Where(tuple => tuple.DateAdded.Value.Month == selectedMonth
                  && tuple.DateAdded.Value.Year == selectedYear)));

        private static Func<ZentityContext, int, IQueryable<Resource>> _getResourcesByYear =
            CompiledQuery.Compile((ZentityContext _context, int selectedYear) =>
                (_context.Resources.Where(tuple => tuple.DateAdded.Value.Year == selectedYear)));


        private static Func<ZentityContext, Guid, IQueryable<Contact>> _getContactById =
            CompiledQuery.Compile((ZentityContext _context, Guid id) =>
            (_context.Resources.OfType<Contact>().Where(tuple => tuple.Id == id)));

        static Func<ZentityContext, Guid, IQueryable<ScholarlyWork>> _getCitedScholarlyWorksCompiledQuery =
          CompiledQuery.Compile((ZentityContext _context, Guid id) =>
          (_context.Resources.OfType<ScholarlyWork>().Include("Cites").Where(tuple => tuple.Id == id)));

        static Func<ZentityContext, IQueryable<Resource>> _getLatestAddedResCompiledQuery =
         CompiledQuery.Compile((ZentityContext _context) =>
             _context.Resources.Where(tuple =>
                 !(tuple is Tag || tuple is CategoryNode || tuple is Person || tuple is Organization || tuple is Contact)));

        static Func<ZentityContext, IQueryable<Resource>> _getLatestModifiedResCompiledQuery =
                CompiledQuery.Compile((ZentityContext _context) =>
                    _context.Resources
                .Where(tuple => !(tuple is Tag || tuple is CategoryNode || tuple is Person || tuple is Organization || tuple is Contact)
                    && tuple.DateModified.HasValue && tuple.DateModified != tuple.DateAdded));

        static Func<ZentityContext, int, IQueryable<Tag>> _getTopTagsCompiledQuery =
           CompiledQuery.Compile((ZentityContext _context, int pageSize) =>
               _context.Resources.OfType<Tag>()
                 .Where(tags => tags.ScholarlyWorkItems.Count() > 0)
               .OrderByDescending(tag => tag.ScholarlyWorkItems.Count())
               .Take(pageSize));


        static Func<ZentityContext, int, IQueryable<Contact>> _getTopAuthorsCompiledQuery =
            CompiledQuery.Compile((ZentityContext _context, int pageSize) =>
                _context.Resources.OfType<Contact>()
                 .Include(_relationshipAsObject)
                 .Select(person => new
                 {
                     Person = person,
                     AuthoredResourcesCount = person.RelationshipsAsObject
                         .Where(tuple => tuple.Predicate.Uri == AuthoredByPredicateUri).Count()
                 })
                 .Where(person => person.AuthoredResourcesCount > 0)
                 .OrderByDescending(author => author.AuthoredResourcesCount)
                 .Select(author => author.Person)
                 .Take(pageSize));

        static Func<ZentityContext, Guid, IEnumerable<string>> _getTypeNameCompiledQuery =
            CompiledQuery.Compile((ZentityContext _context, Guid typeId) =>
            _context.DataModel.Modules.
                  SelectMany(tuple => tuple.ResourceTypes).
                  Where(tuple => tuple.Id == typeId).Select(tuple => tuple.Name));


        private static Func<ZentityContext, Guid, IQueryable<Predicate>> _getPredicateById =
            CompiledQuery.Compile((ZentityContext _context, Guid id) =>
            (_context.Predicates.Where(tuple => tuple.Id == id)));
        #endregion

        #endregion

        #endregion

        #region Constructors & finalizers

        #region public

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

        #region Public

        /// <summary>
        /// Fetches resource object based on resource Id.
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <returns>Resource object</returns>
        /// <exception cref="EntityNotFoundException">Thrown when a resource with the specified 
        /// id is not found.</exception>
        public Resource GetResource(Guid resourceId)
        {
            IQueryable<Resource> resource = _getResourceById.Invoke(_context, resourceId);
            Resource foundResource = resource.FirstOrDefault();

            if (foundResource == null)
            {
                throw new EntityNotFoundException(EntityType.Resource, resourceId);
            }
            return foundResource;
        }

        /// <summary>
        /// Fetches predicate object based on predicate Id.
        /// </summary>
        /// <param name="predicateId">Predicate Id</param>
        /// <returns>Predicate Object</returns>
        public Predicate GetPredicate(Guid predicateId)
        {
            IQueryable<Predicate> predicate = _getPredicateById.Invoke(_context, predicateId);
            Predicate foundPredicate = predicate.FirstOrDefault();

            if (foundPredicate == null)
            {
                throw new EntityNotFoundException(EntityType.Resource, predicateId);
            }
            return foundPredicate;
        }


        /// <summary>
        /// Fetches resource object with loaded categorynode. 
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="userPermission">Permission name.</param>
        /// <returns>Resource object</returns>
        /// <exception cref="EntityNotFoundException">Thrown when a resource with the specified 
        /// id is not found.</exception>
        public ScholarlyWorkItem GetScholarlyWorkItem(Guid resourceId, AuthenticatedToken token, string userPermission)
        {
            IQueryable<ScholarlyWorkItem> resource = _getScholarlyWorkItemById.Invoke(_context, resourceId);

            ScholarlyWorkItem foundResource = resource.FirstOrDefault();

            if (foundResource == null)
            {
                throw new EntityNotFoundException(EntityType.Resource, resourceId);
            }

            AuthorizeResource<ScholarlyWorkItem>(token, userPermission, foundResource, true);
            return foundResource;
        }

        public Resource GetResource(Guid resourceId, string typeFullName)
        {
            Resource resourceObj = null;

            Type classType = GetTypeOfResource(CoreHelper.ExtractNamespace(typeFullName), typeFullName);

            MethodInfo method = this.GetType().GetMethod("GetResource",
                        BindingFlags.NonPublic | BindingFlags.Instance);
            Resource classObj = Activator.CreateInstance(classType, null) as Resource;

            if (method != null)
            {
                resourceObj = (Resource)method.MakeGenericMethod(classObj.GetType()).
                            Invoke(this, new object[] { resourceId });
            }
            else
            {
                throw new MissingMethodException("GetResource");
            }

            if (resourceObj == null)
            {
                throw new EntityNotFoundException(EntityType.Resource, resourceId);
            }

            return resourceObj;
        }

        /// <summary>
        /// Gets a resource by id.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="resourceId">Resource id.</param>
        /// <returns>Resource.</returns>
        private Resource GetResource<T>(Guid resourceId) where
            T : Core.Resource
        {

            return _context.Resources.OfType<T>()
                .Where(tuple => tuple.Id == resourceId).FirstOrDefault();
        }

        /// <summary>
        ///     Returns type of Resource with full name.
        /// </summary>
        /// <param name="namespaceName"> Name of namespace to which the clas belongs</param>
        /// <param name="className">Class name</param>
        /// <returns>Type of resource with full name</returns>
        private static Type GetTypeOfResource(string namespaceName, string className)
        {
            Type resourceSystemType = null;

            Assembly zentityAssembly = Assembly.Load(namespaceName);

            resourceSystemType = zentityAssembly.GetType(className);

            return resourceSystemType;
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
        /// Returns is it valid resource type.
        /// </summary>
        /// <param name="resourceType">Name of the resource type.</param>
        /// <returns>Return true if resource type is valid.</returns>
        public bool IsValidResourceType(string resourceType)
        {
            return GetResourceType(resourceType) != null;
        }

        /// <summary>
        /// Fetches all object resources for given subject resource and predicate.
        /// </summary>
        /// <param name="resourceId">Subject resource Id</param>
        /// <param name="property">Navigation property</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="userPermission">Permission name.</param>
        /// <returns>List of object resources.</returns>
        public List<Resource> GetRelatedResources(Guid resourceId, NavigationProperty property, AuthenticatedToken token, string userPermission)
        {
            List<Resource> resources = new List<Resource>();

            if (property.Direction == AssociationEndType.Subject)
            {
                GetRelatedResourcesAsSubject(resourceId, property, resources);
            }
            else if (property.Direction == AssociationEndType.Object)
            {
                GetRelatedResourcesAsObject(resourceId, property, resources);
            }

            if (token != null)
            {
                return GetAuthorizedResources<Resource>(token, userPermission, resources).ToList();
            }
            else
            {
                return resources;
            }
        }

        private void GetRelatedResourcesAsSubject(Guid resourceId, NavigationProperty property, List<Resource> resources)
        {
            List<Relationship> relations = _getRelationshipsAsSubject(_context, property.Association.PredicateId, resourceId).ToList();

            if (relations != null)
            {
                foreach (Relationship rel in relations)
                {

                    Resource resource = GetResource(new Guid(rel.ObjectReference.EntityKey.EntityKeyValues[0].Value.ToString()));
                    resources.Add(resource);
                }
                CoreHelper.UpdateResourcesEmptyTitle(resources);
            }
        }

        private void GetRelatedResourcesAsObject(Guid resourceId, NavigationProperty property, List<Resource> resources)
        {
            List<Relationship> relations = _getRelationshipsAsObject(_context, property.Association.PredicateId, resourceId)
                .ToList();
            if (relations != null)
            {
                foreach (Relationship rel in relations)
                {

                    Resource resource = GetResource(new Guid(rel.SubjectReference.EntityKey.EntityKeyValues[0].Value.ToString()));
                    resources.Add(resource);
                }
                CoreHelper.UpdateResourcesEmptyTitle(resources);
            }
        }

        /// <summary>
        /// Returns a list of tags associated with a scholarly work item.
        /// </summary>
        /// <param name="scholarlyItemId">ScholarlyWorkItem id.</param>
        /// <param name="token">Authenticated token.</param>
        /// <returns>List of tags.</returns>
        public List<Tag> GetScholarlyWorkItemTags(Guid scholarlyItemId, AuthenticatedToken token)
        {
            List<Tag> tags = new List<Tag>();

            ScholarlyWorkItem scholarlyWorkItem = (ScholarlyWorkItem)GetResource(scholarlyItemId);
            scholarlyWorkItem.Tags.Load();
            tags = scholarlyWorkItem.Tags.ToList();

            return GetAuthorizedResources<Tag>(token, UserResourcePermissions.Update, tags).ToList();
        }

        /// <summary>
        /// Returns a list of scholarlyWorkItem associated with a scholarly work item.
        /// </summary>
        /// <param name="scholarlyItemId">ScholarlyWorkItem id.</param>
        /// <param name="token">Authenticated token.</param>
        /// <returns>List of categories.</returns>
        public List<CategoryNode> GetScholarlyWorkItemCategoryNodes(Guid scholarlyItemId, AuthenticatedToken token)
        {
            List<CategoryNode> categoryNodes = new List<CategoryNode>();

            ScholarlyWorkItem scholarlyWorkItem = (ScholarlyWorkItem)GetResource(scholarlyItemId);
            scholarlyWorkItem.CategoryNodes.Load();
            categoryNodes = scholarlyWorkItem.CategoryNodes.ToList();

            return GetAuthorizedResources<CategoryNode>(token, UserResourcePermissions.Update, categoryNodes).ToList();
        }

        /// <summary>
        /// Saves resource tag association.
        /// </summary>
        /// <param name="scholarlyWorkItemId">Subject resource id.</param>
        /// <param name="destinationList">Object tags/categories.</param>        
        /// <returns>Boolean value indicating success or failure.</returns>
        public bool SaveScholarlyWorkItemTagAssociation(Guid scholarlyWorkItemId, IList<Tag> destinationList)
        {
            bool result = false;

            List<Tag> prevTags = new List<Tag>();

            ScholarlyWorkItem scholarlyItemObj = null;

            if (destinationList != null)
            {
                scholarlyItemObj = (ScholarlyWorkItem)GetResource(scholarlyWorkItemId, Constants.ScholarlyWorkItemFullName);

                if (scholarlyItemObj != null)
                {
                    scholarlyItemObj.Tags.Load();
                    prevTags = scholarlyItemObj.Tags.ToList();

                    foreach (Tag tagObj in prevTags)
                    {
                        scholarlyItemObj.Tags.Remove(tagObj);
                    }

                    foreach (Tag tagObj in destinationList)
                    {
                        Tag tagToInclude = _context.Resources.OfType<Tag>().Where(tuple => tuple.Id == tagObj.Id).FirstOrDefault(); //prevTags.Where(tuple => tuple.Id == tagObj.Id).FirstOrDefault();

                        if (tagToInclude != null)
                        {
                            scholarlyItemObj.Tags.Add(tagToInclude);
                        }
                    }
                    scholarlyItemObj.DateModified = DateTime.Now;
                    _context.SaveChanges();
                    result = true;
                }
            }

            return result;
        }


        /// <summary>
        /// Saves resource category association.
        /// </summary>
        /// <param name="scholarlyWorkItemId">Subject resource id.</param>
        /// <param name="destinationList">bject tags/categories.</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="userPermission"></param>
        /// <returns>Boolean value indicating success or failure.</returns>
        public bool SaveScholarlyItemCategoryAssociation(Guid scholarlyWorkItemId,
                           IList<CategoryNode> destinationList, AuthenticatedToken token, string userPermission)
        {
            bool result = false;

            List<CategoryNode> prevCategories = new List<CategoryNode>();

            ScholarlyWorkItem scholarlyItemObj = null;

            if (destinationList != null)
            {
                scholarlyItemObj = (ScholarlyWorkItem)
                    GetResource(scholarlyWorkItemId, Constants.ScholarlyWorkItemFullName);


                if (scholarlyItemObj != null)
                {
                    scholarlyItemObj.CategoryNodes.Load();
                    prevCategories = scholarlyItemObj.CategoryNodes.ToList();

                    foreach (CategoryNode categoryNodeObj in prevCategories)
                    {
                        CategoryNode categoryNodeToRemove = destinationList.Where
                            (tuple => tuple.Id == categoryNodeObj.Id).FirstOrDefault();

                        // The category node has to be removed only if the user has requested for it and 
                        // he has read access on it.
                        if (categoryNodeToRemove == null &&
                            AuthorizeResource<CategoryNode>(token, userPermission, categoryNodeObj, false))
                        {
                            scholarlyItemObj.CategoryNodes.Remove(categoryNodeObj);
                        }
                    }

                    foreach (CategoryNode categoryNodeObj in destinationList)
                    {
                        CategoryNode categoryToInclude = prevCategories.Where(tuple => tuple.Id == categoryNodeObj.Id).
                            FirstOrDefault();

                        if (categoryToInclude == null)
                        {
                            _context.Attach(categoryNodeObj);
                            scholarlyItemObj.CategoryNodes.Add(categoryNodeObj);
                        }
                    }
                    scholarlyItemObj.DateModified = DateTime.Now;
                    _context.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        ///  Retunrs search result depends on search crieteria.
        /// </summary>
        /// <param name="searchCriteria">Search criteria.</param>
        /// <param name="currentCursor">Current page no.</param>
        /// <param name="pageSize">No. of result required.</param>
        /// <param name="totalRecords">No. of total records.</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="isSecurityAwareControl">Check is security aware.</param>
        /// <returns>Returns matched results.</returns>
        public IEnumerable<Resource> SearchResources(string searchCriteria, int currentCursor,
            int pageSize, out int totalRecords, AuthenticatedToken token, bool isSecurityAwareControl)
        {
            SearchEngine search = new SearchEngine(pageSize, isSecurityAwareControl, token);

            //Fetch and return Resources
            IEnumerable<Resource> resources = search.SearchResources(
                searchCriteria, _context, currentCursor, out totalRecords);
            if (resources != null && resources.Count() > 0)
            {
                foreach (Resource res in resources)
                {
                    res.RelationshipsAsSubject.Load();
                    res.RelationshipsAsObject.Load();

                    ScholarlyWork schol = res as ScholarlyWork;
                    res.Files.Load();
                    if (schol != null)
                    {
                        schol.Authors.Load();
                    }
                }

                CoreHelper.UpdateResourcesEmptyTitle(resources);
            }

            return resources;
        }

        /// <summary>
        /// Retunrs search result depends on search crieteria.
        /// </summary>
        /// <param name="searchCriteria">Search criteria.</param>
        /// <param name="currentCursor">Current page no.</param>
        /// <param name="pageSize">No. of result required.</param>
        /// <param name="totalRecords">No. of total records.</param>
        /// <param name="userPermission">User permission.</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="isSecurityAwareControl">Check is security aware.</param>
        /// <returns>Returns matched results.</returns>
        public IEnumerable<Resource> SearchResources(string searchCriteria, int currentCursor,
            int pageSize, out int totalRecords, string userPermission, AuthenticatedToken token, bool isSecurityAwareControl)
        {
            IEnumerable<Resource> resources = SearchResources(searchCriteria, currentCursor,
                    pageSize, out totalRecords, token, isSecurityAwareControl);

            return GetAuthorizedResources<Resource>(token, userPermission, resources);
        }

        /// <summary>
        /// Retunrs search result depends on search crieteria.
        /// </summary>
        /// <param name="searchCriteria">Search criteria.</param>
        /// <param name="currentCursor">Current page no.</param>
        /// <param name="pageSize">No. of result required.</param>
        /// <param name="sortProperty"></param>
        /// <param name="totalRecords">No. of total records.</param>
        /// <param name="userPermission">User permission.</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="isSecurityAwareControl">Check is security aware.</param>
        /// <returns>Returns matched results.</returns>
        public IEnumerable<Resource> SearchResources(string searchCriteria, int currentCursor,
            int pageSize, SortProperty sortProperty, out int totalRecords, string userPermission, AuthenticatedToken token, bool isSecurityAwareControl)
        {
            IEnumerable<Resource> resources = SearchResources(searchCriteria, currentCursor,
                    pageSize, sortProperty, out totalRecords, token, isSecurityAwareControl);

            if (resources != null && resources.Count() > 0)
            {
                foreach (Resource res in resources)
                {
                    res.Files.Load();
                    ScholarlyWork scholWork = res as ScholarlyWork;
                    if (scholWork != null)
                    {
                        scholWork.Authors.Load();
                    }
                }
            }

            return GetAuthorizedResources<Resource>(token, userPermission, resources);
        }

        /// <summary>
        /// Retunrs search result depends on search crieteria.
        /// </summary>
        /// <param name="searchCriteria">Search criteria.</param>
        /// <param name="currentCursor">Current page no.</param>
        /// <param name="pageSize">No. of result required.</param>
        /// <param name="totalRecords">No. of total records.</param>
        /// <param name="sortDirection">Sort direction.</param>
        /// <param name="sortExpression">Sort expression.</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="isSecurityAwareControl">Check is security aware.</param>
        /// <returns>Returns matched results.</returns>
        public IEnumerable<Resource> SearchResources(string searchCriteria, int currentCursor,
            int pageSize, out int totalRecords, SortDirection sortDirection, string sortExpression, AuthenticatedToken token, bool isSecurityAwareControl)
        {
            return SearchResources(searchCriteria, currentCursor, pageSize, new SortProperty(sortExpression, sortDirection), out totalRecords, token, isSecurityAwareControl);
        }

        /// <summary>
        /// Retunrs search result depends on search crieteria.
        /// </summary>
        /// <param name="searchCriteria">Search criteria.</param>
        /// <param name="currentCursor">Current page no.</param>
        /// <param name="pageSize">No. of result required.</param>
        /// <param name="sortProperty"></param>
        /// <param name="totalRecords">No. of total records.</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="isSecurityAwareControl">Check is security aware.</param>
        /// <returns>Returns matched results.</returns>
        public IEnumerable<Resource> SearchResources(string searchCriteria, int currentCursor,
            int pageSize, SortProperty sortProperty, out int totalRecords, AuthenticatedToken token, bool isSecurityAwareControl)
        {
            SearchEngine search = null;
            if (token == null)
                search = new SearchEngine(pageSize);
            else
                search = new SearchEngine(pageSize, isSecurityAwareControl, token);

            //Fetch and return Resources
            IEnumerable<Resource> resources = search.SearchResources(
                searchCriteria, _context, sortProperty, currentCursor, out totalRecords);
            if (resources != null && resources.Count() > 0)
            {
                foreach (Resource res in resources)
                {
                    res.RelationshipsAsSubject.Load();
                    res.RelationshipsAsObject.Load();

                    ScholarlyWork schol = res as ScholarlyWork;
                    res.Files.Load();
                    if (schol != null)
                    {
                        schol.Authors.Load();
                    }
                }

                CoreHelper.UpdateResourcesEmptyTitle(resources);
            }

            return resources;
        }

        /// <summary>
        /// Add/Update the resource into DB
        /// </summary>
        /// <param name="isEditMode">True if Edit mode</param>
        /// <param name="resourceObj">Resource object to be saved</param>
        /// <returns>True if operation is successful</returns>
        /// <exception cref="EntityNotFoundException">Thrown when a resource with the specified 
        /// id is not found.</exception>
        /// <exception cref="ConcurrentAccessException">Thrown when an exception is raised due to concurrency
        /// issues.</exception>
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
                throw new ConcurrentAccessException(GlobalResource.ConcurrentAccessExceptionMessage, exception);
            }
            return true;
        }

        /// <summary>
        /// Validates Resource to Resource association.
        /// </summary>
        /// <typeparam name="T">Type derived from Resource class.</typeparam>
        /// <param name="resourceId">Subject Resourc Id.</param>
        /// <param name="property">Subject NavigationProperty.</param>
        /// <param name="destinationList">Object Resource List.</param>
        /// <returns>true, if association is valid.</returns>
        public bool ValidateAssociation<T>(Guid resourceId, NavigationProperty property,
            IList<T> destinationList) where T : Resource
        {
            Resource subjectResource = GetResource(resourceId);

            //If there is many-to-ZeroOrOne/ZeroOrOne-to-ZeroOrOne association.
            if (property.Association.ObjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne &&
                destinationList.Count > 1)
            {
                throw new AssociationException(string.Format(CultureInfo.CurrentCulture,
                    GlobalResource.MsgSubCannotAssociateWithMoreThanOneObj,
                    subjectResource.Title));
            }
            //If there is ZeroOrOne-to-many/ZeroOrOne-to-ZeroOrOne association.
            else if (property.Association.SubjectMultiplicity == AssociationEndMultiplicity.ZeroOrOne)
            {
                subjectResource.RelationshipsAsSubject.Load();
                foreach (Relationship rel in subjectResource.RelationshipsAsSubject)
                    rel.ObjectReference.Load();

                List<Guid> associatedObjectIds = subjectResource.RelationshipsAsSubject.Select(tuple => tuple.Object.Id).ToList();

                foreach (T objResource in destinationList.Where(tuple => !associatedObjectIds.Contains(tuple.Id)))
                {
                    IQueryable<Relationship> relQuery = _context.Relationships
                        .Where(
                            tuple => tuple.Object.Id == objResource.Id &&
                            tuple.Subject.Id != subjectResource.Id &&
                            tuple.Predicate.Id == property.Association.PredicateId);

                    //Chech whether object resource is already association with another resource.
                    if (relQuery.Count() > 0)
                    {
                        throw new AssociationException(string.Format(
                            CultureInfo.CurrentCulture, GlobalResource.MsgObjectAlreadyAssociated,
                            subjectResource.Title, objResource.Title));
                    }
                    //Check for circular association in both direction.
                    else if (CheckForCircularAssociation<T>(subjectResource as T, objResource, property.Association.PredicateId) ||
                        CheckForCircularAssociation<T>(objResource, subjectResource as T, property.Association.PredicateId))
                    {
                        throw new AssociationException(string.Format(
                            CultureInfo.CurrentCulture, GlobalResource.MsgCircularAssociationDetected,
                            subjectResource.Title, objResource.Title));
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks recursively for circular relationship.
        /// </summary>
        /// <typeparam name="T">Type of Resource.</typeparam>
        /// <param name="subjectResource">Subject Resource.</param>
        /// <param name="objectResource">Object Resource.</param>
        /// <param name="predicateID">Predicate Id.</param>
        /// <returns>true if there is circular relationship</returns>
        private bool CheckForCircularAssociation<T>(T subjectResource,
            T objectResource, Guid predicateID) where T : Resource
        {
            bool isCircularAssociation = false;
            if (subjectResource != null && objectResource != null)
            {
                foreach (Relationship rel in _context.Relationships
                    .Include("Subject").Include("Predicate").Include("Object")
                    .Where(tuple => tuple.Predicate.Id == predicateID
                        && tuple.Object.Id == subjectResource.Id))
                {
                    if (rel.Subject.Id == objectResource.Id)
                        isCircularAssociation = true;
                    else
                        isCircularAssociation = CheckForCircularAssociation<T>(rel.Subject as T, objectResource, predicateID);

                    if (isCircularAssociation)
                        break;
                }
            }

            return isCircularAssociation;
        }

        /// <summary>
        /// Saves Resource to Resource Association
        /// </summary>
        /// <typeparam name="T">Resource Type</typeparam>
        /// <param name="resourceId">Resource Id</param>
        /// <param name="property">navigational property</param>
        /// <param name="destinationList">List of destination resources</param>
        /// <param name="token">Authenticated token.</param>
        /// <param name="userPermission">User permission.</param>
        /// <returns>True if operation successfully completed.</returns>
        /// <exception cref="ConcurrentAccessException">Thrown when an exception is raised due to concurrency
        /// issues.</exception>
        public bool SaveResourceToResourceAssociation<T>(
            Guid resourceId, NavigationProperty property, IList<T> destinationList,
            AuthenticatedToken token, string userPermission) where T : Resource
        {
            bool success = false;

            Predicate predicate = PredicateDataAccess.GetPredicate(property.Association.PredicateId, _context);

            if (predicate != null && destinationList != null)
            {
                Resource resource = GetResource(resourceId);
                if (property.Direction == AssociationEndType.Subject)
                {
                    SaveRelationshipsAsSubject<T>(destinationList, predicate, resource, token, userPermission);
                }
                else if (property.Direction == AssociationEndType.Object)
                {
                    SaveRelationshipsAsObject<T>(destinationList, predicate, resource, token, userPermission);
                }

                try
                {
                    resource.DateModified = DateTime.Now;
                    _context.SaveChanges();
                }
                catch (OptimisticConcurrencyException exception)
                {
                    throw new ConcurrentAccessException(GlobalResource.ConcurrentAccessExceptionMessage, exception);
                }
                success = true;
            }

            return success;
        }

        private Guid SaveRelationshipsAsSubject<T>(IList<T> destinationList, Predicate predicate, Resource resource,
            AuthenticatedToken token, string userPermission) where T : Resource
        {
            Guid resourceId = resource.Id;
            List<Relationship> existingRelationships = _getRelationshipsAsSubject(_context, predicate.Id, resourceId).ToList();

            int existingRelationshipsCount = existingRelationships.Count;
            for (int index = existingRelationshipsCount - 1; index >= 0; index--)
            {
                Guid objectResourceId = new Guid(existingRelationships[index].ObjectReference.EntityKey.EntityKeyValues[0].Value.ToString());
                Resource removedResource = destinationList.Where(res => res.Id == objectResourceId).FirstOrDefault();
                if (removedResource == null &&
                      AuthorizeResource<Resource>(token, userPermission, existingRelationships[index].Object, false))
                {
                    // Relationship has been deleted.
                    _context.DeleteObject(existingRelationships[index]);
                    existingRelationships.RemoveAt(index);
                }
            }

            int ordinalPosition = 1;
            foreach (Resource resourceForRelationship in destinationList)
            {
                Relationship existingRelationship = existingRelationships.Where(rel =>
                    (Guid)rel.ObjectReference.EntityKey.EntityKeyValues[0].Value == resourceForRelationship.Id)
                    .FirstOrDefault();

                if (existingRelationship == null)
                {
                    Relationship newRelationship = new Relationship();
                    Resource objectResource;
                    try
                    {
                        objectResource = GetResource(resourceForRelationship.Id);
                    }
                    catch (EntityNotFoundException)
                    {
                        // Someone else might have deleted the resource.
                        throw new ConcurrentAccessException();
                    }

                    newRelationship.Subject = resource;
                    newRelationship.Object = objectResource;
                    newRelationship.Predicate = predicate;
                    newRelationship.OrdinalPosition = ordinalPosition;
                    existingRelationships.Add(newRelationship);
                }
                else
                {
                    existingRelationship.OrdinalPosition = ordinalPosition;
                }
                ordinalPosition++;
            }
            return resourceId;
        }

        private Guid SaveRelationshipsAsObject<T>(IList<T> destinationList, Predicate predicate, Resource resource,
            AuthenticatedToken token, string userPermission) where T : Resource
        {
            Guid resourceId = resource.Id;
            List<Relationship> existingRelationships = _getRelationshipsAsObject(_context, predicate.Id, resourceId).ToList();

            int existingRelationshipsCount = existingRelationships.Count;
            for (int index = existingRelationshipsCount - 1; index >= 0; index--)
            {
                Guid subjectResourceId = new Guid(existingRelationships[index].SubjectReference.EntityKey.EntityKeyValues[0].Value.ToString());
                Resource removedResource = destinationList.Where(res => res.Id == subjectResourceId).FirstOrDefault();
                if (removedResource == null &&
                      AuthorizeResource<Resource>(token, userPermission, existingRelationships[index].Subject, false))
                {
                    // Relationship has been deleted.
                    _context.DeleteObject(existingRelationships[index]);
                    existingRelationships.RemoveAt(index);
                }
            }

            int ordinalPosition = 1;
            foreach (Resource resourceForRelationship in destinationList)
            {
                Relationship existingRelationship = existingRelationships
                    .Where(rel => (Guid)rel.SubjectReference.EntityKey.EntityKeyValues[0].Value == resourceForRelationship.Id)
                    .FirstOrDefault();

                if (existingRelationship == null)
                {
                    Relationship newRelationship = new Relationship();
                    Resource subjectResource;
                    try
                    {
                        subjectResource = GetResource(resourceForRelationship.Id);
                    }
                    catch (EntityNotFoundException)
                    {
                        // Someone else might have deleted the resource.
                        throw new ConcurrentAccessException();
                    }

                    newRelationship.Subject = subjectResource;
                    newRelationship.Object = resource;
                    newRelationship.Predicate = predicate;
                    newRelationship.OrdinalPosition = ordinalPosition;
                    existingRelationships.Add(newRelationship);
                }
                else
                {
                    existingRelationship.OrdinalPosition = ordinalPosition;
                }
                ordinalPosition++;
            }
            return resourceId;
        }

        /// <summary>
        /// Upload File content into DB.
        /// </summary>
        /// <param name="fileObj">File object</param>
        /// <param name="fileContent">File stream </param>
        public void UploadFileContent(Zentity.Core.File fileObj, Stream fileContent)
        {
            _context.UploadFileContent(fileObj, fileContent);
        }

        /// <summary>
        /// Deletes input Resources from Database
        /// </summary>
        /// <param name="resourceIds">List of resource Guids to be deleted</param>
        /// <exception cref="ConcurrentAccessException">Thrown when an exception is raised due to concurrency
        /// issues.</exception>
        public void DeleteResources(Collection<Guid> resourceIds)
        {
            foreach (Guid resourceId in resourceIds)
            {
                DeleteResource(resourceId, _context);
            }

            try
            {
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(GlobalResource.ConcurrentAccessExceptionMessage, exception);
            }
        }

        /// <summary>
        /// Deletes Resource from Database
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <param name="context">ZentityConttext object</param>
        /// <returns>True if operation completed successfully</returns>
        private static bool DeleteResource(Guid resourceId, ZentityContext context)
        {
            bool success = false;

            IQueryable<Resource> resourceObject = _getResourceById.Invoke(context, resourceId);

            if (resourceObject != null)
            {
                Resource resource = resourceObject.FirstOrDefault();

                if (resource != null)
                {
                    //Delete relationships as a subject 
                    resource.RelationshipsAsSubject.Load();
                    List<Relationship> relationships = resource.RelationshipsAsSubject.ToList();

                    foreach (Relationship relation in relationships)
                    {
                        context.DeleteObject(relation);
                    }

                    // Delete associated Resource propertes
                    resource.ResourceProperties.Load();
                    List<ResourceProperty> resProperties = resource.ResourceProperties.ToList();
                    foreach (ResourceProperty property in resProperties)
                    {
                        resource.ResourceProperties.Remove(property);
                        context.DeleteObject(property);
                    }

                    //Delete relationships as a object
                    resource.RelationshipsAsObject.Load();
                    relationships = resource.RelationshipsAsObject.ToList();

                    foreach (Relationship relation in relationships)
                    {
                        context.DeleteObject(relation);
                    }

                    //Delete resource object
                    context.DeleteObject(resource);

                    success = true;
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
        public void DeleteResourcesCategory(Collection<Guid> resourceIds)
        {
            foreach (Guid resourceId in resourceIds)
            {
                DeleteResourceCategory(resourceId, _context);
            }

            try
            {
                _context.SaveChanges();
            }
            catch (OptimisticConcurrencyException exception)
            {
                throw new ConcurrentAccessException(GlobalResource.ConcurrentAccessExceptionMessage, exception);
            }
        }

        /// <summary>
        /// Deletes Resource from Database
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <param name="context">ZentityConttext object</param>
        /// <returns>True if operation completed successfully</returns>
        private static bool DeleteResourceCategory(Guid resourceId, ZentityContext context)
        {
            bool success = false;

            IQueryable<Resource> resourceObject = _getResourceById.Invoke(context, resourceId);
            CategoryNode resource = (CategoryNode)resourceObject.FirstOrDefault();

            if (resource != null)
            {
                context.DeleteObject(resource);

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Returns CategoryNode details.
        /// </summary>
        /// <param name="resourceId">Resource Id</param>
        /// <returns>Returns CategoryNode</returns>
        public CategoryNode GetCategoryNode(Guid resourceId)
        {
            IQueryable<CategoryNode> myResource = _getCategoryNodeById(_context, resourceId);
            CategoryNode resource = myResource.FirstOrDefault();
            if (resource == null)
            {
                throw new EntityNotFoundException(EntityType.Resource, resourceId);
            }
            resource.Children.Load();
            return resource;
        }

        /// <summary>
        /// Fetches CategoryNode with its hierarchy loaded.
        /// </summary>
        /// <param name="categoryNodeId">Root CategoryNode.</param>
        /// <returns>CategoryNode with its hierarchy loaded.</returns>
        public CategoryNode GetCategoryNodeWithHierarchy(Guid categoryNodeId)
        {
            CategoryNode categoryNode = _getCategoryNodeById(_context, categoryNodeId).FirstOrDefault(); ;

            categoryNode.Children.Load();
            return LoadChildrenNodes(categoryNode);
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

        /// <summary>
        /// Get root node of CategoryNodes.   
        /// </summary>
        /// <returns>Returns CategoryNode Root</returns>
        public IList<CategoryNode> GetRootCategoryNodes()
        {
            ObjectQuery<CategoryNode> myCategoryNode = _getCategoryRootNode(_context);
            return myCategoryNode.Where(tuple => tuple.Parent == null).
                OrderBy(tuple => tuple.Title).ToList();
        }

        /// <summary>
        /// Get root node of CategoryNodes.   
        /// </summary>
        /// <returns>Returns CategoryNode root nodes.</returns>
        public IEnumerable<CategoryNode> GetRootCategoryNodesWithHierarchy()
        {
            ObjectQuery<CategoryNode> myCategoryNode = _getCategoryRootNode(_context);
            IList<CategoryNode> rootCatNodes = myCategoryNode.Where(tuple => tuple.Parent == null).
                OrderBy(tuple => tuple.Title).ToList();

            foreach (CategoryNode categoryNode in rootCatNodes)
            {
                LoadChildrenNodes(categoryNode);
            }
            return rootCatNodes;
        }

        /// <summary>
        /// Get root node of CategoryNodes.   
        /// </summary>
        /// <param name="token">Authenticated token.</param>
        /// <returns>Returns CategoryNode root nodes.</returns>
        public IEnumerable<CategoryNode> GetRootCategoryNodesWithHierarchy(AuthenticatedToken token)
        {
            IEnumerable<CategoryNode> categories = GetRootCategoryNodesWithHierarchy();
            return GetAuthorizedResources<CategoryNode>(token, UserResourcePermissions.Update, categories);
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
        /// Get list of categorynode.
        /// </summary>
        /// <param name="list">List of categorynode ids</param>
        /// <returns>List of categorynode objects</returns>
        public List<CategoryNode> GetCategoryNodeList(List<Guid> list)
        {
            List<CategoryNode> resourceList = new List<CategoryNode>();
            for (int Index = 0; Index < list.Count; Index++)
            {
                resourceList.Add(GetCategoryNode(list[Index]));
            }
            return resourceList;
        }

        /// <summary>
        /// Fetches Collection of Resource types
        /// </summary>
        /// <param name="entityTypeFullName">Entity Type</param>
        /// <returns>Collection of ResourceTypeInfo</returns>
        public Collection<ResourceType> GetResourceTypeList(string entityTypeFullName)
        {
            Collection<ResourceType> resTypes = new Collection<ResourceType>();

            ResourceType parentType = GetResourceType(entityTypeFullName);

            if (parentType != null)
            {
                resTypes.Add(parentType);
                AddChildResourceTypes(resTypes, parentType, _context);
            }

            return resTypes;
        }

        /// <summary>
        /// This function recursively find the child resource types and add to resTypes list
        /// </summary>
        /// <param name="resTypes">List of ResourceTypeInfo</param>
        /// <param name="parentType">Parent ResourceTypeInfo</param>
        /// <param name="context"> instance of ZentityContext object</param>
        private void AddChildResourceTypes(Collection<ResourceType> resTypes,
            ResourceType parentType, ZentityContext context)
        {
            List<ResourceType> resTypesList = context.DataModel.Modules.
                    SelectMany(tuple => tuple.ResourceTypes).ToList();

            foreach (ResourceType resType in resTypesList)
            {
                if (resType.BaseType != null &&
                    resType.BaseType.FullName.Equals(parentType.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    resTypes.Add(resType);
                    AddChildResourceTypes(resTypes, resType, context);
                }
            }
        }

        /// <summary>
        /// Returns a content stream of the file.
        /// </summary>
        /// <param name="fileId">File id.</param>
        /// <returns>Content stream.</returns>
        public Stream GetContentStream(Guid fileId)
        {
            Zentity.Core.File fileObj = GetFile(fileId);
            return _context.GetContentStream(fileObj);
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
        /// Returns size of the specified file in KB.
        /// </summary>
        /// <param name="fileObj">File object</param>
        /// <returns>File Size in KB</returns>
        public string GetUploadedFileSize(Zentity.Core.File fileObj)
        {
            string fileSize = string.Empty;
            if (fileObj != null)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    _context.DownloadFileContent(fileObj, stream);
                    if (stream.Length > 0)
                    {
                        if (stream.Length <= 1024)
                        {
                            fileSize = stream.Length.ToString(CultureInfo.CurrentCulture) + " Bytes";
                        }
                        else if (stream.Length > 1024 && stream.Length <= (1024 * 1024))
                        {
                            fileSize = Math.Round(((double)stream.Length / (double)1024), 2).ToString(CultureInfo.CurrentCulture) + " KB";
                        }
                        else
                        {
                            fileSize = Math.Round(((double)stream.Length / (double)(1024 * 1024)), 2).ToString(CultureInfo.CurrentCulture) + " MB";
                        }
                    }
                }
            }

            return fileSize;
        }

        /// <summary>
        /// Download the file.
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <param name="response">Page response</param>
        public void DownloadFile(Guid fileId, HttpResponse response)
        {
            Zentity.Core.File fileObj = null;
            try
            {
                fileObj = this.GetFile(fileId);

                if (fileObj == null)
                {
                    throw new System.IO.FileNotFoundException(GlobalResource.NoFileDownload);
                }

                string fileName = string.Empty;
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    this.DownloadFileContent(fileId, stream);

                    if (stream.Length == 0)
                    {
                        throw new System.IO.FileNotFoundException(GlobalResource.NoFileDownload);
                    }

                    fileName = CoreHelper.GetFileName(fileObj.Title, fileObj.FileExtension);

                    response.AddHeader(Constants.ResponseHeaderContentDispositionName,
                       Constants.ResponseHeaderContentDispositionValue + fileName);
                    response.AddHeader(Constants.ResponseHeaderContentLengthName,
                        stream.Length.ToString(CultureInfo.InvariantCulture));
                    response.ContentType = Constants.ResponseContentType;
                    response.BinaryWrite(stream.GetBuffer());
                    response.Flush();
                    response.End();
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(GlobalResource.UnableDownload);
            }
        }

        /// <summary>
        /// Fetches File object for given file Id.
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <returns>File object</returns>
        public Zentity.Core.File GetFile(Guid fileId)
        {
            IQueryable<Zentity.Core.File> myFile = _getFileById(_context, fileId);

            Zentity.Core.File foundFile = myFile.FirstOrDefault();
            if (foundFile == null)
            {
                throw new EntityNotFoundException(EntityType.Resource, fileId);
            }

            return foundFile;
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
        /// Returns list of top tags based on count of resource on which the tag is applied. Note that the
        /// resources are not loaded.
        /// </summary>
        /// <param name="pageSize">Maximum number of records to be fetched.</param>
        /// <returns>Tags.</returns>
        public List<Tag> GetTopTags(int pageSize)
        {
            return _getTopTagsCompiledQuery.Invoke(_context, pageSize).ToList();
        }

        /// <summary>
        ///  Returns the resource type.   
        /// </summary>
        /// <param name="resTypeId">Resource TypeId</param>
        /// <returns>Resource Type.</returns>
        public string GetResourceType(Guid resTypeId)
        {
            return _getTypeNameCompiledQuery.Invoke(_context, resTypeId).FirstOrDefault();
        }

        /// <summary>
        /// Fetches scalar property based on type and property name. IF object is present in cache 
        /// then it is fetched from cache else from Database.
        /// </summary>
        /// <param name="cache">Cache object</param>
        ///<param name="typeName"> type name </param>
        /// <param name="propertyName">property name</param>
        /// <returns>ScalarProperty object is successful else null value</returns>
        public ScalarProperty GetScalarProperty(Cache cache, string typeName, string propertyName)
        {
            ScalarProperty property = null;

            //GetAll ScalarProperties
            Collection<ScalarProperty> scalarPropertyCollection = GetScalarProperties(cache, typeName);

            if (scalarPropertyCollection != null)
            {
                //Get scalar property
                property = scalarPropertyCollection.Where(prop => prop.Name == propertyName).FirstOrDefault();
            }

            return property;
        }

        /// <summary>
        /// Returns colletion of the scalar properties.
        /// </summary>
        /// <param name="pageCache">Cache.</param>
        /// <param name="typeName">Type name for with scalar proeprty returns.</param>
        /// <returns>Colletion of the scalar properties.</returns>
        public Collection<ScalarProperty> GetScalarProperties(Cache pageCache, string typeName)
        {
            ResourceType type = GetResourceType(typeName);

            Collection<ScalarProperty> scalarProperties = null;
            if (type != null)
            {
                scalarProperties = CoreHelper.GetScalarProperties(pageCache, type);
            }

            return scalarProperties;
        }

        /// <summary>
        /// Returns the navigation properties for specific type name.
        /// </summary>
        /// <param name="pageCache">Page Cache object.</param>
        /// <param name="typeName">Type name for with scalar proeprty returns.</param>
        /// <returns>Colletion of the scalar properties.</returns>
        public Collection<NavigationProperty> GetNavigationalProperties(Cache pageCache, string typeName)
        {
            ResourceType type = GetResourceType(typeName);

            Collection<NavigationProperty> navigationalProperties = null;
            if (type != null)
            {
                navigationalProperties = CoreHelper.GetNavigationalProperties(pageCache, type);
            }

            return navigationalProperties;
        }

        /// <summary>
        /// Fetches scalar property based on type and property name. IF object is present in cache 
        /// then it is fetched from cache else from Database.
        /// </summary>
        /// <param name="cache">Cache object</param>
        ///<param name="typeName"> type name </param>
        /// <param name="propertyName">property name</param>
        /// <returns>ScalarProperty object is successful else null value</returns>
        public NavigationProperty GetNavigationProperty(Cache cache, string typeName, string propertyName)
        {
            NavigationProperty property = null;

            //GetAll ScalarProperties
            Collection<NavigationProperty> navigationalPropertyCollection = GetNavigationalProperties(cache, typeName);

            if (navigationalPropertyCollection != null)
            {
                //Get scalar property
                property = navigationalPropertyCollection.Where(prop => prop.Name == propertyName).FirstOrDefault();
            }

            return property;
        }

        #region Browsable Methods

        #region Methods to populate  4 Browsable View

        /// <summary>
        /// Returns list of authors base on count of Author works submitted
        /// </summary>
        /// <param name="token">Authenticated token.</param>
        /// <returns>Authors.</returns>
        public IEnumerable<Contact> GetAuthors(AuthenticatedToken token)
        {
            return GetAuthorizedResources<Contact>(token, UserResourcePermissions.Read, _getContactsQuery(_context));
        }

        /// <summary>
        /// Returns list of authors base on count of Author works submitted
        /// </summary>
        /// <param name="token">AuthenticatedToken</param>
        /// <param name="range">Range to search.</param>
        /// <param name="subString">Sub string</param>
        /// <param name="pagesize">Page Size</param>
        /// <param name="fetchedRecordCount">Total Fetched Record Count</param>
        /// <param name="totalRecords">Total Records</param>
        /// <returns>List of Contact object</returns>
        public IEnumerable<Contact> GetAuthors(AuthenticatedToken token, string range, string subString,
            int pagesize, int fetchedRecordCount, out int totalRecords)
        {
            totalRecords = 0;
            IEnumerable<Contact> contacts = null;
            IQueryable<Contact> query = null;

            if (!string.IsNullOrEmpty(range) && range.Equals(Constants.OtherAuthors) && !string.IsNullOrEmpty(subString))
            {
                query = _context.CreateQuery<Contact>(string.Format(CultureInfo.InvariantCulture,
                        _otherAuthorsWithSubStringQuery, subString));
            }
            else if (!string.IsNullOrEmpty(range) && range.Equals(Constants.OtherAuthors))
            {
                query = _context.CreateQuery<Contact>(_otherAuthorsQuery);
            }
            else if (!string.IsNullOrEmpty(range) && !string.IsNullOrEmpty(subString))
            {
                query = _context.CreateQuery<Contact>(string.Format(CultureInfo.InvariantCulture,
                        _authorsRangeAndSubstringQuery, range, subString));

            }
            else if (!string.IsNullOrEmpty(range) && string.IsNullOrEmpty(subString))
            {
                query = _context.CreateQuery<Contact>(string.Format(CultureInfo.InvariantCulture,
                        _authorsRangeQuery, range));
            }
            else if (string.IsNullOrEmpty(range) && !string.IsNullOrEmpty(subString))
            {
                query = _context.CreateQuery<Contact>(string.Format(CultureInfo.InvariantCulture,
                        _authorsSubStringQuery, subString));
            }

            if (query != null)
            {
                query = query.Where(tuple => tuple.AuthoredWorks.Count() > 0);

                if (token != null)
                {
                    query = query.Authorize<Contact>(UserResourcePermissions.Read, _context, token);
                }

                totalRecords = query.Count();

                contacts = query
                    .OrderBy(tupl => tupl.Title)
                    .Skip(fetchedRecordCount)
                    .Take(pagesize)
                    .ToList();
            }

            return contacts;
        }


        /// <summary>
        /// Get the year range for populating years in the tree view
        /// </summary>
        /// <param name="token">Authenticated token.</param>
        /// <returns>Dates.</returns>
        public List<DateTime?> GetYear(AuthenticatedToken token)
        {
            var allResources = _allResources(_context);
            var resources = GetAuthorizedResources<Resource>(token, UserResourcePermissions.Read, allResources);
            IEnumerable<DateTime?> dates = resources.Select<Resource, DateTime?>(resource => resource.DateAdded);
            return new List<DateTime?>() { dates.Min(), dates.Max() };
        }

        /// <summary>
        /// Get subject area(categories) to populate in the tree view
        /// </summary>
        /// <param name="token">Authenticated token.</param>
        /// <returns>Subject areas.</returns>
        public IEnumerable<CategoryNode> GetCategoryHierarchy(AuthenticatedToken token)
        {
            return GetAuthorizedResources<CategoryNode>(token, UserResourcePermissions.Read, _getCategoryNodesQuery(_context));
        }

        /// <summary>
        /// Get all resource types to populate in the tree view
        /// </summary>
        /// <returns>Resource types.</returns>
        public IEnumerable<ResourceType> GetResourceTypes()
        {
            return _context.DataModel.Modules.SelectMany(tuple => tuple.ResourceTypes);
        }

        #endregion

        #region  Get search results
        /// <summary>
        /// Get resources by the selected author.
        /// </summary>
        /// <param name="selectedNodeId">Id of tthe selected author</param>
        /// <param name="fetchedRecords">Number of fetched records.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total matching records.</param>
        /// <returns></returns>
        public List<Resource> GetScholarlyWorksForAuthor(Guid selectedNodeId, int fetchedRecords,
            int pageSize, out int totalRecords)
        {
            List<Resource> scholarlyWorkList = null;
            Contact author = _getContactById.Invoke(_context, selectedNodeId).FirstOrDefault();
            totalRecords = 0;
            if (author != null)
            {
                author.AuthoredWorks.Load();
                totalRecords = author.AuthoredWorks.Count();
                if (totalRecords > 0)
                {
                    if (totalRecords < fetchedRecords)
                    {
                        scholarlyWorkList = null;
                    }
                    else
                    {
                        scholarlyWorkList = author.AuthoredWorks.Skip(fetchedRecords).Take(pageSize).Select(tuple => tuple as Resource).ToList();

                        //Load related files and authors.
                        foreach (Resource resource in scholarlyWorkList)
                        {
                            resource.Files.Load();

                            ScholarlyWork scholWork = resource as ScholarlyWork;
                            if (scholWork != null)
                                scholWork.Authors.Load();
                        }
                    }
                }
            }
            return scholarlyWorkList;
        }

        /// <summary>
        /// Get Scholarlywork item by selected category id.
        /// </summary>
        /// <param name="selectedNodeId">Selected category id.</param>
        /// <param name="fetchedRecords">Number of fetched records.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total matching records.</param>
        /// <returns>Resources.</returns>
        public List<Resource> GetScholarlyWorkItemForCategory(Guid selectedNodeId,
            int fetchedRecords, int pageSize, out int totalRecords)
        {
            CategoryNode categoryNode = _getCategoryNodeById.Invoke(_context, selectedNodeId).FirstOrDefault();
            List<Resource> scholarlyWorkItemList = null;
            totalRecords = 0;
            if (categoryNode != null)
            {
                categoryNode.ScholarlyWorkItems.Load();

                totalRecords = categoryNode.ScholarlyWorkItems.Count();

                if (totalRecords > 0)
                {
                    if (totalRecords < fetchedRecords)
                    {
                        scholarlyWorkItemList = null;
                    }
                    else
                    {
                        scholarlyWorkItemList = categoryNode.ScholarlyWorkItems.Skip(fetchedRecords).Take(pageSize).Select(tuple => tuple as Resource).ToList();

                        //Load related files and authors.
                        foreach (Resource resource in scholarlyWorkItemList)
                        {
                            resource.Files.Load();

                            ScholarlyWork scholWork = resource as ScholarlyWork;
                            if (scholWork != null)
                                scholWork.Authors.Load();
                        }
                    }
                }
            }
            return scholarlyWorkItemList;
        }

        /// <summary>
        /// Get scholarly works by selected year
        /// </summary>
        /// <param name="selectedMonth">Selected month.</param>
        /// <param name="selectedYear">Selected year.</param>
        /// <param name="fetchedRecords">Number of fetched records.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total matching records.</param>
        /// <returns></returns>
        public List<Resource> GetResourcesByMonthAndYear(int selectedMonth, int selectedYear,
            int fetchedRecords, int pageSize, out int totalRecords)
        {
            List<Resource> resourceList = null;

            if (selectedMonth != 0)
            {
                resourceList = _getResourcesByMonthAndYear(_context, selectedMonth, selectedYear).ToList();
            }
            else
            {
                resourceList = _getResourcesByYear.Invoke(_context, selectedYear).ToList();
            }


            totalRecords = resourceList != null ? resourceList.Count : 0;

            if (totalRecords > 0)
            {
                if (totalRecords < fetchedRecords)
                {
                    resourceList = null;
                }
                else
                {
                    resourceList = resourceList.Skip(fetchedRecords).Take(pageSize).ToList();

                    //Load related files and authors.
                    foreach (Resource resource in resourceList)
                    {
                        resource.Files.Load();

                        ScholarlyWork scholWork = resource as ScholarlyWork;
                        if (scholWork != null)
                            scholWork.Authors.Load();
                    }
                }
            }
            return resourceList;
        }

        #region commented code


        #endregion

        ///  <summary>
        /// Get resources by the selected type.
        /// </summary>
        /// <param name="typeName">The name of the resource type selected.</param>
        /// <param name="fetchedRecords">Number of fetched records.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total matching records.</param>
        /// <returns>Resources.</returns>
        public List<Resource> GetResourceByType(string typeName, int fetchedRecords,
            int pageSize, out int totalRecords)
        {
            List<Resource> resourceList = null;

            Type classType = GetTypeOfResource(CoreHelper.ExtractNamespace(typeName), typeName);
            MethodInfo method = this.GetType().GetMethod("GetCount",
                       BindingFlags.NonPublic | BindingFlags.Instance);
            Resource classObj = Activator.CreateInstance(classType, null) as Resource;

            totalRecords = (int)method.MakeGenericMethod(classObj.GetType()).
                            Invoke(this, null);

            if (totalRecords > 0)
            {
                if (totalRecords < fetchedRecords)
                {
                    resourceList = null;
                }
                else
                {
                    string actualPageSize = pageSize.ToString(CultureInfo.InvariantCulture);
                    string actualFetchRecord = fetchedRecords.ToString(CultureInfo.InvariantCulture);
                    ObjectQuery<Resource> resourcesByTypeQuery = new ObjectQuery<Resource>(
                            string.Format(CultureInfo.InvariantCulture, _getResourcesByResourceTypeQuery,
                            typeName, actualFetchRecord, actualPageSize), _context);

                    resourceList = resourcesByTypeQuery.ToList();

                    //Load related files and authors.
                    foreach (Resource resource in resourceList)
                    {
                        resource.Files.Load();

                        ScholarlyWork scholWork = resource as ScholarlyWork;
                        if (scholWork != null)
                            scholWork.Authors.Load();
                    }
                }
            }

            return resourceList;
        }

        ///// <summary>
        ///// Gets the count of the specified type of resource.
        ///// </summary>
        ///// <typeparam name="T">Resource type.</typeparam>
        ///// <returns>Count.</returns>
        ///// <remarks>Used using reflection in method GetResourceByType.</remarks>
        //private int GetCount<T>() where T : Resource
        //{
        //    return _context.Resources.OfType<T>().Count();
        //}

        #endregion

        #endregion

        #region Security Aware Methods

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
        /// Fetches resource object and permissions based on resource Id.
        /// </summary>
        /// <param name="token">Authenticated Token.</param>
        /// <param name="resourceId">Resource Id.</param>
        /// <returns>Resource object and permissions.</returns>
        public ResourcePermissions<T> GetResourcePermissions<T>(AuthenticatedToken token, Guid resourceId) where T : Resource
        {
            ResourcePermissions<T> resPermissons = null;

            if (resourceId != Guid.Empty && token != null)
            {
                //Load metadata
                _context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly);

                T resource = this.GetResource(resourceId) as T;
                if (resource != null)
                {
                    resPermissons = resource.GetPermissions<T>(_context, token);
                }
            }

            return resPermissons;
        }


        /// <summary>
        /// Fetches related resource object and permissions based on resource Id.
        /// </summary>
        /// <param name="token">Authenticated Token.</param>
        /// <param name="resourceId">Resource Id.</param>
        /// <param name="property">Navigation property.</param>
        /// <returns>Resource object and permissions.</returns>
        public IEnumerable<ResourcePermissions<Resource>> GetRelatedResourceWithPermissions(AuthenticatedToken token, Guid resourceId, NavigationProperty property)
        {
            IEnumerable<ResourcePermissions<Resource>> resourcePermissions = null;

            if (resourceId != Guid.Empty && token != null)
            {
                List<Resource> resources = GetRelatedResources(resourceId, property, null, UserResourcePermissions.Read);

                if (resources != null)
                {
                    resourcePermissions = resources.GetPermissions<Resource>(_context, token);
                }
            }

            return resourcePermissions;
        }

        /// <summary>
        /// Search for reaources on which user is having atleast Read permission.
        /// </summary>
        /// <param name="token">Aithentication token.</param>
        /// <param name="searchCriteria">search criteria.</param>
        /// <param name="currentCursor">Fetched record count.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total records.</param>
        /// <param name="isSecurityAwareControl">Indicates whether the control is security aware or not.</param>
        /// <returns>List of resources</returns>
        public IEnumerable<Resource> SearchForResources(AuthenticatedToken token, string searchCriteria, int currentCursor,
            int pageSize, out int totalRecords, bool isSecurityAwareControl)
        {
            SearchEngine search = null;
            if (token == null)
                search = new SearchEngine(pageSize);
            else
                search = new SearchEngine(pageSize, isSecurityAwareControl, token);
            //Fetch and return Resources
            IEnumerable<Resource> resources = search.SearchResources(
                searchCriteria, _context, currentCursor, out totalRecords);
            if (resources != null && resources.Count() > 0)
            {
                foreach (Resource res in resources)
                {
                    res.RelationshipsAsSubject.Load();
                    res.RelationshipsAsObject.Load();

                    ScholarlyWork schol = res as ScholarlyWork;
                    res.Files.Load();
                    if (schol != null)
                    {
                        schol.Authors.Load();
                    }
                }

                CoreHelper.UpdateResourcesEmptyTitle(resources);
            }

            return resources;

        }

        /// <summary>
        /// Search for reaources on which user is having atleast Read permission.
        /// </summary>
        /// <param name="token">Aithentication token.</param>
        /// <param name="searchCriteria">search criteria.</param>
        /// <param name="currentCursor">Fetched record count.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total records.</param>
        /// <param name="sortExpression">Column to sort on</param>
        /// <param name="sortDirection">Direction to sort in</param>
        /// <param name="isSecurityAwareControl">Indicates whether the control is security aware or not.</param>
        /// <returns>List of resources</returns>
        public IEnumerable<Resource> SearchForResources(AuthenticatedToken token, string searchCriteria, int currentCursor,
            int pageSize, out int totalRecords, string sortExpression, SortDirection sortDirection, bool isSecurityAwareControl)
        {
            //Load metadata
            _context.MetadataWorkspace.LoadFromAssembly(typeof(Identity).Assembly);

            SearchEngine search = null;
            if (token == null)
                search = new SearchEngine(pageSize);
            else
                search = new SearchEngine(pageSize, isSecurityAwareControl, token);

            //Fetch and return Resources           
            IEnumerable<Resource> resources = search.SearchResources(
                   searchCriteria, _context, new SortProperty(sortExpression, sortDirection), currentCursor,
                   out totalRecords);
            if (resources != null && resources.Count() > 0)
            {
                foreach (Resource res in resources)
                {
                    res.RelationshipsAsSubject.Load();
                    res.RelationshipsAsObject.Load();

                    ScholarlyWork schol = res as ScholarlyWork;
                    res.Files.Load();
                    if (schol != null)
                    {
                        schol.Authors.Load();
                    }
                }

                CoreHelper.UpdateResourcesEmptyTitle(resources);
            }

            return resources;

        }


        /// <summary>
        /// Search for reaources and user permissions on those resources.
        /// </summary>
        /// <param name="token">Aithentication token.</param>
        /// <param name="searchCriteria">search criteria.</param>
        /// <param name="currentCursor">Fetched record count.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total records.</param>
        /// <param name="isSecurityAwareControl">Indicates whether the control is security aware or not.</param>
        /// <returns>List of resources mapped with user permissions.</returns>
        public IEnumerable<ResourcePermissions<Resource>> SearchForResourcePermissons(AuthenticatedToken token, string searchCriteria,
            int currentCursor, int pageSize, out int totalRecords, bool isSecurityAwareControl)
        {
            IEnumerable<ResourcePermissions<Resource>> resPermissions = null;
            totalRecords = 0;

            if (token != null)
            {
                IList<Resource> resources = SearchForResources(token, searchCriteria, currentCursor,
                        pageSize, out totalRecords, isSecurityAwareControl).ToList();

                if (resources != null && resources.Count > 0)
                {
                    resPermissions = resources.GetPermissions<Resource>(_context, token);
                }
            }

            return resPermissions;
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
        /// Returns list of top tags based on count of resource on which the tag is applied.
        /// </summary>
        /// <param name="pageSize">Maximum number of records to be fetched.</param>
        /// <param name="token">Authenticated token.</param>
        /// <returns>Tag cloud entries.</returns>
        public List<TagCloudEntry> GetTopTagsForTagCloud(int pageSize, AuthenticatedToken token)
        {
            ObjectQuery<Tag> topTags = _context.Resources.OfType<Tag>();

            if (token != null)
            {
                topTags = topTags.Authorize<Tag>(UserResourcePermissions.Read, _context, token);
            }

            return topTags
                .Include("ScholarlyWorkItems")
                .Where(tags => tags.ScholarlyWorkItems.Count() > 0)
                .Select(tag => new TagCloudEntry { Tag = tag, ReferenceCount = tag.ScholarlyWorkItems.Count() })
                .OrderByDescending(tagCloudEntry => tagCloudEntry.ReferenceCount)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Returns a list of resources authoized with given user permission.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="token">Authentication token.</param>
        /// <param name="userPermission">User permissions</param>
        /// <param name="resources">Resource list to be authorized.</param>
        /// <returns>List of read authorized resources.</returns>
        public IEnumerable<T> GetAuthorizedResources<T>(AuthenticatedToken token, string userPermission, IEnumerable<T> resources) where T : Resource
        {
            if (token != null && resources != null && !string.IsNullOrEmpty(userPermission))
            {
                if (resources != null && resources.Count() > 0 && token != null)
                    return resources.Authorize<T>(userPermission, _context, token);
            }

            return resources;
        }

        /// <summary>
        /// Authorizes a resource for read access.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="token">Authentication token.</param>
        /// <param name="userPermission">User permissions</param>
        /// <param name="resourceId">Id of resource to be authorized.</param>
        /// <param name="throwExceptionOnUnauthorize">Indicates whether an exception should be thrown 
        /// or whether a boolean value should be returned when token is unauthorized.</param>
        /// <returns>Boolean whether the operation was successful.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when token is unauthorized and 
        /// throwExceptionOnUnauthorize is false.</exception>
        public bool AuthorizeResource<T>(AuthenticatedToken token, string userPermission, Guid resourceId, bool throwExceptionOnUnauthorize) where T : Resource
        {
            T resource = GetResource(resourceId, typeof(T).FullName) as T;
            return AuthorizeResource<T>(token, userPermission, resource, throwExceptionOnUnauthorize);
        }

        /// <summary>
        /// Authorizes a resource for read access.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="token">Authentication token.</param>
        /// <param name="userPermission">User permissions</param>
        /// <param name="resource">Resource to be authorized.</param>
        /// <param name="throwExceptionOnUnauthorize">Indicates whether an exception should be thrown 
        /// or whether a boolean value should be returned when token is unauthorized.</param>
        /// <returns>Boolean whether the operation was successful.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when token is unauthorized and 
        /// throwExceptionOnUnauthorize is false.</exception>
        public bool AuthorizeResource<T>(AuthenticatedToken token, string userPermission, T resource, bool throwExceptionOnUnauthorize) where T : Resource
        {
            if (token != null && resource != null)
            {
                if (!resource.Authorize<T>(userPermission, _context, token))
                {
                    if (throwExceptionOnUnauthorize)
                    {
                        throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture,
                            GlobalResource.UnauthorizedAccessException, userPermission));
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Check if user has create permission.
        /// </summary>
        /// <param name="token">Authentication token.</param>
        /// <returns>Returns true if have permission.</returns>
        public bool HasCreatePermission(AuthenticatedToken token)
        {
            if (token != null)
                return AuthorizationManager.HasCreatePermission(token, _context);

            return false;
        }

        /// <summary>
        /// Check if users have permission of specific resource.
        /// </summary>
        /// <param name="token">Authentication token.</param>
        /// <param name="permission">Permisson</param>
        /// <param name="resourceId">Resource Id on which to check access</param>
        /// <returns>Return true is have access</returns>
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
        /// Returns list of latest added resources
        /// </summary>
        /// <param name="pageSize">Maximum No. of records to be fetched</param>
        /// <param name="token">Authenticated token</param>
        /// <returns></returns>
        public List<Resource> GetLatestAddedResources(AuthenticatedToken token, int pageSize)
        {
            IQueryable<Resource> resources = _getLatestAddedResCompiledQuery.Invoke(_context);

            if (token != null)
            {
                resources = resources
                    .Where(resource => !(resource is Identity || resource is Zentity.Security.Authorization.Group))
                    .Authorize<Resource>(UserResourcePermissions.Read, _context, token);
            }

            resources = resources.OrderByDescending(r => r.DateAdded).Take(pageSize);

            List<Resource> resourceList = resources.ToList();
            if (resourceList.Count > 0)
            {
                foreach (Resource res in resourceList)
                {
                    res.Files.Load();
                    ScholarlyWork scholWork = res as ScholarlyWork;
                    if (scholWork != null)
                        scholWork.Authors.Load();
                }
                CoreHelper.UpdateResourcesEmptyTitle(resourceList);
            }
            return resourceList;
        }

        /// <summary>
        /// Returns list of latest modified resources
        /// </summary>
        /// <param name="pageSize">Maximum No. of records to be fetched</param>
        /// <param name="token">Authenticated token</param>
        /// <returns>Returns list of resource.</returns>
        public List<Resource> GetLatestModifiedResources(AuthenticatedToken token, int pageSize)
        {
            IQueryable<Resource> resources = _getLatestModifiedResCompiledQuery.Invoke(_context);

            if (token != null)
            {
                resources = resources
                    .Where(resource => !(resource is Identity || resource is Zentity.Security.Authorization.Group))
                    .Authorize<Resource>(UserResourcePermissions.Read, _context, token);
            }

            resources = resources.OrderByDescending(r => r.DateAdded).Take(pageSize);

            List<Resource> resourceList = resources.ToList();

            if (resourceList.Count > 0)
            {
                foreach (Resource res in resourceList)
                {
                    res.Files.Load();
                    ScholarlyWork scholWork = res as ScholarlyWork;
                    if (scholWork != null)
                        scholWork.Authors.Load();
                }
                CoreHelper.UpdateResourcesEmptyTitle(resourceList);
            }

            return resourceList;
        }

        /// <summary>
        /// Returns list of Top authors base on count of Author works submitted
        /// </summary>
        /// <param name="token">Authenticated Token</param>
        /// <param name="pageSize">Maximum No. of records to be fetched</param>
        /// <returns></returns>
        public List<Contact> GetTopAuthors(AuthenticatedToken token, int pageSize)
        {

            List<Contact> topAuthorsList = new List<Contact>();

            if (token != null)
            {
                topAuthorsList = _getTopAuthorsCompiledQuery.Invoke(_context, pageSize)
                    .Authorize<Contact>(UserResourcePermissions.Read, _context, token).ToList();
            }
            else
                topAuthorsList = _getTopAuthorsCompiledQuery.Invoke(_context, pageSize).ToList();

            if (topAuthorsList.Count > 0)
            {
                foreach (Contact contact in topAuthorsList)
                {
                    contact.Files.Load();
                }
                CoreHelper.UpdateResourcesEmptyTitle(topAuthorsList.Cast<Resource>().AsEnumerable());
            }

            return topAuthorsList;

        }

        /// <summary>
        /// Authorize user for delete permission on specified resource.
        /// </summary>
        /// <param name="token">Authenticated Token.</param>
        /// <param name="resourceId">Resource Id.</param>
        /// <returns>true, if user is authorized to delete the specified resource, else false.</returns>
        public bool AuthorizeUserForDeletePermission(AuthenticatedToken token, Guid resourceId)
        {
            if (token == null || resourceId == Guid.Empty)
            {
                return false;
            }

            bool isAuthorized = true;
            Resource res = GetResource(resourceId);
            if (res != null)
            {
                res.RelationshipsAsSubject.Load();
                res.RelationshipsAsObject.Load();

                if (!res.Authorize(UserResourcePermissions.Delete, _context, token))
                {
                    isAuthorized = false;
                }

                if (isAuthorized)
                {
                    IList<Resource> relatedResource = new List<Resource>();

                    foreach (Relationship relSubResource in res.RelationshipsAsSubject)
                    {
                        relSubResource.ObjectReference.Load();
                        if (relatedResource.Where(tuple => tuple.Id == relSubResource.Object.Id).Count() == 0)
                            relatedResource.Add(relSubResource.Object);
                    }
                    foreach (Relationship relObjResource in res.RelationshipsAsObject)
                    {
                        relObjResource.SubjectReference.Load();
                        if (relatedResource.Where(tuple => tuple.Id == relObjResource.Subject.Id).Count() == 0)
                            relatedResource.Add(relObjResource.Subject);
                    }
                    foreach (Resource relRes in relatedResource)
                    {
                        if (!relRes.Authorize(UserResourcePermissions.Update, _context, token))
                        {
                            isAuthorized = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                isAuthorized = false;
            }

            return isAuthorized;
        }

        /// <summary>
        /// Grant the default ownership to user on specific resource.
        /// </summary>
        /// <param name="token">Authenticated Token.</param>
        /// <param name="resourceId">Resource Id on which grant permission.</param>
        /// <returns>Returns true if permission granted successfully.</returns>
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

        /// <summary>
        /// Grant delete permission on category
        /// </summary>
        /// <param name="token">Authenticated Token.</param>
        /// <param name="categoryId">Category id on which give permission.</param>
        /// <returns>Returns true if permission granted successfully.</returns>
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

        internal SortProperty GetSortProperty(string resourceType)
        {
            ResourceType type = GetResourceType(resourceType);

            if (type != null && type.Name == "Tag")
            {
                return new SortProperty("Name", Zentity.Platform.SortDirection.Ascending);
            }
            else
            {
                return new SortProperty("Title", Zentity.Platform.SortDirection.Ascending);
            }
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

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Inherited method from Idisposable Interface for handling manual dispose of object.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
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
