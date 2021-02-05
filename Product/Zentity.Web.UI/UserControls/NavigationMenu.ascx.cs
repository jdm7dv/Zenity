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
using Zentity.Web.UI.ToolKit;
using Zentity.Security.Authentication;
using System.Globalization;

public partial class UserControls_NavigationMenu : System.Web.UI.UserControl
{
    #region Member Variables
    ZentityBase _selectedTreeView;
    #endregion

    #region Constants

    const string _BrowseByYear = "BrowseByYear";
    const string _BrowseByCategoryHierarchy = "BrowseByCategoryHierarchy";
    const string _BrowseByResourceType = "BrowseByResourceType";
    const string _BrowseByAuthors = "BrowseByAuthors";
    const string _browseViewKey = "BrowseView";
    const string _onClickEvent = "onclick";
    const string _redirectToSearchPageFunction = "javascript: RedirectToSearchPage('{0}', '{1}'); return false;";
    const string _searchPagePath = "/ResourceManagement/BasicSearch.aspx?SearchText=";
    #endregion

    #region Properties

    public ZentityBase SeletedBrowTreeView
    {
        get
        {
            return _selectedTreeView;
        }
    }

    #endregion

    #region Event Handlers

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        YearMonthtreePanel.Visible = CategoryNodeTreePanel.Visible = 
            ResourceTypeTreePanel.Visible = AuthorsTreePanel.Visible = false;

        string view = Request.QueryString[_browseViewKey];
        if (!string.IsNullOrEmpty(view))
        {
            switch (view)
            {
                case _BrowseByYear:
                    YearMonthtreePanel.Visible = true;
                    _selectedTreeView = YearMonthBrowseTree;
                    break;
                case _BrowseByCategoryHierarchy:
                    CategoryNodeTreePanel.Visible = true;
                    _selectedTreeView = CategoryNodeBrowseTree;
                    break;
                case _BrowseByResourceType:
                    ResourceTypeTreePanel.Visible = true;
                    _selectedTreeView = ResourceTypeBrowseTree;
                    break;
                case _BrowseByAuthors:
                    AuthorsTreePanel.Visible = true;
                    _selectedTreeView = AuthorsView;
                    break;
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //Set AuthenticatedToken to controls.
        AuthenticatedToken token = this.Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        YearMonthBrowseTree.AuthenticatedToken = CategoryNodeBrowseTree.AuthenticatedToken =
            ResourceTypeBrowseTree.AuthenticatedToken = AuthorsView.AuthenticatedToken = token;

        //Set base Url to HiddenUrl control.
        string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
        HiddenUrl.Value = baseUrl + _searchPagePath;

        //Add onclick event Search button
        SearchButton.Attributes.Add(_onClickEvent, string.Format(CultureInfo.InstalledUICulture, 
            _redirectToSearchPageFunction, SearchTextBox.ClientID, HiddenUrl.ClientID));
    }

    #endregion
}
