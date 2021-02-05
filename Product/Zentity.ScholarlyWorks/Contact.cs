// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Example below shows simple creation of a Contact resource.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using Zentity.ScholarlyWorks;
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res://Zentity.ScholarlyWorks;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a contact. 
    ///                Contact aContact = new Contact
    ///                {
    ///                    Email = &quot;someone@contoso.com&quot;,
    ///                    Uri = &quot;urn:zentity-samples:contact:aContact&quot;
    ///                };
    ///                context.AddToResources(aContact);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created contact with Uri: [{0}]&quot;, aContact.Uri);
    ///
    ///                //Retrieve all contacts from the repository. 
    ///                Console.WriteLine(&quot;All contacts in the repository:&quot;);
    ///                foreach (Contact c in context.Contacts())
    ///                    Console.WriteLine(&quot;\t\t{0}&quot;, c.Uri);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Contact
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnEmailChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.email)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Email, MaxLengths.email));
        }
    }
}
