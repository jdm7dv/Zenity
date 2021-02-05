<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="Security_Groups" EnableEventValidation="false" Codebehind="Groups.aspx.cs" %>

<%@ Register Src="~/UserControls/GroupAssignment.ascx" TagName="GroupAssignment"
    TagPrefix="ucGroupAssignment" %>
<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <table style="width: 100%;">
            <tr>
                <td colspan="3" class="MasterContentHEAD" style="text-align: left">
                    <%= Resources.Resources.LabelManageGroupsText %>
                </td>
            </tr>
            <tr>
                <td colspan="3" align="left">
                    <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Panel ID="GroupManagerPanel" runat="server" DefaultButton="SubmitButton">
                        <table>
                            <tr>
                                <td class="UsersBorder" style="width: 58%" valign="top">
                                    <table style="width: 100%;">
                                        <tr>
                                            <td class="titleStyle" colspan="2">
                                                <%= Resources.Resources.LabelGroupPropertiesText%>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <table style="width: 100%;">
                                                    <asp:Panel ID="groupInfoPanel" runat="Server">
                                                        <tr class="rStyle">
                                                            <td>
                                                                <%= Resources.Resources.GroupNameText%><span class="RedText">*</span>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtGroupName" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                                                &nbsp;
                                                                <asp:RequiredFieldValidator ID="requiredGroupName" runat="server" ValidationGroup="Submit"
                                                                    ControlToValidate="txtGroupName" Display="Dynamic" ErrorMessage="Required"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr class="arStyle">
                                                            <td style="width: 30%">
                                                                <%= Resources.Resources.GroupTitleText%>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtTitle" runat="server"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="rStyle">
                                                            <td>
                                                                <%= Resources.Resources.DescriptionText%>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="arStyle">
                                                            <td>
                                                                <%= Resources.Resources.UriText%>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtUri" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                    </asp:Panel>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td>
                                </td>
                                <td class="UsersBorder" style="width: 41%">
                                    <ucGroupAssignment:GroupAssignment ID="groupAssignment" runat="server" />
                                </td>
                            </tr>
                            <tr style="width: 100%;">
                                <td align="center" colspan="3">
                                    <br />
                                    <asp:Button runat="server" ID="SubmitButton" Text="Save" CausesValidation="true"
                                        ValidationGroup="Submit" OnClick="Submit_Click" />
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
                <td colspan="3" align="left">
                    <div class="OverflowStretch">
                        <Zentity:ResourceDataGridView ID="GroupTable" GridLines="None" CssClass="ResourceGridStyle"
                            ShowExtraCommandColumns="true" PermissionUrl="~/Security/Permissions.aspx?Id={0}"
                            runat="server" Width="100%" EntityType="Group" ShowFooter="True" AllowSorting="True"
                            EnableDelete="True" AllowPaging="True" ShowCommandColumns="false" OnPageChanged="GroupTable_PageChanged"
                            ViewUrl="~/Security/Groups.aspx?Id={0}" ViewColumn="GroupName">
                            <HeaderStyle CssClass="hStyle" />
                            <RowStyle CssClass="rStyle" />
                            <AlternatingRowStyle CssClass="arStyle" />
                            <ButtonStyle CssClass="Masterbutton" />
                            <DisplayColumns>
                                <Zentity:ZentityGridViewColumn ColumnName="GroupName" HeaderText="Group Name">
                                </Zentity:ZentityGridViewColumn>
                                <Zentity:ZentityGridViewColumn ColumnName="Description" HeaderText="Description">
                                </Zentity:ZentityGridViewColumn>
                            </DisplayColumns>
                        </Zentity:ResourceDataGridView>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
