// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control displays abstract details of the resource as a data source.
    /// </summary>
    [DefaultProperty("Text")]

    internal class ResourceView : WebControl, INamingContainer
    {
        #region Member Variables

        private Label _titleHeaderLabel;
        private HyperLink _titleLink;
        private Label _dateAddedHeaderLabel;
        private Label _dateAddedLabel;
        private Label _authorsHeaderLabel;
        private Panel _authorsPanel;
        private Label _filesHeaderLabel;
        private Panel _filesPanel;
        private Label _descHeaderLabel;
        private Label _descLabel;
        private Dictionary<Guid, IEnumerable<string>> _userPermissions;
        private int _maxTitleCharWidth = 30;
        private int _maxDescriptionCharWidth = 200;
        private int _maxAuthorsCount = 2;
        private int _maxFilesCount = 2;

        #endregion

        #region Constants

        private const string _titleHeaderLabelId = "titleHeaderLabelId";
        private const string _titleLinkId = "titleLinkId";
        private const string _dateAddedHeaderLabelId = "dateAddedHeaderLabelId";
        private const string _dateAddedLabelId = "dateAddedLabelId";
        private const string _authorsHeaderLabelId = "authorsHeaderLabelId";
        private const string _authorsPanelId = "authorsPanelId";
        private const string _filesHeaderLabelId = "filesHeaderLabelId";
        private const string _filesPanelId = "filesPanelId";
        private const string _descHeaderLabelId = "descHeaderLabelId";
        private const string _descLabelId = "descLabelId";

        private const string _dataSourceKey = "dataSourceKey";
        private const string _userPermissionsKey = "userPermissionsKey";
        private const string _titleLinkKey = "titleLinkKey";
        private const string _authorsLinkKey = "authorsLinkKey";
        private const string _filesLinkKey = "filesLinkKey";
        private const string _showDateKey = "showDateKey";

        private const string _colan = ":";
        private const string _comma = ", ";
        private const string _etc = "...";

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets data source.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceViewDataSource")]
        public Resource DataSource
        {
            get { return ViewState[_dataSourceKey] as Resource; }
            set { ViewState[_dataSourceKey] = value; }
        }

        /// <summary>
        /// Gets or sets resource title link.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceViewTitleLink")]
        public string TitleLink
        {
            get { return ViewState[_titleLinkKey] as string; }
            set { ViewState[_titleLinkKey] = value; }
        }

        /// <summary>
        /// Gets or sets link for authors.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceViewAuthorsLink")]
        public string AuthorsLink
        {
            get { return ViewState[_authorsLinkKey] as string; }
            set { ViewState[_authorsLinkKey] = value; }
        }

        /// <summary>
        /// Gets or sets link for files.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceViewFilesLink")]
        public string FilesLink
        {
            get { return ViewState[_filesLinkKey] as string; }
            set { ViewState[_filesLinkKey] = value; }
        }

        /// <summary>
        /// Gets or sets type of date to be displayed.
        /// </summary>
        public DateType ShowDate
        {
            get { return ViewState[_showDateKey] != null ? (DateType)ViewState[_showDateKey] : DateType.DateAdded; }
            set { ViewState[_showDateKey] = value; }
        }

        /// <summary>
        /// Gets or sets user permissions on the resources.
        /// </summary>
        public IDictionary<Guid, IEnumerable<string>> UserPermissions
        {
            get { return _userPermissions; }
            set { _userPermissions = value as Dictionary<Guid, IEnumerable<string>>; }
        }


        #endregion

        #region Private

        private Label TitleHeader
        {
            get
            {
                if (_titleHeaderLabel == null)
                {
                    _titleHeaderLabel = new Label();
                    _titleHeaderLabel.ID = _titleHeaderLabelId;
                    _titleHeaderLabel.Text = GlobalResource.TitleText + _colan;
                    _titleHeaderLabel.Font.Bold = true;
                }

                return _titleHeaderLabel;
            }
        }

        private HyperLink ResourceTitle
        {
            get
            {
                if (_titleLink == null)
                {
                    _titleLink = new HyperLink();
                    _titleLink.ID = _titleLinkId;
                    if (DesignMode)
                    {
                        _titleLink.Text = "Title1";
                    }
                }

                return _titleLink;
            }
        }

        private Label AuthorsHeader
        {
            get
            {
                if (_authorsHeaderLabel == null)
                {
                    _authorsHeaderLabel = new Label();
                    _authorsHeaderLabel.ID = _authorsHeaderLabelId;
                    _authorsHeaderLabel.Text = GlobalResource.AuthorsText + _colan;
                    _authorsHeaderLabel.Font.Bold = true;
                }

                return _authorsHeaderLabel;
            }
        }

        private Panel Authors
        {
            get
            {
                if (_authorsPanel == null)
                {
                    _authorsPanel = new Panel();
                    _authorsPanel.ID = _authorsPanelId;
                    if (DesignMode)
                    {
                        HyperLink dummyLink = new HyperLink();
                        dummyLink.Text = "Author1";
                        _authorsPanel.Controls.Add(dummyLink);
                    }
                }

                return _authorsPanel;
            }
        }

        private Label DescriptionHeader
        {
            get
            {
                if (_descHeaderLabel == null)
                {
                    _descHeaderLabel = new Label();
                    _descHeaderLabel.ID = _descHeaderLabelId;
                    _descHeaderLabel.Text = GlobalResource.DescriptionText + _colan;
                    _descHeaderLabel.Font.Bold = true;
                }

                return _descHeaderLabel;
            }

        }

        private Label Description
        {
            get
            {
                if (_descLabel == null)
                {
                    _descLabel = new Label();
                    _descLabel.ID = _descLabelId;
                    _descLabel.Text = "Description";
                }

                return _descLabel;
            }
        }

        private Label DateAddedHeader
        {
            get
            {
                if (_dateAddedHeaderLabel == null)
                {
                    _dateAddedHeaderLabel = new Label();
                    _dateAddedHeaderLabel.ID = _dateAddedHeaderLabelId;
                    _dateAddedHeaderLabel.Text = GlobalResource.DateAddedText + _colan;
                    _dateAddedHeaderLabel.Font.Bold = true;
                }

                return _dateAddedHeaderLabel;
            }
        }

        private Label DateAdded
        {
            get
            {
                if (_dateAddedLabel == null)
                {
                    _dateAddedLabel = new Label();
                    _dateAddedLabel.ID = _dateAddedLabelId;
                    _dateAddedLabel.Text = DateTime.Now.ToString();
                }

                return _dateAddedLabel;
            }

        }

        private Label FilesHeader
        {
            get
            {
                if (_filesHeaderLabel == null)
                {
                    _filesHeaderLabel = new Label();
                    _filesHeaderLabel.ID = _filesHeaderLabelId;
                    _filesHeaderLabel.Text = GlobalResource.FilesText + _colan;
                    _filesHeaderLabel.Font.Bold = true;
                }

                return _filesHeaderLabel;
            }
        }

        private Panel Files
        {
            get
            {
                if (_filesPanel == null)
                {
                    _filesPanel = new Panel();
                    _filesPanel.ID = _filesPanelId;
                    if (DesignMode)
                    {
                        HyperLink dummyLink = new HyperLink();
                        dummyLink.Text = "File1";
                        _filesPanel.Controls.Add(dummyLink);
                    }
                }

                return _filesPanel;
            }

        }


        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Updates the status of controls base on DataSource.
        /// </summary>
        public override void DataBind()
        {
            base.DataBind();

            if (DataSource != null)
            {
                this.TitleHeader.Text = GlobalResource.TitleText + _colan;
                //Set title with link
                this.ResourceTitle.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(
                    CoreHelper.GetTitleByResourceType(DataSource)), _maxTitleCharWidth)) + " (" + DataSource.GetType().Name + ")";

                if (!string.IsNullOrEmpty(TitleLink) &&
                    (UserPermissions == null ||
                    (UserPermissions.Keys.Contains(DataSource.Id) && UserPermissions[DataSource.Id].Contains(UserResourcePermissions.Read))))
                {
                    this.ResourceTitle.NavigateUrl = string.Format(CultureInfo.CurrentCulture, TitleLink, DataSource.Id);
                }
                else
                {
                    this.ResourceTitle.NavigateUrl = string.Empty;
                }

                //Set date based on ShowDate value.
                if (ShowDate == DateType.DateAdded)
                {
                    this.DateAddedHeader.Text = GlobalResource.DateAddedText + _colan;
                    this.DateAdded.Text = DataSource.DateAdded != null ? DataSource.DateAdded.Value.ToString() : string.Empty;
                }
                else
                {
                    this.DateAddedHeader.Text = GlobalResource.DateModifiedText + _colan;
                    this.DateAdded.Text = DataSource.DateModified != null ? DataSource.DateModified.Value.ToString() : string.Empty;
                }
                //Set Description.
                this.Description.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(DataSource.Description, _maxDescriptionCharWidth));
                //Set files.
                BindRelatedFiles();
                
                //set authors.
                ScholarlyWork resource = DataSource as ScholarlyWork;
                if (resource != null && resource.Authors != null && resource.Authors.Count > 0)
                {
                    int index = 0;
                    foreach (Contact author in resource.Authors)
                    {
                        if (UserPermissions == null ||
                            (UserPermissions.Keys.Contains(author.Id) && UserPermissions[author.Id].Contains(UserResourcePermissions.Read)))
                        {
                            HyperLink link = new HyperLink();
                            link.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(CoreHelper.GetTitleByResourceType(author)), _maxTitleCharWidth));

                            if (!string.IsNullOrEmpty(AuthorsLink))
                                link.NavigateUrl = string.Format(CultureInfo.CurrentCulture, AuthorsLink, author.Id);

                            this.Authors.Controls.Add(link);

                            if (index < (resource.Authors.Count - 1))
                            {
                                Label commalbl = new Label();
                                commalbl.Text = _comma;
                                this.Authors.Controls.Add(commalbl);
                            }

                        }


                        if (index >= _maxAuthorsCount - 1)
                        {
                            if (index < (resource.Authors.Count - 1))
                            {
                                Label etcLbl = new Label();
                                etcLbl.Text = _etc;
                                this.Authors.Controls.Add(etcLbl);
                            }
                            break;
                        }

                        index++;
                    }
                }

            }
        }

        private void BindRelatedFiles()
        {
            if (DataSource.Files != null && DataSource.Files.Count > 0)
            {
                int index = 0;
                foreach (File file in DataSource.Files)
                {
                    if (UserPermissions == null ||
                            (UserPermissions.Keys.Contains(file.Id) && UserPermissions[file.Id].Contains(UserResourcePermissions.Read)))
                    {
                        HyperLink link = new HyperLink();
                        link.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(CoreHelper.GetTitleByResourceType(file)), _maxTitleCharWidth));

                        if (!string.IsNullOrEmpty(FilesLink))
                            link.NavigateUrl = string.Format(CultureInfo.CurrentCulture, FilesLink, file.Id);

                        this.Files.Controls.Add(link);

                        if (index < (DataSource.Files.Count - 1))
                        {
                            Label commalbl = new Label();
                            commalbl.Text = _comma;
                            this.Files.Controls.Add(commalbl);
                        }
                    }

                    if (index >= _maxFilesCount - 1)
                    {
                        if (index < (DataSource.Files.Count - 1))
                        {
                            Label etcLbl = new Label();
                            etcLbl.Text = _etc;
                            this.Files.Controls.Add(etcLbl);
                        }
                        break;
                    }

                    index++;
                }
            }
        }

        #endregion

        #region Protected

        /// <summary>
        /// Creates child controls layout.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            //Add child Controls.
            Table table = CreateControlLayout();
            this.Controls.Add(table);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Add child Controls.
            Table table = CreateControlLayout();
            this.Controls.Add(table);
        }

        #endregion

        #region Private

        private Table CreateControlLayout()
        {
            Table table = new Table();
            table.CellSpacing = 5;

            TableRow titlRow = new TableRow();
            TableCell titleHeaderCell = new TableCell();
            titleHeaderCell.Width = Unit.Percentage(10);
            titleHeaderCell.HorizontalAlign = HorizontalAlign.Left;
            titleHeaderCell.VerticalAlign = VerticalAlign.Top;
            titleHeaderCell.Controls.Add(TitleHeader);
            titlRow.Cells.Add(titleHeaderCell);
            TableCell titleDataCell = new TableCell();
            titleDataCell.Width = Unit.Percentage(40);
            titleDataCell.HorizontalAlign = HorizontalAlign.Left;
            titleDataCell.VerticalAlign = VerticalAlign.Top;
            titleDataCell.Controls.Add(ResourceTitle);
            titlRow.Cells.Add(titleDataCell);
            TableCell dateAddedHeaderCell = new TableCell();
            dateAddedHeaderCell.Width = Unit.Percentage(20);
            dateAddedHeaderCell.HorizontalAlign = HorizontalAlign.Left;
            dateAddedHeaderCell.VerticalAlign = VerticalAlign.Top;
            dateAddedHeaderCell.Controls.Add(DateAddedHeader);
            titlRow.Cells.Add(dateAddedHeaderCell);
            TableCell dateAddedDataCell = new TableCell();
            dateAddedDataCell.HorizontalAlign = HorizontalAlign.Left;
            dateAddedDataCell.VerticalAlign = VerticalAlign.Top;
            dateAddedDataCell.Controls.Add(DateAdded);
            titlRow.Cells.Add(dateAddedDataCell);
            table.Rows.Add(titlRow);

            TableRow authorsRow = new TableRow();
            TableCell authorsHeaderCell = new TableCell();
            authorsHeaderCell.Width = Unit.Percentage(10);
            authorsHeaderCell.HorizontalAlign = HorizontalAlign.Left;
            authorsHeaderCell.VerticalAlign = VerticalAlign.Top;
            authorsHeaderCell.Controls.Add(AuthorsHeader);
            authorsRow.Cells.Add(authorsHeaderCell);
            TableCell authorsDataCell = new TableCell();
            authorsDataCell.Width = Unit.Percentage(40);
            authorsDataCell.HorizontalAlign = HorizontalAlign.Left;
            authorsDataCell.VerticalAlign = VerticalAlign.Top;
            authorsDataCell.Controls.Add(Authors);
            authorsRow.Cells.Add(authorsDataCell);
            TableCell filesHeaderCell = new TableCell();
            filesHeaderCell.Width = Unit.Percentage(20);
            filesHeaderCell.HorizontalAlign = HorizontalAlign.Left;
            filesHeaderCell.VerticalAlign = VerticalAlign.Top;
            filesHeaderCell.Controls.Add(FilesHeader);
            authorsRow.Cells.Add(filesHeaderCell);
            TableCell filesDataCell = new TableCell();
            filesDataCell.HorizontalAlign = HorizontalAlign.Left;
            filesDataCell.VerticalAlign = VerticalAlign.Top;
            filesDataCell.Controls.Add(Files);
            authorsRow.Cells.Add(filesDataCell);
            table.Rows.Add(authorsRow);

            TableRow descRow = new TableRow();
            TableCell descHeaderCell = new TableCell();
            descHeaderCell.Width = Unit.Percentage(10);
            descHeaderCell.HorizontalAlign = HorizontalAlign.Left;
            descHeaderCell.VerticalAlign = VerticalAlign.Top;
            descHeaderCell.Controls.Add(DescriptionHeader);
            descRow.Cells.Add(descHeaderCell);
            TableCell descDataCell = new TableCell();
            descDataCell.ColumnSpan = 3;
            descDataCell.HorizontalAlign = HorizontalAlign.Left;
            descDataCell.VerticalAlign = VerticalAlign.Top;
            descDataCell.Controls.Add(Description);
            descRow.Cells.Add(descDataCell);
            table.Rows.Add(descRow);

            return table;
        }

        #endregion

        #endregion
    }
}
