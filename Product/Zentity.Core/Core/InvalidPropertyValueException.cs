// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Core
{
    using System;

    /// <summary>
    /// Custom exception that is thrown if there is a property definition mismatch either for a 
    /// resource type or a scalar property.
    /// </summary>
    [Serializable]
    internal sealed class InvalidPropertyValueException : ZentityException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPropertyValueException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        internal InvalidPropertyValueException(string message)
            : base(message)
        {
        }
    }
}
