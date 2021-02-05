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
    public partial class PersonalCommunication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnFromChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.from)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.From, MaxLengths.from));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnToChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.to)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.To, MaxLengths.to));
        }
    }
}
