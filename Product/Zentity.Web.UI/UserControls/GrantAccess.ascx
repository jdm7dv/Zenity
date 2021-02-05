<%@ Control Language="C#" AutoEventWireup="true" Inherits="UserControls_GrantAccess" Codebehind="GrantAccess.ascx.cs" %>

<script language="javascript" type="text/javascript">

    function AllowPermission(chkCtrl) {

        var ctrSpan = chkCtrl.parentNode;
        var givenPriority = Number(ctrSpan.attributes["alt"].value);
        var td = ctrSpan.parentNode;
        var tr = td.parentNode;
        var table = tr.parentNode;

        var chkList = table.getElementsByTagName("input");
        for (i = 0; i < chkList.length; i++) {
            var span = chkList[i].parentNode;
            var chk = chkList[i];
            var priority = Number(span.attributes["alt"].value);
            if (chkList[i].parentNode.attributes["name"].value == "Allow") {
                if (priority > givenPriority || (priority == givenPriority && chkCtrl.checked)) {
                    chk.checked = true;
                    InversCheckBoxUncheck(chk)
                }
                else
                    chk.checked = false;
            }
        }

    }
    function DenyPermission(chkCtrl) {
        var ctrSpan = chkCtrl.parentNode;
        var givenPriority = Number(ctrSpan.attributes["alt"].value) * -1;
        var td = ctrSpan.parentNode;
        var tr = td.parentNode;
        var table = tr.parentNode;

        var chkList = table.getElementsByTagName("input");
        for (i = 0; i < chkList.length; i++) {
            var span = chkList[i].parentNode;
            var chk = chkList[i];
            var priority = Number(span.attributes["alt"].value) * -1;
            if (span.attributes["name"].value == "Deny") {
                if (priority > givenPriority || (priority == givenPriority && chkCtrl.checked)) {
                    chk.checked = true;
                    InversCheckBoxUncheck(chk)
                }
                else
                    chk.checked = false;
            }
        }
    }

    function InversCheckBoxUncheck(chk) {

        if (!chk.checked)
            return;
        var tr = chk.parentNode.parentNode;
        if (tr.nodeName == "TD")
            tr = tr.parentNode
        var pairOfChkList = tr.getElementsByTagName("input");
        for (j = 0; j < pairOfChkList.length; j++)
            pairOfChkList[j].checked = false;
        chk.checked = true;
    }
</script>

<asp:Panel ID="GrantAccessPanel" runat="Server" DefaultButton="btnGrantAccess">
    <table style="padding-left: 15px;" cellpadding="0" cellspacing="0">
        <tr>
            <td>
                <asp:GridView ID="grdVwPermission" runat="server" AutoGenerateColumns="False" HeaderStyle-CssClass="TitleStyle"
                    CellPadding="5">
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
                                    name="Allow" alt='<%# DataBinder.Eval(Container.DataItem, "Priority") %>' onclick='javascript:AllowPermission(this);' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:Label ID="lblDeny" Text="<%$ Resources:Resources, DenyPermission %>" runat="server"></asp:Label>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox runat="server" ID="chkRevokePermission" Checked='<%# DataBinder.Eval(Container.DataItem, "Deny") %>'
                                    name="Deny" alt='<%# DataBinder.Eval(Container.DataItem, "Priority") %>' onclick='javascript:DenyPermission(this);' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
        <tr>
            <td style="text-align: center">
                <asp:Button ID="btnGrantAccess" runat="server" Text="Grant" OnClick="btnGrantAccess_Click" />
            </td>
        </tr>
    </table>
</asp:Panel>
