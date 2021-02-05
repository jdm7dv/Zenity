// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Runtime.Serialization;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This exception is thrown when resource to resource association fails.
    /// </summary>
    [Serializable]
    public class AssociationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationException" /> class.
        /// </summary>
        public AssociationException()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationException" /> class with a 
        /// specified error message.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        public AssociationException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationException" /> class with a 
        /// specified error message and inner exception.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        /// <param name="inner">Inner exception.</param>
        public AssociationException(string message, Exception inner)
            : base(message, inner)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationException" /> class.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        protected AssociationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
