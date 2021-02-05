<%@ Page Title="" Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true" Inherits="ResourceManagement_TopTags" Codebehind="TopTags.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <div id="divTopTags" runat="server">
            <Zentity:TagCloud ID="TagCloudControl" runat="server" CssClass="TopTags" EnableViewState="False"
                MaximumFontSize="36" Width="780px" MinimumFontSize="12" MaximumTagsToFetch="150"
                TagClickDestinationPageUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}"
                IsSecurityAwareControl="true">
            </Zentity:TagCloud>
        </div>
    </div>

    <script type="text/javascript" language="javascript">
        var tagCloudContainerClientID = '<%= divTopTags.ClientID %>';
        AdjustTopTags(tagCloudContainerClientID, false); 
    </script>

</asp:Content>
