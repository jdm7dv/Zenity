// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;

    /// <summary>
    /// This class represents a mapping error occurred while BibTeX import or export.
    /// </summary>
    [Serializable]
    public class BibTeXMappingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXMappingException"/> class.
        /// </summary>
        public BibTeXMappingException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXMappingException"/> class.
        /// </summary>
        /// <param name="message">Message describing error behavior</param>
        public BibTeXMappingException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXMappingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BibTeXMappingException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXMappingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected BibTeXMappingException(global::System.Runtime.Serialization.SerializationInfo info,
                                    global::System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
