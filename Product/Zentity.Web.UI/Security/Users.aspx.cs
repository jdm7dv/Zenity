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
using System.Globalization;
using System.Text;
using Zentity.Security.AuthenticationProvider;
using Zentity.Security.Authorization;
using Zentity.Security.Authentication;
using Zentity.Security.AuthorizationHelper;
using Zentity.Core;
using Zentity.Web.UI;
using Zentity.Web.UI.ToolKit;
using System.Data;
using System.Collections;

public partial class Security_Users : ZentityBasePage
{
    private const string Identity = "IdentityViewState";
    private const string _assignmentClass = "groupAssignment";
    private string loginName = string.Empty;
    private ZentityUser currentUser = null;
    private ZentityUserProfile currentUserProfile = null;
    const int _pageSize = 10;
    string id = string.Empty;
    Identity identity = null;
    private AuthenticatedToken userToken = null;

    private enum AccountStatus
    {
        Active,
        InActive
    }

    #region Protected member

    /// <summary>
    /// Initialize the server controls on page load.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void Page_Load(object sender, EventArgs e)
    {

        userToken = (AuthenticatedToken)Session[Constants.AuthenticationTokenKey];

        if (!userToken.IsAdmin(Utility.CreateContext()))
        {
            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                Resources.Resources.UnauthorizedAccessException, UserResourcePermissions.Read));
        }

        id = Convert.ToString(Request.QueryString[Resources.Resources.QuerystringResourceId], CultureInfo.InvariantCulture);
        if (id != null)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
            {
                identity = (Identity)dataAccess.GetResource(new Guid(id));
                identity.Groups.Load();
                loginName = identity.IdentityName;
            }
        }

        InitializeControls();

        if (!string.IsNullOrEmpty(loginName) && !Page.IsPostBack)
        {
            ZentityUserAdmin adminObject = new ZentityUserAdmin(userToken);
            currentUserProfile = adminObject.GetUserProfile(loginName);

            txtLoginName.Text = loginName;
            txtLoginName.Enabled = false;
            passwordRow.Visible = false;
            reEnterPasswordRow.Visible = false;
            securityQuesRow.Visible = false;
            answerRow.Visible = false;

            txtFirstName.Text = currentUserProfile.FirstName;
            txtMiddlename.Text = currentUserProfile.MiddleName;
            txtLastName.Text = currentUserProfile.LastName;
            txtEmail.Text = currentUserProfile.Email;
            txtCity.Text = currentUserProfile.City;
            txtState.Text = currentUserProfile.State;
            txtCountry.Text = currentUserProfile.Country;
            if (currentUserProfile.AccountStatus == AccountStatus.Active.ToString())
                chkAccountStatus.Checked = true;
            else if (currentUserProfile.AccountStatus == AccountStatus.InActive.ToString())
                chkAccountStatus.Checked = false;

            currentUser = new ZentityUser(loginName, userToken, currentUserProfile);
        }
        else
        {
            if (!string.IsNullOrEmpty(loginName) && Page.IsPostBack)
            {
                ZentityUserAdmin adminObject = new ZentityUserAdmin(userToken);
                currentUserProfile = adminObject.GetUserProfile(loginName);
                currentUser = new ZentityUser(loginName, userToken, currentUserProfile);
            }
            else
            {
                currentUserProfile = new ZentityUserProfile();
            }
        }

        if (!Page.IsPostBack)
            FillUserGrid();

        UserTable.DeleteClicked += new EventHandler<ZentityGridEventArgs>(UserTable_DeleteClicked);
    }

    /// <summary>
    /// Delete the selected users from DB.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void UserTable_DeleteClicked(object sender, ZentityGridEventArgs e)
    {
        if (e.EntityIdList != null && e.EntityIdList.Count > 0)
        {
            using (ResourceDataAccess resourceDAL = new ResourceDataAccess(Utility.CreateContext()))
            {
                Utility.ShowMessage(GridErrorLabel, resourceDAL.DeleteUsers(e.EntityIdList, userToken), true);
            }

            FillUserGrid();

            if (!string.IsNullOrEmpty(txtLoginName.Text.Trim()))
            {
                ResetRegistrationForm();
                Response.Redirect(Resources.Resources.UsersPage);
            }
        }
    }

    /// <summary>
    /// Reset the form.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void btnReset_Click(object sender, EventArgs e)
    {
        Response.Redirect(Resources.Resources.UsersPage);
    }

    /// <summary>
    /// Event will Save or Update the User information.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        try
        {
            bool result = false;

            GetUserInformation();
            ZentityUserAdmin adminObject = new ZentityUserAdmin(userToken);

            if (!string.IsNullOrEmpty(loginName))
            {
                currentUser = new ZentityUser(loginName, userToken, currentUserProfile);

                if (userInfoPanel.Enabled)
                    result = UserManager.UpdateUser(currentUser, userToken);

                if (groupAssignment.IsEnable)
                {
                    if (result)
                    {
                        result = adminObject.SetAccountStatus(currentUser.LogOnName, GetAccountStatus());
                        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
                        {
                            List<String> groupList = AddGroupsInIdentity();
                            List<String> allSearchList = groupAssignment.GetSearchList();

                            Identity identity = (Identity)dataAccess.GetResource(new Guid(id));
                            identity.Groups.Load();

                            List<string> existingGroups = identity.Groups
                                .Select(group => group.Id.ToString())
                                .ToList();

                            foreach (string exsitingId in existingGroups)
                            {
                                if (!groupList.Contains(exsitingId))
                                {
                                    if (allSearchList.Contains(exsitingId))
                                    {
                                        Group group = (Group)dataAccess.GetResource(new Guid(exsitingId));
                                        dataAccess.RemoveIdentityFromGroup(identity, group, userToken);
                                    }
                                }
                            }

                            foreach (string selectedId in groupList)
                            {
                                if (!existingGroups.Contains(selectedId))
                                {
                                    Group group = (Group)dataAccess.GetResource(new Guid(selectedId));
                                    result = dataAccess.AddIdentityToGroup(group, identity, userToken);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
                {
                    result = dataAccess.CreateUser(currentUser, userToken);
                    if (result)
                    {
                        result = adminObject.SetAccountStatus(currentUser.LogOnName, GetAccountStatus());
                        Identity identity = UserManager.GetIdentity(currentUser.LogOnName, Utility.CreateContext());
                        List<String> groupList = AddGroupsInIdentity();
                        foreach (String groupId in groupList)
                        {
                            Group group = (Group)dataAccess.GetResource(new Guid(groupId));
                            result = dataAccess.AddIdentityToGroup(group, identity, userToken);
                        }
                    }
                }
            }

            if (!result)
            {
                if (string.IsNullOrEmpty(loginName))
                    Utility.ShowMessage(lblMessage, Resources.Resources.LabelErrorRegistrationFail, true);
                else
                    Utility.ShowMessage(lblMessage, Resources.Resources.LabelErrorUpdateUserFail, true);
            }
            else
            {
                if (!string.IsNullOrEmpty(loginName))
                {
                    Utility.ShowMessage(lblMessage,
                        string.Format(CultureInfo.InvariantCulture, Resources.Resources.LabelUserInfoUpdated, currentUser.LogOnName),
                        false);
                }
                else
                {
                    Utility.ShowMessage(lblMessage,
                        string.Format(CultureInfo.InvariantCulture, Resources.Resources.LabelRegistrationCompleted, currentUser.LogOnName),
                        false);
                    ResetRegistrationForm();
                }
                FillUserGrid();
            }

        }
        catch (Exception ex)
        {
            Utility.ShowMessage(lblMessage, ex.Message, true);
        }
    }

    /// <summary>
    /// Paging event for the user list.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="e">Event argument</param>
    protected void UserTable_PageChanged(object sender, EventArgs e)
    {
        FillUserGrid();
    }

    #endregion Protected members

    #region Private members

    /// <summary>
    /// Populate user grid with paging.
    /// </summary>
    private void FillUserGrid()
    {
        int totalRecords = 0;
        int fetchedRecords = _pageSize * UserTable.PageIndex;
        UserTable.PageSize = _pageSize;

        List<Identity> usersList = new List<Identity>();
        using (ZentityContext context = Utility.CreateContext())
        {
            IEnumerable<Identity> users = UserManager.GetAllIdentities(context).OrderBy(tuple => tuple.IdentityName);
            totalRecords = users.Count();
            UpdatePageCount(UserTable, totalRecords);
            if (totalRecords > 0)
            {
                int lastPage = UserTable.PageCount - 1;
                if (UserTable.PageIndex > lastPage)
                {
                    // If requested page is not found, show last page.
                    UserTable.PageIndex = lastPage;
                    fetchedRecords = _pageSize * UserTable.PageIndex;
                }
                usersList = context.Resources.OfType<Identity>().OrderBy(tuple => tuple.IdentityName).Skip(fetchedRecords).Take(_pageSize).ToList();
            }
        }

        if (usersList.Count > 0)
        {
            Utility.UpdateResourcesEmptyTitle(usersList);
            UserTable.DataSource = usersList;
            UserTable.DataBind();
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
                grdView.PageCount = Convert.ToInt32(Math.Ceiling((double)totalRecords / _pageSize));
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

    /// <summary>
    /// Initialized the group assignment user control.
    /// </summary>
    private void InitializeControls()
    {
        if (!IsPostBack)
        {
            groupAssignment.ClearGroupList();
        }
        if (!string.IsNullOrEmpty(loginName))
        {
            if (identity.Groups != null)
                groupAssignment.SelectedList = identity.Groups.ToList();
        }

        groupAssignment.ControlMode = UserControls_GroupAssignment.RoleType.Group;
        groupAssignment.CssClass = _assignmentClass;
        groupAssignment.GroupSearchButton = Resources.Resources.ButtonSearchText;

        if (loginName != null && (loginName == UserManager.AdminUserName || loginName == UserManager.GuestUserName))
        {
            userInfoPanel.Enabled = false;
            accountStatusPanel.Enabled = false;
            groupAssignment.IsEnable = false;

            if (loginName == UserManager.AdminUserName)
            {
                groupAssignment.SelectedUserOrGroupType = UserControls_GroupAssignment.SelectedType.AdminUserName;
            }
            else
            {
                groupAssignment.SelectedUserOrGroupType = UserControls_GroupAssignment.SelectedType.GuestUserName;
            }
        }
        else
        {
            userInfoPanel.Enabled = true;
            accountStatusPanel.Enabled = true;
            groupAssignment.IsEnable = true;
            groupAssignment.SelectedUserOrGroupType = UserControls_GroupAssignment.SelectedType.GuestUserName;
        }

        Utility.ShowMessage(lblMessage, string.Empty, false);
        Utility.ShowMessage(GridErrorLabel, string.Empty, false);
    }

    /// <summary>
    /// Returns Account status for current user.
    /// </summary>
    /// <returns>Account status value</returns>
    private string GetAccountStatus()
    {
        if (chkAccountStatus.Checked)
        {
            return AccountStatus.Active.ToString();
        }
        else
        {
            return AccountStatus.InActive.ToString();
        }
    }

    /// <summary>
    /// Reset all control on the user managment page.
    /// </summary>
    private void ResetRegistrationForm()
    {
        txtLoginName.Text = string.Empty;
        txtPassword.Text = string.Empty;
        txtSecurityQues.Text = string.Empty;
        txtAnswer.Text = string.Empty;
        txtFirstName.Text = string.Empty;
        txtMiddlename.Text = string.Empty;
        txtLastName.Text = string.Empty;
        txtEmail.Text = string.Empty;
        txtCity.Text = string.Empty;
        txtState.Text = string.Empty;
        txtCountry.Text = string.Empty;

        groupAssignment.DeSelectAllItem();
    }

    /// <summary>
    /// Get user information entered in UI by user.
    /// </summary>
    /// <param name="currentUser"></param>
    private void GetUserInformation()
    {
        currentUserProfile.FirstName = txtFirstName.Text.Trim();

        currentUserProfile.MiddleName = txtMiddlename.Text;
        currentUserProfile.LastName = txtLastName.Text;
        currentUserProfile.Email = txtEmail.Text;
        if (!string.IsNullOrEmpty(txtSecurityQues.Text.Trim()))
        {
            currentUserProfile.SecurityQuestion = txtSecurityQues.Text.Trim();
        }
        if (!string.IsNullOrEmpty(txtAnswer.Text.Trim()))
        {
            currentUserProfile.Answer = txtAnswer.Text;
        }

        currentUserProfile.City = txtCity.Text;
        currentUserProfile.State = txtState.Text;
        currentUserProfile.Country = txtCountry.Text;

        if (!string.IsNullOrEmpty(txtLoginName.Text.Trim())
            && !string.IsNullOrEmpty(txtPassword.Text.Trim()))
        {
            currentUser = new ZentityUser(txtLoginName.Text.Trim(),
                txtPassword.Text.Trim(), currentUserProfile);
        }
    }

    /// <summary>
    /// Get groups to be added in identity.
    /// </summary>
    /// <returns>Identity id list</returns>
    private List<String> AddGroupsInIdentity()
    {
        return groupAssignment.GetSelectGroup();
    }

    #endregion Private members
}
