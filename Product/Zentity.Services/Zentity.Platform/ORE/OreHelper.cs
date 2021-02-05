// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************








namespace Zentity.Platform
{
    #region Using Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using Zentity.Core;
    using System.Configuration;
    #endregion

    #region OreHelper Class
    internal static class OreHelper
    {
        #region Internal methods

        /// <summary>
        /// Gets all the scalar properties for a resource type 
        /// </summary>
        /// <param name="resourceTypeInfo">type of resource</param>
        /// <returns>scalar property collection</returns>
        internal static IEnumerable<ScalarProperty> GetScalarPropertyCollection(string resourceTypeInfo)
        {
            if(string.IsNullOrEmpty(resourceTypeInfo))
            {
                throw new ArgumentNullException("resourceTypeInfo");
            }
            IEnumerable<ScalarProperty> unionProperties = new List<ScalarProperty>();
            ResourceType matchedResourceType = new ResourceType();

            while(!string.IsNullOrEmpty(resourceTypeInfo))
            {
                matchedResourceType = new ResourceType();
                matchedResourceType = new CoreHelper().GetResourceType(resourceTypeInfo);
                // Get scalar properties for resource type
                ScalarPropertyCollection propertyCollection = matchedResourceType.ScalarProperties;

                // Union of scalar properties of from current resource type to base type
                unionProperties = unionProperties.AsEnumerable().Union(propertyCollection.AsEnumerable());

                if(matchedResourceType.BaseType != null)
                {
                    resourceTypeInfo = matchedResourceType.BaseType.FullName.Substring(
                    matchedResourceType.BaseType.FullName.LastIndexOf(".", StringComparison.Ordinal) + 1);
                }
                else
                {
                    break;
                }
            }
            return unionProperties;
        }

        #endregion


        #region Private methods
        #endregion
    }
    #endregion
}
