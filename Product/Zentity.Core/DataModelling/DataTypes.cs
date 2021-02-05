// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Core
{
    /// <summary>
    /// Represents data types that can be used in entity framework csdl files.
    /// </summary>
    public enum DataTypes
    {
        /// <summary>
        /// Maps to SQL data type int.
        /// </summary>
        Int32,
        /// <summary>
        /// Maps to SQL data type varbinary.
        /// </summary>
        Binary,
        /// <summary>
        /// Maps to SQL data type bit.
        /// </summary>
        Boolean,
        /// <summary>
        /// Maps to SQL data type int.
        /// </summary>
        Byte,
        /// <summary>
        /// Maps to SQL data type datetime.
        /// </summary>
        DateTime,
        /// <summary>
        /// Maps to SQL data type decimal.
        /// </summary>
        Decimal,
        /// <summary>
        /// Maps to SQL data type float.
        /// </summary>
        Double,
        /// <summary>
        /// Maps to SQL data type uniqueidentifier.
        /// </summary>
        Guid,
        /// <summary>
        /// Maps to SQL data type real.
        /// </summary>
        Single,
        /// <summary>
        /// Maps to SQL data type smallint.
        /// </summary>
        Int16,
        /// <summary>
        /// Maps to SQL data type bigint.
        /// </summary>
        Int64,
        /// <summary>
        /// Maps to SQL data type nvarchar.
        /// </summary>
        String
    }
}
