// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Zentity.Core;
using System.Configuration;
using System.ComponentModel;
using System.Globalization;
using Zentity.Security.Authentication;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Represents base class for Toolkit controls.
    /// </summary>
    public abstract class ZentityBase : CompositeControl
    {
        #region MemberVariables
        private bool _isSecurityAwareControl;
        private AuthenticatedToken _authenticatedToken;

        #endregion

        #region Constants

        #region Private

        private const string _connectionViewStateKey = "ConnectionViewStateKey";
        private const string _localSqlServer = "LocalSqlServer";
        private const string _entityClient = "System.Data.EntityClient";
        private const string _providerConString = "provider connection string=";
        private const string _connectionTimeout = "Connection Timeout";
        private const string _connectTimeout = "Connect Timeout";
        private const string _titleViewStateKey = "Title";
        private const string _cssFilePath = "Zentity.Web.UI.ToolKit.StyleSheet.DefaultToolKit.css";
        private const int _minimumConnTimeoutInterval = 300;
        private const char _quote = '"';
        private const char _invertedComma = '\'';
        private TableItemStyle _titleStyle;

        #endregion Private

        #endregion Constants

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the title of the control.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionTableTitle")]
        [Localizable(true)]
        public string Title
        {
            get
            {
                return ViewState[_titleViewStateKey] != null ? (string)ViewState[_titleViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_titleViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionConnectionString")]
        [Localizable(true)]
        public virtual string ConnectionString
        {
            get
            {
                return ViewState[_connectionViewStateKey] != null ? (string)ViewState[_connectionViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_connectionViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the Title row.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableTitleStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle TitleStyle
        {
            get
            {
                if (this._titleStyle == null)
                {
                    this._titleStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._titleStyle).TrackViewState();
                    }
                }
                return this._titleStyle;
            }
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether the control is security aware or not.
        /// </summary>
        public bool IsSecurityAwareControl
        {
            get { return _isSecurityAwareControl; }
            set { _isSecurityAwareControl = value; }
        }

        /// <summary>
        /// Gets or sets the authenticated token to be used if the control is security aware.
        /// </summary>
        public AuthenticatedToken AuthenticatedToken
        {
            get { return _authenticatedToken; }
            set { _authenticatedToken = value; }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Handles the loading of the control.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            string includeTemplate = "<link rel='stylesheet' text='text/css' href='{0}' />";
            string includeLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), _cssFilePath);
            LiteralControl include = new LiteralControl(String.Format(CultureInfo.InvariantCulture, includeTemplate, includeLocation));
            ((HtmlHead)Page.Header).Controls.Add(include);

        }

        #region Public

        /// <summary>
        /// Creates instance of <see cref="ZentityContext"/> class with specified connection string if provided otherwise 
        /// with default connection string.
        /// </summary>
        /// <returns>Instance of <see cref="ZentityContext"/>.</returns>
        public ZentityContext CreateContext()
        {
            ZentityContext context = null;

            // if connection string is empty then creates default context
            if (string.IsNullOrEmpty(this.ConnectionString))
            {
                context = new ZentityContext();
            }
            else
            {
                context = new ZentityContext(this.ConnectionString);
            }
            context.CommandTimeout = this.GetConnectionTimeoutInterval();

            CoreHelper.LoadMetadata(context);

            return context;
        }

        #endregion

        #region Private

        /// <summary>
        /// Gets "connection timeout" period from connection string.
        /// </summary>
        /// <returns>Connection timeout interval</returns>
        private int GetConnectionTimeoutInterval()
        {
            string actualConnectionString = string.Empty;
            int timeoutInterval;

            // True if Property "ConnectionString" was not provided for control.
            if (string.IsNullOrEmpty(this.ConnectionString))
            {
                // Get connection strings from configuration file
                ConnectionStringSettingsCollection collection = ConfigurationManager.ConnectionStrings;
                foreach (ConnectionStringSettings connection in collection)
                {
                    // True for connection string that points to "Zentity Database".
                    if ((!connection.Name.Equals(_localSqlServer, StringComparison.Ordinal)) && connection.ProviderName.Equals(_entityClient, StringComparison.Ordinal))
                    {
                        actualConnectionString = connection.ConnectionString;
                        break;
                    }
                }

            }
            else
            {
                actualConnectionString = this.ConnectionString;

            }
            if (string.IsNullOrEmpty(actualConnectionString))
            {
                return _minimumConnTimeoutInterval;
            }

            // Get connection string without "provider connection string".
            actualConnectionString = actualConnectionString.Split(new string[] { _providerConString }, StringSplitOptions.RemoveEmptyEntries).Last();
            actualConnectionString = actualConnectionString.Trim(_invertedComma).Trim(_quote).Trim();

            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(actualConnectionString))
            {
                timeoutInterval = connection.ConnectionTimeout;

                // Set timeout interval to minimum period if connection timeout did not specify in connection
                // string.
                if (timeoutInterval <= _minimumConnTimeoutInterval)
                {
                    timeoutInterval = _minimumConnTimeoutInterval;
                }
            }

            return timeoutInterval;
        }

        #endregion

        #endregion
    }
}