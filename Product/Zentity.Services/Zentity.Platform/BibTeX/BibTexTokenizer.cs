// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// BibTex Tokenizer class.
    /// </summary>
    internal class BibTeXTokenizer : IDisposable
	{
		#region Private Member

		private System.IO.BinaryReader tokenStream;
		private bool lastTokenEncountered;
		private string prevToken = String.Empty;
		private string prevToPrevToken = String.Empty;
		private int currentTokenLineNumber = 1;
		private int currentTokenColumnNumber;
		private Regex entityRegEx;
		private Regex fieldRegEx;
		private bool firstTime;
		private char nextCharacter;
		private char prevCharacter;

		#endregion

		#region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXTokenizer"/> class.
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
		public BibTeXTokenizer(Stream tokenStream)
		{
			this.tokenStream = new BinaryReader(tokenStream);
			this.entityRegEx = new Regex("[^@(){},\"=#]", RegexOptions.IgnoreCase);
			this.fieldRegEx = new Regex("[^{}\"]", RegexOptions.IgnoreCase);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether the last token 
        /// has been encountered in the BibTeX stream.
		/// </summary>
		public bool LastTokenEncountered
		{
			get
			{
				return this.lastTokenEncountered;
			}
		}

		#endregion

		#region Public Method

		/// <summary>
		/// Create next token from BibTeX stream 
		/// </summary>
		/// <returns>Return instance of BibTeXToken class</returns> 
		public BibTeXToken GetNextToken()
		{
			BibTeXToken bibToken = new BibTeXToken();
			StringBuilder token = new StringBuilder();
			string tokenInput = string.Empty;
			bibToken.LineNumber = this.currentTokenLineNumber;
			bibToken.ColumnNumber = this.currentTokenColumnNumber + 1;
			try
			{
				// If below condition met then field value is occurred  in the BibTeX stream
				// If below condition met then field value is occurred  in the BibTeX stream
				if ((this.prevToPrevToken.Equals(BibTeXHelper.BT_ENTRY_EQUAL) &&
							(this.prevToken.Equals(BibTeXHelper.BT_ENTRY_LEFT_BRACE) ||
							 this.prevToken.Equals(BibTeXHelper.BT_ENTRY_QUOTATION)))
					||
					  (this.prevToPrevToken.Equals(BibTeXHelper.BT_ENTRY_CONCATENATION) &&
										 this.prevToken.Equals(BibTeXHelper.BT_ENTRY_QUOTATION))
					)
				{
					List<string> fieldValue = this.ProcessFieldValue();
					foreach (string fieldData in fieldValue)
					{
						token.Append(fieldData.Trim());

					}
					bibToken.Value = token.ToString();
				}
				else
				{
					if (!this.lastTokenEncountered)
					{
						tokenInput = this.GetToken(this.entityRegEx);
						bibToken.Value = tokenInput;
					}
				}
			}
			catch (BibTeXParserException ex)
			{
				// This can happen only when the token has un balanced parenthesis.
				// Here we update the line and column numbers only
				bibToken.Value = ex.ErrorToken;
				FormatToken(bibToken);
				throw new BibTeXParserException(ex.Message, bibToken.LineNumber,
													bibToken.ColumnNumber, bibToken.Value);
			}
			// TOD0: check it update passing parameter. Is it require to return BibTeXToken by FormatToken method
			FormatToken(bibToken);
			return bibToken;
		}

		#endregion

		#region Private Methods

        /// <summary>
        /// Format value,line and column number of BibTeXToken object
        /// </summary>
        /// <param name="bibToken">The bib token.</param>
		private static void FormatToken(BibTeXToken bibToken)
		{
			string tokenWithoutNewLine = string.Empty;
			int indexOfNewLineInToken = 0;
			if (bibToken.Value.Replace(Properties.Resources.SPACE,
											string.Empty).Replace(BibTeXHelper.BT_ENTRY_TAB,
														 string.Empty).StartsWith(System.Environment.NewLine,
																	   StringComparison.OrdinalIgnoreCase))
			{
				// Update column number of token if token's value start with the new line
				bibToken.ColumnNumber = 1;

				// Get index of first character in trimmed token value
				int indexOfFirstCharacter = bibToken.Value.Length - bibToken.Value.TrimStart().Length;

				// Get last index of new line before "indexOfFirstCharacter" inside the token value
				indexOfNewLineInToken = bibToken.Value.LastIndexOf(System.Environment.NewLine,
											 indexOfFirstCharacter, indexOfFirstCharacter + 1,
											 StringComparison.OrdinalIgnoreCase);
				tokenWithoutNewLine = bibToken.Value.Substring((indexOfNewLineInToken) + 2);
			}
			else
			{
				tokenWithoutNewLine = bibToken.Value.Substring(indexOfNewLineInToken);
			}
			int tokenColumnNumber = bibToken.ColumnNumber + tokenWithoutNewLine.Length
												- tokenWithoutNewLine.TrimStart().Length;
			bibToken.ColumnNumber = tokenColumnNumber;
			int numberOfNewLineInToken = 0;
			int indexOfNewLine = -1;
			StringBuilder formatedInput = new StringBuilder();
			bibToken.Value = bibToken.Value.TrimEnd();
			string bibTexTokenValue = bibToken.Value;
			while (bibToken.Value.Contains(System.Environment.NewLine))
			{
				if (bibTexTokenValue.Replace(Properties.Resources.SPACE,
										   string.Empty).Replace(BibTeXHelper.BT_ENTRY_TAB,
														 string.Empty).StartsWith(System.Environment.NewLine, StringComparison.OrdinalIgnoreCase))
				{
					numberOfNewLineInToken++;
					bibTexTokenValue = bibTexTokenValue.Substring(bibTexTokenValue.IndexOf
																(System.Environment.NewLine, StringComparison.OrdinalIgnoreCase) + 2);
				}
				indexOfNewLine = bibToken.Value.IndexOf(System.Environment.NewLine, StringComparison.OrdinalIgnoreCase);
				if (formatedInput.Length != 0)
				{
					formatedInput.Append(Properties.Resources.SPACE);
				}
				if (indexOfNewLine >= 0)
				{
					formatedInput.Append(bibToken.Value.Substring(0, indexOfNewLine).Trim());
				}
				bibToken.Value = bibToken.Value.Substring(bibToken.Value.IndexOf
														(BibTeXHelper.BT_CHAR_ENTRY_NEWLINE) + 1);
			}
			bibToken.LineNumber = bibToken.LineNumber + numberOfNewLineInToken;
			formatedInput.Append(Properties.Resources.SPACE + bibToken.Value.Trim());
			bibToken.Value = formatedInput.ToString().Trim();
			//return bibToken;
		}

		/// <summary>
		/// Process field value inside the BibTeX stream
		/// </summary>
		/// <returns>Returns separated list of string for field value on basis of regular 
		/// expression([^{}\"])</returns>
		private List<string> ProcessFieldValue()
		{
			List<string> fieldValue = new List<string>();

			// Check field value start with the left parenthesis.
			if (this.prevToken.Equals(BibTeXHelper.BT_ENTRY_LEFT_BRACE))
			{
				fieldValue = this.SeparateTokenByParenthesis(false);
			}
			else
			{
				fieldValue = this.SeparateTokenByQuotes();
			}
			return fieldValue;
		}

        /// <summary>
        /// Process field value inside the parenthesis.
        /// </summary>
        /// <param name="isCallFromSeparateByQuote">if set to <c>true</c> indicates that the 
        /// call from is seperated by quotes.</param>
        /// <returns>
        /// Returns separated list of string for field value on basis of regular 
        /// expression([^{}\"]) inside the parenthesis.
        /// </returns>
		private List<string> SeparateTokenByParenthesis(bool isCallFromSeparateByQuote)
		{
			List<string> fieldValue = new List<string>();
			int parenthesisCount = 1;

			// Check matching right parenthesis occurred  in field value for left parenthesis
			while (parenthesisCount != 0 && !this.lastTokenEncountered)
			{
				// Decrement parenthesisCount if right parenthesis occurred
				if (this.nextCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_RIGHT_BRACE))
				{
					// Handle field value like "No space between parenthesis"
					/*
					@book{journals/aim/,
					title = {},
					}
					*/
					bool isRightNexToLeft = this.prevCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_LEFT_BRACE)
										  && this.nextCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_RIGHT_BRACE);
					if (isCallFromSeparateByQuote)
					{
						string partOfFieldValue = this.GetToken(this.fieldRegEx);
						fieldValue.Add(partOfFieldValue);
					}
					else
					{
						if (parenthesisCount != 1 || isRightNexToLeft)
						{
							string partOfFieldValue = this.GetToken(this.fieldRegEx);
							fieldValue.Add(partOfFieldValue);
						}
					}
					parenthesisCount--;
				}

				// Increment parenthesisCount if left parenthesis occurred.
				else if (this.nextCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_LEFT_BRACE))
				{
					string partOfFieldValue = this.GetToken(this.fieldRegEx);
					fieldValue.Add(partOfFieldValue);
					parenthesisCount++;
				}
				else
				{
					string partOfFieldValue = this.GetToken(this.fieldRegEx);
					fieldValue.Add(partOfFieldValue);
				}
			}
			if (parenthesisCount != 0)
			{
				throw new BibTeXParserException(Properties.Resources.PARENTHESIS_IS_NOT_MATCH, 0, 0,
					fieldValue.Count > 0 ? fieldValue[0] : string.Empty);
			}
			return fieldValue;
		}

		/// <summary>
		/// Process field value inside the quotes
		/// </summary>
		/// <exception cref="BibTeXParserException"></exception>
		/// <returns>Returns separated list of string for field value on basis of regular 
		/// expression([^{}\"]) inside the quotes</returns>
		private List<string> SeparateTokenByQuotes()
		{
			bool firstTimeQuotesEncountered = true;
			List<string> fieldValue = new List<string>();

			// Check ending quote occurred for field value.
			while (firstTimeQuotesEncountered && !this.lastTokenEncountered)
			{
				// If below condition is true then ending quote for field value is encountered
				if (this.nextCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_QUOTATION))
				{
					// If below condition is true then ending quote for field value is encountered 
					if (this.prevCharacter.Equals(this.nextCharacter))
					{
						string partOfFieldValue = this.GetToken(this.fieldRegEx);
						fieldValue.Add(partOfFieldValue);
					}
					firstTimeQuotesEncountered = !firstTimeQuotesEncountered;
				}
				else if (this.nextCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_RIGHT_BRACE))
				{
					throw new BibTeXParserException(Properties.Resources.UNBALANCED_TOKEN, 0, 0,
								fieldValue.Count > 0 ? fieldValue[0] : string.Empty);
				}
				else
				{
					string partOfFieldValue = this.GetToken(this.fieldRegEx);
					fieldValue.Add(partOfFieldValue);
					if (partOfFieldValue.Equals((BibTeXHelper.BT_ENTRY_LEFT_BRACE)))
					{
						List<string> temp = this.SeparateTokenByParenthesis(true);
						fieldValue.AddRange(temp);
					}
				}
			}
			if (firstTimeQuotesEncountered)
			{
				throw new BibTeXParserException(Properties.Resources.QUOTE_IS_NOT_MATCH, 0, 0,
					fieldValue.Count > 0 ? fieldValue[0] : string.Empty);
			}
			return fieldValue;
		}

		/// <summary>
		/// Get value of the token from BibTeX stream by regular expression
		/// </summary>
		/// <exception cref="EndOfStreamException"></exception>
		/// <param name="seprators">Instance of Regex provides separators to get token</param>
		/// <returns>Return value of token</returns> 
		private string GetToken(Regex seprators)
		{
			StringBuilder token = new StringBuilder();
			try
			{
				//  this._nextCharacter is empty if condition is true.
				if (!this.firstTime)
				{
					this.nextCharacter = (char)this.tokenStream.ReadByte();
					this.firstTime = true;
				}
				// if true then separator is occurred in the BibTeX stream.
				if (!seprators.IsMatch(this.nextCharacter.ToString()))
				{
					token.Append(this.nextCharacter);
					this.ReadNextChar();
				}
				else
				{
					// Read character till separator is not occurred then create token value from read 
					// character
					while (seprators.IsMatch(this.nextCharacter.ToString()))
					{
						token.Append(this.nextCharacter);
						this.ReadNextChar();
					}
					// Ignore empty token if token is not the part of the field value
					if (token.ToString().Trim().Length == 0 && !seprators.Equals(this.fieldRegEx))
					{
						token.Append(this.nextCharacter);
						this.ReadNextChar();
					}
				}
			}
			catch (EndOfStreamException)
			{
				this.lastTokenEncountered = true;
			}
			this.prevToPrevToken = this.prevToken;
			this.prevToken = token.ToString().Trim();
			return token.ToString();
		}

		/// <summary>
		/// Read next character from BibTex stream and update line and column number
		/// </summary>
		private void ReadNextChar()
		{
			this.prevCharacter = this.nextCharacter;
			this.nextCharacter = (char)this.tokenStream.ReadByte();
			if (this.prevCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_CARRIAGERETURN)
						 && this.nextCharacter.Equals(BibTeXHelper.BT_CHAR_ENTRY_NEWLINE))
			{
				this.currentTokenLineNumber++;
				this.currentTokenColumnNumber = -1;
			}
			else
			{
				this.currentTokenColumnNumber++;
			}
		}

		#endregion

		#region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
		public void Dispose()
		{
			this.tokenStream.Close();
			GC.SuppressFinalize(this);
		}

		#endregion
	}

}
