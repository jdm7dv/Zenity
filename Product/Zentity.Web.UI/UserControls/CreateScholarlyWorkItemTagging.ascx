<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_CreateScholarlyWorkItemTagging" Codebehind="CreateScholarlyWorkItemTagging.ascx.cs" %>
<div>
    <div>
        <asp:Label ID="AssociationErrorLabel" runat="server" Text="" Visible="false"></asp:Label>
    </div>
    <div>
        <asp:Label ID="errorMessage" runat="server" Visible="false" Text="">
        </asp:Label>
    </div>
    <div>
        <Zentity:ScholarlyWorkItemTagging ID="association1" AssociationListHeight="150px"
            AssociationListWidth="250px" runat="server" ValidationGroup="Submit" IsSecurityAwareControl="true">
            <MoveRightButton Text=">>" />
            <MoveLeftButton Text="<<" />
            <TitleStyle CssClass="ManageTabTitleStyle" />
            <LabelStyle CssClass="lStyle" />
            <ButtonStyle CssClass="Masterbutton" />
            <FilterCriteriaRowStyle CssClass="rStyle" />
            <FilterCriteriaAlternatingRowStyle CssClass="arStyle" />
        </Zentity:ScholarlyWorkItemTagging>
    </div>
    <div style="text-align: center">
        <asp:Button ID="SaveButton" runat="server" ValidationGroup="Submit" OnClick="SaveButton_Click"
            Text="Save" />
    </div>
</div>
