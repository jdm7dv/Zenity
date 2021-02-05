<%@ Page Language="C#" MasterPageFile="~/Master.master" AutoEventWireup="true"
    Inherits="BibTeXImport" Title="<%$ Resources:Resources, ImportCitationLabelTitleText %>" Codebehind="BibTeXImport.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="mainCopy" runat="Server">
    <div class="RightContentInnerPane">
        <asp:Label ID="labelError" runat="server" Text="Label"></asp:Label>
        <asp:Panel ID="panelImport" runat="server">
            <table style="width: 100%;">
                <tr style="width: 100%;">
                    <td>
                        <div class="TitleStyle">
                            <asp:Label ID="ImportHeaderTitle" Text="<%$ Resources:Resources, ImportCitationLabelTitleText %>"
                                runat="server">
                            </asp:Label>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <br />
                    </td>
                </tr>
                <tr>
                    <td>
                        <label id="ResourceTitleLabel" runat="server" style="font-weight: bold">
                        </label>
                        <asp:HyperLink ID="LabelImportResourceTitle" runat="server"></asp:HyperLink>
                    </td>
                </tr>
                <tr>
                    <td>
                        <br />
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="LabelMessage" runat="server" Text="" Visible="false"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Wizard ID="Wizard1" runat="server" ActiveStepIndex="0" Height="90%" Width="98%"
                            StepNextButtonText="Import" StepPreviousButtonText="Back">
                            <StartNavigationTemplate>
                                <asp:Button ID="Step1NextButton" runat="server" Text="Next" OnClick="Step1NextButton_Click"
                                    Width="20%" />
                            </StartNavigationTemplate>
                            <WizardSteps>
                                <asp:WizardStep runat="server">
                                    <div style="vertical-align: top">
                                        Click on the Browse button to upload a BibTeX file. Parsed contents of the uploaded
                                        file will be displayed on the next step.
                                        <br />
                                        <br />
                                        <table cellspacing="0" cellpadding="0" border="0">
                                            <tr style="width: 100%; vertical-align: top">
                                                <td>
                                                    <asp:FileUpload ID="fileUploadBibTeXFile" contentEditable="false" runat="server" />
                                                    <br />
                                                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorFileUPload" runat="server"
                                                        ControlToValidate="fileUploadBibTeXFile" ErrorMessage="" Display="Dynamic"></asp:RequiredFieldValidator>
                                                    <asp:RegularExpressionValidator ID="ragularExpBibTeXFile" runat="server" ControlToValidate="fileUploadBibTeXFile"
                                                        ValidationExpression="^.+\.(bib|BIB){1}$" Display="Dynamic"></asp:RegularExpressionValidator>
                                                    <%--  <asp:Label ID="LabelFileUploadMessage" runat="server" Text="Please browse and select a BibTeX file to upload."
                                        Visible="false" ForeColor="Red"></asp:Label>--%>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </asp:WizardStep>
                                <asp:WizardStep runat="server">
                                    <asp:Panel ID="PanelParserError" runat="server">
                                        <table style="width: 100%;">
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TitleStyle">
                                                        <asp:Label ID="ErrorEntriesLabel" runat="server" Font-Bold="True" Font-Size="Small"
                                                            Text="<%$ Resources:Resources, BibtexImportErrorEntries %>"></asp:Label>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <br />
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TitleStyle">
                                                        <asp:Label ID="ParserErrorsLabel" runat="server" Font-Bold="True" Font-Size="Small"
                                                            Text="Parser Errors"></asp:Label>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <asp:TextBox ID="ParserErrorsTxtBox" runat="server" TextMode="MultiLine" Rows="5"
                                                        ReadOnly="True" Width="625px"></asp:TextBox>
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <br />
                                    <asp:Panel ID="PanelMappingErrors" runat="server">
                                        <table style="width: 100%;">
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TitleStyle">
                                                        <asp:Label ID="MappingErrorsLabel" runat="server" Font-Bold="True" Font-Size="Small"
                                                            Text="Mapping Errors"></asp:Label>
                                                    </div>
                                                </td>
                                            </tr>
                                            <%--<tr>
                                <td>
                                    <br />
                                </td>
                             </tr>--%>
                                            <tr>
                                                <td>
                                                    <asp:TextBox ID="MappingErrorsTxtBox" runat="server" TextMode="MultiLine" Rows="5"
                                                        ReadOnly="True" Width="625px"></asp:TextBox>
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <br />
                                    <asp:Panel ID="PanelParsedEntries" runat="server">
                                        <table style="width: 100%;">
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TitleStyle">
                                                        <asp:Label Font-Bold="True" Font-Size="Small" runat="server" ID="Label2" Text="Successfully parsed entries"></asp:Label>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <br />
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TitleStyle">
                                                        <asp:Label ID="ResourceWithCitationLabel" runat="server" Font-Bold="True" Font-Size="Small"
                                                            Text="Resources exist and already cited"></asp:Label>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <div class="TableContainer">
                                                        <Zentity:ResourceDataGridView ID="ResourcesCited" runat="server" EnableDelete="false"
                                                            ShowCommandColumns="false" AllowSorting="True" CssClass="ResourceGridViewContainer"
                                                            Width="100%" ViewColumn="Title" EntityType="ScholarlyWork" OnPageChanged="ResourceDataGridView_PageChanged"
                                                            ViewUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}">
                                                            <AlternatingRowStyle CssClass="arStyle" />
                                                            <RowStyle CssClass="ResourceGridViewItemContainer" />
                                                            <HeaderStyle CssClass="ResourceGridViewTitleStyle" />
                                                            <DisplayColumns>
                                                                <Zentity:ZentityGridViewColumn ColumnName="Title" HeaderText="Title">
                                                                </Zentity:ZentityGridViewColumn>
                                                                <Zentity:ZentityGridViewColumn ColumnName="DateAdded" HeaderText="Date Added">
                                                                </Zentity:ZentityGridViewColumn>
                                                            </DisplayColumns>
                                                        </Zentity:ResourceDataGridView>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <br />
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TitleStyle">
                                                        <asp:Label ID="ResourceWithoutCitationLabel" runat="server" Font-Bold="True" Font-Size="Small"
                                                            Text="Resources exist but not cited"></asp:Label>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TableContainer">
                                                        <Zentity:ResourceDataGridView ID="ResourcesToBeCited" runat="server" EnableDelete="false"
                                                            ShowCommandColumns="false" AllowSorting="True" CssClass="ResourceGridViewContainer"
                                                            Width="100%" ViewColumn="Title" EntityType="ScholarlyWork" OnPageChanged="ResourceDataGridView_PageChanged"
                                                            ViewUrl="~/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={0}">
                                                            <RowStyle CssClass="ResourceGridViewItemContainer" />
                                                            <HeaderStyle CssClass="ResourceGridViewTitleStyle" />
                                                            <DisplayColumns>
                                                                <Zentity:ZentityGridViewColumn ColumnName="Title" HeaderText="Title">
                                                                </Zentity:ZentityGridViewColumn>
                                                                <Zentity:ZentityGridViewColumn ColumnName="DateAdded" HeaderText="Date Added">
                                                                </Zentity:ZentityGridViewColumn>
                                                            </DisplayColumns>
                                                        </Zentity:ResourceDataGridView>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <br />
                                                    <asp:Label ID="ResourcesNotToBeCitedLabel" runat="server" Font-Size="Small" ForeColor="Red"
                                                        Text="<%$ Resources:Resources, MsgResourcesCannotBeCited %>"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TableContainer">
                                                        <Zentity:ResourceDataGridView ID="ResourcesNotToBeCited" runat="server" EnableDelete="false"
                                                            ShowCommandColumns="false" AllowSorting="True" CssClass="ResourceGridViewContainer"
                                                            Width="100%" ViewColumn="Title" EntityType="ScholarlyWork" OnPageChanged="ResourceDataGridView_PageChanged"
                                                            ViewUrl="javascript:return false;">
                                                            <RowStyle CssClass="ResourceGridViewItemContainer" />
                                                            <HeaderStyle CssClass="ResourceGridViewTitleStyle" />
                                                            <DisplayColumns>
                                                                <Zentity:ZentityGridViewColumn ColumnName="Title" HeaderText="Title">
                                                                </Zentity:ZentityGridViewColumn>
                                                                <Zentity:ZentityGridViewColumn ColumnName="DateAdded" HeaderText="Date Added">
                                                                </Zentity:ZentityGridViewColumn>
                                                            </DisplayColumns>
                                                        </Zentity:ResourceDataGridView>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <br />
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <div class="TitleStyle">
                                                        <asp:Label ID="NewResourcesLabel" runat="server" Font-Bold="True" Font-Size="Small"
                                                            Text="New resources"></asp:Label>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <div class="TableContainer">
                                                        <Zentity:ResourceDataGridView ID="ResourcesToBeImported" runat="server" EnableDelete="false"
                                                            ShowCommandColumns="false" AllowSorting="True" CssClass="ResourceGridViewContainer"
                                                            Width="100%" EntityType="ScholarlyWork" OnPageChanged="ResourceDataGridView_PageChanged"
                                                            ViewUrl="javascript:return false;">
                                                            <RowStyle CssClass="ResourceGridViewItemContainer" />
                                                            <HeaderStyle CssClass="ResourceGridViewTitleStyle" />
                                                            <DisplayColumns>
                                                                <Zentity:ZentityGridViewColumn ColumnName="Title" HeaderText="Title">
                                                                </Zentity:ZentityGridViewColumn>
                                                            </DisplayColumns>
                                                        </Zentity:ResourceDataGridView>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr style="width: 100%;">
                                                <td>
                                                    <br />
                                                    <asp:Label ID="ResourcesNotToBeImportedLabel" runat="server" Font-Size="Small" ForeColor="Red"
                                                        Text="<%$ Resources:Resources, MsgResourcesCannotBeImported %>"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <div class="TableContainer">
                                                        <Zentity:ResourceDataGridView ID="ResourcesNotToBeImported" runat="server" EnableDelete="false"
                                                            ShowCommandColumns="false" AllowSorting="True" CssClass="ResourceGridViewContainer"
                                                            Width="100%" EntityType="ScholarlyWork" OnPageChanged="ResourceDataGridView_PageChanged"
                                                            ViewUrl="javascript:return false;">
                                                            <RowStyle CssClass="ResourceGridViewItemContainer" />
                                                            <HeaderStyle CssClass="ResourceGridViewTitleStyle" />
                                                            <DisplayColumns>
                                                                <Zentity:ZentityGridViewColumn ColumnName="Title" HeaderText="Title">
                                                                </Zentity:ZentityGridViewColumn>
                                                            </DisplayColumns>
                                                        </Zentity:ResourceDataGridView>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <br />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <asp:Label ID="ImportError" runat="server" Visible="false" ForeColor="Red" Font-Bold="true"></asp:Label>
                                </asp:WizardStep>
                                <asp:WizardStep runat="server">
                                    <div class="TableContainer">
                                        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" ShowHeader="true"
                                            CssClass="ResourceGridViewContainer" Width="100%">
                                            <RowStyle CssClass="ResourceGridViewItemContainer" />
                                            <HeaderStyle CssClass="ResourceGridViewTitleStyle" />
                                            <Columns>
                                                <asp:TemplateField HeaderText="External Resource">
                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="CheckBox1" runat="server" Checked="false" Enabled="true" />
                                                        <asp:HiddenField ID="Hidden1" Value='<%# Bind("ID") %>' runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:BoundField DataField="Title" HeaderText="Title" />
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                </asp:WizardStep>
                            </WizardSteps>
                            <FinishNavigationTemplate>
                                <br />
                                <asp:Button ID="Step3FinishButton" runat="server" Text="Update & Finish" OnClick="Step3FinishButton_Click" />
                            </FinishNavigationTemplate>
                            <StepNavigationTemplate>
                                <table width="100%">
                                    <tr>
                                        <td style="width: 50%; text-align: left">
                                            <asp:Button ID="Step2BackButton" runat="server" Text="Back" OnClick="Step2BackButton_Click" />
                                        </td>
                                        <td style="width: 50%; text-align: right">
                                            <asp:Button ID="Step2ImportButton" runat="server" Text="Import" OnClick="Step2ImportButton_Click" />
                                            &nbsp;&nbsp;&nbsp;
                                        </td>
                                    </tr>
                                </table>
                            </StepNavigationTemplate>
                        </asp:Wizard>
                    </td>
                </tr>
                <tr style="width: 100%;" class="ContentHEAD">
                    <td>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </div>
</asp:Content>
