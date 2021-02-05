// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Zentity.Core;
    using PivotConfig = Configuration.Pivot;

    /// <summary>
    /// Contains various extension methods for use in this project.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Serializes to ResourceTypeSetting to XElement.
        /// </summary>
        /// <param name="settingElement">An instance of the ResourceTypeSetting class.</param>
        /// <returns>XElement instance containing the serialized ResourceTypeSetting</returns>
        internal static XElement SerializeToXElement(this PivotConfig.ResourceTypeSetting settingElement)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            if (settingElement != null)
            {
                Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                return ExtensionMethods.Serialize(settingElement);
            }

            Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
            return null;
        }

        /// <summary>
        /// Serializes PublishRequest into XElement.
        /// </summary>
        /// <param name="publishRequest">The publish request.</param>
        /// <returns>Serialized object as XElement</returns>
        internal static XElement SerializeToXElement(this PublishRequest publishRequest)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            if (publishRequest != null)
            {
                try
                {
                    return ExtensionMethods.SerializeDataContract(publishRequest);
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                }
            }

            Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
            return null;
        }

        /// <summary>
        /// Deserializes PublishRequest from the XElement property (ObjectData) of PublishRequestDB.
        /// </summary>
        /// <param name="publishRequestDB">The publish request from DB.</param>
        /// <returns>PublishRequest from the XElement property (ObjectData) of PublishRequestDB</returns>
        internal static PublishRequest DeserializeFromXElement(this PublishRequestRecovery publishRequestDB)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            if (publishRequestDB != null)
            {
                try
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof (PublishRequest));
                    return (PublishRequest) serializer.ReadObject(publishRequestDB.ObjectData.CreateReader(ReaderOptions.OmitDuplicateNamespaces));
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                }
            }

            Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
            return null;
        }

        /// <summary>
        /// Creates a default ResourceTypeSetting instance from a Core.ResourceType object
        /// </summary>
        /// <param name="resourceType">An instance of Core.ResourceType.</param>
        /// <returns>A default ResourceTypeSetting instance</returns>
        internal static PivotConfig.ResourceTypeSetting ToResourceTypeSetting(this Core.ResourceType resourceType)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            PivotConfig.ResourceTypeSetting newSetting = new PivotConfig.ResourceTypeSetting
                                                             {
                                                                 Name = resourceType.Name,
                                                                 Visual = new PivotConfig.Visual
                                                                          {
                                                                              PropertyName = string.Empty,
                                                                              Type = PivotConfig.VisualType.Default
                                                                          },
                                                                 Link = new PivotConfig.Link { PropertyName = string.Empty }
                                                             };
            newSetting.Facets = GetConfigFacetList(resourceType).ToArray();
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
            return newSetting;
        }

        /// <summary>
        /// Validates the ResourceTypeSetting instance with the current properties of a Core.ResourceType object.
        /// </summary>
        /// <param name="resourceType">The Core.ResourceType instance to do the validation against.</param>
        /// <param name="resourceTypeSetting">The instance of ResourceTypeSetting.</param>
        /// <returns>Return true if success else false.</returns>
        internal static bool ValidateResourceTypeSetting(this Core.ResourceType resourceType, PivotConfig.ResourceTypeSetting resourceTypeSetting)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            bool validationSuccess = true;
            PivotConfig.ResourceTypeSetting currentResourceTypeSetting = resourceType.ToResourceTypeSetting();
            if (resourceTypeSetting.Facets == null)
            {
                return false;
            }

            List<PivotConfig.Facet> resourceTypeFacets = new List<PivotConfig.Facet>();
            resourceTypeSetting.Facets.ToList().ForEach(f =>
            {
                if (f != null && !string.IsNullOrWhiteSpace(f.PropertyName) && !string.IsNullOrEmpty(f.PropertyName))
                {
                    resourceTypeFacets.Add(f);
                }
            });
            PivotConfig.FacetNameComparer facetComparer = new PivotConfig.FacetNameComparer();

            IEnumerable<PivotConfig.Facet> missingFacets = resourceTypeFacets.Except(currentResourceTypeSetting.Facets, facetComparer);
            if (missingFacets.Count() > 0)
            {
                validationSuccess = false;
                while (missingFacets.Count() > 0)
                {
                    var facetToRemove = missingFacets.FirstOrDefault();
                    if (facetToRemove != null)
                    {
                        resourceTypeFacets.Remove(facetToRemove);
                    }
                }
            }

            resourceTypeFacets.ForEach(facet =>
            {
                if (facet.DataType == PivotConfig.FacetDataType.Link || facet.DataType == PivotConfig.FacetDataType.LongString)
                {
                    facet.ShowInFilter = false;
                }
            });

            IEnumerable<string> duplicateFacet = resourceTypeFacets.GroupBy(f => f.DisplayName).Where(f => f.Count() > 1).Select(f=>f.Key).AsEnumerable();
            if (duplicateFacet.Count() > 0)
            {
                throw new FaultException(new FaultReason(string.Format(Properties.Messages.DuplicateFacetsNotAllowed, string.Join(",", duplicateFacet))));
            }

            resourceTypeSetting.Facets = resourceTypeFacets.ToArray();
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
            return validationSuccess;
        }

        /// <summary>
        /// Generates the list of PivotConfig.Facet for a resource type and all its base types recursively.
        /// </summary>
        /// <param name="resourceType">ResourceType intance for the Facet generation</param>
        /// <param name="addResourceTypeFacet">if set to <c>true</c> [add ResourceType facet].</param>
        /// <returns>List of PivotConfig.Facet objects</returns>
        private static IEnumerable<PivotConfig.Facet> GetConfigFacetList(Core.ResourceType resourceType, bool addResourceTypeFacet = true)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            if (resourceType == null)
            {
                Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
                return null;
            }

            List<PivotConfig.Facet> listFacets = new List<PivotConfig.Facet>();
            if (addResourceTypeFacet)
            {
                listFacets.Add(new PivotConfig.Facet
                {
                    DisplayName = "Zentity " + typeof(ResourceType).Name,
                    PropertyName = typeof(ResourceType).Name,
                    Format = string.Empty,
                    KeywordSearch = true,
                    ShowInFilter = true,
                    ShowInInfoPane = true,
                    Delimiter = string.Empty,
                    DataType = PivotConfig.FacetDataType.String,
                });
            }

            string[] excludedPropertyNames = Properties.Resources.ExcludedProperties.Split(new[] { ',' });

            // Check if the current ResourceType has a base ResourceType. 
            // Generate the Facet list for base ResourceType and add it to the current Facet collection
            if (resourceType.BaseType != null)
            {
                listFacets.AddRange(GetConfigFacetList(resourceType.BaseType, false));
            }

            #region Scalar Properties

            // Iterate through the Scalar Properties and add them to the PivotConfig.Facet list.
            foreach (Core.ScalarProperty scalarProperty in resourceType.ScalarProperties)
            {
                if (excludedPropertyNames.Contains(scalarProperty.Name))
                {
                    continue;
                }

                PivotConfig.Facet newFacet = new PivotConfig.Facet
                {
                    DisplayName = scalarProperty.Name,
                    PropertyName = scalarProperty.Name,
                    Format = string.Empty,
                    KeywordSearch = false,
                    ShowInFilter = true,
                    ShowInInfoPane = true,
                    Delimiter = string.Empty
                };

                switch (scalarProperty.DataType)
                {
                    case Core.DataTypes.String:
                    case Core.DataTypes.Guid:
                        newFacet.DataType = scalarProperty.MaxLength > 500 ? PivotConfig.FacetDataType.LongString : PivotConfig.FacetDataType.String;
                        newFacet.KeywordSearch = true;
                        if (newFacet.DataType == PivotConfig.FacetDataType.LongString)
                        {
                            newFacet.ShowInFilter = false;
                        }
                        if (newFacet.PropertyName.Equals(typeof(Uri).Name, StringComparison.OrdinalIgnoreCase))
                        {
                            newFacet.DataType = PivotConfig.FacetDataType.Link;
                            newFacet.ShowInFilter = false;
                        }
                        break;

                    case Core.DataTypes.Int16:
                    case Core.DataTypes.Int32:
                    case Core.DataTypes.Int64:
                    case Core.DataTypes.Byte:
                        newFacet.DataType = PivotConfig.FacetDataType.Number;
                        newFacet.Format = "0";
                        break;

                    case Core.DataTypes.Single:
                    case Core.DataTypes.Double:
                    case Core.DataTypes.Decimal:

                        newFacet.DataType = PivotConfig.FacetDataType.Number;
                        newFacet.Format = "0.000";
                        break;

                    case Core.DataTypes.Boolean:
                        newFacet.DataType = PivotConfig.FacetDataType.String;
                        break;

                    case Core.DataTypes.DateTime:
                        newFacet.DataType = PivotConfig.FacetDataType.DateTime;
                        newFacet.Format = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                        break;

                    default:
                        continue;
                }

                listFacets.Add(newFacet);
            }
            #endregion

            #region Navigation Properties

            // Iterate through the Navigation Properties and add them to the PivotConfig.Facet list.
            foreach (Core.NavigationProperty navProperty in resourceType.NavigationProperties)
            {
                if (excludedPropertyNames.Contains(navProperty.Name))
                {
                    continue;
                }

                PivotConfig.Facet newFacet = new PivotConfig.Facet
                                                 {
                                                     DisplayName = navProperty.Name,
                                                     PropertyName = navProperty.Name,
                                                     Format = string.Empty,
                                                     DataType = PivotConfig.FacetDataType.Link,
                                                     KeywordSearch = true,
                                                     ShowInFilter = false,
                                                     ShowInInfoPane = true
                                                 };

                listFacets.Add(newFacet);
            }

            #endregion

            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
            return listFacets;
        }

        /// <summary>
        /// Serializes the specified object to an XElement object.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns>XElement instance containing the serialized object</returns>
        private static XElement Serialize(object objectToSerialize)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            StringBuilder stringBuilderSerializeOutput = new StringBuilder();
            if (objectToSerialize != null)
            {
                XmlSerializer serializer = new XmlSerializer(objectToSerialize.GetType());
                using (XmlWriter writer = XmlWriter.Create(stringBuilderSerializeOutput))
                {
                    if (writer != null) serializer.Serialize(writer, objectToSerialize);
                    Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.FinishedFunctionMessage);
                    return XElement.Parse(stringBuilderSerializeOutput.ToString(), LoadOptions.None);
                }
            }
            else
            {
                Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
                return null;
            }
        }

        /// <summary>
        /// Serializes the specified object to an XElement object.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns>XElement instance containing the serialized object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "The 'StringWriter' instance is used within a using block. The 'ObjectDisposed' exception wont be raised.")]
        private static XElement SerializeDataContract(object objectToSerialize)
        {
            Globals.TraceMessage(TraceEventType.Verbose, string.Empty, Properties.Messages.StartedFunctionMessage);
            if (objectToSerialize != null)
            {
                try
                {
                    DataContractSerializer serializer = new DataContractSerializer(objectToSerialize.GetType());
                    using (StringWriter stringWriter = new StringWriter())
                    {
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter))
                        {
                            serializer.WriteObject(xmlWriter, objectToSerialize);
                            return XElement.Parse(stringWriter.ToString(), LoadOptions.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                }
            }
            
            Globals.TraceMessage(TraceEventType.Verbose, Properties.Messages.NullEmptyMessage, Properties.Messages.NullEmptyTitle);
            return null;
        }
    }
}
