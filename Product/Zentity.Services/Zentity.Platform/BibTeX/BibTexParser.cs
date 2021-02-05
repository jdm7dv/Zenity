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
	using System.Globalization;

	/// <summary>
	/// BibTex parser class.
	/// </summary>
	internal class BibTeXParser
	{
		private Dictionary<string, string> symbolTable;

		/// <summary>
		/// Initializes a new instance of the <see cref="BibTeXParser"/> class.
		/// </summary>
		/// <param name="symbolTable">The symbol table.</param>
		public BibTeXParser(Dictionary<string, string> symbolTable)
		{
			this.symbolTable = symbolTable;
		}

		/// <summary>
		/// Parses the set of tokens and returns a BibTeXEntry object. 
		/// </summary>
		/// <exception cref="BibTeXParserException"></exception>
		/// <param name="tokens">Set of tokens to be parsed</param>
		/// <returns>Returns parsed tokens are BibTeXEntry object</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public BibTeXEntry GetBibTeXEntry(ICollection<BibTeXToken> tokens)
		{
			//TODO: Need to reduce complexity of this function
			//
			// Algorithm to implement the logic for the parsing a set of tokens
			//
			// * Algorithm to parse a set of tokens
			// * 1. check for minimum no of tokens.
			//		Minimum tokens:
			//		@	proceedings	{	DBLP:conf/vldb/2005		,	}
			// * 2. Retrieve name and key from the list. Check for sync characters like '{' before and after ',' key.
			// * 3. Build all properties. Check for sync character also. Like '=', ',' 
			//
			// The function follows DFA (Deterministic Finite State Automate):
			//		It reads one value at a time and moves to expected state. In the new state if the current
			//		symbol is not expected then it throws error.

			List<BibTeXToken> allTokens = new List<BibTeXToken>(tokens);
			BibTeXEntry bibtex = new BibTeXEntry();

			#region Check for empty values, Minimum size

			// Check for length here... must be at least 4
			// Must be atleast 4 tokens. For Example: @  typeName  {  }
			if (allTokens.Count < 4)
			{
				throw new BibTeXParserException(Properties.Resources.BIBTEXPARSER_NOT_WELL_FORMED,
												allTokens[0].LineNumber, allTokens[0].ColumnNumber, allTokens[0].Value);
			}

			#endregion

			#region Retrieve Name and Key. Check for sync tokens also

			// First token must be '@'
			if (!AreEqual(allTokens[0].Value, BibTeXHelper.BT_ENTRY_AT))
			{
				throw new BibTeXParserException("'@' expected",
					allTokens[0].LineNumber, allTokens[0].ColumnNumber, allTokens[0].Value);
			}

			// Check for valid entry name
			if (!BibTeXHelper.IsValidEntyName(allTokens[1].Value))
			{
				throw new BibTeXParserException("Invalid entry name",
					allTokens[1].LineNumber, allTokens[1].ColumnNumber, allTokens[1].Value);
			}
			bibtex.Name = allTokens[1].Value;

			// third token must be start of the entry '{'
			if (!AreEqual(allTokens[2].Value, BibTeXHelper.BT_ENTRY_LEFT_BRACE) &&
				!AreEqual(allTokens[2].Value, BibTeXHelper.BT_ENTRY_LEFT_PARENTHESIS))
			{
				throw new BibTeXParserException("'{' or '(' expected",
					allTokens[2].LineNumber, allTokens[2].ColumnNumber, allTokens[2].Value);
			}

			// Check for valid key
			if (!BibTeXHelper.IsValidKey(allTokens[3].Value))
			{
				throw new BibTeXParserException("Invalid key",
					allTokens[3].LineNumber, allTokens[3].ColumnNumber, allTokens[3].Value);
			}
			bibtex.Key = allTokens[3].Value;

			// Last token must be '}' : TODO: Do this check at last.
			//if (!this.AreEqual(allTokens[allTokens.Count - 1].Value, BibTeXHelper.BT_ENTRY_RIGHT_BRACE))
			//{
			//    throw new BibTeXParserException("'}' expected",	allTokens[allTokens.Count - 1].LineNumber,
			//        allTokens[allTokens.Count - 1].ColumnNumber, allTokens[allTokens.Count - 1].Value);
			//}

			#endregion


			#region Check whether any key-value pair is present?
			if (allTokens.Count == 4)
			{
				if (AreEqual(allTokens[2].Value, BibTeXHelper.BT_ENTRY_LEFT_PARENTHESIS))
				{
					throw new BibTeXParserException("')' expected",
							allTokens[3].LineNumber, allTokens[3].ColumnNumber + allTokens[3].Value.Length, String.Empty);
				}
				else
				{
					throw new BibTeXParserException("'}' expected",
							allTokens[3].LineNumber, allTokens[3].ColumnNumber + allTokens[3].Value.Length, String.Empty);
				}
			}
			#endregion

			#region Process Properties. Operating liner way (one symbol at a time) allows easy error populating.

			List<BibTeXToken>.Enumerator enumerator = allTokens.GetEnumerator();
			// skip first four entries which are already processed (Name and Key).
			int i;
			for (i = 0; i < 4; i++)
			{
				enumerator.MoveNext();
			}
			i--;

			EntryStates nextExpectedSymbol = EntryStates.CommaSign;
			//ValueEncapsulator encapsulator = ValueEncapsulator.None;
			ComplexValueStates nextTokenComplexVlaue = ComplexValueStates.Exited;

			string key = string.Empty;
			string value = string.Empty;
			bool skipMoveNext = false;

			System.Text.RegularExpressions.Regex nonAlphaNumericPattern =
										new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]");

			// Format of a particular key-value pair 
			// ,	title	=	{	value	}
			while (true)
			{
				if (!skipMoveNext)
				{
					enumerator.MoveNext();
					//	break; // should not be called in any case,
					// as exiting from the loop is handled in following 'if'
					i++;
				}
				else
				{
					skipMoveNext = false;
				}

				// Following 'if' ensures that the loop will break, before enumerator.MoveNext() returns false
				if (i == allTokens.Count - 1)
				{
					if ((AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_RIGHT_BRACE) &&
						  AreEqual(allTokens[2].Value, BibTeXHelper.BT_ENTRY_LEFT_BRACE)
						  )
						||
						(AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_RIGHT_PARENTHESIS)) &&
						  AreEqual(allTokens[2].Value, BibTeXHelper.BT_ENTRY_LEFT_PARENTHESIS)
						)
					{
						// probably the last element so exist from the loop
						// but before exiting, is there any pending complexValue creation running ?
						if (nextTokenComplexVlaue == ComplexValueStates.ConcatenationOrComma && !String.IsNullOrEmpty(key))
						{
							bibtex.Properties[key] = value;
							nextExpectedSymbol = EntryStates.CommaSign; // graceful termination ;)
							break;
						}
						else if (nextTokenComplexVlaue == ComplexValueStates.Exited)
						{
							//nextExpectedSymbol = EntryStates.CommaSign; // graceful termination ;)
							break;
						}
						else
						{
							throw new BibTeXParserException(Properties.Resources.BIBTEXTPARSER_UNEXPECTED_TOKEN,
									enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}

					}
					else
					{
						if (AreEqual(allTokens[2].Value, BibTeXHelper.BT_ENTRY_LEFT_PARENTHESIS))
						{
							throw new BibTeXParserException("')' expected",
									enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}
						else
						{
							throw new BibTeXParserException("'}' expected",
									enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}
					}
				}
				switch (nextExpectedSymbol)
				{
					case EntryStates.CommaSign:
						if (!AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_COMMA))
						{
							throw new BibTeXParserException("',' expected",
								enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}
						nextExpectedSymbol = EntryStates.KeyName;
						break;
					case EntryStates.KeyName:
						if (!BibTeXHelper.IsValidPropertyName(enumerator.Current.Value))
						{
							throw new BibTeXParserException("Invalid property name",
								enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}
						// Key name is always converted to lower case. Hence,  'EDITOR', 'EDItor' and 'editor' are same.
						key = enumerator.Current.Value.ToLowerInvariant();
						nextExpectedSymbol = EntryStates.EqualSign;
						break;
					case EntryStates.EqualSign:
						if (!AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_EQUAL))
						{
							throw new BibTeXParserException("'=' expected",
								enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}
						nextExpectedSymbol = EntryStates.OpeningValue;
						break;

					case EntryStates.OpeningValue:
						if (AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_LEFT_BRACE))
						{
							//encapsulator = ValueEncapsulator.LeftBrace;
							nextExpectedSymbol = EntryStates.ValueSimple;
							//skipMoveNext = true;
						}
						else if (AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_QUOTATION))
						{
							//encapsulator = ValueEncapsulator.Quotation;
							nextExpectedSymbol = EntryStates.ValueComplex;
							skipMoveNext = true;
						}
						else if (this.symbolTable.ContainsKey(enumerator.Current.Value))
						{
							nextExpectedSymbol = EntryStates.ValueComplex;
							skipMoveNext = true; // skip as we need to process the token ;)
						}
						else if (!nonAlphaNumericPattern.IsMatch(enumerator.Current.Value))
						{
							// Hmm... the value is -
							//		1. not encapsulated with braces or quotation mark
							//		2. is not defined as symbol name
							//		3. and alphanumeric.
							// Hence, accept it as value itself.
							value = enumerator.Current.Value;
							bibtex.Properties[key] = value;
							key = string.Empty;
							value = string.Empty;
							nextExpectedSymbol = EntryStates.CommaSign;
						}
						else
						{
							throw new BibTeXParserException("'{' or '\"' or symbol name expected",
								enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}
						break;
					case EntryStates.ValueSimple:
						if (AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_RIGHT_BRACE))
						{	// if a pair is like -->  title={}  <<- the value must be empty
							value = string.Empty;
							skipMoveNext = true;
						}
						else
						{
							value = enumerator.Current.Value;
						}
						nextExpectedSymbol = EntryStates.ClosingValueBrace;
						break;
					case EntryStates.ValueComplex:
						#region Process Complex Vlaues: may be formed of symbol names, concatenation operatios, etc;

						if (nextTokenComplexVlaue == ComplexValueStates.Exited)
						{ nextTokenComplexVlaue = ComplexValueStates.Started; }

						switch (nextTokenComplexVlaue)
						{
							case ComplexValueStates.Started:
								// First symbol can  only be a variable name or quotation mark
								if (this.symbolTable.ContainsKey(enumerator.Current.Value))
								{
									value += this.symbolTable[enumerator.Current.Value];
									nextTokenComplexVlaue = ComplexValueStates.ConcatenationOrComma;
								}
								else if (AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_QUOTATION))
								{
									nextTokenComplexVlaue = ComplexValueStates.Value;
								}
								else
								{
									throw new BibTeXParserException("'\"' or symbol name expected",
										enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
								}
								break;
							case ComplexValueStates.VariableName:
								break;
							case ComplexValueStates.ConcatenationOrComma:
								if (AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_CONCATENATION))
								{
									// move back to started, i.e. it may be variableName or QUOTATION mark
									nextTokenComplexVlaue = ComplexValueStates.Started;
									skipMoveNext = false;
								}
								else if (AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_COMMA))
								{
									// if next is ',' then value is built
									//	  1. Save the key-value ; and reset key/value
									//	  2. exit from inner switch
									//	  3. move next in the outer switch,
									//	  4. skip the call to enumerator.MoveNext() as the comma is already fetched but yet to be processed

									bibtex.Properties[key] = value;
									key = value = string.Empty;

									nextTokenComplexVlaue = ComplexValueStates.Exited;
									nextExpectedSymbol = EntryStates.CommaSign;
									skipMoveNext = true;
								}
								else
								{
									throw new BibTeXParserException("',' expected.",
										enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
								}
								break;
							case ComplexValueStates.Concatenation:
								nextTokenComplexVlaue = ComplexValueStates.VariableName;
								break;
							case ComplexValueStates.OpeningQuotation:
								nextTokenComplexVlaue = ComplexValueStates.Value;
								break;
							case ComplexValueStates.Value:
								if (AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_QUOTATION))
								{   // if a pair is like -->  title=""  <<- the value must be empty
									value += string.Empty;
									skipMoveNext = true;
								}
								else
								{
									value += enumerator.Current.Value;
								}
								nextTokenComplexVlaue = ComplexValueStates.ClosingQuotation;

								break;
							case ComplexValueStates.ClosingQuotation:
								if (!AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_QUOTATION))
								{
									throw new BibTeXParserException("'}' expected",
										enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
								}
								nextTokenComplexVlaue = ComplexValueStates.ConcatenationOrComma;
								break;
							case ComplexValueStates.Exited:
								nextExpectedSymbol = EntryStates.CommaSign;
								break;
							default:
								// This case should not be called in any case
								throw new BibTeXParserException("Unexpected error");
						}
						#endregion
						break;

					case EntryStates.ClosingValueBrace:
						if (!AreEqual(enumerator.Current.Value, BibTeXHelper.BT_ENTRY_RIGHT_BRACE))
						{
							throw new BibTeXParserException("'}' expected",
								enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
						}
						// So the value is well encapsulated. Add it to the list. and clear key and value.
						bibtex.Properties[key] = value;
						key = string.Empty;
						value = string.Empty;

						//proceed to the next key-value pair.
						nextExpectedSymbol = EntryStates.CommaSign;
						break;

					default:
						// This case should not be called any how
						throw new BibTeXParserException("Unexpected error");
				}
			}

			if (nextExpectedSymbol != EntryStates.CommaSign && nextExpectedSymbol != EntryStates.KeyName)
			{
				throw new BibTeXParserException("Not well formed property value. Last symbol processed: " + enumerator.Current.Value, enumerator.Current.LineNumber, enumerator.Current.ColumnNumber, enumerator.Current.Value);
			}

			#endregion

			return bibtex;
		}

		/// <summary>
		/// Compares , ignores case, invariant culture
		/// </summary>
		/// <param name="s">First string.</param>
		/// <param name="t">Second string.</param>
		/// <returns>true if both strings are equal</returns>
		private static bool AreEqual(string s, string t)
		{
			return String.Compare(s, t, StringComparison.OrdinalIgnoreCase) == 0;
		}

		private enum EntryStates
		{
			CommaSign,
			KeyName,
			EqualSign,
			OpeningValue,
			ValueSimple,	// simple value encapsulated within {}
			ValueComplex,	// if quotation was there then # may appear 
			ClosingValueBrace,
		}

		private enum ComplexValueStates
		{
			Started,
			VariableName,
			Concatenation,	
			ConcatenationOrComma,	
			OpeningQuotation,
			Value,
			ClosingQuotation,
			Exited
		}

		private enum ValueEncapsulator
		{
			None,
			LeftBrace,
			RightBrace,
			Quotation
		}
	}
}
