// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class inherits Association class.
    /// It allows association of entities with the subject entity 
    /// using various predicates.
    /// </summary>
    /// <example>
    ///     The code below is the source for PredicateAssociation.aspx. It shows an example of using 
    ///     <see cref="PredicateAssociation"/> control.
    ///     It can be configured to provide Resource To Resource Association, Tag To Tag Association 
    ///     and Category to Category Association.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///         TagPrefix="Zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;    
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head runat="server"&gt;
    ///             &lt;title&gt;PredicateAssociation Sample&lt;/title&gt;
    ///             
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_Load(object sender, EventArgs e)
    ///                 {
    ///                     string id = Convert.ToString(Request.QueryString["Id"]);
    ///                     if (string.IsNullOrEmpty(id))
    ///                     {
    ///                         StatusLabel.Text = "Please pass an Id parameter (GUID) in the query string. Example: " +
    ///                             Request.Url.AbsolutePath + "?Id=6bd35f74-5a07-4df8-98a0-7ddb71f88c24";
    ///                         PredicateAssociation1.Visible = false;
    ///                         SaveButton.Visible = false;
    ///                    }
    ///                     else
    ///                     {
    ///                         StatusLabel.Text = string.Empty;
    ///                         Guid entityId = new Guid(id);
    ///                         PredicateAssociation1.SubjectItemId = entityId;
    ///                     }
    ///                 }
    ///                 
    ///                 protected void Save_Click(object sender, EventArgs e)
    ///                 {
    ///                     PredicateAssociation1.SaveAssociation();
    ///                 }
    ///             &lt;/script&gt;
    ///             
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:PredicateAssociation ID="PredicateAssociation1" runat="server"
    ///                     AssociationListHeight="120px" IsSecurityAwareControl="false"&gt;
    ///                     &lt;MoveRightButton Text="&gt;&gt;" /&gt;
    ///                     &lt;MoveLeftButton Text="&lt;&lt;" /&gt;
    ///                     &lt;MoveUpButton Text="Up" /&gt;
    ///                     &lt;MoveDownButton Text="Down" /&gt;
    ///                 &lt;/Zentity:PredicateAssociation&gt;
    ///                 &lt;br /&gt;
    ///                 &lt;asp:Button ID="SaveButton" runat="server" Text="Save" OnClick="Save_Click" /&gt;
    ///                 &lt;asp:Label ID="StatusLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [ToolboxData("<{0}:PredicateAssociation runat=\"server\">"
        + "<MoveRightButton Text =\">>\"/>"
        + "<MoveLeftButton Text =\"<<\"/>"
        + "<MoveUpButton Text =\"Up\"/>"
        + "<MoveDownButton Text =\"Down\"/>"
        + "</{0}:PredicateAssociation>")]
    public class PredicateAssociation : Association
    {
        #region Constants

        #region Private

        const string _resourceObjectTypeDataTextField = "Title";
        const string _validationGroupViewStateKey = "ValidationGroup";
        const string _resourceTypeViewStateKey = "ResourceType";
        const string _selectedPredicateViewStateKey = "SelectedPredicateViewStateKey";
        const string _predicateDataTextField = "Name";
        const string _predicateDataValueField = "Id";
        const string _predicateTags = "Tags";

        const string _predicateLabelId = "PredicateLabel";
        const string _predicateDropDownId = "PredicateDropDown";
        const string _objectLabelId = "ObjectLabel";
        const string _ObjectDropDownId = "ObjectDropDown";
        const int _filterControlsPosition = 1;

        #endregion

        #endregion

        #region Member variables

        #region Private

        Label _predicateLabel;
        DropDownList _predicateDropDownList;
        Label _objectTypeLabel;
        DropDownList _objectTypeDropDownList;
        string typeName;

        #endregion Private

        #endregion

        #region Enums

        #region Public


        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets validation group on which child validators will fire.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionPredicateAssociationValidationGroup")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ValidationGroup
        {
            get
            {
                return (ViewState[_validationGroupViewStateKey] != null) ?
                    (string)ViewState[_validationGroupViewStateKey] :
                    string.Empty;
            }
            set
            {
                ViewState[_validationGroupViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets validation group on which child validators will fire.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionPredicateAssociationValidationGroup")]
        [DefaultValue("")]
        [Localizable(true)]
        public string SelectedPredicateId
        {
            get
            {
                return (ViewState[_selectedPredicateViewStateKey] != null) ?
                    (string)ViewState[_selectedPredicateViewStateKey] :
                    string.Empty;
            }
            set
            {
                ViewState[_selectedPredicateViewStateKey] = value;
            }
        }


        #endregion

        #endregion Properties

        #region Events

        /// <summary>
        /// Checks subject entity and loads the control, raises the Load event and 
        /// updates the button status.
        /// </summary>
        protected override void CreateChildControls()
        {
            object entity = null;
            bool result = false;

            if (!this.DesignMode)
            {
                result = CheckPrimaryInputs(out entity);
            }
            else
            {
                result = true;
            }

            if (result)
            {
                RenderControls(entity);
            }
            else
            {
                RenderControlsInErrorMode();
            }
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Saves associations for the subject entity.
        /// </summary>
        /// <returns>A Boolean value indicating success of operation.</returns>
        public override bool SaveAssociation()
        {
            bool result = false;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                AuthorizeResourcesBeforeSave<Resource>(dataAccess);

                NavigationProperty property = dataAccess.GetNavigationProperty(this.Page.Cache
                    , typeName, _predicateDropDownList.SelectedItem.Text);

                if (property != null &&
                    dataAccess.ValidateAssociation<Resource>(SubjectItemId, property,
                        DestinationList as List<Resource>))
                {
                    result = dataAccess.SaveResourceToResourceAssociation<Resource>
                        (SubjectItemId, property, DestinationList as List<Resource>,
                        AuthenticatedToken, Constants.PermissionRequiredForAssociation);
                }
            }

            return result;
        }

        #endregion

        #region Protected


        /// <summary>
        /// Returns list of items to be used as a source list.
        /// </summary>        
        /// <returns>List of source items.</returns>
        protected override IList GetSourceItems()
        {
            SourceCheckBoxList.Items.Clear();
            IList sourceItems = null;

            sourceItems = GetSourceResources(_objectTypeDropDownList.SelectedValue);

            return sourceItems;
        }

        /// <summary>
        /// Returns list of items to be used as a destination list.
        /// </summary>        
        /// <returns>List of destination items.</returns>
        protected override IList GetDestinationItems()
        {
            IList resources = null;
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                NavigationProperty property = dataAccess.GetNavigationProperty(this.Page.Cache,
                     typeName, _predicateDropDownList.SelectedItem.Text);

                if (IsSecurityAwareControl)
                {
                    if (AuthenticatedToken != null)
                    {
                        resources = dataAccess.GetRelatedResources(SubjectItemId, property, AuthenticatedToken, Constants.PermissionRequiredForAssociation);
                    }
                }
                else
                {
                    resources = dataAccess.GetRelatedResources(SubjectItemId, property, null, Constants.PermissionRequiredForAssociation);
                }
            }
            return resources;
        }

        #endregion

        #region Private

        private void RenderControls(object entity)
        {
            base.CreateChildControls();
            FilterCriteriaGrid.ValidationGroup = this.ValidationGroup;
            FilterButton.ValidationGroup = this.ValidationGroup;
            AddPredicateAndObjectRows();
            Panel _containerPanel = new Panel();
            _containerPanel.DefaultButton = FilterButton.ID;
            _containerPanel.Controls.Add(ContainerTable);
            this.Controls.Add(_containerPanel);
            
            if (this.DesignMode)
            {
                _subjectLabel.Text = GlobalResource.AssociationSubjectLabel;
                _objectTypeLabel.Text = GlobalResource.AssociationObjectTypeLabel;
            }
            else
            {
                SetSubjectControlValues(entity);
                SetObjectControlValues();
            }
        }

        private void RenderControlsInErrorMode()
        {
            Table table = new Table();
            table.Width = Unit.Percentage(100);
            table.Rows.Add(CreateTitleRow());
            this.Controls.AddAt(0, table);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool CheckPrimaryInputs(out object entity)
        {
            bool result = false;
            string errorMessage = string.Empty;

            entity = GetSubjectEntity();
            if (entity == null)
            {
                errorMessage = Resources.GlobalResource.
                    PredicateAssociationSubjectEntityErrorMessage;
                result = false;
            }
            else
            {
                result = true;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorLabel.Text = errorMessage;
                ErrorLabel.ForeColor = System.Drawing.Color.Red;
            }
            return result;
        }

        /// <summary>
        /// Gets the subject entity.
        /// </summary>
        /// <returns> type of object </returns>
        private object GetSubjectEntity()
        {
            Resource entity = null;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                entity = dataAccess.GetResource(SubjectItemId);
                dataAccess.AuthorizeResource<Resource>(AuthenticatedToken, UserResourcePermissions.Update, entity, true);
                typeName = ((Resource)entity).GetType().Name;
            }

            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddPredicateAndObjectRows()
        {
            if (FilterTable != null)
            {
                if (this.DesignMode)
                {
                    CreatePredicateAndObjectControlsInDesignMode();
                }
                else
                {
                    CreatePredicateAndObjectControls();
                }
                TableRow[] rows = CreateFilterControlsLayout();

                int i = _filterControlsPosition;
                foreach (TableRow row in rows)
                {
                    FilterTable.Rows.AddAt(i, row);
                    i++;
                }
            }
        }

        /// <summary>
        /// Sets the value of all subject related controls.
        /// </summary>
        private void SetSubjectControlValues(object entity)
        {
            _subjectLabel.Text = GlobalResource.AssociationSubjectLabel;
            if (entity != null)
            {
                string title = CoreHelper.UpdateEmptyTitle(CoreHelper.GetTitleByResourceType((Resource)entity));
                SubjectDisplayLink.Text = System.Web.HttpUtility.HtmlEncode(CoreHelper.FitString(title, 40));
            }
        }

        /// <summary>
        /// Sets the value of all object related controls.
        /// </summary>
        private void SetObjectControlValues()
        {
            _objectTypeLabel.Text = GlobalResource.AssociationObjectTypeLabel;
            _objectTypeDropDownList.Items.Clear();

            if (!this.DesignMode)
            {
                RefreshObjectTypeDropDown();
            }
        }


        private void RefreshObjectTypeDropDown()
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                NavigationProperty property = dataAccess.GetNavigationProperty(this.Page.Cache
                , typeName, _predicateDropDownList.SelectedItem.Text);

                string resTypeName = string.Empty;

                if (property.Direction == AssociationEndType.Subject)
                {
                    resTypeName = property.Association.ObjectNavigationProperty.Parent.Name;
                }
                else if (property.Direction == AssociationEndType.Object)
                {
                    resTypeName = property.Association.SubjectNavigationProperty.Parent.Name;
                }
                List<ResourceType> resourceTypeList = dataAccess.GetResourceTypeList
                    (resTypeName)
                    .OrderBy(res => res.Name).ToList();

                _objectTypeDropDownList.DataSource = resourceTypeList;
                _objectTypeDropDownList.DataBind();
                FilterCriteriaGrid.EntityType = _objectTypeDropDownList.SelectedValue;
            }
        }

        /// <summary>
        ///  Returns table row with filter controls.
        /// </summary>
        private TableRow[] CreateFilterControlsLayout()
        {
            List<TableRow> rows = new List<TableRow>();

            rows.Add(GetRow(CreateCell(Unit.Percentage(20), _predicateLabel), CreateCell(_predicateDropDownList)));
            rows.Add(GetRow(CreateCell(Unit.Percentage(20), _objectTypeLabel), CreateCell(_objectTypeDropDownList)));

            return rows.ToArray();
        }

        /// <summary>
        /// Creates the controls required for association in design mode.
        /// </summary>
        private void CreatePredicateAndObjectControlsInDesignMode()
        {
            _predicateLabel = CreateLabel(GlobalResource.AssociationPredicateLabel, _predicateLabelId);
            _predicateDropDownList = CreateDropDownList(true, _predicateDropDownId);

            _predicateDropDownList.SelectedIndexChanged += new EventHandler
                (PredicateTypeDropDownList_SelectedIndexChanged);

            _objectTypeLabel = CreateLabel(null, _objectLabelId);

            _objectTypeDropDownList = CreateDropDownList(true, _ObjectDropDownId);
            _objectTypeDropDownList.SelectedIndexChanged += new EventHandler
                (ObjectTypeDropDownList_SelectedIndexChanged);
        }


        /// <summary>
        /// Creates the controls required for association.
        /// </summary>
        private void CreatePredicateAndObjectControls()
        {
            _predicateLabel = CreateLabel(GlobalResource.AssociationPredicateLabel, _predicateLabelId);

            List<NavigationProperty> navigationalProperties = null;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                navigationalProperties = dataAccess.GetNavigationalProperties(this.Page.Cache,
                        typeName).OrderBy(property => property.Name).ToList();
            }


            _predicateDropDownList = CreateDropDownList(null, _predicateDataTextField,
            _predicateDataValueField, true, _predicateDropDownId);

            foreach (NavigationProperty property in navigationalProperties)
            {
                _predicateDropDownList.Items.Add(new ListItem(property.Name, property.Id.ToString()));
            }

            if (!string.IsNullOrEmpty(SelectedPredicateId))
            {
                _predicateDropDownList.SelectedValue = SelectedPredicateId;
            }

            _predicateDropDownList.SelectedIndexChanged += new EventHandler
                (PredicateTypeDropDownList_SelectedIndexChanged);

            _objectTypeLabel = CreateLabel(null, _objectLabelId);

            _objectTypeDropDownList = CreateDropDownList(null, "Name", "FullName", true, _ObjectDropDownId);
            _objectTypeDropDownList.SelectedIndexChanged += new EventHandler
                (ObjectTypeDropDownList_SelectedIndexChanged);
        }

        void ObjectTypeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                FilterCriteriaGrid.EntityType = _objectTypeDropDownList.SelectedValue;
            }
        }

        void PredicateTypeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DestinationCheckBoxList.Items.Clear();
            SourceCheckBoxList.Items.Clear();
            if (DestinationList != null)
            {
                DestinationList.Clear();
            }
            if (SourceList != null)
            {
                SourceList.Clear();
            }
            SourceListSizeDropDown.Items.Clear();
            SourceListSizeDropDown.Visible = false;
            SourceListSizeLabel.Visible = false;

            //Set DataTextField for Source and Destination list box.
            if (_predicateDropDownList.SelectedItem.Text.Equals(_predicateTags))
                ObjectDataTextField = _predicateDataTextField;
            else
                ObjectDataTextField = _resourceObjectTypeDataTextField;

            RefreshObjectTypeDropDown();
        }


        #endregion Private

        #endregion Methods
    }
}
