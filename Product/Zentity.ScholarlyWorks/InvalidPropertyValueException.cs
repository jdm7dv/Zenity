// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;

namespace Zentity.ScholarlyWorks
{
    /// <summary>
    /// Custom exception that is thrown if there is a property definition mismatch either for a resource type or a scalar property.
    /// </summary>
    [Serializable]
    internal sealed class InvalidPropertyValueException : Exception
    {
        internal InvalidPropertyValueException()
            : base()
        {
        }
        internal InvalidPropertyValueException(string message)
            : base(message)
        {
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InvalidPropertyValueException(string message, Exception inner) :
            base(message, inner)
        {
        }
    }
}
