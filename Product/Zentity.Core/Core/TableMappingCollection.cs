// *********************************************************
// 
//     Copyright (c) Microsoft. All rights reserved.
//     This code is licensed under the Apache License, Version 2.0.
//     THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//     ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//     IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//     PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// 
// *********************************************************

using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;

namespace Zentity.Core
{
    internal sealed class TableMappingCollection : ICollection<TableMapping>
    {
        Collection<TableMapping> innerMappings = new Collection<TableMapping>();

        internal TableMapping this[string tableName, StringComparison comparisonType]
        {
            get
            {
                return this.innerMappings.Where(tuple => tuple.TableName.Equals(tableName,
                    comparisonType)).First();
            }
        }

        internal void LoadFromXml(XmlDocument xDocMappings)
        {
            // TODO: Validate the xml against a schema.
            foreach (XmlNode xTable in xDocMappings.SelectNodes(CoreResources.XPathTable))
            {
                XmlElement eTable = xTable as XmlElement;
                TableMapping tableMapping = new TableMapping
                {
                    TableName =
                        eTable.Attributes[CoreResources.Name].Value
                };

                this.Add(tableMapping);

                foreach (XmlNode xColumn in eTable.SelectNodes(CoreResources.XPathRelativeColumn))
                {
                    XmlElement eColumn = xColumn as XmlElement;
                    ColumnMapping columnMapping = new ColumnMapping
                    {
                        ColumnName = eColumn.Attributes[CoreResources.Name].Value,
                        IsMapped = Convert.ToBoolean(Convert.ToInt32(
                        eColumn.Attributes[CoreResources.IsMapped].Value,
                        CultureInfo.InvariantCulture))
                    };

                    tableMapping.ColumnMappings.Add(columnMapping);

                    // Some of the columns are not mapped at all.
                    if (columnMapping.IsMapped)
                    {
                        columnMapping.PropertyId =
                            new Guid(eColumn.Attributes[CoreResources.PropertyId].Value);
                        columnMapping.IsScalarProperty = Convert.ToBoolean(Convert.ToInt32(
                            eColumn.Attributes[CoreResources.IsScalarProperty].Value,
                            CultureInfo.InvariantCulture));
                    }
                }
            }
        }

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
                throw new ZentityException(CoreResources.ExceptionMultipleColumnMappingsForProperty);

            return validMappings.First();
        }

        #region ICollection<TableMapping> Members

        public void Add(TableMapping item)
        {
            this.innerMappings.Add(item);
        }

        public void Clear()
        {
            this.innerMappings.Clear();
        }

        public bool Contains(TableMapping item)
        {
            // Perform a reference comparison here.
            return this.innerMappings.Contains(item);
        }

        public void CopyTo(TableMapping[] array, int arrayIndex)
        {
            this.innerMappings.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this.innerMappings.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(TableMapping item)
        {
            // Perform reference comparison.
            return this.innerMappings.Remove(item);
        }

        #endregion

        #region IEnumerable<TableMapping> Members

        public IEnumerator<TableMapping> GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerMappings.GetEnumerator();
        }

        #endregion
    }
}
