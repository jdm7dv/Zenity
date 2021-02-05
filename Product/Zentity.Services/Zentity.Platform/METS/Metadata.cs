// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class which holds the metadata values.
    /// </summary>
    public class Metadata
    {
        #region Fields

        private ReadOnlyCollection<string> title;
        private ReadOnlyCollection<string> creator;
        private ReadOnlyCollection<string> subject;
        private ReadOnlyCollection<string> description;
        private ReadOnlyCollection<string> publisher;
        private ReadOnlyCollection<string> contributor;
        private ReadOnlyCollection<string> dateCreated;
        private ReadOnlyCollection<string> resourceType;
        private ReadOnlyCollection<string> format;
        private ReadOnlyCollection<string> identifier;
        private ReadOnlyCollection<string> source;
        private ReadOnlyCollection<string> language;
        private ReadOnlyCollection<string> relation;
        private ReadOnlyCollection<string> coverage;
        private ReadOnlyCollection<string> rights;

        #endregion

        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class.
        /// </summary>
        internal Metadata()
        {
        }

        #endregion

        #region Properties

        #region Dublin Core Metadata

        /// <summary>
        /// Gets the Rights information.
        /// </summary>
        public ReadOnlyCollection<string> Rights
        {
            get { return rights; }
            internal set { rights = value; }
        }

        /// <summary>
        /// Gets the Coverage information.
        /// </summary>
        public ReadOnlyCollection<string> Coverage
        {
            get { return coverage; }
            internal set { coverage = value; }
        }

        /// <summary>
        /// Gets the Relation information.
        /// </summary>
        public ReadOnlyCollection<string> Relation
        {
            get { return relation; }
            internal set { relation = value; }
        }

        /// <summary>
        /// Gets the Language of metadata.
        /// </summary>
        public ReadOnlyCollection<string> Language
        {
            get { return language; }
            internal set { language = value; }
        }

        /// <summary>
        /// Gets the Source information.
        /// </summary>
        public ReadOnlyCollection<string> Source
        {
            get { return source; }
            internal set { source = value; }
        }

        /// <summary>
        /// Gets the Identifier of the metadata.
        /// </summary>
        public ReadOnlyCollection<string> Identifier
        {
            get { return identifier; }
            internal set { identifier = value; }
        }

        /// <summary>
        /// Gets the Format of the metadata.
        /// </summary>
        public ReadOnlyCollection<string> Format
        {
            get { return format; }
            internal set { format = value; }
        }

        /// <summary>
        /// Gets the ResourceType information.
        /// </summary>
        public ReadOnlyCollection<string> ResourceType
        {
            get { return resourceType; }
            internal set { resourceType = value; }
        }

        /// <summary>
        /// Gets the DateCreated.
        /// </summary>
        public ReadOnlyCollection<string> DateCreated
        {
            get { return dateCreated; }
            internal set { dateCreated = value; }
        }

        /// <summary>
        /// Gets the Contributor information.
        /// </summary>
        public ReadOnlyCollection<string> Contributor
        {
            get
            {
                return contributor;
            }
            internal set { contributor = value; }
        }

        /// <summary>
        /// Gets the Publisher information.
        /// </summary>
        public ReadOnlyCollection<string> Publisher
        {
            get { return publisher; }
            internal set { publisher = value; }
        }

        /// <summary>
        /// Gets the Description.
        /// </summary>
        public ReadOnlyCollection<string> Description
        {
            get { return description; }
            internal set { description = value; }
        }

        /// <summary>
        /// Gets the Subject information.
        /// </summary>
        public ReadOnlyCollection<string> Subject
        {
            get { return subject; }
            internal set { subject = value; }
        }

        /// <summary>
        /// Gets the Creator information.
        /// </summary>
        public ReadOnlyCollection<string> Creator
        {
            get { return creator; }
            internal set { creator = value; }
        }

        /// <summary>
        /// Gets the Title of the metadata.
        /// </summary>
        public ReadOnlyCollection<string> Title
        {
            get { return title; }
            internal set { title = value; }
        }

        #endregion

        /// <summary>
        /// Get the metadata value for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property to get value.</param>
        /// <returns>Value of given property.</returns>
        public object this[string propertyName]
        {
            get
            {
                return GetPropertyValue(propertyName);
            }
            internal set
            {
                SetPropertyValue(propertyName, value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the value for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property to set the value.</param>
        /// <param name="value">Value for the property to set.</param>
        private void SetPropertyValue(string propertyName, object value)
        {
            PropertyInfo property = this.GetType().GetProperties()
                                        .FirstOrDefault(pr => MetsConstants.Item != pr.Name && pr.Name.ToUpperInvariant() == propertyName.ToUpperInvariant());
            if (null != property)
            {
                property.SetValue(this, value, null);
            }
        }

        /// <summary>
        /// Get the value for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property to get the value.</param>
        /// <returns>Value for the given property.</returns>
        private object GetPropertyValue(string propertyName)
        {
            PropertyInfo property = this.GetType().GetProperties()
                                        .FirstOrDefault(pr => MetsConstants.Item != pr.Name && pr.Name == propertyName);
            if (null == property)
            {
                return null;
            }
            return property.GetValue(this, null);
        }

        #endregion
    }
}
