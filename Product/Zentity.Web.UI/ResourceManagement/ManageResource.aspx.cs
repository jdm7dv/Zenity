// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using System.Globalization;
using Zentity.Web.UI;
using Zentity.Web.UI.ToolKit;
using Zentity.Security.Authentication;
using Zentity.ScholarlyWorks;
using Zentity.Security.AuthorizationHelper;

public partial class ResourceManagement_ManageResource : ZentityBasePage
{
    #region Member Variables
    string _activeTab = string.Empty;
    Guid _resourceId = Guid.Empty;
    string _resourceType = string.Empty;
    UserControls_CreateResource _createResourceControl;
    #endregion

    #region Constants

    private const string _selectedResourceTypeKey = "selectedResourceTypeKey";
    private const string _uriMangeResource = "ManageResource.aspx?";
    private const string _mangeResourcePage = "ManageResource.aspx";
    private const string _uriCreateResource = "~/UserControls/CreateResource.ascx";
    private const string _uriCreateAssociation = "~/UserControls/CreateAssociation.ascx";
    private const string _uriCategoryAssociation = "~/UserControls/CategoryNodeAssociation.ascx";
    private const string _uriScholItemTagging = "~/UserControls/CreateScholarlyWorkItemTagging.ascx";
    private const string _uriResourceDetails = "~/UserControls/DisplayResourceDetails.ascx";
    private const string _uriChangeHistory = "~/UserControls/ChangeHistory.ascx";
    private const string _uriResourcePermissions = "~/UserControls/ResourcePermissions.ascx";
    private const string _uriSubjectNavigation = "~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=";

    private const string _currentTabCSS = "currentTab";
    private const string _classAttribute = "class";

    private const string _resourceTypeKey = "ResourceType";
    private const string _resourceTypeParam = "ResourceType={0}";
    private const string _activeTabKey = "ActiveTab";
    private const string _activeTabParam = "ActiveTab={0}";
    private const string _activeTabParam1 = "&ActiveTab={0}";
    private const string _idParam = "Id={0}";
    private const string _categoryNodesProperty = "CategoryNodes";
    private const string _tagsProperty = "Tags";

    #endregion

    #region Properties

    private string SelectedResourceType
    {
        get { return ViewState[_selectedResourceTypeKey] != null ? (string)ViewState[_selectedResourceTypeKey] : Constants.ResourceEntityType; }
        set { ViewState[_selectedResourceTypeKey] = value; }
    }

    private Guid ResourceId
    {
        get
        {
            if (_resourceId == Guid.Empty)
            {
                string id = Convert.ToString(Request.QueryString[Constants.Id]);
                if (!string.IsNullOrEmpty(id) && ValidateIdParameter(id))
                    _resourceId = new Guid(id);
            }

            return _resourceId;
        }
    }

    private bool IsEditMode
    {
        get
        {
            if (ResourceId != Guid.Empty)
                return true;
            return false;
        }
    }

    private string ActiveTab
    {
        get
        {
            if (string.IsNullOrEmpty(_activeTab))
            {
                _activeTab = GetActiveTab();
            }

            return _activeTab;
        }
        set
        {
            _activeTab = value;
        }

    }

    #endregion

    #region EventHandlers

    protected void Page_Init(object sender, EventArgs e)
    {
        Initialize();
        LoadTabControl();
        if (!IsEditMode)
        {
            ResourceTypeControl.OnResourceTypeSelected += new EventHandler<EventArgs>(ResourceTypeControl_OnResourceTypeSelected);
        }
    }

    protected void ResourceTypeControl_OnResourceTypeSelected(object sender, EventArgs e)
    {
        Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceTypeControl.ResourceType, Constants.MetadataTab));
    }

    protected void btnResourceType_Click(object sender, EventArgs e)
    {
        SelectedResourceType = string.Empty;
        Response.Redirect(_mangeResourcePage);
    }

    protected void btnMetadata_Click(object sender, EventArgs e)
    {
        if (IsEditMode)
            Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceId, Constants.MetadataTab));
        else
            Response.Redirect(_uriMangeResource + ConstructQueryString(SelectedResourceType, Constants.MetadataTab));
    }

    protected void btnAssociations_Click(object sender, EventArgs e)
    {
        Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceId, Constants.AssociationTab));
    }

    protected void btnTags_Click(object sender, EventArgs e)
    {
        Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceId, Constants.TagsTab));
    }

    protected void btnSummary_Click(object sender, EventArgs e)
    {
        Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceId, Constants.SummaryTab));
    }

    protected void btnCategories_Click(object sender, EventArgs e)
    {
        Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceId, Constants.CategoriesTab));
    }

    protected void btnChangeHistory_Click(object sender, EventArgs e)
    {
        Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceId, Constants.ChangeHistoryTab));
    }

    protected void btnResourcePermission_Click(object sender, EventArgs e)
    {
        Response.Redirect(_uriMangeResource + ConstructQueryString(ResourceId, Constants.ResourcePermissionsTab));
    }

    protected void OnMetadataSave(object sender, EventArgs e)
    {
        if (!IsEditMode)
        {
            Response.Redirect(_uriMangeResource + ConstructQueryString(_createResourceControl.ResourceId,
                Constants.SummaryTab));
        }
    }

    protected void objControl_OnSuccessfulDelete(object sender, EventArgs e)
    {
        ResourceTypeTab.Visible = MetadataTab.Visible = AssociationsTab.Visible =
            CategoriesTab.Visible = TagsTab.Visible = SummaryTab.Visible
            = ChangeHistoryTab.Visible = ResourcePermissionsTab.Visible = false;
    }

    #endregion

    #region Methods

    private void Initialize()
    {
        ResourceType type = null;
        IEnumerable<NavigationProperty> propertyCollection = null;
        ResourcePermissions<Resource> userPermissions = null;
        bool isAdmin = false;
        bool isOwner = false;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess())
        {
            if (IsEditMode)
            {
                AuthenticatedToken token = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
                userPermissions = dataAccess.GetResourcePermissions(token, ResourceId);

                //Throw exception is user is not having atleast read permission on the resource.
                if (userPermissions == null || !userPermissions.Permissions.Contains(UserResourcePermissions.Read))
                {
                    throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                        Resources.Resources.MsgUnAuthorizeAccess, UserResourcePermissions.Read));
                }
                isAdmin = dataAccess.IsAdmin(token);
                isOwner = dataAccess.IsOwner(token, userPermissions.Resource);

                type = dataAccess.GetResourceType(ResourceId);
                propertyCollection = dataAccess.GetNavigationProperties(Cache, ResourceId);
            }
            else
            {
                string resType = Convert.ToString(Request.QueryString[_resourceTypeKey]);
                if (!string.IsNullOrEmpty(resType))
                {
                    type = dataAccess.GetResourceType(resType);
                }
            }
        }
        if (type != null)
        {
            SelectedResourceType = type.Name;
        }


        UpdateControlsStatus(propertyCollection, userPermissions, isAdmin, isOwner);
    }

    private void UpdateControlsStatus(IEnumerable<NavigationProperty> propertyCollection,
        ResourcePermissions<Resource> userPermissions, bool isAdmin, bool isOwner)
    {
        ResourceTypeControl.ResourceHierarchy = SelectedResourceType;

        if (IsEditMode)
        {
            ResourceTypeTab.Visible = false;

            //Apply security checks
            SummaryTab.Visible = ChangeHistoryTab.Visible =
            userPermissions.Permissions.Contains(UserResourcePermissions.Read) ? true : false;

            MetadataTab.Visible = AssociationsTab.Visible =
                userPermissions.Permissions.Contains(UserResourcePermissions.Update) ? true : false;

            //TODO: Add one more condittion to check whether the current user is admin or not(after changes in Security API)
            ResourcePermissionsTab.Visible = isAdmin | isOwner ? true : false;

            if (!userPermissions.Permissions.Contains(UserResourcePermissions.Update) ||
                propertyCollection.Where(tuple => tuple.Name.Equals(_categoryNodesProperty)).Count() == 0)
                CategoriesTab.Visible = false;
            else
                CategoriesTab.Visible = true;

            if (!userPermissions.Permissions.Contains(UserResourcePermissions.Update) ||
                propertyCollection.Where(tuple => tuple.Name.Equals(_tagsProperty)).Count() == 0)
                TagsTab.Visible = false;
            else
                TagsTab.Visible = true;
        }
        else
        {
            ResourceTypeTab.Visible = true;

            MetadataTab.Visible = Constants.MetadataTab.Equals(ActiveTab) ? true : false;

            SummaryTab.Visible = AssociationsTab.Visible =
                ResourcePermissionsTab.Visible = TagsTab.Visible =
                CategoriesTab.Visible = ChangeHistoryTab.Visible = false;
        }
    }

    private bool ValidateIdParameter(string id)
    {
        bool result = false;
        if (!string.IsNullOrEmpty(id))
        {
            try
            {
                Guid resourceId = new Guid(id);
                result = IsValidResourceId(resourceId);
                if (!result)
                {
                    throw new EntityNotFoundException(EntityType.Resource, resourceId);
                }
            }
            catch (FormatException)
            {
                throw new FormatException(Resources.Resources.InvalidResource);
            }
        }

        return result;
    }

    private bool IsValidResourceId(Guid resourceId)
    {
        Resource resourceObj = null;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess())
        {
            resourceObj = dataAccess.GetResource(resourceId);
        }

        return resourceObj != null;
    }

    private string GetActiveTab()
    {
        string activeTab = Convert.ToString(Request.QueryString[_activeTabKey]);
        if (!string.IsNullOrEmpty(activeTab))
        {
            if (!IsValidTab(activeTab))
            {
                throw new ArgumentException(Resources.Resources.MsgInvalidTab);
            }
            if (!IsEditMode)
            {
                if (SelectedResourceType == null)
                {
                    activeTab = null;
                }
                else if (activeTab != Constants.MetadataTab)
                {
                    activeTab = Constants.MetadataTab;
                }
            }
            else
            {
                if ((SummaryTab.Visible &&
                    (activeTab.Equals(Constants.MetadataTab, StringComparison.OrdinalIgnoreCase) && !MetadataTab.Visible) ||
                    (activeTab.Equals(Constants.AssociationTab, StringComparison.OrdinalIgnoreCase) && !AssociationsTab.Visible) ||
                    (activeTab.Equals(Constants.CategoriesTab, StringComparison.OrdinalIgnoreCase) && !CategoriesTab.Visible) ||
                    (activeTab.Equals(Constants.TagsTab, StringComparison.OrdinalIgnoreCase) && !TagsTab.Visible) ||
                    (activeTab.Equals(Constants.ChangeHistoryTab, StringComparison.OrdinalIgnoreCase) && !ChangeHistoryTab.Visible) ||
                    (activeTab.Equals(Constants.ResourcePermissionsTab, StringComparison.OrdinalIgnoreCase) && !ResourcePermissionsTab.Visible)))
                {
                    activeTab = Constants.SummaryTab;
                }
            }
        }
        else
        {
            if (IsEditMode)
            {
                activeTab = Constants.SummaryTab;
            }
            else
            {
                if (!string.IsNullOrEmpty(SelectedResourceType))
                {
                    using (ResourceDataAccess dataAccess = new ResourceDataAccess())
                    {
                        int childTypeCount = dataAccess.GetChildResourceTypesCount(SelectedResourceType);
                        if (childTypeCount == 0)
                        {
                            activeTab = Constants.MetadataTab;
                        }
                    }
                }
            }
        }

        return activeTab;
    }

    private bool IsValidTab(string activeTab)
    {
        bool result = false;

        if (activeTab.Equals(Constants.MetadataTab, StringComparison.OrdinalIgnoreCase) ||
            activeTab.Equals(Constants.AssociationTab, StringComparison.OrdinalIgnoreCase) ||
                activeTab.Equals(Constants.CategoriesTab, StringComparison.OrdinalIgnoreCase) ||
                activeTab.Equals(Constants.TagsTab, StringComparison.OrdinalIgnoreCase) ||
                activeTab.Equals(Constants.SummaryTab, StringComparison.OrdinalIgnoreCase) ||
                activeTab.Equals(Constants.ChangeHistoryTab, StringComparison.OrdinalIgnoreCase) ||
                activeTab.Equals(Constants.ResourcePermissionsTab, StringComparison.OrdinalIgnoreCase))
        {
            result = true;
        }
        return result;
    }

    private void LoadTabControl()
    {
        vwMetadata.Controls.Clear();
        vwAssociations.Controls.Clear();
        vwTags.Controls.Clear();
        vwCategories.Controls.Clear();
        vwSummary.Controls.Clear();
        vwChangeHistory.Controls.Clear();
        vwResourcePermission.Controls.Clear();

        //Highlight selected tab using CSS.
        SelectTab(ActiveTab);

        if (!string.IsNullOrEmpty(ActiveTab))
        {
            if (ActiveTab.Equals(Constants.MetadataTab, StringComparison.OrdinalIgnoreCase))
            {
                SetMetadataControl();
            }
            else if (ActiveTab.Equals(Constants.AssociationTab, StringComparison.OrdinalIgnoreCase))
            {
                SetAssociationControl();
            }
            else if (ActiveTab.Equals(Constants.CategoriesTab, StringComparison.OrdinalIgnoreCase))
            {
                SetCategorizationControl();
            }
            else if (ActiveTab.Equals(Constants.TagsTab, StringComparison.OrdinalIgnoreCase))
            {
                SetTaggingControl();
            }
            else if (ActiveTab.Equals(Constants.SummaryTab, StringComparison.OrdinalIgnoreCase))
            {
                SetDisplayControl();
            }
            else if (ActiveTab.Equals(Constants.ChangeHistoryTab, StringComparison.OrdinalIgnoreCase))
            {
                SetChangeHistoryControl();
            }
            else if (ActiveTab.Equals(Constants.ResourcePermissionsTab, StringComparison.OrdinalIgnoreCase))
            {
                SetResourcePermissionsControl();
            }
        }
    }

    private void SetMetadataControl()
    {
        _createResourceControl = (UserControls_CreateResource)this.LoadControl
            (_uriCreateResource);
        if (ResourceId != Guid.Empty)
        {
            _createResourceControl.ResourceId = ResourceId;
        }
        else
        {
            _createResourceControl.ResourceType = SelectedResourceType.ToString();
        }
        _createResourceControl.EnableViewState = true;
        _createResourceControl.OnResourceSave += new EventHandler<EventArgs>(OnMetadataSave);
        vwMetadata.Controls.Clear();
        vwMetadata.Controls.Add(_createResourceControl);
        mtvManage.SetActiveView(vwMetadata);
    }

    private void SetAssociationControl()
    {
        UserControls_CreateAssociation createAssociationControl = (UserControls_CreateAssociation)this.LoadControl
                     (_uriCreateAssociation);
        createAssociationControl.ResourceId = ResourceId;
        createAssociationControl.EnableViewState = true;
        createAssociationControl.SubjectNavigationalUrl = _uriSubjectNavigation + Constants.SummaryTab;
        vwAssociations.Controls.Clear();
        vwAssociations.Controls.Add(createAssociationControl);
        mtvManage.SetActiveView(vwAssociations);
    }

    private void SetDisplayControl()
    {
        UserControls_DisplayResourceDetails displayDetailsControl = (UserControls_DisplayResourceDetails)
                  this.LoadControl(_uriResourceDetails);
        displayDetailsControl.ResourceId = ResourceId;
        displayDetailsControl.EnableViewState = true;
        displayDetailsControl.OnSuccessfulDelete += new EventHandler<EventArgs>(objControl_OnSuccessfulDelete);
        vwSummary.Controls.Clear();
        vwSummary.Controls.Add(displayDetailsControl);
        mtvManage.SetActiveView(vwSummary);
    }

    private void SetCategorizationControl()
    {
        UserControls_CategoryNodeAssociation categoryAssociationControl = (UserControls_CategoryNodeAssociation)
            this.LoadControl(_uriCategoryAssociation);
        categoryAssociationControl.ResourceId = ResourceId;
        categoryAssociationControl.EnableViewState = true;
        categoryAssociationControl.SubjectNavigationalUrl = _uriSubjectNavigation + Constants.SummaryTab;
        vwCategories.Controls.Clear();
        vwCategories.Controls.Add(categoryAssociationControl);
        mtvManage.SetActiveView(vwCategories);
    }

    private void SetTaggingControl()
    {
        UserControls_CreateScholarlyWorkItemTagging createTaggingControl = (UserControls_CreateScholarlyWorkItemTagging)
                   this.LoadControl(_uriScholItemTagging);
        createTaggingControl.ResourceId = ResourceId;
        createTaggingControl.ObjectType = ObjectEntityType.Tag;
        createTaggingControl.EnableViewState = true;
        createTaggingControl.SubjectNavigationalUrl = _uriSubjectNavigation + Constants.SummaryTab;
        vwTags.Controls.Clear();
        vwTags.Controls.Add(createTaggingControl);
        mtvManage.SetActiveView(vwTags);
    }

    private void SetChangeHistoryControl()
    {
        UserControls_ChangeHistory changeHistoryControl = (UserControls_ChangeHistory)
              this.LoadControl(_uriChangeHistory);
        changeHistoryControl.ResourceId = ResourceId;
        changeHistoryControl.EnableViewState = true;
        vwChangeHistory.Controls.Clear();
        vwChangeHistory.Controls.Add(changeHistoryControl);
        mtvManage.SetActiveView(vwChangeHistory);
    }

    private void SetResourcePermissionsControl()
    {
        UserControls_ResourcePermissions resourcePermissionsControl = (UserControls_ResourcePermissions)this.LoadControl
                    (_uriResourcePermissions);

        resourcePermissionsControl.ResourceId = ResourceId;
        resourcePermissionsControl.Visible = true;
        vwResourcePermission.Controls.Clear();
        vwResourcePermission.Controls.Add(resourcePermissionsControl);
        mtvManage.SetActiveView(vwResourcePermission);
    }

    private void SelectTab(string activeTab)
    {
        System.Web.UI.HtmlControls.HtmlGenericControl tabControl = null;
        switch (activeTab)
        {
            case Constants.MetadataTab:
                tabControl = MetadataTab;
                break;
            case Constants.AssociationTab:
                tabControl = AssociationsTab;
                break;
            case Constants.CategoriesTab:
                tabControl = CategoriesTab;
                break;
            case Constants.TagsTab:
                tabControl = TagsTab;
                break;
            case Constants.SummaryTab:
                tabControl = SummaryTab;
                break;
            case Constants.ChangeHistoryTab:
                tabControl = ChangeHistoryTab;
                break;
            case Constants.ResourcePermissionsTab:
                tabControl = ResourcePermissionsTab;
                break;
            default:
                tabControl = ResourceTypeTab;
                break;
        }
        tabControl.Attributes.Add(_classAttribute, _currentTabCSS);
    }

    private string ConstructQueryString(string resourceType, string tabToDisplay)
    {
        string queryString = string.Empty;
        if (!string.IsNullOrEmpty(resourceType))
        {
            queryString = string.Format(CultureInfo.InvariantCulture, _resourceTypeParam, resourceType);
        }
        if (!string.IsNullOrEmpty(tabToDisplay))
        {
            queryString += string.IsNullOrEmpty(queryString) ?
                string.Format(CultureInfo.InvariantCulture, _activeTabParam, tabToDisplay) :
                string.Format(CultureInfo.InvariantCulture, _activeTabParam1, tabToDisplay);
        }

        return queryString;
    }

    private string ConstructQueryString(Guid id, string tabToDisplay)
    {
        string queryString = string.Empty;
        if (id != Guid.Empty)
        {
            queryString = string.Format(CultureInfo.InvariantCulture, _idParam, id.ToString());
        }
        if (!string.IsNullOrEmpty(tabToDisplay))
        {
            queryString += string.IsNullOrEmpty(queryString) ?
                string.Format(CultureInfo.InvariantCulture, _activeTabParam, tabToDisplay) :
                string.Format(CultureInfo.InvariantCulture, _activeTabParam1, tabToDisplay);
        }
        return queryString;
    }

    #endregion
}