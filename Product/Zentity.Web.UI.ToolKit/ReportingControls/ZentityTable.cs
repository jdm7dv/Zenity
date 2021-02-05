// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.ToolKit
{
    #region Using Namespace
    using System;
    using System.Data;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using Zentity.Core;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI.Design;
    using System.IO;
    using System.Globalization;
    using System.Text;
    #endregion

    /// <summary>
    /// Implements the basic functionality required by controls that require a tabular structure.
    /// </summary>
    [Designer(typeof(ZentityTableDesigner))]
    public abstract class ZentityTable : ZentityBase
    {
        #region Constants

        #region Private

        const string _pageSizeViewStateKey = "PageSize";
        const int _defaultPageSize = 20;
        const string _resourceTableWidth = "100%";
        const string _zentityTableCSS = "tableContainer1";
        const int _headerRowIndex = 0;
        const int _dataRowStartIndex = 1;
        // Consider http://msdn.microsoft.com/en-us/library/az4se3k1.aspx for more DateTime formats.
        const string _dateTimeFormat = "G";

        #endregion Private

        #endregion Constants

        #region Member variables

        #region Protected

        /// <summary>
        /// Container table.
        /// </summary>
        protected Table _resourceTable = new Table();

        #endregion Protected

        #region Private

        private Label _titleLabel;
        private Panel _titlePanel = new Panel();
        private TableItemStyle _headerStyle;
        private TableItemStyle _rowStyle;
        private TableItemStyle _alternateRowStyle;

        #endregion Private

        #endregion Member variables

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the maximum number of records to be displayed.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionTablePageSize")]
        [Localizable(true)]
        public int PageSize
        {
            get
            {
                return ViewState[_pageSizeViewStateKey] != null ? (int)ViewState[_pageSizeViewStateKey] : _defaultPageSize;
            }
            set
            {
                ViewState[_pageSizeViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of even rows.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle RowStyle
        {
            get
            {
                if (this._rowStyle == null)
                {
                    this._rowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._rowStyle).TrackViewState();
                    }
                }
                return this._rowStyle;
            }
            set
            {
                _rowStyle = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the header row.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableHeaderStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle HeaderStyle
        {
            get
            {
                if (this._headerStyle == null)
                {
                    this._headerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._headerStyle).TrackViewState();
                    }
                }
                return this._headerStyle;
            }
            set
            {
                _headerStyle = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of alternate (odd) rows.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableAlternateRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle AlternateRowStyle
        {
            get
            {
                if (this._alternateRowStyle == null)
                {
                    this._alternateRowStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._alternateRowStyle).TrackViewState();
                    }
                }
                return this._alternateRowStyle;
            }
            set
            {
                _alternateRowStyle = value;
            }
        }

        #endregion Public

        #endregion Properties

        #region Methods

        #region Protected

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode)
            {
                CreateDesignTimeDataSource();
            }
            else
            {
                GetDataSource();
            }

            CreateTitle();
            CreateContainer();
            CreateHeader();
            CreateRows();           
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
            script.Append("if(document.getElementById('" + scrollableContainer.ClientID + "').clientHeight != null){");
            script.Append("if(document.getElementById('" + _titleLabel.ClientID + "').clientHeight != null){");
            script.Append("var i = (document.getElementById('" + scrollableContainer.ClientID + "').clientHeight - document.getElementById('" + _titleLabel.ClientID + "').clientHeight);");
            script.Append("if(i>0){");
            script.Append("document.getElementById('" + scrollableContainer.ClientID + "').style.height = ((document.getElementById('" + scrollableContainer.ClientID + "').clientHeight - document.getElementById('" + _titleLabel.ClientID + "').clientHeight) + 'px');");
            script.Append("}}}}");
            script.Append("</script>");
            this.Page.ClientScript.RegisterStartupScript(Page.GetType(), scrollableContainer.ClientID, script.ToString());
       }

        /// <summary>
        /// Writes the control content to the specified <see cref="HtmlTextWriter"/> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="HtmlTextWriter"/> that represents the output stream to 
        /// render HTML content on the client.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode)
            {
                // Required to create the control in design mode.
                OnLoad(EventArgs.Empty);
            }

            ApplyStyleToTitle();
            ApplyStyleToHeader();
            ApplyStyleToRows();

            base.Render(writer);
        }

        /// <summary>
        /// Fetches the data to be displayed in the control.  
        /// </summary>
        protected abstract void GetDataSource();

        /// <summary>
        /// Creates header for the table.
        /// </summary>
        protected abstract void CreateHeader();

        /// <summary>
        /// Creates rows for the table.
        /// </summary>
        protected abstract void CreateRows();

        /// <summary>
        /// Creates the data to be displayed in the control at design time.
        /// </summary>
        protected abstract void CreateDesignTimeDataSource();

        /// <summary>
        /// Creates a header cell with the specified text.
        /// </summary>
        /// <param name="text">Text to be displayed in the cell.</param>
        /// <returns>Header cell.</returns>
        protected static TableHeaderCell CreateHeaderCell(string text)
        {
            TableHeaderCell cell = new TableHeaderCell();
            cell.Text = text;
            return cell;
        }

        /// <summary>
        /// Creates a label cell with the specified parameters.
        /// </summary>
        /// <param name="id">Id of the label cell.</param>
        /// <param name="text">Text to be displayed in the cell.</param>
        /// <param name="newStyle">New style to be applied.</param>
        /// <returns>Table cell.</returns>
        protected static TableCell CreateLabelCell(string id, string text, Style newStyle)
        {
            TableCell cell = new TableCell();
            Label labelControl = new Label();
            labelControl.ID = id;
            labelControl.Text = text;
            labelControl.ApplyStyle(newStyle);
            cell.Controls.Add(labelControl);
            return cell;
        }

        /// <summary>
        ///  Creates a cell with hyperlink.
        /// </summary>
        /// <param name="text">Text to be displayed in the cell.</param>
        /// <param name="navigationUrl">Url to navigate on click of the hyperlink.</param>
        /// <returns>Table cell.</returns>
        protected static TableCell CreateHyperlinkCell(string text, string navigationUrl)
        {
            TableCell cell = new TableCell();
            cell.Controls.Add(CreateHyperlink(text, navigationUrl));
            return cell;
        }

        /// <summary>
        ///   Creates a cell with text.
        /// </summary>
        /// <param name="text">Text to be displayed in the cell.</param>
        /// <returns>Table cell.</returns>
        protected static TableCell CreateTextCell(string text)
        {
            TableCell cell = new TableCell();
            cell.Text = HttpUtility.HtmlEncode(text);
            return cell;
        }

        /// <summary>
        ///   Creates a cell with a nullable DateTime.
        /// </summary>
        /// <param name="date">Date.</param>
        /// <returns>Table cell.</returns>
        protected static TableCell CreateDateCell(DateTime? date)
        {
            return CreateTextCell((
                date.HasValue ?
                date.Value.ToString(_dateTimeFormat, CultureInfo.CurrentCulture)
                : string.Empty));
        }

        /// <summary>
        ///  Creates a hyperlink.
        /// </summary>
        /// <param name="text">Text to be displayed in the hyperlink.</param>
        /// <param name="navigationUrl">Url to navigate on click of the hyperlink.</param>
        /// <returns>Hyperlink.</returns>
        protected static HyperLink CreateHyperlink(string text, string navigationUrl)
        {
            HyperLink hyperLink = new HyperLink();
            hyperLink.NavigateUrl = navigationUrl;
            hyperLink.Text = HttpUtility.HtmlEncode(text);
            return hyperLink;
        }

        #endregion Protected

        #region Private

        /// <summary>
        /// Creates the container for the child controls.
        /// </summary>
        private void CreateContainer()
        {
            _resourceTable.CellSpacing = 0;
            _resourceTable.Width = new Unit(_resourceTableWidth, CultureInfo.InvariantCulture);
            HtmlContainerControl scrollableContainer = new HtmlGenericControl();
            if (this.Height != null)
            {
                scrollableContainer.Style.Add("height", (this.Height).ToString());
            }
            if (this.Width != null)
            {
                scrollableContainer.Style.Add("width", (this.Width).ToString());
            }
            scrollableContainer.Attributes.Add("class", _zentityTableCSS);
            scrollableContainer.Controls.Add(_resourceTable);

            this.Controls.Add(scrollableContainer);

            RegisterScript(scrollableContainer);
        }

        /// <summary>
        /// Creates a title row for the control.
        /// </summary>
        private void CreateTitle()
        {
            _titleLabel = new Label();
            _titleLabel.Text = Title;
            _titlePanel.Controls.Add(_titleLabel);
            this.Controls.Add(_titlePanel);
        }

        /// <summary>
        ///  Applies the specified style to the specified row.
        /// </summary>
        /// <param name="row"> Table row to be applied style </param>
        /// <param name="style">Style to be applied to table row</param>
        private static void ApplyRowStyle(TableRow row, TableItemStyle style)
        {
            for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
            {
                TableCell cell = row.Cells[cellIndex];
                cell.MergeStyle(style);
            }
        }

        /// <summary>
        /// Applies style to title.
        /// </summary>
        private void ApplyStyleToTitle()
        {
            _titlePanel.ApplyStyle(this.TitleStyle);
        }

        /// <summary>
        /// Applies style to header.
        /// </summary>
        private void ApplyStyleToHeader()
        {
            TableRow tableHeader = (TableRow)_resourceTable.Controls[_headerRowIndex];
            ApplyRowStyle(tableHeader, this.HeaderStyle);
        }

        /// <summary>
        /// Applies style to rows.
        /// </summary>        
        private void ApplyStyleToRows()
        {
            for (int rowIndex = _dataRowStartIndex; rowIndex < _resourceTable.Controls.Count; rowIndex++)
            {
                TableRow tableRow = (TableRow)_resourceTable.Controls[rowIndex];

                if ((rowIndex % 2) != 0)
                {
                    ApplyRowStyle(tableRow, this.RowStyle);
                }
                else
                {
                    ApplyRowStyle(tableRow, this.AlternateRowStyle);
                }
            }
        }

        #endregion Private

        #endregion Methods
    }
}
