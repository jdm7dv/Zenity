<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_BrowseViews" Codebehind="BrowsableViews.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">

    <script type="text/javascript" language="javascript">
        var additionalFiltersId = '<%= AdditionalFilters.ClientID %>';
        var additionalFiltersStatusHiddenId = '<%= AdditionalFiltersStatusHidden.ClientID %>';
        var toggleImageId = '<%= ToggleImage.ClientID %>';
        var toggleImagePathHiddenId = '<%= ToggleImagePathHidden.ClientID %>';
        var expandedImagePath = '<%= ExpandedImagePath %>';
        var collapsedImagePath = '<%= CollapsedImagePath %>';
    </script>

    <div class="RightContentInnerPane">
        <div>
            <div id="AdditionalFiltersHeader" class="CollapsibleHeader">
                <div class="FloatLeft">
                    <%= Resources.Resources.AdditionalFilters %>
                </div>
                <div class="FloatRight">
                    <asp:Image ID="ToggleImage" runat="server" CssClass="ToggleImage" onclick="javascript:ToggleDiv(additionalFiltersId, additionalFiltersStatusHiddenId, toggleImageId, toggleImagePathHiddenId, expandedImagePath, collapsedImagePath)">
                    </asp:Image>
                </div>
                <asp:HiddenField ID="AdditionalFiltersStatusHidden" runat="server" />
                <asp:HiddenField ID="ToggleImagePathHidden" runat="server" />
            </div>
            <div id="AdditionalFilters" runat="server">
                <div>
                    <asp:Panel ID="YearMonthTreePanel" runat="server" CssClass="BrowsableTreeContainer">
                        <Zentity:BrowseTreeView ID="YearMonthtree" runat="server" ExpandDepth="1" TreeViewCaption="<%$ Resources:Resources, YearsTreeTitle %>"
                            BrowseBy="BrowseByYear" IsSecurityAwareControl="true">
                            <TreeViewContainerStyle CssClass="TreeViewContainer" />
                            <TreeViewStyle CssClass="TreeViewPanel" />
                            <TreeViewSelectedNodeStyle CssClass="TreeViewSelectedNode" />
                            <TreeViewCaptionStyle CssClass="BrowseBoxTitleStyle" />
                        </Zentity:BrowseTreeView>
                    </asp:Panel>
                    <asp:Panel ID="CategoryNodeTreePanel" runat="server" CssClass="BrowsableTreeContainer">
                        <Zentity:BrowseTreeView ID="CategoryNodeTree" runat="server" ExpandDepth="1" TreeViewCaption="<%$ Resources:Resources, CategoriesTreeTitle %>"
                            BrowseBy="BrowseByCategoryHierarchy" IsSecurityAwareControl="true">
                            <TreeViewContainerStyle CssClass="TreeViewContainer" />
                            <TreeViewStyle CssClass="CategoryTreeViewPanel" />
                            <TreeViewSelectedNodeStyle CssClass="TreeViewSelectedNode" />
                            <TreeViewCaptionStyle CssClass="BrowseBoxTitleStyle" />
                        </Zentity:BrowseTreeView>
                    </asp:Panel>
                    <asp:Panel ID="ResourceTypeTreePanel" runat="server" CssClass="BrowsableTreeContainer">
                        <Zentity:BrowseTreeView ID="ResourceTypeTree" runat="server" ExpandDepth="2" TreeViewCaption="<%$ Resources:Resources, ResourceTypesTreeTitle %>"
                            BrowseBy="BrowseByResourceType" IsSecurityAwareControl="true">
                            <TreeViewContainerStyle CssClass="TreeViewContainer" />
                            <TreeViewStyle CssClass="TreeViewPanel" />
                            <TreeViewSelectedNodeStyle CssClass="TreeViewSelectedNode" />
                            <TreeViewCaptionStyle CssClass="BrowseBoxTitleStyle" />
                        </Zentity:BrowseTreeView>
                    </asp:Panel>
                    <asp:Panel ID="AuthorsTreePanel" runat="server" CssClass="BrowsableTreeContainer">
                        <Zentity:AuthorsListView ID="AuthorsView" runat="server" Title="<%$ Resources:Resources, AuthorsTitle %>"
                            IsSecurityAwareControl="true">
                            <TitleStyle CssClass="BrowseBoxTitleStyle" />
                            <ContainerStyle CssClass="TreeViewContainer" />
                            <ButtonStyle CssClass="Masterbutton" />
                        </Zentity:AuthorsListView>
                    </asp:Panel>
                    <div style="clear: left">
                        <asp:Panel ID="filter1Panel" runat="server" DefaultButton="btnFilter">
                            <asp:Label ID="lblFilter" runat="server" Text="<%$ Resources:Resources, BrowseFilterLabelText %>"></asp:Label>
                            <asp:TextBox ID="txtFilter" runat="server" Width="300px"></asp:TextBox>
                            &nbsp; &nbsp;
                            <asp:Button ID="btnFilter" runat="server" Text="<%$ Resources:Resources, ButtonFiltersText %>"
                                OnClick="btnFilter_Click" />
                            <asp:Button ID="btnResetFilters" runat="server" Text="<%$ Resources:Resources, ButtonResetFilters %>"
                                OnClick="btnResetFilters_Click" />
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
        <div>
            <br />
            <Zentity:ResourceListView ID="ResourceListView" runat="server" Width="100%" 
                IsFeedSupported="true"
                OnOnPageChanged="SearchResourceListView_PageChanged"
                TitleStyle-CssClass="TitleStyle" 
                ContainerStyle-CssClass="ResourceListViewContainer"
                ItemStyle-CssClass="ResourceListViewItemContainer" 
                SeparatorStyle-CssClass="ResourceListViewInternalDynDiv"
                OnOnSortButtonClicked="ResourceListView_OnSortButtonClicked" 
                ViewUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}">
            </Zentity:ResourceListView>
        </div>
    </div>
</asp:Content>
