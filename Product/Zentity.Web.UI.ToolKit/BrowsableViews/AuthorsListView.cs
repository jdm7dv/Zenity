// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control allows user to view and filter list of authors.
    /// </summary>
    /// <example>
    ///     The code below is the source for AuthorsListView.aspx.
    ///     It shows an example of using <see cref="AuthorsListView"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="Zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head id="Head1" runat="server"&gt;
    ///             &lt;title&gt;AuthorsListView Sample&lt;/title&gt;
    ///             
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_PreRender(object sender, EventArgs e)
    ///                 {
    ///                     // Display the search string.
    ///                     SearchStringLabel.Text = AuthorsListView1.SelectedSearchValue;
    ///                 }
    ///             &lt;/script&gt;
    ///             
    ///          &lt;/head&gt;
    ///          &lt;body&gt;
    ///             &lt;form id="mainForm" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:AuthorsListView ID="AuthorsListView1" runat="server" IsSecurityAwareControl="false"&gt;
    ///                 &lt;/Zentity:AuthorsListView&gt;
    ///                 &lt;br /&gt;
    ///                 Search string:
    ///                 &lt;asp:Label ID="SearchStringLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [DefaultProperty("Text")]
    [Designer(typeof(AuthorsListViewDesigner))]
    public class AuthorsListView : ZentityBase
    {
        #region Member Variables

        private Panel _containerPanel;
        private Panel _titlePanel;
        private Label _titleLabel;
        private TextBox _filterTextBox;
        private Button _goButton;
        private ListBox _authorsListBox;
        private LinkButton _nextButton;
        private LinkButton _prevButton;
        private LinkButton _AtoELink;
        private LinkButton _FtoJLink;
        private LinkButton _KtoOLink;
        private LinkButton _PtoTLink;
        private LinkButton _UtoZLink;
        private LinkButton _otherLink;
        private TableItemStyle _containerStyle;
        private Style _buttonStyle;
        private HtmlContainerControl _scrollableContainer = new HtmlGenericControl();
        #endregion

        #region Constants

        private const string _filterTextBoxId = "filterTextBoxId";
        private const string _goButtonId = "goButtonId";
        private const string _containerPanelId = "containerPanelId";
        private const string _titlePanelId = "titlePanelId";
        private const string _titleLabelId = "titleLabelId";
        private const string _authorsListBoxId = "authorsListBoxId";
        private const string _nextButtonId = "nextButtonId";
        private const string _prevButtonId = "prevButtonId";
        private const string _AtoELinkId = "AtoELinkId";
        private const string _FtoJLinkId = "FtoJLinkId";
        private const string _KtoOLinkId = "KtoOLinkId";
        private const string _PtoTLinkId = "PtoTLinkId";
        private const string _UtoZLinkId = "UtoZLinkId";
        private const string _otherLinkId = "otherLinkId";

        private const string _nodeAE = "a-eA-E";
        private const string _nodeFJ = "f-jF-J";
        private const string _nodeKO = "k-oK-O";
        private const string _nodePT = "p-tP-T";
        private const string _nodeUZ = "u-zU-Z";

        private const string _pageIndexKey = "pageIndexKey";
        private const string _pageSizeKey = "pageSizeKey";
        private const string _pageCountKey = "pageCountKey";

        private const string _contactClass = "Contact";
        private const string _idProperty = "Id";
        private const string _titleProperty = "Title";

        private const string _searchAuthorString = "author:(resourcetype:contact Id:='{0}')";

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets selected Search value.
        /// </summary>
        public string SelectedSearchValue
        {
            get
            {
                if (!string.IsNullOrEmpty(AuthorsListBox.SelectedValue))
                {
                    return string.Format(CultureInfo.InstalledUICulture, _searchAuthorString, AuthorsListBox.SelectedValue);
                }
                return string.Empty;
            }
        }

        /// <summary>
        ///  Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the container panel.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         ZentityCategory("CategoryApperance"),
         NotifyParentProperty(true),
         PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ContainerStyle
        {
            get
            {
                if (this._containerStyle == null)
                {
                    this._containerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._containerStyle).TrackViewState();
                    }
                }
                return this._containerStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the buttons.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public Style ButtonStyle
        {
            get
            {
                if (this._buttonStyle == null)
                {
                    this._buttonStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._buttonStyle).TrackViewState();
                    }
                }
                return this._buttonStyle;
            }
        }

        /// <summary>
        /// Occurs when the selection from the list control changes between posts to the server.
        /// </summary>
        public event EventHandler<EventArgs> SelectedIndexChanged;

        #endregion

        #region Private

        private Panel ContainerPanel
        {
            get
            {
                if (_containerPanel == null)
                {
                    _containerPanel = new Panel();
                    _containerPanel.ID = _containerPanelId;
                }

                return _containerPanel;
            }
        }

        private Panel TitlePanel
        {
            get
            {
                if (_titlePanel == null)
                {
                    _titlePanel = new Panel();
                    _titlePanel.ID = _titlePanelId;
                    _titlePanel.Controls.Add(TitleLabel);
                }

                return _titlePanel;
            }
        }

        private Label TitleLabel
        {
            get
            {
                if (_titleLabel == null)
                {
                    _titleLabel = new Label();
                    _titleLabel.ID = _titleLabelId;
                    _titleLabel.Text = GlobalResource.AuthorsText;
                }

                return _titleLabel;
            }
        }

        private TextBox FilterTextBox
        {
            get
            {
                if (_filterTextBox == null)
                {
                    _filterTextBox = new TextBox();
                    _filterTextBox.ID = _filterTextBoxId;
                    _filterTextBox.Width = Unit.Percentage(80);
                }

                return _filterTextBox;
            }
        }

        private Button GoButton
        {
            get
            {
                if (_goButton == null)
                {
                    _goButton = new Button();
                    _goButton.ID = _goButtonId;
                    _goButton.Text = GlobalResource.ButtonSubmitText;
                    _goButton.Width = Unit.Percentage(15);
                    _goButton.Click += new EventHandler(GoButton_Click);
                }

                return _goButton;
            }
        }

        private ListBox AuthorsListBox
        {
            get
            {
                if (_authorsListBox == null)
                {
                    _authorsListBox = new ListBox();
                    _authorsListBox.ID = _authorsListBoxId;
                    //_authorsListBox.Width = Unit.Percentage(65);
                    _authorsListBox.Height = Unit.Pixel(150);
                    _authorsListBox.SelectionMode = ListSelectionMode.Single;
                    _authorsListBox.DataMember = _contactClass;
                    _authorsListBox.DataTextField = _titleProperty;
                    _authorsListBox.DataValueField = _idProperty;
                    _authorsListBox.AutoPostBack = true;
                    _authorsListBox.SelectedIndexChanged += new EventHandler(AuthorsListBox_SelectedIndexChanged);
                }

                return _authorsListBox;
            }
        }

        private LinkButton NextButton
        {
            get
            {
                if (_nextButton == null)
                {
                    _nextButton = new LinkButton();
                    _nextButton.ID = _nextButtonId;
                    _nextButton.Text = GlobalResource.ButtonNextText;
                    _nextButton.Width = Unit.Percentage(15);
                    _nextButton.Style.Add(HtmlTextWriterStyle.TextAlign, GlobalResource.CssButtonTextAlignCenter);
                    _nextButton.Click += new EventHandler(NavigationButton_Click);
                    _nextButton.Enabled = false;
                }

                return _nextButton;
            }
        }

        private LinkButton PrevButton
        {
            get
            {
                if (_prevButton == null)
                {
                    _prevButton = new LinkButton();
                    _prevButton.ID = _prevButtonId;
                    _prevButton.Text = GlobalResource.ButtonPrevText;
                    _prevButton.Width = Unit.Percentage(15);
                    _prevButton.Style.Add(HtmlTextWriterStyle.TextAlign, GlobalResource.CssButtonTextAlignCenter);
                    _prevButton.Click += new EventHandler(NavigationButton_Click);
                    _prevButton.Enabled = false;
                }

                return _prevButton;
            }
        }

        private LinkButton AtoELink
        {
            get
            {
                if (_AtoELink == null)
                {
                    _AtoELink = new LinkButton();
                    _AtoELink.ID = _AtoELinkId;
                    _AtoELink.Text = GlobalResource.A_EText;
                    _AtoELink.Click += new EventHandler(AuthorsLink_Click);
                }

                return _AtoELink;
            }
        }

        private LinkButton FtoJLink
        {
            get
            {
                if (_FtoJLink == null)
                {
                    _FtoJLink = new LinkButton();
                    _FtoJLink.ID = _FtoJLinkId;
                    _FtoJLink.Text = GlobalResource.F_JText;
                    _FtoJLink.Click += new EventHandler(AuthorsLink_Click);
                }

                return _FtoJLink;
            }
        }

        private LinkButton KtoOLink
        {
            get
            {
                if (_KtoOLink == null)
                {
                    _KtoOLink = new LinkButton();
                    _KtoOLink.ID = _KtoOLinkId;
                    _KtoOLink.Text = GlobalResource.K_OText;
                    _KtoOLink.Click += new EventHandler(AuthorsLink_Click);
                }

                return _KtoOLink;
            }
        }

        private LinkButton PtoTLink
        {
            get
            {
                if (_PtoTLink == null)
                {
                    _PtoTLink = new LinkButton();
                    _PtoTLink.ID = _PtoTLinkId;
                    _PtoTLink.Text = GlobalResource.P_TText;
                    _PtoTLink.Click += new EventHandler(AuthorsLink_Click);
                }

                return _PtoTLink;
            }
        }

        private LinkButton UtoZLink
        {
            get
            {
                if (_UtoZLink == null)
                {
                    _UtoZLink = new LinkButton();
                    _UtoZLink.ID = _UtoZLinkId;
                    _UtoZLink.Text = GlobalResource.U_ZText;
                    _UtoZLink.Click += new EventHandler(AuthorsLink_Click);
                }

                return _UtoZLink;
            }
        }

        private LinkButton OtherLink
        {
            get
            {
                if (_otherLink == null)
                {
                    _otherLink = new LinkButton();
                    _otherLink.ID = _otherLinkId;
                    _otherLink.Text = GlobalResource.OthersText;
                    _otherLink.Click += new EventHandler(AuthorsLink_Click);
                }

                return _otherLink;
            }
        }

        private int PageIndex
        {
            get
            {
                return ViewState[_pageIndexKey] != null ? (int)ViewState[_pageIndexKey] : 0;
            }
            set
            {
                ViewState[_pageIndexKey] = value;
            }
        }

        private int PageSize
        {
            get
            {
                return ViewState[_pageSizeKey] != null ? (int)ViewState[_pageSizeKey] : 50;
            }
        }

        private int PageCount
        {
            get
            {
                return ViewState[_pageCountKey] != null ? (int)ViewState[_pageCountKey] : 0;
            }
            set
            {
                ViewState[_pageCountKey] = value;
            }
        }

        #endregion


        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Clears selections of item in Authors ListBox.
        /// </summary>
        public void ClearSelection()
        {
            if (AuthorsListBox.SelectedItem != null)
                AuthorsListBox.SelectedItem.Selected = false;
        }

        #endregion

        #region Protected

        /// <summary>
        /// Creates child controls layout.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ContainerPanel.Controls.Add(TitlePanel);

            Panel linkPanel = new Panel();
            linkPanel.Style.Add(HtmlTextWriterStyle.Padding, GlobalResource.CssPanelPadding);
            linkPanel.Controls.Add(AtoELink);
            linkPanel.Controls.Add(CreateSpace());
            linkPanel.Controls.Add(FtoJLink);
            linkPanel.Controls.Add(CreateSpace());
            linkPanel.Controls.Add(KtoOLink);
            linkPanel.Controls.Add(CreateSpace());
            linkPanel.Controls.Add(PtoTLink);
            linkPanel.Controls.Add(CreateSpace());
            linkPanel.Controls.Add(UtoZLink);
            linkPanel.Controls.Add(CreateSpace());
            linkPanel.Controls.Add(OtherLink);
            ContainerPanel.Controls.Add(linkPanel);

            Panel filterPanel = new Panel();
            filterPanel.Style.Add(HtmlTextWriterStyle.Padding, GlobalResource.CssPanelPadding);
            filterPanel.Controls.Add(FilterTextBox);
            filterPanel.Controls.Add(GoButton);
            filterPanel.DefaultButton = GoButton.ID;
            ContainerPanel.Controls.Add(filterPanel);

            _scrollableContainer.Style.Add("width", GlobalResource.CssScrollableContainerWidth);
            _scrollableContainer.Attributes.Add("class", "tableContainer1");
            _scrollableContainer.Controls.Add(AuthorsListBox);

            if (this.DesignMode)
            {
                FilterTextBox.Width = new Unit(GlobalResource.CssTextFilterWidth, CultureInfo.InvariantCulture);
                AuthorsListBox.Width = new Unit(GlobalResource.CssListAuthorWidth, CultureInfo.InvariantCulture);
            }

            Panel authorsPanel = new Panel();
            authorsPanel.Style.Add(HtmlTextWriterStyle.Padding, GlobalResource.CssPanelPadding);
            authorsPanel.Style.Add(HtmlTextWriterStyle.VerticalAlign, GlobalResource.CssVerticalAlignBottom);
            authorsPanel.Controls.Add(PrevButton);
            authorsPanel.Controls.Add(_scrollableContainer);
            authorsPanel.Controls.Add(NextButton);
            ContainerPanel.Controls.Add(authorsPanel);

            this.Controls.Add(ContainerPanel);

            RegisterScript(_scrollableContainer);
        }


        /// <summary>
        /// Register script for the scrollable container.
        /// </summary>
        /// <param name="scrollableContainer">Scrollable container.</param>
        private void RegisterScript(HtmlContainerControl scrollableContainer)
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script language='javascript'>");
            script.Append("if(document.getElementById('" + scrollableContainer.ClientID + "') != null){");
            script.Append("if(document.getElementById('" + scrollableContainer.ClientID + "').clientWidth != null){");
            script.Append("if(document.getElementById('" + AuthorsListBox.ClientID + "').clientWidth != null){");
            script.Append("if(document.getElementById('" + AuthorsListBox.ClientID + "').length > 0){");
            script.Append("}else{");
            script.Append("document.getElementById('" + AuthorsListBox.ClientID + "').style.width = '100%';");
            script.Append("}}}}");
            script.Append("</script>");
            this.Page.ClientScript.RegisterStartupScript(Page.GetType(), scrollableContainer.ClientID, script.ToString());
        }

        /// <summary>
        /// Applies style to the controls and sets the title.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            this.TitleLabel.Text = Title;
            ApplyStyle();

            base.OnPreRender(e);
        }

        #endregion

        #region Private

        void AuthorsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(sender, e);
        }

        private void NavigationButton_Click(object sender, EventArgs e)
        {
            if (sender == NextButton)
            {
                PageIndex += 1;
            }
            else
            {
                if (PageIndex > 0)
                    PageIndex -= 1;
            }

            Refresh();
        }

        private void AuthorsLink_Click(object sender, EventArgs e)
        {
            PageIndex = 0;
            AtoELink.Font.Bold = FtoJLink.Font.Bold = KtoOLink.Font.Bold =
                PtoTLink.Font.Bold = UtoZLink.Font.Bold = OtherLink.Font.Bold = false;


            if (sender == AtoELink)
            {
                AtoELink.Font.Bold = true;
            }
            else if (sender == FtoJLink)
            {
                FtoJLink.Font.Bold = true;
            }
            else if (sender == KtoOLink)
            {
                KtoOLink.Font.Bold = true;
            }
            else if (sender == PtoTLink)
            {
                PtoTLink.Font.Bold = true;

            }
            else if (sender == UtoZLink)
            {
                UtoZLink.Font.Bold = true;
            }
            else if (sender == OtherLink)
            {
                OtherLink.Font.Bold = true;
            }

            Refresh();

        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            PageIndex = 0;
            Refresh();
        }

        private static Control CreateSpace()
        {
            Label space = new Label();
            space.Text = " | ";
            return space;
        }

        private void Refresh()
        {
            IList<Contact> authors = null;
            string range = string.Empty;
            AuthorsListBox.Items.Clear();

            //Find selected bucket
            if (AtoELink.Font.Bold)
            {
                range = Constants.A_EAuthors;
            }
            else if (FtoJLink.Font.Bold)
            {
                range = Constants.F_JAuthors;
            }
            else if (KtoOLink.Font.Bold)
            {
                range = Constants.K_OAuthors;
            }
            else if (PtoTLink.Font.Bold)
            {
                range = Constants.P_TAuthors;
            }
            else if (UtoZLink.Font.Bold)
            {
                range = Constants.U_ZAuthors;
            }
            else if (OtherLink.Font.Bold)
            {
                range = Constants.OtherAuthors;
            }

            int fetchRecordCount = 0;
            int totalRecords = 0;

            //Calculate fetched record count
            if (PageIndex > 0)
                fetchRecordCount = PageSize * this.PageIndex;

            //Fetch authors
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                if (IsSecurityAwareControl)
                {
                    if (AuthenticatedToken != null)
                    {
                        authors = (IList<Contact>)dataAccess.GetAuthors(this.AuthenticatedToken, range,
                            FilterTextBox.Text.Trim(), PageSize, fetchRecordCount, out totalRecords);
                    }
                }
                else
                {
                    authors = (IList<Contact>)dataAccess.GetAuthors(null, range,
                        FilterTextBox.Text.Trim(), PageSize, fetchRecordCount, out totalRecords);
                }
            }

            //Set Page count.
            if (totalRecords > 0 && totalRecords > PageSize)
                PageCount = Convert.ToInt32(Math.Ceiling((double)totalRecords / PageSize));
            else
                PageCount = 0;

            //Set status of Next and Previous navigation buttons
            NextButton.Enabled = PrevButton.Enabled = false;
            NextButton.Enabled = PageCount == 1 | PageCount <= (PageIndex + 1) ? false : true;
            PrevButton.Enabled = PageIndex == 0 ? false : true;

            if (authors != null)
            {
                foreach (Contact contact in authors)
                {
                    string title = contact.Title;
                    contact.Title = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.GetTitleByResourceType(contact), 30));

                    Person person = contact as Person;
                    if (person != null)
                    {
                        if (!string.IsNullOrEmpty(person.FirstName) || !string.IsNullOrEmpty(person.LastName))
                        {
                            contact.Title += "(" + title + ")";
                        }
                    }
                }
            }

            AuthorsListBox.DataSource = authors;
            AuthorsListBox.DataBind();
        }

        private void ApplyStyle()
        {
            this.ContainerPanel.ApplyStyle(ContainerStyle);
            this.TitlePanel.ApplyStyle(TitleStyle);
            this.GoButton.ApplyStyle(ButtonStyle);
        }

        #endregion

        #endregion
    }


  
}

