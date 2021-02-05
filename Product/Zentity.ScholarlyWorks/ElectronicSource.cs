// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Example below shows the creation of an ElectronicSource resource.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using Zentity.ScholarlyWorks;
    ///
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
    ///            Guid sourceId = Guid.Empty;
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create an electronic source.
    ///                ElectronicSource anExternalReference = new ElectronicSource();
    ///                anExternalReference.Uri = &quot;urn:zentity-samples:ElectronicSource:Team-Test-Links&quot;;
    ///                anExternalReference.Title = &quot;Team Test Useful Links&quot;;
    ///                anExternalReference.Reference = &quot;http://msdn2.microsoft.com/en-us/library/ms182631.aspx&quot;;
    ///                sourceId = anExternalReference.Id;
    ///
    ///                context.AddToResources(anExternalReference);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created electronic source with Uri: [{0}]&quot;, anExternalReference.Uri);
    ///            }
    ///
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Retrieve the test related electronic sources.
    ///                Console.WriteLine(&quot;Electronic Sources in the repository:&quot;);
    ///                foreach (ElectronicSource es in context.ElectronicSources().
    ///                    Where(tuple =&gt; tuple.Title.Contains(&quot;test&quot;)))
    ///                {
    ///                    Console.WriteLine(&quot;\t\t{0}:{1}&quot;, es.Title, es.Reference);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ElectronicSource
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnReferenceChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.reference)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Reference, MaxLengths.reference));
        }
    }
}
