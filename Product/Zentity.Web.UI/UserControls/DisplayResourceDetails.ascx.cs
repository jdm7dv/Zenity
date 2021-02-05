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
using Zentity.Security.Authentication;
using System.Collections;
using System.Globalization;
using Zentity.Core;
using Zentity.ScholarlyWorks;

public partial class UserControls_DisplayResourceDetails : System.Web.UI.UserControl
{
    #region Member variables

    #region Private

    Guid _resourceId = Guid.Empty;

    #endregion

    #endregion

    #region Properties

    #region Public

    public Guid ResourceId
    {
        get
        {
            return _resourceId;
        }
        set
        {
            _resourceId = value;
        }
    }

    #endregion

    #endregion

    #region Events

    #region Public

    public event EventHandler<EventArgs> OnSuccessfulDelete;

    #endregion

    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        //Set authenticatedToken to resource search control
        ResourceDetailView.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;

        if (!IsPostBack)
        {
            DeleteButton.Attributes.Add("OnClick", "javascript: return confirm('" + Resources.Resources.ConfirmDeleteResource + "');");
            ResourceDetailView.ResourceId = ResourceId;

            if (ResourceDetailView.ResourceId != Guid.Empty && ResourceDetailView.AuthenticatedToken != null)
            {
                //Check whether use is authorize to delete the resource. If not then make Delete buttion invisible.
                using (ResourceDataAccess dataAccess = new ResourceDataAccess())
                {
                    if (!dataAccess.AuthorizeUser(ResourceDetailView.AuthenticatedToken, UserResourcePermissions.Delete, ResourceDetailView.ResourceId))
                    {
                        DeleteButton.Visible = false;
                    }
                }
            }
            else
            {
                DeleteButton.Visible = false;
            }
        }
    }

    /// <summary>
    /// Delete given resource from database.
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">Event Args</param>
    protected void DeleteButton_OnClick(object sender, EventArgs e)
    {
        using (ResourceDataAccess dataAccess = new ResourceDataAccess())
        {
            if (ResourceDetailView.ResourceId != Guid.Empty)
            {
                Resource resource = dataAccess.GetResource(ResourceDetailView.ResourceId);
                bool isAuthorized = resource is CategoryNode ?
                          dataAccess.AuthorizeUserForDeletePermissionOnCategory(ResourceDetailView.AuthenticatedToken, resource.Id) :
                          dataAccess.AuthorizeUser(ResourceDetailView.AuthenticatedToken, UserResourcePermissions.Delete, resource.Id);

                if (isAuthorized)
                {
                    bool isDeleted = resource is CategoryNode ?
                        dataAccess.DeleteCategoryNodeWithHierarchy(ResourceDetailView.ResourceId) :
                        dataAccess.DeleteResource(ResourceDetailView.ResourceId);
                    //Delete resource
                    if (isDeleted)
                    {
                        DeleteButton.Visible = false;
                        ResourceDetailView.Visible = false;
                        //Show Delete successful message
                        MessageLabel.ForeColor = System.Drawing.Color.Black;
                        MessageLabel.Text = Resources.Resources.AlertRecordDelerted;
                        if (OnSuccessfulDelete != null)
                        {
                            OnSuccessfulDelete(this, new EventArgs());
                        }
                    }
                    else
                    {
                        //Show delete failure message
                        MessageLabel.ForeColor = System.Drawing.Color.Red;
                        MessageLabel.Text = Resources.Resources.AlertResourceDelertedError;
                    }
                }
                else
                {
                    //Show delete failure message
                    MessageLabel.ForeColor = System.Drawing.Color.Red;
                    if (resource is CategoryNode)
                        MessageLabel.Text = Resources.Resources.MsgUnauthorizeAccessDeleteCategory;
                    else
                        MessageLabel.Text = string.Format(CultureInfo.InvariantCulture, Resources.Resources.MsgUnAuthorizeAccess,
                            UserResourcePermissions.Delete);
                }
            }
        }
    }
}
