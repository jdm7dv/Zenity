// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Custom exception that is thrown if the data model fails validation tests.
    /// </summary>
    [Serializable]
    internal sealed class ModelItemValidationException : ZentityException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelItemValidationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        internal ModelItemValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelItemValidationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        internal ModelItemValidationException(string message, Exception inner) :
            base(message, inner)
        {
        }
    }
}
