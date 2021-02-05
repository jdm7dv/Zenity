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
    public partial class Experiment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnReportChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.report)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Report, MaxLengths.report));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnStatusChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.status)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Status, MaxLengths.status));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnPlanChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.plan)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Plan, MaxLengths.plan));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnNameChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.experimentName)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Name, MaxLengths.experimentName));
        }
    }
}
