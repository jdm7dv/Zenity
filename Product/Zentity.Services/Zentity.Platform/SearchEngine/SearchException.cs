﻿// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an error that occurs during search.
    /// </summary>
    [Serializable]
    public class SearchException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchException"/> class.
        /// </summary>
        public SearchException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SearchException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference
        /// if no inner exception is specified.</param>
        public SearchException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object
        /// data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        ///<exception cref="ArgumentNullException">The info parameter is null.</exception>
        ///<exception cref="SerializationException">The class name is null or <see cref="Exception.HResult"/>
        ///is zero (0).</exception>
        protected SearchException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion 
    }
}