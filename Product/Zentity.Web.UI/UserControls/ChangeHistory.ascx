<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="UserControls_ChangeHistory" Codebehind="ChangeHistory.ascx.cs" %>
<div>
    <Zentity:ChangeHistory ID="ChangeHistoryControl" runat="server" MaximumChangeSetToFetch="5"
        Title="<%$ Resources:Resources, ChangeHistoryTitle%>" NavigateUrlForResource="~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=Summary"
        NavigateUrlForCategory="~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=Summary"
        NavigateUrlForTag="~/ResourceManagement/ManageResource.aspx?Id={0}&ActiveTab=Summary"
        IsSecurityAwareControl="true">
        <TitleStyle CssClass="ManageTabTitleStyle" />
        <ScalarPropertyChangeSetStyle HeaderTitle="<%$ Resources:Resources, ScalarPropertyChangeText %>">
            <HeaderStyle CssClass="HistoryHeaderStyle" />
            <RowStyle CssClass="HistoryRowStyle" />
            <InnerTableHeaderStyle CssClass="HistoryInnerHeaderStyle" />
            <InnerTableRowStyle CssClass="HistoryInnerRowStyle" />
            <InnerTableAlternateRowStyle CssClass="HistoryInnerRowStyle" />
        </ScalarPropertyChangeSetStyle>
        <RelationshipChangeSetStyle HeaderTitle="<%$ Resources:Resources, RelationshipChangesText %>">
            <HeaderStyle CssClass="HistoryHeaderStyle" />
            <RowStyle CssClass="HistoryRowStyle" />
            <InnerTableHeaderStyle CssClass="HistoryInnerHeaderStyle" />
            <InnerTableRowStyle CssClass="HistoryInnerRowStyle" />
            <InnerTableAlternateRowStyle CssClass="HistoryInnerRowStyle" />            
        </RelationshipChangeSetStyle>
        <CategoryAssociationChangeSetStyle HeaderTitle="<%$ Resources:Resources, CategoryAssociationChangesText %>">
            <HeaderStyle CssClass="HistoryHeaderStyle" />
            <RowStyle CssClass="HistoryRowStyle" />
            <InnerTableHeaderStyle CssClass="HistoryInnerHeaderStyle" />
            <InnerTableRowStyle CssClass="HistoryInnerRowStyle" />
            <InnerTableAlternateRowStyle CssClass="HistoryInnerRowStyle" />                    
        </CategoryAssociationChangeSetStyle>
        <ResourceTaggingChangeSetStyle HeaderTitle="<%$ Resources:Resources, ResourceTaggingChangesText %>">
            <HeaderStyle CssClass="HistoryHeaderStyle" />
            <RowStyle CssClass="HistoryRowStyle" />
            <InnerTableHeaderStyle CssClass="HistoryInnerHeaderStyle" />
            <InnerTableRowStyle CssClass="HistoryInnerRowStyle" />
            <InnerTableAlternateRowStyle CssClass="HistoryInnerRowStyle" />                            
        </ResourceTaggingChangeSetStyle>
    </Zentity:ChangeHistory>
</div>
