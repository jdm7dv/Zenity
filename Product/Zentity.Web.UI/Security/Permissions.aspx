<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="Security_Permissions" Codebehind="Permissions.aspx.cs" %>

<%@ Register Src="../UserControls/GrantAccess.ascx" TagName="GrantAccess" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">

    <script language="javascript" type="text/javascript">
        function Permission(chkCtrl) {

            var ctrSpan = chkCtrl.parentNode;
            var td = ctrSpan.parentNode;
            var tr = td.parentNode;
            var table = tr.parentNode;
            var chkList = table.getElementsByTagName("input");
            if (chkCtrl.checked) {

                for (i = 0; i < chkList.length; i++) {

                    chkList[i].checked = false;
                }
                chkCtrl.checked = true;
            }
        }
    </script>

    <div class="RightContentInnerPane">
        <table style="width: 100%">
            <tr>
                <td class="MasterContentHEAD" style="text-align: left">
                    <%= Resources.Resources.ZentityManagePermissions%>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblErrorOrMessage" runat="server" ForeColor="Red"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblPageTitle" runat="server" Font-Bold="true"></asp:Label>
                </td>
            </tr>
            <tr>
                <td id="PageContent" runat="server">
                    <table style="width: 100%">
                        <tr>
                            <td>
                                <asp:Panel ID="GlobalPermissionPanel" runat="Server" DefaultButton="btnGrantAccess">
                                    <table style="width: 100%">
                                        <tr>
                                            <td class="UsersBorder">
                                                <table style="width: 100%">
                                                    <tr>
                                                        <td class="titleStyle">
                                                            <asp:Label ID="lblGlobal" runat="server"><%= Resources.Resources.GlobalPermission%></asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            &nbsp;
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td colspan="2" style="padding-left: 20px">
                                                            <asp:Label ID="lblErrorGlobalPermission" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            &nbsp;
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="padding-left: 5%">
                                                            <asp:GridView ID="grdGlobalPermission" runat="server" AutoGenerateColumns="False"
                                                                HeaderStyle-CssClass="TitleStyle" CellPadding="5">
                                                                <Columns>
                                                                    <asp:TemplateField>
                                                                        <HeaderTemplate />
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="lblPermission" Text='<%# DataBinder.Eval(Container.DataItem, "Permission") %>'
                                                                                runat="server" name="lblPermission"></asp:Label>
                                                                        </ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Left" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField>
                                                                        <HeaderTemplate>
                                                                            <asp:Label ID="lblAllow" Text="<%$ Resources:Resources, AllowPermission %>" runat="server"></asp:Label>
                                                                        </HeaderTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:CheckBox runat="server" ID="chkGrantPermission" Checked='<%# DataBinder.Eval(Container.DataItem, "Allow") %>'
                                                                                name="Allow" alt='' onclick='javascript:Permission(this);' />
                                                                        </ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Center" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField>
                                                                        <HeaderTemplate>
                                                                            <asp:Label ID="lblDeny" Text="<%$ Resources:Resources, DenyPermission %>" runat="server"></asp:Label>
                                                                        </HeaderTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:CheckBox runat="server" ID="chkRevokePermission" Checked='<%# DataBinder.Eval(Container.DataItem, "Deny") %>'
                                                                                name="Deny" alt='' onclick='javascript:Permission(this);' />
                                                                        </ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Center" />
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                            </asp:GridView>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="padding-left: 13%">
                                                            <asp:Button ID="btnGrantAccess" runat="server" Text="Grant" OnClick="btnGrantAccess_Click" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                &nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table style="width: 100%" class="UsersBorder">
                                    <tr>
                                        <td class="titleStyle" colspan="2">
                                            <asp:Label ID="lblResourceAssigmnet" runat="server"><%= Resources.Resources.ResourcePermission %></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            &nbsp;
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="width: 100%">
                                            <asp:Panel runat="server" DefaultButton="btnSearchResources">
                                                <div style="width: 300px; float: left">
                                                    <table style="width: 100%">
                                                        <tr>
                                                            <td>
                                                                <asp:Label ID="lblTitle" runat="server" Font-Bold="true" Text="<%$ Resources:Resources, TagLabelTitleText %>"></asp:Label>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtSearchText" runat="server" Width="250px"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td colspan="2">
                                                                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:Label ID="lblNote" runat="server"
                                                                    Font-Size="Smaller" Text="<%$ Resources:Resources, NoteUserOrGroupSearch %>"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>
                                                <div>
                                                    <asp:Button ID="btnSearchResources" runat="server" Text="<%$Resources:Resources,  SearchResources %>"
                                                        OnClick="btnSearchResources_Click" />
                                                </div>
                                            </asp:Panel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" style="padding-left: 20px">
                                            <asp:RadioButtonList ID="rbSearchOptions" runat="server" RepeatDirection="Horizontal">
                                                <asp:ListItem Selected="True" Text="<%$ Resources:Resources, SearchAll %>" />
                                                <asp:ListItem Text="<%$ Resources:Resources, SearchInExistingList %>" />
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            &nbsp;
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2" style="padding-left: 20px">
                                            <asp:Label ID="lblErrorResourcePermission" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            &nbsp;
                                        </td>
                                    </tr>
                                    <tr valign="top">
                                        <td style="width: 70%; vertical-align: top; padding-left: 20px">
                                            <div style="overflow: auto;">
                                                <Zentity:ResourceDataGridView ID="ResourceTable" GridLines="None" CssClass="ResourceGridStyle"
                                                    ShowExtraCommandColumns="true" PermissionUrl="" runat="server" Width="500px"
                                                    EntityType="Resource" ShowFooter="True" AllowSorting="True" EnableDelete="False"
                                                    AllowPaging="True" ShowCommandColumns="false" OnPageChanged="ResourceTable_PageChanged"
                                                    ViewUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}" OnRowCommand="ResourceTable_RowCommand"
                                                    DataKeyNames="Id">
                                                    <HeaderStyle CssClass="hStyle" />
                                                    <RowStyle CssClass="rStyle" />
                                                    <AlternatingRowStyle CssClass="arStyle" />
                                                    <ButtonStyle CssClass="Masterbutton" />
                                                    <DisplayColumns>
                                                        <Zentity:ZentityGridViewColumn ColumnName="Title" HeaderText="Title">
                                                        </Zentity:ZentityGridViewColumn>
                                                    </DisplayColumns>
                                                </Zentity:ResourceDataGridView>
                                            </div>
                                        </td>
                                        <td style="vertical-align: top; width: 30%;" align="center">
                                            <uc1:GrantAccess ID="grantAccess" runat="server" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
