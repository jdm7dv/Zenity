// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System;
using System.Runtime.Serialization;

namespace Zentity.Platform
{
    /// <summary>
    /// This class represents the exception thrown by METSDocument.
    /// </summary>
    [Serializable()]
    public class MetsException : Exception
    {
        /// <summary>
        /// Create a new instance of Zentity.Platform.MetsException
        /// </summary>
        public MetsException()
            : base()
        {
        }
        /// <summary>
        /// Create a new instance of Zentity.Platform.MetsException
        /// </summary>
        /// <param name="message">The message that describe the error.</param>
        public MetsException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Create a new instance of Zentity.Platform.MetsException
        /// </summary>
        /// <param name="message">The message that describe the error.</param>
        /// <param name="innerException">The System.Exception class that represent the exception details.</param>
        public MetsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        /// <summary>
        /// Create a new instance of Zentity.Platform.MetsException
        /// </summary>
        /// <param name="info">The System.RunTime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context"></param>
        protected MetsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
