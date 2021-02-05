// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects.DataClasses;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Zentity.Core;
    using Zentity.ScholarlyWorks;

	/// <summary>
    /// This class convert a BibTeX entry to a Zentity Resource type or vice versa.
	/// </summary> 
	public class BibTeXConverter
	{
		private BibTeXDocument bibTeXDocument;
		private BibTeXParserBehavior parserBehavior = BibTeXParserBehavior.StopOnFirstError;
		private ICollection<BibTeXMappingException> mappingErrors;
        private static HashSet<Person> peopleAdded = new HashSet<Person>();

		/// <summary>
		/// Gets a collection of mapping errors which 
        /// occurred in the last call to Import() or Export() function.
		/// </summary>
		public ICollection<BibTeXMappingException> MappingErrors
		{
			get
			{
				return mappingErrors;
			}
		}

		/// <summary>
		/// Gets the collection of parsing errors which were occurred 
        /// in the last call to Import() function.
		/// </summary>
		public ICollection<BibTeXParserException> ParserErrors
		{
			get
			{
				return bibTeXDocument.ParserExceptions;
			}
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXConverter"/> class.
        /// </summary>
        /// <param name="behavior">The parsing behavior.</param>
		public BibTeXConverter(BibTeXParserBehavior behavior)
		{
			this.parserBehavior = behavior;
			this.mappingErrors = new List<BibTeXMappingException>();
		}

		/// <summary>
		/// Converts BibTeX data to Resource objects. Any parsing errors will be populated into BibTeXConverter.ParserErrors
		/// </summary>
		/// <param name="inputStream">Input stream from which BibTeX data to be read.</param>
		/// <returns>Returns a enumerable holding parsed objects.</returns>
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
		///      class Program
		///      {
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
		///                  BibTeXConverter converter = new BibTeXConverter(BibTeXParserBehavior.IgnoreParseErrors);
		///                  using (Stream inputStream = (new StreamReader(@"..\..\SampleBibTexFile.bib")).BaseStream)
		///                  {
		///                        IEnumerable&lt;Resource&gt; allReouseces = converter.Import(inputStream);
		///                        Console.WriteLine("Titles of imported resources:");
		///                        foreach (Resource resource in allReouseces)
		///                        {
		///                            Console.WriteLine(resource.Title);
		///                        }
		///                        if (converter.ParserErrors.Count &gt; 0)
		///                        {
		///                            Console.WriteLine();
		///                            Console.WriteLine("Parser Errors:");
		///                            foreach (BibTeXParserException error in converter.ParserErrors)
		///                            {
		///                                Console.WriteLine(error.Message);
		///                            }
		///                        }
		///                        if (converter.MappingErrors.Count &gt; 0)
		///                        {
		///                           Console.WriteLine();
		///                           Console.WriteLine("Mapping Errors:");
		///                           foreach (BibTeXMappingException error in converter.MappingErrors)
		///                            {
		///                                Console.WriteLine(error.Message);
		///                            }
		///                        }
		///                   }
		///             }
		///       }    
		/// }
		/// </code>
		/// </example>
		public IEnumerable<ScholarlyWork> Import(Stream inputStream)
		{
			if (inputStream == null)
				throw new ArgumentNullException("inputStream");

            List<ScholarlyWork> resources = new List<ScholarlyWork>();
            peopleAdded.Clear();
            
			this.bibTeXDocument = new BibTeXDocument(this.parserBehavior);
			this.bibTeXDocument.Load(inputStream);

			if (this.parserBehavior == BibTeXParserBehavior.StopOnFirstError && this.ParserErrors.Count > 0)
			{
				return null;
			}
			else
			{
				for (int i = 0; i < this.bibTeXDocument.Count; i++)
				{
					#region Build Resource from BibTeXEntry and add it ot the collection of Resources
					try
					{
                        ScholarlyWork res = GetResource(this.bibTeXDocument[i]);
                        
						if (res != null)
						{
							resources.Add(res);
						}
					}
					catch (BibTeXMappingException mappingExce)
					{
						if (this.parserBehavior == BibTeXParserBehavior.StopOnFirstError)
						{
							this.mappingErrors.Add(mappingExce);
							return null;
						}
						else if (this.parserBehavior == BibTeXParserBehavior.IgnoreParseErrors)
						{
							this.mappingErrors.Add(mappingExce);
						}
					}
					#endregion
				}

                return resources.AsEnumerable<ScholarlyWork>();
			}
		}

        /// <summary>
		/// Converts BibTeX data to Resource objects. Any parsing errors will be populated into BibTeXConverter.ParserErrors.
		/// </summary>
		/// <param name="filePath">Path of the input file to be read.</param>
		/// <returns>Returns a enumerable holding parsed objects.</returns>
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
		///      class Program
		///      {
		///           static void Main(string[] args)
		///            {
		///                  // BibTex File "SampleBibTexFile.bib"
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
		///                  BibTeXConverter converter = new BibTeXConverter(BibTeXParserBehavior.IgnoreParseErrors);
		///                  string filePath=@"..\..\SampleBibTexFile.bib";
		///                  IEnumerable&lt;Resource&gt; allReouseces = converter.Import(filePath);
		///                  Console.WriteLine("Titles of imported resources:");
		///                  foreach (Resource resource in allReouseces)
		///                  {
		///                       Console.WriteLine(resource.Title);
		///                  }
		///                  if (converter.ParserErrors.Count &gt; 0)
		///                  {
		///                       Console.WriteLine();
		///                       Console.WriteLine("Parser Errors:");
		///                       foreach (BibTeXParserException error in converter.ParserErrors)
		///                       {
		///                           Console.WriteLine(error.Message);
		///                       }
		///                  }
		///                  if (converter.MappingErrors.Count &gt; 0)
		///                  {
		///                      Console.WriteLine();
		///                      Console.WriteLine("Mapping Errors:");
		///                      foreach (BibTeXMappingException error in converter.MappingErrors)
		///                      {
		///                           Console.WriteLine(error.Message);
		///                      }
		///                  }
		///             }
		///       }    
		/// }
		/// </code>
		/// </example>
        public IEnumerable<ScholarlyWork> Import(String filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			using (Stream stream = (new StreamReader(filePath)).BaseStream)
			{
				return this.Import(stream);
			}
		}

		#region Private Helpers Function for Import() function

		/// <summary>
		/// Builds a Resource object from BibTeXEntry
		/// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
		private static ScholarlyWork GetResource(BibTeXEntry bibTeXEntry)
		{
			//  Following Exception can be thrown
			//	FormatException;
			//	OverflowException;
			//	KeyNotFoundException;

            ScholarlyWork resource = null;

            // Handled ZentityException occurred if property's value is invalid. 
            try
            {
                switch (bibTeXEntry.Name.ToUpperInvariant())
                {
                    case "MISC":
                    resource = BibTeXConverter.GetResourcePublication(bibTeXEntry);
                        break;
                    case "BOOK":
                        resource = BibTeXConverter.GetResourceBook(bibTeXEntry);
                        break;
                    case "BOOKLET":
                        resource = BibTeXConverter.GetResourceBooklet(bibTeXEntry);
                        break;
                    case "INBOOK":
                        resource = BibTeXConverter.GetResourceChapter(bibTeXEntry);
                        break;
                    case "ARTICLE":
                        resource = BibTeXConverter.GetResourceJournalArticle(bibTeXEntry);
                        break;
                    case "MANUAL":
                        resource = BibTeXConverter.GetResourceManual(bibTeXEntry);
                        break;
                    case "PROCEEDINGS":
                        resource = BibTeXConverter.GetResourceProceedings(bibTeXEntry);
                        break;
                    case "INPROCEEDINGS":
                        resource = BibTeXConverter.GetResourceProceedingsArticle(bibTeXEntry);
                        break;
                    case "TECHREPORT":
                        resource = BibTeXConverter.GetResourceTechnicalReport(bibTeXEntry);
                        break;
                    case "MASTERSTHESIS":
                        resource = BibTeXConverter.GetResourceThesisMsc(bibTeXEntry);
                        break;
                    case "PHDTHESIS":
                        resource = BibTeXConverter.GetResourceThesisPhD(bibTeXEntry);
                        break;
                    case "UNPUBLISHED":
                        resource = BibTeXConverter.GetResourceUnpublished(bibTeXEntry);
                        break;
                    default:
                        throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                         Properties.Resources.BIBTEXCONVERTER_MAPPING_NOT_DEFINED,
                                                         bibTeXEntry.Name));
                }
                peopleAdded.UnionWith(resource.Authors.OfType<Person>());
                peopleAdded.UnionWith(resource.Editors.OfType<Person>());
            }
            catch (ZentityException ex)
            {
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                        Properties.Resources.BIBTEXCONVERTER_INVALID_FIELD_VALUE,
                                                        new object[] { ex.Message, bibTeXEntry.Name, bibTeXEntry.Key }
                                                        ));
                //throw new BibTeXMappingException(ex.Message + "  in   " + bibTeXEntry.Name + ":" + bibTeXEntry.Key);
            }

			return resource;
		}

        /// <summary>
        /// Sets the published date.
        /// </summary>
        /// <param name="pub">The pub.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        private static void SetPublishedDate(Publication pub, int year, int month, int day)
        {
            pub.DatePublished = new DateTime(year, month, day);
            pub.YearPublished = year;
            pub.MonthPublished = month;
            pub.DayPublished = day;
        }

        /// <summary>
        /// Gets the publication resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourcePublication(BibTeXEntry bibTeXEntry)
		{
			Publication publication = new Publication();
			//Required fields: none

            if (!bibTeXEntry.Properties.ContainsKey("title"))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
                                                new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
                                            ));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

            publication.Title = bibTeXEntry.Properties["title"];

			//Optional fields: author, title, howpublished, month, year, note, key
			//Match : author, title, note
			BibTeXConverter.ConvertToPersons(publication.Authors,
				bibTeXEntry.Properties.ContainsKey("author") ? bibTeXEntry.Properties["author"] : string.Empty);
			//publication.Title = bibTeXEntry.Properties.ContainsKey("title") ? bibTeXEntry.Properties["title"] : null;
			publication.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties.ContainsKey("year") ? bibTeXEntry.Properties["year"] : string.Empty, out year))
				year = DateTime.Now.Year;
			if( year <= 0 )
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ? bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;
			SetPublishedDate (publication, year, month, 1);

			return publication;
		}

        /// <summary>
        /// Gets the book resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceBook(BibTeXEntry bibTeXEntry)
		{
			Book book = new Book();

			// Required fields: author/editor, title, publisher, year 
			if (!bibTeXEntry.Properties.ContainsKey("author") && !bibTeXEntry.Properties.ContainsKey("editor"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
													new object[] { "author' or 'editor", bibTeXEntry.Name, bibTeXEntry.Key }
													));
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
																new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
															   ));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] {bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("publisher"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
															   new object[] { "publisher", bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
															   new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
															   ));

            BibTeXConverter.ConvertToPersons(book.Authors, bibTeXEntry.Properties.ContainsKey("author") ? bibTeXEntry.Properties["author"] : String.Empty);
			book.Title = bibTeXEntry.Properties["title"];

            peopleAdded.UnionWith(book.Authors.OfType<Person>());
            BibTeXConverter.ConvertToPersons(book.Editors, bibTeXEntry.Properties.ContainsKey("editor") ? bibTeXEntry.Properties["editor"] : String.Empty);
			book.Publisher = bibTeXEntry.Properties["publisher"];

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ? bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (book, year, month, 1);

			// Optional fields : volume, series, address, edition, month, note, key 
			book.Volume = bibTeXEntry.Properties.ContainsKey("volume") ? bibTeXEntry.Properties["volume"] : null;
			book.Series = bibTeXEntry.Properties.ContainsKey("series") ? bibTeXEntry.Properties["series"] : null;
			book.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ? bibTeXEntry.Properties["address"] : null;
			book.Edition = bibTeXEntry.Properties.ContainsKey("edition") ? bibTeXEntry.Properties["edition"] : null;
			book.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return book;
		}

        /// <summary>
        /// Gets the booklet resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceBooklet(BibTeXEntry bibTeXEntry)
		{
			Booklet book = new Booklet();

			// Required fields: title 
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
															   new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
															   ));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			book.Title = bibTeXEntry.Properties["title"];

			// Optional fields: author, howpublished, address, month, year, note, key 
			//TODO: howpublished, key - yet to decide what to do for these fields

			BibTeXConverter.ConvertToPersons(book.Authors,
				bibTeXEntry.Properties.ContainsKey("author") ? bibTeXEntry.Properties["author"] : string.Empty);
			book.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ? bibTeXEntry.Properties["address"] : null;
			book.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties.ContainsKey("year") ?
								bibTeXEntry.Properties["year"] : string.Empty, out year))
				year = DateTime.Now.Year;
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ?
												bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (book, year, month, 1);

			return book;
		}

        /// <summary>
        /// Gets the chapter resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceChapter(BibTeXEntry bibTeXEntry)
		{
			Chapter chapter = new Chapter();

			//Required fields: author/editor, title, chapter/pages, publisher, year 
			if (!bibTeXEntry.Properties.ContainsKey("author") && !bibTeXEntry.Properties.ContainsKey("editor"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "author' or 'editor", bibTeXEntry.Name, bibTeXEntry.Key }
												 ));
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
												 ));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("chapter") && !bibTeXEntry.Properties.ContainsKey("pages"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "chapter' or 'pages", bibTeXEntry.Name, bibTeXEntry.Key }
												 ));
			if (!bibTeXEntry.Properties.ContainsKey("publisher"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "publisher", bibTeXEntry.Name, bibTeXEntry.Key }
												 ));
			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
												 ));

			BibTeXConverter.ConvertToPersons(chapter.Authors, bibTeXEntry.Properties.ContainsKey("author") ?
											  bibTeXEntry.Properties["author"] : String.Empty);

            peopleAdded.UnionWith(chapter.Authors.OfType<Person>());
			BibTeXConverter.ConvertToPersons(chapter.Editors, bibTeXEntry.Properties.ContainsKey("editor") ?
                                              bibTeXEntry.Properties["editor"] : String.Empty);

			chapter.Title = bibTeXEntry.Properties["title"];
			chapter.Publisher = bibTeXEntry.Properties["publisher"];
			chapter.Pages = bibTeXEntry.Properties.ContainsKey("pages") ? bibTeXEntry.Properties["pages"] : null;
			chapter.Chapter = bibTeXEntry.Properties.ContainsKey("chapter") ? bibTeXEntry.Properties["chapter"] : null;

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ?
												 bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;
			SetPublishedDate (chapter, year, month, 1);

			//Optional fields: volume, series, address, edition, month, note, key 
			//TODO: for Key: yet to decide
			chapter.Volume = bibTeXEntry.Properties.ContainsKey("volume") ? bibTeXEntry.Properties["volume"] : null;
			chapter.Series = bibTeXEntry.Properties.ContainsKey("series") ? bibTeXEntry.Properties["series"] : null;
			chapter.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ? bibTeXEntry.Properties["address"] : null;
			chapter.Edition = bibTeXEntry.Properties.ContainsKey("edition") ? bibTeXEntry.Properties["edition"] : null;
			chapter.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return chapter;
		}

        /// <summary>
        /// Gets the journal article resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceJournalArticle(BibTeXEntry bibTeXEntry)
		{
			JournalArticle article = new JournalArticle();

			// Required fields: author, title, journal, year
			if (!bibTeXEntry.Properties.ContainsKey("author"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "author", bibTeXEntry.Name, bibTeXEntry.Key }
												));
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
												));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("journal"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "journal", bibTeXEntry.Name, bibTeXEntry.Key }
												));
			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
												));

			BibTeXConverter.ConvertToPersons(article.Authors, bibTeXEntry.Properties["author"]);
			article.Title = bibTeXEntry.Properties["title"];
			article.Journal = bibTeXEntry.Properties["journal"];

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ? bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;
			SetPublishedDate (article, year, month, 1);

			// Optional fields: volume, number, pages, month, note, key
			//TODO: Key
			article.Volume = bibTeXEntry.Properties.ContainsKey("volume") ? bibTeXEntry.Properties["volume"] : null;
			article.Number = bibTeXEntry.Properties.ContainsKey("number") ? bibTeXEntry.Properties["number"] : null;
			article.Pages = bibTeXEntry.Properties.ContainsKey("pages") ? bibTeXEntry.Properties["pages"] : null;
			article.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return article;
		}

        /// <summary>
        /// Gets the manual resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceManual(BibTeXEntry bibTeXEntry)
		{
			Manual manual = new Manual();

			// Required fields: title
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
											));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			manual.Title = bibTeXEntry.Properties["title"];

			// Optional fields: author, organization, address, edition, month, year, note, key
			// TODO: key 
			BibTeXConverter.ConvertToPersons(manual.Authors,
				bibTeXEntry.Properties.ContainsKey("author") ? bibTeXEntry.Properties["author"] : string.Empty);

			manual.Organization = bibTeXEntry.Properties.ContainsKey("organization") ? bibTeXEntry.Properties["organization"] : null;
			manual.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ? bibTeXEntry.Properties["address"] : null;
			manual.Edition = bibTeXEntry.Properties.ContainsKey("edition") ? bibTeXEntry.Properties["edition"] : null;
			manual.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties.ContainsKey("year") ?
								bibTeXEntry.Properties["year"] : string.Empty, out year))
				year = DateTime.Now.Year;
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ? bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (manual, year, month, 1);

			return manual;
		}

        /// <summary>
        /// Gets the proceedings resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceProceedings(BibTeXEntry bibTeXEntry)
		{
			Proceedings proceeding = new Proceedings();

			// Required fields: title, year
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
											));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
											));

			proceeding.Title = bibTeXEntry.Properties["title"];

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ? bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (proceeding, year, month, 1);

			// Optional fields: editor, publisher, organization, address, month, note, key
			//TODO: key : yet to decide
			BibTeXConverter.ConvertToPersons(proceeding.Editors,
				bibTeXEntry.Properties.ContainsKey("editor") ? bibTeXEntry.Properties["editor"] : string.Empty);
			proceeding.Publisher = bibTeXEntry.Properties.ContainsKey("publisher") ? bibTeXEntry.Properties["publisher"] : null;
			proceeding.Organization = bibTeXEntry.Properties.ContainsKey("organization") ? bibTeXEntry.Properties["organization"] : null;
			proceeding.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ? bibTeXEntry.Properties["address"] : null;
			proceeding.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return proceeding;
		}

        /// <summary>
        /// Gets the proceedings article resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceProceedingsArticle(BibTeXEntry bibTeXEntry)
		{
			ProceedingsArticle proArticle = new ProceedingsArticle();

			// Required fields: author, title, booktitle, year  
			if (!bibTeXEntry.Properties.ContainsKey("author"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "author", bibTeXEntry.Name, bibTeXEntry.Key }
												 ));
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
												));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("booktitle"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "booktitle", bibTeXEntry.Name, bibTeXEntry.Key }
												));
			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
												));

			BibTeXConverter.ConvertToPersons(proArticle.Authors, bibTeXEntry.Properties["author"]);
			proArticle.Title = bibTeXEntry.Properties["title"];
			proArticle.BookTitle = bibTeXEntry.Properties["booktitle"];

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ?
												 bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (proArticle, year, month, 1);

			// Optional fields: editor, pages, organization, publisher, address, month, note, key
            peopleAdded.UnionWith(proArticle.Authors.OfType<Person>());
			BibTeXConverter.ConvertToPersons(proArticle.Editors,
                bibTeXEntry.Properties.ContainsKey("editor") ? bibTeXEntry.Properties["editor"] : string.Empty);
			proArticle.Pages = bibTeXEntry.Properties.ContainsKey("pages") ? bibTeXEntry.Properties["pages"] : null;
			proArticle.Organization = bibTeXEntry.Properties.ContainsKey("organization") ?
											 bibTeXEntry.Properties["organization"] : null;
			proArticle.Publisher = bibTeXEntry.Properties.ContainsKey("publisher") ?
											 bibTeXEntry.Properties["publisher"] : null;
			proArticle.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ?
											 bibTeXEntry.Properties["address"] : null;
			proArticle.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return proArticle;
		}

        /// <summary>
        /// Gets the technical report resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceTechnicalReport(BibTeXEntry bibTeXEntry)
		{
			TechnicalReport techReport = new TechnicalReport();

			// Required fields: author, title, institution, year
			if (!bibTeXEntry.Properties.ContainsKey("author"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "author", bibTeXEntry.Name, bibTeXEntry.Key }
											));
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
											));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("institution"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "institution", bibTeXEntry.Name, bibTeXEntry.Key }
											));
			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
											));

			BibTeXConverter.ConvertToPersons(techReport.Authors, bibTeXEntry.Properties["author"]);
			techReport.Title = bibTeXEntry.Properties["title"];
			techReport.Institution = bibTeXEntry.Properties["institution"];

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ?
												 bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (techReport, year, month, 1);

			// Optional fields: type, number, address, month, note, key
			// TODO:  for key - yet to decide

			techReport.Category = bibTeXEntry.Properties.ContainsKey("type") ? bibTeXEntry.Properties["type"] : null;
			techReport.Number = bibTeXEntry.Properties.ContainsKey("number") ? bibTeXEntry.Properties["number"] : null;
			techReport.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ?
																			bibTeXEntry.Properties["address"] : null;
			techReport.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return techReport;
		}

        /// <summary>
        /// Gets the Msc resource thesis.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceThesisMsc(BibTeXEntry bibTeXEntry)
		{
			ThesisMsc thesis = new ThesisMsc();

			// Required fields: author, title, school, year
			if (!bibTeXEntry.Properties.ContainsKey("author"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "author", bibTeXEntry.Name, bibTeXEntry.Key }
												));
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
												));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("school"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "school", bibTeXEntry.Name, bibTeXEntry.Key }
												));
			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
												));

			BibTeXConverter.ConvertToPersons(thesis.Authors, bibTeXEntry.Properties["author"]);
			thesis.Title = bibTeXEntry.Properties["title"];
			thesis.Organization = bibTeXEntry.Properties["school"];

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ? bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (thesis, year, month, 1);

			// Optional fields: address, month, note, key
			thesis.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ? bibTeXEntry.Properties["address"] : null;
			thesis.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return thesis;
		}

        /// <summary>
        /// Gets the Phd resource thesis.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceThesisPhD(BibTeXEntry bibTeXEntry)
		{
			ThesisPhD thesisPhd = new ThesisPhD();

			// Required fields: author, title, school, year 
			if (!bibTeXEntry.Properties.ContainsKey("author"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "author", bibTeXEntry.Name, bibTeXEntry.Key }
												));

			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
												));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("school"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "school", bibTeXEntry.Name, bibTeXEntry.Key }
												));
			if (!bibTeXEntry.Properties.ContainsKey("year"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												new object[] { "year", bibTeXEntry.Name, bibTeXEntry.Key }
												));

			BibTeXConverter.ConvertToPersons(thesisPhd.Authors, bibTeXEntry.Properties["author"]);
			thesisPhd.Title = bibTeXEntry.Properties["title"];
			thesisPhd.Organization = bibTeXEntry.Properties["school"];

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties["year"], out year))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ?
												 bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (thesisPhd, year, month, 1);

			// Optional fields: address, month, note, key
			thesisPhd.PublisherAddress = bibTeXEntry.Properties.ContainsKey("address") ?
																	bibTeXEntry.Properties["address"] : null;
			thesisPhd.Notes = bibTeXEntry.Properties.ContainsKey("note") ? bibTeXEntry.Properties["note"] : null;

			return thesisPhd;
		}

        /// <summary>
        /// Gets the unpublished resource.
        /// </summary>
        /// <param name="bibTeXEntry">The bib teX entry.</param>
        /// <returns>The <see cref="ScholarlyWork"/>.</returns>
        private static ScholarlyWork GetResourceUnpublished(BibTeXEntry bibTeXEntry)
		{
			Unpublished book = new Unpublished();

			// Required fields: author, title, note
			if (!bibTeXEntry.Properties.ContainsKey("author"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "author", bibTeXEntry.Name, bibTeXEntry.Key }
												));
			if (!bibTeXEntry.Properties.ContainsKey("title"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "title", bibTeXEntry.Name, bibTeXEntry.Key }
												));

            // Title's value is required according to update in design
            if (String.IsNullOrEmpty(bibTeXEntry.Properties["title"]))
                throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                 Properties.Resources.BIBTEXCONVERTER_MISSING_TITLE_VALUE, new object[] { bibTeXEntry.Name, bibTeXEntry.Key }));

			if (!bibTeXEntry.Properties.ContainsKey("note"))
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												  Properties.Resources.BIBTEXCONVERTER_MISSING_REQUIRED_FIELD,
												 new object[] { "note", bibTeXEntry.Name, bibTeXEntry.Key }
												));

			BibTeXConverter.ConvertToPersons(book.Authors, bibTeXEntry.Properties["author"]);
			book.Title = bibTeXEntry.Properties["title"];
			book.Notes = bibTeXEntry.Properties["note"];

			// Optional fields: month, year, key
			//TODO: yet to decide about key

			int year = DateTime.Now.Year;
			int month = 1;
			if (!Int32.TryParse(bibTeXEntry.Properties.ContainsKey("year") ?
								bibTeXEntry.Properties["year"] : string.Empty, out year))
				year = DateTime.Now.Year;
			if (year <= 0)
				throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_YEAR,
															   new object[] { bibTeXEntry.Properties["year"], bibTeXEntry.Name, bibTeXEntry.Key }
															   ));
			if (!BibTeXConverter.TryParseMonth(bibTeXEntry.Properties.ContainsKey("month") ?
												bibTeXEntry.Properties["month"] : string.Empty, out month))
				month = 1;

			SetPublishedDate (book, year, month, 1);

			return book;
		}

        /// <summary>
        /// Builds up a collection of Person object from string value.
        /// </summary>
        /// <param name="outputCollection">The collection which would be updated</param>
        /// <param name="value">Value from which Person objects will be built</param>
        private static void ConvertToPersons(EntityCollection<Contact> outputCollection, string value)
        {
            if(outputCollection == null)
                throw new ArgumentNullException("outputCollection");
            if(value == null)
                throw new ArgumentNullException("value");

            string[] authorsList = value.Split(new string[] { " and ", " AND " }, StringSplitOptions.RemoveEmptyEntries);
            foreach(String authorFullName in authorsList)
            {
                BibTeXAuthorNameParser authorName = new BibTeXAuthorNameParser(authorFullName);
                authorName.Parse();

                Func<Person, bool> predicate = GetPersonsonFilterExpression(authorName.FirstName, 
                    authorName.LastName, authorName.MiddleName);

                Person person = null;
                person = peopleAdded.FirstOrDefault(predicate);

                if(null == person)
                {
                    person = new Person
                    {
                        Title = authorFullName.Trim(),
                        FirstName = authorName.FirstName,
                        LastName = authorName.LastName,
                        MiddleName = authorName.MiddleName
                    };
                }
                outputCollection.Add(person);
            }
        }

        /// <summary>
        /// Gets the personson filter expression.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="middleName">Name of the middle.</param>
        /// <returns>The predicate function call.</returns>
        private static Func<Person, bool> GetPersonsonFilterExpression(
                                                                string firstName, 
                                                                string lastName, 
                                                                string middleName)
        {
            System.Linq.Expressions.ParameterExpression param = System.Linq.Expressions.Expression.Parameter(typeof(Person), "person");
            System.Linq.Expressions.Expression filter = null;
            AtomPubHelper.GeneratePersonFilterExpression("FirstName", firstName, param, ref filter);
            AtomPubHelper.GeneratePersonFilterExpression("MiddleName", middleName, param, ref filter);
            AtomPubHelper.GeneratePersonFilterExpression("LastName", lastName, param, ref filter);
            Func<Person, bool> predicate = null;

            if(null != filter)
            {
                System.Linq.Expressions.Expression<Func<Person, bool>> predicateExpression = System.Linq.Expressions.Expression.Lambda<Func<Person, bool>>(filter, param);
                predicate = predicateExpression.Compile();
            }

            return predicate;
        }

		/// <summary>
		/// Converts string to corresponding Data Month
		/// </summary>
		/// <param name="month">String to convert</param>
		/// <param name="converted">Converted month</param>
		/// <returns>Returns true if parsed successfully</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability",
			"CA1502:AvoidExcessiveComplexity")]
		private static bool TryParseMonth(string month, out int converted)
		{
			switch (month.Trim().ToUpperInvariant())
			{
				case "1":
				case "01":
				case "JAN":
				case "JANUARY":
					converted = 1;
					break;
				case "2":
				case "02":
				case "FEB":
				case "FEBRUARY":
					converted = 2;
					break;
				case "3":
				case "03":
				case "MAR":
				case "MARCH":
					converted = 3;
					break;
				case "4":
				case "04":
				case "APR":
				case "APRIL":
					converted = 4;
					break;
				case "5":
				case "05":
				case "MAY":
					converted = 5;
					break;
				case "6":
				case "06":
				case "JUN":
				case "JUNE":
					converted = 6;
					break;
				case "7":
				case "07":
				case "JUL":
				case "JULY":
					converted = 7;
					break;
				case "8":
				case "08":
				case "AUG":
				case "AUGUST":
					converted = 8;
					break;
				case "9":
				case "09":
				case "SEP":
				case "SEPTEMBER":
					converted = 9;
					break;
				case "10":
				case "OCT":
				case "OCTOBER":
					converted = 10;
					break;
				case "11":
				case "NOV":
				case "NOVEMBER":
					converted = 11;
					break;
				case "12":
				case "DEC":
				case "DECEMBER":
					converted = 12;
					break;
				case "":
					converted = 0;
					return false;
				default:
					throw new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture, Properties.Resources.BIBTEXCONVERTER_INVALID_MONTH,
																   new object[] { month }
																   ));
			}
			return true;

		}

		#endregion

		/// <summary>
		/// Writes a collection of Resource objects to the output stream.
		/// </summary>
		/// <param name="resources">Collection of Resource objects to be written.</param>
		/// <param name="outputStream">Output stream.</param>
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
		///      class Program
		///      {
		///           static void Main(string[] args)
		///           {
        ///                // Initialize instance of Person class
		///                Person firstPerson = new Person();
		///                firstPerson.FirstName = "Robert";
		///                firstPerson.MiddleName = "Almond";
		///                firstPerson.LastName = "Dicosta";
		///                    
        ///                // Initialize instance of Publication class
		///                Publication publication = new Publication();
		///                publication.Authors.Add(firstPerson);
		///                publication.Title = "BibTex Publication : Title";
		///                publication.Notes = "BibTex Publication : Note"; ;
		///                publication.MonthPublished = 3;
		///                publication.YearPublished = 2008;
		///                
		///                using (Stream outputStream = (new FileStream(@"..\..\SampleBibTexFile.bib", FileMode.OpenOrCreate,
		///                                              FileAccess.ReadWrite)))
		///                {
		///                      BibTeXConverter converter = new BibTeXConverter(BibTeXParserBehavior.IgnoreParseErrors);
		///                      List&lt;Resource&gt; resourceList = new List&lt;Resource&gt;();
		///                      resourceList.Add(publication);
		///                      IEnumerable&lt;Resource&gt; resourceCollection = resourceList;
		///                      converter.Export(resourceCollection, outputStream);  
		///                }
		///           }
		///      }    
		/// }
		/// </code>
		/// </example>
		public void Export(IEnumerable<ScholarlyWork> resources, Stream outputStream)
		{
			// TODO: Howpublished and key field name are ignored for Export functionality
			if (resources == null)
				throw new ArgumentNullException("resources");

			if (outputStream == null)
				throw new ArgumentNullException("outputStream");

			if (!outputStream.CanWrite)
				throw new ArgumentException("outputStream does not support write", "outputStream");

			this.bibTeXDocument = new BibTeXDocument();
			this.mappingErrors.Clear();

			foreach (ScholarlyWork resource in resources)
			{
				BibTeXEntry bibEntry = new BibTeXEntry();

				Book book = resource as Book;
				if (book != null)
				{
					bibEntry = GetBibTeXEntry(book);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				Booklet booklet = resource as Booklet;
				if (booklet != null)
				{
					bibEntry = GetBibTeXEntry(booklet);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				Chapter chapter = resource as Chapter;
				if (chapter != null)
				{
					bibEntry = GetBibTeXEntry(chapter);
                    this.bibTeXDocument.Add(bibEntry);
                    continue;
				}

				JournalArticle journalArticle = resource as JournalArticle;
				if (journalArticle != null)
				{
					bibEntry = GetBibTeXEntry(journalArticle);
                    this.bibTeXDocument.Add(bibEntry);
                    continue;
				}

				Manual manual = resource as Manual;
				if (manual != null)
				{
					bibEntry = GetBibTeXEntry(manual);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				ProceedingsArticle proceedingsArticle = resource as ProceedingsArticle;
				if (proceedingsArticle != null)
				{
					bibEntry = GetBibTeXEntry(proceedingsArticle);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				Proceedings proceedings = resource as Proceedings;
				if (proceedings != null)
				{
					bibEntry = GetBibTeXEntry(proceedings);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				TechnicalReport technicalReport = resource as TechnicalReport;
				if (technicalReport != null)
				{
					bibEntry = GetBibTeXEntry(technicalReport);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				ThesisMsc thesisMsc = resource as ThesisMsc;
				if (thesisMsc != null)
				{
					bibEntry = GetBibTeXEntry(thesisMsc);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				ThesisPhD thesisPhD = resource as ThesisPhD;
				if (thesisPhD != null)
				{
					bibEntry = GetBibTeXEntry(thesisPhD);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				Unpublished unpublished = resource as Unpublished;
				if (unpublished != null)
				{
					bibEntry = GetBibTeXEntry(unpublished);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				Publication publication = resource as Publication;
				if (publication != null)
				{
					bibEntry = GetBibTeXEntry(publication);
					this.bibTeXDocument.Add(bibEntry);
					continue;
				}

				//make a note and skip adding to the collection
				this.mappingErrors.Add(new BibTeXMappingException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
												 Properties.Resources.RESOURCE_TYPE_NOT_SUPPORTED,
												resource.ToString()
												)));

			}

			this.bibTeXDocument.Save(outputStream);

			return;
		}

		/// <summary>
		/// Writes the collection of Resource object to the file specified.
		/// </summary>
		/// <param name="resources">Collection of Resource objects to be written.</param>
		/// <param name="filePath">Full path of the file to be written.</param>
		/// <param name="overwrite">if true, overwrites any existing file.</param>
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
		///      class Program
		///      {
		///           static void Main(string[] args)
		///           {
        ///                // Initialize instance of Person class
		///                Person firstPerson = new Person();
		///                firstPerson.FirstName = "Robert";
		///                firstPerson.MiddleName = "Almond";
		///                firstPerson.LastName = "Dicosta";
		///                    
        ///                // Initialize instance of Publication class
		///                Publication publication = new Publication();
		///                publication.Authors.Add(firstPerson);
		///                publication.Title = "BibTex Publication : Title";
		///                publication.Notes = "BibTex Publication : Note"; ;
		///                publication.MonthPublished = 3;
		///                publication.YearPublished = 2008;
		///                
		///                BibTeXConverter converter = new BibTeXConverter(BibTeXParserBehavior.IgnoreParseErrors);
		///                string filePath = @"..\..\SampleBibTexFile.bib";
		///                bool overwriteExisting=true;
		///                List&lt;Resource&gt; resourceList = new List&lt;Resource&gt;();
		///                resourceList.Add(publication);
		///                IEnumerable&lt;Resource&gt; resourceCollection = resourceList;
		///                converter.Export(resourceCollection, filePath, overwriteExisting);  
		///           }
		///      }    
		/// }
		/// </code>
		/// </example>
		public void Export(IEnumerable<ScholarlyWork> resources, String filePath, bool overwrite)
		{
			if (resources == null)
				throw new ArgumentNullException("resources");
 
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
			using (FileStream fileStream = new FileStream(filePath, mode, FileAccess.Write, FileShare.None))
			{
				this.Export(resources, fileStream);
			}
		}

		#region Private Export Helper Method

		/// <summary>
		/// Create BibTeX entry for resource type Publication.
		/// </summary>
		/// <param name="publication">Instance of resource type Publication</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(Publication publication)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "misc";
            bibEntry.Key = publication.Id.ToString("D");

			// Optional fields: author, title, howpublished, month, year, note, key 
			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = publication.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			bibEntry.Properties.Add("title", publication.Title);
			bibEntry.Properties.Add("note", publication.Notes);
			bibEntry.Properties.Add("month", publication.MonthPublished.ToString());
			bibEntry.Properties.Add("year", publication.YearPublished.ToString());
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type Book.
		/// </summary>
		/// <param name="book">Instance of resource type Book</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(Book book)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "book";
            bibEntry.Key = book.Id.ToString("D");


			// Required fields: author/editor, title, publisher, year 

			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = book.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			bibEntry.Properties.Add("title", book.Title);

			System.Data.Objects.DataClasses.EntityCollection<Contact> editorCollection = book.Editors;
			string editors = ConvertToBibTeXFormat(editorCollection);
			bibEntry.Properties.Add("editor", editors);

			bibEntry.Properties.Add("publisher", book.Publisher);
			bibEntry.Properties.Add("year", book.YearPublished.ToString());

			// Optional fields : volume, series, address, edition, month, note, key 
			bibEntry.Properties.Add("volume", book.Volume);
			bibEntry.Properties.Add("series", book.Series);
			bibEntry.Properties.Add("address", book.PublisherAddress);
			bibEntry.Properties.Add("edition", book.Edition);
			bibEntry.Properties.Add("month", book.MonthPublished.ToString());
			bibEntry.Properties.Add("note", book.Notes);

			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type Booklet.
		/// </summary>
		/// <param name="booklet">Instance of resource type Booklet</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(Booklet booklet)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "booklet";
            bibEntry.Key = booklet.Id.ToString("D");

			// Required fields: title 
			bibEntry.Properties.Add("title", booklet.Title);

			// Optional fields: author, howpublished, address, month, year, note, key
			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = booklet.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			bibEntry.Properties.Add("address", booklet.PublisherAddress);
			bibEntry.Properties.Add("month", booklet.MonthPublished.ToString());
			bibEntry.Properties.Add("year", booklet.YearPublished.ToString());
			bibEntry.Properties.Add("note", booklet.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type Chapter.
		/// </summary>
		/// <param name="chapter">Instance of resource type Chapter</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(Chapter chapter)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "inbook";
            bibEntry.Key = chapter.Id.ToString("D");

			// Required fields: author/editor, title, chapter/pages, publisher, year  
			string authors = ConvertToBibTeXFormat(chapter.Authors);
			bibEntry.Properties.Add("author", authors);

			string editors = ConvertToBibTeXFormat(chapter.Authors);
			bibEntry.Properties.Add("editor", editors);

			bibEntry.Properties.Add("title", chapter.Title);
			bibEntry.Properties.Add("chapter", chapter.Chapter);
			bibEntry.Properties.Add("pages", chapter.Pages);
			bibEntry.Properties.Add("publisher", chapter.Publisher);
			bibEntry.Properties.Add("year", chapter.YearPublished.ToString());

			// Optional fields: volume, series, address, edition, month, note, key 
			bibEntry.Properties.Add("volume", chapter.Volume);
			bibEntry.Properties.Add("series", chapter.Series);
			bibEntry.Properties.Add("address", chapter.PublisherAddress);
			bibEntry.Properties.Add("edition", chapter.Edition);
			bibEntry.Properties.Add("month", chapter.MonthPublished.ToString());
			bibEntry.Properties.Add("note", chapter.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type JournalArticle.
		/// </summary>
		/// <param name="journalArticle">Instance of resource type JournalArticle</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(JournalArticle journalArticle)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "article";
            bibEntry.Key = journalArticle.Id.ToString("D");

			// Required fields: author, title, journal, year 
			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = journalArticle.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			bibEntry.Properties.Add("title", journalArticle.Title);
			bibEntry.Properties.Add("journal", journalArticle.Journal);
			bibEntry.Properties.Add("year", journalArticle.YearPublished.ToString());

			// Optional fields: volume, number, pages, month, note, key
			bibEntry.Properties.Add("volume", journalArticle.Volume);
			bibEntry.Properties.Add("number", journalArticle.Number);
			bibEntry.Properties.Add("pages", journalArticle.Pages);
			bibEntry.Properties.Add("month", journalArticle.MonthPublished.ToString());
			bibEntry.Properties.Add("note", journalArticle.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type Manual.
		/// </summary>
		/// <param name="manual">Instance of resource type Manual</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(Manual manual)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "manual";
            bibEntry.Key = manual.Id.ToString("D");

			// Required fields: title 
			bibEntry.Properties.Add("title", manual.Title);

			// Optional fields: author, organization, address, edition, month, year, note, key
			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = manual.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			bibEntry.Properties.Add("organization", manual.Organization);
			bibEntry.Properties.Add("address", manual.PublisherAddress);
			bibEntry.Properties.Add("edition", manual.Edition);
			bibEntry.Properties.Add("month", manual.MonthPublished.ToString());
			bibEntry.Properties.Add("year", manual.YearPublished.ToString());
			bibEntry.Properties.Add("note", manual.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type Proceedings.
		/// </summary>
		/// <param name="proceedings">Instance of resource type Proceedings</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(Proceedings proceedings)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "proceedings";
            bibEntry.Key = proceedings.Id.ToString("D");

			// Required fields : title, year
			bibEntry.Properties.Add("title", proceedings.Title);
			bibEntry.Properties.Add("year", proceedings.YearPublished.ToString());

			// Optional fields : editor, publisher, organization, address, month, note, key
			System.Data.Objects.DataClasses.EntityCollection<Contact> editorCollection = proceedings.Editors;
			string editors = ConvertToBibTeXFormat(editorCollection);
			bibEntry.Properties.Add("editor", editors);

			bibEntry.Properties.Add("publisher", proceedings.Publisher);
			bibEntry.Properties.Add("address", proceedings.PublisherAddress);
			bibEntry.Properties.Add("organization", proceedings.Organization);
			bibEntry.Properties.Add("month", proceedings.MonthPublished.ToString());
			bibEntry.Properties.Add("note", proceedings.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type ProceedingsArticle.
		/// </summary>
		/// <param name="proceedingsArticle">Instance of resource type ProceedingsArticle</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(ProceedingsArticle proceedingsArticle)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "inproceedings";
            bibEntry.Key = proceedingsArticle.Id.ToString("D");

			// Required fields :  author, title, booktitle, year 
			bibEntry.Properties.Add("title", proceedingsArticle.Title);
			bibEntry.Properties.Add("year", proceedingsArticle.YearPublished.ToString());
			bibEntry.Properties.Add("booktitle", proceedingsArticle.BookTitle);

            string authors = ConvertToBibTeXFormat(proceedingsArticle.Authors);
			bibEntry.Properties.Add("author", authors);

			// Optional fields : editor, pages, organization, publisher, address, month, note, key
			string editors = ConvertToBibTeXFormat(proceedingsArticle.Editors);
			bibEntry.Properties.Add("editor", editors);

			bibEntry.Properties.Add("pages", proceedingsArticle.Pages);
			bibEntry.Properties.Add("address", proceedingsArticle.PublisherAddress);
			bibEntry.Properties.Add("organization", proceedingsArticle.Organization);
			bibEntry.Properties.Add("month", proceedingsArticle.MonthPublished.ToString());
			bibEntry.Properties.Add("publisher", proceedingsArticle.Publisher);
			bibEntry.Properties.Add("note", proceedingsArticle.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type TechnicalReport.
		/// </summary>
		/// <param name="technicalReport">Instance of resource type TechnicalReport</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(TechnicalReport technicalReport)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "techreport";
            bibEntry.Key = technicalReport.Id.ToString("D");

			// Required fields :   author, title, institution, year 
			bibEntry.Properties.Add("title", technicalReport.Title);
			bibEntry.Properties.Add("year", technicalReport.YearPublished.ToString());
			bibEntry.Properties.Add("institution", technicalReport.Institution);

			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = technicalReport.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			// Optional fields : type, number, address, month, note, key
			// type map to Category as according to excel work book.
			bibEntry.Properties.Add("type", technicalReport.Category);
			bibEntry.Properties.Add("number", technicalReport.Number);
			bibEntry.Properties.Add("address", technicalReport.PublisherAddress);
			bibEntry.Properties.Add("month", technicalReport.MonthPublished.ToString());
			bibEntry.Properties.Add("note", technicalReport.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type ThesisMsc.
		/// </summary>
		/// <param name="thesisMsc">Instance of resource type ThesisMsc</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(ThesisMsc thesisMsc)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "mastersthesis";
            bibEntry.Key = thesisMsc.Id.ToString("D");

			// Required fields : author, title, school, year 
			bibEntry.Properties.Add("title", thesisMsc.Title);
			bibEntry.Properties.Add("year", thesisMsc.YearPublished.ToString());
			bibEntry.Properties.Add("school", thesisMsc.Organization);

			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = thesisMsc.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			// Optional fields : address, month, note, key
			bibEntry.Properties.Add("address", thesisMsc.PublisherAddress);
			bibEntry.Properties.Add("month", thesisMsc.MonthPublished.ToString());
			bibEntry.Properties.Add("note", thesisMsc.Notes);
			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type ThesisPhD.
		/// </summary>
		/// <param name="thesisPhD">Instance of resource type ThesisPhD</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(ThesisPhD thesisPhD)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "phdthesis";
            bibEntry.Key = thesisPhD.Id.ToString("D");

			// Required fields : author, title, school, year 
			bibEntry.Properties.Add("title", thesisPhD.Title);
			bibEntry.Properties.Add("year", thesisPhD.YearPublished.ToString());
			bibEntry.Properties.Add("school", thesisPhD.Organization);

			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = thesisPhD.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			// Optional fields : address, month, note, key
			bibEntry.Properties.Add("address", thesisPhD.PublisherAddress);
			bibEntry.Properties.Add("month", thesisPhD.MonthPublished.ToString());
			bibEntry.Properties.Add("note", thesisPhD.Notes);

			return bibEntry;
		}

		/// <summary>
		/// Create BibTeX entry for resource type Unpublished.
		/// </summary>
		/// <param name="unpublished">Instance of resource type Unpublished</param>
		/// <returns>Returns a instance of BibTeXEntry class</returns>
		private static BibTeXEntry GetBibTeXEntry(Unpublished unpublished)
		{
			BibTeXEntry bibEntry = new BibTeXEntry();
			bibEntry.Name = "unpublished";
            bibEntry.Key = unpublished.Id.ToString("D");

			// Required fields : author, title, note
			bibEntry.Properties.Add("title", unpublished.Title);
			bibEntry.Properties.Add("note", unpublished.Notes);

			System.Data.Objects.DataClasses.EntityCollection<Contact> authorCollection = unpublished.Authors;
			string authors = ConvertToBibTeXFormat(authorCollection);
			bibEntry.Properties.Add("author", authors);

			// Optional fields : month, year, key
			bibEntry.Properties.Add("year", unpublished.YearPublished.ToString());
			bibEntry.Properties.Add("month", unpublished.MonthPublished.ToString());
			return bibEntry;
		}

		/// <summary>
		/// Convert Authors and Editors collection to BibTeX format
		/// </summary>
		/// <param name="personCollection">Collection of person to be formatted</param>
		/// <returns>Returns formatted string</returns>
		private static string ConvertToBibTeXFormat(
            System.Data.Objects.DataClasses.EntityCollection<Contact> personCollection)
		{
			StringBuilder personInfo = new StringBuilder();
			int personCount = 1;
			foreach (Contact contact in personCollection)
			{
                Person person = contact as Person;
				if (personCollection.Count != personCount)
				{
                    if(person != null)
					    personInfo.Append(person.FirstName + Properties.Resources.SPACE +
									      person.MiddleName + Properties.Resources.SPACE +
									      person.LastName + Properties.Resources.AND);
                    else
                        personInfo.Append(contact.Title + Properties.Resources.AND);
				}
				else
				{
                    if (person != null)
                        personInfo.Append(person.FirstName + Properties.Resources.SPACE +
                            person.MiddleName + Properties.Resources.SPACE + person.LastName);
                    else
                        personInfo.Append(contact.Title);
				}
				personCount++;

			}
			return personInfo.ToString();
		}

		#endregion

	}
}
