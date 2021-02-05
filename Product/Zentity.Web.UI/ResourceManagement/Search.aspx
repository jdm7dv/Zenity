<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_Search" Codebehind="Search.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="server">
    <div class="RightContentInnerPane">
        <div class="PlainTextContainer">
            <span class="SectionFont">
                <asp:Label runat="server" ID="searchLabel"></asp:Label>
            </span>
            <h5>
                <br />
                <asp:Label ID="basicSearchLabel" runat="server"></asp:Label>
            </h5>
            <asp:Label ID="basicSearchDescriptionLabel" runat="server"></asp:Label>
        </div>
    </div>
</asp:Content>
