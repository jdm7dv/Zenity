// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Example below shows a simple creation of Person resource.
    /// <code>
    ///using Zentity.Core;
    ///using System.Linq;
    ///using System;
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
    ///            Guid personId = Guid.Empty;
    ///
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a person.   
    ///                Person p = new Person
    ///                {
    ///                    Uri = &quot;urn:zentity-samples:person:aPerson&quot;,
    ///                    FirstName = &quot;first&quot;,
    ///                    LastName = &quot;last&quot;,
    ///                    Email = &quot;first_last@contoso.com&quot;
    ///                };
    ///                personId = p.Id;
    ///
    ///                // Add the person to context.    
    ///                context.AddToResources(p);
    ///
    ///                // Save the context.    
    ///                context.SaveChanges();
    ///
    ///                // Retrieve the person.   
    ///                Person thePerson = context.People().
    ///                    Where(tuple =&gt; tuple.Id == personId).FirstOrDefault();
    ///                Console.WriteLine(&quot;Name = {0} {1}, Email = {2}&quot;,
    ///                    thePerson.FirstName, thePerson.LastName, thePerson.Email);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Person
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnLastNameChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.lastName)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.LastName, MaxLengths.lastName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnFirstNameChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.firstName)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.FirstName, MaxLengths.firstName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnMiddleNameChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.middleName)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.MiddleName, MaxLengths.middleName));
        }
    }
}
