<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_Resources" Title="" Codebehind="Resources.aspx.cs" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="mainCopy">
    <div class="RightContentInnerPane">
        <div>
            <asp:Label ID="titleLabel" runat="server" Text=""></asp:Label>
        </div>
        <div>
            <Zentity:EntitySearch ID="resourceSearch" runat="server" ViewUrl="ManageResource.aspx?ActiveTab=Summary&Id={0}"
                EnableDeleteOption="true" IsSecurityAwareControl="true">
                <ButtonStyle CssClass="Masterbutton" />
                <FilterCriteriaAlternatingRowStyle CssClass="arStyle" />
                <FilterCriteriaHeaderStyle CssClass="hStyle" />
                <FilterCriteriaRowStyle CssClass="rStyle" />
                <TitleStyle CssClass="TitleStyle" />
                <ResourceListViewTitleStyle CssClass="TitleStyle" />
                <ResourceListViewContainerStyle CssClass="ResourceListViewContainer" />
                <ResourceListViewItemStyle CssClass="ResourceListViewItemContainer" />
                <ResourceListViewSeparatorStyle CssClass="ResourceListViewInternalDynDiv" />
                <ResourceListViewButtonStyle CssClass="Masterbutton" />
            </Zentity:EntitySearch>
        </div>
        <div>
            <asp:Label ID="errorLabel" runat="server" Text="" ForeColor="Red"></asp:Label>
        </div>
    </div>
</asp:Content>
