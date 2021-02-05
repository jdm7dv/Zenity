// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.ObjectModel;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class contains delete event data.
    /// </summary>
    public class DeleteEventArgs : EventArgs
    {
        #region Member variables

        #region Private
        private Collection<Guid> _entityIdList;
        #endregion

        #endregion

        #region Constructors & finalizers

        #region Public

        /// <summary>
        /// Constructor to set list of resource Ids.
        /// </summary>
        /// <param name="entityIdList">List of resource Ids.</param>
        public DeleteEventArgs(Collection<Guid> entityIdList)
        {
            this._entityIdList = entityIdList;
        }

        #endregion

        #endregion

        #region Properties
        /// <summary>
        /// Gets Id of selected item in the GridView.
        /// </summary>
        public Collection<Guid> EntityIdList
        {
            get { return this._entityIdList; }
        }
        #endregion
    }
}
