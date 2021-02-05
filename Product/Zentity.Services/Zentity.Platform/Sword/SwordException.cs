// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class represents the exception thrown by Sword processor.
    /// </summary>
    [Serializable()]
    public class SwordException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwordException"/> class.
        /// </summary>
        public SwordException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwordException"/> class.
        /// </summary>
        /// <param name="message">The message that describe the error.</param>
        public SwordException(string message)
            : base(message)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SwordException"/> class.
        /// </summary>
        /// <param name="message">The message that describe the error.</param>
        /// <param name="innerException">The System.Exception class that represent the exception details.</param>
        public SwordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwordException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected SwordException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
