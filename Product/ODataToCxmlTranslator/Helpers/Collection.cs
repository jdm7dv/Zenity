// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;    

    public class Collection
    {
        #region Constructors, Finalizer and Dispose

        public Collection()
        {
            this.SchemaVersion = 1.0;
            this.EnableInfoPaneBingSearch = true;
        }

        #endregion

        #region Public Properties

        public string Name { get; set; }
        public Uri IconUrl { get; set; }
        public CultureInfo Culture { get; set; }
        public string ImgBaseName { get; set; }
        public string HrefBase { get; set; }

        public bool EnableInfoPaneBingSearch { get; set; }

        #endregion

        #region Internal Properties

        internal double SchemaVersion { get; set; }

        internal IList<CollectionItem> Items
        {
            get { return m_items; }
        }

        internal IEnumerable<FacetCategory> FacetCategories
        {
            get
            {
                foreach (var entry in m_facetCategories)
                {
                    yield return entry;
                }
            }
        }

        internal bool HasFacets
        {
            get { return m_facetCategories.Count > 0;  }
        }

        #endregion

        #region Public Methods

        public void AddItem(string name, string url, string description, ItemImage image, params Facet[] facets)
        {
            CollectionItem item = new CollectionItem() { Name = name, Url = url, Description = description };
            item.SetImage(image);

            if (null != facets)
            {
                EnsureFacetCategories(facets);
                item.FacetValues = facets;
            }
            m_items.Add(item);
        }
        
        public void AddItem(string id, string name, string url, string description, ItemImage image, params Facet[] facets)
        {
            CollectionItem item = new CollectionItem() { Id=id, Name = name, Url = url, Description = description };
            item.SetImage(image);

            if (null != facets)
            {
                EnsureFacetCategories(facets);
                item.FacetValues = facets;
            }
            m_items.Add(item);
        }

        public void SetFacetDisplay(string category, bool showInFacetPane, bool showInInfoPane, bool isTextFilter)
        {
            FacetCategory cat = GetFacetCategory(category);
            cat.IsShowInFacetPane = showInFacetPane;
            cat.IsShowInInfoPane = showInInfoPane;
            cat.IsTextFilter = isTextFilter;
        }

        /// <summary>
        /// Set a format string for display of a numeric or datetime facet.
        /// Setting a format string for other facet types has no effect.
        /// Custom number formatting: http://msdn.microsoft.com/en-us/library/0c899ak8.aspx
        /// DateTime formatting: http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx
        /// </summary>
        public void SetFacetFormat(string category, string format)
        {
            FacetCategory cat = GetFacetCategory(category);
            cat.DisplayFormat = format;
        }

        public string ToCxml()
        {
            return CxmlSerializer.Serialize(this);
        }

        public void ToCxml(TextWriter textWriter)
        {
            CxmlSerializer.Serialize(textWriter, this);
        }

        public void ToCxml(string filePath)
        {
            using (StreamWriter writer = File.CreateText(filePath))
            {
                CxmlSerializer.Serialize(writer.BaseStream, this);
            }
        }

        public void ToDzc(TextWriter textWriter)
        {
            DzcSerializer.Serialize(textWriter, this);
        }

        #endregion

        #region Private Methods

        private FacetCategory GetFacetCategory(string category)
        {
            FacetCategory facetCategory = m_facetCategories.TryGet(category);
            if (null == facetCategory)
            {
                throw new ArgumentException("Facet category \"" + category + "\" has not been added.");
            }
            return facetCategory;
        }

        private FacetCategory MakeFacetCategory(string category, FacetType facetType)
        {
            ThrowIfReservedCategoryName(category);

            FacetCategory facetCategory = m_facetCategories.TryGet(category);
            if (null != facetCategory)
            {
                if (facetCategory.FacetType != facetType)
                {
                    throw new ArgumentException(
                        string.Format("Facet category \"{0}\" already has type \"{1}\", which does not match the requested type \"{2}\".",
                            facetCategory.Name, facetCategory.FacetType, facetType));
                }
            }
            else
            {
                facetCategory = new FacetCategory(category, facetType);
                m_facetCategories.Add(facetCategory);
            }
            return facetCategory;
        }

        internal static bool IsReservedCategoryName(string name)
        {
            foreach (string s in reservedCategoryNames_c)
            {
                if (0 == string.Compare(s, name, true))
                {
                    return true;
                }
            }
            return false;
        }

        private static void ThrowIfReservedCategoryName(string name)
        {
            if(IsReservedCategoryName(name))
            {
                throw new ArgumentException(
                    string.Format("The facet category \"{0}\" is reserved and may not be used", name));
            }
        }

        private void EnsureFacetCategories(IEnumerable<Facet> facets)
        {
            foreach (Facet f in facets)
            {
                if (null == f.Category)
                {
                    throw new ArgumentNullException("Facet.Category");
                }

                MakeFacetCategory(f.Category, f.DataType);
            }
        }

        #endregion

        #region Private Fields

        class FacetCategoryCollection : KeyedCollection<string, FacetCategory>
        {
            public FacetCategory TryGet(string key)
            {
                return Contains(key) ? this[key] : null;
            }

            protected override string GetKeyForItem(FacetCategory item)
            {
                return item.Name;
            }
        }

        static readonly string[] reservedCategoryNames_c = { "Name", "Description" };

        List<CollectionItem> m_items = new List<CollectionItem>();
        FacetCategoryCollection m_facetCategories = new FacetCategoryCollection();

        #endregion
    }

}
