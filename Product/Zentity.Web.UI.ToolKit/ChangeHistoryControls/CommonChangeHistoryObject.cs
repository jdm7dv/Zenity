// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Zentity.Administration;

namespace Zentity.Web.UI.ToolKit
{
    internal class CommonChangeHistoryObject
    {
        #region Member variables

        #region Private

        private Table _controlContainer;
        private CommonProperty _styleProperty;
        private string _showMoreBtnId;
        private string _showAllBtnId;
        private int _preFetchedChangesetCnt;
        private int _changeSetCnt;
        private List<RelationshipChange> _relShipChanges = new List<RelationshipChange>();

        #endregion Private

        #endregion

        #region Constructors

        #region Public

        /// <summary>
        /// This parameterized constructor builds an object that will be used to keep related information about 
        /// one of the entity changes among category association changes, relationship changes, resource tagging changes etc...
        /// </summary>
        /// <param name="relShipChanges">List of relationship changes</param>
        /// <param name="controlContainer">Instance of Table</param>
        /// <param name="styleProperty">Instance of CommonPropertyCollection</param>
        /// <param name="showMoreBtnId">Id of "ShowMore" button</param>
        /// <param name="showAllBtnId">Id of "ShowAll" button</param>
        /// <param name="preFetchedChangesetCnt">Specifies number of change sets fetched in earlier operation</param>
        /// <param name="changeSetCnt">Specifies number of change sets</param>
        public CommonChangeHistoryObject(List<RelationshipChange> relShipChanges, Table controlContainer, CommonProperty styleProperty,
                                        string showMoreBtnId, string showAllBtnId,
                                          int preFetchedChangesetCnt, int changeSetCnt)
        {
            this._controlContainer = controlContainer;
            this._styleProperty = styleProperty;
            this._showMoreBtnId = showMoreBtnId;
            this._showAllBtnId = showAllBtnId;
            this._preFetchedChangesetCnt = preFetchedChangesetCnt;
            this._changeSetCnt = changeSetCnt;
            this._relShipChanges = relShipChanges;
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public CommonChangeHistoryObject()
        {
        }

        #endregion Pulic

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets relationship change sets.
        /// </summary>
        internal List<RelationshipChange> RelShipChanges
        {
            get
            {
                return _relShipChanges;
            }
            set
            {
                _relShipChanges = value;
            }
        }

        /// <summary>
        /// Gets or sets number of change sets already fetched from repository.
        /// </summary>
        internal int PreFetchedChangesetCnt
        {
            get
            {
                return _preFetchedChangesetCnt;
            }
            set
            {
                _preFetchedChangesetCnt = value;
            }
        }

        /// <summary>
        /// Gets or sets overall count of change sets belong to an entity.
        /// </summary>
        internal int ChangeSetCnt
        {
            get
            {
                return _changeSetCnt;
            }
            set
            {
                _changeSetCnt = value;
            }
        }

        /// <summary>
        /// Gets or sets id of "ShowAll" button.
        /// </summary>
        internal string ShowAllBtnId
        {
            get
            {
                return _showAllBtnId;
            }
            set
            {
                _showAllBtnId = value;
            }
        }

        /// <summary>
        /// Gets or sets id of "ShowMore" button.
        /// </summary>
        internal string ShowMoreBtnId
        {
            get
            {
                return _showMoreBtnId;
            }
            set
            {
                _showMoreBtnId = value;
            }
        }

        /// <summary>
        /// Gets or sets styling apply to an entity.
        /// </summary>
        internal CommonProperty StyleProperty
        {
            get
            {
                return _styleProperty;
            }
            set
            {
                _styleProperty = value;
            }
        }

        /// <summary>
        /// Gets or sets container holds all controls for an entity.
        /// </summary>
        internal Table ControlContainer
        {
            get
            {
                return _controlContainer;
            }
            set
            {
                _controlContainer = value;
            }
        }

        #endregion

        #endregion
    }
}
