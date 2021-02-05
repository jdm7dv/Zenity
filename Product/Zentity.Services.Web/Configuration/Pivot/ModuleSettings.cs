// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Configuration.Pivot
{
    /// <summary>
    /// Module Settings
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration", IsNullable = false)]
    public class ModuleSettings
    {
        /// <summary>
        /// Module Setting
        /// </summary>
        private ModuleSetting[] moduleSetting;

        /// <summary>
        /// Gets or sets the module setting.
        /// </summary>
        /// <value>The module setting.</value>
        [System.Xml.Serialization.XmlElementAttribute("ModuleSetting")]
        public ModuleSetting[] ModuleSetting
        {
            get
            {
                return this.moduleSetting;
            }

            set
            {
                this.moduleSetting = value;
            }
        }
    }

    /// <summary>
    /// Module Setting
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration", IsNullable = false)]
    public class ModuleSetting
    {
        /// <summary>
        /// Resource Type Settings
        /// </summary>
        private ResourceTypeSetting[] resourceTypeSettings;

        /// <summary>
        /// Name
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the resource type settings.
        /// </summary>
        /// <value>The resource type settings.</value>
        [System.Xml.Serialization.XmlElementAttribute("ResourceTypeSetting")]
        public ResourceTypeSetting[] ResourceTypeSettings
        {
            get
            {
                return this.resourceTypeSettings;
            }

            set
            {
                this.resourceTypeSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }
    }

    /// <summary>
    /// ResourceTypeSetting
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration", IsNullable = false)]
    public class ResourceTypeSetting
    {
        /// <summary>
        /// Disable Collection Creation
        /// </summary>
        private bool disableCollectionCreation;

        /// <summary>
        /// Visual
        /// </summary>
        private Visual visual;

        /// <summary>
        /// Facets
        /// </summary>
        private Facet[] facets;

        /// <summary>
        /// Link
        /// </summary>
        private Link link;

        /// <summary>
        /// Name
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the visual.
        /// </summary>
        /// <value>The visual.</value>
        public Visual Visual
        {
            get
            {
                return this.visual;
            }

            set
            {
                this.visual = value;
            }
        }

        /// <summary>
        /// Gets or sets the facets.
        /// </summary>
        /// <value>The facets.</value>
        [System.Xml.Serialization.XmlArrayItemAttribute("Facet", IsNullable = false)]
        public Facet[] Facets
        {
            get
            {
                return this.facets;
            }

            set
            {
                this.facets = value;
            }
        }

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        /// <value>The link.</value>
        public Link Link
        {
            get
            {
                return this.link;
            }

            set
            {
                this.link = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the collection creation for the resource type is disabled or not.
        /// </summary>
        /// <value>The boolean value whether to disabled or not.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("disableCollectionCreation")]
        public bool DisableCollectionCreation
        {
            get
            {
                return this.disableCollectionCreation;
            }

            set
            {
                this.disableCollectionCreation = value;
            }
        }
    }

    /// <summary>
    /// Visual
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration", IsNullable = false)]
    public class Visual
    {
        /// <summary>
        /// Type
        /// </summary>
        private VisualType type;

        /// <summary>
        /// Property Name
        /// </summary>
        private string propertyName;

        /// <summary>
        /// Clean Up Images
        /// </summary>
        private bool cleanUpImages = true;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("type")]
        public VisualType Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("propertyName")]
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }

            set
            {
                this.propertyName = value;
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the boolean value for cleanUpImages.
        /// </summary>
        /// <value>The boolean value.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("cleanup")]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool CleanUpImages
        {
            get { return this.cleanUpImages; }

            set { this.cleanUpImages = value; }
        }
    }

    /// <summary>
    /// Visual Type Enum
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    public enum VisualType
    {
        /// <summary>
        /// Default
        /// </summary>
        Default,

        /// <summary>
        /// WebCapture
        /// </summary>
        WebCapture,

        /// <summary>
        /// ImageUri
        /// </summary>
        ImageUri,

        /// <summary>
        /// ImageResource
        /// </summary>
        ImageResource
    }

    /// <summary>
    /// Facet
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration", IsNullable = false)]
    public class Facet
    {
        /// <summary>
        /// ShowInInfoPane
        /// </summary>
        private bool showInInfoPane;

        /// <summary>
        /// ShowInFilter
        /// </summary>
        private bool showInFilter;

        /// <summary>
        /// KeywordSearch
        /// </summary>
        private bool keywordSearch;

        /// <summary>
        /// Property Name
        /// </summary>
        private string propertyName;

        /// <summary>
        /// Display Name
        /// </summary>
        private string displayName;

        /// <summary>
        /// Facet Data Type
        /// </summary>
        private FacetDataType dataType;

        /// <summary>
        /// Format
        /// </summary>
        private string format;

        /// <summary>
        /// Delimiter
        /// </summary>
        private string delimiter;

        /// <summary>
        /// Initializes a new instance of the Facet class
        /// </summary>
        public Facet()
        {
            this.showInInfoPane = true;
            this.showInFilter = false;
            this.keywordSearch = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in info pane].
        /// </summary>
        /// <value><c>true</c> if [show in info pane]; otherwise, <c>false</c>.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("showInInfoPane")]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ShowInInfoPane
        {
            get
            {
                return this.showInInfoPane;
            }

            set
            {
                this.showInInfoPane = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in filter].
        /// </summary>
        /// <value><c>true</c> if [show in filter]; otherwise, <c>false</c>.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("showInFilter")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowInFilter
        {
            get
            {
                return this.showInFilter;
            }

            set
            {
                this.showInFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [keyword search].
        /// </summary>
        /// <value><c>true</c> if [keyword search]; otherwise, <c>false</c>.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("keywordSearch")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool KeywordSearch
        {
            get
            {
                return this.keywordSearch;
            }

            set
            {
                this.keywordSearch = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("propertyName")]
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }

            set
            {
                this.propertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("displayName")]
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }

            set
            {
                this.displayName = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("dataType")]
        public FacetDataType DataType
        {
            get
            {
                return this.dataType;
            }

            set
            {
                this.dataType = value;
            }
        }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("format")]
        public string Format
        {
            get
            {
                return this.format;
            }

            set
            {
                this.format = value;
            }
        }

        /// <summary>
        /// Gets or sets the delimiter.
        /// </summary>
        /// <value>The delimiter.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("delimiter")]
        public string Delimiter
        {
            get
            {
                return this.delimiter;
            }

            set
            {
                this.delimiter = value;
            }
        }
    }

    /// <summary>
    /// FacetDataType
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    public enum FacetDataType
    {
        /// <summary>
        /// String DataType
        /// </summary>
        String,

        /// <summary>
        /// LongString DataType
        /// </summary>
        LongString,

        /// <summary>
        /// Number DataType
        /// </summary>
        Number,

        /// <summary>
        /// DateTime DataType
        /// </summary>
        DateTime,

        /// <summary>
        /// Link DataType
        /// </summary>
        Link,
    }

    /// <summary>
    /// Link
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration", IsNullable = false)]
    public class Link
    {
        /// <summary>
        /// Property Name
        /// </summary>
        private string propertyName;

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        [System.Xml.Serialization.XmlAttributeAttribute("propertyName")]
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }

            set
            {
                this.propertyName = value;
            }
        }
    }

    /// <summary>
    /// Facets
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/zentity/pivot/configuration", IsNullable = false)]
    public class Facets
    {
        /// <summary>
        /// Facet
        /// </summary>
        private Facet[] facet;

        /// <summary>
        /// Gets or sets the facet.
        /// </summary>
        /// <value>The facet.</value>
        [System.Xml.Serialization.XmlElementAttribute("Facet")]
        public Facet[] Facet
        {
            get
            {
                return this.facet;
            }

            set
            {
                this.facet = value;
            }
        }
    }

    /// <summary>
    /// This class implemtents the IEqualityComparer and used for Facet comparison on the basis of name property.
    /// </summary>
    internal class FacetNameComparer : System.Collections.Generic.IEqualityComparer<Facet>
    {
        #region IEqualityComparer<Facet> Members

        /// <summary>
        /// Implement the Equals Method
        /// </summary>
        /// <param name="x">First Facet</param>
        /// <param name="y">Seconf Facet</param>
        /// <returns>Return true if equal or else false</returns>
        public bool Equals(Facet x, Facet y)
        {
            return x.PropertyName.Equals(y.PropertyName, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the Hash Code for PropertyName
        /// </summary>
        /// <param name="obj">Facet object</param>
        /// <returns>Hash Code for PropertyName</returns>
        public int GetHashCode(Facet obj)
        {
            return obj.PropertyName.GetHashCode();
        }

        #endregion
    }
}
