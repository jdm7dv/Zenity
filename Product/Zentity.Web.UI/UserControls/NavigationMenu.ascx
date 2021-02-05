<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_NavigationMenu" Codebehind="NavigationMenu.ascx.cs" %>

<script language="javascript" type="text/javascript">

    function RedirectToSearchPage(textBoxId, urlId) {
        var textBox = document.getElementById(textBoxId);
        var hiddenUrlControl = document.getElementById(urlId);
        if (textBox != null && textBox.value != null && hiddenUrlControl != null && hiddenUrlControl.value != null) {
            window.location = hiddenUrlControl.value + textBox.value;
        }
    }
   
</script>

<table>
    <tr>
        <td>
            <div class="SidebarBoxSection">
                <div class="SidebarBoxEmptyHeader">
                    <asp:HyperLink ID="HyperLink1" CssClass="SideBarBoxHeaderLink" runat="server" Text="<%$ Resources:Resources,ZentityHome %>"
                        NavigateUrl="~/Default.aspx"></asp:HyperLink>
                </div>
            </div>
            <div class="SidebarBoxSection">
                <div class="SidebarBoxHeader">
                    <asp:HyperLink ID="SearchHyperLink" CssClass="SideBarBoxHeaderLink" runat="server"
                        Text="<%$ Resources:Resources,ZentitySearch %>" NavigateUrl="~/ResourceManagement/Search.aspx"></asp:HyperLink>
                </div>
                <div class="SidebarBoxContent">
                    <asp:Panel ID="SearchPanel" runat="server" DefaultButton="SearchButton">
                        <div>
                            <input id="HiddenUrl" type="hidden" runat="server" />
                            <asp:TextBox ID="SearchTextBox" runat="server"></asp:TextBox>
                            <asp:Button ID="SearchButton" runat="server" Text="<%$ Resources:Resources, ButtonGoText %>">
                            </asp:Button>
                            <br />
                            <asp:Label ID="SampleSearchLabel" runat="server" Text="<%$ Resources:Resources, ZentitySearchExample %>"
                                CssClass="HomeSearchExample"></asp:Label>
                        </div>
                    </asp:Panel>
                </div>
                <div class="SidebarBoxFooter">
                </div>
            </div>
            <div class="SidebarBoxSection">
                <div class="SidebarBoxHeader">
                    <asp:HyperLink ID="BrowseHyperLink" CssClass="SideBarBoxHeaderLink" runat="server"
                        Text="<%$ Resources:Resources, ZentityBrowse  %>" NavigateUrl="~/ResourceManagement/Browse.aspx"></asp:HyperLink>
                </div>
                <div class="SidebarBoxContent">
                    <asp:HyperLink ID="BrowseYearHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByYear %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByYear"></asp:HyperLink>
                    <asp:Panel ID="YearMonthtreePanel" runat="server" Visible="false">
                        <Zentity:BrowseTreeView ID="YearMonthBrowseTree" runat="server" ExpandDepth="1" BrowseBy="BrowseByYear"
                            IsSecurityAwareControl="true">
                            <TreeViewContainerStyle CssClass="TreeViewContainer" />
                            <TreeViewStyle CssClass="TreeViewPanel" />
                            <TreeViewNodeStyle CssClass="TreeViewNode" />
                            <TreeViewSelectedNodeStyle CssClass="TreeViewSelectedNode" />
                        </Zentity:BrowseTreeView>
                    </asp:Panel>
                    <br />
                    <asp:HyperLink ID="BrowseCategoryHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByCategory %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByCategoryHierarchy"></asp:HyperLink>
                    <asp:Panel ID="CategoryNodeTreePanel" runat="server" Visible="false">
                        <Zentity:BrowseTreeView ID="CategoryNodeBrowseTree" runat="server" ExpandDepth="1"
                            BrowseBy="BrowseByCategoryHierarchy" IsSecurityAwareControl="true">
                            <TreeViewContainerStyle CssClass="TreeViewContainer" />
                            <TreeViewStyle CssClass="CategoryTreeViewPanel" />
                            <TreeViewNodeStyle CssClass="TreeViewNode" />
                            <TreeViewSelectedNodeStyle CssClass="TreeViewSelectedNode" />
                        </Zentity:BrowseTreeView>
                    </asp:Panel>
                    <br />
                    <asp:HyperLink ID="BrowseResourceTypeHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByResType %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByResourceType"></asp:HyperLink>
                    <asp:Panel ID="ResourceTypeTreePanel" runat="server" Visible="false">
                        <Zentity:BrowseTreeView ID="ResourceTypeBrowseTree" runat="server" ExpandDepth="2"
                            BrowseBy="BrowseByResourceType" IsSecurityAwareControl="true">
                            <TreeViewContainerStyle CssClass="TreeViewContainer" />
                            <TreeViewStyle CssClass="TreeViewPanel" />
                            <TreeViewNodeStyle CssClass="TreeViewNode" />
                            <TreeViewSelectedNodeStyle CssClass="TreeViewSelectedNode" />
                        </Zentity:BrowseTreeView>
                    </asp:Panel>
                    <br />
                    <asp:HyperLink ID="BrowseAuthorHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByAuthor %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByAuthors"></asp:HyperLink>
                    <asp:Panel ID="AuthorsTreePanel" runat="server" Visible="false">
                        <Zentity:AuthorsListView ID="AuthorsView" runat="server" IsSecurityAwareControl="true">
                            <ContainerStyle CssClass="TreeViewContainer" />
                            <ButtonStyle CssClass="Masterbutton" />
                        </Zentity:AuthorsListView>
                    </asp:Panel>
                    <br />
                    <asp:HyperLink ID="RecentlyAddedResourcesHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseRecentlyAddedResource %>"
                        NavigateUrl="~/ResourceManagement/RecentlyAddedResources.aspx"><br /></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="TopAuthorsHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseTopAuthors %>"
                        NavigateUrl="~/ResourceManagement/TopAuthors.aspx"></asp:HyperLink>
                </div>
                <div class="SidebarBoxFooter">
                </div>
            </div>
            <div class="SidebarBoxSection">
                <div class="SidebarBoxHeader">
                    <asp:HyperLink ID="ManageHyperLink" CssClass="SideBarBoxHeaderLink" runat="server"
                        Text="<%$ Resources:Resources,ZentityManage %>" NavigateUrl="~/ResourceManagement/Manage.aspx">
                    </asp:HyperLink>
                </div>
                <div class="SidebarBoxContent">
                    <asp:HyperLink ID="AddResourceHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageAddResource %>"
                        NavigateUrl="~/ResourceManagement/ManageResource.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="AddTagHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageAddTag %>"
                        NavigateUrl="~/ResourceManagement/ManageResource.aspx?ResourceType=Tag"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="ResourcesHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageResources %>"
                        NavigateUrl="~/ResourceManagement/Resources.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="TagsHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageTags %>"
                        NavigateUrl="~/ResourceManagement/Tags.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="CategoryHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageCategory %>"
                        NavigateUrl="~/ResourceManagement/ManageCategories.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="UserHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageUser %>"
                        NavigateUrl="~/Security/Users.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="GroupHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageGroup %>"
                        NavigateUrl="~/Security/Groups.aspx"></asp:HyperLink>
                </div>
                <div class="SidebarBoxFooter">
                </div>
            </div>
            <div class="SidebarBoxSection">
                <div class="SidebarBoxHeader">
                    <asp:HyperLink ID="HelpHyperLink" CssClass="SideBarBoxHeaderLink" runat="server"
                        Text="<%$ Resources:Resources,ZentityHelp %>" NavigateUrl="~/Help/Help.aspx"></asp:HyperLink>
                </div>
                <div class="SidebarBoxContent">
                    <asp:HyperLink ID="GuideHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityHelpGuide %>"
                        NavigateUrl="~/Help/Help.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="ReferenceHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityHelpReference %>"
                        NavigateUrl="~/Help/Help.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="ForumsHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityHelpForums %>"
                        NavigateUrl="http://community.research.microsoft.com/forums/90.aspx" Target="_blank"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="AboutHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityHelpAbout %>"
                        NavigateUrl="~/Help/About.aspx"></asp:HyperLink>
                </div>
                <div class="SidebarBoxFooter">
                </div>
            </div>
        </td>
        <td class="SidebarSeperator">
        </td>
    </tr>
</table>
