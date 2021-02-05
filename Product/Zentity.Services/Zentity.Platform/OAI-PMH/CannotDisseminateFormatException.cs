// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





namespace Zentity.Platform
{
    #region Using namespace

    using System;
    using System.Data;
    using System.Configuration;
    using System.Linq;
    using System.Xml.Linq;
    using System.Runtime.Serialization;

    #endregion

    #region CannotDisseminateFormatException Class

    /// <summary>
    /// The exception that is thrown if the metadata format associated with the Request is not supported by the repository.
    /// </summary>
    [Serializable()]
    public class CannotDisseminateFormatException : NotSupportedException
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.CannotDisseminateFormatException class.
        /// </summary>
        public CannotDisseminateFormatException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.CannotDisseminateFormatException class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        public CannotDisseminateFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.CannotDisseminateFormatException class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected CannotDisseminateFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.CannotDisseminateFormatException class
        /// with a specified error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.
        /// </param>
        public CannotDisseminateFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }

    #endregion
}
