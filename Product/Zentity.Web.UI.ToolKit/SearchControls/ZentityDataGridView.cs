// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using System.Web.UI;
using System.Globalization;
using Zentity.Core;
using System.Web;
using Zentity.Web.UI.ToolKit.Resources;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Permissions;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control provides Abstract GridView class to 
    /// handle Entities.
    /// </summary>
    public abstract class ZentityDataGridView : GridView
    {
        #region Constants

        #region Private

        private const string _gridDataSourceViewStateKey = "GRID_DATA_SOURCE";
        private const string _idColumn = "Id";
        private const string _sortDirectionViewStateKey = "SortDirection";
        private const string _sortExpressionViewStateKey = "SortExpression";
        private const string _deleteButtonCheckBox = "DeleteButtonCheckBox";
        private const string _headerDeleteButtonCheckBox = "HeaderDeleteButtonCheckBox";
        private const string _deleteButton = "DeleteButton";
        private const string _viewUrlViewStateKey = "ViewUrl";
        private const string _editUrlViewStateKey = "EditUrl";
        private const string _permissionUrlViewStateKey = "PermissionUrl";
        private const string _editAssociationUrlViewStateKey = "EditAssociationUrl";
        private const string _enableDeleteViewStateKey = "EnableDelete";
        private const string _javascriptFile = "JavascriptFile";
        private const string _searchControlScriptPath = "Zentity.Web.UI.ToolKit.SearchControls.SearchControlScript.js";

        private const string _defaultEnableDeleteValue = "true";
        private const string _validateDeleteCheckBoxSelection = "javascript: return ValidateDeleteCheckBoxSelection('{0}','{1}', '{2}');";
        private const string _selectDeselectAll = "javascript: SelectDeselectAll('{0}','{1}');";
        private const string _selectDeselectHeaderCheckBox = "javascript: SelectDeselectHeaderCheckBox('{0}','{1}');";
        private const string _onClickEvent = "OnClick";

        private const string _firstPageButtonId = "FirstPageButton";
        private const string _previousPageButtonId = "PreviousPageButton";
        private const string _nextPageButtonId = "NextPageButton";
        private const string _lastPageButtonId = "LastPageButton";
        private const string _pageLabelId = "PageLabel";
        private const string _ofLabelId = "OfLabel";
        private const string _pageIndexLabelId = "PageIndexLabel";
        private const string _pageCountLabelId = "PageCountLabel";
        private const string _pageIndexKey = "PageIndex";
        private const string _pageCountKey = "PageCount";
        private const string _defaultPageIndexText = "1";
        private const string _defaultPageCountText = "1";
        private const string _OnKeyPressEvent = "onkeypress";
        private const string _handleKeyPressEvent = "javascript:return HandleKeyPressEvent('{0}',{1},{2});";

        private const string _showCommandColumnsKey = "ShowCommandColumnsKey";
        private const string _showExtraColumnsKey = "ShowExtraColumnsKey";


        #endregion

        #region Protected

        #endregion

        #endregion

        #region Member variables

        #region Private

        //Event keys
        private static readonly object _deleteEventKey = new object();
        private ColumnCollection _displayColumnCollection = new ColumnCollection();
        private string _connectionString;
        TableItemStyle _buttonStyle;
        Table _pagingTable;
        private static readonly object _pageCalledKey = new object();

        #endregion

        #endregion

        #region Constructors & finalizers

        #region Public
        /// <summary>
        /// Initializes a new instance of <see cref="ZentityDataGridView"/>.
        /// </summary>
        protected ZentityDataGridView()
            : base()
        {
            AutoGenerateColumns = false;
            this.AllowPaging = true;
            this.ShowFooter = this.ShowHeader = true;
            DataKeyNames = new string[] { _idColumn };
            PageIndexChanging += new GridViewPageEventHandler(GridView_PageIndexChanging);
            Sorting += new GridViewSortEventHandler(GridView_Sorting);
            RowDataBound += new GridViewRowEventHandler(GridView_RowDataBound);
            this.DataBound += new EventHandler(ZentityDataGridView_DataBound);
        }

        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets connection string  of database to be used.
        /// </summary>
        [Browsable(false)]
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        /// <summary>
        /// Gets or sets data source.
        /// </summary>
        [Browsable(false)]
        public override object DataSource
        {
            get
            {
                if (base.DataSource == null)
                    return ViewState[_gridDataSourceViewStateKey];
                return base.DataSource;
            }
            set
            {
                ViewState[_gridDataSourceViewStateKey] = value;
                base.DataSource = value;
            }
        }

        /// <summary>
        /// Gets sort direction of the column selected for sorting operation.
        /// </summary>
        [Browsable(false)]
        public new SortDirection SortDirection
        {
            get
            {
                return ViewState[_sortDirectionViewStateKey] != null ?
                    (SortDirection)ViewState[_sortDirectionViewStateKey] : SortDirection.Ascending;
            }
            internal set
            {
                ViewState[_sortDirectionViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets sort expression associated with the column selected for sorting operation.
        /// </summary>
        [Browsable(false)]
        public new string SortExpression
        {
            get
            {
                return ViewState[_sortExpressionViewStateKey] != null ?
                  (string)ViewState[_sortExpressionViewStateKey] : string.Empty;
            }
            internal set
            {
                ViewState[_sortExpressionViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets columns to be displayed.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionZentityGridViewDisplayColumns")]
        [MergableProperty(false), PersistenceMode(PersistenceMode.InnerProperty)]
        public ColumnCollection DisplayColumns
        {
            get
            {
                return _displayColumnCollection;
            }
        }

        /// <summary>
        /// Gets or sets view link url. Url format should be Url?Id={0}. 
        /// {0} will be replace by actual entity id.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionZentityGridViewViewUrl")]
        [Browsable(true), Localizable(true)]
        public string ViewUrl
        {
            get
            {
                return ViewState[_viewUrlViewStateKey] != null ?
                  (string)ViewState[_viewUrlViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_viewUrlViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets edit link url. Url format should be Url?Id={0}.
        /// {0} will be replace by actual entity id.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionZentityGridViewEditUrl")]
        [Browsable(true), Localizable(true)]
        public string EditUrl
        {
            get
            {
                return ViewState[_editUrlViewStateKey] != null ?
                  (string)ViewState[_editUrlViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_editUrlViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets edit link url. Url format should be Url?Id={0}.
        /// {0} will be replace by actual entity id.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionZentityGridViewEditUrl")]
        [Browsable(true), Localizable(true)]
        public string PermissionUrl
        {
            get
            {
                return ViewState[_permissionUrlViewStateKey] != null ?
                  (string)ViewState[_permissionUrlViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_permissionUrlViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets EditAssociation link url. Url format will change based on Entity Type.
        /// if EntityType is Tag/Category then Format is Url?Id={0}&amp;EntityType={1}. 
        /// {0} will be replaced by actual entity id and {1} will be replaced by actual EntityType. 
        /// If EntityType is Resource then Format is Url?Id={0}&amp;ResourceType={1}&amp;EntityType={2}.{
        /// 0} will be replaced by actual entity id and {1} will be replaced by EntityType and 
        /// {2} will be replaced by base type of EntityType.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionZentityGridViewEditAssociationUrl")]
        [Browsable(true), Localizable(true)]
        public string EditAssociationUrl
        {
            get
            {
                return ViewState[_editAssociationUrlViewStateKey] != null ?
                  (string)ViewState[_editAssociationUrlViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_editAssociationUrlViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets entity type.
        /// </summary>   
        /// [Browsable(true), Localizable(true)]
        public abstract string EntityType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets column name to have a View link.
        /// </summary>
        [Browsable(true), Localizable(true)]
        public abstract string ViewColumn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag to display or hide delete column.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionZentityGridViewEnableDelete")]
        [DefaultValue(typeof(bool), _defaultEnableDeleteValue), Browsable(true), Localizable(true)]
        public bool EnableDelete
        {
            get
            {
                return ViewState[_enableDeleteViewStateKey] != null ? (bool)ViewState[_enableDeleteViewStateKey] : true;
            }
            set
            {
                ViewState[_enableDeleteViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets DeleteClicked event.
        /// </summary>
        [Localizable(true)]
        public event EventHandler<ZentityGridEventArgs> DeleteClicked
        {
            add { Events.AddHandler(_deleteEventKey, value); }
            remove { Events.RemoveHandler(_deleteEventKey, value); }
        }

        /// <summary>
        /// Gets style defined for Buttons.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionEntitySearchButtonStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ButtonStyle
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
        /// Gets or sets page index.
        /// </summary>
        [Browsable(false)]
        public new int PageIndex
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

        /// <summary>
        /// Gets or sets page count.
        /// </summary>
        [Browsable(false)]
        public new int PageCount
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

        /// <summary>
        /// Event handler to handle paging event.
        /// </summary>
        [Localizable(true)]
        public event EventHandler<EventArgs> PageChanged
        {
            add { Events.AddHandler(_pageCalledKey, value); }
            remove { Events.RemoveHandler(_pageCalledKey, value); }
        }

        /// <summary>
        /// Gets or sets flag to indicate whether to
        /// hide or show the Command columns.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [Browsable(true), Localizable(true)]
        public bool ShowCommandColumns
        {
            get
            {
                return ViewState[_showCommandColumnsKey] != null ?
                  (bool)ViewState[_showCommandColumnsKey] : true;
            }
            set { ViewState[_showCommandColumnsKey] = value; }
        }


        /// <summary>
        /// Gets or sets flag to indicate whether to
        /// hide or show the Command columns.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [Browsable(true), Localizable(true)]
        public bool ShowExtraCommandColumns
        {
            get
            {
                return ViewState[_showExtraColumnsKey] != null ?
                  (bool)ViewState[_showExtraColumnsKey] : false;
            }
            set { ViewState[_showExtraColumnsKey] = value; }
        }



        #endregion

        #region Private
        private Table PagingTable
        {
            get
            {
                if (_pagingTable == null)
                {
                    _pagingTable = CreatePagingTable();
                }

                return _pagingTable;
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Sorts the grid view data based on SortDirection and SortExpression.
        /// </summary>
        public abstract void SortDataSource();

        #endregion

        #region Protected

        /// <summary>
        /// Calls DataBind() method if data source is not null 
        /// after completion of ViewState load operation.
        /// </summary>
        /// <param name="savedState">A System.Object that contains the saved view state values for the control.</param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            if (this.DataSource != null)
            {
                this.DataBind();
            }
        }

        /// <summary>
        /// Registers client script.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Register javascript for search control
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _javascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _searchControlScriptPath));

            base.OnLoad(e);
        }

        /// <summary>
        /// Creates GridView’s columns.
        /// </summary>
        /// <param name="dataSource">Represents the data source.</param>
        /// <param name="useDataSource">True to use the data source specified by the dataSource parameter; otherwise, false.</param>
        /// <returns>Collection of columns.</returns>
        protected override ICollection CreateColumns(PagedDataSource dataSource, bool useDataSource)
        {
            if (this.Columns.Count == 0)
            {
                this.CreateGridViewColumns();
            }
            else
            {
                this.InitializeTemplateFields();

            }
            return this.Columns;
        }

        /// <summary>
        /// Adds view column to the column collection.
        /// </summary>
        /// <param name="headerText">Header text of column.</param>
        /// <param name="dataTextField">Data text field of column.</param>
        /// <param name="sortExpression">Sort expression of column.</param>
        protected void AddViewColumn(string headerText, string dataTextField, string sortExpression)
        {
            if (!string.IsNullOrEmpty(ViewUrl))
            {
                HyperLinkField viewfield = new HyperLinkField();
                viewfield.DataNavigateUrlFormatString = ViewUrl;
                viewfield.DataNavigateUrlFields = new string[] { _idColumn };
                viewfield.HeaderText = headerText;
                viewfield.DataTextField = dataTextField;
                viewfield.SortExpression = sortExpression;
                this.Columns.Add(viewfield);
            }
            else
            {
                BoundField field = new BoundField();
                field.HeaderText = headerText;
                field.DataField = dataTextField;
                field.SortExpression = sortExpression;
                this.Columns.Add(field);
            }
        }

        /// <summary>
        /// Adds columns will be displayed in the column collection.
        /// </summary>
        protected abstract void AddDisplayColumns();

        /// <summary>
        /// Sets DataNavigateUrlFormatString property of editAssociation column.
        /// </summary>
        /// <param name="editAssociationColumn">HyperLinkField.</param>
        protected abstract void SetEditAssociationUrlFormat(HyperLinkField editAssociationColumn);

        /// <summary>
        /// Registers client side event to handle paging.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            //Register OnKeyPress Event for Pageindex textbox
            TextBox pageIndex = PagingTable.FindControl(_pageIndexLabelId) as TextBox;
            if (pageIndex != null)
            {
                pageIndex.Attributes.Add(_OnKeyPressEvent,
                    string.Format(CultureInfo.InvariantCulture, _handleKeyPressEvent, pageIndex.ClientID, 1, PageCount));
            }

            base.OnPreRender(e);
        }

        #endregion

        #region Private

        #region Columns Creation Methods

        private void CreateGridViewColumns()
        {
            //If Delete operation is enabled then add Delete checkbox column
            if (EnableDelete)
            {
                AddDeleteCheckBoxColumn();
            }

            //Add columns to be displayed
            AddDisplayColumns();

            if (ShowCommandColumns)
            {
                //Add Edit/EditAssociation/Delete command buttons
                AddCommandColumns();
            }

            if (ShowExtraCommandColumns)
            {
                //Add Edit/EditAssociation/Delete command buttons
                AddExtraCommandColumns();
            }

            this.PagerTemplate = new CompiledBindableTemplateBuilder(BuildPagerTemplate, null);
        }

        private void InitializeTemplateFields()
        {
            //If delete operation is enabled
            if (EnableDelete)
            {
                //Initialize Delete column Template
                TemplateField deleteColumn = this.Columns[0] as TemplateField;
                if (deleteColumn == null)
                {
                    deleteColumn = new TemplateField();
                    this.Columns.Insert(0, deleteColumn);
                }
                if (deleteColumn.ItemTemplate == null)
                {
                    deleteColumn.ItemTemplate = new CompiledBindableTemplateBuilder(BuildDeleteCheckBoxControls, null);
                }
                if (deleteColumn.HeaderTemplate == null)
                {
                    deleteColumn.HeaderTemplate = new CompiledBindableTemplateBuilder(BuildDeleteHeaderCheckBoxControls, null);
                    deleteColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
                }
                if (deleteColumn.FooterTemplate == null)
                {
                    deleteColumn.FooterTemplate = new CompiledBindableTemplateBuilder(BuildDeleteFooterButtonControls, null);
                }
            }

            this.PagerTemplate = new CompiledBindableTemplateBuilder(BuildPagerTemplate, null);
        }

        private void AddDeleteCheckBoxColumn()
        {
            TemplateField deleteColumn = new TemplateField();
            deleteColumn.ItemStyle.Width = deleteColumn.FooterStyle.Width = 50;
            deleteColumn.ItemTemplate = new CompiledBindableTemplateBuilder(BuildDeleteCheckBoxControls, null);
            deleteColumn.HeaderTemplate = new CompiledBindableTemplateBuilder(BuildDeleteHeaderCheckBoxControls, null);
            deleteColumn.FooterTemplate = new CompiledBindableTemplateBuilder(BuildDeleteFooterButtonControls, null);
            deleteColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            this.Columns.Add(deleteColumn);

        }

        private void AddCommandColumns()
        {
            HyperLinkField editfield = new HyperLinkField();
            editfield.Text = Resources.GlobalResource.ButtonEditText;
            editfield.DataNavigateUrlFormatString = EditUrl;
            editfield.DataNavigateUrlFields = new string[] { _idColumn };
            editfield.ItemStyle.Width = 80;
            this.Columns.Add(editfield);

            HyperLinkField editAssociationfield = new HyperLinkField();
            editAssociationfield.Text = Resources.GlobalResource.ButtonEditAssociationText;
            editAssociationfield.ItemStyle.Width = 120;
            SetEditAssociationUrlFormat(editAssociationfield);
            editAssociationfield.DataNavigateUrlFields = new string[] { _idColumn };
            this.Columns.Add(editAssociationfield);
        }


        private void AddExtraCommandColumns()
        {
            if (!string.IsNullOrEmpty(PermissionUrl))
            {
                HyperLinkField permissionfield = new HyperLinkField();
                permissionfield.Text = Resources.GlobalResource.ButtonPermisstionText;
                permissionfield.DataNavigateUrlFormatString = PermissionUrl;
                permissionfield.DataNavigateUrlFields = new string[] { _idColumn };
                permissionfield.ItemStyle.Width = 80;
                this.Columns.Add(permissionfield);
            }
            else
            {

                ButtonField field = new ButtonField();
                field.HeaderText = Resources.GlobalResource.ButtonPermisstionText;
                field.Text = Resources.GlobalResource.ButtonPermisstionText;
                field.ButtonType = ButtonType.Link;
                this.Columns.Add(field);

            }
        }

        private void BuildDeleteCheckBoxControls(Control control)
        {
            CheckBox deleteCheck = new CheckBox();
            deleteCheck.ID = _deleteButtonCheckBox;
            deleteCheck.EnableViewState = true;

            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(deleteCheck);
        }

        private void BuildDeleteHeaderCheckBoxControls(Control control)
        {
            CheckBox deleteCheck = new CheckBox();
            deleteCheck.ID = _headerDeleteButtonCheckBox;
            deleteCheck.EnableViewState = true;
            deleteCheck.AutoPostBack = false;
            deleteCheck.CheckedChanged += new EventHandler(deleteCheck_CheckedChanged);
            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(deleteCheck);
        }

        private void BuildDeleteFooterButtonControls(Control control)
        {
            Button deleteButton = new Button();
            deleteButton.ID = _deleteButton;
            deleteButton.Text = Resources.GlobalResource.ButtonDeleteText;
            deleteButton.ApplyStyle(ButtonStyle);

            deleteButton.Attributes.Add(_onClickEvent,
                string.Format(CultureInfo.InvariantCulture, _validateDeleteCheckBoxSelection,
                    this.ClientID,
                    Resources.GlobalResource.MessageDeleteRecords,
                    Resources.GlobalResource.MessageRecordNotSelected));

            deleteButton.Click += new EventHandler(deleteButton_Click);

            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(deleteButton);
        }

        /// <summary>
        /// Build Pager Row of FamlusGridView
        /// </summary>
        /// <param name="control">Parent control</param>
        private void BuildPagerTemplate(Control control)
        {
            if (!control.Controls.Contains(PagingTable))
            {
                IParserAccessor access = control as IParserAccessor;
                access.AddParsedSubObject(PagingTable);
            }
        }

        /// <summary>
        /// Create table control containing child paging controls.
        /// </summary>
        /// <returns></returns>
        private Table CreatePagingTable()
        {
            LinkButton firstButton = new LinkButton();
            firstButton.ID = _firstPageButtonId;
            firstButton.Text = Resources.GlobalResource.LinkButtonFirstText;
            firstButton.Click += new EventHandler(firstButton_Click);

            LinkButton previousButton = new LinkButton();
            previousButton.ID = _previousPageButtonId;
            previousButton.Text = Resources.GlobalResource.LinkButtonPreviousText;
            previousButton.Click += new EventHandler(previousButton_Click);

            LinkButton nextButton = new LinkButton();
            nextButton.ID = _nextPageButtonId;
            nextButton.Text = Resources.GlobalResource.LinkButtonNextText;
            nextButton.Click += new EventHandler(nextButton_Click);

            LinkButton lastButton = new LinkButton();
            lastButton.ID = _lastPageButtonId;
            lastButton.Text = Resources.GlobalResource.LinkButtonLastText;
            lastButton.Click += new EventHandler(lastButton_Click);

            Label pageLabel = new Label();
            pageLabel.ID = _pageLabelId;
            pageLabel.Text = Resources.GlobalResource.LabelPageText;

            TextBox pageIndexLabel = new TextBox();
            pageIndexLabel.Width = 30;
            pageIndexLabel.MaxLength = 5;
            pageIndexLabel.TextChanged += new EventHandler(pageIndexLabel_TextChanged);
            pageIndexLabel.ID = _pageIndexLabelId;
            pageIndexLabel.Text = _defaultPageIndexText;


            Label OfLabel = new Label();
            OfLabel.ID = _ofLabelId;
            OfLabel.Text = Resources.GlobalResource.LabelOfText;

            Label pageCountLabel = new Label();
            pageCountLabel.ID = _pageCountLabelId;
            pageCountLabel.Text = _defaultPageCountText;

            Table pagingTable = new Table();

            TableRow row = new TableRow();
            TableCell firstCell = new TableCell();
            firstCell.Controls.Add(firstButton);
            row.Cells.Add(firstCell);

            TableCell prevCell = new TableCell();
            prevCell.Controls.Add(previousButton);
            row.Cells.Add(prevCell);

            TableCell pageCell = new TableCell();
            pageCell.Controls.Add(pageLabel);
            row.Cells.Add(pageCell);

            TableCell pageIndexCell = new TableCell();
            pageIndexCell.Controls.Add(pageIndexLabel);
            row.Cells.Add(pageIndexCell);

            TableCell ofCell = new TableCell();
            ofCell.Controls.Add(OfLabel);
            row.Cells.Add(ofCell);

            TableCell pageCountCell = new TableCell();
            pageCountCell.Controls.Add(pageCountLabel);
            row.Cells.Add(pageCountCell);

            TableCell nextCell = new TableCell();
            nextCell.Controls.Add(nextButton);
            row.Cells.Add(nextCell);

            TableCell lastCell = new TableCell();
            lastCell.Controls.Add(lastButton);
            row.Cells.Add(lastCell);

            pagingTable.Rows.Add(row);

            return pagingTable;
        }

        #endregion

        private void firstButton_Click(object sender, EventArgs e)
        {
            PageIndex = 0;
            Refresh();
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            PageIndex = PageIndex > 0 ? PageIndex - 1 : 0;
            Refresh();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            PageIndex = PageIndex < (PageCount - 1) && PageIndex >= 0 ? PageIndex + 1 : 0;
            Refresh();
        }

        private void lastButton_Click(object sender, EventArgs e)
        {
            PageIndex = PageCount > 0 ? PageCount - 1 : 0;
            Refresh();
        }

        private void pageIndexLabel_TextChanged(object sender, EventArgs e)
        {
            TextBox pageIndexTextBox = PagingTable.FindControl(_pageIndexLabelId) as TextBox;

            //set selected page index and update DataSource.
            if (pageIndexTextBox != null)
            {
                int selectedPageIndex = 0;
                if (string.IsNullOrEmpty(pageIndexTextBox.Text.Trim()))
                {
                    if (PageIndex >= 0)
                    {
                        //Set previous PageIndex
                        selectedPageIndex = PageIndex;
                        pageIndexTextBox.Text = (PageIndex + 1).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        //Set default page index
                        pageIndexTextBox.Text = _defaultPageIndexText;
                    }
                }
                else
                {
                    try
                    {
                        //Get selected page index
                        int value = Convert.ToInt32(pageIndexTextBox.Text, CultureInfo.InvariantCulture);
                        if (value < 1)
                        {
                            pageIndexTextBox.Text = _defaultPageIndexText;
                        }
                        else if (value > PageCount)
                        {
                            selectedPageIndex = PageCount - 1;
                            pageIndexTextBox.Text = PageCount.ToString(CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            //if (PageIndex > 0)
                            //{
                            selectedPageIndex = value - 1;
                            //}
                        }
                    }
                    catch (FormatException)
                    {
                        pageIndexTextBox.Text = _defaultPageIndexText;
                    }
                }
                if (selectedPageIndex <= PageCount)
                {
                    PageIndex = selectedPageIndex;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Updated status of paging controls
        /// </summary>
        private void UpdatePagingStatus()
        {
            LinkButton btnFirstPage = (LinkButton)this.BottomPagerRow.FindControl(_firstPageButtonId);
            LinkButton btnNextPage = (LinkButton)this.BottomPagerRow.FindControl(_nextPageButtonId);
            LinkButton btnPrevPage = (LinkButton)this.BottomPagerRow.FindControl(_previousPageButtonId);
            LinkButton btnLastPage = (LinkButton)this.BottomPagerRow.FindControl(_lastPageButtonId);
            TextBox labelPageIndex = (TextBox)this.BottomPagerRow.FindControl(_pageIndexLabelId);
            Label labelPageCount = (Label)this.BottomPagerRow.FindControl(_pageCountLabelId);

            this.BottomPagerRow.Visible = true;
            //If only single page is there
            if (PageCount <= 1)
            {
                this.BottomPagerRow.Visible = false;
            }
            else
            {
                //If current page is not first page or last page and Page count is greater than 1
                if (PageIndex > 0 && (PageCount - 1) > PageIndex)
                {
                    btnFirstPage.Enabled = btnNextPage.Enabled =
                        btnPrevPage.Enabled = btnLastPage.Enabled = true;
                }
                //If page is a first page and page count is greater than 1
                else if (PageIndex == 0 && (PageCount - 1) > PageIndex)
                {
                    btnFirstPage.Enabled = btnPrevPage.Enabled = false;
                    btnNextPage.Enabled = btnLastPage.Enabled = true;
                }
                //if page is last page and page count is greater than 1
                else if (PageIndex > 0 && (PageCount - 1) == PageIndex)
                {
                    btnLastPage.Enabled = btnNextPage.Enabled = false;
                    btnFirstPage.Enabled = btnPrevPage.Enabled = true;
                }

                labelPageIndex.Text = (PageIndex + 1).ToString(CultureInfo.InvariantCulture);
                labelPageCount.Text = PageCount.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Perform sorting operation on grid control data.
        /// </summary>
        /// <param name="sender">Sender control.</param>
        /// <param name="e">Grid event arguments with SortExpression and SortDirection.</param>
        private void GridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Switch the Sort direction only if SortExpression is same
            if (this.SortExpression.Equals(e.SortExpression))
            {
                if (this.SortDirection == SortDirection.Ascending)
                    this.SortDirection = SortDirection.Descending;
                else
                    this.SortDirection = SortDirection.Ascending;
            }
            else
            {
                this.SortDirection = SortDirection.Ascending;
            }

            this.SortExpression = e.SortExpression;

            this.SortDataSource();
            this.DataBind();
        }

        /// <summary>
        /// Set status of all check boxes in the Delete column as per 
        /// status of Header Checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void deleteCheck_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox headerChk = sender as CheckBox;
            bool value = headerChk.Checked;
            foreach (GridViewRow gvr in this.Rows)
            {
                CheckBox rowChk = gvr.Cells[0].FindControl(_deleteButtonCheckBox) as CheckBox;

                if (rowChk != null)
                {
                    rowChk.Checked = value;
                }
            }
        }

        /// <summary>
        /// Raises DeletedClicked Event on Delete button clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            Collection<Guid> entrityIds = GetSelectedEntityIds();
            RaiseDeleteEvent(entrityIds);

        }

        /// <summary>
        /// This event handler set the row index as a command arguments for delete button on each row.
        /// </summary>
        /// <param name="sender">Sender control.</param>
        /// <param name="e">GridView row event arguments.</param>
        private void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                CheckBox headerCheckBox = e.Row.FindControl(_headerDeleteButtonCheckBox) as CheckBox;
                if (headerCheckBox != null)
                {
                    headerCheckBox.Attributes.Add(_onClickEvent,
                        string.Format(CultureInfo.InvariantCulture, _selectDeselectAll, this.ClientID, headerCheckBox.ClientID));
                }
            }
            else
            {
                CheckBox headerCheckBox = this.HeaderRow.FindControl(_headerDeleteButtonCheckBox) as CheckBox;
                CheckBox checkBox = e.Row.FindControl(_deleteButtonCheckBox) as CheckBox;
                if (checkBox != null && headerCheckBox != null)
                {
                    checkBox.Attributes.Add(_onClickEvent,
                        string.Format(CultureInfo.InvariantCulture, _selectDeselectHeaderCheckBox, this.ClientID, headerCheckBox.ClientID));
                }
            }
            if (this.Columns != null && this.Columns.Count > 0)
            {
                EncodeDataInHtml(e.Row);
            }

            if (this.DesignMode)
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    e.Row.Cells[1].Text = GlobalResource.DesignTimeDummyDataString;
                }
            }

        }

        /// <summary>
        /// Updates current page index of grid view.
        /// </summary>
        /// <param name="sender"> Sender control.</param>
        /// <param name="e"> Event argument providing new page index.</param>
        private void GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.PageIndex = e.NewPageIndex;
        }

        private void ZentityDataGridView_DataBound(object sender, EventArgs e)
        {
            if (AllowPaging && PageCount > 0 && this.DataSource != null
              && ((IList)this.DataSource).Count > 0)
            {
                this.UpdatePagingStatus();
            }
        }

        /// <summary>
        /// Raise Delete event
        /// </summary>
        /// <param name="entityIds">List of ids of selected entities</param>
        private void RaiseDeleteEvent(Collection<Guid> entityIds)
        {
            EventHandler<ZentityGridEventArgs> handler = Events[_deleteEventKey] as EventHandler<ZentityGridEventArgs>;

            if (handler != null)
                handler(this, new ZentityGridEventArgs(entityIds));
        }

        /// <summary>
        /// Return list of Ids of entities for which Checkbox in the Delete column is checked.
        /// </summary>
        /// <returns>list of selected entity Ids</returns>
        private Collection<Guid> GetSelectedEntityIds()
        {
            Collection<Guid> selectedEntityList = new Collection<Guid>();
            if (this.DataSource != null)
            {
                for (int i = 0; i < this.Rows.Count; i++)
                {
                    CheckBox chk = this.Rows[i].Cells[0].
                        FindControl(_deleteButtonCheckBox) as CheckBox;
                    if (chk != null)
                    {
                        if (chk.Checked && this.DataKeys[i].Value != null)
                        {
                            Guid resID = (Guid)this.DataKeys[i].Value;
                            selectedEntityList.Add(resID);
                        }
                    }
                }
            }

            return selectedEntityList;
        }

        /// <summary>
        /// Handle occurrence of special character like &lt; and &gt; etc in column's data.
        /// </summary>
        /// <param name="row">Represents grid view row.</param>
        private static void EncodeDataInHtml(GridViewRow row)
        {
            if (row.Cells.Count > 0)
            {
                foreach (TableCell cell in row.Cells)
                {
                    foreach (Control control in cell.Controls)
                    {
                        HyperLink linkButtton = control as HyperLink;
                        if (linkButtton != null)
                        {
                            linkButtton.Text = HttpUtility.HtmlEncode(linkButtton.Text);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raise PageChanged event
        /// </summary>
        private void Refresh()
        {
            EventHandler<EventArgs> handler = Events[_pageCalledKey] as EventHandler<EventArgs>;

            if (handler != null)
                handler(this, new EventArgs());

        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Represents Collection of ZentityGridViewColumn.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class ColumnCollection : Collection<ZentityGridViewColumn>
    {

    }


}
