// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Pivot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;
    using Zentity.Core;
    using Configuration.Pivot;

    /// <summary>
    /// The concrete implementation of the ConfigurationService class.
    /// This class implements the IConfigurationSercice contract and provides the functionality for Pivot Configuration Service.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        #region IPivotConfigurationService Members

        /// <summary>
        /// Gets the complete configuration including resource types and their facets for a particular model.
        /// </summary>
        /// <param name="modelNamespace">The model namespace name.</param>
        /// <returns>An instance of the ModuleSetting configuration class </returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public ModuleSetting GetModelConfiguration(string modelNamespace)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(modelNamespace);
            if (resourceTypes != null)
            {
                ModuleSetting moduleSetting = new ModuleSetting { Name = modelNamespace };

                if (resourceTypes.Count() > 0)
                {
                    List<ResourceTypeSetting> resourceTypeSettings = new List<ResourceTypeSetting>();
                    foreach (ResourceType resourceType in resourceTypes)
                    {
                        if (resourceType.ConfigurationXml != null && !string.IsNullOrWhiteSpace(resourceType.ConfigurationXml.ToString()))
                        {
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResourceTypeSetting));
                            XmlReader configXmlReader = resourceType.ConfigurationXml.CreateReader();
                            if (xmlSerializer.CanDeserialize(configXmlReader))
                            {
                                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.ConfigXmlDB);
                                resourceTypeSettings.Add((ResourceTypeSetting)xmlSerializer.Deserialize(configXmlReader));
                            }
                            else
                            {
                                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.ConfigXmlCreation);
                                resourceTypeSettings.Add(resourceType.ToResourceTypeSetting());
                            }
                        }
                        else
                        {
                            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.ConfigXmlCreation);
                            resourceTypeSettings.Add(resourceType.ToResourceTypeSetting());
                        }
                    }

                    moduleSetting.ResourceTypeSettings = resourceTypeSettings.ToArray();
                }

                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                return moduleSetting;
            }

            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.NullEmptyStringMessage);
            return null;
        }

        /// <summary>
        /// Gets the complete configuration including facets for a particular resource type.
        /// </summary>
        /// <param name="modelNamespace">The model namespace name.</param>
        /// <param name="resourceTypeName">The resource type name.</param>
        /// <returns>Retuns ResourceTypeSetting.</returns>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityUsers")]
        public ResourceTypeSetting GetResourceTypeConfiguration(string modelNamespace, string resourceTypeName)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            #region Validating Parameters

            if (string.IsNullOrEmpty(modelNamespace) || string.IsNullOrWhiteSpace(modelNamespace) || string.IsNullOrEmpty(resourceTypeName) || string.IsNullOrWhiteSpace(resourceTypeName))
            {
                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.NullEmptyStringMessage);
                return null;
            }
            #endregion
            IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(modelNamespace);
            if (resourceTypes == null || resourceTypes.Count() == 0)
            {
                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.NullEmptyStringMessage);
                return null;
            }
            ResourceType specificResourceType = (from ResourceType resourceType in resourceTypes
                                                 where resourceType.Name.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase)
                                                 select resourceType).FirstOrDefault();

            if (specificResourceType != null)
            {
                if (specificResourceType.ConfigurationXml != null && !string.IsNullOrWhiteSpace(specificResourceType.ConfigurationXml.ToString()))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResourceTypeSetting));
                    XmlReader configXmlReader = specificResourceType.ConfigurationXml.CreateReader();
                    if (xmlSerializer.CanDeserialize(configXmlReader))
                    {
                        ResourceTypeSetting resourceTypeSetting = (ResourceTypeSetting)xmlSerializer.Deserialize(configXmlReader);
                        specificResourceType.ValidateResourceTypeSetting(resourceTypeSetting);
                        Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.ConfigXmlCreation);
                        return resourceTypeSetting;
                    }

                    Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                    return specificResourceType.ToResourceTypeSetting();
                }

                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                return specificResourceType.ToResourceTypeSetting();
            }

            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.NullEmptyStringMessage);
            return null;
        }

        /// <summary>
        /// Sets the complete module configuration including resource types and their facets.
        /// </summary>
        /// <param name="modelNamespace">The model namespace name.</param>
        /// <param name="modelConfiguration">The model configuration details.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void SetModelConfiguration(string modelNamespace, ModuleSetting modelConfiguration)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            if (modelConfiguration != null)
            {
                IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(modelNamespace);
                IEnumerable<ResourceTypeSetting> resourceTypeSettings = modelConfiguration.ResourceTypeSettings.AsEnumerable();
                if (resourceTypes != null)
                {
                    using (ZentityContext context = Utilities.CreateZentityContext())
                    {
                        if (modelConfiguration.ResourceTypeSettings == null)
                        {
                            foreach (ResourceType resourceType in resourceTypes)
                            {
                                resourceType.ConfigurationXml = null;
                                resourceType.Update(context.StoreConnectionString);
                            }
                        }
                        else
                        {
                            foreach (ResourceTypeSetting resourceTypeSetting in resourceTypeSettings)
                            {
                                ResourceType specificResourceType = resourceTypes.Where(res => res.Name.Equals(resourceTypeSetting.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                if (specificResourceType != null)
                                {
                                    specificResourceType.ValidateResourceTypeSetting(resourceTypeSetting);
                                    specificResourceType.ConfigurationXml = resourceTypeSetting.SerializeToXElement();
                                    specificResourceType.Update(context.StoreConnectionString);
                                    Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new FaultException<ArgumentException>(new ArgumentException(Properties.Messages.ModelNamespaceParameter), Properties.Messages.NullModelNamespaceMessage);
                }
            }
            else
            {
                throw new FaultException<ArgumentNullException>(new ArgumentNullException(Properties.Messages.ModelConfigurationParameter), Properties.Messages.NullModelConfigurationMessage);
            }
        }

        /// <summary>
        /// Sets the complete resource type configuration including the facets.
        /// </summary>
        /// <param name="modelNamespace">The model namespace name.</param>
        /// <param name="resourceTypeName">The resource type name.</param>
        /// <param name="resourceTypeConfig">The resource type configuration details.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = "ZentityAdministrators")]
        public void SetResourceTypeConfiguration(string modelNamespace, string resourceTypeName, ResourceTypeSetting resourceTypeConfig)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            if (string.IsNullOrWhiteSpace(modelNamespace) || string.IsNullOrWhiteSpace(resourceTypeName))
            {
                throw new FaultException<ArgumentException>(new ArgumentException(Properties.Messages.ArgumentsNullOrEmpty), Properties.Messages.SpecifyNamespaceAndResourceName);
            }

            if (resourceTypeConfig != null && (resourceTypeConfig.Facets == null || resourceTypeConfig.Facets.Count() == 0))
            {
                throw new FaultException(new FaultReason(Properties.Messages.FacetsNullOrEmpty));
            }

            IEnumerable<ResourceType> resourceTypes = Utilities.GetResourceTypes(modelNamespace);
            if (resourceTypes != null)
            {
                ResourceType specificResourceType = resourceTypes.Where(res => res.Name.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (specificResourceType != null)
                {
                    using (ZentityContext context = Utilities.CreateZentityContext())
                    {
                        if (resourceTypeConfig != null)
                        {
                            resourceTypeConfig.Name = resourceTypeName;
                            specificResourceType.ValidateResourceTypeSetting(resourceTypeConfig);
                            specificResourceType.ConfigurationXml = resourceTypeConfig.SerializeToXElement();
                            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                        }
                        else
                        {
                            specificResourceType.ConfigurationXml = null;
                            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                        }

                        specificResourceType.Update(context.StoreConnectionString);
                    }
                }
                else
                {
                    throw new FaultException<ArgumentException>(new ArgumentException(Properties.Messages.ResourceTypeNameParameter), Properties.Messages.NullRResourceTypeNameMessage);
                }
            }
            else
            {
                throw new FaultException<ArgumentException>(new ArgumentException(Properties.Messages.ModelNamespaceParameter), Properties.Messages.NullModelNamespaceMessage);
            }
        }

        #endregion
    }
}
