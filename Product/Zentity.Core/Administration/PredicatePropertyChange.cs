// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Administration
{
    /// <example>This example shows simple creation of a <see cref="Zentity.Core.PredicateProperty"/> 
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
    ///            //Create Predicate property
    ///            Guid predicatePropertyId = CreatePredicateProperty();
    ///            PrintPredicateProperty(predicatePropertyId);
    ///            RetrivePredicatePropertyChanges();
    ///
    ///            //Modify Predicate property
    ///            predicatePropertyId = ModifyPredicateProperty(predicatePropertyId);
    ///            PrintPredicateProperty(predicatePropertyId);
    ///            RetrivePredicatePropertyChanges();
    ///
    ///            //Delete Predicate property 
    ///            DeletePredicateProperty(predicatePropertyId);
    ///            RetrivePredicatePropertyChanges();
    ///        }
    ///
    ///        static Guid CreatePredicateProperty()
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                //Create Predicate
    ///                Predicate predicate = new Predicate();
    ///                predicate.Name = &quot;Sample Predicate&quot;;
    ///                predicate.Uri = &quot;urn:change-history-samples/predicate/sample-predicate&quot;;
    ///
    ///                //Create Property
    ///                Property property = new Property();
    ///                property.Name = &quot;Description&quot;;
    ///                property.Uri = &quot;urn:change-history-samples/property/description&quot;;
    ///
    ///                PredicateProperty preProperty = new PredicateProperty();
    ///                preProperty.Predicate = predicate;
    ///                preProperty.Property = property;
    ///                preProperty.Value = Guid.NewGuid().ToString();
    ///
    ///                context.AddToPredicateProperties(preProperty);
    ///                context.SaveChanges();
    ///                return preProperty.Id;
    ///            }
    ///        }
    ///
    ///        static Guid ModifyPredicateProperty(Guid predicatePropertyId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                PredicateProperty preProperty = context.PredicateProperties.
    ///                    Where(tuple =&gt; tuple.Id == predicatePropertyId).First();
    ///                preProperty.Value = Guid.NewGuid().ToString();
    ///                context.SaveChanges();
    ///                return preProperty.Id;
    ///            }
    ///        }
    ///
    ///        static void DeletePredicateProperty(Guid predicatePropertyId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                PredicateProperty preProperty = context.PredicateProperties
    ///                                      .Where(tuple =&gt; tuple.Id == predicatePropertyId)
    ///                                      .First();
    ///                context.DeleteObject(preProperty);
    ///                context.SaveChanges();
    ///            }
    ///        }
    ///
    ///        static void PrintPredicateProperty(Guid predicatePropertyId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                PredicateProperty predicateProperty = context.PredicateProperties.
    ///                    Include(&quot;Property&quot;).Include(&quot;Predicate&quot;).
    ///                    Where(tuple =&gt; tuple.Id == predicatePropertyId).First();
    ///
    ///                Console.WriteLine(&quot;Predicate property details...&quot;);
    ///                Console.WriteLine(&quot;Predicate Id    [{0}]&quot;, predicateProperty.Predicate.Id);
    ///                Console.WriteLine(&quot;Predicate Name [{0}]&quot;, predicateProperty.Predicate.Name);
    ///                Console.WriteLine(&quot;Property Id    [{0}]&quot;, predicateProperty.Property.Id);
    ///                Console.WriteLine(&quot;Property Name  [{0}]&quot;, predicateProperty.Property.Name);
    ///                Console.WriteLine(&quot;Property Value [{0}]&quot;, predicateProperty.Value);
    ///            }
    ///
    ///        }
    ///        static void RetrivePredicatePropertyChanges()
    ///        {
    ///            // Changeset entries are processed by a background 
    ///            // job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///                // Pick the last PredicatePropertyChange.
    ///                var predicatePropertyChange = adminContext.PredicatePropertyChanges.Include(&quot;Changeset&quot;).
    ///                    Include(&quot;Operation&quot;).OrderByDescending(tuple =&gt; tuple.ChangeSet.DateCreated).First();
    ///
    ///                if (predicatePropertyChange != null)
    ///                {
    ///                    Console.WriteLine(&quot;&quot;);
    ///                    Console.WriteLine(&quot;Changeset [{0}] created on [{1}]&quot;, predicatePropertyChange.ChangeSet.Id, 
    ///                        predicatePropertyChange.ChangeSet.DateCreated);
    ///                    Console.WriteLine(&quot;Operation           [{0}]&quot;, predicatePropertyChange.Operation.Name);
    ///                    Console.WriteLine(&quot;PredicatePropertyId  [{0}]&quot;, predicatePropertyChange.PredicatePropertyId);
    ///                    Console.WriteLine(&quot;PreviousPropertyId  [{0}]&quot;, predicatePropertyChange.PreviousPropertyId);
    ///                    Console.WriteLine(&quot;NextPropertyId      [{0}]&quot;, predicatePropertyChange.NextPropertyId);
    ///                    Console.WriteLine(&quot;PreviousPredicateId  [{0}]&quot;, predicatePropertyChange.PreviousPredicateId);
    ///                    Console.WriteLine(&quot;NextPredicateId      [{0}]&quot;, predicatePropertyChange.NextPredicateId);
    ///                    Console.WriteLine(&quot;PreviousValue       [{0}]&quot;, predicatePropertyChange.PreviousValue);
    ///                    Console.WriteLine(&quot;NextValue           [{0}]&quot;, predicatePropertyChange.NextValue);
    ///                    Console.WriteLine(&quot;&quot;);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class PredicatePropertyChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PredicatePropertyChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal PredicatePropertyChange()
        {
        }
    }
}
