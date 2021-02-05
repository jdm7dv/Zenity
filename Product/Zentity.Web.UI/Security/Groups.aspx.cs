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
using Zentity.Web.UI.ToolKit;
using System.Globalization;
using Zentity.Web.UI;
using Zentity.Security.Authentication;
using System.Data;
using Zentity.ScholarlyWorks;
using Zentity.Security.Authorization;
using Zentity.Security.AuthorizationHelper;


public partial class Security_Groups : ZentityBasePage
{
    #region Private Fields
    const int _pageSize = 10;
    private string _resourceName = string.Empty;
    AuthenticatedToken userToken = null;
    private string Id = string.Empty;
    private Group groupInformation = new Group();
    #endregion Private Fields

    #region Protected member

    /// <summary>
    ///     Initialize the server controls on page load.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        userToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;

        if (!userToken.IsAdmin(Utility.CreateContext()))
        {
            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                                 Resources.Resources.UnauthorizedAccessException, UserResourcePermissions.Read));
        }
        if (Request.QueryString[Resources.Resources.QuerystringResourceId] != null)
            Id = Convert.ToString(Request.QueryString[Resources.Resources.QuerystringResourceId], CultureInfo.InvariantCulture);


        InitializeControls();
        ManageGroupSection();

        if (!Page.IsPostBack)
        {
            FillGroupGrid();
        }

        GroupTable.DeleteClicked += new EventHandler<ZentityGridEventArgs>(Group_DeleteClicked);
    }

    /// <summary>
    /// Event will Save or Update the Group information.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void Submit_Click(object sender, EventArgs e)
    {
        bool result;
        bool isAdded = false;
        try
        {
            result = SaveResource(Utility.CreateContext(), out isAdded);

            if (result == true)
            {
                string messgae = string.Empty;
                if (isAdded)
                {
                    messgae = Resources.Resources.LabelAddedGroupText;
                    ResetManageGroupForm();
                }
                else
                    messgae = Resources.Resources.LabelUpdatedGrouptext;

                Utility.ShowMessage(lblMessage,
                    string.Format(CultureInfo.InvariantCulture, messgae, _resourceName),
                    false);

                FillGroupGrid();
            }
            else
            {
                if (isAdded)
                    Utility.ShowMessage(lblMessage,
                        string.Format(CultureInfo.InvariantCulture, Resources.Resources.LabelErrorGroupAddText, _resourceName),
                        true);
                else
                    Utility.ShowMessage(lblMessage,
                        string.Format(CultureInfo.InvariantCulture, Resources.Resources.LabelErrorGroupUpdateText, _resourceName),
                        true);

            }
        }
        catch (Exception ex)
        {
            Utility.ShowMessage(lblMessage, ex.Message, true);
            return;
        }
        lblMessage.Visible = true;
    }

    /// <summary>
    /// Delete the selected groups from DB.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void Group_DeleteClicked(object sender, ZentityGridEventArgs e)
    {
        if (e.EntityIdList != null && e.EntityIdList.Count > 0)
        {
            using (ResourceDataAccess resourceDAL = new ResourceDataAccess(Utility.CreateContext()))
            {
                Utility.ShowMessage(GridErrorLabel, resourceDAL.DeleteGroups(e.EntityIdList, userToken), true);
            }

            FillGroupGrid();
        }
    }

    /// <summary>
    /// Paging event for the groupList.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void GroupTable_PageChanged(object sender, EventArgs e)
    {
        FillGroupGrid();
    }

    /// <summary>
    /// Reset the form.    
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void btnReset_Click(object sender, EventArgs e)
    {
        Response.Redirect(Resources.Resources.GroupsPage);
    }

    #endregion Protected members

    #region Private members

    /// <summary>
    /// Initialized the user assignment user control.
    /// </summary>
    private void InitializeControls()
    {
        Group group = null;
        if (!Page.IsPostBack)
        {
            groupAssignment.ClearGroupList();

        }

        groupAssignment.ControlMode = UserControls_GroupAssignment.RoleType.User;
        groupAssignment.UserSearchButton = Resources.Resources.ButtonSearchText;

        if (!string.IsNullOrEmpty(Id))
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
            {
                group = (Group)dataAccess.GetResource(new Guid(Id));

                if (group == null)
                    return;
                group.Identities.Load();
                if (group.Identities != null)
                    groupAssignment.SelectedIdentityList = group.Identities.ToList();
            }
        }

        if (!string.IsNullOrEmpty(Id))
        {
            if (group.GroupName == UserManager.AllUsersGroupName)
            {
                groupInfoPanel.Enabled = false;
                groupAssignment.IsEnable = false;
                groupAssignment.SelectedUserOrGroupType = UserControls_GroupAssignment.SelectedType.AllUsersGroupName;
            }
            else if (group.GroupName == UserManager.AdminGroupName)
            {
                groupInfoPanel.Enabled = false;
                groupAssignment.IsEnable = true;
                groupAssignment.SelectedUserOrGroupType = UserControls_GroupAssignment.SelectedType.AdminGroupName;
            }
        }
        else
        {
            groupInfoPanel.Enabled = true;
            groupAssignment.IsEnable = true;
        }

        Utility.ShowMessage(lblMessage, string.Empty, false);
    }

    /// <summary>
    /// Manage the group information section while updating group information.
    /// </summary>
    private void ManageGroupSection()
    {
        if (!string.IsNullOrEmpty(Id))
        {
            txtGroupName.Enabled = false;
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
            {
                //Populate group info on UI
                Group groupObject = (Group)dataAccess.GetResource(new Guid(Id));

                if (groupObject == null)
                    return;

                groupInformation = dataAccess.GetGroup(groupObject.GroupName);

                if (!Page.IsPostBack)
                {
                    txtTitle.Text = groupInformation.Title;
                    txtGroupName.Text = groupInformation.GroupName;
                    txtDescription.Text = groupInformation.Description;
                    txtUri.Text = groupInformation.Uri;
                }
            }
        }
    }

    /// <summary>
    /// Reset all control on the group managment page.
    /// </summary>
    private void ResetManageGroupForm()
    {
        groupAssignment.DeSelectAllItem();
        txtGroupName.Text = string.Empty;
        txtTitle.Text = string.Empty;
        txtDescription.Text = string.Empty;
        txtUri.Text = string.Empty;
    }

    /// <summary>
    /// Add or Update group information.
    /// </summary>
    /// <param name="context">Zentity Context object</param>
    /// <param name="isAdded">Flag for the specify group is added or updated</param>
    /// <returns></returns>
    private bool SaveResource(ZentityContext context, out bool isAdded)
    {
        bool result = false;
        isAdded = true;
        GetGroupInformation(groupInformation);

        List<String> seletcedList = groupAssignment.GetSelectGroup();

        _resourceName = groupInformation.GroupName;
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(context))
        {
            if (string.IsNullOrEmpty(Id))
            {
                result = dataAccess.CreateGroup(groupInformation, seletcedList, userToken);

                foreach (String identityId in seletcedList)
                {
                    Identity identity = (Identity)dataAccess.GetResource(new Guid(identityId));
                    result = dataAccess.AddIdentityToGroup(groupInformation, identity, userToken);
                }
                isAdded = true;
            }
            else
            {
                if (groupInfoPanel.Enabled)
                    result = dataAccess.UpdateGroup(groupInformation, userToken);


                //result = dataAccess.RemoveIdentityFromGroup(new Guid(Id));
                if (groupAssignment.IsEnable)
                {
                    Group group = (Group)dataAccess.GetResource(new Guid(Id));
                    group.Identities.Load();

                    List<string> existingIdentities = group.Identities
                        .Select(identity => identity.Id.ToString())
                        .ToList();

                    List<String> allSearchList = groupAssignment.GetSearchList();
                    foreach (string exsitingId in existingIdentities)
                    {
                        if (!seletcedList.Contains(exsitingId))
                        {
                            if (allSearchList.Contains(exsitingId))
                            {
                                Identity identity = (Identity)dataAccess.GetResource(new Guid(exsitingId));
                                result = dataAccess.RemoveIdentityFromGroup(identity, group, userToken);
                            }
                        }
                    }

                    foreach (string selectedId in seletcedList)
                    {
                        if (!existingIdentities.Contains(selectedId))
                        {
                            Identity identity = (Identity)dataAccess.GetResource(new Guid(selectedId));
                            result = dataAccess.AddIdentityToGroup(groupInformation, identity, userToken);
                        }
                    }
                }
                isAdded = false;
            }
        }

        return result;
    }

    /// <summary>
    /// Get group information entered in UI by user.
    /// </summary>
    /// <param name="groupInformation">Group object</param>
    private void GetGroupInformation(Group groupInformation)
    {
        if (groupInformation == null)
        {
            groupInformation = new Group();
        }

        groupInformation.Title = txtTitle.Text.Trim();
        groupInformation.GroupName = txtGroupName.Text.Trim();
        groupInformation.Uri = txtUri.Text.Trim();
        groupInformation.Description = txtDescription.Text.Trim();


    }

    /// <summary>
    /// Populate group grid with paging.
    /// </summary>
    private void FillGroupGrid()
    {
        int totalRecords = 0;
        int fetchedRecords = _pageSize * GroupTable.PageIndex;
        GroupTable.PageSize = _pageSize;

        List<Group> groupList = new List<Group>();
        using (ZentityContext context = Utility.CreateContext())
        {
            IEnumerable<Group> groups = UserManager.GetAllGroups(context).OrderBy(tuple => tuple.GroupName);
            totalRecords = groups.Count();
            UpdatePageCount(GroupTable, totalRecords);
            if (totalRecords > 0)
            {
                int lastPage = GroupTable.PageCount - 1;
                if (GroupTable.PageIndex > lastPage)
                {
                    // If requested page is not found, show last page.
                    GroupTable.PageIndex = lastPage;
                    fetchedRecords = _pageSize * GroupTable.PageIndex;
                }
                groupList = context.Resources.OfType<Group>().OrderBy(tuple => tuple.GroupName).Skip(fetchedRecords).Take(_pageSize).ToList();
            }
        }

        if (groupList.Count > 0)
        {
            Utility.UpdateResourcesEmptyTitle(groupList);
            GroupTable.DataSource = groupList;
            GroupTable.DataBind();
        }
    }

    /// <summary>
    /// Update page count for paging.
    /// </summary>
    /// <param name="grdView">GridView object</param>
    /// <param name="totalRecords">Total result records</param>
    private void UpdatePageCount(ZentityDataGridView grdView, int totalRecords)
    {
        //Update page count
        if (totalRecords > 0)
        {
            if (_pageSize > 0 && totalRecords > _pageSize)
            {
                grdView.PageCount =
                    Convert.ToInt32(Math.Ceiling((double)totalRecords / _pageSize));
            }
            else
            {
                grdView.PageCount = 1;
            }
        }
        else
        {
            grdView.PageCount = 0;
        }
    }

    #endregion Private members
}
