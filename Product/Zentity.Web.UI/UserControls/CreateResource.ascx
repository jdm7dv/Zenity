<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_CreateResource" Codebehind="CreateResource.ascx.cs" %>
<div class="container">
    <%--    <div>
        <asp:Label ID="PageTitleLabel" runat="server"></asp:Label>
    </div>
--%>
    <div>
        <asp:Label ID="errorMessage" runat="server" Visible="false" Text="Fail to add resource."></asp:Label>
    </div>
    <asp:Panel ID="CreateResourcePanel" runat="server" DefaultButton="SubmitButton">
        <div id="resourceTable" runat="server">
            <div>
                <Zentity:ResourceProperties ID="resourceProperties" runat="server" IsSecurityAwareControl="true">
                    <TitleStyle CssClass="ManageTabTitleStyle" />
                    <RowStyle CssClass="rStyle" />
                    <AlternateRowStyle CssClass="arStyle" />
                    <ControlsStyle CssClass="resourceProperties" />
                </Zentity:ResourceProperties>
            </div>
            <div>
                <br />
            </div>
            <div style="text-align: center">
                <asp:Button runat="server" ID="SubmitButton" Text="Save" CausesValidation="true"
                    ValidationGroup="CreateResource" OnClick="Submit_Click" />
                <asp:Button runat="server" ID="ButtonSimilarMatch" Text="Match Similar Records" CausesValidation="true"
                    ValidationGroup="CreateResource" OnClick="SimilarRecords_Click" />
            </div>
        </div>
    </asp:Panel>
</div>
