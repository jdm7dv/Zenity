// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects.DataClasses;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Zentity.Administration;
using Zentity.Core;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control displays resource, relationship, resource tagging and category association
    /// change sets. These change sets contain details about data changes occurred to the entities.
    /// </summary>
    /// <example>
    ///     The code below is the source for ChangeHistory.aspx. 
    ///     It shows an example of using <see cref="ChangeHistory"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="Zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head id="Head1" runat="server"&gt;
    ///             &lt;title&gt;ChangeHistory Sample&lt;/title&gt;
    ///             
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_Load(object sender, EventArgs e)
    ///                 {
    ///                     string id = Convert.ToString(Request.QueryString["Id"]);
    ///                     if (string.IsNullOrEmpty(id))
    ///                     {
    ///                         StatusLabel.Text = "Please pass an Id parameter (GUID) in the query string. Example: " +
    ///                             Request.Url.AbsolutePath + "?Id=6bd35f74-5a07-4df8-98a0-7ddb71f88c24";
    ///                         ChangeHistory1.Visible = false;
    ///                    }
    ///                     else
    ///                     {
    ///                         StatusLabel.Text = string.Empty;
    ///                         Guid entityId = new Guid(id);
    ///                         ChangeHistory1.EntityId = entityId;
    ///                     }
    ///                 }
    ///             &lt;/script&gt;
    ///             
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="mainForm" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:ChangeHistory ID="ChangeHistory1" runat="server" IsSecurityAwareControl="false" 
    ///                     MaximumChangeSetToFetch="5" Title="Change History" NavigateUrlForResource="ResourceDetailView.aspx?Id={0}"
    ///                     NavigateUrlForCategory="ResourceDetailView.aspx?Id={0}" NavigateUrlForTag="ResourceDetailView.aspx?Id={0}" &gt;
    ///                 &lt;/Zentity:ChangeHistory&gt;
    ///                 &lt;asp:Label ID="StatusLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [ParseChildren(true)]
    public class ChangeHistory : ZentityBase
    {
        #region Constants

        #region Private

        private const string _propChange = "PropertyChange";
        private const string _scalarPropResTypeId = "ResourceTypeId";
        private const string _subRes = "Subject Resource";
        private const string _objRes = "Object Resource";
        private const string _predicate = "Predicate";
        private const string _ordinalPos = "Ordinal Position";
        private const string _cateResTypeId = "AE40BD66-D25E-408E-BE5D-237224E33297";
        private const string _tagResTypeId = "76A9CB80-FB24-4249-8C09-194F0ACFC895";
        private const string _space = "&nbsp; " + "&nbsp; ";
        private const string _onClick = "onclick";
        private const string _viewPanelFunction = "ViewPanel('{0}','{1}','{2}','{3}');";
        const string _javascriptFile = "ChangeHistoryJavascriptFile";
        const string _commonScriptPath = "Zentity.Web.UI.ToolKit.Scripts.CommonScript.js";
        private const string _smallPlusImagePath = "Zentity.Web.UI.ToolKit.Image.SmallPlus.gif";
        private const string _smallMinusImagePath = "Zentity.Web.UI.ToolKit.Image.SmallMinus.gif";
        private const string _maxChangesetDisplayCountViewStateKey = "MaxChangesetDisplayCount";
        private const string _propertyName = "Property Name";
        private const string _preValue = "PreviousValue";
        private const string _nextValue = "NextValue";
        private const string _changedFlag = "Changed";
        private const string _relAttribute = "Relation Attribute";
        private const string _showMoreLinkText = "Show more";
        private const string _showAllLinkText = "Show all";
        private const string _changeset = "Changeset: ";
        private const string _dateCreated = "DateCreated: ";
        private const string _operation = "Operation: ";
        private const string _imageUrl = "";
        private const string _changeHistoryEnableField = "IsChangeHistoryEnabled";
        private const string _resId = "ResourceId";
        private const string _highCntViewStateKey = "HighestChangeSetCount";

        #region Scalar Property Constants

        private const string _prevFetchedChangesetCntViewStateKey = "PreviouslyFetchedChangesetCount";
        private const string _changesetCntViewStateKey = "ChangesetCount";
        private const string _showMoreBtnId = "ShowMoreBtnId";
        private const string _showAllBtnId = "ShowAllBtnId";

        #endregion

        #region Designer Constants

        private const string _desScalProrTitle = "Scalar Property Changes";
        private const string _desRelationTitle = "Relationship Changes";
        private const string _desResTaggingTitle = "Resource Tagging Changes";
        private const string _desCateAssocTitle = "Category Association";
        private const string _controlTitle = "Change History Logging";

        #endregion

        #region Relationship Constants

        private const string _prevFetchedRelChangesetCntViewStateKey = "PreviouslyFetchedRelChangesetCount";
        private const string _changesetRelCntViewStateKey = "ChangesetRelCount";
        private const string _resNavigateURLViewStateKey = "ResourceNavigateURLViewStateKey";
        private const string _showMoreRelBtnId = "ShowMoreRelBtnId";
        private const string _showAllRelBtnId = "ShowAllRelBtnId";

        #endregion

        #region Resource Tagging

        private const string _prevFetchedTagChangesetCntViewStateKey = "PreviouslyFetchedTagChangesetCount";
        private const string _changesetTagCntViewStateKey = "ChangesetTagCount";
        private const string _tagNavigateURLViewStateKey = "TagNavigateURLViewStateKey";
        private const string _showMoreTagBtnId = "ShowMoreTagBtnId";
        private const string _showAllTagBtnId = "ShowAllTagBtnId";

        #endregion

        #region Resource Categorization

        private const string _prevFetchedCatChangesetCntViewStateKey = "PreviouslyFetchedCategoryChangesetCount";
        private const string _changesetCateCntViewStateKey = "ChangesetCategoryCount";
        private const string _cateNavigateURLViewStateKey = "TCategoryNavigateURLViewStateKey";
        private const string _showMoreCateBtnId = "ShowMoreCateBtnId";
        private const string _showAllCateBtnId = "ShowAllCateBtnId";

        #endregion

        #endregion

        #endregion

        #region Member Variables

        #region Private

        private string _nextObjResId;
        private bool _isAlternateRow;
        private bool _isAlternateRowRel;
        private const int _defaultMaxChangesetCount = 10;
        private CommonProperty _scalarPropertyChangesetStyle = new CommonProperty();
        private CommonProperty _categoryAssociationChangesetStyle = new CommonProperty();
        private CommonProperty _relationshipChangesetStyle = new CommonProperty();
        private CommonProperty _resourceTaggingChangesetStyle = new CommonProperty();
        private ChangeHistoryEntityType _entityType;

        private Table _changeHistoryTable;
        private Table _changeHistoryTagTable;
        private Table _changeHistoryRelationShipTable;
        private Table _changeHistoryCategoryTable;

        #endregion Private
        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets navigation path for tag.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionNavigateUrlForTag")]
        public string NavigateUrlForTag
        {
            get
            {
                return ViewState[_tagNavigateURLViewStateKey] != null ?
                                              (string)ViewState[_tagNavigateURLViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_tagNavigateURLViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets navigation path for resource.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionNavigateUrlForResource")]
        public string NavigateUrlForResource
        {
            get
            {
                return ViewState[_resNavigateURLViewStateKey] != null ? (string)ViewState[_resNavigateURLViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_resNavigateURLViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets navigation path for category.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionNavigateUrlForCategory")]
        public string NavigateUrlForCategory
        {
            get
            {
                return ViewState[_cateNavigateURLViewStateKey] != null ?
                                              (string)ViewState[_cateNavigateURLViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_cateNavigateURLViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="CommonProperty" /> object that allows you to set the 
        /// appearance of the relationship change sets.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionRelationshipChangeSetStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public CommonProperty RelationshipChangeSetStyle
        {
            get
            {
                return _relationshipChangesetStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="CommonProperty" /> object that allows you to set the 
        /// appearance of the resource tagging change sets.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionResourceTaggingChangeSetStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public CommonProperty ResourceTaggingChangeSetStyle
        {
            get
            {
                return _resourceTaggingChangesetStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="CommonProperty" /> object that allows you to set the 
        /// appearance of the category association change sets.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionCategoryAssociationChangeSetStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public CommonProperty CategoryAssociationChangeSetStyle
        {
            get
            {
                return _categoryAssociationChangesetStyle;
            }
        }

        /// <summary>
        /// Gets a reference to the <see cref="CommonProperty" /> object that allows you to set the 
        /// appearance of the scalar properties change sets.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionScalarPropertyChangeSetStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public CommonProperty ScalarPropertyChangeSetStyle
        {
            get { return _scalarPropertyChangesetStyle; }
        }

        /// <summary>
        /// Gets or sets count of maximum change sets to be displayed for each entity.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionMaximumChangeSetToFetch")]
        [Localizable(true)]
        public int MaximumChangeSetToFetch
        {
            get
            {
                return ViewState[_maxChangesetDisplayCountViewStateKey] != null ?
                                            (int)ViewState[_maxChangesetDisplayCountViewStateKey] : _defaultMaxChangesetCount;
            }
            set
            {
                ViewState[_maxChangesetDisplayCountViewStateKey] = value;
            }
        }




        /// <summary>
        /// Gets or sets id of entity.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionEntityId")]
        [Localizable(true)]
        public Guid EntityId
        {
            get
            {
                return ViewState[_resId] != null ? (Guid)ViewState[_resId] : Guid.Empty;
            }
            set
            {
                ViewState[_resId] = value;
            }
        }

        #endregion Public

        #region Internal

        /// <summary>
        /// Gets or sets number of change sets of type resource tagging changes already fetched from 
        /// repository.
        /// </summary>
        internal int HighestChangeSetCnt
        {
            set
            {
                ViewState[_highCntViewStateKey] = value;
            }
            get
            {
                return (int)ViewState[_highCntViewStateKey];
            }
        }

        /// <summary>
        /// Gets or sets number of change sets of type resource tagging changes already fetched from 
        /// repository.
        /// </summary>
        internal int ScalarPropChangsetCnt
        {
            set
            {
                ViewState[_changesetCntViewStateKey] = value;
            }
            get
            {
                return (int)ViewState[_changesetCntViewStateKey];
            }
        }

        /// <summary>
        /// Gets or sets number of change sets of type resource tagging changes already fetched from 
        /// repository.
        /// </summary>
        internal int RelationshipChangsetCnt
        {
            set
            {
                ViewState[_changesetRelCntViewStateKey] = value;
            }
            get
            {
                return (int)ViewState[_changesetRelCntViewStateKey];
            }
        }

        /// <summary>
        /// Gets or sets number of change sets of type resource tagging changes already fetched from 
        /// repository.
        /// </summary>
        internal int ResTaggingChangsetCnt
        {
            set
            {
                ViewState[_changesetTagCntViewStateKey] = value;
            }
            get
            {
                return (int)ViewState[_changesetTagCntViewStateKey];
            }
        }

        /// <summary>
        /// Gets or sets number of change sets of type resource tagging changes already fetched from 
        /// repository.
        /// </summary>
        internal int CateAssociationChangsetCnt
        {
            set
            {
                ViewState[_changesetCateCntViewStateKey] = value;
            }
            get
            {
                return (int)ViewState[_changesetCateCntViewStateKey];
            }
        }


        internal ChangeHistoryEntityType ChangeHistoryEntityTypeName
        {
            set
            {
                _entityType = value;
            }
            get
            {
                return _entityType;
            }
        }

        /// <summary>
        /// Gets or sets number of change sets of type resource tagging changes already fetched from 
        /// repository.
        /// </summary>
        internal int PrevTagChangsetCnt
        {
            set
            {
                ViewState[_prevFetchedTagChangesetCntViewStateKey] = value;
            }
            get
            {
                return ViewState[_prevFetchedTagChangesetCntViewStateKey] != null ?
                                                 (int)ViewState[_prevFetchedTagChangesetCntViewStateKey] : this.MaximumChangeSetToFetch;
            }
        }

        /// <summary>
        /// Gets or sets id of resource in the relationship change set.
        /// </summary>
        internal string NextObjResId
        {
            get
            {
                return _nextObjResId;
            }
            set
            {
                _nextObjResId = value;
            }
        }

        /// <summary>
        /// Gets or sets number of change sets of type category association changes already fetched from 
        /// repository.
        /// </summary>
        internal int PrevCategoryChangsetCnt
        {
            set
            {
                ViewState[_prevFetchedCatChangesetCntViewStateKey] = value;
            }
            get
            {
                return ViewState[_prevFetchedCatChangesetCntViewStateKey] != null ?
                                                 (int)ViewState[_prevFetchedCatChangesetCntViewStateKey] : this.MaximumChangeSetToFetch;
            }
        }

        /// <summary>
        /// Gets or sets number of change sets of type scalar properties changes already fetched from 
        /// repository.
        /// </summary>
        internal int PrevScalarPropChangsetCnt
        {
            set
            {
                ViewState[_prevFetchedChangesetCntViewStateKey] = value;
            }
            get
            {
                return ViewState[_prevFetchedChangesetCntViewStateKey] != null ?
                                            (int)ViewState[_prevFetchedChangesetCntViewStateKey] : this.MaximumChangeSetToFetch;
            }

        }

        /// <summary>
        /// Gets or sets number of change sets of type relationship changes already fetched from repository.
        /// </summary>
        internal int PrevRelChangsetCnt
        {
            set
            {
                ViewState[_prevFetchedRelChangesetCntViewStateKey] = value;
            }
            get
            {

                return ViewState[_prevFetchedRelChangesetCntViewStateKey] != null ?
                                                 (int)ViewState[_prevFetchedRelChangesetCntViewStateKey] : this.MaximumChangeSetToFetch;
            }
        }

        #endregion Internal

        #endregion

        #region Methods

        #region Protected

        /// <summary>
        /// Creates design time appearance of the control.
        /// </summary>
        /// <param name="writer">Instance of HtmlTextWriter that receives the server control content</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode)
            {
                this.CreateDesignerView();
            }
            base.Render(writer);
        }


        /// <summary>
        /// Adds child controls to the control and fire OnLoad event.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Register javascript.
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _javascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _commonScriptPath));

            Table titleContainer = CreateTable();
            CreateTitleRow(titleContainer, this.Title, this.TitleStyle);
            this.Controls.Add(titleContainer);
            if (!IsChangeHistoryEnabled())
            {
                titleContainer.Controls.Add(CreateDesignTitleRow(Resources.GlobalResource.ChangeHistoryFeatureNotEnabled));
                return;
            }

            if (!Page.IsPostBack)
            {
                InitializeScalarPropChangesetCount();
                InitializeRelChangesetCount();
                InitializeCateAssociateChangesetCount();
                InitializeResTaggingChangesetCount();
                SetHighChangeSetCnt();
            }




            CreateChangeHistoryControls();
            base.OnLoad(e);
        }

        /// <summary>
        /// Find highest change set value among change sets of each type and set it to "HighestChangeSetCnt" property.
        /// </summary>
        private void SetHighChangeSetCnt()
        {
            List<int> changeSetCnt = new List<int>();
            changeSetCnt.Add(this.ScalarPropChangsetCnt);
            changeSetCnt.Add(this.RelationshipChangsetCnt);
            changeSetCnt.Add(this.ResTaggingChangsetCnt);
            changeSetCnt.Add(this.CateAssociationChangsetCnt);
            this.HighestChangeSetCnt = changeSetCnt.Max();
        }

        #endregion

        #region Private

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Return true if tracking changes feature is enable on the 
        /// repository otherwise false.</returns>
        private bool IsChangeHistoryEnabled()
        {
            ZentityContext context = base.CreateContext();
            string configValue = context.GetConfiguration(_changeHistoryEnableField);
            bool enabled;
            bool flag = bool.TryParse(configValue, out enabled);

            if (!flag)
                return false;
            return enabled;
        }

        /// <summary>
        /// Create design time appearance of control.
        /// </summary>
        private void CreateDesignerView()
        {
            this.Controls.Clear();
            Table designViewContainer = CreateTable();
            designViewContainer.BorderWidth = Unit.Pixel(1);
            designViewContainer.BorderStyle = BorderStyle.Solid;
            designViewContainer.BorderColor = System.Drawing.Color.Black;
            designViewContainer.Controls.Add(CreateDesignerHeaderRow(_controlTitle));
            designViewContainer.Controls.Add(CreateDesignTitleRow(_desScalProrTitle));
            designViewContainer.Controls.Add(CreateDesignerRow(Resources.GlobalResource.ChangeHistoryDesignerScalarProperty));
            designViewContainer.Controls.Add(CreateDesignTitleRow(_desRelationTitle));
            designViewContainer.Controls.Add(CreateDesignerRow(Resources.GlobalResource.ChangeHistoryDesignerRelationship));
            designViewContainer.Controls.Add(CreateDesignTitleRow(_desResTaggingTitle));
            designViewContainer.Controls.Add(CreateDesignerRow(Resources.GlobalResource.ChangeHistoryDesignerResTagging));
            designViewContainer.Controls.Add(CreateDesignTitleRow(_desCateAssocTitle));
            designViewContainer.Controls.Add(CreateDesignerRow(Resources.GlobalResource.ChangeHistoryDesignerCateAssociation));
            this.Controls.Add(designViewContainer);
        }

        /// <summary>
        /// Creates a row for design time view of the control.
        /// </summary>
        /// <param name="text">Text to be displayed on row.</param>
        /// <returns>Designer row.</returns>
        private static TableRow CreateDesignerRow(string text)
        {
            TableRow emptyRow = new TableRow();
            emptyRow.Controls.Add(CreateCell(text));
            return emptyRow;
        }

        /// <summary>
        /// Create a header row and apply style to the row.
        /// </summary>
        /// <param name="headerText">Text to be displayed on row.</param>
        /// <returns>Designer header row.</returns>
        private static TableHeaderRow CreateDesignerHeaderRow(string headerText)
        {
            TableHeaderRow headerRow = new TableHeaderRow();
            headerRow.Controls.Add(CreateHeaderCell(headerText));
            headerRow.CssClass = "titleStyle";
            return headerRow;
        }

        /// <summary>
        /// Create child controls of the control.
        /// </summary>
        private void CreateChangeHistoryControls()
        {
            FillScalarPropertyTable();

            // Add empty table row.
            this.Controls.Add(CreateSpace());

            FillRelationshipTable();

            this.Controls.Add(CreateSpace());

            FillResourceTaggingTable();

            this.Controls.Add(CreateSpace());

            FillCategoryAssociationTable();
        }

        /// <summary>
        /// Fills category association change sets.
        /// </summary>
        private void FillCategoryAssociationTable()
        {
            this._changeHistoryCategoryTable = CreateTable();
            this.Controls.Add(this._changeHistoryCategoryTable);
            CreateTitleRow(this._changeHistoryCategoryTable, GlobalResource.CategoryAssociationChangesText,
                                  this.CategoryAssociationChangeSetStyle.HeaderStyle);

            if (this.MaximumChangeSetToFetch <= 0)
            {
                this.PrevCategoryChangsetCnt = this.HighestChangeSetCnt;
            }
            List<RelationshipChange> relChanges = GetCategoryAssociationChangesets().Skip(0).
                                                    Take(this.PrevCategoryChangsetCnt)
                                                    .ToList();

            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges,
                                                    this._changeHistoryCategoryTable,
                                                    this.CategoryAssociationChangeSetStyle,
                                                    _showMoreCateBtnId, _showAllCateBtnId,
                                                        this.PrevCategoryChangsetCnt,
                                                    (int)ViewState[_changesetCateCntViewStateKey]);

            if (relChanges.Count > 0)
            {
                this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Category;
                this.FillRelationChangesets(historyObject);
                TableRow linkContainer = GetLinkRow(this._changeHistoryCategoryTable);
                LinkButton showMoreBtn = linkContainer.Controls[0].Controls[0] as LinkButton;
                LinkButton showAllBtn = linkContainer.Controls[0].Controls[2] as LinkButton;
                showMoreBtn.Click += new EventHandler(showMoreCategoryBtn_Click);
                showAllBtn.Click += new EventHandler(showAllCategoryBtn_Click);
            }
        }

        /// <summary>
        /// Fills resource tagging change sets.
        /// </summary>
        private void FillResourceTaggingTable()
        {
            this._changeHistoryTagTable = CreateTable();
            this.Controls.Add(this._changeHistoryTagTable);
            CreateTitleRow(this._changeHistoryTagTable, GlobalResource.ResourceTaggingChangesText,
                                    this.ResourceTaggingChangeSetStyle.HeaderStyle);
            if (this.MaximumChangeSetToFetch <= 0)
            {
                this.PrevTagChangsetCnt = this.HighestChangeSetCnt;
            }
            List<RelationshipChange> relChanges = GetResTaggingChangesets().Skip(0).
                                                   Take(this.PrevTagChangsetCnt)
                                                   .ToList();

            if (relChanges.Count > 0)
            {

                this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Tag;
                CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges, this._changeHistoryTagTable,
                                   this.ResourceTaggingChangeSetStyle, _showMoreTagBtnId, _showAllTagBtnId, this.PrevTagChangsetCnt,
                                   (int)ViewState[_changesetTagCntViewStateKey]);

                this.FillRelationChangesets(historyObject);
                TableRow linkContainer = GetLinkRow(this._changeHistoryTagTable);
                LinkButton showMoreBtn = linkContainer.Controls[0].Controls[0] as LinkButton;
                LinkButton showAllBtn = linkContainer.Controls[0].Controls[2] as LinkButton;
                showMoreBtn.Click += new EventHandler(showMoreTagBtn_Click);
                showAllBtn.Click += new EventHandler(showAllTagBtn_Click);
            }
        }

        /// <summary>
        /// Fills scalar property change sets.
        /// </summary>
        private void FillScalarPropertyTable()
        {
            this._changeHistoryTable = CreateTable();
            this.Controls.Add(this._changeHistoryTable);
            CreateTitleRow(this._changeHistoryTable, GlobalResource.ScalarPropertyChangeText,
                                this.ScalarPropertyChangeSetStyle.HeaderStyle);
            if (this.MaximumChangeSetToFetch <= 0)
            {
                this.PrevScalarPropChangsetCnt = this.HighestChangeSetCnt;
            }
            this.FillScalarPropChangesets(this.PrevScalarPropChangsetCnt, 0);
        }

        /// <summary>
        /// Create an instance of Table control.
        /// </summary>
        /// <returns>Object of type Table.</returns>
        private static Table CreateTable()
        {
            Table table = new Table();
            table.Width = Unit.Percentage(100);
            table.CellSpacing = 0;
            return table;
        }

        /// <summary>
        /// Fills relationship changes change sets.
        /// </summary>
        private void FillRelationshipTable()
        {
            this._changeHistoryRelationShipTable = CreateTable();
            this.Controls.Add(this._changeHistoryRelationShipTable);
            CreateTitleRow(this._changeHistoryRelationShipTable, GlobalResource.RelationshipChangesText,
                                        this.RelationshipChangeSetStyle.HeaderStyle);
            if (this.MaximumChangeSetToFetch <= 0)
            {
                this.PrevRelChangsetCnt = this.HighestChangeSetCnt;
            }
            List<RelationshipChange> relChanges = GetRelshipChangesets().Skip(0).Take(this.PrevRelChangsetCnt)
                                    .ToList();
            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges, this._changeHistoryRelationShipTable,
                                    this.RelationshipChangeSetStyle, _showMoreRelBtnId, _showAllRelBtnId, this.PrevRelChangsetCnt,
                                    (int)ViewState[_changesetRelCntViewStateKey]);

            this.FillRelationChangesets(historyObject);

            if (relChanges.Count > 0)
            {
                this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Resource;
                TableRow linkContainer = GetLinkRow(this._changeHistoryRelationShipTable);
                LinkButton showMoreBtn = linkContainer.Controls[0].Controls[0] as LinkButton;
                LinkButton showAllBtn = linkContainer.Controls[0].Controls[2] as LinkButton;
                showMoreBtn.Click += new EventHandler(showMoreRelChangesetLink_Click);
                showAllBtn.Click += new EventHandler(showAllRelChangesetLink_Click);
            }
        }

        /// <summary>
        /// Fills details of change sets of type relationship changes, resource tagging etc...
        /// </summary>
        /// <param name="historyObject">Object of type class CommonChangeHistoryObject</param>
        private void FillRelationChangesets(CommonChangeHistoryObject historyObject)
        {
            List<string> securityPredicates = Zentity.Security.AuthorizationHelper.PermissionManager.GetSecurityPredicates().ToList();

            foreach (RelationshipChange changeSet in historyObject.RelShipChanges)
            {
                changeSet.OperationReference.Load();
                changeSet.ChangeSetReference.Load();

                string imageId = Guid.NewGuid().ToString();
                TableRow dataRow = CreateChangesetRow(changeSet.ChangeSet.Id, changeSet.Operation.Name, changeSet.ChangeSet.DateCreated, imageId, this.RelationshipChangeSetStyle);
                historyObject.ControlContainer.Controls.Add(dataRow);

                string panelId = Guid.NewGuid().ToString();
                historyObject.ControlContainer.Controls.Add(CreatePanelRow(panelId, changeSet, historyObject.StyleProperty));

                string panelClientId = historyObject.ControlContainer.FindControl(panelId).ClientID;
                string imageClientId = historyObject.ControlContainer.FindControl(imageId).ClientID;

                dataRow.Attributes.Add(_onClick, string.Format(CultureInfo.InvariantCulture, _viewPanelFunction,
                                            panelClientId, imageClientId, Page.ClientScript.GetWebResourceUrl
                                            (this.GetType(), _smallPlusImagePath),
                                            Page.ClientScript.GetWebResourceUrl(
                                            this.GetType(), _smallMinusImagePath)));
            }

            if (historyObject.RelShipChanges.Count() > 0)
            {
                TableRow linkRow = CreateLinksRow(historyObject.ShowMoreBtnId + new Guid().ToString(), historyObject.ShowAllBtnId, historyObject.StyleProperty.LinkStyle);
                historyObject.ControlContainer.Controls.Add(linkRow);
                LinkButton showMoreRelBtn = linkRow.Controls[0].Controls[0] as LinkButton;
                LinkButton showAllRelBtn = linkRow.Controls[0].Controls[2] as LinkButton;
                if (historyObject.PreFetchedChangesetCnt >= historyObject.ChangeSetCnt)
                {
                    showMoreRelBtn.Enabled = false;
                    showAllRelBtn.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Get a row contains link buttons from specified table control.
        /// </summary>
        /// <param name="table">Container contains link buttons.</param>
        /// <returns>A row contains links.</returns>
        private static TableRow GetLinkRow(Table table)
        {
            int lastRowCnt = table.Controls.Count;
            TableRow linkRow = table.Controls[lastRowCnt - 1] as TableRow;
            return linkRow;
        }

        /// <summary>
        /// Get id of type category node.
        /// </summary>
        /// <returns>Category node type id.</returns>
        private static Guid GetCategoryNodeResType()
        {
            string categoryResType = _cateResTypeId;
            return new Guid(categoryResType);
        }

        /// <summary>
        /// Get id of type tag.
        /// </summary>
        /// <returns>Id of type tag.</returns>
        private static Guid GetTagResType()
        {
            string tagResType = _tagResTypeId;
            return new Guid(tagResType);
        }

        /// <summary>
        /// Get relationship change sets contains specified resource as subject or object. 
        /// </summary>
        /// <returns>Collection of relationship change sets.</returns>
        private List<RelationshipChange> GetRelshipChangesets()
        {
            AdministrationContext context = CreateAdminContext();
            Guid tagResTypeId = GetTagResType();
            Guid categoryResTypeId = GetCategoryNodeResType();
            Guid resId = this.EntityId;
            IEnumerable<RelationshipChange> relChanges = context.RelationshipChanges
                                .Where(tuple => (tuple.NextSubjectResourceId.Value == resId ||
                                    tuple.PreviousSubjectResourceId.Value == resId ||
                                    tuple.NextObjectResourceId.Value == resId ||
                                    tuple.PreviousObjectResourceId.Value == resId) &&
                                    (((tuple.NextPredicateId.Value != tagResTypeId) || (tuple.PreviousPredicateId.Value != tagResTypeId)) &&
                                    ((tuple.NextPredicateId.Value != categoryResTypeId) || (tuple.PreviousPredicateId.Value != categoryResTypeId)))).OrderByDescending(tuple =>
                                            tuple.ChangeSet.DateCreated);

            List<string> securityPredicates = Zentity.Security.AuthorizationHelper.PermissionManager.GetSecurityPredicates().ToList();

            List<RelationshipChange> finalRelChanges = new List<RelationshipChange>();
            foreach (RelationshipChange changeSet in relChanges)
            {
                if (changeSet.NextPredicateId != null)
                {
                    if (IsSecurityAwareControl)
                    {
                        Predicate predicate = null;
                        using (ResourceDataAccess resAccess = new ResourceDataAccess(base.CreateContext()))
                        {
                            predicate = resAccess.GetPredicate(new Guid(changeSet.NextPredicateId.ToString()));
                        }
                        if (securityPredicates.Contains(predicate.Uri))
                        {

                            continue;
                        }
                    }
                }
                else if (changeSet.PreviousPredicateId != null)
                {
                    if (IsSecurityAwareControl)
                    {
                        Predicate predicate = null;
                        using (ResourceDataAccess resAccess = new ResourceDataAccess(base.CreateContext()))
                        {
                            predicate = resAccess.GetPredicate(new Guid(changeSet.PreviousPredicateId.ToString()));
                        }
                        if (securityPredicates.Contains(predicate.Uri))
                        {
                            continue;
                        }
                    }
                }
                finalRelChanges.Add(changeSet);
            }
            return finalRelChanges;
        }
        /// <summary>
        /// Create instance of administration context.
        /// </summary>
        /// <returns>Instance of AdministrationContext.</returns>
        private static AdministrationContext CreateAdminContext()
        {
            AdministrationContext context = new AdministrationContext();
            return context;
        }

        /// <summary>
        /// Get resource tagging change sets contains specified resource. 
        /// </summary>
        /// <returns>Collection of relationship change sets.</returns>
        private IEnumerable<RelationshipChange> GetResTaggingChangesets()
        {
            AdministrationContext context = CreateAdminContext();
            Guid tagResTypeId = GetTagResType();
            Guid resId = this.EntityId;
            IEnumerable<RelationshipChange> relChanges = context.RelationshipChanges
                                .Where(tuple => (tuple.NextSubjectResourceId.Value == resId ||
                                    tuple.PreviousSubjectResourceId.Value == resId ||
                                    tuple.NextObjectResourceId.Value == resId ||
                                    tuple.PreviousObjectResourceId.Value == resId) &&
                                    ((tuple.NextPredicateId.Value == tagResTypeId) || (tuple.PreviousPredicateId.Value == tagResTypeId))).OrderByDescending(tuple =>
                                            tuple.ChangeSet.DateCreated);
            return relChanges;
        }

        /// <summary>
        /// Get category association change sets contains specified resource. 
        /// </summary>
        /// <returns>Collection of relationship change sets.</returns>
        private IEnumerable<RelationshipChange> GetCategoryAssociationChangesets()
        {
            AdministrationContext context = CreateAdminContext();
            Guid categoryResTypeId = GetCategoryNodeResType();
            Guid resId = this.EntityId;
            IEnumerable<RelationshipChange> relChanges = context.RelationshipChanges
                                                        .Where(tuple => (tuple.NextSubjectResourceId.Value == resId ||
                                                          tuple.PreviousSubjectResourceId.Value == resId ||
                                                        tuple.NextObjectResourceId.Value == resId ||
                                                        tuple.PreviousObjectResourceId.Value == resId) &&
                                                        ((tuple.NextPredicateId.Value == categoryResTypeId) || (tuple.PreviousPredicateId.Value == categoryResTypeId))).
                                                        OrderByDescending(tuple => tuple.ChangeSet.DateCreated);
            return relChanges;
        }

        /// <summary>
        /// Get scalar properties change sets contains specified resource. 
        /// </summary>
        /// <returns>Collection of relationship change sets.</returns>
        private IEnumerable<ResourceChange> GetScalarPropertyChangesets()
        {
            AdministrationContext context = CreateAdminContext();
            IEnumerable<ResourceChange> resChanges = context.ResourceChanges
                               .Where(tuple => tuple.ResourceId == this.EntityId)
                               .OrderByDescending(tuple => tuple.ChangeSet.DateCreated);
            return resChanges;
        }

        /// <summary>
        /// Fills sort of details like subject resource, predicate, object resource etc... of a
        /// relationship changes set.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="changeSet"></param>
        /// <param name="style"></param>
        private void FillRelDetail(Table table, RelationshipChange changeSet, CommonProperty style)
        {
            table.Controls.Add(CreateRelTableRow(_subRes, GetResTitle(changeSet.PreviousSubjectResourceId),
                                        GetResTitle(changeSet.NextSubjectResourceId), style));
            table.Controls.Add(CreateRelTableRow(_predicate, GetPredicateName(changeSet.PreviousPredicateId), GetPredicateName(changeSet.NextPredicateId), style));
            table.Controls.Add(CreateRelTableRow(_objRes, GetResTitle(changeSet.PreviousObjectResourceId), GetResTitle(changeSet.NextObjectResourceId), style));
            table.Controls.Add(CreateRelTableRow(_ordinalPos, changeSet.PreviousOrdinalPosition.HasValue ? changeSet.PreviousOrdinalPosition.Value.ToString(CultureInfo.InvariantCulture) :
                                 string.Empty, changeSet.NextOrdinalPosition.HasValue ?
                                 changeSet.NextOrdinalPosition.Value.ToString(CultureInfo.InvariantCulture) : string.Empty, style));
        }

        /// <summary>
        /// Get predicate name for specified predicate id. 
        /// </summary>
        /// <param name="predicateId">Id of predicate.</param>
        /// <returns>Name of predicate.</returns>
        private string GetPredicateName(Guid? predicateId)
        {
            string predicateUri = string.Empty;
            if (predicateId.HasValue)
            {
                using (ZentityContext context = base.CreateContext())
                {
                    Predicate predicate = context.Predicates
                                .Where(tuple => tuple.Id == predicateId.Value).FirstOrDefault();

                    if (predicate != null)
                    {
                        predicateUri = predicate.Name;
                    }
                }
            }
            return predicateUri;
        }

        /// <summary>
        /// Create a row having values for a relation attribute like subject resource, object resource etc...
        /// </summary>
        /// <param name="relAttribute">Represent relationship attribute.</param>
        /// <param name="preValue">Previous value for attribute.</param>
        /// <param name="nextValue">new value for attribute.</param>
        /// <param name="style">Style to be applied to row.</param>
        /// <returns>Instance of table row.</returns>
        private TableRow CreateRelTableRow(string relAttribute, string preValue, string nextValue, CommonProperty style)
        {
            TableRow row = new TableRow();
            row.Controls.Add(CreateCell(relAttribute));
            row.Controls.Add(CreateCell(preValue));
            if (string.IsNullOrEmpty(this.NextObjResId))
            {
                row.Controls.Add(CreateCell(nextValue));
            }
            else
            {
                row.Controls.Add(CreateHyperLinkCell(nextValue));
            }

            if (this._isAlternateRowRel)
            {
                ApplyRowStyle(row, style.InnerTableAlternateRowStyle);
                this._isAlternateRowRel = false;
            }
            else
            {
                ApplyRowStyle(row, style.InnerTableRowStyle);
                this._isAlternateRowRel = true;
            }
            return row;
        }

        /// <summary>
        /// Get title of specified resource id.
        /// </summary>
        /// <param name="id">Id of resource.</param>
        /// <returns>Title of resource.</returns>
        private string GetResTitle(Guid? id)
        {
            string title = string.Empty;
            this.NextObjResId = String.Empty;
            if (id.HasValue)
            {
                //this.NextObjResId = id.ToString();
                using (ResourceDataAccess resAccess = new ResourceDataAccess(base.CreateContext()))
                {
                    try
                    {
                        Resource res = resAccess.GetResource(id.Value);

                        if (IsSecurityAwareControl)
                        {
                            bool isPermission = resAccess.AuthorizeResource<Resource>(AuthenticatedToken, UserResourcePermissions.Read, res, false);
                            if (isPermission)
                            {
                                this.NextObjResId = id.ToString();
                            }
                        }
                        else
                            this.NextObjResId = id.ToString();
                        title = res.Title;
                    }
                    catch (EntityNotFoundException)
                    {
                        title = Resources.GlobalResource.ChangeHistoryEntityNotFound;
                    }
                }
            }
            return title;
        }

        /// <summary>
        /// Encode specified string.
        /// </summary>
        /// <param name="htmlString">Input string.</param>
        /// <returns>Encoded string.</returns>
        private static string EncodeHtmlTag(string htmlString)
        {
            return HttpUtility.HtmlEncode(htmlString);
        }

        /// <summary>
        /// Create hyperlink cell.
        /// </summary>
        /// <param name="nextValue">text to be displayed as hyperlink.</param>
        /// <returns>Table cell contains hyperlink.</returns>
        private TableCell CreateHyperLinkCell(string nextValue)
        {
            TableCell cell = new TableCell();
            HyperLink hyperLink = new HyperLink();
            if (string.IsNullOrEmpty(nextValue))
            {
                nextValue = Resources.GlobalResource.ResourceEmptyTitle;
            }
            hyperLink.Text = EncodeHtmlTag(nextValue);

            if (this.ChangeHistoryEntityTypeName == ChangeHistoryEntityType.Resource)
            {
                hyperLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, this.NavigateUrlForResource,
                                                            this.NextObjResId);
            }
            else if (this.ChangeHistoryEntityTypeName == ChangeHistoryEntityType.Tag)
            {
                hyperLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, this.NavigateUrlForTag,
                                                        this.NextObjResId);
            }
            else
            {
                hyperLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, this.NavigateUrlForCategory,
                                                                    this.NextObjResId);
            }
            cell.Controls.Add(hyperLink);
            this.NextObjResId = string.Empty;
            return cell;
        }

        /// <summary>
        /// Initialize scalar property change sets count.
        /// </summary>
        private void InitializeScalarPropChangesetCount()
        {
            this.ScalarPropChangsetCnt = GetScalarPropertyChangesets().Count();
        }

        /// <summary>
        /// Initialize relationship change sets count.
        /// </summary>
        private void InitializeRelChangesetCount()
        {
            this.RelationshipChangsetCnt = GetRelshipChangesets().Count();
        }

        /// <summary>
        /// Initialize category associations change sets count.
        /// </summary>
        private void InitializeCateAssociateChangesetCount()
        {
            this.CateAssociationChangsetCnt = GetCategoryAssociationChangesets().Count();
        }

        /// <summary>
        /// Initialize resource tagging change sets count.
        /// </summary>
        private void InitializeResTaggingChangesetCount()
        {
            this.ResTaggingChangsetCnt = GetResTaggingChangesets().Count();
        }

        /// <summary>
        /// Fill scalar properties change sets. 
        /// </summary>
        /// <param name="numberOfChangesetFetched">Represents number of change sets to be fetched from repository.</param>
        /// <param name="skipPreviousEntry">Represents number of change sets will be skipped.</param>
        private void FillScalarPropChangesets(int numberOfChangesetFetched, int skipPreviousEntry)
        {
            List<ResourceChange> resChanges = GetScalarPropertyChangesets().Skip(skipPreviousEntry).
                                               Take(numberOfChangesetFetched).ToList();
            int actualScalarChangesCount = 0;
            foreach (ResourceChange changeSet in resChanges)
            {
                if (IsScalarChangesPresent(changeSet))
                {
                    changeSet.OperationReference.Load();
                    changeSet.ChangeSetReference.Load();

                    string imageId = Guid.NewGuid().ToString();
                    TableRow dataRow = CreateChangesetRow(changeSet.ChangeSet.Id, changeSet.Operation.Name,
                                                    changeSet.ChangeSet.DateCreated, imageId,
                                                    this.ScalarPropertyChangeSetStyle);
                    this._changeHistoryTable.Controls.Add(dataRow);

                    string panelId = Guid.NewGuid().ToString();
                    this._changeHistoryTable.Controls.Add(CreatePanelRow(panelId, changeSet,
                                                            this.ScalarPropertyChangeSetStyle));

                    string panelClientId = this._changeHistoryTable.FindControl(panelId).ClientID;
                    string imageClientId = this._changeHistoryTable.FindControl(imageId).ClientID;

                    dataRow.Attributes.Add(_onClick, string.Format(CultureInfo.InvariantCulture, _viewPanelFunction,
                                               panelClientId, imageClientId, Page.ClientScript.GetWebResourceUrl
                                               (this.GetType(), _smallPlusImagePath),
                                               Page.ClientScript.GetWebResourceUrl(
                                               this.GetType(), _smallMinusImagePath)));
                    actualScalarChangesCount++;
                }
            }
            if (actualScalarChangesCount > 0)
            {
                TableRow linkRow = CreateLinksRow(_showMoreBtnId, _showAllBtnId,
                                                    this.ScalarPropertyChangeSetStyle.LinkStyle);
                this._changeHistoryTable.Controls.Add(linkRow);
                LinkButton showMoreBtn = linkRow.Controls[0].Controls[0] as LinkButton;
                LinkButton showAllBtn = linkRow.Controls[0].Controls[2] as LinkButton;
                if (this.PrevScalarPropChangsetCnt >= (int)ViewState[_changesetCntViewStateKey])
                {
                    showMoreBtn.Enabled = false;
                    showAllBtn.Enabled = false;
                }
                showMoreBtn.Click += new EventHandler(showMoreChangesetLink_Click);
                showAllBtn.Click += new EventHandler(showAllChangesetLink_Click);
            }
        }

        /// <summary>
        /// Create row contains link buttons
        /// </summary>
        /// <param name="moreLinkId">Id of "ShowMore" link button</param>
        /// <param name="allLinkId">Id of "ShowAll" link button</param>
        /// <param name="linkStyle">style to be applied on the link buttons.</param>
        /// <returns></returns>
        private static TableRow CreateLinksRow(string moreLinkId, string allLinkId, Style linkStyle)
        {
            TableRow linkRow = new TableRow();
            TableCell linkCell = new TableCell();

            LinkButton showMoreLinkBtn = new LinkButton();
            showMoreLinkBtn.ID = moreLinkId;
            showMoreLinkBtn.MergeStyle(linkStyle);
            showMoreLinkBtn.Text = _showMoreLinkText;
            linkCell.Controls.Add(showMoreLinkBtn);
            linkCell.Controls.Add(CreateSpace());

            LinkButton showAllBLinkBtn = new LinkButton();
            showAllBLinkBtn.Text = _showAllLinkText;
            showAllBLinkBtn.MergeStyle(linkStyle);
            showAllBLinkBtn.ID = allLinkId;
            linkCell.Controls.Add(showAllBLinkBtn);
            linkCell.Controls.Add(CreateSpace());
            linkCell.ColumnSpan = 4;
            linkRow.Controls.Add(linkCell);
            linkRow.HorizontalAlign = HorizontalAlign.Right;

            return linkRow;
        }

        /// <summary>
        /// Remove row that contains link button from specified table.
        /// </summary>
        /// <param name="table">Table contains link buttons.</param>
        private static void RemoveLinkRow(Table table)
        {
            int lastRowCnt = table.Controls.Count;
            Control control = table.Controls[lastRowCnt - 1];
            table.Controls.Remove(control);
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
        /// Creates header cell with text and returns TableHeaderCell.
        /// </summary>
        /// <param name="text">text as string</param>
        /// <returns>returns TableHeaderCell</returns>
        private static TableCell CreateCell(string text)
        {
            TableCell cell = new TableCell();
            cell.Text = text;
            cell.Width = Unit.Percentage(33);
            return cell;
        }

        /// <summary>
        /// Creates header cell with text and returns TableHeaderCell.
        /// </summary>
        /// <param name="text">text as string</param>
        /// <returns>returns TableHeaderCell</returns>
        private static TableCell CreateDesignerCell(string text)
        {
            TableCell cell = new TableCell();
            cell.Text = text;
            cell.ColumnSpan = 4;
            cell.HorizontalAlign = HorizontalAlign.Left;
            // Header style in ToolKit.css.
            return cell;
        }
        /// <summary>
        /// Create row that shows change set details like id,operation name, date when change set created etc...
        /// </summary>
        /// <param name="changsetId">Id of change set.</param>
        /// <param name="operationName">Operation name.</param>
        /// <param name="dateCreated">Represents date of change set creation.</param>
        /// <param name="imageId">Id of Image object</param>
        /// <param name="sytle">Style to be applied to row.</param>
        /// <returns>Table row contains change set's information.</returns>
        private TableRow CreateChangesetRow(string changsetId, string operationName,
                                            DateTime dateCreated, string imageId, CommonProperty sytle)
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            System.Web.UI.WebControls.Image image = new System.Web.UI.WebControls.Image();
            image.ID = imageId;
            image.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _smallPlusImagePath);
            cell.Controls.Add(image);
            row.Controls.Add(cell);
            row.Controls.Add(CreateCell(_changeset + changsetId));
            row.Controls.Add(CreateCell(_dateCreated + dateCreated.ToString()));
            row.Controls.Add(CreateCell(_operation + operationName));
            ApplyRowStyle(row, sytle.RowStyle);
            return row;
        }

        /// <summary>
        /// Creates header cell with text and returns TableHeaderCell.
        /// </summary>
        /// <param name="text">text as string</param>
        /// <returns>returns TableHeaderCell</returns>
        private static TableHeaderCell CreateHeaderCell(string text)
        {
            TableHeaderCell cell = new TableHeaderCell();
            cell.Text = text;
            return cell;
        }

        /// <summary>
        /// Create a row have panel that contains change sets details. 
        /// </summary>
        /// <param name="panelId">Id of panel.</param>
        /// <param name="changeset">Change set to be displayed.</param>
        /// <param name="style">Style to be applied to row.</param>
        /// <returns>Table row.</returns>
        private TableRow CreatePanelRow(string panelId, EntityObject changeset, CommonProperty style)
        {
            Panel panel1 = new Panel();
            Table panelTable = CreateTable();
            if (changeset.GetType() == typeof(ResourceChange))
            {
                panelTable.Controls.Add(CreatePanelHeaderRow(_propertyName, _preValue,
                                                _nextValue, style.InnerTableHeaderStyle));

                FillScalarProperties(panelTable, (ResourceChange)changeset);
            }
            else
            {
                panelTable.Controls.Add(CreatePanelHeaderRow(_relAttribute, _preValue,
                                                         _nextValue, style.InnerTableHeaderStyle));
                FillRelDetail(panelTable, (RelationshipChange)changeset, style);
            }
            TableRow panelRow = new TableRow();
            TableCell cell = new TableCell();
            cell.ColumnSpan = 4;
            panel1.ID = panelId;
            panel1.Style.Add("display", GlobalResource.CssPanelDisplayNone);
            panel1.Controls.Add(panelTable);
            cell.Controls.Add(panel1);
            panelRow.Controls.Add(cell);
            return panelRow;
        }

        /// <summary>
        /// Fills specified table with scalar properties.
        /// </summary>
        /// <param name="panelTable">Table to be filled with scalar properties.</param>
        /// <param name="changeSet">Change Set whose details will be displayed.</param>
        private void FillScalarProperties(Table panelTable, ResourceChange changeSet)
        {
            XElement changeXml = XElement.Parse(changeSet.PropertyChanges);

            // Filtered out changed properties.
            List<XElement> propertyChanges = changeXml.Elements()
                .Where(element =>
                    element.Attributes().Where(tuple => tuple.Name.LocalName.Equals(_changedFlag, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(tuple.Value) &&
                    tuple.Value.Equals("true", StringComparison.OrdinalIgnoreCase)).Count() > 0
                ).ToList();

            this._isAlternateRow = false;

            foreach (XElement property in propertyChanges)
            {
                panelTable.Controls.Add(CreateScalarProperptyTableRow(property));
            }

        }

        private static bool IsScalarChangesPresent(ResourceChange changeSet)
        {
            XElement changeXml = XElement.Parse(changeSet.PropertyChanges);

            // find count of changed properties.
            int propertyChangesCount = changeXml.Elements()
                .Where(element =>
                    element.Attributes().Where(tuple => tuple.Name.LocalName.Equals(_changedFlag, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(tuple.Value)
                    && tuple.Value.Equals("true", StringComparison.OrdinalIgnoreCase)).Count() > 0
                ).Count();

            return propertyChangesCount > 0;
        }

        /// <summary>
        /// Create a header row with specified columns.
        /// </summary>
        /// <param name="firstCol">First column name.</param>
        /// <param name="secondCol">Second column name.</param>
        /// <param name="thirdCol">Third column name.</param>
        /// <param name="headerTitleStyle">Style to be applied to header row.</param>
        /// <returns></returns>
        private static TableHeaderRow CreatePanelHeaderRow(string firstCol, string secondCol,
                                                        string thirdCol, TableItemStyle headerTitleStyle)
        {
            TableHeaderRow headerRow = new TableHeaderRow();
            headerRow.Controls.Add(CreateHeaderCell(firstCol));
            headerRow.Controls.Add(CreateHeaderCell(secondCol));
            headerRow.Controls.Add(CreateHeaderCell(thirdCol));
            ApplyRowStyle(headerRow, headerTitleStyle);
            return headerRow;
        }

        /// <summary>
        /// Create a table row which shows value of specified scalar property.
        /// </summary>
        /// <param name="property">Represents scalar property.</param>
        /// <returns>Table row contains property's value.</returns>
        private TableRow CreateScalarProperptyTableRow(XElement property)
        {
            TableRow propertyRow = new TableRow();
            XElement nextValueElement = property.Elements(_nextValue).FirstOrDefault();
            XElement preValueElement = property.Elements(_preValue).FirstOrDefault();
            string nextValue = string.Empty;
            string prevValue = string.Empty;
            string propertyName = property.FirstAttribute.Value;
            if (nextValueElement != null)
            {
                if (propertyName == GlobalResource.ImageScalarProperpty)
                    nextValue = GlobalResource.BinaryDataText;
                else
                    nextValue = nextValueElement.Value;
            }
            if (preValueElement != null)
            {
                if (propertyName == GlobalResource.ImageScalarProperpty)
                    prevValue = GlobalResource.BinaryDataText;
                else
                    prevValue = preValueElement.Value;
            }
            if (propertyName == _scalarPropResTypeId)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    if (!string.IsNullOrEmpty(prevValue))
                    {
                        prevValue = dataAccess.GetResourceType(new Guid(prevValue));
                    }
                    if (!string.IsNullOrEmpty(nextValue))
                    {
                        nextValue = dataAccess.GetResourceType(new Guid(nextValue));
                    }
                }
            }
            propertyRow.Controls.Add(CreateCell(EncodeHtmlTag(propertyName)));
            propertyRow.Controls.Add(CreateCell(EncodeHtmlTag(prevValue)));
            propertyRow.Controls.Add(CreateCell(EncodeHtmlTag(nextValue)));
            if (this._isAlternateRow)
            {
                ApplyRowStyle(propertyRow, this.ScalarPropertyChangeSetStyle.InnerTableAlternateRowStyle);
                this._isAlternateRow = false;
            }
            else
            {
                ApplyRowStyle(propertyRow, this.ScalarPropertyChangeSetStyle.InnerTableRowStyle);
                this._isAlternateRow = true;
            }
            return propertyRow;
        }

        /// <summary>
        /// Create a title row and adds it to specified table. 
        /// </summary>
        /// <param name="table">Represents table which will contain title row.</param>
        /// <param name="rowTitle">Represents title of row.</param>
        /// <param name="headerStyle">Header style will be applied to row.</param>
        private static void CreateTitleRow(Table table, string rowTitle, TableItemStyle headerStyle)
        {
            TableHeaderRow tableHeaderRow = new TableHeaderRow();
            TableHeaderCell headerCell = CreateHeaderCell(rowTitle);
            headerCell.ColumnSpan = 4;
            tableHeaderRow.Controls.Add(headerCell);
            ApplyRowStyle(tableHeaderRow, headerStyle);
            table.CellSpacing = 0;
            table.Controls.Add(tableHeaderRow);
        }

        /// <summary>
        /// Create Title row for design time view of the control.
        /// </summary>
        /// <param name="rowTitle">Title of row.</param>
        /// <returns>Designer title row.</returns>
        private static TableRow CreateDesignTitleRow(string rowTitle)
        {
            TableRow tableRow = new TableRow();
            tableRow.Controls.Add(CreateDesignerCell(rowTitle));
            return tableRow;
        }

        /// <summary>
        /// Create space and add it to Literal control.
        /// </summary>
        private static Literal CreateSpace()
        {
            Literal litSpace = new Literal();
            litSpace.Text = _space;
            return litSpace;
        }

        #endregion Private

        #endregion

        #region Event Handlers

        #region Private

        /// <summary>
        /// Event handler for "ShowAll" button on category association section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showAllCategoryBtn_Click(object sender, EventArgs e)
        {
            RemoveLinkRow(this._changeHistoryCategoryTable);
            int recordsSkipped = this.PrevCategoryChangsetCnt;
            int diffCount = this.CateAssociationChangsetCnt - this.PrevCategoryChangsetCnt;
            this.PrevCategoryChangsetCnt = this.CateAssociationChangsetCnt;

            List<RelationshipChange> relChanges = GetCategoryAssociationChangesets().Skip(recordsSkipped).
                                                   Take(diffCount).ToList();
            this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Category;
            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges,
                                                        this._changeHistoryCategoryTable,
                                                        this.CategoryAssociationChangeSetStyle,
                                                        _showMoreCateBtnId, _showAllCateBtnId,
                                                        this.PrevCategoryChangsetCnt,
                                                        (int)ViewState[_changesetCateCntViewStateKey]);
            this.FillRelationChangesets(historyObject);
        }

        /// <summary>
        /// Event handler for "ShowMore" button on category association section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showMoreCategoryBtn_Click(object sender, EventArgs e)
        {
            RemoveLinkRow(this._changeHistoryCategoryTable);
            int recordsSkipped = this.PrevCategoryChangsetCnt;
            this.PrevCategoryChangsetCnt = this.PrevCategoryChangsetCnt + this.MaximumChangeSetToFetch;
            List<RelationshipChange> relChanges = GetCategoryAssociationChangesets().Skip(recordsSkipped).
                                                    Take(this.MaximumChangeSetToFetch).ToList();
            this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Category;
            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges,
                                                            this._changeHistoryCategoryTable,
                                                            this.CategoryAssociationChangeSetStyle,
                                                            _showMoreCateBtnId, _showAllCateBtnId,
                                                            this.PrevCategoryChangsetCnt,
                                                       (int)ViewState[_changesetCateCntViewStateKey]);
            this.FillRelationChangesets(historyObject);
        }

        /// <summary>
        /// Event handler for "ShowAll" button on resource tagging section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showAllTagBtn_Click(object sender, EventArgs e)
        {

            RemoveLinkRow(this._changeHistoryTagTable);
            int recordsSkipped = this.PrevTagChangsetCnt;
            int diffCount = this.ResTaggingChangsetCnt - this.PrevTagChangsetCnt;
            this.PrevTagChangsetCnt = this.ResTaggingChangsetCnt;

            List<RelationshipChange> relChanges = GetResTaggingChangesets().Skip(recordsSkipped)
                                                  .Take(diffCount).ToList();
            this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Tag;
            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges, this._changeHistoryTagTable,
                                                        this.ResourceTaggingChangeSetStyle, _showMoreTagBtnId,
                                                        _showAllTagBtnId, this.PrevTagChangsetCnt,
                                                        (int)ViewState[_changesetTagCntViewStateKey]);
            this.FillRelationChangesets(historyObject);
        }

        /// <summary>
        /// Event handler for "ShowMore" button on resource tagging section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showMoreTagBtn_Click(object sender, EventArgs e)
        {
            RemoveLinkRow(this._changeHistoryTagTable);
            int recordsSkipped = this.PrevTagChangsetCnt;
            this.PrevTagChangsetCnt = this.PrevTagChangsetCnt + this.MaximumChangeSetToFetch;


            List<RelationshipChange> relChanges = GetResTaggingChangesets().Skip(recordsSkipped).
                                                  Take(this.MaximumChangeSetToFetch).ToList();
            this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Tag;
            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges,
                                                    this._changeHistoryTagTable, this.ResourceTaggingChangeSetStyle,
                                                    _showMoreTagBtnId, _showAllTagBtnId, this.PrevTagChangsetCnt,
                                                    (int)ViewState[_changesetTagCntViewStateKey]);
            this.FillRelationChangesets(historyObject);
        }

        /// <summary>
        /// Event handler for "ShowAll" button on scalar property changes section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showAllChangesetLink_Click(object sender, EventArgs e)
        {
            RemoveLinkRow(this._changeHistoryTable);
            int recordsSkipped = this.PrevScalarPropChangsetCnt;
            int diffCount = this.ScalarPropChangsetCnt - this.PrevScalarPropChangsetCnt;
            this.PrevScalarPropChangsetCnt = this.ScalarPropChangsetCnt;
            FillScalarPropChangesets(diffCount, recordsSkipped);
        }

        /// <summary>
        /// Event handler for "ShowMore" button on scalar property changes section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showMoreChangesetLink_Click(object sender, EventArgs e)
        {
            RemoveLinkRow(this._changeHistoryTable);
            int recordsSkipped = this.PrevScalarPropChangsetCnt;
            this.PrevScalarPropChangsetCnt = this.PrevScalarPropChangsetCnt +
                                                this.MaximumChangeSetToFetch;
            FillScalarPropChangesets(this.MaximumChangeSetToFetch, recordsSkipped);
        }

        /// <summary>
        /// Event handler for "ShowAll" button on relationship changes section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showAllRelChangesetLink_Click(object sender, EventArgs e)
        {
            RemoveLinkRow(this._changeHistoryRelationShipTable);
            int recordsSkipped = this.PrevRelChangsetCnt;
            int diffCount = this.RelationshipChangsetCnt - this.PrevRelChangsetCnt;
            this.PrevRelChangsetCnt = this.RelationshipChangsetCnt;
            List<RelationshipChange> relChanges = GetRelshipChangesets().Skip(recordsSkipped).Take(diffCount)
                                   .ToList();

            this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Resource;
            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges,
                                                        this._changeHistoryRelationShipTable, this.RelationshipChangeSetStyle,
                                                        _showMoreRelBtnId, _showAllRelBtnId,
                                                        this.PrevRelChangsetCnt, (int)ViewState[_changesetRelCntViewStateKey]);
            this.FillRelationChangesets(historyObject);
        }

        /// <summary>
        /// Event handler for "ShowMore" button on relationship changes section.
        /// </summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        void showMoreRelChangesetLink_Click(object sender, EventArgs e)
        {
            RemoveLinkRow(this._changeHistoryRelationShipTable);
            int recordsSkipped = this.PrevRelChangsetCnt;
            this.PrevRelChangsetCnt = this.PrevRelChangsetCnt + this.MaximumChangeSetToFetch;

            List<RelationshipChange> relChanges = GetRelshipChangesets().Skip(recordsSkipped).
                                        Take(this.MaximumChangeSetToFetch).ToList();
            this.ChangeHistoryEntityTypeName = ChangeHistoryEntityType.Resource;
            CommonChangeHistoryObject historyObject = new CommonChangeHistoryObject(relChanges, this._changeHistoryRelationShipTable,
                                    this.RelationshipChangeSetStyle, _showMoreRelBtnId, _showAllRelBtnId, this.PrevRelChangsetCnt,
                                    (int)ViewState[_changesetRelCntViewStateKey]);
            this.FillRelationChangesets(historyObject);
        }

        #endregion Private

        #endregion
    }

    /// <summary>
    /// Represents entity type for changes history control.
    /// </summary>
    internal enum ChangeHistoryEntityType
    {
        Resource,
        Tag,
        Category
    }
}