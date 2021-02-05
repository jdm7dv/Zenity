<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="Security_ChangePassword" Codebehind="ChangePassword.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <table width="100%">
        <tr>
            <td class="MasterContentHEAD">
                <%= Resources.Resources.ChangePasswordText %>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Panel ID="ChangePasswordPanel" runat="server" DefaultButton="ChangePasswordButton">
                    <table width="100%">
                        <tr>
                            <td style="width: 30%">
                                <asp:Label ID="CurrentPasswordLabel" runat="server" Text="<%$ Resources:Resources, LabelPasswordText %>"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="CurrentPasswordTextBox" runat="server" TextMode="Password"></asp:TextBox>
                                &nbsp;
                                <asp:RequiredFieldValidator ID="CurrentPasswordRequired" runat="server" ControlToValidate="CurrentPasswordTextBox"
                                    ErrorMessage="<%$ Resources:Resources, RequiredText %>" ValidationGroup="ChangePassword1"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="NewPasswordLabel" runat="server" Text="<%$ Resources:Resources, LabelNewPasswordText %>"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="NewPasswordTextBox" runat="server" TextMode="Password"></asp:TextBox>
                                &nbsp;
                                <asp:RequiredFieldValidator ID="NewPasswordRequired" runat="server" ControlToValidate="NewPasswordTextBox"
                                    ErrorMessage="<%$ Resources:Resources, RequiredText %>" ValidationGroup="ChangePassword1"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="ConfirmNewPasswordLabel" runat="server" Text="<%$ Resources:Resources, LabelConfirmNewPasswordText %>"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="ConfirmNewPasswordTextBox" runat="server" TextMode="Password"></asp:TextBox>
                                &nbsp;
                                <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" ControlToValidate="ConfirmNewPasswordTextBox"
                                    ErrorMessage="<%$ Resources:Resources, RequiredText %>" ValidationGroup="ChangePassword1"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:CompareValidator ID="NewPasswordCompare" runat="server" ControlToCompare="NewPasswordTextBox"
                                    ControlToValidate="ConfirmNewPasswordTextBox" ErrorMessage="<%$ Resources:Resources, CompareNewPasswordError %>"
                                    ValidationGroup="ChangePassword1"></asp:CompareValidator>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Button ID="ChangePasswordButton" runat="server" Text="<%$ Resources:Resources, ChangePasswordText %>"
                                    ValidationGroup="ChangePassword1" OnClick="ChangePasswordButton_Click" />
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="StatusLabel" runat="server" EnableViewState="False"></asp:Label>
            </td>
        </tr>
    </table>
</asp:Content>
