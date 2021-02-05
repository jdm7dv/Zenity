// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control provides paging functionality.
    /// </summary>
    [DefaultProperty("Text")]
    public class Pager : WebControl, INamingContainer
    {
        #region Member Variables

        private TextBox _pageIndexTextBox;
        private Label _pageCountLabel;
        private Table _pagingTable;
        private LinkButton _firstButton;
        private LinkButton _nextButton;
        private LinkButton _previousButton;
        private LinkButton _lastButton;

        #endregion

        #region Constants

        private const string _firstPageButtonId = "FirstPageButton";
        private const string _previousPageButtonId = "PreviousPageButton";
        private const string _nextPageButtonId = "NextPageButton";
        private const string _lastPageButtonId = "LastPageButton";
        private const string _pageLabelId = "PageLabel";
        private const string _ofLabelId = "OfLabel";
        private const string _pageIndexLabelId = "PageIndexLabel";
        private const string _pageCountLabelId = "PageCountLabel";
        private const string _defaultPageIndexText = "1";
        private const string _defaultPageCountText = "1";
        private const string _pageIndexKey = "pageIndexKey";
        private const string _totalPagesKey = "totalPagesKey";

        private const string _javascriptFile = "JavascriptFile";
        private const string _searchControlScriptPath = "Zentity.Web.UI.ToolKit.ResourceViewControls.ResourceViewScript.js";
        private const string _OnKeyPressEvent = "onkeypress";
        private const string _handleKeyPressEvent = "javascript:return HandleKeyPressEvent('{0}',{1},{2});";


        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets page index.
        /// </summary>
        public int PageIndex
        {
            get { return ViewState[_pageIndexKey] != null ? (int)ViewState[_pageIndexKey] : 0; }
            set
            {
                if (value < 0)
                    throw new ArgumentException(GlobalResource.MsgPageIndexLessThanZero);

                ViewState[_pageIndexKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets total pages.
        /// </summary>
        public int TotalPages
        {
            get { return ViewState[_totalPagesKey] != null ? (int)ViewState[_totalPagesKey] : 0; }
            set
            {
                if (value < 0)
                    throw new ArgumentException(GlobalResource.MsgTotalPagesLessThanZero);

                ViewState[_totalPagesKey] = value;
            }
        }

        #endregion

        #region Private

        private Table PagingTable
        {
            get
            {
                if (_pagingTable == null)
                    _pagingTable = CreatePagingTable();
                return _pagingTable;
            }
        }

        private TextBox PageIndexTextBox
        {
            get
            {
                if (_pageIndexTextBox == null)
                {
                    _pageIndexTextBox = new TextBox();
                    _pageIndexTextBox.Width = 30;
                    _pageIndexTextBox.MaxLength = 5;
                    _pageIndexTextBox.TextChanged += new EventHandler(pageIndexLabel_TextChanged);
                    _pageIndexTextBox.ID = _pageIndexLabelId;
                    _pageIndexTextBox.Text = _defaultPageIndexText;
                }

                return _pageIndexTextBox;
            }
        }

        private LinkButton FirstButton
        {
            get
            {
                if (_firstButton == null)
                {
                    _firstButton = new LinkButton();
                    _firstButton.ID = _firstPageButtonId;
                    _firstButton.Text = Resources.GlobalResource.LinkButtonFirstText;
                    _firstButton.Click += new EventHandler(firstButton_Click);
                }
                return _firstButton;
            }
        }

        private LinkButton PreviousButton
        {
            get
            {
                if (_previousButton == null)
                {
                    _previousButton = new LinkButton();
                    _previousButton.ID = _previousPageButtonId;
                    _previousButton.Text = Resources.GlobalResource.LinkButtonPreviousText;
                    _previousButton.Click += new EventHandler(previousButton_Click);
                }
                return _previousButton;
            }
        }

        private LinkButton NextButton
        {
            get
            {
                if (_nextButton == null)
                {
                    _nextButton = new LinkButton();
                    _nextButton.ID = _nextPageButtonId;
                    _nextButton.Text = Resources.GlobalResource.LinkButtonNextText;
                    _nextButton.Click += new EventHandler(nextButton_Click);
                }
                return _nextButton;
            }
        }

        private LinkButton LastButton
        {
            get
            {
                if (_lastButton == null)
                {
                    _lastButton = new LinkButton();
                    _lastButton.ID = _lastPageButtonId;
                    _lastButton.Text = Resources.GlobalResource.LinkButtonLastText;
                    _lastButton.Click += new EventHandler(lastButton_Click);

                }
                return _lastButton;
            }
        }

        private Label PageCountLabel
        {
            get
            {
                if (_pageCountLabel == null)
                {
                    _pageCountLabel = new Label();
                    _pageCountLabel.ID = _pageCountLabelId;
                    _pageCountLabel.Text = _defaultPageCountText;
                }

                return _pageCountLabel;
            }
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// This event is fired when page is changed.
        /// </summary>
        public event EventHandler<EventArgs> OnPageChanged;

        #endregion

        #region Methods

        #region Protected

        /// <summary>
        /// Register javascript;
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Register javascript for pager control
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _javascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _searchControlScriptPath));

            base.OnLoad(e);
        }

        /// <summary>
        /// Creates child controls layout.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Add child controls.
            this.Controls.Add(PagingTable);

        }

        /// <summary>
        /// Updates the status of child controls.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            //Update controls status.
            PageIndexTextBox.Text = (PageIndex + 1).ToString(CultureInfo.CurrentCulture);
            PageCountLabel.Text = TotalPages.ToString(CultureInfo.CurrentCulture);

            NextButton.Enabled = LastButton.Enabled = PageIndex >= (TotalPages - 1) ? false : true;
            FirstButton.Enabled = PreviousButton.Enabled = PageIndex <= 0 ? false : true;

            //register javascript function.
            PageIndexTextBox.Attributes.Add(_OnKeyPressEvent,
                string.Format(CultureInfo.InvariantCulture, _handleKeyPressEvent, PageIndexTextBox.ClientID, 1, TotalPages));

            base.OnPreRender(e);
        }

        #endregion

        #region Private

        private Table CreatePagingTable()
        {
            Label pageLabel = new Label();
            pageLabel.ID = _pageLabelId;
            pageLabel.Text = Resources.GlobalResource.LabelPageText;

            Label OfLabel = new Label();
            OfLabel.ID = _ofLabelId;
            OfLabel.Text = Resources.GlobalResource.LabelOfText;

            Table pagingTable = new Table();

            TableRow row = new TableRow();
            TableCell firstCell = new TableCell();
            firstCell.Controls.Add(FirstButton);
            row.Cells.Add(firstCell);

            TableCell prevCell = new TableCell();
            prevCell.Controls.Add(PreviousButton);
            row.Cells.Add(prevCell);

            TableCell pageCell = new TableCell();
            pageCell.Controls.Add(pageLabel);
            row.Cells.Add(pageCell);

            TableCell pageIndexCell = new TableCell();
            pageIndexCell.Controls.Add(PageIndexTextBox);
            row.Cells.Add(pageIndexCell);

            TableCell ofCell = new TableCell();
            ofCell.Controls.Add(OfLabel);
            row.Cells.Add(ofCell);

            TableCell pageCountCell = new TableCell();
            pageCountCell.Controls.Add(PageCountLabel);
            row.Cells.Add(pageCountCell);

            TableCell nextCell = new TableCell();
            nextCell.Controls.Add(NextButton);
            row.Cells.Add(nextCell);

            TableCell lastCell = new TableCell();
            lastCell.Controls.Add(LastButton);
            row.Cells.Add(lastCell);

            pagingTable.Rows.Add(row);

            return pagingTable;
        }

        private void firstButton_Click(object sender, EventArgs e)
        {
            PageIndex = 0;

            if (OnPageChanged != null)
                OnPageChanged(sender, e);
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            PageIndex = PageIndex > 0 ? PageIndex - 1 : 0;

            if (OnPageChanged != null)
                OnPageChanged(sender, e);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            PageIndex = PageIndex < (TotalPages - 1) && PageIndex >= 0 ? PageIndex + 1 : 0;

            if (OnPageChanged != null)
                OnPageChanged(sender, e);
        }

        private void lastButton_Click(object sender, EventArgs e)
        {
            PageIndex = TotalPages > 0 ? TotalPages - 1 : 0;

            if (OnPageChanged != null)
                OnPageChanged(sender, e);
        }

        private void pageIndexLabel_TextChanged(object sender, EventArgs e)
        {
            int pageIndex = 0;
            if (int.TryParse(PageIndexTextBox.Text, out pageIndex) && pageIndex != this.PageIndex)
            {
                if (pageIndex > TotalPages)
                    this.PageIndex = TotalPages;
                else if (pageIndex < 0)
                    this.PageIndex = 0;
                else
                    this.PageIndex = pageIndex - 1;
            }

            if (OnPageChanged != null)
                OnPageChanged(sender, e);
        }

        #endregion

        #endregion
    }
}
