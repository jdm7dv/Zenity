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
using Zentity.Web.UI.ToolKit;
using System.Globalization;

public partial class UserControls_CreateScholarlyWorkItemTagging : System.Web.UI.UserControl
{
    #region Contants

    #region Private

    const string _objectTypeQueryString = "ObjectType";

    #endregion

    #endregion

    #region Member variables

    #region Private

    Guid _resourceId = Guid.Empty;
    ObjectEntityType _objectType;
    string _subjectNavigationalUrl;

    #endregion

    #endregion

    #region Properties

    #region Public

    public Guid ResourceId
    {
        get
        {
            return association1.SubjectItemId;
        }
        set
        {
            association1.SubjectItemId = value;
        }
    }

    public ObjectEntityType ObjectType
    {
        get
        {
            return _objectType;
        }
        set
        {
            _objectType = value;
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

    #region Event Handlers

    protected void Page_Init(object sender, EventArgs e)
    {
        if (ResourceId != Guid.Empty)
        {
            if (IsValidEntityType())
            {
                association1.ObjectType = _objectType;
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        association1.SubjectItemDestinationPageUrl = _subjectNavigationalUrl;
        Utility.ShowMessage(errorMessage, string.Empty, false);
        errorMessage.Visible = false;

        #region Set authentication token
        association1.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        #endregion
    }

    protected void SaveButton_Click(object sender, EventArgs e)
    {
        string message = CheckAssociatioList();
        if (string.IsNullOrEmpty(message))
        {
            bool isSuccess = association1.SaveAssociation();

            if (isSuccess)
            {
                if (_objectType == ObjectEntityType.Tag)
                {
                    message = Resources.Resources.ResourceTaggingSuccesMessage;
                }
                else
                {
                    message = Resources.Resources.ResourceCategorizationSuccesMessage;
                }
            }
            else
            {
                if (_objectType == ObjectEntityType.Tag)
                {
                    message = Resources.Resources.ResourceTaggingFailMessage;
                }
                else
                {
                    message = Resources.Resources.ResourceCategorizationFailMessage;
                }
            }
            Utility.ShowMessage(errorMessage, message, !isSuccess);
        }
        else
        {
            Utility.ShowMessage(errorMessage, message, true);
        }
    }

    #endregion

    #region Methods

    #region Private

    private string CheckAssociatioList()
    {
        string message = null;

        if ((association1.SourceList == null || association1.DestinationList == null) ||
            (association1.SourceList.Count < 1 && association1.DestinationList.Count < 1))
        {
            message = Resources.Resources.AlertNoAssociationToSave;
        }

        return message;
    }

    private bool IsValidEntityType()
    {
        bool result = true;
        if (ObjectType == ObjectEntityType.Tag)
        {
            _objectType = ObjectEntityType.Tag;
            association1.Title = Resources.Resources.ResourceTaggingTitle;
            Page.Title = Resources.Resources.ResourceTaggingTitle;
        }
        else if (ObjectType == ObjectEntityType.CategoryNode)
        {
            _objectType = ObjectEntityType.CategoryNode;
            association1.Title = Resources.Resources.ScholarlyWorkItemCategorizationTitle;
            Page.Title = Resources.Resources.ScholarlyWorkItemCategorizationTitle;
        }
        else
        {
            association1.Visible = false;
            AssociationErrorLabel.Visible = true;
            AssociationErrorLabel.Text = Resources.Resources.AssociationInvalidObjectEntity;
            AssociationErrorLabel.ForeColor = System.Drawing.Color.Red;
            SaveButton.Visible = false;
            result = false;
        }
        return result;
    }

    #endregion

    #endregion
}
