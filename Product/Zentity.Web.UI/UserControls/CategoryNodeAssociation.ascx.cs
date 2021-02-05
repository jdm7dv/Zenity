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
using Zentity.Web.UI;
using Zentity.Security.Authentication;

public partial class UserControls_CategoryNodeAssociation : System.Web.UI.UserControl
{
    #region Member variables

    #region Private

    Guid _resourceId = Guid.Empty;
    string _subjectNavigationalUrl;

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

    public string SubjectNavigationalUrl
    {
        private get
        {
            return _subjectNavigationalUrl;
        }
        set
        {
            _subjectNavigationalUrl = value;
        }
    }

    #endregion

    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        LocalizePage();

        if (ResourceId != Guid.Empty)
        {
            categoryNodeAssociation.SubjectNavigationUrl = SubjectNavigationalUrl;
            categoryNodeAssociation.ResourceItemId = ResourceId;
            categoryNodeAssociation.TreeNodeNavigationUrl = Constants.UriDisplayResourceDetailWithId;
        }

        #region Set authentication token
        categoryNodeAssociation.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        #endregion
    }

    protected void Save_Click(object sender, EventArgs e)
    {
        categoryNodeAssociation.SaveAssociation();
    }

    private void LocalizePage()
    {
        Page.Title = Resources.Resources.CategoryNodeAssociationPageTitle;
    }
}
