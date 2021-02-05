// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider.DigestAuthentication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;

    /// <summary>
    /// Represents a security token which contains user credentials in the form of Http Digest header. 
    /// </summary>
    public class DigestSecurityToken : SecurityToken
    {
        #region Private variables
        private string digest;
        private string userName;
        private string nonce;
        private string realm;
        private string requestUri;
        private string algoForDigest, algoForChecksum;
        private string httpMethod;
        private DateTime validFrom;
        private DateTime validUpto;
        private Guid id;
        private ReadOnlyCollection<SecurityKey> securityKeys;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the DigestSecurityToken class.
        /// </summary>
        /// <param name="digestResponse">Value of the 'response' directive in the HTTP Digest header.</param>
        /// <param name="userName">LogOn name of the user.</param>
        /// <param name="nonce">Value of the 'nonce' directive in the digest header.</param>
        /// <param name="realm">Value of 'realm' directive in the digest header.</param>
        /// <param name="requestUri">Http request Uri</param>
        /// <param name="httpMethod">Http Method - GET, PUT, POST</param>
        /// <param name="algorithmForDigest">Value of the 'algorithm' directive in the digest header.
        /// Set this to 'MD5' if the digest header does not contain this directive.</param>
        /// <param name="algorithmForChecksum">Value of the 'algorithm' directive in the checksum header.
        /// Set this to 'MD5' if the digest header does not contain this directive.</param>
        public DigestSecurityToken(
            string digestResponse,
            string userName,
            string nonce,
            string realm,
            string requestUri,
            string httpMethod,
            string algorithmForDigest, 
            string algorithmForChecksum)
        {
            this.digest = digestResponse;
            this.userName = userName;
            this.nonce = nonce;
            this.realm = realm;
            this.algoForDigest = algorithmForDigest;
            this.algoForChecksum = algorithmForChecksum;
            this.requestUri = requestUri;
            this.httpMethod = httpMethod;
            this.validFrom = DateTime.Now;
            this.validUpto = DateTime.Now.AddYears(100);
            this.id = Guid.NewGuid();
            this.securityKeys = new ReadOnlyCollection<SecurityKey>(new List<SecurityKey>(0));
        }

        /// <summary>
        /// Initializes a new instance of the DigestSecurityToken class.
        /// </summary>
        /// <param name="digestResponse">Value of the 'response' directive in the HTTP Digest header.</param>
        /// <param name="userName">LogOn name of the user.</param>
        /// <param name="nonce">Value of the 'nonce' directive in the digest header.</param>
        /// <param name="realm">Value of 'realm' directive in the digest header.</param>
        /// <param name="requestUri">Http request Uri. Se</param>
        /// <param name="httpMethod">Http Method - GET, PUT, POST</param>
        /// <param name="algorithmForDigest">Value of the 'algorithm' directive in the digest header.
        /// Set this to 'MD5' if the digest header does not contain this directive.</param>
        /// <param name="algorithmForChecksum">Value of the 'algorithm' directive in the checksum header.
        /// Set this to 'MD5' if the digest header does not contain this directive.</param>
        public DigestSecurityToken(
            string digestResponse,
            string userName,
            string nonce,
            string realm,
            Uri requestUri,
            string httpMethod,
            string algorithmForDigest,
            string algorithmForChecksum)
        {
            this.digest = digestResponse;
            this.userName = userName;
            this.nonce = nonce;
            this.realm = realm;
            this.algoForDigest = algorithmForDigest;
            this.algoForChecksum = algorithmForChecksum;
            this.requestUri = requestUri.AbsoluteUri;
            this.httpMethod = httpMethod;
            this.validFrom = DateTime.Now;
            this.validUpto = DateTime.Now.AddYears(100);
            this.id = Guid.NewGuid();
            //// Initialize to empty list. This class does not use SecurityKeys collection.
            this.securityKeys = new ReadOnlyCollection<SecurityKey>(new List<SecurityKey>(0));
        }
        #endregion

        #region SecurityToken members
        /// <summary>
        /// Returns id of this token.
        /// </summary>
        public override string Id
        {
            get { return this.id.ToString(); }
        }

        /// <summary>
        /// SecurityKeys collection is not used in DigestSecurityToken.
        /// </summary>
        public override System.Collections.ObjectModel.ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return this.securityKeys; }
        }

        /// <summary>
        /// The instance in time the token is valid from.
        /// </summary>
        public override DateTime ValidFrom
        {
            get { return this.validFrom; }
        }

        /// <summary>
        /// The instance in time the token is valid upto.
        /// </summary>
        public override DateTime ValidTo
        {
            get { return this.validUpto; }
        }
        #endregion

        #region Internal properties
        /// <summary>
        /// Gets 'Response' directive value in the HTTP digest header.
        /// </summary>
        internal string DigestResponse
        {
            get { return this.digest; }
        }

        /// <summary>
        /// Gets the algorithm used for generating the digest response.
        /// </summary>
        internal string DigestAlgorithm
        {
            get { return this.algoForDigest; }
        }

        /// <summary>
        /// Gets the algorithm used for generating the checksum.
        /// </summary>
        internal string ChecksumAlgorithm
        {
            get { return this.algoForChecksum; }
        }

        /// <summary>
        /// Gets realm directive value in the HTTP digest header.
        /// </summary>
        internal string Realm
        {
            get { return this.realm; }
        }

        /// <summary>
        /// Gets nonce value in the HTTP digest header.
        /// </summary>
        internal string Nonce
        {
            get { return this.nonce; }
        }

        /// <summary>
        /// Gets userName value in the HTTP digest header.
        /// </summary>
        internal string UserName
        {
            get { return this.userName; }
        }

        /// <summary>
        /// Gets request Uri (Digest Uri) value in the HTTP digest header.
        /// </summary>
        internal string RequestUri
        {
            get { return this.requestUri; }
        }

        /// <summary>
        /// Gets Http Method used for request
        /// </summary>
        internal string HttpMethod
        {
            get { return this.httpMethod; }
        }

        #endregion
    }
}
