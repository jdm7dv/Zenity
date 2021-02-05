<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_ManageCategories" Title="<%$ Resources:Resources, ManageCategoryTitle %>"
    MaintainScrollPositionOnPostback="true" Codebehind="ManageCategories.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <div>
            <Zentity:CategoryHierarchy ID="categoryHierarchy" runat="server" TreeViewCaption="<%$ Resources:Resources, ManageCategoryTitle %>"
                Width="100%" TreeViewHeader="Header Row" TreeNodeNavigationUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}"
                DefaultContextButtonStyle="ContextButton" SelectedContextButtonStyle="HighLightButton"
                IsSecurityAwareControl="true">
                <ResourcePropertyRowStyle CssClass="rStyle" />
                <ResourcePropertyAlternateRowStyle CssClass="arStyle" />
                <ButtonStyle CssClass="Masterbutton" />
                <ResourcePropertyTitleStyle CssClass="ResourcePropertiesTitleStyle" />
                <LeftContainerStyle CssClass="CategoryHierarchyLeftContainer" />
                <RightContainerStyle CssClass="CategoryHierarchyRightContainer" />
                <TreeViewStyle CssClass="CategoryHierarchyTreeView" />
                <TreeViewContainerStyle CssClass="CategoryHierarchyContainer" />
                <TreeViewCaptionStyle CssClass="TitleStyle" />
                <TreeViewNodeStyle CssClass="TreeViewNode" />
                <TreeViewSelectedNodeStyle CssClass="TreeViewSelectedNode" />
            </Zentity:CategoryHierarchy>
        </div>
    </div>
</asp:Content>
