// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Configuration.Pivot
{
    /// <summary>
    /// The PivotConfigurationSection Configuration Section.
    /// </summary>
    public class PivotConfigurationSection : global::System.Configuration.ConfigurationSection
    {
        #region Singleton Instance
        /// <summary>
        /// The XML name of the PivotConfigurationSection Configuration Section.
        /// </summary>
        internal const string PivotConfigurationSectionSectionName = "PivotConfig";

        /// <summary>
        /// PivotConfigurationSection object.
        /// </summary>
        private static PivotConfigurationSection pivotConfigurationSection;

        /// <summary>
        /// Gets and sets the PivotConfigurationSection instance.
        /// </summary>
        public static PivotConfigurationSection Instance
        {
            get
            {
                if (pivotConfigurationSection == null)
                {
                    return ((PivotConfigurationSection)(global::System.Configuration.ConfigurationManager.GetSection(PivotConfigurationSection.PivotConfigurationSectionSectionName)));
                }

                else
                    return pivotConfigurationSection;
            }
            internal set
            {
                pivotConfigurationSection = value;
            }
        }
        #endregion

        #region Xmlns Property
        /// <summary>
        /// The XML name of the <see cref="Xmlns"/> property.
        /// </summary>
        internal const string XmlnsPropertyName = "xmlns";

        /// <summary>
        /// Gets the XML namespace of this Configuration Section.
        /// </summary>
        /// <remarks>
        /// This property makes sure that if the configuration file contains the XML namespace,
        /// the parser doesn't throw an exception because it encounters the unknown "xmlns" attribute.
        /// </remarks>
        [global::System.Configuration.ConfigurationPropertyAttribute(PivotConfigurationSection.XmlnsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Xmlns
        {
            get
            {
                return ((string)(base[PivotConfigurationSection.XmlnsPropertyName]));
            }
        }
        #endregion

        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region GlobalSettings Property
        /// <summary>
        /// The XML name of the <see cref="GlobalSettings"/> property.
        /// </summary>
        internal const string GlobalSettingsPropertyName = "GlobalSettings";

        /// <summary>
        /// Gets or sets the GlobalSettings.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The GlobalSettings.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(PivotConfigurationSection.GlobalSettingsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public GlobalSettingsElement GlobalSettings
        {
            get
            {
                return ((GlobalSettingsElement)(base[PivotConfigurationSection.GlobalSettingsPropertyName]));
            }

            set
            {
                base[PivotConfigurationSection.GlobalSettingsPropertyName] = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// The GlobalSettingsElement Configuration Element.
    /// </summary>
    public class GlobalSettingsElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region DefaultDeepZoom Property

        /// <summary>
        /// The XML DefaultDeepZoom property.
        /// </summary>
        internal const string DefaultDeepZoomPropertyName = "DefaultDeepZoom";

        /// <summary>
        /// Gets or sets the WebCapture.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The DefaultDeepZoom Implementation.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(GlobalSettingsElement.DefaultDeepZoomPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public DefaultDeepZoomElement DefaultDeepZoom
        {
            get
            {
                return ((DefaultDeepZoomElement)(base[GlobalSettingsElement.DefaultDeepZoomPropertyName]));
            }

            set
            {
                base[GlobalSettingsElement.DefaultDeepZoomPropertyName] = value;
            }
        }

        #endregion

        #region WebCapture Property
        /// <summary>
        /// The XML web capture property.
        /// </summary>
        internal const string WebCapturePropertyName = "WebCapture";

        /// <summary>
        /// Gets or sets the WebCapture.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Web Capture Implementation.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(GlobalSettingsElement.WebCapturePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public WebCaptureElement WebCapture
        {
            get
            {
                return ((WebCaptureElement)(base[GlobalSettingsElement.WebCapturePropertyName]));
            }

            set
            {
                base[GlobalSettingsElement.WebCapturePropertyName] = value;
            }
        }

        #endregion

        #region OutputSettings Property
        /// <summary>
        /// The XML name of the <see cref="OutputSettings"/> property.
        /// </summary>
        internal const string OutputSettingsPropertyName = "OutputSettings";

        /// <summary>
        /// Gets or sets the OutputSettings.
        /// </summary>
        /// <value>The output settings.</value>
        [global::System.ComponentModel.DescriptionAttribute("The OutputSettings.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(GlobalSettingsElement.OutputSettingsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public OutputSettingsElement OutputSettings
        {
            get
            {
                return ((OutputSettingsElement)(base[GlobalSettingsElement.OutputSettingsPropertyName]));
            }

            set
            {
                base[GlobalSettingsElement.OutputSettingsPropertyName] = value;
            }
        }
        #endregion

        #region ThreadsPerCore Property
        /// <summary>
        /// The XML name property.
        /// </summary>
        internal const string ThreadsPerCorePropertyName = "threadsPerCore";

        /// <summary>
        /// Gets or sets the threads per core value.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Threads Per Core value.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(ThreadsPerCorePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = 10)]
        public int ThreadsPerCore
        {
            get { return ((int) (base[ThreadsPerCorePropertyName])); }
            set { base[ThreadsPerCorePropertyName] = value; }
        }

        #endregion
    }

    /// <summary>
    /// Generate Default Deep Zoom Element
    /// </summary>
    public class DefaultDeepZoomElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region TemplateLocation Property

        /// <summary>
        /// The XML templateLocation property.
        /// </summary>
        internal const string LocationPropertyName = "templateLocation";

        /// <summary>
        /// Gets or sets the TemplateLocation Property.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The GenerateDeepZoom TemplateLocation Property.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(DefaultDeepZoomElement.LocationPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false, DefaultValue="default")]
        public string TemplateLocation
        {
            get
            {
                return ((string)(base[DefaultDeepZoomElement.LocationPropertyName]));
            }

            set
            {
                base[DefaultDeepZoomElement.LocationPropertyName] = value;
            }
        }

        #endregion

        #region ImageLocation Property

        /// <summary>
        /// The XML imageLocation property.
        /// </summary>
        internal const string ImageLocationPropertyName = "imageLocation";

        /// <summary>
        /// Gets or sets the ImageLocation Property.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The GenerateDeepZoom ImageLocation Property.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(DefaultDeepZoomElement.ImageLocationPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "DefaultDeepZoom\\Default.dzi")]
        public string ImageLocation
        {
            get
            {
                return ((string) (base[DefaultDeepZoomElement.ImageLocationPropertyName]));
            }

            set
            {
                base[DefaultDeepZoomElement.ImageLocationPropertyName] = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// The WebCapture Configuration Element.
    /// </summary>
    public class WebCaptureElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region Type Property
        /// <summary>
        /// The XML type property.
        /// </summary>
        internal const string TypePropertyName = "type";

        /// <summary>
        /// Gets or sets the Output Type.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Web Capture Type.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputToElement.TypePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public string Type
        {
            get
            {
                return ((string)(base[WebCaptureElement.TypePropertyName]));
            }

            set
            {
                base[WebCaptureElement.TypePropertyName] = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// The OutputSettingsElement Configuration Element.
    /// </summary>
    public class OutputSettingsElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region OutputTo Property
        /// <summary>
        /// The XML name of the <see cref="OutputTo"/> property.
        /// </summary>
        internal const string OutputToPropertyName = "OutputTo";

        /// <summary>
        /// Gets or sets the OutputTo.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The OutputTo.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputSettingsElement.OutputToPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public OutputToElement OutputTo
        {
            get
            {
                return ((OutputToElement)(base[OutputSettingsElement.OutputToPropertyName]));
            }

            set
            {
                base[OutputSettingsElement.OutputToPropertyName] = value;
            }
        }
        #endregion

        #region UriFormat Property
        /// <summary>
        /// The XML name of the <see cref="UriFormat"/> property.
        /// </summary>
        internal const string UriFormatPropertyName = "UriFormat";

        /// <summary>
        /// Gets or sets the UriFormat.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The UriFormat.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputSettingsElement.UriFormatPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public UriFormatElement UriFormat
        {
            get
            {
                return ((UriFormatElement)(base[OutputSettingsElement.UriFormatPropertyName]));
            }

            set
            {
                base[OutputSettingsElement.UriFormatPropertyName] = value;
            }
        }
        #endregion

        #region BaseUri Property
        /// <summary>
        /// The XML name of the <see cref="BaseUri"/> property.
        /// </summary>
        internal const string BaseUriPropertyName = "BaseUri";

        /// <summary>
        /// Gets or sets the BaseUri.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The BaseUri.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputSettingsElement.BaseUriPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public BaseUriElement BaseUri
        {
            get
            {
                return ((BaseUriElement)(base[OutputSettingsElement.BaseUriPropertyName]));
            }

            set
            {
                base[OutputSettingsElement.BaseUriPropertyName] = value;
            }
        }
        #endregion

        #region OutputFolder Property
        /// <summary>
        /// The XML name of the <see cref="OutputFolder"/> property.
        /// </summary>
        internal const string OutputFolderPropertyName = "OutputFolder";

        /// <summary>
        /// Gets or sets the OutputFolder.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The OutputFolder.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputSettingsElement.OutputFolderPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public OutputFolderElement OutputFolder
        {
            get
            {
                return ((OutputFolderElement)(base[OutputSettingsElement.OutputFolderPropertyName]));
            }

            set
            {
                base[OutputSettingsElement.OutputFolderPropertyName] = value;
            }
        }
        #endregion

        #region SplitSize Property
        /// <summary>
        /// The XML split size property.
        /// </summary>
        internal const string SplitSizePropertyName = "SplitSize";

        /// <summary>
        /// Gets or sets the SplitSize.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Split Size Implementation.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputSettingsElement.SplitSizePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public SplitSize SplitSize
        {
            get
            {
                return ((SplitSize)(base[OutputSettingsElement.SplitSizePropertyName]));
            }

            set
            {
                base[OutputSettingsElement.SplitSizePropertyName] = value;
            }
        }

        #endregion

        #region GenerateReslatedCollections Property

        /// <summary>
        /// The XML GenerateReslatedCollections property.
        /// </summary>
        internal const string GenerateRelatedCollectionsPropertyName = "GenerateRelatedCollections";

        /// <summary>
        /// Gets or sets the SplitSize.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The GenerateReslatedCollections Implementation.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputSettingsElement.GenerateRelatedCollectionsPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public GenerateRelatedCollections GenerateRelatedCollections
        {
            get
            {
                return ((GenerateRelatedCollections)(base[OutputSettingsElement.GenerateRelatedCollectionsPropertyName]));
            }

            set
            {
                base[OutputSettingsElement.GenerateRelatedCollectionsPropertyName] = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// The SplitSize Configuration Element.
    /// </summary>
    public class SplitSize : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region Value Property
        /// <summary>
        /// The XML name property.
        /// </summary>
        internal const string ValuePropertyName = "value";

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Split Size value.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(SplitSize.ValuePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public int Value
        {
            get { return ((int)(base[SplitSize.ValuePropertyName])); }
            set { base[SplitSize.ValuePropertyName] = value; }
        }

        #endregion
    }

    /// <summary>
    /// The GenerateReslatedCollections Configuration Element.
    /// </summary>
    public class GenerateRelatedCollections : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region Value Property
        /// <summary>
        /// The XML name property.
        /// </summary>
        internal const string ValuePropertyName = "value";

        /// <summary>
        /// Gets or sets a value indicating whether the Value is true.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The boolean value.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(GenerateRelatedCollections.ValuePropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false)]
        public bool Value
        {
            get { return ((bool)(base[GenerateRelatedCollections.ValuePropertyName])); }
            set { base[GenerateRelatedCollections.ValuePropertyName] = value; }
        }

        #endregion
    }

    /// <summary>
    /// The OutputToElement Configuration Element.
    /// </summary>
    public class OutputToElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region Type Property
        /// <summary>
        /// The XML name of the <see cref="OutputType"/> property.
        /// </summary>
        internal const string TypePropertyName = "type";

        /// <summary>
        /// Gets or sets the Output Type.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Output Type.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputToElement.TypePropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public OutputType Type
        {
            get
            {
                return ((OutputType)(base[OutputToElement.TypePropertyName]));
            }

            set
            {
                base[OutputToElement.TypePropertyName] = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// The OutputFolderElement Configuration Element.
    /// </summary>
    public class OutputFolderElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region TemplateLocation Property
        /// <summary>
        /// The XML name of the <see cref="Location"/> property.
        /// </summary>
        internal const string LocationPropertyName = "location";

        /// <summary>
        /// Gets or sets the TemplateLocation.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The TemplateLocation.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(OutputFolderElement.LocationPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Location
        {
            get
            {
                return ((string)(base[OutputFolderElement.LocationPropertyName]));
            }

            set
            {
                base[OutputFolderElement.LocationPropertyName] = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Output Type Enum
    /// </summary>
    public enum OutputType
    {
        /// <summary>
        /// File System
        /// </summary>
        FileSystem,

        /// <summary>
        /// Zip
        /// </summary>
        Zip
    }

    /// <summary>
    /// Uri Format Enum
    /// </summary>
    public enum UriFormatType
    {
        /// <summary>
        /// Relative
        /// </summary>
        Relative,

        /// <summary>
        /// Absolute
        /// </summary>
        Absolute
    }

    /// <summary>
    /// The BaseUriElement Configuration Element.
    /// </summary>
    public class BaseUriElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region Uri Property
        /// <summary>
        /// The XML name of the <see cref="Uri"/> property.
        /// </summary>
        internal const string UriPropertyName = "uri";

        /// <summary>
        /// Gets or sets the Uri.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Uri.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(BaseUriElement.UriPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public string Uri
        {
            get
            {
                return ((string)(base[BaseUriElement.UriPropertyName]));
            }

            set
            {
                base[BaseUriElement.UriPropertyName] = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// The UriFormatElement Configuration Element.
    /// </summary>
    public class UriFormatElement : global::System.Configuration.ConfigurationElement
    {
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion

        #region Format Property
        /// <summary>
        /// The XML name of the <see cref="Format"/> property.
        /// </summary>
        internal const string FormatPropertyName = "format";

        /// <summary>
        /// Gets or sets the Format.
        /// </summary>
        [global::System.ComponentModel.DescriptionAttribute("The Format.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(UriFormatElement.FormatPropertyName, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public UriFormatType Format
        {
            get
            {
                return ((UriFormatType)(base[UriFormatElement.FormatPropertyName]));
            }

            set
            {
                base[UriFormatElement.FormatPropertyName] = value;
            }
        }
        #endregion
    }
}