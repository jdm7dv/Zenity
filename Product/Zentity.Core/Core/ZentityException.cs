// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;

namespace Zentity.Core
{
    /// <summary>
    /// This class acts as a base for all the exceptions in Zentity.Core. This class is reserved 
    /// for internal use and is not intended to be used directly from your code.
    /// </summary>
    [Serializable]
    public class ZentityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityException"/> class.
        /// </summary>
        internal ZentityException()
            : base()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        internal ZentityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZentityException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        internal ZentityException(string message, Exception inner) :
            base(message, inner)
        {
        }
    }
}
