// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Core
{
    using System;
    using System.Globalization;

    /// <summary>Adds functionality and handles events for the Property entity class.</summary>
    /// <example>Example below shows how to extend the metadata on a resource using custom 
    /// properties.
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
    ///            string propertyName = &quot;Property&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
    ///            Guid resourceId;
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a property.    
    ///                Property prop = new Property { Name = propertyName };
    ///
    ///                // Create a resource.    
    ///                Resource aLecture = new Resource { Uri = &quot;urn:zentity-samples:lecture:aLecture&quot; };
    ///
    ///                // Create an extended property.    
    ///                aLecture.ResourceProperties.Add(new ResourceProperty
    ///                {
    ///                    Property = prop,
    ///                    Value = &quot;Some custom value&quot;
    ///                });
    ///
    ///                context.AddToResources(aLecture);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created resource with extended properties.&quot;);
    ///                resourceId = aLecture.Id;
    ///            }
    ///
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Resource r = context.Resources.Include(&quot;ResourceProperties.Property&quot;).
    ///                    Where(tuple =&gt; tuple.Id == resourceId).First();
    ///
    ///                ResourceProperty rp = r.ResourceProperties.First();
    ///
    ///                Console.WriteLine(&quot;Resource:[{0}], Property:[{1}], Value:[{2}]&quot;,
    ///                    r.Id, rp.Property.Name, rp.Value);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Property
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        public Property()
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
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.PropertyName)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, 
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Name, 
                    MaxLengths.PropertyName));
        }

        /// <summary>
        /// Called when URI changing.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnUriChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.PropertyUri)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Uri,
                    MaxLengths.PropertyUri));
        }
        #endregion
    }
}
