<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="Help_Help" Codebehind="Help.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <div class="PlainTextContainer">
            <span class="SectionFont">
                <%= Resources.Resources.ZentityHelpTitle %>
            </span>
            <br />
            <br />
            <span>
                <%= Resources.Resources.ZentityHelpSummary %>
            </span>
            <br />
            <br />
            <div class="ServiceMargin">
                <h5>
                    <%= Resources.Resources.SelectThemeHeader %>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.SelectThemeSummary %>
                </span>
                <asp:RadioButtonList ID="ThemeRadioButtonList" runat="server" OnSelectedIndexChanged="ThemeRadioButtonList_SelectedIndexChanged"
                    AutoPostBack="true" />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkQuickGuide" runat="server" NavigateUrl="~/Documents/Zentity Quick Guide - Version 1.docx"
                        Text="<%$ Resources:Resources, ZentityHelpQuickGuide %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpQuickGuideSummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkCoreReference" runat="server" NavigateUrl="~/Documents/Zentity.Core.chm"
                        Text="<%$ Resources:Resources, ZentityHelpCoreReference %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpCoreReferenceSummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkPlatformReference" runat="server" NavigateUrl="~/Documents/Zentity.Platform.chm"
                        Text="<%$ Resources:Resources, ZentityHelpPlatformReference %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpPlatformReferenceSummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkSecurityReference" runat="server" NavigateUrl="~/Documents/Zentity.Security.chm"
                        Text="<%$ Resources:Resources, ZentityHelpSecurityReference %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpSecurityReferenceSummary%>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkToolKitReference" runat="server" NavigateUrl="~/Documents/Zentity.Web.UI.ToolKit.chm"
                        Text="<%$ Resources:Resources, ZentityHelpToolkitReference %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpToolkitReferenceSummary%>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkUIGuide" runat="server" NavigateUrl="~/Documents/Zentity Web UI User Guide - Version 1.docx"
                        Text="<%$ Resources:Resources, ZentityHelpUIGuide %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpUIGuideSummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkExtensibility" runat="server" NavigateUrl="~/Documents/Zentity Data Model Extensibility User Guide - Version 1.docx"
                        Text="<%$ Resources:Resources, ZentityHelpDataModelExtensibility %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpDataModelExtensibilitySummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkChangeHistory" runat="server" NavigateUrl="~/Documents/Zentity Change History Logging User Guide - Version 1.docx"
                        Text="<%$ Resources:Resources, ZentityHelpChangeHistory %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpChangeHistorySummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkSearch" runat="server" NavigateUrl="~/Documents/Zentity Search User Guide - Version 1.docx"
                        Text="<%$ Resources:Resources, ZentityHelpSearch %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpSearchSummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <h5>
                    <br />
                    <asp:HyperLink ID="HyperLinkSecurity" runat="server" NavigateUrl="~/Documents/Zentity Security User Guide - Version 1.docx"
                        Text="<%$ Resources:Resources, ZentityHelpSecurity %>" Target="_blank"></asp:HyperLink>
                </h5>
                <br />
                <span>
                    <%= Resources.Resources.ZentityHelpSecuritySummary %>
                </span>
                <br />
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
        </div>
    </div>
</asp:Content>
