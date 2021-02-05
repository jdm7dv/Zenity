// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Administration
{
    /// <summary>Adds functionality to PredicateChange entity class.</summary>
    /// <example>This example shows simple creation of a <see cref="Zentity.Core.Predicate"/> 
    /// and retrieving the changes. Use <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
    /// to enable the change history feature if not enabled already. 
    /// <code>
    ///using System;
    ///using System.Linq;
    ///using Zentity.Core;
    ///using System.Threading;
    ///using Zentity.Administration;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res://Zentity.Core;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///
    ///            ZentityContext context = new ZentityContext(connStr);
    ///
    ///            // Create a predicate.    
    ///            Predicate pred = new Predicate
    ///            {
    ///                Name = &quot;Pred&quot; + Guid.NewGuid().ToString(&quot;N&quot;),
    ///                Uri = &quot;urn:zentity-samples:predicate:delivered&quot;
    ///            };
    ///
    ///            // Save off the changes.   
    ///            context.AddToPredicates(pred);
    ///            context.SaveChanges();
    ///            Console.WriteLine(&quot;Created predicate with Id: [{0}]&quot;, pred.Id);
    ///
    ///            // Changeset entries are processed by a background 
    ///            // job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            using (AdministrationContext adminContext = new AdministrationContext(connStr))
    ///            {
    ///                // Pick the last changeset.
    ///                ChangeSet changeSet = adminContext.ChangeSets.
    ///                    Include(&quot;PredicateChanges.Operation&quot;).
    ///                    OrderByDescending(tuple =&gt; tuple.DateCreated).First();
    ///
    ///                if (changeSet != null)
    ///                {
    ///                    Console.WriteLine(&quot;Changeset [{0}] created on [{1}]&quot;,
    ///                        changeSet.Id, changeSet.DateCreated);
    ///
    ///                    foreach (PredicateChange predicateChange in changeSet.PredicateChanges)
    ///                    {
    ///                        Console.WriteLine(&quot;&quot;);
    ///                        predicateChange.ChangeSetReference.Load();
    ///                        predicateChange.OperationReference.Load();
    ///                        Console.WriteLine(&quot;ChangeSet      [{0}]&quot;, predicateChange.ChangeSet.Id);
    ///                        Console.WriteLine(&quot;SequenceNumber [{0}]&quot;, predicateChange.SequenceNumber);
    ///                        Console.WriteLine(&quot;Operation      [{0}]&quot;, predicateChange.Operation.Name);
    ///                        Console.WriteLine(&quot;PredicateId    [{0}]&quot;, predicateChange.PredicateId);
    ///                        Console.WriteLine(&quot;PreviousName   [{0}]&quot;, predicateChange.PreviousName);
    ///                        Console.WriteLine(&quot;NextName       [{0}]&quot;, predicateChange.NextName);
    ///                        Console.WriteLine(&quot;PreviousUri    [{0}]&quot;, predicateChange.PreviousUri);
    ///                        Console.WriteLine(&quot;NextUri        [{0}]&quot;, predicateChange.NextUri);
    ///                    }
    ///                }
    ///            }
    ///
    ///            // Update Predicate properties.
    ///            pred.Name = &quot;Pred&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
    ///            pred.Uri = &quot;urn:zentity-samples:predicate:lecuturedeliveredbyspeaker&quot;;
    ///            context.SaveChanges();
    ///
    ///            // Wait for the background job to process the changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            Console.WriteLine(&quot;\n----------- Updated Predicate -----------&quot;);
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///                PredicateChange predicateChange = adminContext.PredicateChanges.
    ///                                        Where(tuple =&gt; tuple.PredicateId == pred.Id).
    ///                                        OrderByDescending(tuple =&gt; tuple.ChangeSet.DateCreated).
    ///                                        FirstOrDefault();
    ///                if (predicateChange != null)
    ///                {
    ///                    predicateChange.ChangeSetReference.Load();
    ///                    predicateChange.OperationReference.Load();
    ///                    Console.WriteLine(&quot;ChangeSet      [{0}]&quot;, predicateChange.ChangeSet.Id);
    ///                    Console.WriteLine(&quot;SequenceNumber [{0}]&quot;, predicateChange.SequenceNumber);
    ///                    Console.WriteLine(&quot;Operation      [{0}]&quot;, predicateChange.Operation.Name);
    ///                    Console.WriteLine(&quot;PredicateId    [{0}]&quot;, predicateChange.PredicateId);
    ///                    Console.WriteLine(&quot;PreviousName   [{0}]&quot;, predicateChange.PreviousName);
    ///                    Console.WriteLine(&quot;NextName       [{0}]&quot;, predicateChange.NextName);
    ///                    Console.WriteLine(&quot;PreviousUri    [{0}]&quot;, predicateChange.PreviousUri);
    ///                    Console.WriteLine(&quot;NextUri        [{0}]&quot;, predicateChange.NextUri);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class PredicateChange
    {        
        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal PredicateChange()
        {
        }
    }
}
