// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Administration
{
    /// <summary>Adds functionality to the ChangeSet entity class.</summary>
    /// <example>This example shows simple creation, updating and deletion of a resource. 
    /// It then shows how to retrieve the repository changes. Use 
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
    ///            DateTime start = DateTime.Now;
    ///            // Use ZentityContext to update repository content.
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                // Create.
    ///                Resource pub = new Resource
    ///                {
    ///                    Title = &quot;sample Resource&quot;,
    ///                    Uri = &quot;urn:zentity-samples:pub1&quot;
    ///                };
    ///                context.AddToResources(pub);
    ///                context.SaveChanges();
    ///
    ///                // Update.
    ///                pub.Title = &quot;new title&quot;;
    ///                context.SaveChanges();
    ///
    ///                // Delete.
    ///                context.DeleteObject(pub);
    ///                context.SaveChanges();
    ///            }
    ///
    ///            // Give some time to the background job to process these changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            // Get all changesets.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                foreach (ChangeSet cs in context.ChangeSets.
    ///                    Where(tuple =&gt; tuple.DateCreated &gt;= start))
    ///                {
    ///                    Console.WriteLine(&quot;Changeset: [{0}] created on [{1}]&quot;, cs.Id, cs.DateCreated);
    ///                    cs.ResourceChanges.Load();
    ///                    foreach (ResourceChange rc in cs.ResourceChanges)
    ///                    {
    ///                        rc.OperationReference.Load();
    ///                        Console.WriteLine(&quot;ResourceChange ResourceId:[{0}], Operation:[{1}]&quot;, 
    ///                            rc.ResourceId, rc.Operation.Name);
    ///                    }
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ChangeSet 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSet"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal ChangeSet()
        {
        }
    }
}
