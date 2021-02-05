<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_ResourcePermissions" Codebehind="ResourcePermissions.ascx.cs" %>
<%@ Register Src="../UserControls/GrantAccess.ascx" TagName="GrantAccess" TagPrefix="uc1" %>
<div style="padding-left: 15px; width: 725px">
    <table style="width: 100%">
        <tr>
            <td colspan="2">
                &nbsp;
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Panel ID="SearchPanel" CssClass="PermissionPanel" runat="server" DefaultButton="btnSearchUsers">
                    <div style="width: 40%; float: left">
                        <asp:TextBox ID="txtSearchText" runat="server" Width="100%"></asp:TextBox>
                        <asp:Label ID="lblNote" runat="server" Font-Size="Smaller" Text="<%$ Resources:Resources, NoteUserOrGroupSearch %>"></asp:Label>
                    </div>
                    &nbsp;&nbsp;
                    <asp:Button ID="btnSearchUsers" runat="server" Text="<%$Resources:Resources,  SearchUsers %>"
                        OnClick="btnSearchUsers_Click" />
                    &nbsp;
                    <asp:Button ID="btnSearchGroups" runat="server" Text="<%$Resources:Resources,  SearchGroups %>"
                        OnClick="btnSearchGroups_Click" />
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:RadioButtonList ID="rbSearchOptions" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True" Text="<%$ Resources:Resources, SearchAll %>" />
                    <asp:ListItem Text="<%$ Resources:Resources, SearchInExistingList %>" />
                </asp:RadioButtonList>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Label ID="lblErrorOrMessage" runat="server" ForeColor="Red"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                &nbsp;
            </td>
        </tr>
        <tr valign="top">
            <td style="width: 70%; vertical-align: top">
                <%-- <asp:GridView ID="grdVwSearchResult" runat="server" AutoGenerateColumns="False" OnSelectedIndexChanged="grdVwSearchResult_SelectedIndexChanged"
                    OnRowDataBound="grdVwSearchResult_RowDataBound" HeaderStyle-CssClass="TitleStyle"
                    Width="100%">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:Label ID="lblSecurityObjectNameHeader" runat="server" Text=""></asp:Label>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblSecurityObjectName" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField Visible="false">
                            <HeaderTemplate>
                                <asp:Label ID="lblSecurityObjectIdHeader" runat="server" Text=""></asp:Label>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblSecurityObjectId" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Id") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>--%>
                <div style="overflow: auto;">
                    <Zentity:ResourceDataGridView ID="grdVwSearchResult" GridLines="None" CssClass="ResourceGridStyle"
                        OnRowCommand="grdVwSearchResult_RowCommand" ShowExtraCommandColumns="true" PermissionUrl=""
                        runat="server" Width="100%" EntityType="Identity" ShowFooter="True" AllowSorting="True"
                        EnableDelete="False" AllowPaging="True" ShowCommandColumns="false" OnPageChanged="grdVwSearchResult_PageChanged"
                        ViewUrl="" DataKeyNames="Id" ViewColumn="IdentityName">
                        <HeaderStyle CssClass="hStyle" />
                        <RowStyle CssClass="rStyle" />
                        <AlternatingRowStyle CssClass="arStyle" />
                        <ButtonStyle CssClass="Masterbutton" />
                        <DisplayColumns>
                            <Zentity:ZentityGridViewColumn ColumnName="IdentityName" HeaderText="Identity Name">
                            </Zentity:ZentityGridViewColumn>
                        </DisplayColumns>
                    </Zentity:ResourceDataGridView>
                </div>
                <div style="overflow: auto;">
                    <Zentity:ResourceDataGridView ID="grdGroupSearchResult" GridLines="None" CssClass="ResourceGridStyle"
                        ShowExtraCommandColumns="true" PermissionUrl="" runat="server" Width="100%" OnRowCommand="grdGroupSearchResult_RowCommand"
                        EntityType="Group" ShowFooter="True" AllowSorting="True" EnableDelete="False"
                        AllowPaging="True" ShowCommandColumns="false" OnPageChanged="grdVwSearchResult_PageChanged"
                        ViewUrl="" ViewColumn="GroupName" DataKeyNames="Id">
                        <HeaderStyle CssClass="hStyle" />
                        <RowStyle CssClass="rStyle" />
                        <AlternatingRowStyle CssClass="arStyle" />
                        <ButtonStyle CssClass="Masterbutton" />
                        <DisplayColumns>
                            <Zentity:ZentityGridViewColumn ColumnName="GroupName" HeaderText="Group Name">
                            </Zentity:ZentityGridViewColumn>
                        </DisplayColumns>
                    </Zentity:ResourceDataGridView>
                </div>
            </td>
            <td style="vertical-align: top;">
                <uc1:GrantAccess ID="grantAccess" runat="server" />
            </td>
        </tr>
    </table>
</div>
