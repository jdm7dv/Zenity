// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Security.Permissions;

    /// <summary>
    /// Represents a parser exception. This exception class describes where the parser error occurred giving 
	/// line and column number in the file.
    /// </summary>  
    [Serializable]
	public class BibTeXParserException : Exception
	{
		private int line;
		private int column;
		private string errorToken = string.Empty;

		/// <summary>
		/// Gets the line number at which error token is present. 
        /// One based index value.
		/// </summary>
		public int Line
		{
			get { return line; }
		}

		/// <summary>
		/// Gets the starting column index of the error token in the specified line. 
        /// One based index value.
		/// </summary>
		public int Column
		{
			get 
            { 
                return column; 
            }
		}

        /// <summary>
        /// Gets the token value which caused error.
        /// </summary>
		public string ErrorToken
		{
			get
			{ 
                return this.errorToken;	
            }
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXParserException"/> class.
        /// </summary>
		public BibTeXParserException()
			: base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
		public BibTeXParserException(String message)
			: base(message)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXParserException"/> class.
        /// </summary>
		/// <param name="message">Message describing error behavior.</param>
        /// <param name="inner">The System.Exception class that represent the exception details.</param>
		public BibTeXParserException(String message, Exception inner)
			: base(message, inner)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXParserException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected BibTeXParserException(global::System.Runtime.Serialization.SerializationInfo info,
									    global::System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			this.column = info.GetInt32("Column");
			this.line = info.GetInt32("Line");
			this.errorToken = info.GetString("ErrorToken");
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXParserException"/> class.
        /// </summary>
		/// <param name="message">Message describing error behavior.</param>
        /// <param name="line">Line number of the error token.</param>
        /// <param name="column">Column number of the error token.</param>
        /// <param name="errorToken">Value of the error token.</param>
		public BibTeXParserException(String message, int line, int column, string errorToken)
			: base(message)
		{
			this.line = line;
			this.column = column;
			this.errorToken = errorToken;
		}

        /// <summary>
		/// Creates and returns a string representation of the current exception.
        /// </summary>
		/// <returns>Returns a string representation of the exception.</returns> 
		public override string  ToString()
		{
			if (this.line == 0 || this.column == 0)
			{
				return this.Message ;
			}
			else if( String.IsNullOrEmpty( this.errorToken ) )
			{
                return (this.Message + " (" + this.line.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + this.column.ToString(System.Globalization.CultureInfo.InvariantCulture) + ") ");
			}
			else
			{
                return (this.Message + " (" + this.line.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + this.column.ToString(System.Globalization.CultureInfo.InvariantCulture) + ") : " + this.errorToken);
			}
		}

		/// <summary>
		/// When overridden in a derived class, sets the System.Runtime.Serialization.SerializationInfo
		/// with information about the exception.
		/// </summary>
		/// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized 
		/// object data about the exception being thrown.
		/// </param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual
		/// information about the source or destination.
		/// </param>
		/// <exception cref="System.ArgumentNullException">The info parameter is a null reference (Nothing in Visual Basic).</exception>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Column", this.column);
			info.AddValue("Line", this.line);
			info.AddValue("ErrorToken", this.errorToken);
		}
     
	}

}
