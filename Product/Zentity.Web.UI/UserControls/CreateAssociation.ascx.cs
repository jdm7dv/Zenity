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
using Zentity.Core;
using Zentity.Web.UI;
using Zentity.Web.UI.ToolKit;
using Zentity.Security.Authentication;

public partial class UserControls_CreateAssociation : System.Web.UI.UserControl
{
    #region Member variables

    #region Private

    string _selectedPredicateId = null;
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

    public string SelectedPredicateId
    {
        get
        {
            return _selectedPredicateId;
        }
        set
        {
            _selectedPredicateId = value;
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

    protected void Page_Load(object sender, EventArgs e)
    {
        association1.SubjectItemDestinationPageUrl = _subjectNavigationalUrl;
        if (!string.IsNullOrEmpty(Request.QueryString[Constants.QueryStringSelectedPredicate]))
        {
            SelectedPredicateId = Request.QueryString[Constants.QueryStringSelectedPredicate];
            association1.SelectedPredicateId = SelectedPredicateId;
        }
        #region Set authentication token
        association1.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        #endregion
    }


    protected void Page_Init(object sender, EventArgs e)
    {
        if (ResourceId != Guid.Empty)
        {
            association1.Title = Resources.Resources.ResourceToResourceAssociation;
            Page.Title = Resources.Resources.ResourceToResourceAssociation;
            if (!string.IsNullOrEmpty(SelectedPredicateId))
            {
                association1.SelectedPredicateId = SelectedPredicateId;
            }
        }
    }

    protected void SaveButton_Click(object sender, EventArgs e)
    {
        string message = CheckAssociationList();
        if (string.IsNullOrEmpty(message))
        {
            try
            {
                bool result = association1.SaveAssociation();
                if (result)
                {
                    Utility.ShowMessage(errorMessage, Resources.Resources.AlertResourceAssociationAdded, false);
                }
                else
                {
                    Utility.ShowMessage(errorMessage, string.Format(CultureInfo.InvariantCulture,
                                Resources.Resources.AssociationUpdateFailure, "ResourceToResource"), true);
                }
            }
            catch (AssociationException ex)
            {
                Utility.ShowMessage(errorMessage, ex.Message, true);
            }
        }
        else
        {
            Utility.ShowMessage(errorMessage, message, true);
        }
    }

    #endregion

    #region Methods

    #region Private

    private string CheckAssociationList()
    {
        string message = null;

        if ((association1.SourceList == null || association1.DestinationList == null) ||
            (association1.SourceList.Count < 1 && association1.DestinationList.Count < 1))
        {
            message = Resources.Resources.AlertNoAssociationToSave;
        }

        return message;
    }

    #endregion

    #endregion
}