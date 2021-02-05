// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;
    using System.Collections.Generic;

    public enum FacetType
    {
        Text,
        Number,
        DateTime,
        Link
    }

    public struct FacetHyperlink
    {
        public FacetHyperlink(string linkName, string linkUrl)
        {
            this.Name = linkName;
            this.Url = linkUrl;
        }

        public string Name;
        public string Url;
    }

    public class Facet
    {
        #region Constructors, Finalizer and Dispose

        /// <summary>
        /// Construct facet values for use with Collection.AddItem()
        /// </summary>
        public Facet(string category, FacetType facetType, params object[] tags)
        {
            this.Category = category;
            this.DataType = facetType;
            this.Tags = tags;
        }

        /// <summary>
        /// Construct text facet values for use with Collection.AddItem()
        /// </summary>
        public Facet(string category, params string[] tags)
        {
            Construct<string>(category, FacetType.Text, tags);
        }

        /// <summary>
        /// Construct numeric facet values for use with Collection.AddItem()
        /// </summary>
        public Facet(string category, params double[] tags)
        {
            Construct<double>(category, FacetType.Number, tags);
        }

        /// <summary>
        /// Construct date/time facet values for use with Collection.AddItem()
        /// </summary>
        public Facet(string category, params DateTime[] tags)
        {
            Construct<DateTime>(category, FacetType.DateTime, tags);
        }

        /// <summary>
        /// Construct hyperlink facet values for use with Collection.AddItem()
        /// </summary>
        public Facet(string category, params FacetHyperlink[] tags)
        {
            Construct<FacetHyperlink>(category, FacetType.Link, tags);
        }

        private void Construct<T>(string category, FacetType facetType, T[] tags)
        {
            this.Category = category;
            this.DataType = facetType;

            if (null != tags)
            {
                this.Tags = new object[tags.Length];
                for (int i = 0; i < tags.Length; ++i)
                {
                    this.Tags[i] = tags[i];
                }
            }
        }

        #endregion

        #region Public Properties

        public string Category { get; private set; }
        public FacetType DataType { get; private set; }
        public object[] Tags { get; private set; }
        public object Tag
        {
            get
            {
                if (null == this.Tags || 0 == this.Tags.Length)
                {
                    return null;
                }
                if (this.Tags.Length > 1)
                {
                    throw new ArgumentOutOfRangeException("Facet is not single-valued");
                }
                return this.Tags[0];
            }
            set
            {
                this.Tags = new object[] { value };
            }
        }

        internal bool IsTags
        {
            get
            {
                return (null != this.Tags)
                    && (this.Tags.Length > 0);
            }
        }

        #endregion

        #region Public Methods

        public static bool IsReservedCategory(string category)
        {
            return Collection.IsReservedCategoryName(category);
        }

        #endregion

        #region Internal Methods

        internal IEnumerable<object> EnumerateNonEmptyTags()
        {
            foreach (object o in this.Tags)
            {
                //Pivot expects facets to have a value, not be empty or null.
                if (null != o)
                {
                    if (this.DataType == FacetType.Text)
                    {
                        if (string.IsNullOrEmpty((string)o))
                        {
                            continue;
                        }
                    }

                    yield return o;
                }
            }
        }

        #endregion
    }
}
