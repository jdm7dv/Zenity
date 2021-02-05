<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="Help_Services" Codebehind="Services.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="server">
    <div class="RightContentInnerPane">
        <div class="PlainTextContainer">
            <span class="SectionFont">
                <%= Resources.Resources.ZentityService%></span>
            <br />
            <br />
            <span>
                <%= Resources.Resources.ZentityServiceSummary%>
            </span>
            <br />
            <br />
            <div class="ServiceMargin">
                <br />
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceName%></span>
                </div>
                <div>
                    <span>
                        <%= Resources.Resources.ZentityServiceAtomPub%></span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceDescription%></span>
                </div>
                <div style="margin-left: 19%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceAtomPubDescription%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceEndPoint%></span>
                </div>
                <div style="width: 80%;">
                    <asp:HyperLink ID="HyperLinkAtomPub" runat="server"></asp:HyperLink>
                    &nbsp;
                </div>
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <br />
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceName%>
                    </span>
                </div>
                <div style="width: 80%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceSword%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceDescription%>
                    </span>
                </div>
                <div style="margin-left: 19%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceSwordDescription%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceEndPoint%></span>
                </div>
                <div style="width: 80%;">
                    <asp:HyperLink ID="HyperLinkSword" runat="server"></asp:HyperLink>
                    &nbsp;
                </div>
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <br />
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceName%></span>
                </div>
                <div style="width: 80%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceSyndication%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceDescription%></span>
                </div>
                <div style="margin-left: 19%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceSyndicationDescription%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceEndPoint%></span>
                </div>
                <div style="width: 80%;">
                    <asp:HyperLink ID="HyperLinkSyndication" runat="server"></asp:HyperLink>
                    &nbsp;
                </div>
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <br />
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceName%></span>
                </div>
                <div style="width: 80%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceOaiPmh%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceDescription%></span>
                </div>
                <div style="margin-left: 19%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceOaiPmhDescription%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceEndPoint%></span>
                </div>
                <div style="width: 80%;">
                    <asp:HyperLink ID="HyperLinkOaiPmh" runat="server"></asp:HyperLink>
                    &nbsp;
                </div>
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
            <div class="ServiceMargin">
                <br />
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceName%></span>
                </div>
                <div style="width: 80%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceOaiOre%></span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceDescription%></span>
                </div>
                <div style="margin-left: 19%;">
                    <span>
                        <%= Resources.Resources.ZentityServiceOaiOreDescription%>
                    </span>
                    <br />
                </div>
                <div class="ServiceInternalContainer">
                    <span>
                        <%= Resources.Resources.ZentityServiceEndPoint%></span>
                </div>
                <div style="width: 80%;">
                    <asp:HyperLink ID="HyperLinkOaiOre" runat="server"></asp:HyperLink>
                    &nbsp;
                </div>
                <br />
                <div class="HelpSectionInternalDiv">
                </div>
            </div>
        </div>
    </div>
</asp:Content>
