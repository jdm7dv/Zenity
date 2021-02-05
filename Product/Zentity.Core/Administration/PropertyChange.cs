// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Administration
{
    /// <summary>Add functionality to PropertyChange entity class.</summary>
    /// <example>This example shows simple creation and updating of a 
    /// <see cref="Zentity.Core.Property"/>. It then shows how to retrieve those changes. 
    /// Use <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
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
    ///            ZentityContext context = new ZentityContext();
    ///
    ///            // Create a property.    
    ///            Property  property = new Property{ Name =&quot;CreatedDate&quot;};
    ///
    ///            // Save off the changes.   
    ///            context.AddToProperties(property);
    ///            context.SaveChanges();
    ///
    ///            // Changeset entries are processed by a background 
    ///            // job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///
    ///                // Pick the last changeset.
    ///                ChangeSet changeSet = adminContext.ChangeSets.
    ///                    Include(&quot;PropertyChanges.Operation&quot;).
    ///                    OrderByDescending(tuple =&gt; tuple.DateCreated).First();
    ///
    ///                if (changeSet != null)
    ///                {
    ///                    Console.WriteLine(&quot;Changeset [{0}] created on [{1}]&quot;,
    ///                        changeSet.Id, changeSet.DateCreated);
    ///
    ///                    foreach (PropertyChange  propertyChange in changeSet.PropertyChanges )
    ///                    {
    ///                        Console.WriteLine(&quot;&quot;);
    ///                        propertyChange.ChangeSetReference.Load();
    ///                        propertyChange.OperationReference.Load();
    ///                        Console.WriteLine(&quot;ChangeSet    [{0}]&quot;, propertyChange.ChangeSet.Id);
    ///                        Console.WriteLine(&quot;Operation    [{0}]&quot;, propertyChange.Operation.Name);
    ///                        Console.WriteLine(&quot;PredicateId  [{0}]&quot;, propertyChange.PropertyId);
    ///                        Console.WriteLine(&quot;PreviousName [{0}]&quot;, propertyChange.PreviousName);
    ///                        Console.WriteLine(&quot;NextName     [{0}]&quot;, propertyChange.NextName);
    ///                    }
    ///                }
    ///            }
    ///
    ///            // Update Property properties.
    ///            property.Name = &quot;Created Date&quot;;
    ///            context.SaveChanges();
    ///
    ///            // Wait for the background job to process the changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            Console.WriteLine(&quot;\n----------- Updated property -----------&quot;);
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///                PropertyChange propertyChange = adminContext.PropertyChanges.
    ///                                        Where(tuple =&gt; tuple.PropertyId == property.Id).
    ///                                        OrderByDescending(tuple =&gt; tuple.ChangeSet.DateCreated).
    ///                                        FirstOrDefault();
    ///                if (propertyChange != null)
    ///                {
    ///                    propertyChange.ChangeSetReference.Load();
    ///                    propertyChange.OperationReference.Load();
    ///                    Console.WriteLine(&quot;ChangeSet    [{0}]&quot;, propertyChange.ChangeSet.Id);
    ///                    Console.WriteLine(&quot;Operation    [{0}]&quot;, propertyChange.Operation.Name);
    ///                    Console.WriteLine(&quot;PredicateId  [{0}]&quot;, propertyChange.PropertyId);
    ///                    Console.WriteLine(&quot;PreviousName [{0}]&quot;, propertyChange.PreviousName);
    ///                    Console.WriteLine(&quot;NextName     [{0}]&quot;, propertyChange.NextName);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
     
    public partial class PropertyChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChange"/> class.
        /// </summary>
        internal PropertyChange()
        {
        }
    }
}
