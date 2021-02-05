<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_CategoryNodeAssociation" Codebehind="CategoryNodeAssociation.ascx.cs" %>
<div>
    <div>
        <asp:Label ID="PageTitleLabel" runat="server" Visible="false"></asp:Label>
    </div>
    <div>
        <Zentity:CategoryNodeAssociation ID="categoryNodeAssociation" runat="server" TreeViewCaption="Associate CategoryNode"
            IsSecurityAwareControl="true">
            <TreeViewCaptionStyle CssClass="ManageTabTitleStyle" />
        </Zentity:CategoryNodeAssociation>
    </div>
    <div>
        <br />
    </div>
    <div style="text-align: center">
        <asp:Button ID="save" runat="server" Text="Save" OnClick="Save_Click" />
    </div>
    <div>
        <asp:Label ID="errorMessage" runat="server" Visible="false" Text="Invalid category node"></asp:Label>
    </div>
</div>
