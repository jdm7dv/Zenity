// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Usage of this class is very similar to that of 
    /// <see cref="Zentity.ScholarlyWorks.Audio"/>.</example>
    public partial class Code
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnProgrammingLanguageChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.programmingLanguage)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.ProgrammingLanguage, MaxLengths.programmingLanguage));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnTechnologyChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.technology)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Technology, MaxLengths.technology));
        }
    }
}
