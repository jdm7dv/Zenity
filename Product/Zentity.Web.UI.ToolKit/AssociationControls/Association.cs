// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.Web.UI.ToolKit.Resources;


namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    ///     This class inherits <see cref="ZentityBase" /> control to create custom base class for association controls.
    /// </summary>
    [Designer(typeof(AssociationDesigner))]
    public abstract class Association : ZentityBase
    {
        #region Constants

        #region Protected

        /// <summary>
        /// Determines the value field for CheckBoxLists.
        /// </summary>
        protected const string ObjectDataValueField = "Id";

        #endregion

        #region Private

        const string _resourceDefaultProperty = "Title";
        const string _subjectItemDestinationUrlViewStateKey = "SubjectItemDestinationUrl";
        const string _associationListHeightViewStateKey = "CheckBoxListHeight";
        const string _associationListWidthViewStateKey = "CheckBoxListWidth";
        const string _sourceListViewStateKey = "SourceList";
        const string _destinationListViewStateKey = "DestinationList";
        const string _subjectItemIdViewStateKey = "SubjectItemId";
        const string _newLineHtmlTag = "<br />";
        const string _subjectItemId = "SubjectItemId";
        const string _objectDataTextFieldViewStateKey = "_objectDataTextField";
        const string _zentityContext = "ZentityContext";
        const string _getResourcesMethodName = "GetResources";
        const string _comma = ",";
        const string _sortTagByName = "Name";
        const string _subjectLabelId = "SubjectLabel";
        const string _subjectDisplayLabelId = "SubjectDisplayLabel";
        const string _filterRadioButtonListId = "FilterRadioButtonList";
        const string _filterButtonId = "FilterButton";
        const string _errorLabelId = "ErrorLabel";
        const string _sourceCheckBoxListDivId = "SourceCheckBoxListDiv";
        const string _sourceCheckBoxListId = "SourceCheckBoxList";
        const string _sourceCheckBoxListLabelId = "SourceCheckBoxListLabel";
        const string _destinationCheckBoxListDivId = "DestinationCheckBoxListDiv";
        const string _destinationCheckBoxListId = "DestinationCheckBoxList";
        const string _destinationCheckBoxListLabelId = "DestinationCheckBoxListLabel";
        const string _moveUpButtonId = "MoveUpButton";
        const string _moveDownButtonId = "MoveDownButton";
        const string _moveLeftButtonId = "MoveLeftButton";
        const string _moveRightButtonId = "MoveRightButton";
        const string _sourceSizeLabelId = "SourceSizeLabel";
        const string _sourceSizeListDropDownId = "SourceSizeDropDown";
        const string _sourceListSize = "SourceListSize";
        const int _defaultPageSize = 50;
        const string _javascriptFile = "JavascriptFile";
        const string _commonScriptPath = "Zentity.Web.UI.ToolKit.Scripts.CommonScript.js";
        const string _showHideFilterCriteria = "javascript: ShowHideControl('{0}','{1}');";
        const string _OnClickEvent = "OnClick";
        const string _true = "true";
        const string _false = "false";
        const string _styleDisplayNone = "none";
        const string _styleDisplayBlocked = "blocked";
        const string _resourceTypeSearchCriterion = "resourcetype:{0}";

        #endregion

        #endregion

        #region MemberVariables

        #region Protected

        /// <summary>
        /// Label control for subject item.
        /// </summary>
        protected Label _subjectLabel;

        /// <summary>
        /// Hyperlink control for subject item.
        /// </summary>
        protected HyperLink SubjectDisplayLink;

        /// <summary>
        /// Radio button list used for toggling filterCriteria control on/off.
        /// </summary>
        protected RadioButtonList FilterSwitchRadioButtonList;

        /// <summary>
        /// Button control on whose click data source should be refreshed.
        /// </summary>
        protected Button FilterButton;

        /// <summary>
        /// Division for source check box list.
        /// </summary>
        protected HtmlGenericControl SourceCheckBoxListDiv;

        /// <summary>
        /// CheckBoxList to hold source items that can be associated with subject item.
        /// </summary>
        protected CheckBoxList SourceCheckBoxList;

        /// <summary>
        /// Label for Source CheckBoxList.
        /// </summary>
        protected Label SourceCheckBoxListLabel;

        /// <summary>
        /// Division for destination check box list.
        /// </summary>
        protected HtmlGenericControl DestinationCheckBoxListDiv;

        /// <summary>
        /// CheckBoxList to hold destination items that are or selected from source CheckBoxList to be
        /// associated with subject item.
        /// </summary>
        protected CheckBoxList DestinationCheckBoxList;

        /// <summary>
        /// Label for Destination CheckBoxList.
        /// </summary>
        protected Label DestinationCheckBoxListLabel;

        /// <summary>
        /// Label control for showing error messages.
        /// </summary>
        protected Label ErrorLabel;

        /// <summary>
        /// Drop down control to select range of records to be retrieved for
        /// source CheckBoxLists in case total records exceed source list size.
        /// </summary>
        protected DropDownList SourceListSizeDropDown;

        /// <summary>
        /// Label control that holds caption text for source list size drop down.
        /// </summary>
        protected Label SourceListSizeLabel;

        #endregion

        #region Private

        Button _moveUpButton;
        Button _moveDownButton;
        Button _moveLeftButton;
        Button _moveRightButton;

        TableItemStyle _labelStyle;
        TableItemStyle _controlStyle;
        TableItemStyle _buttonStyle;

        Table _container;
        Table _filterTable;
        Table _filterGridTable;
        Table _associationTable;

        TableCell sourceTableCell;
        TableCell destinationTableCell;
        PropertyFilterCriteria _filterCriteriaControl;


        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the maximum number of items to be displayed in source CheckBoxList.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionAssociationListSizes")]
        [Localizable(true)]
        public int SourceListSize
        {
            get
            {
                return ViewState[_sourceListSize] != null ? (int)ViewState[_sourceListSize] : _defaultPageSize;
            }
            set
            {
                ViewState[_sourceListSize] = value;
            }
        }

        /// <summary>
        /// Gets or sets the height for association CheckBoxLists.      
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionAssociationListHeight")]
        [Localizable(true)]
        public Unit AssociationListHeight
        {
            get
            {
                return ViewState[_associationListHeightViewStateKey] != null ?
                    (Unit)ViewState[_associationListHeightViewStateKey] : Unit.Empty;
            }
            set
            {
                ViewState[_associationListHeightViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the width for association CheckBoxLists.      
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionAssociationListWidth")]
        [Localizable(true)]
        public Unit AssociationListWidth
        {
            get
            {
                return ViewState[_associationListWidthViewStateKey] != null ?
                    (Unit)ViewState[_associationListWidthViewStateKey] : Unit.Empty;
            }
            set
            {
                ViewState[_associationListWidthViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the labels of the control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionZentityLabelStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle LabelStyle
        {
            get
            {
                if (this._labelStyle == null)
                {
                    this._labelStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._labelStyle).TrackViewState();
                    }
                }
                return this._labelStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the input controls of the control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionZentityControlStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public new TableItemStyle ControlStyle
        {
            get
            {
                if (this._controlStyle == null)
                {
                    this._controlStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._controlStyle).TrackViewState();
                    }
                }
                return this._controlStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the buttons of the control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionZentityButtonStyle"),
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
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the header row of the filter criteria table.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaHeaderStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaHeaderStyle
        {
            get
            {
                return FilterCriteriaGrid.HeaderStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the rows of the filter criteria table.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaRowStyle
        {
            get
            {
                return FilterCriteriaGrid.RowStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the alternate rows of the filter criteria table.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaAlternateRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaAlternatingRowStyle
        {
            get
            {
                return FilterCriteriaGrid.AlternatingRowStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of the controls of the filter criteria table. 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaControlStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle FilterCriteriaControlStyle
        {
            get
            {
                return FilterCriteriaGrid.InputControlStyle;
            }
        }

        /// <summary>
        /// Gets or sets the list of items in the source list.
        /// </summary>
        [Browsable(false)]
        public IList SourceList
        {
            get
            {
                return ViewState[_sourceListViewStateKey] as IList;
            }
            set
            {
                ViewState[_sourceListViewStateKey] = value;
                // Only bind the CheckBoxList if it has been initialized.
                if (SourceCheckBoxList != null)
                {
                    DataBindControl(SourceCheckBoxList, SourceList, ObjectDataTextField, ObjectDataValueField);
                    UpdateButtons();
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of items in the destination list.
        /// </summary>
        [Browsable(false)]
        public IList DestinationList
        {
            get
            {
                return ViewState[_destinationListViewStateKey] as IList;
            }
            set
            {
                ViewState[_destinationListViewStateKey] = value;
                // Only bind the CheckBoxList if it has been initialized.
                if (DestinationCheckBoxList != null)
                {
                    DataBindControl(DestinationCheckBoxList, DestinationList, ObjectDataTextField, ObjectDataValueField);
                    UpdateButtons();
                }
            }
        }

        /// <summary>
        /// Gets or sets the properties of the button that moves selected items in the 
        /// destination list in upward direction.
        /// </summary>
        [ZentityCategory("CategoryAppearance"),
        ZentityDescription("DescriptionAssociationMoveUpButton"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public Button MoveUpButton
        {
            get
            {
                return _moveUpButton;
            }
            set
            {
                _moveUpButton = value;
            }
        }

        /// <summary>
        /// Gets or sets the properties of the button that moves selected items in the 
        /// destination list in downward direction.
        /// </summary>
        [ZentityCategory("CategoryAppearance"),
        ZentityDescription("DescriptionAssociationMoveDownButton"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public Button MoveDownButton
        {
            get
            {
                return _moveDownButton;
            }
            set
            {
                _moveDownButton = value;
            }
        }

        /// <summary>
        /// Gets or sets the properties of the button that moves selected items from the 
        /// destination list to the source list.
        /// </summary>
        [ZentityCategory("CategoryAppearance"),
        ZentityDescription("DescriptionAssociationMoveLeftButton"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public Button MoveLeftButton
        {
            get
            {
                return _moveLeftButton;
            }
            set
            {
                _moveLeftButton = value;
            }
        }

        /// <summary>
        /// Gets or sets the properties of the button that moves selected items from the 
        /// source list to the destination list.
        /// </summary>
        [ZentityCategory("CategoryAppearance"),
        ZentityDescription("DescriptionAssociationMoveRightButton"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public Button MoveRightButton
        {
            get
            {
                return _moveRightButton;
            }
            set
            {
                _moveRightButton = value;
            }
        }

        /// <summary>
        /// Gets or sets the id of the item that has to be associated using this control.
        /// </summary>
        [Browsable(false)]
        public Guid SubjectItemId
        {
            get
            {
                return (ViewState[_subjectItemIdViewStateKey] != null) ?
                    (Guid)ViewState[_subjectItemIdViewStateKey] : Guid.Empty;
            }
            set
            {
                ViewState[_subjectItemIdViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL to be provided on Subject item.
        /// If the URL contains '{0}', it will be replaced by the id of the resource.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceTableTitleUrl")]
        [Localizable(true)]
        public string SubjectItemDestinationPageUrl
        {
            get
            {
                return ViewState[_subjectItemDestinationUrlViewStateKey] != null ?
                    (string)ViewState[_subjectItemDestinationUrlViewStateKey] : Constants.Hash;
            }
            set
            {
                ViewState[_subjectItemDestinationUrlViewStateKey] = value;
            }
        }

        #endregion

        #region Protected

        /// <summary>
        /// Gets or sets the text field for CheckBoxLists.
        /// </summary>
        protected string ObjectDataTextField
        {
            get
            {
                return ViewState[_objectDataTextFieldViewStateKey] != null ? (string)ViewState[_objectDataTextFieldViewStateKey] : _resourceDefaultProperty;
            }
            set
            {
                ViewState[_objectDataTextFieldViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets table control holding filter controls.
        /// </summary>
        protected Table FilterTable
        {
            get
            {
                return _filterTable;
            }
        }

        /// <summary>
        /// Gets table control holding filter grid control.
        /// </summary>
        protected Table FilterGridTable
        {
            get
            {
                return _filterGridTable;
            }
        }

        /// <summary>
        /// Gets parent table holding all child tables.
        /// </summary>
        protected Table ContainerTable
        {
            get
            {
                return _container;
            }
        }

        /// <summary>
        /// Gets table container control holding association controls.
        /// </summary>
        protected Table AssociationTable
        {
            get
            {
                return _associationTable;
            }
        }

        /// <summary>
        /// Gets filter criteria grid control.
        /// </summary>
        protected PropertyFilterCriteria FilterCriteriaGrid
        {
            get
            {
                if (_filterCriteriaControl == null)
                {
                    _filterCriteriaControl = new PropertyFilterCriteria();
                    _filterCriteriaControl.ID = GetControlId("FilterControl");
                    _filterCriteriaControl.Height = _filterCriteriaControl.Width =
                        new Unit("100%", CultureInfo.InvariantCulture);
                }

                return _filterCriteriaControl;
            }
        }


        #endregion

        #endregion

        #region Events

        #region Protected

        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            CreateFilterControls();
            CreateFilterGridControls();
            CreateAssociationControls();
            CreateControlsLayout();
            base.CreateChildControls();
        }

        /// <summary>
        ///  Handles the Load event of the control.
        /// </summary>
        /// <param name="e">Event argument.</param>
        protected override void OnLoad(EventArgs e)
        {
            //Register javascript
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _javascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _commonScriptPath));

            base.OnLoad(e);
        }

        /// <summary>
        /// Handles the PreRender event of the control.
        /// </summary>
        /// <param name="e">Event argument.</param>
        protected override void OnPreRender(EventArgs e)
        {
            if (FilterSwitchRadioButtonList != null)
            {
                //Register OnClick events of radio buttons.
                FilterSwitchRadioButtonList.Items[0].Attributes.Add(_OnClickEvent,
                   string.Format(CultureInfo.InvariantCulture, _showHideFilterCriteria,
                   _filterCriteriaControl.ClientID, _false));

                FilterSwitchRadioButtonList.Items[1].Attributes.Add(_OnClickEvent,
                   string.Format(CultureInfo.InvariantCulture, _showHideFilterCriteria,
                   _filterCriteriaControl.ClientID, _true));

                //Set the visibility of FilterCriteria control based on status of radio buttons.
                if (FilterSwitchRadioButtonList.Items[0].Selected)
                    _filterCriteriaControl.Style.Add(HtmlTextWriterStyle.Display, _styleDisplayNone);
                else
                    _filterCriteriaControl.Style.Add(HtmlTextWriterStyle.Display, _styleDisplayBlocked);
            }
            if (!this.Page.IsPostBack)
                FilterButton_Click(null, null);

            base.OnPreRender(e);
        }

        #endregion

        #endregion

        #region EventHandlers

        #region Private

        /// <summary>
        /// Filter button event handler.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        void FilterButton_Click(object sender, EventArgs e)
        {
            SourceListSizeDropDown.Items.Clear();
            SourceListSizeDropDown.Visible = false;
            SourceListSizeLabel.Visible = false;
            RefreshDataSource();
        }

        /// <summary>
        /// Moves selected items upward in the destination list.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            List<int> selectedIndices = GetSelectedIndices(DestinationCheckBoxList);

            if (selectedIndices.Count > 0 && selectedIndices[0] > 0)
            {
                MoveUpItemsInDestinationList(selectedIndices);
                DataBindControl(DestinationCheckBoxList, DestinationList, ObjectDataTextField, ObjectDataValueField);
                SelectVerticallyMovedListItems(DestinationCheckBoxList, selectedIndices, -1);
                UpdateButtons();
            }
        }

        private static List<int> GetSelectedIndices(CheckBoxList checkBoxList)
        {
            List<int> selectedIndices = new List<int>();
            int count = checkBoxList.Items.Count;
            for (int index = 0; index < count; index++)
            {
                if (checkBoxList.Items[index].Selected)
                {
                    selectedIndices.Add(index);
                }
            }
            return selectedIndices;
        }

        /// <summary>
        /// Moves selected items downward in the destination list.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            List<int> selectedIndices = GetSelectedIndices(DestinationCheckBoxList);

            if (selectedIndices.Count > 0 && selectedIndices[selectedIndices.Count - 1] < (DestinationList.Count - 1))
            {
                MoveDownItemsInDestinationList(selectedIndices);
                DataBindControl(DestinationCheckBoxList, DestinationList, ObjectDataTextField, ObjectDataValueField);
                SelectVerticallyMovedListItems(DestinationCheckBoxList, selectedIndices, 1);
                UpdateButtons();
            }
        }

        /// <summary>
        /// Fetches the selected range of records to be displayed in source CheckBoxList.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        void SourceListDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            SourceList = GetSourceItems();
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        #region Abstract

        /// <summary>
        /// Save method to be implemented by deriving types.
        /// </summary>
        /// <returns>A Boolean value indicating success of operation.</returns>
        public abstract bool SaveAssociation();

        #endregion

        /// <summary>
        /// Refreshes the data sources of source list and destination list.
        /// </summary>
        public void RefreshDataSource()
        {
            IList destinationItems = GetDestinationItems();

            if (destinationItems == null)
            {
                destinationItems = (IList)Activator.CreateInstance(SourceList.GetType());
            }

            DestinationList = destinationItems;

            SourceList = GetSourceItems();

            UpdateButtons();
        }

        #endregion

        #region Protected

        #region Static

        /// <summary>
        /// Returns a table row with the specified table cells as child controls.
        /// </summary>
        /// <param name="childCells">Child cells to be added to the new row.</param>
        /// <returns>New row.</returns>
        protected static TableRow GetRow(params TableCell[] childCells)
        {
            TableRow row = new TableRow();

            row.Cells.AddRange(childCells);

            return row;
        }

        /// <summary>
        /// Returns a table cell containing the specified child controls.
        /// </summary>
        /// <param name="childControls">Child controls of the table cell to be created.</param>
        /// <returns>Table cell containing specified child controls.</returns>
        protected static TableCell CreateCell(params Control[] childControls)
        {
            int cellSpan = 1;
            return CreateCell(cellSpan, false, childControls);
        }

        /// <summary>
        /// Creates a table cell with the specified width containing the specified child controls.
        /// </summary>
        /// <param name="cellWidth">Width of the cell.</param>
        /// <param name="childControls">Child controls of the table cell to be created.</param>
        /// <returns>TableCell containing specified child controls.</returns>
        protected static TableCell CreateCell(Unit cellWidth, params Control[] childControls)
        {
            int cellSpan = 1;
            TableCell cell = CreateCell(cellSpan, false, childControls);
            cell.Width = cellWidth;
            return cell;
        }

        /// <summary>
        /// Creates a table cell with the specified column span containing the specified child controls separated by new lines.
        /// </summary>
        /// <param name="columnSpan">Column span of the cell.</param>
        /// <param name="childControls">Child controls of the table cell to be created.</param>
        /// <returns>Table cell containing specified child controls.</returns>
        protected static TableCell CreateCell(int columnSpan,
            params Control[] childControls)
        {
            return CreateCell(columnSpan, true, childControls);
        }

        /// <summary>
        /// Creates a table cell with the specified column span containing the specified child controls. The <paramref name="newLine"/>
        /// parameter determines whether to separate the child controls with a new line.
        /// </summary>
        /// <param name="columnSpan">Column span of the cell.</param>
        /// <param name="newLine">Determines whether to render new line after each control. </param>
        /// <param name="childControls">Child controls of the table cell to be created.</param>
        /// <returns>Table cell containing specified child controls.</returns>
        protected static TableCell CreateCell(int columnSpan, bool newLine
            , params Control[] childControls)
        {
            TableCell cell = new TableCell();
            cell.ColumnSpan = columnSpan;
            foreach (Control childControl in childControls)
            {
                cell.Controls.Add(childControl);
                if (newLine)
                {
                    cell.Controls.Add(CreateNewLine());
                }
            }
            return cell;
        }

        /// <summary>
        /// Returns a control representing a new line.
        /// </summary>
        protected static Literal CreateNewLine()
        {
            Literal literalNewLine = new Literal();
            literalNewLine.Text = _newLineHtmlTag;
            return literalNewLine;
        }

        /// <summary>
        /// Creates a Label control.
        /// </summary>
        /// <param name="text">Text caption displayed in the Label.</param>
        /// <param name="id">Id of the control.</param>
        /// <returns>A Label control.</returns>
        protected Label CreateLabel(string text, string id)
        {
            Label label = new Label();
            label.ID = this.GetControlId(id);
            label.Text = text;
            label.ToolTip = text;
            label.ApplyStyle(LabelStyle);
            return label;
        }

        /// <summary>
        /// Creates a HyperLink control.
        /// </summary>
        /// <param name="displayText">Text caption to be displayed in the hyperlink.</param>
        /// <param name="navigationUrl">Url to navigate to.</param>
        /// <returns>A HyperLink control.</returns>
        protected HyperLink CreateHyperlink(string displayText, string navigationUrl)
        {
            HyperLink hyperLink = new HyperLink();
            hyperLink.ID = GetControlId(displayText);
            hyperLink.NavigateUrl = navigationUrl;
            hyperLink.Text = HttpUtility.HtmlEncode(displayText);
            return hyperLink;
        }

        /// <summary>
        /// Creates a <see cref="DropDownList"/> control.
        /// </summary>
        /// <param name="autoPostBack">Determines whether control causes post back.</param>
        /// <param name="id">Id of the control.</param>
        /// <returns>A DropDownList control.</returns>
        protected DropDownList CreateDropDownList(bool autoPostBack, string id)
        {
            DropDownList dropDownList = new DropDownList();
            dropDownList.ID = this.GetControlId(id);
            dropDownList.AutoPostBack = autoPostBack;
            dropDownList.ApplyStyle(ControlStyle);
            return dropDownList;
        }

        /// <summary>
        /// Creates a <see cref="DropDownList"/> control.
        /// </summary>
        /// <param name="dataSource">Data source of the DropDownList control.</param>
        /// <param name="dataTextField">Field of the data source that provides the text content
        /// of the list items.</param>
        /// <param name="dataValueField">Field of the data source that provides the value
        /// of each list item.</param>
        /// <param name="autoPostBack">Determines whether control causes post back.</param>
        /// <param name="id">Id of the control.</param>
        /// <returns>A DropDownList control.</returns>
        protected DropDownList CreateDropDownList(object dataSource,
            string dataTextField, string dataValueField,
            bool autoPostBack, string id)
        {
            DropDownList dropDownList = this.CreateDropDownList(autoPostBack, id);
            DataBindControl(dropDownList, dataSource, dataTextField, dataValueField);
            return dropDownList;
        }

        #endregion

        #endregion

        #region Internal

        internal void AuthorizeResourcesBeforeSave<T>(ResourceDataAccess dataAccess) where T : Resource
        {
            if (IsSecurityAwareControl)
            {
                int authorizedResourcesCount = dataAccess.GetAuthorizedResources<T>(AuthenticatedToken,
                    Constants.PermissionRequiredForAssociation, DestinationList as IEnumerable<T>).Count();
                if (authorizedResourcesCount != DestinationList.Count)
                {
                    throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture,
                        GlobalResource.UnauthorizedAccessExceptionMultipleResources, Constants.PermissionRequiredForAssociation));
                }
            }
        }

        #endregion

        #region Protected

        #region Abstract

        /// <summary>
        /// Returns a list of items that should be part of the source list. This abstract method should be 
        /// overridden by derived class. 
        /// </summary>
        /// <returns>List of source items. </returns>
        protected abstract IList GetSourceItems();

        /// <summary>
        /// Returns a list of items that should be part of the destination list. This abstract method should be 
        /// overridden by derived class. 
        /// </summary>
        /// <returns> List of destination items.</returns>
        protected abstract IList GetDestinationItems();

        #endregion

        /// <summary>
        /// Gets resource type from the type name.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        /// <returns>Resource type.</returns>
        protected ResourceType GetEntityType(string typeName)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                return dataAccess.GetResourceType(typeName);
            }
        }

        /// <summary>
        /// Creates the title row.
        /// </summary>
        /// <returns>Table row containing controls to display the title.</returns>
        protected TableRow CreateTitleRow()
        {
            TableCell titleCell = new TableCell();
            titleCell.Text = Title;
            titleCell.ApplyStyle(TitleStyle);
            return GetRow(titleCell);
        }

        /// <summary>
        /// Gets resources for source list.
        /// </summary>
        /// <param name="resourceType">Type of resource.</param>
        /// <returns>List of resources.</returns>
        protected List<Resource> GetSourceResources(string resourceType)
        {
            List<Resource> resources = null;
            int totalRecords = 0;

            int totalParsedCount = GetFetchedRecordsCount();
            string searchCriteria = GetSearchCriteria(resourceType);

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                if (IsSecurityAwareControl)
                {
                    if (AuthenticatedToken != null)
                    {
                        resources = dataAccess.SearchResources(searchCriteria, totalParsedCount,
                        SourceListSize, dataAccess.GetSortProperty(resourceType),
                        out totalRecords, Constants.PermissionRequiredForAssociation, AuthenticatedToken, IsSecurityAwareControl).ToList();
                    }
                }
                else
                {
                    resources = dataAccess.SearchResources(searchCriteria, totalParsedCount,
                        SourceListSize, dataAccess.GetSortProperty(resourceType),
                        out totalRecords, Constants.PermissionRequiredForAssociation, null, IsSecurityAwareControl).ToList();
                }
            }

            ExcludeResource(resources, DestinationList);

            FillSourceListSizeDropDown(totalRecords);

            return resources;
        }

        /// <summary>
        /// Determines whether filter criteria is applied.
        /// </summary>
        /// <returns>A Boolean value indicating whether filter is on.</returns>
        protected bool IsFilterOn()
        {
            return FilterSwitchRadioButtonList.
                SelectedValue.Equals(GlobalResource.Filter,
                StringComparison.OrdinalIgnoreCase) ? true : false;
        }

        /// <summary>
        /// Disables sequence changing of items in destination CheckBoxList.
        /// </summary>
        protected void DisableDestinationItemsSequencing()
        {
            MoveUpButton.Visible = false;
            MoveDownButton.Visible = false;
        }

        #endregion

        #region Private

        /// <summary>
        /// Fills source list drop down box.
        /// </summary>
        /// <param name="totalRecords">Total records retrieved count.</param>
        private void FillSourceListSizeDropDown(int totalRecords)
        {
            if (totalRecords <= SourceListSize)
            {
                SourceListSizeDropDown.Visible = false;
                SourceListSizeLabel.Visible = false;
            }
            else
            {
                SourceListSizeDropDown.Visible = true;
                SourceListSizeLabel.Visible = true;
                if (SourceListSizeDropDown.Items.Count == 0)
                {
                    int listitems = totalRecords / SourceListSize;

                    ListItem lstitem = null;
                    for (int i = 0; i <= listitems; i++)
                    {
                        int recordsFetched = i * SourceListSize;
                        int lowerLimit = recordsFetched + 1;
                        int upperLimit = recordsFetched + SourceListSize;
                        if (totalRecords < upperLimit)
                        {
                            upperLimit = totalRecords;
                        }
                        if (lowerLimit > upperLimit)
                        {
                            continue;
                        }
                        lstitem = new ListItem();
                        lstitem.Text = lowerLimit + "-" + upperLimit;
                        lstitem.Value = Convert.ToString((recordsFetched), CultureInfo.CurrentCulture);
                        SourceListSizeDropDown.Items.Add(lstitem);
                    }
                }
            }
        }

        private string GetSearchCriteria(string resourceType)
        {
            StringBuilder searchStringBuilder = new StringBuilder();

            searchStringBuilder.Append(string.Format(CultureInfo.CurrentCulture, _resourceTypeSearchCriterion,
                   resourceType));

            if (IsFilterOn())
            {
                string propertyCriteria = FilterCriteriaGrid.SearchString;
                if (!string.IsNullOrEmpty(propertyCriteria))
                {
                    searchStringBuilder.Append(Constants.Space);
                    searchStringBuilder.Append(propertyCriteria);
                }
            }
            return searchStringBuilder.ToString();
        }

        /// <summary>
        /// Gets source list records fetched till now.
        /// </summary>
        /// <returns>Number of fetched records.</returns>
        private int GetFetchedRecordsCount()
        {
            int fetchedRecords = 0;

            if (SourceListSizeDropDown.Items.Count > 0)
            {
                fetchedRecords = Convert.ToInt32(SourceListSizeDropDown.SelectedValue,
                    CultureInfo.InvariantCulture);
            }

            return fetchedRecords;
        }

        /// <summary>
        /// Binds the control to the data source.
        /// </summary>
        /// <param name="control">Control to be bound.</param>
        /// <param name="dataSource">Data source to bind to.</param>
        /// <param name="dataTextField">Field of the data source that provides the text content
        /// of the list items.</param>
        /// <param name="dataValueField">Field of the data source that provides the value
        /// of each list item.</param>
        private static void DataBindControl(ListControl control, object dataSource,
            string dataTextField, string dataValueField)
        {
            control.DataTextField = dataTextField;
            control.DataValueField = dataValueField;
            control.DataSource = dataSource;
            control.DataBind();
        }

        /// <summary>
        /// creates blank row.
        /// </summary>
        /// <returns></returns>
        private static TableRow CreateBlankRow()
        {
            TableRow row = new TableRow();
            TableCell cell = CreateCell();
            row.Controls.Add(cell);
            return row;
        }

        /// <summary>
        /// Creates a CheckBoxList control.
        /// </summary>
        /// <param name="id">Id of the control</param>
        /// <returns>A CheckBoxList control.</returns>
        private CheckBoxList CreateCheckBoxList(string id)
        {
            CheckBoxList checkBoxList = new CheckBoxList();
            checkBoxList.ID = this.GetControlId(id);
            checkBoxList.ApplyStyle(ControlStyle);
            return checkBoxList;
        }

        /// <summary>
        /// Creates a RadioButtonList control.
        /// </summary>
        /// <param name="id">Id of the control</param>
        /// <param name="items">Items to be added in the RadioButtonList.</param>
        /// <returns>A RadioButtonList control.</returns>
        private RadioButtonList CreateRadioButtonList(string id, params ListItem[] items)
        {
            RadioButtonList radioButtonList = new RadioButtonList();
            radioButtonList.ApplyStyle(LabelStyle);
            radioButtonList.ID = this.GetControlId(id);
            radioButtonList.Items.AddRange(items);
            if (radioButtonList.Items.Count > 0)
            {
                radioButtonList.SelectedIndex = 0;
            }
            return radioButtonList;
        }

        /// <summary>
        /// Creates a Button control.
        /// </summary>
        /// <param name="id">Id of the control</param>
        /// <param name="text">Text caption displayed in the Button.</param>
        /// <returns>A Button control.</returns>
        private Button CreateButton(string id, string text)
        {
            Button button = new Button();
            button.ApplyStyle(ButtonStyle);
            button.ID = id;
            button.Text = text;
            button.ToolTip = text;
            return button;
        }

        /// <summary>
        /// Moves items one step up in the destination list.
        /// </summary>
        /// <param name="selectedIndices">Indices of items to be moved.</param>
        private void MoveUpItemsInDestinationList(List<int> selectedIndices)
        {
            foreach (int index in selectedIndices)
            {
                SwapDestinationListItems(index - 1, index);
            }
        }

        /// <summary>
        /// Moves items one step down in the destination list.
        /// </summary>
        /// <param name="selectedIndices">Indices of items to be moved.</param>
        private void MoveDownItemsInDestinationList(List<int> selectedIndices)
        {
            for (int itemIndex = (selectedIndices.Count - 1); itemIndex >= 0; itemIndex--)
            {
                SwapDestinationListItems(selectedIndices[itemIndex] + 1, selectedIndices[itemIndex]);
            }
        }

        /// <summary>
        /// Swaps 2 items in the destination list.
        /// </summary>
        /// <param name="itemIndexA">Index of first item.</param>
        /// <param name="itemIndexB">Index of second item.</param>
        private void SwapDestinationListItems(int itemIndexA, int itemIndexB)
        {
            object nextItem = DestinationList[itemIndexA];
            DestinationList[itemIndexA] = DestinationList[itemIndexB];
            DestinationList[itemIndexB] = nextItem;
        }

        /// <summary>
        /// Moves selected items from destination list to source list.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        private void MoveLeftButton_Click(object sender, EventArgs e)
        {
            MoveCheckBoxListItemsHorizontally(DestinationCheckBoxList, DestinationList, SourceCheckBoxList, SourceList);

        }

        /// <summary>
        /// Moves selected items from source list to destination list.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="e">Event arguments.</param>
        private void MoveRightButton_Click(object sender, EventArgs e)
        {
            MoveCheckBoxListItemsHorizontally(SourceCheckBoxList, SourceList, DestinationCheckBoxList, DestinationList);
        }

        /// <summary>
        /// Moves items from one CheckBoxList to another.
        /// </summary>
        /// <param name="sourceCheckBoxList">Source CheckBoxList.</param>
        /// <param name="sourceList">Source list.</param>
        /// <param name="destinationCheckBoxList">Destination CheckBoxList.</param>
        /// <param name="destinationList">Destination list.</param>
        private void MoveCheckBoxListItemsHorizontally(CheckBoxList sourceCheckBoxList, IList sourceList,
            CheckBoxList destinationCheckBoxList, IList destinationList)
        {
            List<int> selectedIndices = GetSelectedIndices(sourceCheckBoxList);

            if (selectedIndices.Count > 0)
            {
                MoveListItems(sourceList, destinationList, selectedIndices);
                DataBindControl(SourceCheckBoxList, SourceList, ObjectDataTextField, ObjectDataValueField);
                DataBindControl(DestinationCheckBoxList, DestinationList, ObjectDataTextField, ObjectDataValueField);
                SelectHorizontallyMovedCheckBoxListItems(destinationCheckBoxList, selectedIndices);
                UpdateButtons();
            }
        }

        /// <summary>
        /// Marks the moved list items in the CheckBoxList as selected.
        /// </summary>
        /// <param name="checkBoxList">CheckBoxList whose items have to be marked as selected.</param>
        /// <param name="selectedIndices">Indices of items that were selected.</param>
        /// <param name="moveSteps">The number of moves by which the selected items were moved.</param>
        private static void SelectVerticallyMovedListItems(CheckBoxList checkBoxList, List<int> selectedIndices, int moveSteps)
        {
            for (int itemIndex = 0; itemIndex < selectedIndices.Count; itemIndex++)
            {
                checkBoxList.Items[selectedIndices[itemIndex] + moveSteps].Selected = true;
            }
        }

        /// <summary>
        /// Moves items from one list to another.
        /// </summary>
        /// <param name="sourceList">Source list.</param>
        /// <param name="destinationList">Destination list.</param>
        /// <param name="selectedIndices">Indices of items that were selected.</param>
        private static void MoveListItems(IList sourceList, IList destinationList, List<int> selectedIndices)
        {
            for (int itemIndex = 0; itemIndex < selectedIndices.Count; itemIndex++)
            {
                object selectedItem = sourceList[selectedIndices[itemIndex] - itemIndex];
                sourceList.Remove(selectedItem);
                destinationList.Add(selectedItem);
            }
        }

        /// <summary>
        /// Marks the moved list items in the CheckBoxList as selected.
        /// </summary>
        /// <param name="checkBoxList">CheckBoxList whose items have to be marked as selected.</param>
        /// <param name="selectedIndices">Indices of items that were selected.</param>
        private static void SelectHorizontallyMovedCheckBoxListItems(CheckBoxList checkBoxList, List<int> selectedIndices)
        {
            int count = checkBoxList.Items.Count;
            for (int index = (count - selectedIndices.Count); index < count; index++)
            {
                checkBoxList.Items[index].Selected = true;
            }
        }

        /// <summary>
        /// Enables/disables buttons in the control based on the status of 
        /// source list and destination list.
        /// </summary>
        private void UpdateButtons()
        {
            if (SourceList == null || SourceList.Count == 0)
            {
                _moveRightButton.Enabled = false;
            }
            else
            {
                _moveRightButton.Enabled = true;
            }
            if (DestinationList == null)
            {
                _moveLeftButton.Enabled = false;
                _moveUpButton.Enabled = false;
                _moveDownButton.Enabled = false;
            }
            else
            {
                if (DestinationList.Count == 0)
                {
                    _moveLeftButton.Enabled = false;
                }
                else
                {
                    _moveLeftButton.Enabled = true;
                }

                if (DestinationList.Count < 2)
                {
                    _moveUpButton.Enabled = false;
                    _moveDownButton.Enabled = false;
                }
                else
                {
                    _moveUpButton.Enabled = true;
                    _moveDownButton.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Excludes specified tags from tag list.
        /// </summary>        
        private void ExcludeResource(IList<Resource> subjectResource, IList destinationResource)
        {
            if (subjectResource != null)
            {
                if (destinationResource != null)
                {
                    foreach (Resource resToExclude in destinationResource)
                    {
                        Resource resourceToRemove = subjectResource.Where
                            (res => res.Id == resToExclude.Id).FirstOrDefault();
                        if (resourceToRemove != null)
                        {
                            subjectResource.Remove(resourceToRemove);
                        }
                    }
                }

                // Remove subject tag.
                if (SubjectItemId != Guid.Empty && subjectResource != null)
                {
                    Resource resourceToRemove = subjectResource.Where
                        (res => res.Id == SubjectItemId).FirstOrDefault();
                    if (resourceToRemove != null)
                    {
                        subjectResource.Remove(resourceToRemove);
                    }
                }
            }
        }

        /// <summary>
        ///  Adds a layout table to the parent control.
        /// </summary>
        private void CreateControlsLayout()
        {
            this.Controls.Clear();
            _container = new Table();
            _container.Width = Unit.Percentage(100);

            if (!string.IsNullOrEmpty(Title))
            {
                _container.Rows.Add(CreateTitleRow());
            }

            //Add ErrorLabel control.
            ErrorLabel = CreateLabel(null, _errorLabelId);
            ErrorLabel.ForeColor = System.Drawing.Color.Red;
            _container.Rows.Add(GetRow(CreateCell(ErrorLabel)));

            //Create filter row and add filter table            
            _filterTable = new Table();
            _filterTable.Rows.AddRange(CreateFilterControlsLayout());
            _filterTable.Width = Unit.Percentage(100);

            TableCell filterCell = CreateCell(_filterTable);
            _container.Rows.Add(GetRow(filterCell));

            //Create filter grid row and add to filter grid table
            _filterGridTable = new Table();
            _filterGridTable.Rows.AddRange(CreateFilterGridLayout());
            _filterGridTable.Width = Unit.Percentage(100);

            _container.Rows.Add(GetRow(CreateCell(_filterGridTable)));

            HtmlGenericControl associationDiv = new HtmlGenericControl("div");
            associationDiv.Style.Add("overflow", GlobalResource.CssDivAssociationOverflow);
            if (this.Width != null)
            {
                associationDiv.Style.Add("width", this.Width.ToString(CultureInfo.InvariantCulture));
            }

            //Create association row and add association table
            _associationTable = new Table();
            _associationTable.Rows.AddRange(CreateAssociationControlsLayout());
            AssignCheckBoxListDimensions();

            associationDiv.Controls.Add(_associationTable);

            _container.Rows.Add(GetRow(CreateCell(associationDiv)));
        }

        /// <summary>
        /// Creates the controls required for association.
        /// </summary>
        private void CreateFilterGridControls()
        {
            FilterButton = CreateButton(_filterButtonId, GlobalResource.AssociationFilterButton);
            FilterButton.Width = Unit.Percentage(15);
            if (!this.DesignMode && _filterCriteriaControl != null)
            {
                FilterButton.ValidationGroup = _filterCriteriaControl.ValidationGroup;
            }
            FilterButton.Click += new EventHandler(FilterButton_Click);
        }

        /// <summary>
        /// Returns table row with filter controls.
        /// </summary>
        /// <returns></returns>
        private TableRow[] CreateFilterGridLayout()
        {
            List<TableRow> rows = new List<TableRow>();

            rows.Add(GetRow(CreateCell(FilterCriteriaGrid)));

            TableCell filterButtonCell = CreateCell(FilterButton);
            filterButtonCell.HorizontalAlign = HorizontalAlign.Center;
            rows.Add(GetRow(filterButtonCell));
            rows.Add(CreateBlankRow());
            rows.Add(CreateBlankRow());
            return rows.ToArray();
        }

        /// <summary>
        ///  Returns table row with filter controls.
        /// </summary>
        private TableRow[] CreateFilterControlsLayout()
        {
            int totalColumns = 2;
            List<TableRow> rows = new List<TableRow>();

            rows.Add(GetRow(CreateCell(Unit.Percentage(20), _subjectLabel),
                CreateCell(SubjectDisplayLink)));
            TableCell cell = CreateCell(totalColumns, false, FilterSwitchRadioButtonList);
            cell.ColumnSpan = totalColumns;
            rows.Add(GetRow(cell));

            return rows.ToArray();
        }

        /// <summary>
        /// Creates the controls required for association.
        /// </summary>
        private void CreateFilterControls()
        {
            _subjectLabel = CreateLabel(null, _subjectLabelId);

            SubjectDisplayLink = CreateHyperlink(_subjectDisplayLabelId,
                string.Format(CultureInfo.InvariantCulture,
                SubjectItemDestinationPageUrl, SubjectItemId));

            FilterSwitchRadioButtonList = CreateRadioButtonList(_filterRadioButtonListId,
                new ListItem(GlobalResource.All),
                new ListItem(GlobalResource.Filter));
            FilterSwitchRadioButtonList.SelectedIndex = 0;
        }

        /// <summary>
        ///  Returns table row with association controls.
        /// </summary>
        private TableRow[] CreateAssociationControlsLayout()
        {
            List<TableRow> rows = new List<TableRow>();

            SourceCheckBoxListDiv.Controls.Add(SourceCheckBoxList);
            sourceTableCell = CreateCell(1, SourceCheckBoxListLabel, SourceCheckBoxListDiv);
            destinationTableCell = CreateCell(1, DestinationCheckBoxListLabel, DestinationCheckBoxListDiv);
            DestinationCheckBoxListDiv.Controls.Add(DestinationCheckBoxList);

            rows.Add(GetRow(sourceTableCell,
                CreateCell(1, MoveRightButton, MoveLeftButton),
                destinationTableCell,
                CreateCell(1, MoveUpButton, MoveDownButton)));

            LiteralControl literalControl = new LiteralControl(Constants.HtmlSpace);

            rows.Add(GetRow(CreateCell(4, literalControl)));
            rows.Add(GetRow(CreateCell(SourceListSizeLabel, literalControl, SourceListSizeDropDown)));

            return rows.ToArray();
        }

        /// <summary>
        /// Creates the controls required for association.
        /// </summary>
        private void CreateAssociationControls()
        {
            SourceCheckBoxListLabel = new Label();
            SourceCheckBoxListLabel.ID = _sourceCheckBoxListLabelId;
            SourceCheckBoxListLabel.Text = GlobalResource.AssociationAvailableResources;

            SourceCheckBoxListDiv = CreateScrollableDiv(_sourceCheckBoxListDivId);

            SourceCheckBoxList = CreateCheckBoxList(_sourceCheckBoxListId);
            SourceCheckBoxList.DataBound += new EventHandler(CheckBoxList_DataBound);
            SourceCheckBoxList.PreRender += new EventHandler(CheckBoxList_PreRender);

            DestinationCheckBoxListLabel = new Label();
            DestinationCheckBoxListLabel.ID = _destinationCheckBoxListLabelId;
            DestinationCheckBoxListLabel.Text = GlobalResource.AssociationSelectedResources;

            DestinationCheckBoxListDiv = CreateScrollableDiv(_destinationCheckBoxListDivId);

            DestinationCheckBoxList = CreateCheckBoxList(_destinationCheckBoxListId);
            DestinationCheckBoxList.DataBound += new EventHandler(CheckBoxList_DataBound);
            DestinationCheckBoxList.PreRender += new EventHandler(CheckBoxList_PreRender);

            if (MoveUpButton == null)
            {
                MoveUpButton = CreateButton(_moveUpButtonId, GlobalResource.AssociationButtonTextMoveUp);
            }
            MoveUpButton.Click += new EventHandler(MoveUpButton_Click);

            if (MoveDownButton == null)
            {
                MoveDownButton = CreateButton(_moveDownButtonId, GlobalResource.AssociationButtonTextMoveDown);
            }
            MoveDownButton.Click += new EventHandler(MoveDownButton_Click);

            if (MoveLeftButton == null)
            {
                MoveLeftButton = CreateButton(_moveLeftButtonId, GlobalResource.AssociationButtonTextMoveLeft);
            }
            MoveLeftButton.Click += new EventHandler(MoveLeftButton_Click);

            if (MoveRightButton == null)
            {
                MoveRightButton = CreateButton(_moveRightButtonId, GlobalResource.AssociationButtonTextMoveRight);
            }
            MoveRightButton.Click += new EventHandler(MoveRightButton_Click);

            SourceListSizeLabel = CreateLabel(Resources.GlobalResource.SourceListSizeText
                , _sourceSizeLabelId);
            SourceListSizeDropDown = CreateDropDownList(true, _sourceSizeListDropDownId);
            SourceListSizeDropDown.SelectedIndexChanged += new EventHandler
                (SourceListDropDown_SelectedIndexChanged);

            SourceListSizeLabel.Visible = false;
            SourceListSizeDropDown.Visible = false;
        }

        void CheckBoxList_DataBound(object sender, EventArgs e)
        {
            CheckBoxList checkBoxList = sender as CheckBoxList;
            if (checkBoxList != null)
            {
                ListItemCollection items = checkBoxList.Items;
                foreach (ListItem item in items)
                {
                    item.Text = HttpUtility.HtmlEncode(item.Text);
                }
            }
        }

        void CheckBoxList_PreRender(object sender, EventArgs e)
        {
            CheckBoxList checkBoxList = sender as CheckBoxList;
            if (checkBoxList != null)
            {
                ListItemCollection items = checkBoxList.Items;
                foreach (ListItem item in items)
                {
                    item.Attributes.Add("title", HttpUtility.HtmlDecode(item.Text));
                    item.Attributes.Add("style", GlobalResource.CssCheckBoxListStyle);
                }
            }
        }

        private static HtmlGenericControl CreateScrollableDiv(string id)
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.ID = id;
            div.Style.Add("overflow", GlobalResource.CssDivAssociationOverflow);
            div.Style.Add("border", GlobalResource.CssDivBorder);
            return div;
        }

        private void AssignCheckBoxListDimensions()
        {
            Unit height = AssociationListHeight;
            Unit width = AssociationListWidth;
            if (height != Unit.Empty)
            {
                string heightValue = height.ToString();
                sourceTableCell.Style.Add("height", heightValue);
                SourceCheckBoxListDiv.Style.Add("height", heightValue);
                destinationTableCell.Style.Add("height", heightValue);
                DestinationCheckBoxListDiv.Style.Add("height", heightValue);
            }
            if (width != Unit.Empty)
            {
                string widthValue = width.ToString();
                sourceTableCell.Style.Add("width", widthValue);
                destinationTableCell.Style.Add("width", widthValue);
                if (width.Type == UnitType.Percentage)
                {
                    AssociationTable.Width = Unit.Percentage(100);
                }
                else
                {
                    SourceCheckBoxListDiv.Style.Add("width", widthValue);
                    DestinationCheckBoxListDiv.Style.Add("width", widthValue);
                }
            }
        }

        /// <summary>
        /// returns control id.
        /// </summary>
        /// <param name="name"> id of the individual control.</param>
        /// <returns> returns unique id which is prefixed with base control id.</returns>
        private string GetControlId(string name)
        {
            if (!string.IsNullOrEmpty(this.ID))
            {
                return this.ID + name;
            }
            return name;
        }

        #endregion

        #endregion
    }
}
