// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Administration
{
    /// <summary>Add functionality to RelationshipChange entity class.</summary>
    /// <example>This example shows creation and deletion of <see cref="Zentity.Core.Relationship"/> 
    /// objects in the repository and how to retrieve the changes. Use 
    /// <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
    /// to enable the change history feature if not enabled already. 
    /// <code>
    ///using System;
    ///using System.Linq;
    ///using Zentity.Administration;
    ///using Zentity.Core;
    ///using System.Threading;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            Guid resourceId = Guid.Empty;
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Resource pub = new Resource
    ///                {
    ///                    Title = &quot;sample Resource&quot;,
    ///                    Uri = &quot;urn:zentity-samples:pub1&quot;
    ///                };
    ///
    ///                Resource author1 = new Resource { Title = &quot;Alice&quot; };
    ///                Resource author2 = new Resource { Title = &quot;Bob&quot; };
    ///                Resource author3 = new Resource { Title = &quot;Charlie&quot; };
    ///
    ///                // Create some relationships.
    ///                Predicate authoredBy = new Predicate { Name = &quot;Author&quot;, Uri = Guid.NewGuid().ToString(&quot;N&quot;) };
    ///                Relationship rel1 = new Relationship { Subject = pub, Object = author1, Predicate = authoredBy };
    ///                Relationship rel2 = new Relationship { Subject = pub, Object = author2, Predicate = authoredBy };
    ///                Relationship rel3 = new Relationship { Subject = pub, Object = author3, Predicate = authoredBy };
    ///
    ///                context.AddToResources(pub);
    ///                context.SaveChanges();
    ///
    ///                // Remove some relationships.
    ///                context.DeleteObject(rel1);
    ///                context.SaveChanges();
    ///                resourceId = pub.Id;
    ///            }
    ///
    ///            // Give some time to the background job to process these changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            // Retrieve all RelationshipChanges for the above Resource.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                foreach (RelationshipChange rc in context.RelationshipChanges.
    ///                    Include(&quot;Operation&quot;).Include(&quot;ChangeSet&quot;).
    ///                    Where(tuple =&gt; tuple.PreviousSubjectResourceId == resourceId ||
    ///                    tuple.NextSubjectResourceId == resourceId ||
    ///                    tuple.PreviousObjectResourceId == resourceId ||
    ///                    tuple.NextObjectResourceId == resourceId))
    ///                {
    ///                    Console.WriteLine(&quot;Relationship Id:[{0}], Operation: [{1}], DateCreated: [{2}]&quot;,
    ///                        rc.RelationshipId, rc.Operation.Name, rc.ChangeSet.DateCreated);
    ///                    Console.WriteLine(&quot;PreviousSubjectId: [{0}], PreviousObjectId: [{1}], PreviousPredicateId: [{2}]&quot;,
    ///                        rc.PreviousSubjectResourceId, rc.PreviousObjectResourceId, rc.PreviousPredicateId);
    ///                    Console.WriteLine(&quot;NextSubjectId: [{0}], NextObjectId: [{1}], NextPredicateId: [{2}]&quot;,
    ///                        rc.NextSubjectResourceId, rc.NextObjectResourceId, rc.NextPredicateId);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class RelationshipChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal RelationshipChange()
        {
        }
    }
}
