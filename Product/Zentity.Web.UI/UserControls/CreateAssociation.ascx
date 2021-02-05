<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_CreateAssociation" Codebehind="CreateAssociation.ascx.cs" %>
<div>
    <div>
        <asp:Label ID="errorMessage" runat="server" Text="" EnableViewState="false">
        </asp:Label>
    </div>
    <div>
        <Zentity:PredicateAssociation ID="association1" AssociationListHeight="150px" AssociationListWidth="250px"
            runat="server" ValidationGroup="Submit" IsSecurityAwareControl="true">
            <MoveRightButton Text=">>" />
            <MoveLeftButton Text="<<" />
            <MoveUpButton Text="Up" />
            <MoveDownButton Text="Down" />
            <TitleStyle CssClass="ManageTabTitleStyle" />
            <LabelStyle CssClass="lStyle" />
            <ButtonStyle CssClass="Masterbutton" />
            <FilterCriteriaRowStyle CssClass="rStyle" />
            <FilterCriteriaAlternatingRowStyle CssClass="arStyle" />
        </Zentity:PredicateAssociation>
    </div>
    <div style="text-align: center">
        <asp:Button ID="SaveButton" runat="server" ValidationGroup="Submit" OnClick="SaveButton_Click"
            Text="Save" />
    </div>
</div>
