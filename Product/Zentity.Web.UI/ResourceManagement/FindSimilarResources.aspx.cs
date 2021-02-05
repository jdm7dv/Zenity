// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Zentity.Core;
using System.Collections.Generic;
using System.Reflection;
using Zentity.Platform;
using Zentity.Web.UI;
using System.Globalization;
using Zentity.Security.Authentication;

public partial class ResourceManagement_FindSimilarResources : ZentityBasePage
{
    #region Member variables
    Resource _resource = null;
    private Guid _guid;
    #endregion

    #region Constants

    private const char _CharDot = '.';
    private const string _TableId = "tableAllProperties";
    private const string _DateAdded = "DateAdded";
    private const string _chk = "chk";
    private const string _MinimumPercentageMatchExpected = "MinimumPercentageMatchExpected";
    private const string _th = "th";
    private const string _FieldName = "Field Name";
    private const string _ValueToBeMatched = "Value to be matched";
    private const string _PercentChar = "%";
    private const string _id = "Id";
    private const string _title = "Title";
    private const string _percentageMatch = "PercentageMatch";
    private const string _percentageMatchValue = "30";
    private const string _hiddenIdField = "hiddenResourceId";

    #endregion

    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        errorMessage.Visible = false;
        errorMessage.Text = string.Empty;

        if (!IsPostBack)
        {
            MinPerMatchTextBox.Text = System.Configuration.ConfigurationManager.AppSettings[_MinimumPercentageMatchExpected];
            if (MinPerMatchTextBox.Text == string.Empty)
            {
                MinPerMatchTextBox.Text = _percentageMatchValue;
            }
            LocalizePage();
            MinPerMatchTextBox.Attributes.Add("onchange", "javascript:return textChange('" + MinPerMatchTextBox.ClientID + "','" + MinPerMatchTextBox.Text + "');");
        }
        pager.OnPageChanged += new EventHandler<EventArgs>(pager_OnPageChanged);
    }

    protected void pager_OnPageChanged(object sender, EventArgs e)
    {
        FillResultGrid();

        if (OnPageChanged != null)
            OnPageChanged(sender, e);
    }

    /// <summary>
    /// This event is fired when current page index changes.
    /// </summary>
    public event EventHandler<EventArgs> OnPageChanged;


    protected void Page_Init(object sender, EventArgs e)
    {
        if (Request.QueryString.AllKeys.Contains(Constants.Id))
        {
            try
            {
                this._guid = new Guid(Request.QueryString[Constants.Id]);
            }
            catch (FormatException)
            {
                this.DisplayError(Resources.Resources.InvalidResource);
                return;
            }
        }
        else
        {
            this.DisplayError(Resources.Resources.SimilarityMatchMissingId);
            return;
        }
        if (Session[this._guid.ToString("D")] != null)
        {
            this._resource = (Resource)Session[this._guid.ToString("D")];

            using (ResourceDataAccess resourceDAL = new ResourceDataAccess())
            {
                IEnumerable<ScalarProperty> properties = resourceDAL.GetScalarPropertyCollection(this._resource.ToString());
                CreateTableForAllProperties(this._resource, properties);
            }
        }
        else
        {
            LabelError.Text = Resources.Resources.SimilarityMatchResourceSaved;
            LabelError.Visible = true;
            ButtonSearch.Visible = false;
            MinimumPerMatchLabel.Visible = false;
            MinPerMatchTextBox.Visible = false;
            RequireFieldLabel.Visible = false;
            SelectPropertyLabel.Visible = false;
            TableAllProperties.Visible = false;
        }
    }

    protected void ButtonSearch_Click(object sender, EventArgs e)
    {
        Page.Title = Resources.Resources.SimilarityMatchLabelMatchingRecordsText;
        if (this._resource == null)
        {
            LabelError.Text = Resources.Resources.SimilarityMatchResourceSaved;
            LabelError.Visible = true;
            ButtonSearch.Visible = false;
            return;
        }

        FillResultGrid();
    }

    void FillResultGrid()
    {
        // Retrieve checkbox
        Table table = (Table)(this.Panel1.FindControl(_TableId));
        ICollection<PropertyValuePair> searchCriteria = GetSearchCriteria(this._resource, table);

        if (searchCriteria.Count == 0)
        {
            LabelCriteriaRequire.Text = Resources.Resources.SimilarityMatchSelectProrperties;
            LabelCriteriaRequire.Visible = true;
            return;
        }

        this.PanelStep1.Visible = false;
        this.PanelStep2.Visible = true;
        LabelRecordNotMatch.Visible = true;
        DataTable similarRecordsTable = GetSearchResult(this._resource, searchCriteria);
        if (similarRecordsTable.Rows.Count > 0)
        {
            GridViewMatchingRecord.DataSource = similarRecordsTable;
            LabelRecordNotMatch.Text = Resources.Resources.SimilarityMatchResourceFound;
            GridViewMatchingRecord.DataBind();
            pager.Visible = true;
        }
        else
        {
            GridViewMatchingRecord.Visible = true;
            LabelRecordNotMatch.Text = Resources.Resources.SimilarityMatchResourceIsNotWithExisting;
            ButtonCreateNew.Visible = true;
            pager.Visible = false;
        }
    }

    protected void CreateNewResource_Click(object sender, EventArgs e)
    {
        if (this._resource == null)
        {
            LabelRecordNotMatch.Text = Resources.Resources.SimilarityMatchResourceSaved;
            ButtonCreateNew.Visible = false;
            LabelRecordNotMatch.Visible = true;
            return;
        }
        string typeName = this._resource.GetType().Name;

        //Add resource to database
        using (ResourceDataAccess resourceDAL = new ResourceDataAccess())
        {
            resourceDAL.AddResource(this._resource);
            GrantOwnership(this._resource.Id);
        }

        //Do clean up and redirect.
        Session.Remove(this._guid.ToString("D"));
        this.ButtonCreateNew.Visible = false;
        this.PanelResultGrid.Visible = false;

        errorMessage.Text = string.Format(CultureInfo.CurrentCulture,
            Resources.Resources.AlertResourceAdded,
            typeName, Utility.GetLinkTag(Resources.Resources.ManageResourceLink + _resource.Id,
                        Utility.FitString(_resource.Title, 40)));
        errorMessage.Visible = true;


    }

    #endregion

    #region Methods

    private bool GrantOwnership(Guid resourceId)
    {
        AuthenticatedToken userToken = (AuthenticatedToken)Session[Constants.AuthenticationTokenKey];
        if (resourceId != Guid.Empty)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
            {
                return dataAccess.GrantDefaultOwnership(userToken, resourceId);
            }
        }

        return false;
    }

    private void LocalizePage()
    {
        ButtonCreateNew.Text = Resources.Resources.SimilarityMatchButtonCreateNew;
        ButtonSearch.Text = Resources.Resources.SimilarityMatchButtonSearchText;
        LabelMatchCriteria.Text = Resources.Resources.SimilarityMatchLabelMatchCriteriaText;
        LabelMatchingRecords.Text = Resources.Resources.SimilarityMatchLabelMatchingRecordsText;
        Page.Title = Resources.Resources.FindSimilarResourcesPageTitle;
    }

    private void DisplayError(string message)
    {
        this.LabelError.Visible = true;
        this.LabelError.Text = message;
        this.ButtonSearch.Visible = false;
        MinimumPerMatchLabel.Visible = false;
        MinPerMatchTextBox.Visible = false;
        RequireFieldLabel.Visible = false;
        SelectPropertyLabel.Visible = false;
        TableAllProperties.Visible = false;
    }

    private void CreateTableForAllProperties(Resource resource, IEnumerable<ScalarProperty> properties)
    {
        Type type = resource.GetType();

        //Add rows
        foreach (ScalarProperty property in properties)
        {
            PropertyInfo propertyInfo = type.GetProperty(property.Name);
            object value = null;
            if (propertyInfo != null)
            {
                value = propertyInfo.GetValue(resource, null);
            }
            if (value != null && !string.IsNullOrEmpty(value.ToString()) && property.Name != Constants.Id &&
                property.Name != _DateAdded && value.GetType() != typeof(System.Byte[]))
            {
                TableRow row = new TableRow();

                TableCell cell = new TableCell();
                CheckBox checkbox = new CheckBox();
                checkbox.ID = _chk + property.Name;
                checkbox.Text = property.Name;
                cell.Controls.Add(checkbox);
                row.Cells.Add(cell);

                cell = new TableCell();

                LiteralControl control = new LiteralControl();
                cell.Controls.Add(control);
                control.Text = Server.HtmlEncode(value.ToString());
                cell.Controls.Add(control);
                row.Cells.Add(cell);
                int count = TableAllProperties.Rows.Count;
                Utility.ApplyRowStyle(row, count);
                TableAllProperties.Rows.Add(row);
            }
        }
    }

    private ICollection<PropertyValuePair> GetSearchCriteria(Resource resource, Table table)
    {
        ICollection<PropertyValuePair> searchCriteria = new List<PropertyValuePair>();

        Type type = resource.GetType();
        foreach (TableRow row in table.Rows)
        {
            if (row.Cells[0].Controls.Count > 0)
            {
                CheckBox checkBox = row.Cells[0].Controls[0] as CheckBox;
                if (checkBox != null && checkBox.Checked)
                {
                    PropertyValuePair criteria; ;
                    PropertyInfo property = type.GetProperty(checkBox.Text);
                    if (property != null)
                    {
                        object propertyValue = property.GetValue(this._resource, null);
                        criteria = new PropertyValuePair(checkBox.Text, propertyValue.ToString());
                        searchCriteria.Add(criteria);
                    }
                }
            }
        }

        return searchCriteria;
    }

    private DataTable GetSearchResult(Resource resource, ICollection<PropertyValuePair> searchCriteria)
    {
        int totalRecords = 0;
        int totalParsedRecords = GridViewMatchingRecord.PageSize * pager.PageIndex;
        int index = resource.ToString().LastIndexOf(_CharDot);
        int minimumPercentageMatchExpected = Convert.ToInt32((MinPerMatchTextBox.Text));

        IEnumerable<SimilarRecord> records = null;
        AuthenticatedToken userToken = (AuthenticatedToken)Session[Constants.AuthenticationTokenKey];
        DataTable similarRecordsTable = new DataTable();
        similarRecordsTable.Columns.Add(_id);
        similarRecordsTable.Columns.Add(_title);
        similarRecordsTable.Columns.Add(_percentageMatch);

        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            records = dataAccess.SearchSimilarResource(resource.ToString().Substring(index + 1), searchCriteria, userToken, totalParsedRecords, out totalRecords, GridViewMatchingRecord.PageSize)
                .Where(tuple => tuple.PercentageMatch > minimumPercentageMatchExpected);

            foreach (SimilarRecord record in records)
            {
                DataRow row = similarRecordsTable.NewRow();
                row[_id] = record.MatchingResource.Id;
                row[_title] = HttpUtility.HtmlEncode(record.MatchingResource.Title);
                row[_percentageMatch] = record.PercentageMatch;
                similarRecordsTable.Rows.Add(row);
            }
        }

        if (GridViewMatchingRecord.PageSize > 0 && totalRecords > 0)
        {
            if (totalRecords > GridViewMatchingRecord.PageSize)
                pager.TotalPages = Convert.ToInt32(Math.Ceiling((double)totalRecords / GridViewMatchingRecord.PageSize));
            else
            {
                pager.PageIndex = 0;
                pager.TotalPages = 1;
            }
        }
        else
        {
            pager.PageIndex = 0;
            pager.TotalPages = 0;
        }

        return similarRecordsTable;
    }

    #endregion
}
