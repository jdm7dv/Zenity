// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Core
{
    using System.Globalization;

    /// <example>
    /// This example shows how to create a simple zentity file and upload content to it. 
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using System.IO;
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            Guid fileId = Guid.Empty;
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                // Create a Zentity file.    
    ///                Zentity.Core.File file = new Zentity.Core.File();
    ///                file.Uri = &quot;urn:zentity-samples:file:lecture-mp3&quot;;
    ///
    ///                // Add the file to context.    
    ///                context.AddToResources(file);
    ///
    ///                // Save off the id.
    ///                fileId = file.Id;
    ///
    ///                // Save the context.    
    ///                context.SaveChanges();
    ///
    ///                // Now upload the actual binary content of the file.    
    ///                using (MemoryStream stream = new MemoryStream())
    ///                {
    ///                    StreamWriter writer = new StreamWriter(stream);
    ///                    writer.AutoFlush = true;
    ///                    string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
    ///                    writer.Write(content);
    ///                    stream.Seek(0, SeekOrigin.Begin);
    ///                    context.UploadFileContent(file, stream);
    ///                    Console.WriteLine(&quot;Created File resource with content: [{0}]&quot;, content);
    ///                }
    ///            }
    ///
    ///            // Fetch the file details from the repository.  
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
    ///                using (MemoryStream stream = new MemoryStream())
    ///                {
    ///                    context.DownloadFileContent((Zentity.Core.File)file, stream);
    ///                    stream.Seek(0, SeekOrigin.Begin);
    ///                    StreamReader reader = new StreamReader(stream);
    ///                    Console.WriteLine(&quot;Downloaded content: [{0}]&quot;, reader.ReadToEnd());
    ///                }
    ///            }
    ///
    ///            // Update the file content.
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
    ///                using (MemoryStream stream = new MemoryStream())
    ///                {
    ///                    StreamWriter writer = new StreamWriter(stream);
    ///                    writer.AutoFlush = true;
    ///                    string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
    ///                    writer.Write(content);
    ///                    stream.Seek(0, SeekOrigin.Begin);
    ///                    context.UploadFileContent(file, stream);
    ///                    Console.WriteLine(&quot;Uploaded new content: [{0}]&quot;, content);
    ///                }
    ///            }
    ///
    ///            // Download new content.  
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
    ///                using (MemoryStream stream = new MemoryStream())
    ///                {
    ///                    context.DownloadFileContent((Zentity.Core.File)file, stream);
    ///                    stream.Seek(0, SeekOrigin.Begin);
    ///                    StreamReader reader = new StreamReader(stream);
    ///                    Console.WriteLine(&quot;Downloaded new content: [{0}]&quot;, reader.ReadToEnd());
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class File
    {
        /// <summary>
        /// Called when changing the checksum.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnChecksumChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.FileChecksum)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Checksum,
                    MaxLengths.FileChecksum));
        }

        /// <summary>
        /// Called when MIME type changing.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnMimeTypeChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.FileMimeType)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.MimeType,
                    MaxLengths.FileMimeType));
        }

        /// <summary>
        /// Called when file extension changing.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnFileExtensionChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.FileFileExtension)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.FileExtension, 
                    MaxLengths.FileFileExtension));
        }
    }
}
