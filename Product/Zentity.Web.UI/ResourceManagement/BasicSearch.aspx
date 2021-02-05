<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="ResourceManagement_BasicSearch" Title="<%$ Resources:Resources, BasicSearch %>" Codebehind="BasicSearch.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <table width="100%">
            <tr class="MasterContentHEAD">
                <td>
                    <asp:Label ID="searchLabel" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Panel ID="searchPanel" runat="server" DefaultButton="GoButton">
                        <table width="100%">
                            <tr>
                                <td align="left" valign="top">
                                    <asp:Label ID="searchForLabel" runat="server"></asp:Label>:
                                </td>
                                <td align="left" valign="top">
                                    <asp:TextBox ID="SearchText" runat="server" Width="400px"></asp:TextBox>
                                    <br />
                                    <asp:RequiredFieldValidator ID="SearchTextRequiredValidator" runat="server" ValidationGroup="Go"
                                        ControlToValidate="SearchText" Display="Dynamic" ErrorMessage=""></asp:RequiredFieldValidator>
                                </td>
                                <td align="left" valign="middle" rowspan="3" width="30%">
                                    <asp:Label ID="SearchExampleLabel" runat="server" CssClass="SearchExample"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td align="left">
                                    <asp:Label ID="pageSizeLabel" runat="server"></asp:Label>:
                                </td>
                                <td align="left">
                                    <asp:DropDownList ID="PageSizeDropDownList" runat="server">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td align="left" colspan="2">
                                    <asp:CheckBox ID="ContentSearchCheckBox" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td align="left" colspan="2">
                                    <asp:Label runat="server" ID="ContentSearchInformationLabel" CssClass="SearchExample" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="3" align="center">
                                    <br />
                                    <asp:Button ID="GoButton" ValidationGroup="Go" runat="server" OnClick="GoButton_Click"
                                        CssClass="Masterbutton" Text="Go" Width="88px" />
                                    <input id="BackButton" runat="server" type="button" onclick="history.back()" class="Masterbutton"
                                        accesskey="B" style="width: 88px" value="Back" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td style="width: 100%; height: 15px">
                    <asp:Label ID="LabelErrorMessage" runat="server" Text="" ForeColor="Red" EnableViewState="false"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <Zentity:ResourceListView ID="SearchResourceListView" runat="server" Width="100%"
                        IsFeedSupported="true"
                        OnOnPageChanged="SearchResourceListView_PageChanged" TitleStyle-CssClass="TitleStyle"
                        ContainerStyle-CssClass="ResourceListViewContainer" ItemStyle-CssClass="ResourceListViewItemContainer"
                        SeparatorStyle-CssClass="ResourceListViewInternalDynDiv" OnOnSortButtonClicked="ResourceListView_OnSortButtonClicked"
                        ButtonStyle-CssClass="ResourceListViewButtonStyle" ViewUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}">
                    </Zentity:ResourceListView>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
