<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_Browse" Codebehind="Browse.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <div class="PlainTextContainer">
            <span class="SectionFont">
                <asp:Label ID="browseLabel" runat="server" Text="<%$ Resources:Resources, ZentityBrowse%>"></asp:Label>
            </span>
            <h5>
                <br />
                <asp:Label ID="browseByYearLabel" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByYear%>"></asp:Label>
            </h5>
            <asp:Label ID="browseByYearDescriptionLabel" runat="server" Text="<%$ Resources:Resources, BrowseByYearDesc%>"></asp:Label>
            <br />
            <h5>
                <br />
                <asp:Label ID="browseByCategoryHierarchyLabel" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByCategory%>"></asp:Label>
            </h5>
            <asp:Label ID="browseByCategoryHierarchyDescriptionLabel" runat="server" Text="<%$ Resources:Resources, BrowseByCategoryDesc%>"></asp:Label>
            <br />
            <h5>
                <br />
                <asp:Label ID="browseByResourceTypeLabel" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByResType%>"></asp:Label>
            </h5>
            <asp:Label ID="browseByResourceTypeDescriptionLabel" runat="server" Text="<%$ Resources:Resources, BrowseByResourceTypeDesc%>"></asp:Label>
            <br />
            <h5>
                <br />
                <asp:Label ID="browseByAuthorLabel" runat="server" Text="<%$ Resources:Resources, ZentityBrowseByAuthor%>"></asp:Label>
            </h5>
            <asp:Label ID="browseByAuthorDescriptionLabel" runat="server" Text="<%$ Resources:Resources, BrowseByAuthorDesc%>"></asp:Label>
            <br />
            <h5>
                <br />
                <asp:Label ID="recentlyAddedResourcesLabel" runat="server" Text="<%$ Resources:Resources, ZentityBrowseRecentlyAddedResource%>"></asp:Label>
            </h5>
            <asp:Label ID="recentlyAddedResourcesDescriptionLabel" runat="server" Text="<%$ Resources:Resources, RecentlyAddedReourcesDesc%>"></asp:Label>
            <br />
            <h5>
                <br />
                <asp:Label ID="topAuthorsLabel" runat="server" Text="<%$ Resources:Resources, ZentityBrowseTopAuthors%>"></asp:Label>
            </h5>
            <asp:Label ID="topAuthorsDescriptionLabel" runat="server" Text="<%$ Resources:Resources, TopAuthorsDesc%>"></asp:Label>
        </div>
    </div>
</asp:Content>
