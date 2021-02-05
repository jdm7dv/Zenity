// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Zentity.Core;
using System.Collections;
using System.Globalization;

namespace Zentity.Platform
{
	/// <summary>
	/// Represents parsing behavior of BibTeX parser
	/// </summary>
	public enum BibTeXParserBehavior
	{
		/// <summary>
		/// Stop parsing BibTeX file when first error is encountered.
		/// </summary>
		StopOnFirstError,

		/// <summary>
		/// Try to parse the BibTeX file completely even if the file contains invalid entries.
		/// </summary> 
		IgnoreParseErrors
	}

	/// <summary>
	/// Represents an in-memory BibTeX file. 
	/// It support add, remove or insert any number of BibTeXEntry objects
	/// </summary> 
	[Serializable]
	public class BibTeXDocument
	{

		#region Private Members

		private Dictionary<string, string> _symbolTable = null;
		private ICollection<BibTeXParserException> _parserExceptions = null;
		private BibTeXParserBehavior _behavior;
		private List<BibTeXEntry> _allBibTexEntries;

		#endregion

		#region Public Properties

		/// <summary>
		/// If the reader is allowed to process input stream while ignoring errors,
		/// this properly returns all those errors that the Load() method encountered
		/// when it was last called.
		/// </summary>
		public ICollection<BibTeXParserException> ParserExceptions
		{
			get
			{
				return _parserExceptions;
			}
		}

		/// <summary>
		/// Contains symbols (declared with @STRING in BibTeX file) declared  
		/// in the stream passed to Load() method.
		/// </summary>
		public Dictionary<string, string> SymbolTable
		{
			get
			{
				return this._symbolTable;
			}
		}

		/// <summary>
		/// Indexer property. Returns a BibTeXEntry at given index in the document.
		/// </summary>
		/// <returns></returns>
		public BibTeXEntry this[int index]
		{
			get
			{
				return this._allBibTexEntries[index];
			}
		}

		/// <summary>
		/// Number of elements in the document.
		/// </summary>
		public int Count
		{
			get
			{
				return this._allBibTexEntries.Count;
			}
		}


		#endregion

		#region Contructors

		/// <summary>
		/// Creates the BibTeXDocument object. Halts on the first parsing error.
		/// </summary>
		public BibTeXDocument()
			: this(BibTeXParserBehavior.StopOnFirstError)
		{
		}

		/// <summary>
		/// Creates the BibTeXDocument object. Parsing error handling strategy can be specified in
		/// argument.
		/// </summary>
		/// <param name="behavior">Specify parsing behavior for the function. i.e. the function should return on first parsing errors or
		/// should try to parse complete stream. Any error caused in parsing can be retrieved using BibTeXDocument.ParserErrors member.
		/// </param>
		public BibTeXDocument(BibTeXParserBehavior behavior)
		{
			this._behavior = behavior;
			this._parserExceptions = new List<BibTeXParserException>();
			this._symbolTable = new Dictionary<string, string>();
			this._allBibTexEntries = new List<BibTeXEntry>();
		}

		#endregion

		#region Public Methods


		/// <summary>
		/// Loads a stream containing bib-text into BibTeXDocument.Any parsing errors are
		/// populated into 'BibTeXDocument.ParserExceptions' field.
		/// </summary>
		/// <param name="inStream">Input stream to be processed.</param>
		/// <returns>Returns true if there is not any parsing error, false otherwise.
		/// </returns>
		/// <example>
		/// <code>
		/// using System;
		/// using System.Collections.Generic;
		/// using System.Linq;
		/// using System.Text;
		/// using Zentity.Platform;
		/// using System.IO;
		/// using Zentity.Core;
		/// using System.Data.Entity;
        /// namespace BibTeXZentitySamples 
		/// {
		///    class Program
		///        {
		///           static void Main(string[] args)
		///            {
		///                 // BibTex File "SampleBibTexFile.bib"
		///                 /*
		///                 @proceedings  {DBLP:conf/vldb/2005,
		///                 EDItor=         {Klemens B{\"o}hm and Christian S. Jensen and Laura
		///                               M. Haas and Martin L. Kersten and Per-{\AA}ke Larson
		///                                and Beng Chin Ooi}, 
		///                 title =         {Proceedings of the 31st International Conference on
		///                               Very Large Data Bases, Trondheim, Norway, August 30
		///                                - September 2, 2005}    ,
		///                 publisher =     {ACM},
		///                 }
		///                 */
		///                BibTeXDocument doc = new BibTeXDocument(BibTeXParserBehavior.IgnoreParseErrors);
		///                using (Stream stream = (new StreamReader(@"..\..\SampleBibTexFile.bib")).BaseStream)
		///                {
		///                    bool isLoadSuccessful=doc.Load(stream);
		///                    if (isLoadSuccessful)
		///                     {
		///                        // Write all symbols
		///                        Console.WriteLine("Symbols in the file:");
		///                        int i = 0;
		///                        foreach (string key in doc.SymbolTable.Keys)
		///                        {
		///                            i++;
		///                             Console.Write("name : '" + key);
		///                            Console.WriteLine("'\tvalue: '" + doc.SymbolTable[key] + "'");
		///                        }
		///                        // Write all BibTeX entries
		///                        for (int j=0; j&lt;doc.Count;  j++)
		///                        {
		///                            BibTeXEntry bibTex = doc[j];
		///                            Console.WriteLine("Type: '" + bibTex.Name + "' Key: '" + bibTex.Key + "'");
		///                            foreach (string key in bibTex.Properties.Keys)
		///                            {
		///                                Console.WriteLine("\tname : " + key);
		///                                Console.WriteLine("\tvalue: " + bibTex.Properties[key]);
		///                                Console.WriteLine();
		///                            }
		///                            Console.WriteLine();
		///                        }
		///                   }
		///                   else
		///                   {
		///                        // Write all exceptions
		///                        Console.WriteLine("Parsing Exceptions :");
		///                       int i = 0;
		///                       foreach (BibTeXParserException ex in doc.ParserExceptions)
		///                        {
		///                           i++;
		///                           Console.WriteLine(i.ToString() + ") " + ex.ToString());
		///                        }
		///                    }
		///                }
		///                Console.WriteLine();
		///            }
		///      }    
		/// }
		/// </code>
		/// </example>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public bool Load(Stream inStream)
		{
			#region Descritption function

			// Algorithm to implement this function:
			//
			// 1. Clear _parserExceptions and _symbolTable collections and initialize
			//    the tokenizer object
			// 2. Create a Tokenize object and pass it stream object.
			// 3. A tool that will process each entry starting with @
			//     a) Build a set of tokens of using tokenizer. the first
			//        element in this set should be '@' and the last should be '}'
			//        (token before the next entry in the BibTeX file.
			//     b) Pass this set to the BibTeX paser's GetBibTeXEntry and retrieve the BibTeXEnty object.
			//        If _throwParserException is false, encapsulate this call in try/catch  and store any exception.
			//     c) Add the BibTeXEntry to the collection (which will be returned).
			//

			#endregion

			List<BibTeXEntry> bibTeXEntries = new List<BibTeXEntry>();
			Dictionary<string, string> symbolTable = new Dictionary<string, string>();

			BibTeXTokenizer tokenProvider = new BibTeXTokenizer(inStream);
			BibTeXParser parser = new BibTeXParser(symbolTable);
			this._parserExceptions.Clear();

			//Queue to store any prepopulated tokens
			Queue<BibTeXToken> retrievedTokens = new Queue<BibTeXToken>();

			while (!tokenProvider.LastTokenEncountered)
			{
				int bracesStack = 0;
				int parenthesisStack = 0;
				List<BibTeXToken> tokenSet = new List<BibTeXToken>();

				#region Build a set of tokens starting with "@" and ending with "}" (upto next '@' symbol)

				try
				{
					//Dequeue, if any other token retrieved earlier
					while (retrievedTokens.Count > 0)
					{
						BibTeXToken token = retrievedTokens.Dequeue();
						if (String.Compare(token.Value, BibTeXHelper.BT_ENTRY_AT, StringComparison.OrdinalIgnoreCase) == 0)
						{
							tokenSet.Add(token);
						}
					}

					while (!tokenProvider.LastTokenEncountered)
					{
						BibTeXToken token = tokenProvider.GetNextToken();
						if (tokenSet.Count == 0 && String.Compare(token.Value, BibTeXHelper.BT_ENTRY_AT,
																	StringComparison.OrdinalIgnoreCase) != 0
							)
						{	// Skip all tokens until we find '@'. Any token outside the '@' are treated as comments
							continue;
						}
						if (!String.IsNullOrEmpty(token.Value)) // just to make sure.
						{
							if (tokenSet.Count > 2 && bracesStack == 0 && parenthesisStack == 0)
							{
								// Enqueue current token. Will use it for next entry.
								retrievedTokens.Enqueue(token);
								break;
							}

							if (String.Compare(token.Value, BibTeXHelper.BT_ENTRY_AT,
												StringComparison.OrdinalIgnoreCase) == 0
								 && tokenSet.Count != 0
								)
							{
								// Enqueue current token. Will use it for next entry.
								retrievedTokens.Enqueue(token);
								break;
							}

							tokenSet.Add(token);

							// Manage stack
							if (String.Compare(token.Value, BibTeXHelper.BT_ENTRY_LEFT_BRACE,
												StringComparison.OrdinalIgnoreCase) == 0)
							{
								bracesStack++;
							}
							else if (String.Compare(token.Value, BibTeXHelper.BT_ENTRY_RIGHT_BRACE,
													StringComparison.OrdinalIgnoreCase) == 0)
							{
								bracesStack--;
							}
							else if (String.Compare(token.Value, BibTeXHelper.BT_ENTRY_LEFT_PARENTHESIS,
														StringComparison.OrdinalIgnoreCase) == 0)
							{
								parenthesisStack++;
							}
							else if (String.Compare(token.Value, BibTeXHelper.BT_ENTRY_RIGHT_PARENTHESIS,
													StringComparison.OrdinalIgnoreCase) == 0)
							{
								parenthesisStack--;
							}
						}
					}

					if (tokenSet.Count == 0)
					{
						// An empty token set was built.
						// This can happened only when the file has nothing (empty file).
						// Continue the loop, the loop will be terminated automatically. 
						continue;
					}
					if (tokenSet.Count == 1)
					{
						//Throw exception as the token set does not have an entry name.
						throw new BibTeXParserException(Properties.Resources.BIBTEXPARSER_NOT_WELL_FORMED,
														tokenSet[0].LineNumber, tokenSet[0].ColumnNumber, String.Empty);
					}

					// Check for entry type and process accordingly.
					switch (tokenSet[1].Value.ToUpperInvariant())
					{
						case BibTeXHelper.BT_ENTRY_VARIABLE_TYPE:
							#region Process Variable Declaration @STRING

							try
							{
								ParseSymbol(tokenSet, ref symbolTable);
							}
							catch (BibTeXParserException pException)
							{
								if (this._behavior == BibTeXParserBehavior.StopOnFirstError)
								{
									// Break the parsing, and return from the function
									// Save the exception for later notification and return false as some error occured
									this._parserExceptions.Add(pException);
									return false;
								}
								else if (this._behavior == BibTeXParserBehavior.IgnoreParseErrors)
								{
									// Save the exception for later notification, and continue.
									this._parserExceptions.Add(pException);
								}
							}

							#endregion
							break;

						case BibTeXHelper.BT_ENTRY_COMMENT_TYPE:
							#region Process Comments @COMMENT
							//simply ignore this set of token
							#endregion
							break;

						default:
							#region Process the BibTeX entry @AUTHOR, @BOOK, @ARTICLE, etc;

                            // Catch only the parsing exception. If any other exceptions (like IOException) then throw them out immediately 
							try
							{
								bibTeXEntries.Add(parser.GetBibTeXEntry(tokenSet));
							}
							catch (BibTeXParserException pException)
							{
								if (this._behavior == BibTeXParserBehavior.StopOnFirstError)
								{
									// Break the parsing, and return from the function
									// Save the exception for later notification and return false as some error occurred
									this._parserExceptions.Add(pException);
									return false;
								}
								else if (this._behavior == BibTeXParserBehavior.IgnoreParseErrors)
								{
									// Save the exception for later notification, and continue.
									this._parserExceptions.Add(pException);
								}
							}
							#endregion
							break;
					}

				}
				catch (BibTeXParserException pException)
				{
					if (this._behavior == BibTeXParserBehavior.StopOnFirstError)
					{
						// Break the parsing, and return from the function
						// Save the exception for later notification and return false as some error occurred
						this._parserExceptions.Add(pException);
						return false;
					}
					else if (this._behavior == BibTeXParserBehavior.IgnoreParseErrors)
					{
						// Save the exception for later notification, and continue.
						this._parserExceptions.Add(pException);
					}
				}

				#endregion

			} // while


			// Update state of the Document only if successfully parsed (in case of StopOnFirstError)
			if ((this._behavior == BibTeXParserBehavior.StopOnFirstError && this._parserExceptions.Count == 0)
				|| this._behavior == BibTeXParserBehavior.IgnoreParseErrors
				)
			{
				this._allBibTexEntries = bibTeXEntries;
				this._symbolTable = symbolTable;
			}

			if (this._parserExceptions.Count == 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Loads a file containing bib-text into BibTeXDocument. Any parsing errors are
		/// populated into 'BibTeXDocument.ParserExceptions' field.
		/// </summary>
		/// <param name="filePath">Full path of the file to be read.</param>
		/// <returns>Returns true if there is not any parsing error, false otherwise.</returns>
		/// <example>
		/// <code>
		/// using System;
		/// using System.Collections.Generic;
		/// using System.Linq;
		/// using System.Text;
		/// using Zentity.Platform;
		/// using System.IO;
		/// using Zentity.Core;
		/// using System.Data.Entity;
		/// namespace BibTeXZentitySamples 
		/// {
		///    class Program
		///        {
		///           static void Main(string[] args)
		///            {
		///                 // BibTex File "SampleBibTexFile.bib"
		///                 /*
		///                 @proceedings  {DBLP:conf/vldb/2005,
		///                 EDItor=         {Klemens B{\"o}hm and Christian S. Jensen and Laura
		///                               M. Haas and Martin L. Kersten and Per-{\AA}ke Larson
		///                                and Beng Chin Ooi}, 
		///                 title =         {Proceedings of the 31st International Conference on
		///                               Very Large Data Bases, Trondheim, Norway, August 30
		///                                - September 2, 2005}    ,
		///                 publisher =     {ACM},
		///                 }
		///                 */
		///                BibTeXDocument doc = new BibTeXDocument(BibTeXParserBehavior.IgnoreParseErrors);
		///                string filePath=@"..\..\SampleBibTexFile.bib";
		///                bool isLoadSuccessful=doc.Load(filePath);
		///                if (isLoadSuccessful)
		///                {
		///                     // Write all symbols
		///                     Console.WriteLine("Symbols in the file:");
		///                     int i = 0;
		///                     foreach (string key in doc.SymbolTable.Keys)
		///                     {
		///                         i++;
		///                         Console.Write("name : '" + key);
		///                         Console.WriteLine("'\tvalue: '" + doc.SymbolTable[key] + "'");
		///                     }
		///                     // Write all BibTeX entries
		///                     for (int j=0; j&lt;doc.Count;  j++)
		///                     {
		///                         BibTeXEntry bibTex = doc[j];
		///                         Console.WriteLine("Type: '" + bibTex.Name + "' Key: '" + bibTex.Key + "'");
		///                         foreach (string key in bibTex.Properties.Keys)
		///                         {
		///                             Console.WriteLine("\tname : " + key);
		///                             Console.WriteLine("\tvalue: " + bibTex.Properties[key]);
		///                             Console.WriteLine();
		///                          }
		///                            Console.WriteLine();
		///                       }
		///                  }
		///                  else
		///                  {
		///                       // Write all exceptions
		///                       Console.WriteLine("Parsing Exceptions :");
		///                       int i = 0;
		///                       foreach (BibTeXParserException ex in doc.ParserExceptions)
		///                       {
		///                           i++;
		///                            Console.WriteLine(i.ToString() + ") " + ex.ToString());
		///                       }
		///                  }
		///            }
		///      }    
		/// }
		/// </code>
		/// </example>
		public bool Load(String filePath)
		{
			if (String.IsNullOrEmpty(filePath))
				throw new ArgumentException(Properties.Resources.BIBTEXTDOCUMENT_SAVE_ARGUMENTEXCEPTION_FILEPATH, "filePath");

			using (Stream stream = (new StreamReader(filePath)).BaseStream)
			{
				return this.Load(stream);
			}
		}

		/// <summary>
		/// Writes the contents of the document to the output stream.
		/// The BibTeX entry will be written in following format :
		/// @TypeName{Key,
		/// PropertyName = [{] Property value [}],
		/// ...
		/// } 
		/// 
		/// Note : Exported file may not be exactly similar to imported file.
		/// </summary>
		/// <param name="outStream">Output stream to be used.</param>
		/// <exception cref="ArgumentException">
		/// Throws an exception when outStream parameter does not support Write operation.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Throws an exception if outStream is null.
		/// </exception>
		/// <example>
		/// <code>
		/// using System;
		/// using System.Collections.Generic;
		/// using System.Linq;
		/// using System.Text;
		/// using Zentity.Platform;
		/// using System.IO;
		/// using Zentity.Core;
		/// using System.Data.Entity;
		/// namespace BibTeXZentitySamples 
		/// {
		///    class Program
		///        {
		///           static void Main(string[] args)
		///            {
		///                BibTeXDocument doc = new BibTeXDocument(BibTeXParserBehavior.IgnoreParseErrors);
		///                 using (Stream outStream = (new FileStream(@"..\..\SampleBibTexFile.bib", FileMode.OpenOrCreate,
		///                                             FileAccess.ReadWrite)))
		///                 {                            
		///                     // Create instance of BibTeXEntry class
		///                     BibTeXEntry bookEntry = new BibTeXEntry();
		///                     bookEntry.Name = "book";
        ///                     bookEntry.Key = "Zentity:book";
		///                     bookEntry.Properties.Add("title", "BibTex book: Title");
		///                     bookEntry.Properties.Add("editor", "List of editors");
		///                     bookEntry.Properties.Add("year", "2004");
		///                     
		///                     // Add BibTeX entry to BibTeX document
		///                     doc.Add(bookEntry);
		///                     try
		///                     {
		///                         doc.Save(outStream);
		///                     }
		///                     catch(ArgumentNullException ex)
		///                     {
		///                         Console.WriteLine(ex.Message);
		///                     }
		///                     catch(ArgumentException ex)
		///                     {
		///                         Console.WriteLine(ex.Message);
		///                     }
		///                 }    
		///            }
		///      }    
		/// }
		/// </code>
		/// </example>
		public void Save(Stream outStream)
		{
			if (outStream == null)
			{
				throw new ArgumentNullException("outStream");
			}
			if (!outStream.CanWrite)
			{
				throw new ArgumentException(Properties.Resources.BIBTEXTDOCUMENT_SAVE_ARGUMENTEXCEPTION_MESSAGE, "outStream");
			}

			using (StreamWriter streamWriter = new StreamWriter(outStream))
			{

				//Loop through all the BibTeX entries to write them in file.
				foreach (BibTeXEntry bibTexEntry in _allBibTexEntries)
				{
					// Write name of the BibTex type, it must be prefixed by '@'          
					streamWriter.Write(BibTeXHelper.BT_ENTRY_AT + bibTexEntry.Name + BibTeXHelper.BT_ENTRY_LEFT_BRACE);
					streamWriter.Write(bibTexEntry.Key.Trim());

					// Write comma separated Properties
					foreach (KeyValuePair<string, string> pair in bibTexEntry.Properties)
					{
						//Append comma to the previous line
						streamWriter.WriteLine(BibTeXHelper.BT_ENTRY_COMMA);

						streamWriter.Write(pair.Key + BibTeXHelper.BT_ENTRY_EQUALTO);

						if (pair.Value == null)
						{
							// if value is null, write empty string. (may be 'null' is intentionally added as value.
							streamWriter.Write(BibTeXHelper.BT_ENTRY_LEFT_BRACE +
											   string.Empty +
											   BibTeXHelper.BT_ENTRY_RIGHT_BRACE);

						}
						else
						{
							streamWriter.Write(BibTeXHelper.BT_ENTRY_LEFT_BRACE +


											   pair.Value +
											   BibTeXHelper.BT_ENTRY_RIGHT_BRACE);
						}
					}//end of foreach                

					//BibTeX Entry End
					streamWriter.WriteLine(System.Environment.NewLine +
										   BibTeXHelper.BT_ENTRY_RIGHT_BRACE +
										   System.Environment.NewLine);
				}
			}

		}

		/// <summary>
		/// Writes the contents of the document to the output stream.
		/// </summary>
		/// <param name="filePath">Full path of the file to be written.</param>
		/// <param name="overwriteExisting">if true, any existing file will be overwritten.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <example>
		/// <code>
		/// using System;
		/// using System.Collections.Generic;
		/// using System.Linq;
		/// using System.Text;
		/// using Zentity.Platform;
		/// using System.IO;
		/// using Zentity.Core;
		/// using System.Data.Entity;
		/// namespace BibTeXZentitySamples 
		/// {
		///    class Program
		///        {
		///           static void Main(string[] args)
		///            {
		///                BibTeXDocument doc = new BibTeXDocument(BibTeXParserBehavior.IgnoreParseErrors);
		///                string filePath=@"..\..\SampleBibTexFile.bib";
		///                
		///                // If true overwrite existing file
		///                bool overwriteExisting = true;
		///                
		///                // Create instance of BibTeXEntry class
		///                BibTeXEntry bookEntry = new BibTeXEntry();
		///                bookEntry.Name = "book";
        ///                bookEntry.Key = "Zentity:book";
		///                bookEntry.Properties.Add("title", "BibTex book: Title");
		///                bookEntry.Properties.Add("editor", "List of editors");
		///                bookEntry.Properties.Add("year", "2004");
		///                
		///                // Add BibTeX entry to BibTeX document
		///                doc.Add(bookEntry);
		///                try
		///                {
		///                    doc.Save(filePath, overwriteExisting);
		///                }
		///                catch(ArgumentException ex)
		///                {
		///                    Console.WriteLine(ex.Message);
		///                }
		///            }
		///      }    
		/// }
		/// </code>
		/// </example>
		public void Save(String filePath, bool overwriteExisting)
		{
			if (String.IsNullOrEmpty(filePath))
				throw new ArgumentException(Properties.Resources.BIBTEXTDOCUMENT_SAVE_ARGUMENTEXCEPTION_FILEPATH, "filePath");

			FileMode mode = overwriteExisting ? FileMode.Create : FileMode.CreateNew;
			using (FileStream fileStream = new FileStream(filePath, mode, FileAccess.Write, FileShare.None))
			{
				this.Save(fileStream);
				fileStream.Close();
			}
		}

		/// <summary>
		/// Adds a BibTeX entry at the last in the document.
		/// </summary>
        /// <param name="bibTexEntry">BibTeX entry to be added to the Bibtex document.</param>
		public void Add(BibTeXEntry bibTexEntry)
		{
			this._allBibTexEntries.Add(bibTexEntry);
		}

		/// <summary>
        /// Inserts a BibTeX entry after the given index in the document.
		/// </summary>
		/// <param name="index">Zero based index at which item should be inserted.</param>
		/// <param name="bibTexEntry">Item to be inserted.</param>
		public void Insert(int index, BibTeXEntry bibTexEntry)
		{
			this._allBibTexEntries.Insert(index, bibTexEntry);
		}

		/// <summary>
		/// Removes the element at the specified index in the document.
		/// </summary>
		/// <param name="index">Index of item.</param>
		public void RemoveAt(int index)
		{
			this._allBibTexEntries.RemoveAt(index);
		}

		/// <summary>
		/// Clears all items in the document.
		/// </summary>
		public void Clear()
		{
			this._allBibTexEntries.Clear();
		}

		#endregion

		/// <summary>
		/// Processes a set of tokens and checks for valid symbol/variable declaration
		/// </summary>
		/// <exception cref="BibTeXParserException"></exception>
		/// <param name="tokenSet">list of tokens to be processed</param>
		/// <param name="symbolTable">if it is a valid variable set the adds it to the symbol table</param>
		private static void ParseSymbol(List<BibTeXToken> tokenSet, ref Dictionary<string, string> symbolTable)
		{
			//number of tokens must be exactly 9
			//-  0     1      2      3     4   5     6    7    8
			//-  @   string   {    name    =   "   Test   "    }
			if (tokenSet.Count != 9)
			{
				throw new BibTeXParserException("Invalid symbol declaration.");
			}

			if (String.Compare(tokenSet[2].Value, BibTeXHelper.BT_ENTRY_LEFT_BRACE,
								StringComparison.OrdinalIgnoreCase) != 0
					&&
				String.Compare(tokenSet[2].Value, BibTeXHelper.BT_ENTRY_LEFT_PARENTHESIS,
								StringComparison.OrdinalIgnoreCase) != 0
				)
			{
				throw new BibTeXParserException("'{' expected.", tokenSet[2].LineNumber, tokenSet[2].ColumnNumber, tokenSet[2].Value);
			}

			if (String.Compare(tokenSet[4].Value, BibTeXHelper.BT_ENTRY_EQUAL,
								StringComparison.OrdinalIgnoreCase) != 0
				)
			{
				throw new BibTeXParserException("'=' expected.", tokenSet[4].LineNumber, tokenSet[4].ColumnNumber, tokenSet[4].Value);
			}

			if (String.Compare(tokenSet[5].Value, BibTeXHelper.BT_ENTRY_QUOTATION,
								StringComparison.OrdinalIgnoreCase) != 0
				)
			{
				throw new BibTeXParserException("'\"' expected.", tokenSet[5].LineNumber, tokenSet[5].ColumnNumber, tokenSet[5].Value);
			}

			if (String.Compare(tokenSet[7].Value, BibTeXHelper.BT_ENTRY_QUOTATION,
								StringComparison.OrdinalIgnoreCase) != 0
				)
			{
				throw new BibTeXParserException("'\"' expected.", tokenSet[7].LineNumber, tokenSet[7].ColumnNumber,
												 tokenSet[7].Value);
			}

			if (String.Compare(tokenSet[2].Value, BibTeXHelper.BT_ENTRY_LEFT_BRACE,
							   StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (String.Compare(tokenSet[8].Value, BibTeXHelper.BT_ENTRY_RIGHT_BRACE,
									StringComparison.OrdinalIgnoreCase) != 0)
				{
					throw new BibTeXParserException("'}' expected.", tokenSet[8].LineNumber, tokenSet[8].ColumnNumber, tokenSet[8].Value);
				}
			}
			else if (String.Compare(tokenSet[2].Value, BibTeXHelper.BT_ENTRY_LEFT_PARENTHESIS, StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (String.Compare(tokenSet[8].Value, BibTeXHelper.BT_ENTRY_RIGHT_PARENTHESIS, StringComparison.OrdinalIgnoreCase) != 0)
				{
					throw new BibTeXParserException("')' expected.", tokenSet[8].LineNumber, tokenSet[8].ColumnNumber, tokenSet[8].Value);
				}
			}
			else
			{
				// Following should not be called in any case, as an error will be thrown while checking tokenSet[2] itself
				throw new BibTeXParserException("'}' expected.", tokenSet[8].LineNumber, tokenSet[8].ColumnNumber, tokenSet[8].Value);
			}

			// add of replace existing (in case redeclared).
			symbolTable[tokenSet[3].Value] = tokenSet[6].Value;
		}

	}
}
