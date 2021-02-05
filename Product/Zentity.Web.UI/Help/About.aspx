<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="Help_About" Codebehind="About.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="server">
    <div class="RightContentInnerPane">
        <div class="PlainTextContainer">
            <span class="SectionFont">
                <%= Resources.Resources.ZentityAboutTitle%>
            </span>
            <h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityAboutMicrosoftResearch%></span></h5>
            <br />
            <span>
                <%= Resources.Resources.ZentityAboutMicrosoftResearchSummary%>
                <asp:HyperLink ID="HyperLinkAboutMicrosoftResearch" runat="server" Text="http://www.microsoft.com/mscorp/tc/scholarly_communication.mspx"
                    NavigateUrl="http://www.microsoft.com/mscorp/tc/scholarly_communication.mspx"
                    Target="_blank">
                </asp:HyperLink>
            </span>
            <br />
            <br />
            <div class="HelpSectionInternalDiv">
            </div>
            <h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityAboutZentity %></span></h5>
            <br />
            <span>
                <%= Resources.Resources.ZentityAboutZentitySummary %>
            </span>
            <br />
            <br />
            <div class="HelpSectionInternalDiv">
            </div>
            <h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityAboutRepositoryForum%></span></h5>
            <br />
            <span>
                <%= Resources.Resources.ZentityAboutRepositoryForumSummary%>
                <asp:HyperLink ID="HyperLinkForum" runat="server" Text="http://community.research.microsoft.com/forums/90.aspx"
                    NavigateUrl="http://community.research.microsoft.com/forums/90.aspx" Target="_blank"></asp:HyperLink>
            </span>
            <br />
            <br />
            <div class="HelpSectionInternalDiv">
            </div>
            <h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityAboutFeedback%></span></h5>
            <br />
            <span>
                <%= Resources.Resources.ZentityAboutEmailUs%>
                <asp:HyperLink ID="HyperLinkEmail" runat="server" Text="irplat@microsoft.com" NavigateUrl="mailto:irplat@microsoft.com"></asp:HyperLink>
            </span>
            <br />
            <br />
            <div class="HelpSectionInternalDiv">
            </div>
            <h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityAboutBlog%></span></h5>
            <br />
            <span>
                <%= Resources.Resources.ZentityAboutBlogSummary%>
                <asp:HyperLink ID="HyperLinkBlog" runat="server" Text="http://savas.me/" NavigateUrl="http://savas.me/"
                    Target="_blank"></asp:HyperLink>
            </span>
            <br />
            <br />
            <div class="HelpSectionInternalDiv">
            </div>
        </div>
    </div>
</asp:Content>
