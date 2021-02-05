<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_DisplayResourceDetails" Codebehind="DisplayResourceDetails.ascx.cs" %>
<div style="text-align: center">
    <asp:Label ID="MessageLabel" runat="server" Text=""></asp:Label>
</div>
<div>
    <Zentity:ResourceDetailView ID="ResourceDetailView" runat="server" ViewUrl="~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=Summary"
        ViewChangeHistoryUrl="~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=ChangeHistory"
        BibTexImportUrl="~/Services/BibTeXImport.aspx?id={0}" EditTagAssociationUrl="~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=Tags"
        EditCategoryAssociationUrl="~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=Categories"
        IsSecurityAwareControl="true">
        <ResourceInfoTitleStyle CssClass="ManageTabTitleStyle" />
        <ResourceInfoContainerStyle CssClass="ResourceInfoStyle" />
        <RelatedResourcesTitleStyle CssClass="ManageTabTitleStyle" />
        <RelatedResourcesContainerStyle CssClass="RelatedResourcesStyle" />
    </Zentity:ResourceDetailView>
</div>
<div style="text-align: center">
    <asp:Button ID="DeleteButton" runat="server" Text="Delete" OnClick="DeleteButton_OnClick" />
</div>
