// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Runtime.Serialization;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Exception thrown when an entity is updated simultaneously by multiple users.
    /// </summary>
    [Serializable]
    public class ConcurrentAccessException : Exception
    {
        #region Construction and Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentAccessException" /> class.
        /// </summary>
        public ConcurrentAccessException()
            : this(GlobalResource.ConcurrentAccessExceptionMessage)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentAccessException" /> class with a 
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ConcurrentAccessException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentAccessException" /> class with 
        /// a specified error message and a reference to the inner exception that is 
        /// the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null 
        /// reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ConcurrentAccessException(string message, Exception inner)
            : base(message, inner)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentAccessException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual
        /// information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The info parameter is null.</exception>
        /// <exception cref="SerializationException">The class name is null or 
        /// HResult is zero (0).</exception>        
        protected ConcurrentAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #endregion Construction and Initialization
    }
}