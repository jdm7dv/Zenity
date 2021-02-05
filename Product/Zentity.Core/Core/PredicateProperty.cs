// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Core
{
    using System;

    /// <summary>Adds functionality and handles events for the PredicateProperty entity class.</summary>
    /// <example>
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        static string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res:///Zentity.Core;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///
    ///        public static void Main(string[] args)
    ///        {
    ///            Guid predicateId = Guid.Empty;
    ///            InitializeRepository();
    ///
    ///            /// Create a resource with extended properties. 
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                /// Create a new Predicate.     
    ///                Predicate myAssesmentBy = new Predicate();
    ///                myAssesmentBy.Name= &quot;AssesmentBy&quot;;
    ///                myAssesmentBy.Uri = &quot;ZentitySamples:AssesmentBy&quot;;
    ///                predicateId = myAssesmentBy.Id;
    ///                /// Create an predicate property.    
    ///                PredicateProperty predicateProperty = new PredicateProperty();
    ///                predicateProperty.Predicate =myAssesmentBy;
    ///
    ///                predicateProperty.Property = context.Properties.Where(queueName =&gt; queueName.Name == &quot;Description&quot;).FirstOrDefault();
    ///
    ///                /// Provide a value for the property.    
    ///                predicateProperty.Value =&quot;Demo value.&quot;;
    ///
    ///                /// Save the context.     
    ///                context.SaveChanges();
    ///            }
    ///
    ///            /// Retrieve the extended properties on the predicate. 
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Predicate predicate = context.Predicates.Include(&quot;Properties&quot;)
    ///                    .Where(tuple =&gt; tuple.Id == predicateId).FirstOrDefault();
    ///                
    ///                foreach (PredicateProperty prop in predicate.Properties)
    ///                {
    ///                    prop.PropertyReference.Load();
    ///                    Console.WriteLine(&quot;Property name = [0], Property Value = [{1}]&quot;, prop.Property.Name, prop.Value);
    ///                }
    ///            }
    ///        }
    ///
    ///        private static void InitializeRepository()
    ///        {
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                /// Create a property.    
    ///                Property note = new Property();
    ///                note.Name = &quot;Description&quot;;
    ///
    ///                /// Add property to context.    
    ///                context.AddToProperties(note);
    ///
    ///                /// Save the context.     
    ///                context.SaveChanges();
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class PredicateProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateProperty"/> class.
        /// </summary>
        public PredicateProperty()
        {
            // Automatically create a new id.
            this._Id = Guid.NewGuid();
        }
    }
}
