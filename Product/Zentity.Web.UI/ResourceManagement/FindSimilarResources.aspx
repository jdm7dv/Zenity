<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_FindSimilarResources" Title="Find Similar Resources" Codebehind="FindSimilarResources.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="server">
    <div class="RightContentInnerPane">
        <asp:Panel ID="PanelStep1" runat="server">
            <table id="Table2" runat="server" class="ResourceListViewContainer" border="0" cellpadding="0"
                cellspacing="0">
                <tr style="width: 100%" class="MasterContentHEAD">
                    <td colspan="2">
                        <asp:Label ID="LabelMatchCriteria" CssClass="MasterContentHEAD" runat="server" Text="Please select at least
                        one of the below properties to get similar records:"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <br />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:Label ID="LabelError" runat="server" Text="" Visible="false"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td style="width: 33%" valign="baseline">
                        <asp:Label ID="MinimumPerMatchLabel" Font-Bold="true" runat="server" Text="<%$ Resources:Resources, SimilarityMatchLabelPercentageText %>"
                            Visible="true">
                        </asp:Label>
                        <asp:Label ID="RequireFieldLabel" runat="server" Text="*" ForeColor="Red" Visible="true">
                        </asp:Label>
                    </td>
                    <td align="left">
                        <asp:TextBox ID="MinPerMatchTextBox" runat="server" Width="20%" MaxLength="15" TabIndex="1"
                            AutoPostBack="true" AccessKey="N" Text=""></asp:TextBox>
                        <br />
                        <asp:RangeValidator ID="MinPerMatchRangeValidator" Type="Integer" runat="server"
                            Text="<%$ Resources:Resources,SimilarityMatchValidatorPercentageRangeText%>"
                            ErrorMessage="Minimum Percentage" SetFocusOnError="True" ValidationGroup="CreateCategory"
                            ControlToValidate="MinPerMatchTextBox" MinimumValue="1" MaximumValue="100">
                        </asp:RangeValidator>
                    </td>
                </tr>
                <tr style="width: 100%">
                    <td colspan="2">
                        <asp:Label ID="SelectPropertyLabel" runat="server" Text="<%$ Resources:Resources,SimilarityMatchLabelSelectProperties%>"
                            Visible="true"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <br />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:Panel ID="Panel1" runat="server" CssClass="TopAuthorsTable">
                            <div class="OverflowStretch">
                                <asp:Table ID="TableAllProperties" runat="server" CellSpacing="0" Width="100%">
                                    <asp:TableHeaderRow CssClass="hStyle">
                                        <asp:TableHeaderCell Text="Field Name"></asp:TableHeaderCell>
                                        <asp:TableHeaderCell Text="Value to be matched"></asp:TableHeaderCell>
                                    </asp:TableHeaderRow>
                                </asp:Table>
                            </div>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <br />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:Button ID="ButtonSearch" ValidationGroup="CreateCategory" runat="server" OnClick="ButtonSearch_Click"
                            Text="Find Similar" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <br />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:Label ID="LabelCriteriaRequire" runat="server" Text="" ForeColor="Red" Visible="false"></asp:Label>
                    </td>
                </tr>
                <tr style="width: 100%; height: 15px" class="MasterContentHEAD">
                    <td colspan="2">
                        <input type="hidden" id="hiddenPanel" class="Masterbutton" runat="server" value="" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="PanelStep2" runat="server" Visible="false">
            <div class="OverflowStretch">
                <table id="Table1" runat="server" border="0" class="ResourceListViewContainer" cellpadding="0"
                    cellspacing="0" width="100%">
                    <tr style="width: 100%" class="MasterContentHEAD">
                        <td>
                            <asp:Label ID="LabelMatchingRecords" runat="server" CssClass="MasterContentHEAD"
                                Text="<%$ Resources:Resources, SimilarityMatchLabelMatchingRecordsText %>"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Panel ID="PanelResultGrid" runat="server">
                                <table>
                                    <tr>
                                        <td>
                                            <div>
                                                <asp:GridView ID="GridViewMatchingRecord" runat="server" GridLines="None" AutoGenerateColumns="False"
                                                    Width="100%" PageSize="10" CssClass="ResourceGridStyle">
                                                    <AlternatingRowStyle CssClass="arStyle" />
                                                    <RowStyle CssClass="rStyle" />
                                                    <HeaderStyle CssClass="hStyle" />
                                                    <Columns>
                                                        <asp:HyperLinkField DataTextField="Title" HeaderText="Title" Target="_blank" DataNavigateUrlFields="Id"
                                                            DataNavigateUrlFormatString="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}" />
                                                        <asp:BoundField DataField="PercentageMatch" HeaderText="Percentage Match" />
                                                    </Columns>
                                                </asp:GridView>
                                                <Zentity:Pager ID="pager" runat="server" />
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="LabelRecordNotMatch" runat="server" Visible="false"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <br />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Button ID="ButtonCreateNew" runat="server" OnClick="CreateNewResource_Click"
                                                Text="Create New" />
                                            <input id="BackButton" type="button" onclick="history.back()" value="Back" class="Masterbutton" />
                                        </td>
                                    </tr>
                                </table>
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr style="width: 100%">
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="errorMessage" runat="server" Visible="false" Text="Fail to add resource."></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <br />
                        </td>
                    </tr>
                    <tr style="width: 100%; height: 15px" class="MasterContentHEAD">
                        <td>
                            <input type="hidden" id="hidden1" runat="server" value="" />
                        </td>
                    </tr>
                </table>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
