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
    public partial class JournalArticle
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnJournalChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.journal)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Journal, MaxLengths.journal));
        }

    }
}
