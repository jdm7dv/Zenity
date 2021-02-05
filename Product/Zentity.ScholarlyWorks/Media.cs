// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Example below shows simple creation of a Media resource.
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
    ///                //Create a media object.
    ///                Media myMediaObject = new Media();
    ///                myMediaObject.Title = &quot;Informational&quot;;
    ///                myMediaObject.Uri = &quot;urn:zentity-samples:Media:my-informational-media&quot;;
    ///                myMediaObject.Duration = 30;
    ///                myMediaObject.DateCopyrighted = DateTime.Now;
    ///
    ///                //Add the media object to context and save. 
    ///                context.AddToResources(myMediaObject);
    ///                context.SaveChanges();
    ///
    ///                //Display a list of all media objects.
    ///                Console.WriteLine(&quot;List of all media objects:&quot;);
    ///                foreach (Media m in context.Medias())
    ///                {
    ///                    Console.WriteLine(&quot;\t[{0}]&quot;, m.Title);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Media
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnLicenseChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.license)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.License, MaxLengths.license));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnCopyrightChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.copyright)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Copyright, MaxLengths.copyright));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnLanguageChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.language)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Language, MaxLengths.language));
        }
    }
}
