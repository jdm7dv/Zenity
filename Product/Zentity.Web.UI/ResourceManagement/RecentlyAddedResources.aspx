<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="ResourceManagement_RecentlyAddedResources" Codebehind="RecentlyAddedResources.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <Zentity:ReportingView ID="ReportingViewDateAdded" runat="server" Title="<%$ Resources:Resources, TitleRecentlyAddedResources %>"
            TitleStyle-CssClass="TitleStyle" ContainerStyle-CssClass="ResourceListViewContainer"
            ItemStyle-CssClass="ResourceListViewItemContainer" SeparatorStyle-CssClass="ResourceListViewInternalDynDiv"
            ViewUrl="ManageResource.aspx?ActiveTab=Summary&Id={0}" ShowReportType="RecentlyAddedResources"
            IsSecurityAwareControl="true">
        </Zentity:ReportingView>
    </div>
</asp:Content>
