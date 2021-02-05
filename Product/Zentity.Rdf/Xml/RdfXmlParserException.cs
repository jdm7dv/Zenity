// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// The exception that is thrown when parsing of the input RDF/XML document fails.
    /// </summary>
    [Serializable]
    public sealed class RdfXmlParserException : Exception
    {
        #region Member Variables

        #region Private
        private string errorMessageId;
        private const string KeyErrorMessageId = "ErrorMessageId";
        #endregion

        #endregion

        #region Properties

        #region Public
        /// <summary>
        /// Gets error message Id.
        /// </summary>
        public string ErrorMessageId
        {
            get
            {
                return errorMessageId;
            }
        }
        #endregion

        #endregion

        #region Constructors

        #region Public

        /// <summary>
        /// Initializes a new instance of the RdfXmlParserException class.
        /// </summary>
        public RdfXmlParserException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParserException class.
        /// </summary>
        /// <param name="messageId">Error message Id.</param>
        public RdfXmlParserException(string messageId)
            : base(Resources.ResourceManager.GetString(messageId))
        {
            errorMessageId = messageId;
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParserException class.
        /// </summary>
        /// <param name="messageId">Error message Id.</param>
        /// <param name="parameters">Parameters to be replaced in Error message.</param>
        public RdfXmlParserException(string messageId, params string[] parameters)
            : base( string.Format(CultureInfo.InstalledUICulture, Resources.ResourceManager.GetString(messageId, CultureInfo.CurrentCulture), parameters))
        {
            errorMessageId = messageId;
        }


        /// <summary>
        /// Initializes a new instance of the RdfXmlParserException class.
        /// </summary>
        /// <param name="messageId">Error message Id.</param>
        /// <param name="innerException">Inner Exception.</param>
        public RdfXmlParserException(string messageId, Exception innerException)
            : base(Resources.ResourceManager.GetString(messageId, CultureInfo.CurrentCulture), innerException)
        {
            errorMessageId = messageId;
        }

        /// <summary>
        /// Initializes a new instance of the RdfXmlParserException class.
        /// </summary>
        /// <param name="innerException">Inner Exception.</param>
        /// <param name="messageId">Error message Id.</param>
        /// <param name="parameters">Parameters to be replaced in Error message.</param>
        public RdfXmlParserException(Exception innerException, string messageId, params string[] parameters)
            : base(string.Format(CultureInfo.InstalledUICulture, Resources.ResourceManager.GetString(messageId, CultureInfo.CurrentCulture), parameters), innerException)
        {
            errorMessageId = messageId;
        }

        #endregion

        #region Private

        /// <summary>
        /// Initializes a new instance of the RdfXmlParserException class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">An object that describes the source or destination of the serialized data.</param>
        private RdfXmlParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.errorMessageId = info.GetString(KeyErrorMessageId);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// sets the System.Runtime.Serialization.SerializationInfo
        /// with information about the exception.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">An object that describes the source or destination of the serialized data.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "info"));
            }
            base.GetObjectData(info, context);
            info.AddValue(KeyErrorMessageId, this.errorMessageId, typeof(string));

        }

        #endregion
    }
}
