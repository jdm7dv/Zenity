<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="ResourceManagement_TopAuthors" Codebehind="TopAuthors.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <asp:Table ID="TopAuthorsTable" CssClass="TopAuthorsTable" runat="server">
            <asp:TableHeaderRow>
                <asp:TableHeaderCell ColumnSpan="4" Text="<%$ Resources:Resources, ZentityMostActiveAuthor %>"></asp:TableHeaderCell>
            </asp:TableHeaderRow>
        </asp:Table>
    </div>
</asp:Content>
