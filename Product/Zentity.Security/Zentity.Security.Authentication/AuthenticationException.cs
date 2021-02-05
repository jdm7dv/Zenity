// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authentication
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The exception that is thrown to when an error occurs during authentication.
    /// </summary>
    [Serializable]
    public class AuthenticationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the AuthenticationException class.
        /// </summary>
        public AuthenticationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthenticationException class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public AuthenticationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthenticationException class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception to be wrapped in a AuthenticationException</param>
        public AuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthenticationException class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected AuthenticationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
