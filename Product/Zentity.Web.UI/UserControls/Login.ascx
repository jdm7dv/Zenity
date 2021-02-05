<%@ Control Language="C#" AutoEventWireup="true" Inherits="UserControls_Login" Codebehind="Login.ascx.cs" %>
<asp:Panel ID="LoginPanel" CssClass="LoginPanel" runat="server" DefaultButton="LoginButton">
    <div>
        <asp:Label ID="UserNameLabel" Text="UserName" runat="server" ForeColor="White"></asp:Label>
        <asp:TextBox ID="UserNameTextBox" runat="server"></asp:TextBox>
    </div>
    <div>
        <asp:Label ID="PasswordLabel" Text="Password" runat="server" ForeColor="White"></asp:Label>
        <asp:TextBox ID="PasswordTextBox" runat="server" TextMode="Password"></asp:TextBox>
    </div>
    <div>
        <asp:RequiredFieldValidator ID="UserNameRequiredFieldValidator" ControlToValidate="UserNameTextBox"
            runat="server" ErrorMessage="<%$ Resources:Resources, MsgEnterUserName %>" ForeColor="Red"
            ValidationGroup="login" Display="Dynamic"></asp:RequiredFieldValidator>
        <asp:RequiredFieldValidator ID="PasswordRequiredFieldValidator" ControlToValidate="PasswordTextBox"
            runat="server" ErrorMessage="<%$ Resources:Resources, MsgEnterPassword %>" ForeColor="Red"
            ValidationGroup="login" Display="Dynamic"></asp:RequiredFieldValidator>
        <asp:Label ID="ErrorMessageLabel" runat="server" ForeColor="Red" EnableViewState="false"></asp:Label>
        <asp:Button ID="LoginButton" runat="server" ValidationGroup="login" Text="<%$ Resources:Resources, ButtonLogin %>"
            OnClick="LoginButton_Click" Width="40px" />
    </div>
    &nbsp;&nbsp;&nbsp;&nbsp;
</asp:Panel>
<asp:Panel ID="LogoutPanel" CssClass="LoginPanel" runat="server" DefaultButton="LogoutButton">
    <asp:Label ID="LoggedInUserLabel" ForeColor="White" runat="server" EnableViewState="true"></asp:Label>
    <asp:Button ID="LogoutButton" runat="server" Text="<%$ Resources:Resources, ButtonLogout %>"
        OnClick="LogoutButton_Click" Width="50px" />
    &nbsp;&nbsp;&nbsp;&nbsp;
    <br />
    <br />
    <asp:HyperLink ID="ChangePasswordHyperLink" runat="server" Text="Change Password"
        NavigateUrl="~/Security/ChangePassword.aspx" />
    &nbsp;&nbsp;&nbsp;&nbsp;
</asp:Panel>
