// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Visual explorer graph filter options. 
    /// Filters the graph based on resource type and resource relationship.
    /// </summary>
    [DataContractAttribute(Name = "VisualExplorerGraphFilterOptions")]
    public class VisualExplorerGraphFilterOptions
    {
        /// <summary>
        /// Gets or sets list of resources. 
        /// </summary>
        [DataMemberAttribute()]
        public List<ResourceType> Resources { get; set; }

        /// <summary>
        /// Gets or sets the relationships.
        /// </summary>
        [DataMemberAttribute()]
        public List<RelationshipType> Relationships { get; set; }

        /// <summary>
        /// Loads the visual explorer graph filter options from the isolated storage. 
        /// Returns a new instance of the type VisualExplorerGraphFilterOptions if no previously 
        /// saved values are found in the isolated storage.
        /// </summary>
        /// <returns>Returns a value of the type VisualExplorerGraphFilterOptions</returns>
        public static VisualExplorerGraphFilterOptions Load()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(VisualExplorerResource.FilterOptionsIsolatedStorageKey))
            {
                return (VisualExplorerGraphFilterOptions)IsolatedStorageSettings.ApplicationSettings[VisualExplorerResource.FilterOptionsIsolatedStorageKey];
            }

            return new VisualExplorerGraphFilterOptions();
        }

        /// <summary>
        /// Merges the list of resources and relationships with the existing values.
        /// </summary>
        /// <param name="resourceTypes">List of resource type names</param>
        /// <param name="relationships">List of relationship names</param>
        /// <returns>Merged VisualExplorerGraphFilterOptions object</returns>
        public static VisualExplorerGraphFilterOptions Merge(List<string> resourceTypes, List<string> relationships)
        {
            VisualExplorerGraphFilterOptions options = Load();
            if (options.Resources == null)
            {
                options.Resources = new List<ResourceType>();
            }

            if (options.Relationships == null)
            {
                options.Relationships = new List<RelationshipType>();
            }

            resourceTypes.ForEach(resource =>
            {
                if (!options.Resources.Contains(new ResourceType(resource)))
                {
                    options.Resources.Add(new ResourceType(resource));
                }
            });

            relationships.ForEach(relationship =>
            {
                if (!options.Relationships.Contains(new RelationshipType(relationship)))
                {
                    options.Relationships.Add(new RelationshipType(relationship));
                }
            });

            options.Relationships = options.Relationships.OrderBy(relationship => relationship.Name).ToList();
            options.Resources = options.Resources.OrderBy(resource => resource.Name).ToList();

            return options;
        }

        /// <summary>
        /// Gets a resource type from the filter options.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>Resource type matching the resource name</returns>
        public ResourceType GetResource(string resourceName)
        {
            int resourceIndex = this.Resources.IndexOf(new ResourceType(resourceName));
            if (resourceIndex < 0)
            {
                return null;
            }

            return this.Resources[resourceIndex];
        }

        /// <summary>
        /// Gets a relationship from the filter options.
        /// </summary>
        /// <param name="relationshipName">Relationship name</param>
        /// <returns>Relationship matching the relationship name</returns>
        public RelationshipType GetRelationship(string relationshipName)
        {
            int relationshipIndex = this.Relationships.IndexOf(new RelationshipType(relationshipName));
            if (relationshipIndex < 0)
            {
                return null;
            }

            return this.Relationships[relationshipIndex];
        }

        /// <summary>
        /// Indicates if a resource is visible in the search results.
        /// </summary>
        /// <param name="resourceName">Resource Name</param>
        /// <returns>System.Boolean value; true if the resource is visible in the search results, false otherwise. </returns>
        public bool IsResourceVisible(string resourceName)
        {
            int resourceIndex = this.Resources.IndexOf(new ResourceType(resourceName));
            if (resourceIndex < 0)
            {
                return false;
            }

            return this.Resources[resourceIndex].IsVisible;
        }

        /// <summary>
        /// Indicates if a relationship is visible in the search results.
        /// </summary>
        /// <param name="relationshipName">Relationship Name</param>
        /// <returns>System.Boolean value; true if the relationship is visible in the search results, false otherwise. </returns>
        public bool IsRelationshipVisible(string relationshipName)
        {
            int relationshipIndex = this.Relationships.IndexOf(new RelationshipType(relationshipName));
            if (relationshipIndex < 0)
            {
                return false;
            }

            return this.Relationships[relationshipIndex].IsVisible;
        }

        /// <summary>
        /// Saves the visual explorer graph filter options to the isolated storage.
        /// </summary>
        public void Save()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(VisualExplorerResource.FilterOptionsIsolatedStorageKey))
            {
                IsolatedStorageSettings.ApplicationSettings[VisualExplorerResource.FilterOptionsIsolatedStorageKey] = this;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add(VisualExplorerResource.FilterOptionsIsolatedStorageKey, this);
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        /// <summary>
        /// Resource Type.
        /// </summary>
        [DataContractAttribute(Name = "ResourceType")]
        public class ResourceType : IEquatable<ResourceType>, INotifyPropertyChanged
        {
            /// <summary>
            /// Resource type name.
            /// </summary>
            private string name;

            /// <summary>
            /// Resource rendering color.
            /// </summary>
            private string color;

            /// <summary>
            /// Resource type visibilty.
            /// </summary>
            private bool isVisible;

            /// <summary>
            /// Initializes a new instance of the ResourceType class
            /// </summary>
            public ResourceType()
            {
            }

            /// <summary>
            /// Initializes a new instance of the ResourceType class
            /// </summary>
            /// <param name="resourceName">Resource name</param>
            /// <param name="isVisible">Is visible in search results</param>
            /// <param name="color">Default color for the ResourceType</param>
            public ResourceType(string resourceName, bool isVisible = true, string color = "000000")
            {
                this.Name = resourceName;
                this.IsVisible = isVisible;
                this.Color = color;
            }

            /// <summary>
            /// This even is handled by the binding system to refresh the data when the value is changed.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Gets or sets the resource name.
            /// </summary>
            [DataMemberAttribute()]
            public string Name
            {
                get
                {
                    return this.name;
                }

                set
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                }
            }

            /// <summary>
            /// Gets or sets the resource rendering color.
            /// </summary>
            [DataMemberAttribute()]
            public string Color
            {
                get
                {
                    return this.color;
                }

                set
                {
                    this.color = value;
                    this.OnPropertyChanged("Color");
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the resource is visible
            /// in the search results.
            /// </summary>
            [DataMemberAttribute()]
            public bool IsVisible
            {
                get
                {
                    return this.isVisible;
                }

                set
                {
                    this.isVisible = value;
                    this.OnPropertyChanged("IsVisible");
                }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object</param>
            /// <returns>System.Boolean value; true is equal, false otherwise</returns>
            public bool Equals(ResourceType other)
            {
                return this.Name.Equals(other.Name);
            }

            /// <summary>
            /// On property changed. Should be called to raise the proerpty change event notification 
            /// so that the binding system refreshes the data on the UI.
            /// </summary>
            /// <param name="propertyName">Property name</param>
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        /// <summary>
        /// Relationship Type.
        /// </summary>
        [DataContractAttribute(Name = "RelationshipType")]
        public class RelationshipType : IEquatable<RelationshipType>, INotifyPropertyChanged
        {
            /// <summary>
            /// Relationship type name.
            /// </summary>
            private string name;

            /// <summary>
            /// Relationship visibilty.
            /// </summary>
            private bool isVisible;

            /// <summary>
            /// Initializes a new instance of the RelationshipType class            
            /// </summary>
            public RelationshipType()
            {
            }

            /// <summary>
            /// Initializes a new instance of the RelationshipType class
            /// </summary>
            /// <param name="relationshipName">Relationship name</param>
            /// <param name="isVisible">Is visible in search results</param>
            public RelationshipType(string relationshipName, bool isVisible = true)
            {
                this.Name = relationshipName;
                this.IsVisible = isVisible;
            }

            /// <summary>
            /// This even is handled by the binding system to refresh the data when the value is changed.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Gets or sets the relationship name.
            /// </summary>
            [DataMemberAttribute()]
            public string Name
            {
                get
                {
                    return this.name;
                }

                set
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the relationship is visible
            /// in the search results.
            /// </summary>
            [DataMemberAttribute()]
            public bool IsVisible
            {
                get 
                { 
                    return this.isVisible; 
                }

                set
                {
                    this.isVisible = value;
                    this.OnPropertyChanged("IsVisible");
                }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object</param>
            /// <returns>System.Boolean value; true is equal, false otherwise</returns>
            public bool Equals(RelationshipType other)
            {
                return this.Name.Equals(other.Name);
            }

            /// <summary>
            /// On property changed. Should be called to raise the proerpty change event notification 
            /// so that the binding system refreshes the data on the UI.
            /// </summary>
            /// <param name="propertyName">Property name</param>
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
