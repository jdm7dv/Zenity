<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_GroupAssignment" EnableViewState="true" Codebehind="GroupAssignment.ascx.cs" %>
<div>
    <table width="100%" cellpadding="0" cellspacing="0">
        <tr>
            <td class="titleStyle">
                <asp:Label ID="lblAssignment" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Panel ID="groupAssignmentPanel" runat="Server">
                    <table>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtSerachGroup" runat="server" Width="200px"></asp:TextBox>
                                &nbsp;&nbsp;<asp:Button ID="btnUserSearch" runat="server" Text="Search" OnClick="btnSearch_Click"
                                    Visible="false" />
                                <asp:Button ID="btnGroupSearch" runat="server" Text="Search" OnClick="btnGroupSearch_Click"
                                    Visible="false" />
                                <asp:Button ID="btnResourceSearch" runat="server" Text="Search" OnClick="btnResourceSearch_Click"
                                    Visible="false" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div class="divGroupList">
                                    <asp:CheckBoxList ID="chkGroupList" runat="server" Width="200px">
                                    </asp:CheckBoxList>
                                    <asp:ListBox ID="lstGroupList" runat="server" Width="400px" Height="100%" Visible="false"
                                        SelectionMode="Single"></asp:ListBox>
                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
    </table>
</div>
