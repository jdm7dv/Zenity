<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="Manage" Title="Manage" Codebehind="Manage.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <div class="PlainTextContainer">
            <span class="SectionFont">
                <asp:Label runat="server" ID="ManageScholarWorkLabel" Text="Manage  Scholarly Works"></asp:Label>
            </span>
            <h5>
                <br />
                <asp:Label ID="addScholarWorkLabel" runat="server"></asp:Label></h5>
            <asp:Label ID="addScholarWorkDescriptionLabel" runat="server"></asp:Label>
            <h5>
                <br />
                <asp:Label ID="addTagLabel" runat="server"></asp:Label>
            </h5>
            <asp:Label ID="addTagDescriptionLabel" runat="server"></asp:Label>
            <h5>
                <br />
                <asp:Label ID="listScholarWorkLabel" runat="server" Text=" List Scholarly Works"></asp:Label>
            </h5>
            <asp:Label ID="listScholarWorkDescriptionLabel" runat="server"></asp:Label>
            <h5>
                <br />
                <asp:Label ID="listTagLabel" runat="server"></asp:Label>
            </h5>
            <asp:Label ID="listTagDescriptionLabel" runat="server"></asp:Label>
            <h5>
                <br />
                <asp:Label ID="categoryLabel" runat="server"></asp:Label>
            </h5>
            <asp:Label ID="categoryDescriptionLabel" runat="server"></asp:Label>
            <h5>
                <br />
                <asp:Label ID="manageUsersLabel" runat="server"></asp:Label></h5>
            <asp:Label ID="manageUsersDescriptionLabel" runat="server"></asp:Label>
            <h5>
                <br />
                <asp:Label ID="manageGroupsLabel" runat="server"></asp:Label></h5>
            <asp:Label ID="manageGroupsDescriptionLabel" runat="server"></asp:Label>
        </div>
    </div>
</asp:Content>
