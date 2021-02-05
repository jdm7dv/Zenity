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
using Zentity.Web.UI;
using Zentity.Core;
using Zentity.Security.Authorization;
using Zentity.Security.AuthorizationHelper;
using Zentity.Security.Authentication;
using Zentity.Platform;

public partial class UserControls_GroupAssignment : System.Web.UI.UserControl
{
    private RoleType _roleType;
    private List<Group> _seletcedList;
    private List<Identity> _selectedIdentityList;
    private List<Resource> _selectedResourceList;
    private string _cssClass = string.Empty;
    private string _userSearchButton = string.Empty;
    private string _groupSearchButton = string.Empty;
    private string _resourceSearchButton = string.Empty;
    private RoleType _searchResultType;
    private bool _isEnable = true;
    private ListSelectionMode _selectionMode = ListSelectionMode.Multiple;
    private SelectedType _selectedType;
    private AuthenticatedToken userToken = null;

    public enum RoleType
    {
        User,
        Group,
        Resource
    }

    public enum SelectedType
    {
        AdminUserName,
        GuestUserName,
        AdminGroupName,
        AllUsersGroupName
    }

    public RoleType ControlMode
    {
        get
        {
            return _roleType;
        }
        set
        {
            _roleType = value;
        }
    }

    public ListSelectionMode SelectedMode
    {
        get
        {
            return _selectionMode;
        }
        set
        {
            _selectionMode = value;
        }
    }

    public RoleType SearchResultFor
    {
        get
        {
            return _searchResultType;
        }
        set
        {
            _searchResultType = value;
        }
    }

    public List<Group> SelectedList
    {
        set
        {
            _seletcedList = value;
        }
        get
        {
            return _seletcedList;
        }
    }

    public List<Identity> SelectedIdentityList
    {
        set
        {
            _selectedIdentityList = value;
        }
        get
        {
            return _selectedIdentityList;
        }
    }

    public List<Resource> SelectedResourceList
    {
        set
        {
            _selectedResourceList = value;
        }
        get
        {
            return _selectedResourceList;
        }
    }

    public string CssClass
    {
        set
        {
            _cssClass = value;
        }
    }


    public string UserSearchButton
    {
        get
        {
            return _userSearchButton;
        }
        set
        {
            _userSearchButton = value;
        }
    }

    public string GroupSearchButton
    {
        get
        {
            return _groupSearchButton;
        }
        set
        {
            _groupSearchButton = value;
        }
    }

    public string ResourceSearchButton
    {
        get
        {
            return _resourceSearchButton;
        }
        set
        {
            _resourceSearchButton = value;
        }
    }

    public bool IsEnable
    {
        get
        {
            return _isEnable;
        }
        set
        {
            _isEnable = value;
        }
    }

    public SelectedType SelectedUserOrGroupType
    {
        get
        {
            return _selectedType;
        }
        set
        {
            _selectedType = value;
        }
    }

    public void ClearGroupList()
    {
        chkGroupList.Items.Clear();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        userToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;

        if (!Page.IsPostBack)
        {
            using (ZentityContext context = Utility.CreateContext())
            {
                if (ControlMode == RoleType.Group)
                {
                    List<Group> listGroup = new List<Group>();
                    using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
                    {
                        listGroup = dataAccess.GetGroups(string.Empty).OrderBy(tuple => tuple.GroupName).ToList();
                    }

                    HandledExsitingGroups(listGroup);
                }
                else if (ControlMode == RoleType.User)
                {
                    List<Identity> listIdentity = new List<Identity>();

                    using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
                    {
                        listIdentity = dataAccess.GetIdentities(string.Empty).OrderBy(tuple => tuple.IdentityName).ToList();
                    }
                    HandledExsitingIdentities(listIdentity);
                }
                else
                {
                    List<Resource> listResource = context.Resources.ToList();

                    foreach (Resource resource in listResource)
                    {
                        if (SelectedMode == ListSelectionMode.Single)
                        {
                            lstGroupList.Items.Add(new ListItem(resource.Title, resource.Id.ToString()));
                            chkGroupList.Visible = false;
                            lstGroupList.Visible = true;
                        }
                        else
                        {
                            chkGroupList.Items.Add(new ListItem(resource.Title, resource.Id.ToString()));
                        }
                    }
                }
            }
        }

        InitializeLabels();
    }

    private void HandledExsitingIdentities(List<Identity> listIdentity)
    {
        foreach (Identity identity in listIdentity)
        {
            chkGroupList.Items.Add(new ListItem(identity.IdentityName, identity.Id.ToString()));
            if (identity.IdentityName == UserManager.GuestUserName || identity.IdentityName == UserManager.AdminUserName)
            {
                chkGroupList.Items[chkGroupList.Items.Count - 1].Enabled = false;
            }

            if (SelectedUserOrGroupType == SelectedType.AllUsersGroupName)
            {
                if (identity.IdentityName != UserManager.AdminUserName)
                    chkGroupList.Items[chkGroupList.Items.Count - 1].Selected = true;
            }
            else if (SelectedUserOrGroupType == SelectedType.AdminGroupName)
            {
                if (identity.IdentityName == UserManager.AdminUserName)
                    chkGroupList.Items[chkGroupList.Items.Count - 1].Enabled = false;
            }
        }
    }

    private void HandledExsitingGroups(List<Group> listGroup)
    {
        foreach (Group group in listGroup)
        {
            chkGroupList.Items.Add(new ListItem(group.GroupName, group.Id.ToString()));
            if (group.GroupName == UserManager.AllUsersGroupName)
            {
                chkGroupList.Items[chkGroupList.Items.Count - 1].Enabled = false;
            }

            if (SelectedUserOrGroupType == SelectedType.GuestUserName)
            {
                if (group.GroupName == UserManager.AllUsersGroupName)
                    chkGroupList.Items[chkGroupList.Items.Count - 1].Selected = true;
            }
        }
    }

    private void InitializeLabels()
    {
        chkGroupList.CssClass = _cssClass;
        if (ControlMode == RoleType.Group)
            lblAssignment.Text = Resources.Resources.AssignmentsGroup;
        else if (ControlMode == RoleType.User)
            lblAssignment.Text = Resources.Resources.AssignmentsUser;
        else
            lblAssignment.Text = Resources.Resources.AssignmentsResource;


        if (!string.IsNullOrEmpty(GroupSearchButton))
        {
            btnGroupSearch.Text = GroupSearchButton;
            btnGroupSearch.Visible = true;
        }
        if (!string.IsNullOrEmpty(UserSearchButton))
        {
            btnUserSearch.Text = UserSearchButton;
            btnUserSearch.Visible = true;
        }

        if (!string.IsNullOrEmpty(ResourceSearchButton))
        {
            btnResourceSearch.Text = ResourceSearchButton;
            btnResourceSearch.Visible = true;
        }

        if (SelectedList != null && !Page.IsPostBack)
            SetSelectedGroup();
        if (SelectedIdentityList != null && !Page.IsPostBack)
            SetSelectedIdentity();

        if (!IsEnable)
        {
            groupAssignmentPanel.Enabled = false;
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string searchText = txtSerachGroup.Text.Trim();
        SearchResultFor = RoleType.User;
        ClearGroupList();
        List<Identity> listIdentity = null;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            listIdentity = dataAccess.GetIdentities(searchText).OrderBy(tuple => tuple.IdentityName).ToList();
        }

        HandledExsitingIdentities(listIdentity);
        if (SelectedIdentityList != null)
            SetSelectedIdentity();
    }

    protected void btnGroupSearch_Click(object sender, EventArgs e)
    {
        string searchText = txtSerachGroup.Text.Trim();
        SearchResultFor = RoleType.Group;
        ClearGroupList();

        SearchEngine search = new SearchEngine();
        List<Group> listGroup = null;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            listGroup = dataAccess.GetGroups(searchText).OrderBy(tuple => tuple.GroupName).ToList();
        }
        HandledExsitingGroups(listGroup);
        if (SelectedList != null)
            SetSelectedGroup();
    }

    protected void btnResourceSearch_Click(object sender, EventArgs e)
    {
        string searchText = txtSerachGroup.Text.Trim();
        SearchResultFor = RoleType.Resource;
        ClearGroupList();
        int totalRecords = 0;

        if (!string.IsNullOrEmpty(searchText))
        {
            SearchEngine search = new SearchEngine();

            List<Resource> listResources = search.SearchResources(Resources.Resources.SearchTitle + searchText, Utility.CreateContext(), 0, out totalRecords).ToList();

            foreach (Resource resource in listResources)
            {
                chkGroupList.Items.Add(new ListItem(resource.Title, resource.Id.ToString()));
            }
        }
    }

    public List<String> GetSearchList()
    {
        List<String> allList = new List<String>();
        foreach (ListItem item in chkGroupList.Items)
        {
            allList.Add(item.Value);
        }
        return allList;
    }

    public List<String> GetSelectGroup()
    {
        List<String> selectedGroup = new List<String>();

        foreach (ListItem item in chkGroupList.Items)
        {
            if (item.Selected && item.Enabled)
            {
                selectedGroup.Add(item.Value);
            }
        }

        return selectedGroup;
    }

    public ListItemCollection GetSelectItemList()
    {
        ListItemCollection selectedItem = new ListItemCollection();

        foreach (ListItem item in chkGroupList.Items)
        {
            if (item.Selected)
            {
                selectedItem.Add(item);
            }
        }
        return selectedItem;
    }

    public void DeSelectAllItem()
    {
        foreach (ListItem item in chkGroupList.Items)
        {
            item.Selected = false;
        }
    }


    public void SetSelectedItem()
    {
        foreach (Resource resource in SelectedResourceList)
        {
            foreach (ListItem item in chkGroupList.Items)
            {
                if (item.Value == resource.Id.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }
        }
    }

    public void SetSelectedGroup()
    {
        foreach (Resource group in SelectedList)
        {
            foreach (ListItem item in chkGroupList.Items)
            {
                if (item.Value == group.Id.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }
        }
    }

    public void SetSelectedIdentity()
    {
        foreach (Resource group in SelectedIdentityList)
        {
            foreach (ListItem item in chkGroupList.Items)
            {
                if (item.Value == group.Id.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }
        }
    }
}