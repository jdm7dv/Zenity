<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="Error" Title="<%$ Resources:Resources, CustomErrorPageHeader %>" Codebehind="Error.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <div class="TitleStyle">
            <%= Resources.Resources.CustomErrorPageHeader %>
        </div>
        <br />
        <div>
            <asp:Label ID="MessageText" runat="server" CssClass="Span"></asp:Label>
        </div>
    </div>
</asp:Content>
