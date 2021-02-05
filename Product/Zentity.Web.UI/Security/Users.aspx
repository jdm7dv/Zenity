<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="Security_Users" Title="User Registration" EnableEventValidation="false" Codebehind="Users.aspx.cs" %>

<%@ Register Src="~/UserControls/GroupAssignment.ascx" TagName="GroupAssignment"
    TagPrefix="ucGroupAssignment" %>
<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <table style="width: 100%;">
            <tr>
                <td colspan="3" class="MasterContentHEAD">
                    <%= Resources.Resources.LabelManageUserText %>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Panel ID="UserManagerPanel" runat="server" DefaultButton="btnSubmit">
                        <table>
                            <tr>
                                <td class="UsersBorder" style="width: 58%;">
                                    <table style="width: 100%;" cellspacing="0px">
                                        <tr>
                                            <td class="titleStyle" colspan="3">
                                                <%= Resources.Resources.LabelUserInformationText %>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Panel ID="userInfoPanel" runat="Server">
                                                    <table>
                                                        <tr class="Space">
                                                            <td style="width: 30%">
                                                                <%= Resources.Resources.LabelLoginNameText%><span class="RedText">*</span>
                                                            </td>
                                                            <td style="width: 40%">
                                                                <asp:TextBox ID="txtLoginName" runat="server"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:RequiredFieldValidator ID="requiredLoginName" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtLoginName" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="passwordRow" runat="server">
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelPasswordText %><span class="RedText">*</span>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:RequiredFieldValidator ID="requiredPassword" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtPassword" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="reEnterPasswordRow" runat="server">
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelReEnterPassword %><span class="RedText">*</span>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:TextBox ID="txtConformPassword" runat="server" TextMode="Password"></asp:TextBox>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:RequiredFieldValidator ID="requiredConformPassword" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtConformPassword" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td>
                                                            </td>
                                                            <td colspan="2">
                                                                <asp:CompareValidator ID="comparePassword" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtConformPassword" ControlToCompare="txtPassword" Type="String"
                                                                    Display="Dynamic" ErrorMessage="<%$ Resources:Resources, PasswordNotmatchMessage %>"></asp:CompareValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="securityQuesRow" runat="server">
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelSecurityQuesText %><span class="RedText">*</span>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:TextBox ID="txtSecurityQues" runat="server"></asp:TextBox>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:RequiredFieldValidator ID="requiredSecurityQues" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtSecurityQues" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="answerRow" runat="server">
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelAnswerText %><span class="RedText">*</span>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:TextBox ID="txtAnswer" runat="server"></asp:TextBox>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:RequiredFieldValidator ID="requiredAnswer" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtAnswer" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelFirstNameText %><span class="RedText">*</span>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:RequiredFieldValidator ID="requiredFirstName" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtFirstName" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelMiddleNameText %>
                                                            </td>
                                                            <td colspan="2" class="Space">
                                                                <asp:TextBox ID="txtMiddlename" runat="server"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelLastNameText %>
                                                            </td>
                                                            <td colspan="2" class="Space">
                                                                <asp:TextBox ID="txtLastName" runat="server"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelEmailText %><span class="RedText">*</span>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
                                                            </td>
                                                            <td class="Space">
                                                                <asp:RequiredFieldValidator ID="requiredEmail" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtEmail" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelCityText %>
                                                            </td>
                                                            <td colspan="2" class="Space">
                                                                <asp:TextBox ID="txtCity" runat="server"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelStateText %>
                                                            </td>
                                                            <td colspan="2" class="Space">
                                                                <asp:TextBox ID="txtState" runat="server"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="Space">
                                                                <%= Resources.Resources.LabelCountryText %>
                                                            </td>
                                                            <td colspan="2" class="Space">
                                                                <asp:TextBox ID="txtCountry" runat="server"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </asp:Panel>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>
                                    &nbsp;&nbsp;
                                </td>
                                <td valign="top" class="UsersBorder">
                                    <asp:Panel ID="accountStatusPanel" runat="Server">
                                        <div>
                                            <%= Resources.Resources.LabelAccountStatusText %>
                                            &nbsp;&nbsp;
                                            <asp:CheckBox ID="chkAccountStatus" runat="server" Text="Enabled" Checked="true" />
                                            <br />
                                            <br />
                                        </div>
                                    </asp:Panel>
                                    <ucGroupAssignment:GroupAssignment ID="groupAssignment" runat="server" />
                                </td>
                            </tr>
                            <tr style="width: 100%;">
                                <td colspan="3" align="center">
                                    <br />
                                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" ValidationGroup="Submit"
                                        OnClick="btnSubmit_Click" />
                                    <asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="btnReset_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td>
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Label ID="GridErrorLabel" runat="server" Visible="false"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="3" style="border-width: 0px;">
                <asp:Panel ID="UserManagerPanefl" runat="server">
                    <Zentity:ResourceDataGridView GridLines="None" CssClass="ResourceGridStyle" ID="UserTable"
                        ShowExtraCommandColumns="true" PermissionUrl="~/Security/Permissions.aspx?Id={0}"
                        runat="server" Width="100%" EntityType="Identity" ShowFooter="True" AllowSorting="True"
                        EnableDelete="True" AllowPaging="True" ShowCommandColumns="false" OnPageChanged="UserTable_PageChanged"
                        ViewUrl="~/Security/Users.aspx?Id={0}" ViewColumn="IdentityName">
                        <HeaderStyle CssClass="hStyle" />
                        <RowStyle CssClass="rStyle" />
                        <AlternatingRowStyle CssClass="arStyle" />
                        <ButtonStyle CssClass="Masterbutton" />
                        <DisplayColumns>
                            <Zentity:ZentityGridViewColumn ColumnName="IdentityName" HeaderText="Identity Name">
                            </Zentity:ZentityGridViewColumn>
                            <Zentity:ZentityGridViewColumn ColumnName="Description" HeaderText="Description">
                            </Zentity:ZentityGridViewColumn>
                        </DisplayColumns>
                    </Zentity:ResourceDataGridView>
                    </asp:Panel>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
