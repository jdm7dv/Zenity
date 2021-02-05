// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class Manage : ZentityBasePage
{
    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        LocalizePage();
    }

    #endregion

    #region Methods

    private void LocalizePage()
    {
        ManageScholarWorkLabel.Text = Resources.Resources.LabelManageScholWorkLabel;
        addScholarWorkLabel.Text = Resources.Resources.LabelAddScholWorkText;
        addScholarWorkDescriptionLabel.Text = Resources.Resources.LabelAddScholWorkDescText;
        addTagLabel.Text = Resources.Resources.LabelAddTagsText;
        addTagDescriptionLabel.Text = Resources.Resources.LabelAddTagsDescText;
        listScholarWorkLabel.Text = Resources.Resources.LabelListScholWorkText;
        listScholarWorkDescriptionLabel.Text = Resources.Resources.LabelListScholWorkDescText;
        listTagDescriptionLabel.Text = Resources.Resources.LabelListTagsDescText;
        listTagLabel.Text = Resources.Resources.LabelListTagsText;
        categoryLabel.Text = Resources.Resources.LabelCategoryText;
        categoryDescriptionLabel.Text = Resources.Resources.LabelCategoryDescText;
        manageUsersLabel.Text = Resources.Resources.LabelManageUsersText;
        manageUsersDescriptionLabel.Text = Resources.Resources.LabelManageUsersDescText;
        manageGroupsLabel.Text = Resources.Resources.LabelManagePageManageGroupsText;
        manageGroupsDescriptionLabel.Text = Resources.Resources.LabelManagePageManageGroupsDescText;
        Page.Title = Resources.Resources.ManagePageTitle;
    }

    #endregion
}
