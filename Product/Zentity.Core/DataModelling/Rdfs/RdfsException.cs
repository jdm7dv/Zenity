// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Security.Permissions;
using System.Globalization;
using System.Runtime.Serialization;
using System.Resources;
using Zentity.Core;

namespace Zentity.Core
{
    /// <summary>
    /// Represents erorrs while parsing the RDF Graph.
    /// </summary>
    [Serializable]
    internal sealed class RdfsException : ZentityException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RdfsException"/> class.
        /// </summary>
        internal RdfsException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RdfsException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        internal RdfsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RdfsException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RdfsException(string message, Exception inner) :
            base(message, inner)
        {
        }
    }
}
