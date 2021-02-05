// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Event Argument class to pass entity type in the event
    /// </summary>
    [Serializable]
    public class EntityTypeEventArgs : EventArgs
    {
        #region Member variables

        #region Private
        private string _entityType;
        #endregion

        #endregion

        #region Constructors & finalizers

        #region Public

        /// <summary>
        /// Initialize entity type.
        /// </summary>
        /// <param name="entityType">Entity Type.</param>
        public EntityTypeEventArgs(string entityType)
        {
            this._entityType = entityType;
        }

        #endregion

        #endregion

        #region Properties

        #region Public
        /// <summary>
        /// Gets entity type.
        /// </summary>
        public string EntityType
        {
            get { return this._entityType; }
        }
        #endregion
        #endregion
    }
}
