// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Globalization;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This controls Build property filter criteria.
    /// </summary>
    public class PropertyFilterCriteria : GridView
    {
        #region Member Variables

        private TableItemStyle _labelStyle;
        private TableItemStyle _controlStyle;
        private TableItemStyle _buttonStyle;
        private string _connectionString;

        #endregion

        #region Contants

        private const string _and = "AND";
        private const string _or = "OR";
        private const string _FilterTable = "FilterTable";
        private const string _Property = "Property";
        private const string _Value = "Value";
        private const string _Operator = "Operator";
        private const string _Condition = "Condition";
        private const string _AddRemove = "AddRemove";
        private const string _Calender = "Calender";
        private const string _MaskedEdit = "MaskedEdit";
        private const string _EntityType = "EntiyType";
        private const string _DataSource = "DataSource";
        private const string _AddRowCommand = "AddRowCommand";
        private const string _RemoveRowCommand = "RemoveRowCommand";
        private const string _AddRemoveImage = "AddRemoveImage";
        private const string _LogicalOperatorAnd = "And";
        private const string _LogicalOperatorOr = "Or";
        private const string _FilterOperatorContains = "Contains";
        private const string _FilterOperatorEqualTo = "EqualTo";
        private const string _FilterOperatorNotEqualTo = "NotEqualTo";
        private const string _FilterOperatorDoesNotContain = "DoesNotContain";
        private const string _PropertyName = "Name";
        private const string _PropertyId = "Id";
        private const string _PlusImageResourcePath = "Zentity.Web.UI.ToolKit.Image.Plus.GIF";
        private const string _MinusImageResourcePath = "Zentity.Web.UI.ToolKit.Image.Minus.GIF";
        private const string _DefaultValidationGroup = "FilterCriteriaValidation";
        private const string _ValidationGroupKey = "ValidationGroupKey";
        private const string _Star = "*";
        private const int _minimumDate = 1899;
        private const int _maximumYearDate = -7920;
        private const int _maximumMonthDate = -6;
        private const int _maximumDayDate = -25;
        private const string _javascriptFile = "JavascriptFile";
        private const string _commonScriptPath = "Zentity.Web.UI.ToolKit.Scripts.CommonScript.js";
        private const string _onBlurEvent = "onblur";
        private const string _rangeValidatorId = "RangeValidatorId";
        private const string _customValidatorId = "CustomValidatorId";
        private const string _causevalidationFunction = "javascript: CauseValidation('{0}');";
        private const string _longTypeRangeValidatorScript = "LongTypeRangeValidatorScript";
        private const string _wordStartsWith = "WordStartsWith";
        private const string _wordEqual = "WordEqual";
        private const string _equalTo = "=";
        private const string _greaterThan = ">";
        private const string _greaterThanOrEqualTo = ">=";
        private const string _lessThan = "<";
        private const string _lessThanOrEqualTo = "<=";
        private const string _propertyCriterion = "{0}{1}{2}";

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets connection string of database to be used.
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
        /// Gets or sets the object from which the data-bound control retrieves its list of data items.
        /// </summary>
        [Browsable(false)]
        public override object DataSource
        {
            get
            {
                if (base.DataSource == null)
                    return ViewState[_DataSource];
                return base.DataSource;
            }
            set
            {
                ViewState[_DataSource] = value;
                base.DataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets entity type.
        /// </summary>       
        [ZentityCategory("CategoryBehavior"),
        ZentityDescription("DescriptionFilterCriteriaEnityType")]
        public string EntityType
        {
            get
            {
                return ViewState[_EntityType] != null ? ViewState[_EntityType].ToString() : Constants.ResourceFullName;
            }
            set
            {
                ViewState[_EntityType] = value;
                if (this.DataSource != null)
                {
                    DataTable dataTable = this.DataSource as DataTable;
                    if (dataTable != null)
                    {
                        dataTable.Rows.Clear();
                        dataTable.Rows.Add(dataTable.NewRow());
                    }
                }
            }
        }

        /// <summary>
        /// Gets selected filter criteria.
        /// </summary>
        [Browsable(false)]
        public string SearchString
        {
            get
            {
                UpdateDataSource();
                return BuildSearchCriteria();
            }
        }

        /// <summary>
        /// Gets or sets ValidationGroup's name for child controls.
        /// </summary>
        [ZentityCategory("CategoryBehavior"),
        ZentityDescription("Gets or sets ValidationGruop Name for child controls.")]
        public string ValidationGroup
        {
            get
            {
                return ViewState[_ValidationGroupKey] != null ?
                    ViewState[_ValidationGroupKey].ToString() : _DefaultValidationGroup;
            }
            set
            {
                ViewState[_ValidationGroupKey] = value;
            }
        }

        /// <summary>
        /// Gets style defined for Labels.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaLabelStyle"),
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
        /// Gets style defined for input controls.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaControlStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle InputControlStyle
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

            set { _controlStyle = value; }
        }

        /// <summary>
        /// Gets style defined for Buttons.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryStyles"),
        ZentityDescription("DescriptionFilterCriteriaButtonStyle"),
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

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public PropertyFilterCriteria()
            : base()
        {
            AutoGenerateColumns = false;
            RowCommand += new GridViewCommandEventHandler(FilterCriteria_RowCommand);
            RowDataBound += new GridViewRowEventHandler(FilterCriteria_RowDataBound);
        }

        #endregion

        #region Methods

        #region protected

        #region Overriden Methods

        /// <summary>
        /// Sets default data source.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Register javascript for search control
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _javascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _commonScriptPath));

            if (this.DataSource == null)
            {
                DataTable dataTable = CreateFilterTable();
                dataTable.Rows.Add(dataTable.NewRow());
                this.DataSource = dataTable;
            }
            else
            {
                UpdateDataSource();
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// Creates default columns.
        /// </summary>
        /// <param name="dataSource">Represents the data source.</param>
        /// <param name="useDataSource">True to use the data source specified by the dataSource parameter; otherwise, false.</param>
        /// <returns>Collection of columns.</returns>
        protected override ICollection CreateColumns(PagedDataSource dataSource, bool useDataSource)
        {
            if (this.Columns.Count == 0)
            {
                CreateGridViewColumns(this.Columns);
            }
            else
            {
                SetColumnItemTemplates(this.Columns);
            }

            return this.Columns;
        }

        /// <summary>
        /// Calls Databind operation to updated the status of GirdView controls.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            this.DataBind();
            base.OnPreRender(e);
        }
        #endregion

        #endregion

        #region Private

        #region Event Handlers

        /// <summary>
        /// Calls event handlers based on Command Name and Command Argument 
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event argument</param>
        private void FilterCriteria_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int rowIndex = 0;

            if (!string.IsNullOrEmpty(e.CommandArgument.ToString()))
            {
                rowIndex = Convert.ToInt32(e.CommandArgument.ToString(), CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(e.CommandName))
            {
                switch (e.CommandName)
                {
                    case _AddRowCommand:
                        AddButtonClicked();
                        break;
                    case _RemoveRowCommand:
                        RemoveButtonClicked(rowIndex);
                        break;
                }
            }
        }

        /// <summary>
        /// Updates control status for current row
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event Arguments.</param>
        private void FilterCriteria_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!this.DesignMode)
            {
                UpdateControlStatus(e.Row);
            }

            TextBox txtControl = e.Row.FindControl(_Value) as TextBox;
            BaseValidator validator = e.Row.FindControl(_rangeValidatorId) as BaseValidator;
            if (txtControl != null && validator != null)
            {
                txtControl.Attributes.Add(_onBlurEvent,
                  string.Format(CultureInfo.InvariantCulture, _causevalidationFunction, validator.ClientID));
            }

        }

        /// <summary>
        /// Updates Data source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PropertyDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.DataSource != null)
            {
                DropDownList scalarPropertyDropdown = (DropDownList)sender;
                int rowIndex = FindRowIndexForControl(scalarPropertyDropdown);
                ResetProperty(rowIndex);
                UpdateDataSource();
            }
        }

        private void ResetProperty(int rowIndex)
        {
            GridViewRow gridViewRow = this.Rows[rowIndex];
            DropDownList operatorDropDown = (DropDownList)gridViewRow.Cells[1].FindControl(_Operator);
            operatorDropDown.Items.Clear();
            TextBox valuetxt = gridViewRow.Cells[2].FindControl(_Value) as TextBox;
            valuetxt.Text = string.Empty;
        }

        private int FindRowIndexForControl(DropDownList dropDownToFind)
        {
            foreach (GridViewRow gridViewRow in this.Rows)
            {
                DropDownList dropDown = gridViewRow.FindControl(dropDownToFind.ID) as DropDownList;
                if (dropDown != null && dropDownToFind == dropDown)
                {
                    return gridViewRow.RowIndex;
                }
            }
            return -1;
        }

        private bool IsStringProperty(string propertyName)
        {
            bool result = false;
            int dotPosition = EntityType.LastIndexOf(Constants.Dot, StringComparison.Ordinal);
            if (dotPosition == -1)
            {
                throw new ArgumentException(GlobalResource.ResourceTypeFullNameRequired);
            }

            string resourceTypeFullName = EntityType + Constants.Comma + EntityType.Substring(0, dotPosition);
            Type resourceType = Type.GetType(resourceTypeFullName);
            if (resourceType != null)
            {
                PropertyInfo propInfo = resourceType.GetProperty(propertyName);
                if (propInfo != null && propInfo.PropertyType == typeof(String))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Adds new row to row collection of Grid
        /// </summary>
        private void AddButtonClicked()
        {
            if (this.DataSource != null)
            {
                DataTable dataTable = this.DataSource as DataTable;
                dataTable.Rows.Add(dataTable.NewRow());
                this.DataSource = dataTable;
            }
        }

        /// <summary>
        /// Removes current row from row collection of Grid
        /// </summary>
        /// <param name="rowIndex"></param>
        private void RemoveButtonClicked(int rowIndex)
        {
            if (this.DataSource != null)
            {
                DataTable dataTable = this.DataSource as DataTable;
                dataTable.Rows.Remove(dataTable.Rows[rowIndex]);
                this.DataSource = dataTable;
            }
        }

        #endregion

        #region Columns Creation Methods

        /// <summary>
        /// Creates Default GridView Columns
        /// </summary>
        /// <param name="columns">Column collection</param>
        private void CreateGridViewColumns(DataControlFieldCollection columns)
        {
            this.Columns.Clear();
            TemplateField propertyColumn = new TemplateField();
            propertyColumn.HeaderText = Resources.GlobalResource.FilterPropertyColumnHeader;
            propertyColumn.ItemTemplate = new CompiledBindableTemplateBuilder(BuildPropertyDropDown, null);
            columns.Add(propertyColumn);

            TemplateField operatorColumn = new TemplateField();
            operatorColumn.HeaderText = Resources.GlobalResource.FilterOperatorColumnHeader;
            operatorColumn.ItemTemplate = new CompiledBindableTemplateBuilder(BuildOperatorDropDown, null);
            columns.Add(operatorColumn);

            TemplateField valueColumn = new TemplateField();
            valueColumn.HeaderText = Resources.GlobalResource.FilterValueColumnHeader;
            valueColumn.ItemTemplate = new CompiledBindableTemplateBuilder(BuildValueTextBox, null);
            columns.Add(valueColumn);

            TemplateField conditionColumn = new TemplateField();
            conditionColumn.HeaderText = Resources.GlobalResource.FilterConditionColumnHeader;
            conditionColumn.ItemTemplate = new CompiledBindableTemplateBuilder(BuildConditionDropDown, null);
            columns.Add(conditionColumn);

            TemplateField addRemoveColumn = new TemplateField();
            addRemoveColumn.ItemTemplate = new CompiledBindableTemplateBuilder(BuildAddRemoveButton, null);
            columns.Add(addRemoveColumn);
        }

        /// <summary>
        /// Initialize ItemTemplate object for Each TemplateField column.
        /// </summary>
        /// <param name="columns">Column collection</param>
        private void SetColumnItemTemplates(DataControlFieldCollection columns)
        {
            if (columns.Count > 0)
            {
                if (((TemplateField)Columns[0]).ItemTemplate == null)
                {
                    ((TemplateField)Columns[0]).ItemTemplate =
                        new CompiledBindableTemplateBuilder(BuildPropertyDropDown, null);
                }
                if (((TemplateField)Columns[1]).ItemTemplate == null)
                {
                    ((TemplateField)Columns[1]).ItemTemplate =
                        new CompiledBindableTemplateBuilder(BuildOperatorDropDown, null);
                }
                if (((TemplateField)Columns[2]).ItemTemplate == null)
                {
                    ((TemplateField)Columns[2]).ItemTemplate =
                        new CompiledBindableTemplateBuilder(BuildValueTextBox, null);
                }
                if (((TemplateField)Columns[3]).ItemTemplate == null)
                {
                    ((TemplateField)Columns[3]).ItemTemplate =
                        new CompiledBindableTemplateBuilder(BuildConditionDropDown, null);
                }
                if (((TemplateField)Columns[4]).ItemTemplate == null)
                {
                    ((TemplateField)Columns[4]).ItemTemplate =
                        new CompiledBindableTemplateBuilder(BuildAddRemoveButton, null);
                }
            }
        }

        /// <summary>
        /// Initializes and Adds Property DropDownList to Current table cell.
        /// </summary>
        /// <param name="control">Current RableCell</param>
        private void BuildPropertyDropDown(Control control)
        {
            DropDownList propertyDropDown = new DropDownList();
            propertyDropDown.ID = _Property;
            propertyDropDown.AutoPostBack = true;
            propertyDropDown.ApplyStyle(InputControlStyle);
            propertyDropDown.SelectedIndexChanged += new EventHandler(PropertyDropDownList_SelectedIndexChanged);

            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(propertyDropDown);
        }

        private void BuildOperatorDropDown(Control control)
        {
            DropDownList operatorDropDown = new DropDownList();
            operatorDropDown.ID = _Operator;
            operatorDropDown.ApplyStyle(InputControlStyle);

            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(operatorDropDown);
        }

        private void BuildValueTextBox(Control control)
        {
            TextBox textBox = new TextBox();
            textBox.ID = _Value;
            textBox.ApplyStyle(InputControlStyle);

            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(textBox);
        }

        /// <summary>
        /// Initializes and Adds Condition DropDownList to Current table cell.
        /// </summary>
        /// <param name="control">Current RableCell</param>
        private void BuildConditionDropDown(Control control)
        {
            DropDownList conditionDropDown = new DropDownList();
            conditionDropDown.ID = _Condition;
            conditionDropDown.ApplyStyle(InputControlStyle);

            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(conditionDropDown);
        }

        /// <summary>
        /// Initializes and Adds Add/Remove Button to Current table cell.
        /// </summary>
        /// <param name="control">Current RableCell</param>
        private void BuildAddRemoveButton(Control control)
        {
            LinkButton button = new LinkButton();
            button.ID = _AddRemove;
            button.ApplyStyle(ButtonStyle);

            System.Web.UI.WebControls.Image image = new System.Web.UI.WebControls.Image();
            image.ID = _AddRemoveImage;
            image.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _PlusImageResourcePath);
            image.Width = 30;
            image.Width = 30;
            button.Controls.Add(image);

            IParserAccessor access = control as IParserAccessor;
            access.AddParsedSubObject(button);
        }

        #endregion

        #region Fill DropDownLists Methods

        private static void FillOperatorDropDown(DropDownList operatorList, bool stringType)
        {
            operatorList.DataSource = GetOperators(stringType);
            operatorList.DataBind();
        }

        private static List<string> GetOperators(bool stringType)
        {
            List<string> operators = new List<string>();
            if (stringType)
            {
                operators.Add(_wordStartsWith);
                operators.Add(_wordEqual);
            }
            operators.Add(_equalTo);
            operators.Add(_greaterThan);
            operators.Add(_greaterThanOrEqualTo);
            operators.Add(_lessThan);
            operators.Add(_lessThanOrEqualTo);
            return operators;
        }

        /// <summary>
        /// Fills Condition DropDownList
        /// </summary>
        /// <param name="conditionDropdDown">Condition DropDownList</param>
        private static void FillConditionDropDown(DropDownList conditionDropdDown)
        {
            conditionDropdDown.Items.Clear();
            conditionDropdDown.Items.Add(new ListItem(_and));
            conditionDropdDown.Items.Add(new ListItem(_or));
        }

        /// <summary>
        /// Fills Property DropDownList
        /// </summary>
        /// <param name="propertyList">Property DropDownList</param>
        private void FillPropertyDropDownList(DropDownList propertyList)
        {
            Collection<ScalarProperty> resourceProperties = GetResourceProperties();
            if (resourceProperties != null)
            {
                propertyList.DataTextField = propertyList.DataValueField = _PropertyName;
                propertyList.DataSource = resourceProperties.OrderBy(property => property.Name).ToList();
                propertyList.DataBind();
            }
            else
            {
                propertyList.DataSource = null;
                propertyList.DataBind();
            }

        }

        /// <summary>
        /// Creates list of properties of selected ResourceType
        /// </summary>
        /// <returns>List of properties of Resource</returns>
        private Collection<ScalarProperty> GetResourceProperties()
        {
            Collection<ScalarProperty> scalarProperties = null;

            if (!this.DesignMode)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    scalarProperties = dataAccess.GetScalarProperties(this.Page.Cache, EntityType);
                }
            }

            if (scalarProperties != null)
            {
                //Since ID property and properties having data type is binary should not be exposed, remove it from the property list
                List<ScalarProperty> removeProperties = scalarProperties.Where(
                    property => property.Name.Equals(_PropertyId,
                        StringComparison.OrdinalIgnoreCase) || property.DataType == DataTypes.Binary).ToList();

                foreach (ScalarProperty property in removeProperties)
                    scalarProperties.Remove(property);
            }

            return scalarProperties;
        }

        #endregion

        #region DataBinding Methods

        /// <summary>
        /// Creates Filter Table with default columns.
        /// </summary>
        /// <returns></returns>
        private static DataTable CreateFilterTable()
        {
            DataTable fitlerTable = new DataTable(_FilterTable);
            fitlerTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
            fitlerTable.Columns.Add(new DataColumn(_Property));
            fitlerTable.Columns.Add(new DataColumn(_Operator));
            fitlerTable.Columns.Add(new DataColumn(_Value));
            fitlerTable.Columns.Add(new DataColumn(_Condition));

            return fitlerTable;
        }

        /// <summary>
        /// Updates Data source based on status of controls in Each GridView row.
        /// </summary>
        private void UpdateDataSource()
        {
            DataTable dataTable = (DataTable)this.DataSource;
            for (int i = 0; i < this.Rows.Count; i++)
            {
                DataRow dataRow = dataTable.Rows[i];
                GridViewRow gridViewRow = this.Rows[i];
                DropDownList propertyDropDown = gridViewRow.Cells[0].FindControl(_Property) as DropDownList;
                if (propertyDropDown != null)
                {
                    dataRow[_Property] = propertyDropDown.Text;
                }

                DropDownList operatorDropDown = gridViewRow.Cells[1].FindControl(_Operator) as DropDownList;
                if (operatorDropDown != null)
                {
                    dataRow[_Operator] = operatorDropDown.Text;
                }

                TextBox valuetxt = gridViewRow.Cells[2].FindControl(_Value) as TextBox;
                if (valuetxt != null)
                {
                    dataRow[_Value] = valuetxt.Text.Trim();
                }

                DropDownList conditionDropDown = gridViewRow.Cells[3].FindControl(_Condition) as DropDownList;
                if (conditionDropDown != null)
                {
                    dataRow[_Condition] = conditionDropDown.Text;
                }
            }
        }

        /// <summary>
        /// Update status of controls of current row based on set data source.
        /// </summary>
        /// <param name="gridViewRow">Current GridView Row</param>
        private void UpdateControlStatus(GridViewRow gridViewRow)
        {
            if (gridViewRow != null && gridViewRow.Cells.Count == 5)
            {
                DropDownList propertyDropDown = gridViewRow.Cells[0].FindControl(_Property) as DropDownList;
                if (propertyDropDown != null)
                {
                    FillPropertyDropDownList(propertyDropDown);
                    propertyDropDown.Text = DataBinder.Eval(gridViewRow.DataItem, _Property).ToString();
                }

                DropDownList operatorDropDown = gridViewRow.Cells[1].FindControl(_Operator) as DropDownList;
                if (operatorDropDown != null)
                {
                    FillOperatorDropDown(operatorDropDown, IsStringProperty(propertyDropDown.Text));
                    operatorDropDown.Text = DataBinder.Eval(gridViewRow.DataItem, _Operator).ToString();
                }

                TextBox valuetxt = gridViewRow.Cells[2].FindControl(_Value) as TextBox;
                if (valuetxt != null && !string.IsNullOrEmpty(propertyDropDown.Text))
                {
                    UpdateValueControl(gridViewRow.Cells[2], propertyDropDown.Text, gridViewRow.RowIndex);
                    valuetxt.Text = DataBinder.Eval(gridViewRow.DataItem, _Value).ToString();
                }

                DropDownList conditionDropDown = gridViewRow.Cells[3].FindControl(_Condition) as DropDownList;
                if (conditionDropDown != null)
                {
                    FillConditionDropDown(conditionDropDown);
                    int rowCount = ((DataTable)this.DataSource).Rows.Count;
                    string logicalOperator = DataBinder.Eval(gridViewRow.DataItem, _Condition).ToString();
                    if (string.IsNullOrEmpty(logicalOperator) && rowCount > 1 && rowCount > (this.Rows.Count + 1))
                    {
                        conditionDropDown.Text = _and;
                    }
                    else
                    {
                        conditionDropDown.Text = DataBinder.Eval(gridViewRow.DataItem, _Condition).ToString();
                    }
                }

                LinkButton addRemoveButton = gridViewRow.Cells[4].FindControl(_AddRemove) as LinkButton;
                if (addRemoveButton != null)
                {
                    System.Web.UI.WebControls.Image img
                        = addRemoveButton.FindControl(_AddRemoveImage) as System.Web.UI.WebControls.Image;

                    if (((DataTable)DataSource).Rows.Count == (gridViewRow.RowIndex + 1))
                    {
                        addRemoveButton.CommandName = _AddRowCommand;

                        if (img != null)
                            img.ImageUrl = Page.ClientScript.GetWebResourceUrl(
                                this.GetType(), _PlusImageResourcePath);
                    }
                    else
                    {
                        addRemoveButton.CommandName = _RemoveRowCommand;

                        if (img != null)
                            img.ImageUrl = Page.ClientScript.GetWebResourceUrl(
                                this.GetType(), _MinusImageResourcePath);
                    }
                    addRemoveButton.CommandArgument = gridViewRow.RowIndex.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Append extender controls to current cell based on data type of given property.
        /// </summary>
        /// <param name="tableCell">Current table cell.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="criteriaIndex">Index of the criteria.</param>
        private void UpdateValueControl(TableCell tableCell, string propertyName, int criteriaIndex)
        {
            DataTypes type = DataTypes.String;

            type = GetPropertyType(propertyName);

            switch (type)
            {
                case DataTypes.DateTime:
                    {
                        if (tableCell.FindControl(_Calender) == null)
                        {
                            RangeValidator rngValidator = new RangeValidator();
                            rngValidator.ID = _rangeValidatorId;
                            rngValidator.Type = ValidationDataType.Date;
                            rngValidator.MinimumValue = DateTime.MinValue.Date.AddYears(_minimumDate).ToString(CoreHelper.GetDateFormat(), CultureInfo.InvariantCulture);
                            rngValidator.MaximumValue = DateTime.MaxValue.Date.AddYears(_maximumYearDate).AddMonths(_maximumMonthDate).AddDays(_maximumDayDate).ToString(CoreHelper.GetDateFormat(), CultureInfo.InvariantCulture);
                            rngValidator.ErrorMessage = string.Format(CultureInfo.CurrentCulture,
                                Resources.GlobalResource.InvalidCriteria, (criteriaIndex + 1), propertyName, CoreHelper.GetDateFormat());
                            rngValidator.Text = _Star;
                            rngValidator.Display = ValidatorDisplay.Dynamic;
                            rngValidator.ControlToValidate = _Value;
                            rngValidator.ValidationGroup = ValidationGroup;
                            tableCell.Controls.Add(rngValidator);
                        }
                        break;
                    }
                case DataTypes.Int16:
                case DataTypes.Int32:
                    {
                        if (tableCell.FindControl(_MaskedEdit) == null)
                        {
                            RangeValidator rngValidator = new RangeValidator();
                            rngValidator.ID = _rangeValidatorId;
                            rngValidator.ControlToValidate = _Value;
                            rngValidator.ValidationGroup = ValidationGroup;
                            rngValidator.Display = ValidatorDisplay.Dynamic;
                            rngValidator.Type = ValidationDataType.Integer;
                            rngValidator.Text = _Star;
                            if (type == DataTypes.Int16)
                            {
                                rngValidator.MinimumValue = short.MinValue.ToString(CultureInfo.InvariantCulture);
                                rngValidator.MaximumValue = short.MaxValue.ToString(CultureInfo.InvariantCulture);
                            }
                            else if (type == DataTypes.Int32)
                            {
                                rngValidator.MinimumValue = int.MinValue.ToString(CultureInfo.InvariantCulture);
                                rngValidator.MaximumValue = int.MaxValue.ToString(CultureInfo.InvariantCulture);
                            }
                            rngValidator.ErrorMessage = string.Format(CultureInfo.InvariantCulture, Resources.GlobalResource.InvalidCriteriaNumber, (criteriaIndex + 1), propertyName); ;
                            tableCell.Controls.Add(rngValidator);
                        }
                        break;
                    }
                case DataTypes.Int64:
                    {
                        CustomValidator customValidator = new CustomValidator();
                        customValidator.ID = _customValidatorId;
                        customValidator.ControlToValidate = _Value;
                        customValidator.ValidationGroup = ValidationGroup;
                        customValidator.Display = ValidatorDisplay.Dynamic;
                        customValidator.Text = _Star;
                        customValidator.ClientValidationFunction = _longTypeRangeValidatorScript;
                        customValidator.ErrorMessage = string.Format(CultureInfo.InvariantCulture, Resources.GlobalResource.InvalidCriteriaNumber, (criteriaIndex + 1), propertyName); ;
                        tableCell.Controls.Add(customValidator);
                        break;
                    }
            }
        }

        /// <summary>
        /// Gets Data type of given property.
        /// </summary>        
        /// <param name="propertyName">Property name</param>
        /// <returns>DataType of given property</returns>
        private DataTypes GetPropertyType(string propertyName)
        {
            DataTypes propertyType = DataTypes.String;
            if (!this.DesignMode)
            {
                ScalarProperty property = null;

                using (ResourceDataAccess dataAccess = new ResourceDataAccess(CreateContext()))
                {
                    property = dataAccess.GetScalarProperty(this.Page.Cache, EntityType, propertyName);
                }
                if (property != null)
                {
                    propertyType = property.DataType;
                }
            }
            return propertyType;
        }

        private string BuildSearchCriteria()
        {
            StringBuilder searchStringBuilder = new StringBuilder();
            if (this.DataSource != null)
            {
                DataTable dataTable = this.DataSource as DataTable;
                if (dataTable != null)
                {
                    string latestSeparator = string.Empty;
                    DataRowCollection dataRows = dataTable.Rows;
                    foreach (DataRow dataRow in dataRows)
                    {
                        string value = (string)dataRow[_Value];
                        if (!string.IsNullOrEmpty(value) || !string.IsNullOrEmpty(value.Trim()))
                        {
                            string oper = (string)dataRow[_Operator];
                            if (ValueEscapeRequired(ref oper))
                            {
                                value = Constants.DoubleQuotes + EscapeDoubleQuotes(value) + Constants.DoubleQuotes;
                            }
                            searchStringBuilder.Append(string.Format(CultureInfo.CurrentCulture, _propertyCriterion,
                                dataRow[_Property], oper, value));
                            latestSeparator = Constants.Space + (string)dataRow[_Condition] + Constants.Space;
                            searchStringBuilder.Append(latestSeparator);
                        }
                    }
                    if (searchStringBuilder.Length != 0)
                    {
                        int latestSeparatorLength = latestSeparator.Length;
                        searchStringBuilder.Remove(searchStringBuilder.Length - latestSeparatorLength, latestSeparatorLength);
                    }
                }
            }
            return searchStringBuilder.ToString();
        }

        private static bool ValueEscapeRequired(ref string oper)
        {
            if (oper == _wordEqual)
            {
                oper = Constants.Colon;
                return true;
            }
            if (oper == _equalTo || oper == _greaterThan || oper == _greaterThanOrEqualTo || oper == _lessThan
                || oper == _lessThanOrEqualTo)
            {
                oper = Constants.Colon + oper;
                return true;
            }
            //Default
            oper = Constants.Colon;
            return false;
        }

        private static string EscapeDoubleQuotes(string value)
        {
            return value.Replace(Constants.DoubleQuotes, Constants.EscapedDoubleQuotes);
        }

        /// <summary>
        /// Create ZentityContext object based on connection string.
        /// </summary>
        /// <returns></returns>
        private ZentityContext CreateContext()
        {
            ZentityContext context = null;
            if (string.IsNullOrEmpty(this.ConnectionString))
            {
                context = new ZentityContext();
            }
            else
            {
                context = new ZentityContext(this.ConnectionString);
            }
            return context;
        }

        #endregion

        #endregion

        #endregion
    }
}