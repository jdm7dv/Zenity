// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Pivot
{
    #region Usings
    using System;
    using Zentity.Services.Web;
    #endregion

    #region Collection Section

    /// <summary>
    /// Collection class
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009", IsNullable = false)]
    public class Collection
    {
        /// <summary>
        /// Facet Categories
        /// </summary>
        [NonSerialized]
        private FacetCategories facetCategories;

        /// <summary>
        /// Items
        /// </summary>
        [NonSerialized]
        private Items[] items;

        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private Extension extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// Schema Version
        /// </summary>
        [NonSerialized]
        private decimal schemaVersion;

        /// <summary>
        /// Icon
        /// </summary>
        [NonSerialized]
        private string icon;

        /// <summary>
        /// Brand Image
        /// </summary>
        [NonSerialized]
        private string brandImage;

        /// <summary>
        /// Supplement
        /// </summary>
        [NonSerialized]
        private string supplement;

        /// <summary>
        /// Additional Serarch Text
        /// </summary>
        [NonSerialized]
        private string additionalSearchText;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the Collection Facet Categories
        /// </summary>
        public FacetCategories FacetCategories
        {
            get
            {
                return this.facetCategories;
            }

            set
            {
                this.facetCategories = value;
            }
        }

        /// <summary>
        /// Gets or sets the Collection Items
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("Items")]
        public Items[] Items
        {
            get
            {
                return this.items;
            }

            set
            {
                this.items = value;
            }
        }

        /// <summary>
        /// Gets or sets the Collection Extension
        /// </summary>
        public Extension Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets the Any Attribute
        /// </summary>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }

            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the SchemaVersion
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public decimal SchemaVersion
        {
            get
            {
                return this.schemaVersion;
            }
            set
            {
                this.schemaVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009", DataType = "anyURI")]
        public string Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the BrandImage
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009", DataType = "anyURI")]
        public string BrandImage
        {
            get
            {
                return this.brandImage;
            }
            set
            {
                this.brandImage = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the Suppliment
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009", DataType = "anyURI")]
        public string Supplement
        {
            get
            {
                return this.supplement;
            }
            set
            {
                this.supplement = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the AdditionalSerarchText
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
        public string AdditionalSearchText
        {
            get
            {
                return this.additionalSearchText;
            }
            set
            {
                this.additionalSearchText = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the AnyAttr
        /// </summary>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    #endregion

    #region FacetCategories Section

    /// <summary>
    /// Collection Facet Categories
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class FacetCategories
    {
        /// <summary>
        /// Facet Category
        /// </summary>
        [NonSerialized]
        private FacetCategory[] facetCategory;

        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the FacetCategory
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("FacetCategory")]
        public FacetCategory[] FacetCategory
        {
            get
            {
                return this.facetCategory;
            }
            set
            {
                this.facetCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets the Extension
        /// </summary>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets the Any Attribute
        /// </summary>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the AnyAttr
        /// </summary>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// FacetCategorie's FacetCategory
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class FacetCategory
    {
        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private FacetCategoryExtension extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// Format
        /// </summary>
        [NonSerialized]
        private string format;

        /// <summary>
        /// Type
        /// </summary>
        [NonSerialized]
        private FacetType type;

        /// <summary>
        /// IsFilterVisible
        /// </summary>
        [NonSerialized]
        private bool isFilterVisible;

        /// <summary>
        /// IsFilterVisibleSpecified
        /// </summary>
        [NonSerialized]
        private bool isFilterVisibleSpecified;

        /// <summary>
        /// IsMetaDataVisible
        /// </summary>
        [NonSerialized]
        private bool isMetaDataVisible;

        /// <summary>
        /// IsMetaDataVisibleSpecified
        /// </summary>
        [NonSerialized]
        private bool isMetaDataVisibleSpecified;

        /// <summary>
        /// IsWordWheelVisible
        /// </summary>
        [NonSerialized]
        private bool isWordWheelVisible;

        /// <summary>
        /// IsWordWheelVisibleSpecified
        /// </summary>
        [NonSerialized]
        private bool isWordWheelVisibleSpecified;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the Extension
        /// </summary>
        public FacetCategoryExtension Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets the Any Attribute
        /// </summary>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the Format
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                this.format = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public FacetType Type
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
        /// Gets or sets a value indicating whether if the filter is visible.
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
        public bool IsFilterVisible
        {
            get
            {
                return this.isFilterVisible;
            }
            set
            {
                this.isFilterVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the filter visibility is specified.
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public bool IsFilterVisibleSpecified
        {
            get
            {
                return this.isFilterVisibleSpecified;
            }
            set
            {
                this.isFilterVisibleSpecified = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the metadata is visible.
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
        public bool IsMetaDataVisible
        {
            get
            {
                return this.isMetaDataVisible;
            }
            set
            {
                this.isMetaDataVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the metadata visibility is set.
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public bool IsMetaDataVisibleSpecified
        {
            get
            {
                return this.isMetaDataVisibleSpecified;
            }
            set
            {
                this.isMetaDataVisibleSpecified = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the word wheel is visible.
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
        public bool IsWordWheelVisible
        {
            get
            {
                return this.isWordWheelVisible;
            }
            set
            {
                this.isWordWheelVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the word wheel visibility is specified.
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public bool IsWordWheelVisibleSpecified
        {
            get
            {
                return this.isWordWheelVisibleSpecified;
            }
            set
            {
                this.isWordWheelVisibleSpecified = value;
            }
        }

        /// <summary>
        /// Gets or sets the AnyAttr
        /// </summary>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// Facet Category Extension
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    public class FacetCategoryExtension
    {
        /// <summary>
        /// Date Ranges
        /// </summary>
        [NonSerialized]
        private PresetDateRange[] dateRanges;

        /// <summary>
        /// Sort Order
        /// </summary>
        [NonSerialized]
        private SortOrderList sortOrder;

        /// <summary>
        /// Gets or sets the DateRanges
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute("DateRange", IsNullable = false)]
        public PresetDateRange[] DateRanges
        {
            get
            {
                return this.dateRanges;
            }
            set
            {
                this.dateRanges = value;
            }
        }

        /// <summary>
        /// Gets or sets the SortOrder
        /// </summary>
        public SortOrderList SortOrder
        {
            get
            {
                return this.sortOrder;
            }
            set
            {
                this.sortOrder = value;
            }
        }
    }

    /// <summary>
    /// Preset Date Range
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    public class PresetDateRange
    {
        /// <summary>
        /// Lower Bound
        /// </summary>
        [NonSerialized]
        private System.DateTime lowerBound;

        /// <summary>
        /// Upper Bound
        /// </summary>
        [NonSerialized]
        private System.DateTime upperBound;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// Gets or sets the lower bound.
        /// </summary>
        /// <value>The lower bound.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public System.DateTime LowerBound
        {
            get
            {
                return this.lowerBound;
            }
            set
            {
                this.lowerBound = value;
            }
        }

        /// <summary>
        /// Gets or sets the upper bound.
        /// </summary>
        /// <value>The upper bound.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public System.DateTime UpperBound
        {
            get
            {
                return this.upperBound;
            }
            set
            {
                this.upperBound = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }
    }

    /// <summary>
    /// Sort Value Type
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    public class SortValueType
    {
        /// <summary>
        /// Value
        /// </summary>
        [NonSerialized]
        private string value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = Globals.CleanupInvalidXmlCharacters(value);
            }
        }
    }

    /// <summary>
    /// Sort Order List
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    public class SortOrderList
    {
        /// <summary>
        /// Sort Value
        /// </summary>
        [NonSerialized]
        private SortValueType[] sortValue;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// Gets or sets the sort value.
        /// </summary>
        /// <value>The sort value.</value>
        [System.Xml.Serialization.XmlElementAttribute("SortValue")]
        public SortValueType[] SortValue
        {
            get
            {
                return this.sortValue;
            }
            set
            {
                this.sortValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }
    }

    /// <summary>
    /// Facet Type Enum
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public enum FacetType
    {
        /// <summary>
        /// String Type
        /// </summary>
        String,

        /// <summary>
        /// LongString Type
        /// </summary>
        LongString,

        /// <summary>
        /// Number Type
        /// </summary>
        Number,

        /// <summary>
        /// DateTime Type
        /// </summary>
        DateTime,

        /// <summary>
        /// Link Type
        /// </summary>
        Link,

        /// <summary>
        /// Item Type
        /// </summary>
        [System.Xml.Serialization.XmlEnumAttribute(".*")]
        Item,
    }

    /// <summary>
    /// Date Range
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009", IsNullable = false)]
    public class DateRange
    {
        /// <summary>
        /// Lower Bound
        /// </summary>
        [NonSerialized]
        private System.DateTime lowerBound;

        /// <summary>
        /// Upper Bound
        /// </summary>
        [NonSerialized]
        private System.DateTime upperBound;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// Gets or sets the lower bound.
        /// </summary>
        /// <value>The lower bound.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public System.DateTime LowerBound
        {
            get
            {
                return this.lowerBound;
            }
            set
            {
                this.lowerBound = value;
            }
        }

        /// <summary>
        /// Gets or sets the upper bound.
        /// </summary>
        /// <value>The upper bound.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public System.DateTime UpperBound
        {
            get
            {
                return this.upperBound;
            }
            set
            {
                this.upperBound = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }
    }

    #endregion

    #region Items Section

    /// <summary>
    /// Item Extension
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    public class ItemExtension
    {
        /// <summary>
        /// Related
        /// </summary>
        [NonSerialized]
        private Copyright[] related;

        /// <summary>
        /// Copyright
        /// </summary>
        [NonSerialized]
        private Copyright copyright;

        /// <summary>
        /// Gets or sets the related.
        /// </summary>
        /// <value>The related.</value>
        [System.Xml.Serialization.XmlArrayItemAttribute("Link", IsNullable = false)]
        public Copyright[] Related
        {
            get
            {
                return this.related;
            }
            set
            {
                this.related = value;
            }
        }

        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>The copyright.</value>
        public Copyright Copyright
        {
            get
            {
                return this.copyright;
            }
            set
            {
                this.copyright = value;
            }
        }
    }

    /// <summary>
    /// Tag
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class Tag
    {
        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Value
        /// </summary>
        [NonSerialized]
        private string value;

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [System.Xml.Serialization.XmlTextAttribute]
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = Globals.CleanupInvalidXmlCharacters(value);
            }
        }
    }

    /// <summary>
    /// Tag List
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class TagList
    {
        /// <summary>
        /// Tag
        /// </summary>
        [NonSerialized]
        private Tag[] tag;

        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>The tag.</value>
        [System.Xml.Serialization.XmlElementAttribute("Tag")]
        public Tag[] Tag
        {
            get
            {
                return this.tag;
            }
            set
            {
                this.tag = value;
            }
        }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// Link Type
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class LinkType
    {
        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// Href
        /// </summary>
        [NonSerialized]
        private string href;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the href.
        /// </summary>
        /// <value>The href.</value>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Href
        {
            get
            {
                return this.href;
            }
            set
            {
                this.href = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// DateTime Type
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class DateTimeType
    {
        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// Value
        /// </summary>
        [NonSerialized]
        private System.DateTime value;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public System.DateTime Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// Number Type
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class NumberType
    {
        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// Value
        /// </summary>
        [NonSerialized]
        private decimal value;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public decimal Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// String Type
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class StringType
    {
        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// Value
        /// </summary>
        [NonSerialized]
        private string value;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// Facet
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class Facet
    {
        /// <summary>
        /// Simple String
        /// </summary>
        [NonSerialized]
        private StringType[] simpleString;

        /// <summary>
        /// Long String
        /// </summary>
        [NonSerialized]
        private StringType[] longString;

        /// <summary>
        /// Number
        /// </summary>
        [NonSerialized]
        private NumberType[] number;

        /// <summary>
        /// Date Time
        /// </summary>
        [NonSerialized]
        private DateTimeType[] dateTime;

        /// <summary>
        /// Link
        /// </summary>
        [NonSerialized]
        private LinkType[] link;

        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the string.
        /// </summary>
        /// <value>The string.</value>
        [System.Xml.Serialization.XmlElementAttribute("String")]
        public StringType[] String
        {
            get
            {
                return this.simpleString;
            }
            set
            {
                this.simpleString = value;
            }
        }

        /// <summary>
        /// Gets or sets the long string.
        /// </summary>
        /// <value>The long string.</value>
        [System.Xml.Serialization.XmlElementAttribute("LongString")]
        public StringType[] LongString
        {
            get
            {
                return this.longString;
            }
            set
            {
                this.longString = value;
            }
        }

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>The number.</value>
        [System.Xml.Serialization.XmlElementAttribute("Number")]
        public NumberType[] Number
        {
            get
            {
                return this.number;
            }
            set
            {
                this.number = value;
            }
        }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>The date time.</value>
        [System.Xml.Serialization.XmlElementAttribute("DateTime")]
        public DateTimeType[] DateTime
        {
            get
            {
                return this.dateTime;
            }
            set
            {
                this.dateTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        /// <value>The link.</value>
        [System.Xml.Serialization.XmlElementAttribute("Link")]
        public LinkType[] Link
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
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// Facets
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class Facets
    {
        /// <summary>
        /// Facet
        /// </summary>
        [NonSerialized]
        private Facet[] facet;

        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

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

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// Item
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class Item
    {
        /// <summary>
        /// Description
        /// </summary>
        [NonSerialized]
        private string description;

        /// <summary>
        /// Facets
        /// </summary>
        [NonSerialized]
        private Facets facets;

        /// <summary>
        /// Tags
        /// </summary>
        [NonSerialized]
        private TagList tags;

        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private ItemExtension extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// DeepZoom Image
        /// </summary>
        [NonSerialized]
        private string img;

        /// <summary>
        /// Identifier
        /// </summary>
        [NonSerialized]
        private string id;

        /// <summary>
        /// Href
        /// </summary>
        [NonSerialized]
        private string href;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [System.Xml.Serialization.XmlIgnore]
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the facets.
        /// </summary>
        /// <value>The facets.</value>
        public Facets Facets
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
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public TagList Tags
        {
            get
            {
                return this.tags;
            }
            set
            {
                this.tags = value;
            }
        }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public ItemExtension Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the img.
        /// </summary>
        /// <value>The img.</value>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Img
        {
            get
            {
                return this.img;
            }
            set
            {
                this.img = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the href.
        /// </summary>
        /// <value>The href.</value>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Href
        {
            get
            {
                return this.href;
            }
            set
            {
                this.href = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    /// <summary>
    /// Items
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/collection/metadata/2009")]
    public class Items
    {
        /// <summary>
        /// Item
        /// </summary>
        [NonSerialized]
        private Item[] item;

        /// <summary>
        /// Extension
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement extension;

        /// <summary>
        /// Any
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlElement[] any;

        /// <summary>
        /// ImageBase
        /// </summary>
        [NonSerialized]
        private string imgBase;

        /// <summary>
        /// HrefBase
        /// </summary>
        [NonSerialized]
        private string hrefBase;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        [System.Xml.Serialization.XmlElementAttribute("Item")]
        public Item[] Item
        {
            get
            {
                return this.item;
            }
            set
            {
                this.item = value;
            }
        }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public System.Xml.XmlElement Extension
        {
            get
            {
                return this.extension;
            }
            set
            {
                this.extension = value;
            }
        }

        /// <summary>
        /// Gets or sets any.
        /// </summary>
        /// <value>Any.</value>
        [System.Xml.Serialization.XmlAnyElementAttribute]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        /// <summary>
        /// Gets or sets the img base.
        /// </summary>
        /// <value>The img base.</value>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string ImgBase
        {
            get
            {
                return this.imgBase;
            }
            set
            {
                this.imgBase = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the href base.
        /// </summary>
        /// <value>The href base.</value>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string HrefBase
        {
            get
            {
                return this.hrefBase;
            }
            set
            {
                this.hrefBase = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    #endregion

    #region Extension Section

    /// <summary>
    /// Extension
    /// </summary>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    public class Extension
    {
        /// <summary>
        /// Copyright
        /// </summary>
        [NonSerialized]
        private Copyright copyright;

        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>The copyright.</value>
        public Copyright Copyright
        {
            get
            {
                return this.copyright;
            }
            set
            {
                this.copyright = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009")]
    public class Copyright
    {
        /// <summary>
        /// Href
        /// </summary>
        [NonSerialized]
        private string href;

        /// <summary>
        /// Name
        /// </summary>
        [NonSerialized]
        private string name;

        /// <summary>
        /// AnyAttr
        /// </summary>
        [NonSerialized]
        private System.Xml.XmlAttribute[] anyAttr;

        /// <summary>
        /// Gets or sets the href.
        /// </summary>
        /// <value>The href.</value>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Href
        {
            get
            {
                return this.href;
            }
            set
            {
                this.href = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = Globals.CleanupInvalidXmlCharacters(value);
            }
        }

        /// <summary>
        /// Gets or sets any attr.
        /// </summary>
        /// <value>Any attr.</value>
        [System.Xml.Serialization.XmlAnyAttributeAttribute]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttr;
            }
            set
            {
                this.anyAttr = value;
            }
        }
    }

    #endregion
}