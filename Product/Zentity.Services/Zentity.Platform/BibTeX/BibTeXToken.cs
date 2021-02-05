// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Represents a token in the BibTeX file
	/// </summary>
	internal class BibTeXToken
	{
		private string tokenValue;
		private int lineNumber;
		private int columnNumber;

		/// <summary>
		/// Initializes a new instance of the <see cref="BibTeXToken"/> class.
		/// </summary>
		public BibTeXToken()
		{
			this.tokenValue = string.Empty;
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value
		{
			get { return tokenValue; }
			set { this.tokenValue = value; }
		}

		/// <summary>
		/// Gets or sets the line number.
		/// </summary>
		/// <value>The line number.</value>
		public int LineNumber
		{
			get { return lineNumber; }
			set { lineNumber = value; }
		}

		/// <summary>
		/// Gets or sets the column number.
		/// </summary>
		/// <value>The column number.</value>
		public int ColumnNumber
		{
			get { return columnNumber; }
			set { columnNumber = value; }
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return ("(" + this.lineNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) +
				"," + this.columnNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) + "):" + this.tokenValue);
		}
	}
}
