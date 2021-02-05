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

    #region BadArgumentException Class

    /// <summary>
    /// The exception that is thrown when argument associated with the Request is invalid.
    /// </summary>
    [Serializable()]
    public class BadArgumentException : ArgumentException
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.BadArgumentException class.
        /// </summary>
        public BadArgumentException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.BadArgumentException class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        public BadArgumentException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.BadArgumentException class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected BadArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.BadArgumentException class
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
        public BadArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.BadArgumentException class
        /// with a specified error message and the name of the parameter that causes this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        public BadArgumentException(string message, string parameterName)
            : base(message, parameterName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Zentity.Platform.BadArgumentException class
        /// with a specified error message, the parameter name, and a reference to the inner exception
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="parameterName">
        /// The name of the parameter that caused the current exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.
        /// </param>
        public BadArgumentException(string message, string parameterName, Exception innerException)
            : base(message, parameterName, innerException)
        {
        }

        #endregion
    }

    #endregion
}
