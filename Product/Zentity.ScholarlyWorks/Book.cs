// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Example below shows simple creation of a Book resource.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using Zentity.ScholarlyWorks;
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res://Zentity.ScholarlyWorks;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///
    ///            Guid bookId = Guid.Empty;
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a book. 
    ///                Book bookObject = new Book();
    ///                bookObject.Uri = &quot;urn:zentity-samples:book:my-ebook&quot;;
    ///                bookObject.Abstract = &quot;This is a sample e-book uploaded to Zentity database.&quot;;
    ///
    ///                // Save off the Id for future references. 
    ///                bookId = bookObject.Id;
    ///
    ///                // Create an author. 
    ///                Person author = new Person();
    ///                author.Title = &quot;The author&quot;;
    ///                bookObject.Authors.Add(author);
    ///                bookObject.BookTitle = &quot;Some title&quot;;
    ///
    ///                //Create a file object to upload the book contents.
    ///                Zentity.Core.File bookContents = new Zentity.Core.File();
    ///                bookContents.Uri = &quot;urn:zentity-samples:file:my-ebook&quot;;
    ///
    ///                //Create association between the book object and the data file object.
    ///                bookObject.Files.Add(bookContents);
    ///
    ///                context.AddToResources(bookObject);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created resources, Book, Person and File.&quot;);
    ///
    ///                //Retrieve all book resources from the repository.
    ///                Console.WriteLine(&quot;All books present in the repository:&quot;);
    ///                foreach (Book b in context.Books())
    ///                    Console.WriteLine(&quot;\t\t{0}&quot;, b.BookTitle);
    ///
    ///                // Upload book contents.
    ///                string filePath = @&quot;C:\Books\EBook.pdf&quot;;
    ///                context.UploadFileContent(bookContents, filePath);
    ///                Console.WriteLine(&quot;Uploaded book contents from path: [{0}]&quot;, filePath);
    ///            }
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                //Download book contents.  
    ///                Book b = context.Books().Include(&quot;Files&quot;).
    ///                    Where(tuple =&gt; tuple.Id == bookId).FirstOrDefault();
    ///                Zentity.Core.File content = b.Files.FirstOrDefault();
    ///                string filePath = @&quot;C:\Books\CopyOfEBook.pdf&quot;;
    ///                context.DownloadFileContent(content, filePath, true);
    ///                Console.WriteLine(&quot;Downloaded book content at path: [{0}]&quot;, filePath);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Book
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnChangeHistoryChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.changeHistory)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.ChangeHistory, MaxLengths.changeHistory));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnISBNChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.isbn)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.ISBN, MaxLengths.isbn));
        }
    }
}
