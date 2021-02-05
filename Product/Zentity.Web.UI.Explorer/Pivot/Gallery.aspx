<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true"
    CodeBehind="Gallery.aspx.cs" Inherits="Zentity.Web.UI.Explorer.Pivot.Gallery"
    ViewStateMode="Enabled" %>

<asp:Content ID="titleContent" ContentPlaceHolderID="titleContentPlaceHolder" runat="server">
    Welcome to Zentity Dashboard
</asp:Content>
<asp:Content ID="defaultContent" ContentPlaceHolderID="defaultContentPlaceHolder"
    runat="server">
    <div id="MainBody">
        <div class="t-p3-container">
            <div class="t-p3-summary">
                <h1><a href="Gallery.aspx" style="color:#000; text-decoration:none;">Zentity Dashboard</a></h1>
            </div>
        </div>
        <div class="t-s3-container">
            <div id="errorSummaryContainer" runat="server" visible="false" style="position: relative">
                <span id="errorMessageSpan" runat="server" style="font-size: medium; font-weight: normal;
                    font-style: normal; color: #FF0000">The Zentity Services are not accessible right
                    now. Please try again later or contact the admininstrator.</span>
            </div>
            <div class="collections-container" style="width: 100%">
                <asp:GridView ID="collectionsGrid" runat="server" CellPadding="4" ForeColor="#333333"
                    AutoGenerateColumns="False" AllowSorting="True" Width="100%" HorizontalAlign="Center"
                    ViewStateMode="Enabled" ShowFooter="true" BorderColor="#A0A0A0" BorderStyle="Solid"
                    BorderWidth="1px">
                    <PagerSettings Mode="NumericFirstLast" />
                    <AlternatingRowStyle BackColor="White" ForeColor="#000" />
                    <Columns>
                        <asp:BoundField DataField="DataModel" HeaderText="Data Model" SortExpression="DataModel"
                            ItemStyle-HorizontalAlign="Left" ItemStyle-Wrap="true" HeaderStyle-HorizontalAlign="Center"
                            HeaderStyle-Wrap="true" HeaderStyle-Width="23%" FooterText="Total" />
                        <asp:BoundField HeaderText="Resource Type" DataField="ResourceType" SortExpression="ResourceType"
                            ItemStyle-HorizontalAlign="Left" ItemStyle-Wrap="true" HeaderStyle-HorizontalAlign="Center"
                            HeaderStyle-Wrap="true" HeaderStyle-Width="23%" />
                        <asp:BoundField DataField="NumberofResources" HeaderText="Number of Resources" ItemStyle-HorizontalAlign="Left"
                            SortExpression="NumberofResources" ItemStyle-Wrap="true" HeaderStyle-HorizontalAlign="Center"
                            HeaderStyle-Wrap="true" HeaderStyle-Width="7%" FooterText="TotalNoOfResources" />
                        <asp:TemplateField HeaderText="Collection" ItemStyle-HorizontalAlign="Left" ItemStyle-Wrap="true"
                            HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="15%" SortExpression="Collection">
                            <ItemTemplate>
                                <asp:Repeater runat="server" ID="subCollection" EnableViewState="false" DataSource='<%# DataBinder.Eval(Container.DataItem, "Collection") %>'>
                                    <ItemTemplate>
                                        <asp:HyperLink runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'
                                            NavigateUrl='<%# DataBinder.Eval(Container.DataItem, "Path") %>'></asp:HyperLink>
                                        <br />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Number Of Elements" ItemStyle-HorizontalAlign="Left"
                            SortExpression="NumberOfElements" ItemStyle-Wrap="true" FooterText="TotalNoOfElements"
                            HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="7%">
                            <ItemTemplate>
                                <asp:Repeater runat="server" ID="subCollection" EnableViewState="false" DataSource='<%# DataBinder.Eval(Container.DataItem, "Collection") %>'>
                                    <ItemTemplate>
                                        <asp:Label ID="Label1" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "NumberOfElements") %>'></asp:Label>
                                        <br />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="LastUpdated" HeaderText="Last Updated" SortExpression="LastUpdated"
                            ItemStyle-Wrap="true" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Center"
                            HeaderStyle-Width="15%" />
                        <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" ItemStyle-HorizontalAlign="Left"
                            ItemStyle-Wrap="true" HeaderStyle-HorizontalAlign="Center" HeaderStyle-Width="10%" />
                    </Columns>
                    <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                    <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                    <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                    <SortedAscendingCellStyle BackColor="#E9E7E2" />
                    <SortedAscendingHeaderStyle BackColor="#506C8C" />
                    <SortedDescendingCellStyle BackColor="#FFFDF8" />
                    <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
                </asp:GridView>
            </div>
            <div id="collections-footer">
                <a title="Visual Explorer" href="../Default.aspx">Visual Explorer</a> <span class="collections-footer-seperator">
                </span><a title="OData Viewer" href="ODataViewer.aspx">OData Viewer</a> <span class="collections-footer-seperator">
                </span><a title="Zentity Website" href="http://research.microsoft.com/en-us/projects/zentity">
                    Zentity Website </a><span class="collections-footer-seperator"></span><a title="Pivot Website"
                        href="http://www.getpivot.com">Pivot Website </a>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ContentPlaceHolderID="footerContentPlaceHolder" runat="server" ID="footerContent"
    ViewStateMode="Disabled">
</asp:Content>
