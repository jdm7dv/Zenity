// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Core
{
    /// <summary>
    /// Contains constants for various entity properties.
    /// </summary>
    internal static class MaxLengths
    {
        // Association.
        internal const int AssociationName = 100;
        internal const int AssociationUri = 1024;

        //Resouce type Property
        internal const int ResourceTypePropertyName = 100;
        internal const int ResourceTypePropertyUri = 1024;
        internal const int ResourceTypePropertyDescription = 4000;

        // Predicate.
        internal const int PredicateName = 128;
        internal const int PredicateUri = 1024;

        // Property.
        internal const int PropertyName = 50;
        internal const int PropertyUri = 1024;

        // Resource.
        internal const int FileChecksum = 256;
        internal const int FileMimeType = 128;
        internal const int FileFileExtension = 128;
        internal const int Title = 425;
        internal const int Uri = 1024;
    }
}
