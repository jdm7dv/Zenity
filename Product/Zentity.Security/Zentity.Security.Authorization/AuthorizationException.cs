// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Represents errors that occur during authorization execution.
    /// </summary>
    [Serializable]
    public class AuthorizationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the AuthorizationException class.
        /// </summary>
        public AuthorizationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthorizationException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AuthorizationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthorizationException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.
        /// If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public AuthorizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AuthorizationException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected AuthorizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
