// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>The example below shows how to create an Audio resource. Note that the
    /// metadata is obtained from ScholarlyWorks assembly. The metadata in this assembly
    /// contains resource types of Core and ScholarlyWorks modules.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using System.IO;
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
    ///            Guid audioId = Guid.Empty;
    ///
    ///            // NOTE: The metadata path is pointing to the 
    ///            // ScholarlyWorks assembly in the connection string.
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a file.    
    ///                Zentity.Core.File audioContent = new Zentity.Core.File();
    ///                audioContent.Uri = &quot;urn:zentity-samples:file:lecture-mp3&quot;;
    ///
    ///                // Create an audio asset.    
    ///                Audio lectureMp3 = new Audio();
    ///                lectureMp3.Uri = &quot;urn:zentity-samples:audio:anAudio&quot;;
    ///                lectureMp3.BitRate = 320 * 1024;
    ///
    ///                // Save off the audio id. 
    ///                audioId = lectureMp3.Id;
    ///
    ///                // Associate file with audio.
    ///                lectureMp3.Files.Add(audioContent);
    ///
    ///                // Add audio to context.    
    ///                context.AddToResources(lectureMp3);
    ///
    ///                // Save the context.    
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created resources, Audio and File.&quot;);
    ///
    ///                // Now upload the actual binary content of the file.    
    ///                using (MemoryStream stream = new MemoryStream())
    ///                {
    ///                    StreamWriter writer = new StreamWriter(stream);
    ///                    writer.AutoFlush = true;
    ///                    string content = &quot;Audio Content&quot;;
    ///                    writer.Write(content);
    ///                    stream.Seek(0, SeekOrigin.Begin);
    ///                    context.UploadFileContent(audioContent, stream);
    ///                    Console.WriteLine(&quot;Uploaded content: [{0}]&quot;, content);
    ///                }
    ///            }
    ///
    ///            // Fetch the audio resource from the repository.  
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Audio lecture = context.Audios().Include(&quot;Files&quot;).
    ///                    Where(tuple =&gt; tuple.Id == audioId).FirstOrDefault();
    ///                Zentity.Core.File containedResource = lecture.Files.FirstOrDefault();
    ///                using (MemoryStream stream = new MemoryStream())
    ///                {
    ///                    context.DownloadFileContent((Zentity.Core.File)containedResource, stream);
    ///                    stream.Seek(0, SeekOrigin.Begin);
    ///                    StreamReader reader = new StreamReader(stream);
    ///                    Console.WriteLine(&quot;Downloaded content: [{0}]&quot;, reader.ReadToEnd());
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Audio
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnCodecChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.codec)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Codec, MaxLengths.codec));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnModeChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.mode)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Mode, MaxLengths.mode));
        }
    }
}
