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
    public partial class Publication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnDOIChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.doi)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.DOI, MaxLengths.doi));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnPagesChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.pages)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Pages, MaxLengths.pages));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnInstitutionChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.institution)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Institution, MaxLengths.institution));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnPublisherAddressChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.publisherAddress)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.PublisherAddress, MaxLengths.publisherAddress));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnLocationChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.location)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Location, MaxLengths.location));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnBookTitleChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.bookTitle)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.BookTitle, MaxLengths.bookTitle));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnVolumeChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.volume)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Volume, MaxLengths.volume));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnCatalogNumberChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.catalogNumber)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.CatalogNumber, MaxLengths.catalogNumber));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnPublisherUriChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.publisherUri)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.PublisherUri, MaxLengths.publisherUri));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnChapterChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.chapter)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Chapter, MaxLengths.chapter));
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnNumberChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.number)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Number, MaxLengths.number));
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnPublisherChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.publisher)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Publisher, MaxLengths.publisher));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnEditionChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.edition)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Edition, MaxLengths.edition));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnOrganizationChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.organization)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Organization, MaxLengths.organization));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnSeriesChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.series)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, ScholarlyWorksResources.ValidationExceptionInvalidLength, ScholarlyWorksResources.Series, MaxLengths.series));
        }
    }
}
