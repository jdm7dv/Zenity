<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_Tags" Title="<%$ Resources:Resources, ZentityTags %>" Codebehind="Tags.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <table style="width: 100%;">
            <tr>
                <td>
                    <Zentity:EntitySearch ID="tagSearch" runat="server" EntityType="Tag" ViewColumnName="Name"
                        ViewUrl="ManageResource.aspx?ActiveTab=Summary&Id={0}" EditUrl="AddTag.aspx?Id={0}"
                        EditAssociationUrl="CreateAssociation.aspx?Id={0}&EntityType={1}" Title="<%$ Resources:Resources, ZentityTags %>"
                        EnableDeleteOption="true" IsSecurityAwareControl="true">
                        <ButtonStyle CssClass="Masterbutton" />
                        <FilterCriteriaAlternatingRowStyle CssClass="arStyle" />
                        <FilterCriteriaHeaderStyle CssClass="hStyle" />
                        <FilterCriteriaRowStyle CssClass="rStyle" />
                        <TitleStyle CssClass="TitleStyle" />
                        <ResourceListViewTitleStyle CssClass="TitleStyle" />
                        <ResourceListViewContainerStyle CssClass="ResourceListViewContainer" />
                        <ResourceListViewItemStyle CssClass="ResourceListViewItemContainer" />
                        <ResourceListViewSeparatorStyle CssClass="ResourceListViewInternalDynDiv" />
                        <ResourceListViewButtonStyle CssClass="Masterbutton" />
                    </Zentity:EntitySearch>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
