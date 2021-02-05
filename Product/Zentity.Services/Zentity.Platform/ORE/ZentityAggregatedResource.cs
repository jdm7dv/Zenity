// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Zentity.Core;
    using Zentity.ScholarlyWorks;
    using Zentity.Security.AuthorizationHelper;

    #region ZentityAggregatedResource Class
    /// <summary>
    /// This class is responsible for extracting resource information
    /// </summary>
    class ZentityAggregatedResource : AbstractAggregatedResource<Guid>
    {
        #region Member Variables
        private int levelOfStripping;

        #endregion

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityAggregatedResource"/> class.
        /// </summary>
        /// <param name="id">id of the resource</param>
        /// <param name="levelOfStripping">stripping level</param>
        public ZentityAggregatedResource(Guid id, int levelOfStripping)
        {
            if(Guid.Empty == id)
                return;

            ResourceUri = id;
            this.levelOfStripping = levelOfStripping;
            Resource resource;
            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                resource = context.Resources
                    .Where(res => res.Id == ResourceUri)
                    .FirstOrDefault();

                if(resource == null
                    || !resource.Authorize("Read", context, CoreHelper.GetAuthenticationToken()))
                {
                    throw new ArgumentException(Properties.Resources.ORE_RESOURCE_NOT_FOUND, "id");
                }
            }

            if(string.IsNullOrEmpty(ResourcesType))
                ResourcesType = resource.GetType().ToString();
            RetrieveRelations();
            RetrieveTagsAndCategories();
            RetrieveMetadata(resource);
            RetrieveAggreagates(levelOfStripping);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Creates a Memento
        /// </summary>
        /// <returns>memento object</returns>
        override public Memento<Guid> CreateMemento()
        {
            Memento<Guid> state = new Memento<Guid>();
            state.AggreagtedResources = AggreagtedResources;
            state.RelationUris = RelationUris;
            state.ObjectResourceIds = ObjectResourceIds;
            state.ObjectTypes = ObjectTypes;
            state.CategoryNames = CategoryNames;
            state.TagNames = TagNames;
            state.ResourceType = ResourcesType;
            state.ResourceCreator = ResourceCreator;
            state.ResourceModified = ResourceModified;
            state.ScalarProperties = ScalarProperties;
            state.AggregateTypes = AggregateTypes;
            return state;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Retrieves relationship of the resource
        /// </summary>        
        private void RetrieveRelations()
        {
            if(levelOfStripping <= 0)
                return;

            List<Relationship> containedResources;
            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                Resource resource = context.Resources
                    .Where(res => res.Id == ResourceUri)
                    .FirstOrDefault();

                if(resource == null
                    || !resource.Authorize("Read", context, CoreHelper.GetAuthenticationToken()))
                {
                    return;
                }

                resource.RelationshipsAsSubject.Load();

                containedResources = resource.RelationshipsAsSubject.ToList();

                foreach(Relationship rel in containedResources)
                {
                    rel.ObjectReference.Load();
                    ObjectResourceIds.Add(rel.Object.Id);

                    Type objectType = rel.Object.GetType();

                    ObjectTypes.Add(objectType);

                    rel.PredicateReference.Load();
                    RelationUris.Add(rel.Predicate.Name);
                }
            }
        }

        /// <summary>
        /// retrieves metadata for resource
        /// </summary>
        /// <param name="resource">the resource object</param>

        private void RetrieveMetadata(Resource resource)
        {
            if(levelOfStripping <= 0)
                return;
            List<ScalarProperty> resourceScalarProperties = OreHelper.GetScalarPropertyCollection(ResourcesType).ToList();
            Type type = CoreHelper.GetSystemResourceType(ResourcesType);
            foreach(ScalarProperty scalarProperty in resourceScalarProperties)
            {
                PropertyInfo property = type.GetProperty(scalarProperty.Name);
                object scalarValue = property.GetValue(resource, null);
                if(scalarValue != null)
                {
                    PropertyInformation propertyInfo = new PropertyInformation();
                    propertyInfo.PropertyType = property.PropertyType.ToString();
                    propertyInfo.PropertyName = property.Name;
                    propertyInfo.PropertyValue = scalarValue.ToString();
                    ScalarProperties.Add(propertyInfo);
                }
            }
        }

        /// <summary>
        /// retrieves aggregate relations for a resource
        /// </summary>
        /// <param name="levelOfStripping">stripping level</param>        
        private void RetrieveAggreagates(int levelOfStripping)
        {
            //No retrievals below stripping level
            if(levelOfStripping <= 0)
                return;
            Debug.Assert(ResourceUri != Guid.Empty);

            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                List<Relationship> contains = context.Relationships.Where(
                   relationship =>
                       relationship.Subject.Id == ResourceUri &&
                       relationship.Predicate.Uri == Properties.Resources.ORE_CONTAINS_PREDICATE
                   ).ToList();
                foreach(Relationship relation in contains)
                {
                    relation.ObjectReference.Load();
                    ZentityAggregatedResource resource = new ZentityAggregatedResource(relation.Object.Id, --levelOfStripping);
                    AggreagtedResources.Add(resource);
                    Type objectType = context.Resources.Where(res => res.Id == relation.Object.Id).First().GetType();
                    AggregateTypes.Add(objectType);

                }
            }
        }

        /// <summary>
        /// retrieves tags and categories for the resource
        /// </summary>        
        private void RetrieveTagsAndCategories()
        {
            using(ZentityContext context = CoreHelper.CreateZentityContext())
            {
                ScholarlyWorkItem resource = context.Resources
                    .OfType<ScholarlyWorkItem>()
                    .Where(res => res.Id == ResourceUri)
                    .FirstOrDefault();

                if(resource == null
                    || !resource.Authorize("Read", context, CoreHelper.GetAuthenticationToken()))
                {
                    return;
                }

                resource.CategoryNodes.Load();
                List<string> categoryNames = resource.CategoryNodes
                    .Select(category => category.Title)
                    .ToList();
                CategoryNames.AddRange(categoryNames);

                resource.Tags.Load();
                List<string> tagNames = resource.Tags
                    .Select(tag => tag.Name)
                    .ToList();
                TagNames.AddRange(tagNames);
            }
        }
        #endregion
    }
    #endregion
}
