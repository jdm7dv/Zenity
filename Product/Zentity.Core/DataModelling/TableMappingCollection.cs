// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Defines the TableMapping collection class.
    /// </summary>
    internal sealed class TableMappingCollection : ICollection<TableMapping>
    {
        Collection<TableMapping> innerMappings = new Collection<TableMapping>();

        /// <summary>
        /// Gets the <see cref="Zentity.Core.TableMapping"/> with the specified table name.
        /// </summary>
        /// <value>The table mapping instance</value>
        internal TableMapping this[string tableName, StringComparison comparisonType]
        {
            get
            {
                return this.innerMappings.Where(tuple => tuple.TableName.Equals(tableName,
                    comparisonType)).First();
            }
        }

        /// <summary>
        /// Loads the TableMappingCollection from XML.
        /// </summary>
        /// <param name="xDocMappings">The mappings xml.</param>
        internal void LoadFromXml(XmlDocument xDocMappings)
        {
            // TODO: Validate the xml against a schema.
            foreach (XmlNode xTable in xDocMappings.SelectNodes(DataModellingResources.XPathTable))
            {
                XmlElement eTable = xTable as XmlElement;
                TableMapping tableMapping = new TableMapping
                {
                    TableName =
                        eTable.Attributes[DataModellingResources.Name].Value
                };

                this.Add(tableMapping);

                foreach (XmlNode xColumn in eTable.SelectNodes(DataModellingResources.XPathRelativeColumn))
                {
                    XmlElement eColumn = xColumn as XmlElement;
                    ColumnMapping columnMapping = new ColumnMapping
                    {
                        ColumnName = eColumn.Attributes[DataModellingResources.Name].Value,
                        IsMapped = Convert.ToBoolean(Convert.ToInt32(
                        eColumn.Attributes[DataModellingResources.IsMapped].Value,
                        CultureInfo.InvariantCulture))
                    };

                    tableMapping.ColumnMappings.Add(columnMapping);

                    // Some of the columns are not mapped at all.
                    if (columnMapping.IsMapped)
                    {
                        columnMapping.PropertyId =
                            new Guid(eColumn.Attributes[DataModellingResources.PropertyId].Value);
                        columnMapping.IsScalarProperty = Convert.ToBoolean(Convert.ToInt32(
                            eColumn.Attributes[DataModellingResources.IsScalarProperty].Value,
                            CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>Table mapping collection</returns>
        internal TableMappingCollection Clone()
        {
            // Do a deep copy.
            TableMappingCollection newTableMappings = new TableMappingCollection();
            foreach (TableMapping sourceTableMapping in this)
            {
                TableMapping targetTableMapping = new TableMapping
                {
                    TableName = sourceTableMapping.TableName
                };
                newTableMappings.Add(targetTableMapping);

                foreach (ColumnMapping sourceColumnMapping in sourceTableMapping.ColumnMappings)
                {
                    ColumnMapping targetColumnMapping = new ColumnMapping
                    {
                        ColumnName = sourceColumnMapping.ColumnName,
                        IsMapped = sourceColumnMapping.IsMapped,
                        PropertyId = sourceColumnMapping.PropertyId,
                        IsScalarProperty = sourceColumnMapping.IsScalarProperty
                    };
                    targetTableMapping.ColumnMappings.Add(targetColumnMapping);
                }
            }

            return newTableMappings;
        }

        /// <summary>
        /// Returns the <see cref="Zentity.Core.ColumnMapping"/> object for the input property id.
        /// </summary>
        /// <param name="propertyId">Property Id.</param>
        /// <returns>ColumnMapping object.</returns>
        internal ColumnMapping GetColumnMappingByPropertyId(Guid propertyId)
        {
            List<ColumnMapping> validMappings = this.SelectMany(tuple => tuple.ColumnMappings).
                Where(tuple => tuple.IsMapped && tuple.PropertyId == propertyId).ToList();

            if (validMappings.Count() == 0)
                return null;

            if (validMappings.Count() > 1)
                throw new ZentityException(DataModellingResources.ExceptionMultipleColumnMappingsForProperty);

            return validMappings.First();
        }

        #region ICollection<TableMapping> Members

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(TableMapping item)
        {
            this.innerMappings.Add(item);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            this.innerMappings.Clear();
        }

        /// <summary>
        /// Determines whether the specified item is present in the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if the specified item is present in the collection; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TableMapping item)
        {
            // Perform a reference comparison here.
            return this.innerMappings.Contains(item);
        }

        /// <summary>
        /// Copies an array of items into the collection.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(TableMapping[] array, int arrayIndex)
        {
            this.innerMappings.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return this.innerMappings.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if successfull</returns>
        public bool Remove(TableMapping item)
        {
            // Perform reference comparison.
            return this.innerMappings.Remove(item);
        }

        #endregion

        #region IEnumerable<TableMapping> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The table mapping enumerator</returns>
        public IEnumerator<TableMapping> GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion
    }
}
