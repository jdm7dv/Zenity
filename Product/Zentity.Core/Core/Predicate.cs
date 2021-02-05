// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Core
{
    using System;
    using System.Globalization;

    /// <summary>Adds functionality and handles events for the Predicate entity class.</summary>
    /// <example>
    /// This example shows creation of a simple predicate and retrieval of all the 
    /// lecture-to-person relationships that use this predicate.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
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
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a predicate.    
    ///                Predicate pred = new Predicate
    ///                {
    ///                    Name = &quot;DeliveredBy&quot;,
    ///                    Uri = &quot;urn:zentity-samples:predicate:deliveredBy&quot;
    ///                };
    ///
    ///                // Create a lecture.    
    ///                Resource aLecture = new Resource { Uri = &quot;urn:zentity-samples:lecture:aLecture&quot; };
    ///
    ///                // Create a person.    
    ///                Resource aPerson = new Resource { Uri = &quot;urn:zentity-samples:person:theSpeaker&quot; };
    ///
    ///                // Create 'deliveredBy' relationship.    
    ///                Relationship apr = new Relationship();
    ///                apr.Subject = aLecture;
    ///                apr.Object = aPerson;
    ///                apr.Predicate = pred;
    ///
    ///                // Save off everything.    
    ///                // Adding just one of the above created objects to the context is sufficient.    
    ///                context.AddToResources(aLecture);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created Lecture, Person and Predicate&quot;);
    ///
    ///                // Now retrieve all the relationships that use our predicate.    
    ///                pred.Relationships.Load();
    ///
    ///                foreach (Relationship rel in pred.Relationships)
    ///                {
    ///                    rel.SubjectReference.Load();
    ///                    rel.ObjectReference.Load();
    ///                    Console.WriteLine(rel.Subject.Uri + &quot;  &lt;--deliveredBy--&gt;  &quot; + rel.Object.Uri);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Predicate
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Predicate"/> class.
        /// </summary>
        public Predicate()
        {
            // Automatically create a new id.
            this._Id = Guid.NewGuid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when name changing.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnNameChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.PredicateName)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Name,
                    MaxLengths.PredicateName));
        }

        /// <summary>
        /// Called when URI changing.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnUriChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.PredicateUri)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, 
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Uri, 
                    MaxLengths.PredicateUri));
        }

        #endregion
    }
}
