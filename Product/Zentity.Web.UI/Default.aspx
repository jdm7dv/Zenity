<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="HomeBoxContainer">
        <div class="HomeBoxContent">
            <div class="HomeBoxSection">
                <div class="HomeBoxBrowseHeader">
                    <asp:HyperLink ID="BrowseHyperLink" CssClass="HomeBoxHeaderLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowse  %>"
                        NavigateUrl="~/ResourceManagement/Browse.aspx"></asp:HyperLink>
                </div>
                <div class="HomeBoxBrowseContent">
                    <br />
                    <asp:Label ID="BrowseSummaryLabel" CssClass="HomeBoxSummary" runat="server" Text="<%$ Resources:Resources, ZentityBrowseSummary %>"></asp:Label>
                    <br />
                    <br />
                    <asp:HyperLink ID="BrowseYearHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByYear %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByYear"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="BrowseCategoryLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByCategory %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByCategoryHierarchy"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="BrowseResourceTypeHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByResType %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByResourceType"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="BrowseAuthorHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByAuthor %>"
                        NavigateUrl="~/ResourceManagement/BrowsableViews.aspx?BrowseView=BrowseByAuthors"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="RecentlyAddedResourcesHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseRecentlyAddedResource %>"
                        NavigateUrl="~/ResourceManagement/RecentlyAddedResources.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="TopAuthorsHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityBrowseTopAuthors %>"
                        NavigateUrl="~/ResourceManagement/TopAuthors.aspx"></asp:HyperLink>
                </div>
            </div>
            <div class="HomeBoxSection">
                <div class="HomeBoxSearchHeader">
                    <asp:HyperLink ID="SearchHyperLink" CssClass="HomeBoxHeaderLink" runat="server" Text="<%$ Resources:Resources,ZentitySearch %>"
                        NavigateUrl="~/ResourceManagement/Search.aspx">
                    </asp:HyperLink>
                </div>
                <div class="HomeBoxSearchContent">
                    <br />
                    <asp:Label ID="SearchDetailLabel" CssClass="HomeBoxSummary" runat="server" Text="<%$ Resources:Resources, ZentitySearchSummary %>"></asp:Label>
                    <br />
                    <br />
                    <asp:Panel ID="SearchPanel" runat="server" DefaultButton="SearchButton">
                        <asp:TextBox ID="SearchTextBox" runat="server"></asp:TextBox>
                        <asp:Button ID="SearchButton" runat="server" Text="<%$ Resources:Resources, ButtonGoText %>"
                            OnClick="SearchButton_Click"></asp:Button>
                        <br />
                        <asp:Label ID="SampleSearchLabel" runat="server" Text="<%$ Resources:Resources, ZentitySearchExample %>"
                            CssClass="HomeSearchExample"></asp:Label>
                        <br />
                    </asp:Panel>
                </div>
            </div>
            <div class="HomeBoxSection">
                <div class="HomeBoxHelpHeader">
                    <asp:HyperLink ID="HelpHyperLink" CssClass="HomeBoxHeaderLink" runat="server" Text="<%$ Resources:Resources, ZentityHelp %>"
                        NavigateUrl="~/Help/Help.aspx">
                    </asp:HyperLink>
                </div>
                <div class="HomeBoxHelpContent">
                    <br />
                    <asp:Label ID="HelpSummaryLabel" CssClass="HomeBoxSummary" runat="server" Text="<%$ Resources:Resources, ZentityHelpSummary %>"></asp:Label>
                    <br />
                    <br />
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
                    <br />
                </div>
            </div>
            <br />
        </div>
        <div class="HomeBoxContent">
            <div class="HomeBoxSection">
                <div class="HomeBoxManageHeader">
                    <asp:HyperLink ID="ManageHyperLink" CssClass="HomeBoxHeaderLink" runat="server" Text="<%$ Resources:Resources,ZentityManage %>"
                        NavigateUrl="~/ResourceManagement/Manage.aspx">
                    </asp:HyperLink>
                </div>
                <div class="HomeBoxManageContent">
                    <br />
                    <asp:Label ID="ManageSummaryLabel" CssClass="HomeBoxSummary" runat="server" Text="<%$ Resources:Resources, ZentityManageSummary %>"></asp:Label>
                    <br />
                    <br />
                    <asp:HyperLink ID="AddResourceHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageAddResource %>"
                        NavigateUrl="~/ResourceManagement/ManageResource.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="ResourcesHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageResources %>"
                        NavigateUrl="~/ResourceManagement/Resources.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="AddTagHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityManageAddTag %>"
                        NavigateUrl="~/ResourceManagement/ManageResource.aspx?ResourceType=Tag"></asp:HyperLink>
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
                    <br />
                </div>
            </div>
            <div class="HomeBoxSection">
                <div class="HomeBoxServicesHeader">
                    <asp:HyperLink ID="ServicesHyperLink" CssClass="HomeBoxHeaderLink" runat="server"
                        Text="<%$ Resources:Resources,ZentityServices %>" NavigateUrl="~/Help/Services.aspx">
                    </asp:HyperLink>
                </div>
                <div class="HomeBoxServicesContent">
                    <br />
                    <asp:Label ID="ServicesSummaryLabel" CssClass="HomeBoxSummary" runat="server" Text="<%$ Resources:Resources, ZentityServicesSummary %>"></asp:Label>
                    <br />
                    <br />
                    <asp:HyperLink ID="AtomPubHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityServicesAtomPub %>"
                        NavigateUrl="~/Help/Services.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="SWORDHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityServicesSWORD %>"
                        NavigateUrl="~/Help/Services.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="SyndicationHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityServicesSyndication %>"
                        NavigateUrl="~/Help/Services.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="OAIPHMHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityServicesOAIPMH %>"
                        NavigateUrl="~/Help/Services.aspx"></asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="OREHyperLink" runat="server" Text="<%$ Resources:Resources, ZentityServicesOAIORE %>"
                        NavigateUrl="~/Help/Services.aspx"></asp:HyperLink>
                    <br />
                </div>
            </div>
            <div class="HomeBoxSection">
                <div class="HomeBoxTagsHeader">
                    <asp:HyperLink ID="HyperLink6" CssClass="HomeBoxHeaderLink" runat="server" Text="<%$ Resources:Resources,ZentityTags %>"
                        NavigateUrl="ResourceManagement/TopTags.aspx">
                    </asp:HyperLink>
                </div>
                <div class="HomeBoxTagsContent">
                    <div id="sidebar" runat="server" style="height: 170px;">
                        <Zentity:TagCloud ID="TagCloudControl" runat="server" ForeColor="White" CssClass="HomeTagCloud"
                            EnableViewState="False" Title="" MaximumFontSize="22" MinimumFontSize="10" MaximumTagsToFetch="20"
                            TagClickDestinationPageUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}"
                            IsSecurityAwareControl="true">
                        </Zentity:TagCloud>
                        <br />
                    </div>
                    <table width="100%">
                        <tr>
                            <td style="text-align: right;">
                                <asp:HyperLink ID="HyperlinkMoreTags" runat="server" Text="<%$ Resources:Resources, ZentityMore %>"
                                    NavigateUrl="~/ResourceManagement/TopTags.aspx"></asp:HyperLink>
                                &nbsp;
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript" language="javascript">
        var tagCloudContainerClientID = '<%= sidebar.ClientID %>';
        AdjustTopTags(tagCloudContainerClientID, true); 
    </script>

</asp:Content>
