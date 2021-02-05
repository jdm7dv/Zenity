// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Administration
{
    /// <summary>Adds functionality to the Operation entity class.</summary>
    /// <example>This example show how to access changes and the associated changesets 
    /// given an operation type.
    /// <code>
    ///using System;
    ///using System.Linq;
    ///using Zentity.Administration;
    ///using Zentity.Core;
    ///using System.Threading;
    ///using Zentity.ScholarlyWorks;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            DateTime startTime = DateTime.Now;
    ///
    ///            // Create some resources.
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Resource t1 = new Resource { Title = &quot;Resource1&quot;, Uri = &quot;urn:zentity-samples:Resource-1&quot; };
    ///                context.AddToResources(t1);
    ///                context.SaveChanges();
    ///
    ///                Resource t2 = new Resource { Title = &quot;Resource2&quot;, Uri = &quot;urn:zentity-samples:Resource-2&quot; };
    ///                context.AddToResources(t2);
    ///                context.SaveChanges();
    ///
    ///                Resource t3 = new Resource { Title = &quot;Resource3&quot;, Uri = &quot;urn:zentity-samples:Resource-3&quot; };
    ///                context.AddToResources(t3);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Inserted resources with IDs: [{0}], [{1}], [{2}]&quot;, t1.Id, t2.Id, t3.Id);
    ///            }
    ///
    ///            // Give some time to the background job to process these changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            // Retrieve all ResourceChanges of type 'Insert'.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                Operation insertOperation = context.Operations.Include(&quot;ResourceChanges.ChangeSet&quot;).
    ///                    Where(tuple =&gt; tuple.Name == &quot;Insert&quot;).First();
    ///
    ///                foreach (ResourceChange tc in insertOperation.ResourceChanges.
    ///                    Where(tuple =&gt; tuple.ChangeSet.DateCreated &gt;= startTime))
    ///                {
    ///                    Console.WriteLine(&quot;Resource [{0}] created on [{1}]&quot;,
    ///                        tc.ResourceId, tc.ChangeSet.DateCreated);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Operation"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal Operation()
        {
        }
    }
}
