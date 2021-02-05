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
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class inherits Association class.
    /// It allows association of tags or categories with the subject resource.
    /// </summary>
    /// <example>The code below is the source for ScholarlyWorkItemTagging.aspx. It shows an example of using 
    ///     <see cref="ScholarlyWorkItemTagging"/> control.
    ///     It can be configured to provide Resource Tagging and Resource Categorization.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register assembly="Zentity.Web.UI.ToolKit" namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="Zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head runat="server"&gt;
    ///             &lt;title&gt;ScholarlyWorkItemTagging Sample&lt;/title&gt;
    ///             
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_Load(object sender, EventArgs e)
    ///                 {
    ///                     string id = Convert.ToString(Request.QueryString["Id"]);
    ///                     if (string.IsNullOrEmpty(id))
    ///                     {
    ///                         StatusLabel.Text = "Please pass an Id parameter (GUID) in the query string. Example: " +
    ///                             Request.Url.AbsolutePath + "?Id=6bd35f74-5a07-4df8-98a0-7ddb71f88c24";
    ///                         ScholarlyWorkItemTagging1.Visible = false;
    ///                         SaveButton.Visible = false;
    ///                    }
    ///                     else
    ///                     {
    ///                         StatusLabel.Text = string.Empty;
    ///                         Guid entityId = new Guid(id);
    ///                         ScholarlyWorkItemTagging1.SubjectItemId = entityId;
    ///                     }
    ///                 }
    ///                 
    ///                 protected void Save_Click(object sender, EventArgs e)
    ///                 {
    ///                     ScholarlyWorkItemTagging1.SaveAssociation();
    ///                 }
    ///             &lt;/script&gt;
    ///             
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:ScholarlyWorkItemTagging ID="ScholarlyWorkItemTagging1" runat="server"
    ///                     AssociationListHeight="120px" Title="Resource Tagging" IsSecurityAwareControl="false"&gt;
    ///                     &lt;MoveRightButton Text="&gt;&gt;" /&gt;
    ///                     &lt;MoveLeftButton Text="&lt;&lt;" /&gt;
    ///                 &lt;/Zentity:ScholarlyWorkItemTagging&gt;
    ///                 &lt;br /&gt;
    ///                 &lt;asp:Button ID="SaveButton" runat="server" Text="Save" OnClick="Save_Click" /&gt;
    ///                 &lt;asp:Label ID="StatusLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [ToolboxData("<{0}:ScholarlyWorkItemTagging runat=\"server\">"
        + "<MoveRightButton Text =\">>\"/>"
        + "<MoveLeftButton Text =\"<<\"/>"
        + "</{0}:ScholarlyWorkItemTagging>")]
    public class ScholarlyWorkItemTagging : Association
    {
        #region Constants

        #region Private

        const string _title = "Title";
        const string _name = "Name";
        const string _objectTypeViewStateKey = "ObjectType";

        #endregion

        #endregion


        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets type of object entity.
        /// </summary>
        [Browsable(true)]
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionAssociationObjectType")]
        public ObjectEntityType ObjectType
        {
            get
            {
                return (ViewState[_objectTypeViewStateKey] != null) ?
                    (ObjectEntityType)ViewState[_objectTypeViewStateKey] : ObjectEntityType.Tag;
            }
            set
            {
                ViewState[_objectTypeViewStateKey] = value;
            }
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Checks subject entity and loads the control, raises the Load event and updates the button status.
        /// </summary>
        protected override void CreateChildControls()
        {
            object entity = null;
            bool result = false;
            if (!this.DesignMode)
            {
                entity = GetSubjectEntity();
                if (entity == null)
                {
                    RenderControlsInErrorMode();
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            if (result)
            {
                RenderControls(entity);
            }
        }

        /// <summary>
        /// Handles Load event of the control.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            switch (ObjectType)
            {
                case ObjectEntityType.Tag:
                    ObjectDataTextField = _name;
                    break;
                case ObjectEntityType.CategoryNode:
                    ObjectDataTextField = _title;
                    break;
            }
        }

        #endregion

        #region Methods

        #region public

        /// <summary>
        /// Saves associations for the subject entity.
        /// </summary>
        /// <returns>A Boolean value indicating success of operation.</returns>
        public override bool SaveAssociation()
        {
            bool result = false;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                AuthorizeResourcesBeforeSave<Tag>(dataAccess);

                switch (ObjectType)
                {
                    case ObjectEntityType.Tag:
                        result = dataAccess.SaveScholarlyWorkItemTagAssociation(SubjectItemId,
                            DestinationList as List<Tag>);
                        base.RefreshDataSource();
                        break;
                    case ObjectEntityType.CategoryNode:
                        result = dataAccess.SaveScholarlyItemCategoryAssociation(SubjectItemId,
                            DestinationList as List<CategoryNode>, AuthenticatedToken, Constants.PermissionRequiredForAssociation);
                        base.RefreshDataSource();
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        #endregion

        #region Protected Internal

        /// <summary>
        /// Returns list of items to be used as a source list.
        /// </summary>        
        /// <returns>List of source items.</returns>
        protected override IList GetSourceItems()
        {
            SourceCheckBoxList.Items.Clear();


            IList sourceItems = null;
            //sourceItems = GetSourceResources("");
            switch (ObjectType)
            {
                case ObjectEntityType.Tag:
                    sourceItems = GetSourceResources(Constants.TagFullName);
                    break;
                case ObjectEntityType.CategoryNode:
                    sourceItems = GetSourceResources(Constants.CategoryNodeFullName);
                    break;
                default:
                    break;
            }

            return sourceItems;
        }

        /// <summary>
        /// Returns list of items to be used as a destination list.
        /// </summary>
        /// <returns>List of destination items.</returns>
        protected override IList GetDestinationItems()
        {
            IList destinationList = null;
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                switch (ObjectType)
                {
                    case ObjectEntityType.Tag:
                        {
                            if (IsSecurityAwareControl)
                            {
                                if (AuthenticatedToken != null)
                                {
                                    destinationList = dataAccess.GetScholarlyWorkItemTags(SubjectItemId, AuthenticatedToken);
                                }
                            }
                            else
                            {
                                destinationList = dataAccess.GetScholarlyWorkItemTags(SubjectItemId, null);
                            }
                            break;
                        }
                    case ObjectEntityType.CategoryNode:
                        {
                            if (IsSecurityAwareControl)
                            {
                                if (AuthenticatedToken != null)
                                {
                                    destinationList = dataAccess.GetScholarlyWorkItemCategoryNodes(SubjectItemId, AuthenticatedToken);
                                }
                            }
                            else
                            {
                                destinationList = dataAccess.GetScholarlyWorkItemCategoryNodes(SubjectItemId, null);
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            return destinationList;
        }

        #endregion

        #region Private

        /// <summary>
        /// 
        /// </summary>
        private void RenderControlsInErrorMode()
        {
            Table table = new Table();
            table.Width = Unit.Percentage(100);
            table.Rows.Add(CreateTitleRow());
            this.Controls.Add(table);

            ErrorLabel.Text = Resources.GlobalResource.
                PredicateAssociationSubjectEntityErrorMessage;
            ErrorLabel.ForeColor = System.Drawing.Color.Red;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        private void RenderControls(object entity)
        {
            base.CreateChildControls();
            this.Controls.Add(ContainerTable);

            SourceCheckBoxListLabel.Text = string.Format(CultureInfo.CurrentCulture,
                GlobalResource.AssociationAvailableTags, ObjectType.ToString());
            DestinationCheckBoxListLabel.Text = string.Format(CultureInfo.CurrentCulture,
                GlobalResource.AssociationSelectedTags, ObjectType.ToString());

            if (ObjectType == ObjectEntityType.CategoryNode)
            {
                FilterCriteriaGrid.EntityType = Constants.CategoryNodeFullName;
            }
            else
            {
                FilterCriteriaGrid.EntityType = Constants.TagFullName;
            }

            if (this.DesignMode)
            {
                _subjectLabel.Text = GlobalResource.AssociationSubjectLabel;
            }
            else
            {
                SetSubjectControlValues(entity);
            }

            DisableDestinationItemsSequencing();
        }

        /// <summary>
        /// Sets the value of all subject related controls.
        /// </summary>
        private void SetSubjectControlValues(object entity)
        {
            _subjectLabel.Text = GlobalResource.AssociationSubjectLabel;
            if (entity != null)
            {
                SubjectDisplayLink.Text = System.Web.HttpUtility.HtmlEncode
                    (CoreHelper.UpdateEmptyTitle(CoreHelper.FitString(CoreHelper.GetTitleByResourceType((Resource)entity), 40)));
            }
        }

        /// <summary>
        /// Gets the subject entity.
        /// </summary>
        /// <returns> Subject entity. </returns>
        private object GetSubjectEntity()
        {
            Resource entity = null;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                entity = dataAccess.GetResource(SubjectItemId, Constants.ScholarlyWorkItemFullName);
                dataAccess.AuthorizeResource<Resource>(AuthenticatedToken, UserResourcePermissions.Update, entity, true);
            }

            return entity;
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Enumeration for identifying type of object entity.
    /// </summary>
    public enum ObjectEntityType
    {
        /// <summary>
        /// Tag entity type.
        /// </summary>
        Tag,
        /// <summary>
        /// Category entity type.
        /// </summary>
        CategoryNode
    }
}
