// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;

namespace Zentity.Core
{
    /// <example>Example below shows how to use the property bag on a resource.
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
    ///
    ///        public static void Main(string[] args)
    ///        {
    ///            Guid resourceId = Guid.Empty;
    ///            InitializeRepository();
    ///
    ///            // Create a resource with extended properties. 
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a new resource.     
    ///                Resource myArticle = new Resource();
    ///                myArticle.Title = &quot;Zentity Data Model Extensibility&quot;;
    ///                myArticle.Uri = &quot;urn:zentity-samples:myArticle&quot;;
    ///                resourceId = myArticle.Id;
    ///
    ///                // Create an resource property.    
    ///                ResourceProperty summary = new ResourceProperty();
    ///                summary.Resource = myArticle;
    ///
    ///                summary.Property = context.Properties.Where(p =&gt; p.Name == &quot;Summary&quot;).FirstOrDefault();
    ///
    ///                // Provide a value for the property.    
    ///                summary.Value = @&quot;Zentity allows developers to introduce new entities and 
    ///                associations in the Core entity data model that comes with the installation. 
    ///                The process of extending the Core entity data model involves defining new entities 
    ///                and associations between them, modifying the database schema to accommodate new entities, 
    ///                generation of source code and generation of Entity Framework artifacts for 
    ///                the new entities. Zentity provides an API to do most of the required tasks.
    ///                This document discusses the details of this API, structure of the generated database 
    ///                objects and Entity Framework artifacts and some performance improvement techniques
    ///                for applications that use the extensibility feature. Also, this document contains 
    ///                simple walkthroughs for some common scenarios.&quot;;
    ///
    ///                // Save the context.     
    ///                context.SaveChanges();
    ///            }
    ///
    ///            // Retrieve the extended properties on the resource. 
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Resource resource = context.Resources.Include(&quot;ResourceProperties.Property&quot;).Where(tuple =&gt; tuple.Id == resourceId).FirstOrDefault();
    ///                foreach (ResourceProperty prop in resource.ResourceProperties)
    ///                    Console.WriteLine(&quot;Property name = [0], Property Value = [{1}]&quot;, prop.Property.Name, prop.Value);
    ///            }
    ///        }
    ///
    ///        private static void InitializeRepository()
    ///        {
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a property.    
    ///                Property summary = new Property();
    ///                summary.Name = &quot;Summary&quot;;
    ///
    ///                // Add property to context.    
    ///                context.AddToProperties(summary);
    ///
    ///                // Save the context.     
    ///                context.SaveChanges();
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ResourceProperty
    {
        /// <summary>
        /// Initializes a new instance of ResourceProperty class with no arguements.
        /// </summary>
        public ResourceProperty()
        {
            // Automatically create a new id.
            this._Id = Guid.NewGuid();
        }
    }
}
