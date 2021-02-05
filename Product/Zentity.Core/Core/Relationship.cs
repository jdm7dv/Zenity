// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Core
{
    using System;

    /// <summary>Adds functionality and handles events for the Relationship entity class.</summary>
    /// <example>
    /// This example shows how to create resource to resource relationships.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        static Guid lectureId = Guid.Empty;
    ///
    ///        public static void Main(string[] args)
    ///        {
    ///            string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res://Zentity.Core;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///
    ///            // Creating explicit relationships.
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Predicate resourceHasFile = context.Predicates.
    ///                    Where(tuple =&gt; tuple.Name == &quot;ResourceHasFile&quot;).First();
    ///
    ///                // Create resources.
    ///                Resource lecture = new Resource();
    ///                lecture.Uri = &quot;urn:zentity-samples:aLecture&quot;;
    ///                lectureId = lecture.Id;
    ///                context.AddToResources(lecture);
    ///
    ///                File lectureContent = new File();
    ///                lectureContent.Uri = &quot;urn:zentity-samples:aSlideDeck&quot;;
    ///
    ///                // Create relationship.
    ///                Relationship explicitRelationship = new Relationship
    ///                {
    ///                    Subject = lecture,
    ///                    Object = lectureContent,
    ///                    Predicate = resourceHasFile
    ///                };
    ///
    ///                context.AddToResources(lecture);
    ///
    ///                // Save the context.     
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created relationship between lecture and file.&quot;);
    ///
    ///                // Retrieve relationships.
    ///                Console.WriteLine(&quot;Retrieving relationships for lecture.&quot;);
    ///                foreach (Relationship rel in lecture.RelationshipsAsSubject)
    ///                    Console.WriteLine(&quot;\t{0} &lt;--ResourceHasFile--&gt; {1}&quot;, rel.Subject.Id,
    ///                        rel.Object.Id);
    ///
    ///                // Since, ResourceHasFile is an association predicate, we can also use
    ///                // navigation properties on resource types to retrieve related resources.
    ///                lecture.Files.Load();
    ///                Console.WriteLine(&quot;Printing files for lecture [{0}]&quot;, lecture.Id);
    ///                foreach (File f in lecture.Files)
    ///                    Console.WriteLine(&quot;\t{0}&quot;, f.Id);
    ///
    ///                // Delete relationship.
    ///                context.DeleteObject(explicitRelationship);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Deleted relationship between lecture and file.&quot;);
    ///
    ///                // The relationship collections for the resources should be empty now.
    ///                Console.WriteLine(&quot;Lecture participates in {0} relationships.&quot;,
    ///                    lecture.RelationshipsAsSubject.Count());
    ///                Console.WriteLine(&quot;File participates in {0} relationships&quot;,
    ///                    lectureContent.RelationshipsAsObject.Count());
    ///
    ///                lectureId = lecture.Id;
    ///            }
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Resource lecture = context.Resources.Where(tuple =&gt; tuple.Id == lectureId).First();
    ///                lecture.Files.Load();
    ///                Console.WriteLine(&quot;Lecture has {0} files.&quot;, lecture.Files.Count());
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Relationship
    {
        /// <summary>
        /// Initializes a new instance of Relationship class with no arguements.
        /// </summary>
        public Relationship()
        {
            // Automatically create a new id.
            this._Id = Guid.NewGuid();
        }
    }
}
