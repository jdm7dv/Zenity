// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using Zentity.Rdf.Concepts;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// This class represents XSD datatypes.
    /// </summary>
    internal class XsdDataType
    {
        #region Variables
        RDFUriReference name;
        DataTypes baseType;
        int maxLength = -1;
        int precision;
        int scale;
        #endregion

        #region Constructors

        #region Internal

        /// <summary>
        /// Initializes a new instance of the XsdDataType class.
        /// </summary>
        internal XsdDataType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XsdDataType class with 
        /// a specified name Uri.
        /// </summary>
        /// <param name="nameUri">Name Uri.</param>
        internal XsdDataType(RDFUriReference nameUri)
        {
            name = nameUri;
        }

        #endregion

        #endregion

        #region Properties

        #region Internal

        /// <summary>
        /// Gets or sets Name Uri of datatype.
        /// </summary>
        internal RDFUriReference Name
        {
            get { return name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets core datatype mapped to this xsd datatype.
        /// </summary>
        internal DataTypes BaseType
        {
            get { return baseType; }
            set { this.baseType = value; }
        }

        /// <summary>
        /// Gets or sets maximum length.
        /// </summary>
        internal int MaxLength
        {
            get
            {
                if (this.baseType != DataTypes.Binary &&
                    this.baseType != DataTypes.String)
                {
                    this.maxLength = 0;
                }

                return this.maxLength;
            }
            set { this.maxLength = value; }
        }

        /// <summary>
        /// Gets or sets precision.
        /// </summary>
        internal int Precision
        {
            get
            {
                return GetPrecision(this.precision);
            }
            set
            {
                this.precision = GetPrecision(value);
            }
        }

        /// <summary>
        /// Gets or sets scale.
        /// </summary>
        internal int Scale
        {
            get { return GetScale(this.scale); }
            set
            {
                this.scale = GetScale(value);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the scale.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The scale value</returns>
        private int GetScale(int value)
        {
            if (this.baseType == DataTypes.Decimal)
            {
                if (value < 0 || value > this.precision || value > 26)
                {
                    value = this.precision;
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the precision.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The precision value</returns>
        private int GetPrecision(int value)
        {
            if (this.baseType == DataTypes.Decimal)
            {
                if (value < 1 || value > 38)
                {
                    value = 18;
                }
            }

            return value;
        }

        #endregion
    }
}
