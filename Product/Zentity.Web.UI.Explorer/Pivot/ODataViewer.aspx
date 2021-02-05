<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true"
    CodeBehind="ODataViewer.aspx.cs" Inherits="Zentity.Web.UI.Explorer.Pivot.ODataViewer" %>

<asp:Content ID="titleContent" ContentPlaceHolderID="titleContentPlaceHolder" runat="server">
    Welcome to Zentity Pivot Viewer </asp:Content>
<asp:Content ID="defaultContent" ContentPlaceHolderID="defaultContentPlaceHolder"
    runat="server">
    <table style="width:100%">
        <tr>
            <td>
                <div id="MainBody" runat="server" style="margin-left:150px;margin-right:150px;height: 85%;">
                    <h1>Zentity Pivot Viewer</h1>
                    <div id="searchContainer" 
                        style="vertical-align: middle; text-align: left; letter-spacing: normal">
                        <asp:Label ID="lblODataUrl" runat="server" Text="Enter a valid OData service URL:"></asp:Label>&nbsp;
                        <asp:TextBox ID="txtODataUrl" runat="server" AutoPostBack="false" TextMode="SingleLine"
                            AutoCompleteType="None" ToolTip="Enter a valid OData serice URL" Width="500px"></asp:TextBox>
                        &nbsp;<asp:Button ID="btnSubmit" runat="server" UseSubmitBehavior="true" Text="Submit"
                            OnClick="OnBtnSubmitClick" />
                    </div>
                    <div id="silverlightControlHost" style="height: 95%;" runat="server">
                        <object data="data:application/x-silverlight-2," type="application/x-silverlight-2"
                            width="100%" height="700px">
                            <param name="source" value="../ClientBin/Zentity.Pivot.Web.Viewer.xap" />
                            <param name="onError" value="onSilverlightError" />
                            <param name="background" value="white" />
                            <param name="minRuntimeVersion" value="4.0.50401.0" />
                            <param name="autoUpgrade" value="true" />
                            <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50401.0" style="text-decoration: none">
                                <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight"
                                    style="border-style: none" />
                            </a>
                        </object>
                        <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px;
                            border: 0px"></iframe>
                    </div>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div id="errorSummaryContainer" runat="server" visible="false" style="position: relative">
                    <span style="font-size: medium; font-weight: normal; font-style: normal; color: #FF0000">
                        The collection you are trying to access is not present or published. Please navigate
                        to the
                        <a href="Gallery.aspx">Pivot Gallery</a>
                        to view available collections.</span>
                </div>
            </td>
        </tr>
    </table>
</asp:Content>
<asp:Content ContentPlaceHolderID="footerContentPlaceHolder" runat="server" ID="footerContent">
    <div id="Footer">
        <ul>
           <li class="first">
                <a title="Visual Explorer" href="../Default.aspx">Visual Explorer</a>
            </li>
            <li>
                <a title="Zentity Dashboard" href="Gallery.aspx">Zentity Dashboard</a>
            </li>
            <li>
                <a title="Zentity Website" href="http://research.microsoft.com/en-us/projects/zentity">
                    Zentity Website </a>
            </li>
            <li class="last">
                <a title="Pivot Website" href="http://www.getpivot.com">Pivot Website </a>
            </li>
        </ul>
    </div>
</asp:Content>
