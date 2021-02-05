// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Zentity.DeepZoom
{
    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/deepzoom/2009")]
    [XmlRoot(Namespace = "http://schemas.microsoft.com/deepzoom/2009", IsNullable = false)]
    public class Image
    {
        private ImageDisplayRect[] displayRectsField;

        private string formatField;
        private ulong overlapField;
        private Uint32Size sizeField;
        private ulong tileSizeField;

        /// <remarks/>
        public Uint32Size Size
        {
            get { return sizeField; }
            set { sizeField = value; }
        }

        /// <remarks/>
        [XmlArrayItem("DisplayRect", IsNullable = false)]
        public ImageDisplayRect[] DisplayRects
        {
            get { return displayRectsField; }
            set { displayRectsField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong TileSize
        {
            get { return tileSizeField; }
            set { tileSizeField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong Overlap
        {
            get { return overlapField; }
            set { overlapField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string Format
        {
            get { return formatField; }
            set { formatField = value; }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://schemas.microsoft.com/deepzoom/2009")]
    public class Uint32Size
    {
        private ulong heightField;
        private ulong widthField;

        /// <remarks/>
        [XmlAttribute]
        public ulong Width
        {
            get { return widthField; }
            set { widthField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong Height
        {
            get { return heightField; }
            set { heightField = value; }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://schemas.microsoft.com/deepzoom/2009")]
    public class Uint32Rect
    {
        private ulong heightField;
        private ulong widthField;
        private ulong xField;

        private ulong yField;

        /// <remarks/>
        [XmlAttribute]
        public ulong X
        {
            get { return xField; }
            set { xField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong Y
        {
            get { return yField; }
            set { yField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong Width
        {
            get { return widthField; }
            set { widthField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong Height
        {
            get { return heightField; }
            set { heightField = value; }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/deepzoom/2009")]
    public class ImageDisplayRect
    {
        private ulong maxLevelField;
        private ulong minLevelField;
        private Uint32Rect rectField;

        /// <remarks/>
        public Uint32Rect Rect
        {
            get { return rectField; }
            set { rectField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong MinLevel
        {
            get { return minLevelField; }
            set { minLevelField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong MaxLevel
        {
            get { return maxLevelField; }
            set { maxLevelField = value; }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/deepzoom/2009")]
    [XmlRoot(Namespace = "http://schemas.microsoft.com/deepzoom/2009", IsNullable = false)]
    public class Collection
    {
        private string formatField;
        private CollectionI[] itemsField;

        private byte maxLevelField;

        private ulong nextItemIdField;
        private float qualityField;
        private ulong tileSizeField;

        public Collection()
        {
            qualityField = ((1F));
        }

        /// <remarks/>
        [XmlArrayItem("I", IsNullable = false)]
        public CollectionI[] Items
        {
            get { return itemsField; }
            set { itemsField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte MaxLevel
        {
            get { return maxLevelField; }
            set { maxLevelField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong TileSize
        {
            get { return tileSizeField; }
            set { tileSizeField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string Format
        {
            get { return formatField; }
            set { formatField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        [DefaultValue(typeof (float), "1")]
        public float Quality
        {
            get { return qualityField; }
            set { qualityField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong NextItemId
        {
            get { return nextItemIdField; }
            set { nextItemIdField = value; }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/deepzoom/2009")]
    public class CollectionI
    {
        private ulong idField;

        private bool isPathField;
        private ulong nField;
        private Uint32Size sizeField;
        private string sourceField;

        private string typeField;
        private CollectionIViewport viewportField;

        public CollectionI()
        {
            isPathField = true;
            typeField = "ImagePixelSource";
        }

        /// <remarks/>
        public Uint32Size Size
        {
            get { return sizeField; }
            set { sizeField = value; }
        }

        /// <remarks/>
        public CollectionIViewport Viewport
        {
            get { return viewportField; }
            set { viewportField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong N
        {
            get { return nField; }
            set { nField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public ulong Id
        {
            get { return idField; }
            set { idField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string Source
        {
            get { return sourceField; }
            set { sourceField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        [DefaultValue(true)]
        public bool IsPath
        {
            get { return isPathField; }
            set { isPathField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        [DefaultValue("ImagePixelSource")]
        public string Type
        {
            get { return typeField; }
            set { typeField = value; }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/deepzoom/2009")]
    public class CollectionIViewport
    {
        private double widthField;

        private double xField;

        private double yField;

        /// <remarks/>
        [XmlAttribute]
        public double Width
        {
            get { return widthField; }
            set { widthField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public double X
        {
            get { return xField; }
            set { xField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public double Y
        {
            get { return yField; }
            set { yField = value; }
        }
    }
}