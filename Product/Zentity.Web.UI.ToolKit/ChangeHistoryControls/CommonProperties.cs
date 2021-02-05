// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class contains common properties for styling. Control utilizes these properties
    /// to apply different styles to each type of changes like Scalar Property Changes, Relationship Changes etc.
    /// </summary>
    public sealed class CommonProperty : IDisposable
    {
        #region Constants

        #region Private

        private const string _defaultheaderTitle = "Change History Header Title";

        // For Relationship
        private const string _previousFetchedRelChangesetCount = "PreviouslyFetchedRelChangesetCount";
        private const string _showMoreRelBtnLinkId = "ShowMoreRelBtnId";
        private const string _showAllRelBtnLinkId = "ShowAllRelBtnId";
        private const string _changesetRelCount = "ChangesetRelCount";

        // For Resource Tagging
        private const string _previousFetchedTagChangesetCount = "PreviouslyFetchedTagChangesetCount";
        private const string _changesetTagCount = "ChangesetTagCount";

        // For Resource Categorization
        private const string _previousFetchedCategoryChangesetCount = "PreviouslyFetchedCategoryChangesetCount";
        private const string _changesetCategoryCount = "ChangesetCategoryCount";

        #endregion Private

        #endregion

        #region Member variables

        #region Private

        private TableItemStyle _rowStyle;
        private TableItemStyle _innerTableRowStyle;
        private TableItemStyle _innerTableAlternateRowStyle;
        private TableItemStyle _headerStyle;
        private const int _defaultMaxChangesetCount = 10;
        private Style _linkStyle;
        private TableItemStyle _headerTitleStyle;
        private string _headerTitle;
        private bool isDisposed;

        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the title of each type changes.
        /// </summary>
        public string HeaderTitle
        {
            get
            {
                if (this._headerTitle == null || string.IsNullOrEmpty(this._headerTitle.Trim()))
                {
                    return _defaultheaderTitle;
                }
                else
                {
                    return this._headerTitle;
                }
            }
            set
            {
                _headerTitle = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of change set row.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle RowStyle
        {
            get
            {
                if (this._rowStyle == null)
                {
                    this._rowStyle = new TableItemStyle();
                }
                return this._rowStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of links in each type changes.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public Style LinkStyle
        {
            get
            {
                if (this._linkStyle == null)
                {
                    this._linkStyle = new Style();
                }
                return this._linkStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the header row in collapsible panel.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle InnerTableHeaderStyle
        {
            get
            {
                if (this._headerStyle == null)
                {
                    this._headerStyle = new TableItemStyle();
                }
                return this._headerStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of even rows in collapsible panel.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle InnerTableRowStyle
        {
            get
            {
                if (this._innerTableRowStyle == null)
                {
                    this._innerTableRowStyle = new TableItemStyle();
                }
                return this._innerTableRowStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of odd rows in collapsible panel.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle InnerTableAlternateRowStyle
        {
            get
            {
                if (this._innerTableAlternateRowStyle == null)
                {
                    this._innerTableAlternateRowStyle = new TableItemStyle();
                }
                return this._innerTableAlternateRowStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of title's row of each type changes.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle HeaderStyle
        {
            get
            {
                if (this._headerTitleStyle == null)
                {
                    this._headerTitleStyle = new TableItemStyle();
                }
                return this._headerTitleStyle;
            }
        }

        #endregion

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Inherited method from Idisposable Interface for handling manual dispose of object.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                _linkStyle.Dispose();
                _rowStyle.Dispose();
                _innerTableRowStyle.Dispose();
                _headerStyle.Dispose();
                _headerTitleStyle.Dispose();
                _innerTableAlternateRowStyle.Dispose();
                isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
