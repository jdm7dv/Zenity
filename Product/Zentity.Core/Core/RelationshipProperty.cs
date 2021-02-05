// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;

namespace Zentity.Core
{
    /// <example>
    /// This example shows how to use the property bag on resource to resource relationship triple.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        static string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res://Zentity.Core;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///        public static void Main(string[] args)
    ///        {
    ///            Guid relationshipId;
    ///
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                relationshipId = CreateRelationship(context);
    ///                Console.WriteLine(&quot;Created relationship with Id [{0}]...&quot;, relationshipId);
    ///
    ///                // Create a Property (which may be reused by other repository items).    
    ///                Property p = new Property();
    ///                p.Name = &quot;AssociationExpiryDate&quot;;
    ///
    ///                // Create a RelationshipProperty.    
    ///                RelationshipProperty relationshipProperty = new RelationshipProperty();
    ///
    ///                // Fetch and associate the relationship with the relationship property.    
    ///                relationshipProperty.Relationship = context.Relationships.
    ///                    Where(r =&gt; r.Id == relationshipId).FirstOrDefault();
    ///
    ///                // Associate the property.    
    ///                relationshipProperty.Property = p;
    ///
    ///                // Associate the value.    
    ///                relationshipProperty.Value = DateTime.MaxValue.ToString();
    ///
    ///                // Save the context.     
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Associated property...&quot;);
    ///            }
    ///
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Relationship rel = context.Relationships.
    ///                    Where(tuple =&gt; tuple.Id == relationshipId).First();
    ///
    ///                rel.Properties.Load();
    ///                Console.WriteLine(&quot;Printing property values...&quot;);
    ///                foreach (RelationshipProperty prop in rel.Properties)
    ///                {
    ///                    prop.PropertyReference.Load();
    ///                    Console.WriteLine(&quot;[{0}]: [{1}]&quot;, prop.Property.Name, prop.Value);
    ///                }
    ///            }
    ///        }
    ///
    ///        private static Guid CreateRelationship(ZentityContext context)
    ///        {
    ///            // Create resources and relationships.    
    ///            Resource lecture = new Resource();
    ///            lecture.Uri = &quot;urn:zentity-samples:aLecture&quot;;
    ///            File material = new File();
    ///            Relationship hasFile = new Relationship();
    ///            hasFile.Predicate = context.Predicates.
    ///                Where(p =&gt; p.Name == &quot;ResourceHasFile&quot;).First();
    ///            hasFile.Subject = lecture;
    ///            hasFile.Object = material;
    ///
    ///            // Save the context.     
    ///            context.SaveChanges();
    ///
    ///            return hasFile.Id;
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class RelationshipProperty
    {
        /// <summary>
        /// Initializes a new instance of RelationshipProperty class with no arguements.
        /// </summary>
        public RelationshipProperty()
        {
            // Automatically create a new id.
            this._Id = Guid.NewGuid();
        }
    }
}
