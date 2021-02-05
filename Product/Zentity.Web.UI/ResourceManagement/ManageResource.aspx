<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="ResourceManagement_ManageResource"
    EnableEventValidation="false" Codebehind="ManageResource.aspx.cs" %>

<%@ Reference Control="~/UserControls/CategoryNodeAssociation.ascx" %>
<%@ Reference Control="~/UserControls/CreateAssociation.ascx" %>
<%@ Reference Control="~/UserControls/CreateResource.ascx" %>
<%@ Reference Control="~/UserControls/CreateScholarlyWorkItemTagging.ascx" %>
<%@ Reference Control="~/UserControls/DisplayResourceDetails.ascx" %>
<%@ Reference Control="~/UserControls/ChangeHistory.ascx" %>
<%@ Register Src="~/UserControls/DisplayResourceDetails.ascx" TagName="DisplayResourceDetails"
    TagPrefix="ucDisplayResourceDetails" %>
<%@ Register Src="~/UserControls/CreateAssociation.ascx" TagName="CreateAssociation"
    TagPrefix="ucCreateAssociation" %>
<%@ Register Src="~/UserControls/CategoryNodeAssociation.ascx" TagName="CategoryNodeAssociation"
    TagPrefix="ucCategoryNodeAssociation" %>
<%@ Register Src="~/UserControls/CreateScholarlyWorkItemTagging.ascx" TagName="CreateScholarlyWorkItemTagging"
    TagPrefix="ucCreateScholarlyWorkItemTagging" %>
<%@ Register Src="~/UserControls/ResourcePermissions.ascx" TagName="ResourcePermissions"
    TagPrefix="ucResourcePermissions" %>
<%@ Register Src="~/UserControls/ChangeHistory.ascx" TagName="ChangeHistory"
    TagPrefix="ucChangeHistory" %>
<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <div>
            <asp:Label ID="errorMessage" Visible="false" runat="server"></asp:Label>
        </div>
        <div class="ManageTab">
            <ul>
                <li id="ResourceTypeTab" runat="server">
                    <asp:LinkButton ID="btnResourceType" runat="server" Text="<%$ Resources:Resources, TabResourceTypeText%>" OnClick="btnResourceType_Click" />
                </li>
                <li id="MetadataTab" runat="server">
                    <asp:LinkButton ID="btnMetadata" runat="server" Text="<%$ Resources:Resources, TabMetadataText%>" OnClick="btnMetadata_Click" />
                </li>
                <li id="AssociationsTab" runat="server">
                    <asp:LinkButton ID="btnAssociations" runat="server" Text="<%$ Resources:Resources, TabRelatedResourcesText%>" OnClick="btnAssociations_Click" />
                </li>
                <li id="CategoriesTab" runat="server">
                    <asp:LinkButton ID="btnCategories" runat="server" Text="<%$ Resources:Resources, TabCategoriesText%>" OnClick="btnCategories_Click" />
                </li>
                <li id="TagsTab" runat="server">
                    <asp:LinkButton ID="btnTags" runat="server" Text="<%$ Resources:Resources, TabTagsText%>" OnClick="btnTags_Click" />
                </li>
                <li id="SummaryTab" runat="server">
                    <asp:LinkButton ID="btnSummary" runat="server" Text="<%$ Resources:Resources, TabSummaryText%>" OnClick="btnSummary_Click" />
                </li>
                <li id="ChangeHistoryTab" runat="server">
                    <asp:LinkButton ID="btnChangeHistory" runat="server" Text="<%$ Resources:Resources, TabChangeHistoryText%>" OnClick="btnChangeHistory_Click" />
                </li>
                <li id="ResourcePermissionsTab" runat="server">
                    <asp:LinkButton ID="btnResourcePermissions" runat="server" Text="<%$ Resources:Resources, TabPermissionsText%>" OnClick="btnResourcePermission_Click" />
                </li>
            </ul>
        </div>
        <div class="ManageTabControlsContainer" style="clear: left">
            <asp:MultiView ID="mtvManage" runat="server" ActiveViewIndex="0">
                <asp:View ID="vwSelectResourceType" runat="server">
                    <Zentity:SelectType runat="server" ID="ResourceTypeControl" />
                </asp:View>
                <asp:View ID="vwMetadata" runat="server">
                </asp:View>
                <asp:View ID="vwAssociations" runat="server">
                </asp:View>
                <asp:View ID="vwCategories" runat="server">
                </asp:View>
                <asp:View ID="vwTags" runat="server">
                </asp:View>
                <asp:View ID="vwSummary" runat="server">
                </asp:View>
                <asp:View ID="vwChangeHistory" runat="server">
                </asp:View>
                <asp:View ID="vwResourcePermission" runat="server">
                </asp:View>
            </asp:MultiView>
        </div>
    </div>
</asp:Content>
