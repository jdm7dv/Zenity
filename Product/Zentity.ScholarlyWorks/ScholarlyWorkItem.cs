// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    public partial class ScholarlyWorkItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnScopeChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.scope)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Scope, MaxLengths.scope));
        }

    }
}
