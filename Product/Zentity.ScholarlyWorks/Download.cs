// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Example below shows a simple creation of Download resource.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using System.IO;
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
    ///                Download myDownload = new Download();
    ///                myDownload.Title = &quot;My Uninstaller&quot;;
    ///                myDownload.Description = @&quot;Uninstalls myProduct.&quot;;
    ///
    ///                FileStream fs = new FileStream(@&quot;C:\EULA.txt&quot;, FileMode.Open, FileAccess.Read);
    ///                StreamReader eulaReader = new StreamReader(fs);
    ///                myDownload.EULA = eulaReader.ReadToEnd();
    ///
    ///                myDownload.HardwareRequirements = &quot;Min 512 MB RAM, 10 MB hard disk space&quot;;
    ///                myDownload.OperatingSystem = &quot;Windows Server 2003, Windows XP Professional SP2&quot;;
    ///                myDownload.VersionInformation = &quot;Version 1.0&quot;;
    ///
    ///                // Associate a file object for saving the software exe/msi to repository. 
    ///                Zentity.Core.File toolFile = new Zentity.Core.File();
    ///                myDownload.Files.Add(toolFile);
    ///
    ///                // Save the Download and File resources, content can only be uploaded to a 'File' 
    ///                // object already present in the repository.
    ///                context.AddToResources(myDownload);
    ///                context.SaveChanges();
    ///
    ///                // Upload file contents.
    ///                context.UploadFileContent(toolFile, @&quot;C:\ContosoCleaner.msi&quot;);
    ///
    ///                var myDownloads = from tuple in context.Downloads().
    ///                    Where(tuple =&gt; tuple.Title.Contains(&quot;My&quot;))
    ///                                  select tuple;
    ///                foreach (Download tool in myDownloads)
    ///                {
    ///                    Console.WriteLine(&quot;{0}:{1}&quot;, tool.Title, tool.Description);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Download
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnVersionInformationChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.versionInformation)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.VersionInformation, MaxLengths.versionInformation));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnCopyrightChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.copyright)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Copyright, MaxLengths.copyright));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnHardwareRequirementsChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.hardwareRequirements)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.HardwareRequirements, MaxLengths.hardwareRequirements));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnEULAChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.eula)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.EULA, MaxLengths.eula));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnOperatingSystemChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.operatingSystem)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.OperatingSystem, MaxLengths.operatingSystem));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnDownloadRequirementsChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.downloadRequirements)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.DownloadRequirements, MaxLengths.downloadRequirements));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnSystemRequirementsChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.systemRequirements)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.SystemRequirements, MaxLengths.systemRequirements));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnLanguageChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.language)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Language, MaxLengths.language));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnLicenseChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.license)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.License, MaxLengths.license));
        }
    }
}
