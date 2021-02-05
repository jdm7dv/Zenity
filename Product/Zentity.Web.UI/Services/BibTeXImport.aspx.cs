// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using System.Collections.Generic;
using Zentity.Platform;
using System.Text;
using System.Reflection;
using System.Data.Objects;
using Zentity.Web.UI;
using Zentity.Web.UI.ToolKit;
using Zentity.Security.Authentication;
using System.Globalization;

public partial class BibTeXImport : ZentityBasePage
{
    #region Constants

    #region Private

    const string _bibtexImportResource = "ResourcesToBeImported";
    const string _bibtexCitedResource = "ResourcesCited";
    const string _bibtexResourceToBeCitedId = "ResourcesToBeCitedId";
    const string _bibtexResourceToBeCited = "ResourcesToBeCited";
    const string _htmlBreakLine = "<br />";
    private const string _scriptManagerId = "ScriptManager";
    private const string _stepNextButtonId = "Step1NextButton";
    private const string _templateContainerID = "StartNavigationTemplateContainerID";
    private const string _stepNavTemplateContainerID = "StepNavigationTemplateContainerID";
    private const string _step2BackButton = "Step2BackButton";
    private const string _step2ImportButton = "Step2ImportButton";
    private const string _finishNavTemplateContainerID = "FinishNavigationTemplateContainerID";
    private const string _step2FinishButton = "Step3FinishButton";
    private const string _resourceDetailUrl = "../ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id=";
    private const string _resourcesCitedKey = "resourcesCitedKey";
    private const string _resourcesToBeCitedKey = "resourcesToBeCitedKey";
    private const string _resourcesNotToBeCitedKey = "resourcesNotToBeCitedKey";
    private const string _resourcesNotToBeImportedKey = "resourcesNotToBeImportedKey";

    #endregion

    #endregion

    #region Member variables

    #region Private

    private const string _queryStringKey = "id";
    private ScholarlyWork _scholarlyWorkObj;
    private bool _isResExist = true;

    #endregion

    #endregion

    #region Properties

    private ICollection<ScholarlyWork> ResourcesCitedDataSource
    {
        get { return ViewState[_resourcesCitedKey] as ICollection<ScholarlyWork>; }
        set { ViewState[_resourcesCitedKey] = value; }
    }

    private ICollection<ScholarlyWork> ResourcesToBeCitedDataSource
    {
        get { return ViewState[_resourcesToBeCitedKey] as ICollection<ScholarlyWork>; }
        set { ViewState[_resourcesToBeCitedKey] = value; }
    }

    private ICollection<ScholarlyWork> ResourcesNotToBeCitedDataSource
    {
        get { return ViewState[_resourcesNotToBeCitedKey] as ICollection<ScholarlyWork>; }
        set { ViewState[_resourcesNotToBeCitedKey] = value; }
    }

    private ICollection<ScholarlyWork> ResourcesNotToBeImportedDataSource
    {
        get { return ViewState[_resourcesNotToBeImportedKey] as ICollection<ScholarlyWork>; }
        set { ViewState[_resourcesNotToBeImportedKey] = value; }
    }

    #endregion

    #region Methods

    #region Private

    /// <summary>
    ///     Return string with end with "..." if string is big.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maximumChars"></param>
    /// <returns></returns>
    private string FitString(string value, int maximumChars)
    {
        if (maximumChars < 1)
        {
            return string.Empty;
        }
        string stringToCompress = value;
        if (stringToCompress.Length > maximumChars)
        {
            string strResult = stringToCompress.Substring(0, maximumChars);
            strResult += "...";
            return (strResult);
        }
        else
        {
            return value;
        }
    }

    private void LocalizePage()
    {
        labelError.Text = Resources.Resources.ResourceBibtextimportError;
        LabelMessage.Text = Resources.Resources.ResourceBibtextimportMessage;

        ParserErrorsLabel.Text = Resources.Resources.ResourceBibtextimportParseerror;

        MappingErrorsLabel.Text = Resources.Resources.ResourceBibtextimportMappingerror;
        Label2.Text = Resources.Resources.ResourceBibtextimportSuccesspareseentries;

        ResourceWithCitationLabel.Text = Resources.Resources.ResourceBibtextimportResourcewithcitations;
        ResourceWithoutCitationLabel.Text = Resources.Resources.ResourceBibtextimportResourcewithoutcitations;
        NewResourcesLabel.Text = Resources.Resources.ResourceBibtextimportNewresource;

        RequiredFieldValidatorFileUPload.ErrorMessage = Resources.Resources.ResourceBibtextimportFileuploadmessage;
        ragularExpBibTeXFile.ErrorMessage = Resources.Resources.ResourceBibtextimportFileuploadmessage;

        // Get reference of buttons "Step2BackButton" and "Step2ImportButton" from template "StepNavigationTemplateContainerID" and 
        // set their text property. 
        Control stepNavigationTemplate = Wizard1.FindControl(_stepNavTemplateContainerID) as Control;
        if (stepNavigationTemplate != null)
        {

            Button step2BackButton = stepNavigationTemplate.FindControl(_step2BackButton) as Button;
            if (step2BackButton != null)
            {
                step2BackButton.Text = Resources.Resources.BibTexImportStep2BackButtonText;
            }
            Button step2ImportButton = stepNavigationTemplate.FindControl(_step2ImportButton) as Button;
            if (step2ImportButton != null)
            {
                step2ImportButton.Text = Resources.Resources.BibTexImportStep2ButtonText;
            }
        }

        // Get reference of button "Step3FinishButton" from template "FinishNavigationTemplate" and 
        // assign it's text property. 
        Control finishNavigationTemplate = Wizard1.FindControl(_finishNavTemplateContainerID) as Control;
        if (finishNavigationTemplate != null)
        {
            Button step3Button = finishNavigationTemplate.FindControl(_step2FinishButton) as Button;
            if (step3Button != null)
            {
                step3Button.Text = Resources.Resources.BibTexImportStep3ButtonText;
            }
        }
    }

    private void DisplayError(string message)
    {
        this.panelImport.Visible = false;
        this.labelError.Text = message;
        this.labelError.Visible = true;
    }

    private void DisplayCannotImportMessage(string message)
    {
        this.ImportError.Text = message;
        this.ImportError.Visible = true;
    }

    private void HideError()
    {
        this.panelImport.Visible = true;
        this.labelError.Visible = false;
        this.ImportError.Visible = false;
        this.LabelMessage.Visible = false;
        //this.LabelFileUploadMessage.Visible = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="grdViewToBind"></param>
    /// <param name="resourceExistsAndCited"></param>
    /// <param name="visible"></param>
    private void DisplayResources(GridView grdViewToBind, ICollection<ScholarlyWork> resourcesToBind,
                                    bool visible)
    {
        if (resourcesToBind != null)
        {
            grdViewToBind.DataSource = new List<ScholarlyWork>(resourcesToBind);
            grdViewToBind.DataBind();
        }
        grdViewToBind.Visible = visible;
    }

    private void DisplayParsingErrors(ICollection<BibTeXParserException> parserErrors,
                                        ICollection<BibTeXMappingException> mappingErrors)
    {
        int count = 1;
        if (parserErrors.Count > 0)
        {
            StringBuilder parseErrors = new StringBuilder(String.Empty);
            string format = Resources.Resources.BibtexImportParserErrorFormat;
            foreach (BibTeXParserException parsingException in parserErrors)
            {
                parseErrors.AppendLine(string.Format(System.Globalization.CultureInfo.CurrentCulture, format,
                                        count, parsingException.Line, parsingException.Column,
                                        parsingException.ErrorToken, parsingException.Message));
                count++;
            }

            this.ParserErrorsTxtBox.Text = parseErrors.ToString();
            this.PanelParserError.Visible = true;
        }
        else
        {
            this.PanelParserError.Visible = false;
        }

        if (mappingErrors.Count > 0)
        {
            count = 1;
            StringBuilder mErrors = new StringBuilder(String.Empty);
            string format = Resources.Resources.BibtexImportMappingErrorFormat;
            foreach (BibTeXMappingException mappingException in mappingErrors)
            {
                mErrors.AppendLine(String.Format(System.Globalization.CultureInfo.CurrentCulture,
                                        format, count, mappingException.Message));
                count++;
            }
            this.MappingErrorsTxtBox.Text = mErrors.ToString();
            this.PanelMappingErrors.Visible = true;
        }
        else
        {
            this.PanelMappingErrors.Visible = false;
        }
    }

    private ICollection<ScholarlyWork> FilterResourcesBasedOnPermissions(AuthenticatedToken token, string userPermission,
        ICollection<ScholarlyWork> resourceList)
    {
        List<ScholarlyWork> filteredResources = new List<ScholarlyWork>();

        if (token != null && resourceList != null && resourceList.Count > 0)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess())
            {
                if (UserResourcePermissions.Create.Equals(userPermission))
                {

                    if (dataAccess.HasCreatePermission(token))
                    {
                        foreach (ScholarlyWork scholWork in resourceList)
                        {
                            bool isAuthorized = true;
                            foreach (Contact contact in scholWork.Authors.Union(scholWork.Editors))
                            {
                                Contact cFound = dataAccess.GetResources<Contact>(ResourceStringComparison.Equals, contact.Title).FirstOrDefault();
                                if (cFound != null && !dataAccess.AuthorizeUser(token, userPermission, cFound.Id))
                                {
                                    isAuthorized = false;
                                    break;
                                }
                            }
                            if (isAuthorized)
                                filteredResources.Add(scholWork);
                        }
                    }
                }
                else
                {
                    foreach (ScholarlyWork scholWork in resourceList)
                    {
                        if (dataAccess.AuthorizeUser(token, userPermission, scholWork.Id))
                            filteredResources.Add(scholWork);
                    }
                }
            }
        }
        return filteredResources;
    }

    #endregion

    #endregion

    #region Event handlers

    #region Protected

    protected void Page_Load(object sender, EventArgs e)
    {
        Control startNavigationTemplate = Wizard1.FindControl(_templateContainerID) as Control;
        if (startNavigationTemplate != null)
        {
            Button stepNextButton = startNavigationTemplate.FindControl(_stepNextButtonId) as Button;
            if (stepNextButton != null)
            {
                stepNextButton.Text = Resources.Resources.BibTexImportStep1NextButtonText;
            }
        }

        // This is required to avoid "PageRequestManagerParserErrorException" exception with update panel
        // because there is rendering problem with update panel.
        Control stepNavigationTemplate = Wizard1.FindControl(_stepNavTemplateContainerID) as Control;
        if (stepNavigationTemplate != null)
        {
            Button step2ImportButton = stepNavigationTemplate.FindControl(_step2ImportButton) as Button;
            if (step2ImportButton != null)
            {
                step2ImportButton.Text = Resources.Resources.BibTexImportStep2ButtonText;
            }
        }

        Guid guid = Guid.Empty;
        this.HideError();

        if (!this.IsPostBack)
        {
            LocalizePage();
        }

        bool isValidGuid = true;

        if (Request.QueryString[_queryStringKey] != null)
        {
            try
            {
                guid = new Guid(Request.QueryString[_queryStringKey]);
            }
            catch (FormatException)
            {
                this.DisplayError(Resources.Resources.InvalidResource);
                isValidGuid = false;
            }
            catch (OverflowException)
            {
                this.DisplayError(Resources.Resources.InvalidResource);
                isValidGuid = false;
            }
        }
        else if (guid == Guid.Empty)
        {
            this.DisplayError(Resources.Resources.BibtexImportMissinfId);
            isValidGuid = false;
        }

        if (isValidGuid)
        {
            using (ResourceDataAccess resourceDAL = new ResourceDataAccess())
            {
                _scholarlyWorkObj = (ScholarlyWork)resourceDAL.GetResource(guid);
                AuthenticatedToken token = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;

                //if user is not having update permission on the subject resource then throw exception.
                if (!resourceDAL.AuthorizeUser(token, UserResourcePermissions.Update, _scholarlyWorkObj.Id))
                {
                    throw new UnauthorizedAccessException(string.Format(CultureInfo.InstalledUICulture,
                        Resources.Resources.MsgUnAuthorizeAccess, UserResourcePermissions.Update));
                }

                if (_scholarlyWorkObj != null)
                {
                    _scholarlyWorkObj.Cites.Load();
                }
                else
                {
                    // Handle scenario which is "a resource deleted by one user and another user operating on the resource".
                    this.DisplayError(Resources.Resources.ResourceNotFound);
                    this._isResExist = false;
                }
            }
            if (!IsPostBack && this.Wizard1.ActiveStepIndex == 0)
            {
                if (_scholarlyWorkObj == null)
                {
                    this.DisplayError(Resources.Resources.ResourceNotFound);
                }
                else
                {
                    ResourceTitleLabel.InnerText = Resources.Resources.ResourceLabelTitleText;
                    LabelImportResourceTitle.Text = FitString(_scholarlyWorkObj.Title, 40);
                    LabelImportResourceTitle.NavigateUrl = _resourceDetailUrl + _scholarlyWorkObj.Id;

                }
            }
        }
    }

    protected void Step1NextButton_Click(object sender, EventArgs e)
    {
        if (this.fileUploadBibTeXFile.HasFile)
        {
            //this.LabelFileUploadMessage.Visible = false;
            using (Stream fileStream = this.fileUploadBibTeXFile.FileContent)
            {
                BibTeXConverter bibConverter = new BibTeXConverter(BibTeXParserBehavior.IgnoreParseErrors);
                ICollection<ScholarlyWork> importResources = (ICollection<ScholarlyWork>)bibConverter.Import(fileStream);

                if (importResources.Count == 0)
                {
                    this.PanelParsedEntries.Visible = false;
                    this.DisplayCannotImportMessage(Resources.Resources.BibtexImportNothingFound);
                    this.DisplayParsingErrors(bibConverter.ParserErrors, bibConverter.MappingErrors);
                }
                else
                {
                    ICollection<ScholarlyWork> scholarlyWorksExistsAndCited = new List<ScholarlyWork>();
                    ICollection<ScholarlyWork> scholarlyWorksExistsButNotCited = new List<ScholarlyWork>();
                    ICollection<Guid> resourceExistsButNotCitedId = new List<Guid>();
                    ICollection<ScholarlyWork> newResources = new List<ScholarlyWork>();
                    IQueryable<ScholarlyWork> citationsOfAScholarlyWork = null;

                    this.PanelParsedEntries.Visible = true;


                    // Create collection because of Linq query operate on "context.Resources" does not accept
                    // equals method in the predicate
                    // Get cites resource of selected resource

                    if (_scholarlyWorkObj != null)
                    {
                        citationsOfAScholarlyWork = _scholarlyWorkObj.Cites.AsQueryable<ScholarlyWork>();
                    }
                    else
                    {
                        this.DisplayError(Resources.Resources.ResourceNotFound);
                        return;
                    }

                    List<ScholarlyWork> actualResourceMatchCriteria = new List<ScholarlyWork>();
                    foreach (ScholarlyWork resource in importResources)
                    {
                        actualResourceMatchCriteria.Clear();
                        using (ResourceDataAccess resourceDAL = new ResourceDataAccess())
                        {
                            var resourcesInZentityContext = resourceDAL.GetResources<ScholarlyWork>
                                (Zentity.Web.UI.ResourceStringComparison.Equals, resource.Title);

                            foreach (ScholarlyWork tempResourcInZentityContext in resourcesInZentityContext)
                            {
                                if (tempResourcInZentityContext.GetType().Equals(resource.GetType()))
                                {
                                    actualResourceMatchCriteria.Add(tempResourcInZentityContext);
                                    break;
                                }
                            }
                        }

                        if (actualResourceMatchCriteria.Count > 0)
                        {
                            var resourceInCites = citationsOfAScholarlyWork.Where(
                                                    citeResource => citeResource.Id.Equals
                                                        (actualResourceMatchCriteria.First<ScholarlyWork>().Id));
                            if (resourceInCites.Count() > 0)
                            {
                                // current resource's title and type matches with cited resource of selected resource
                                scholarlyWorksExistsAndCited.Add(actualResourceMatchCriteria.First());
                            }
                            else
                            {
                                // current resource's title and type matches with resource in the repository but not in cited                                 
                                scholarlyWorksExistsButNotCited.Add(actualResourceMatchCriteria.First());
                                resourceExistsButNotCitedId.Add(actualResourceMatchCriteria.First().Id);
                            }
                        }
                        else
                        {
                            // Current resource is not exist in the repository
                            newResources.Add(resource);
                        }
                    }
                    AuthenticatedToken token = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
                    ICollection<ScholarlyWork> filteredResourceToBeCited = FilterResourcesBasedOnPermissions(token,
                        Constants.PermissionRequiredForAssociation, scholarlyWorksExistsButNotCited);
                    ICollection<ScholarlyWork> resourceNotToBeCited = scholarlyWorksExistsButNotCited.Except(filteredResourceToBeCited).ToList();
                    ICollection<ScholarlyWork> filteredResourceToBeImported = FilterResourcesBasedOnPermissions(token,
                        UserResourcePermissions.Create, newResources);
                    ICollection<ScholarlyWork> resourceNotToBeImported = newResources.Except(filteredResourceToBeImported).ToList();

                    Session.Add(_bibtexImportResource, filteredResourceToBeImported);
                    Session.Add(_bibtexResourceToBeCitedId, filteredResourceToBeCited.Select(tuple => tuple.Id).ToList());
                    ResourcesCitedDataSource = scholarlyWorksExistsAndCited;
                    ResourcesToBeCitedDataSource = filteredResourceToBeCited;
                    ResourcesNotToBeCitedDataSource = resourceNotToBeCited;
                    ResourcesNotToBeImportedDataSource = resourceNotToBeImported;

                    RefreshResults(ResourcesCited, scholarlyWorksExistsAndCited);
                    RefreshResults(ResourcesToBeCited, filteredResourceToBeCited);
                    RefreshResults(ResourcesToBeImported, filteredResourceToBeImported);

                    if (resourceNotToBeCited.Count > 0)
                        RefreshResults(ResourcesNotToBeCited, resourceNotToBeCited);
                    else
                        ResourcesNotToBeCitedLabel.Visible = false;

                    if (resourceNotToBeImported.Count > 0)
                        RefreshResults(ResourcesNotToBeImported, resourceNotToBeImported);
                    else
                        ResourcesNotToBeImportedLabel.Visible = false;

                    this.DisplayParsingErrors(bibConverter.ParserErrors, bibConverter.MappingErrors);
                }
            }
            Wizard1.ActiveStepIndex = 1;
        }
    }

    protected void ResourceDataGridView_PageChanged(object sender, EventArgs e)
    {
        ResourceDataGridView gridView = (ResourceDataGridView)sender;
        if (gridView.ID == ResourcesCited.ID)
        {
            RefreshResults(gridView, ResourcesCitedDataSource);
        }
        else if (gridView.ID == ResourcesToBeCited.ID)
        {
            RefreshResults(gridView, ResourcesToBeCitedDataSource);
        }
        else if (gridView.ID == ResourcesNotToBeCited.ID)
        {
            RefreshResults(gridView, ResourcesNotToBeCitedDataSource);
        }
        else if (gridView.ID == ResourcesNotToBeImported.ID)
        {
            RefreshResults(gridView, ResourcesNotToBeImportedDataSource);
        }
        else
        {
            RefreshResults(gridView, Session[gridView.ID] as ICollection<ScholarlyWork>);
        }
    }

    private void RefreshResults(ResourceDataGridView grdView, ICollection<ScholarlyWork> sourceList)
    {
        IList resultList = null;
        int totalRecords = 0;
        int pageSize = 10;

        if (grdView != null)
        {
            if (sourceList == null) return;

            grdView.Visible = true;

            int fetchedRecords = pageSize * grdView.PageIndex;
            grdView.PageSize = pageSize;
            totalRecords = sourceList.Count;

            if (totalRecords > 0)
            {
                if (totalRecords < fetchedRecords)
                {
                    resultList = null;
                }
                else
                {
                    resultList = sourceList.Skip(fetchedRecords).Take(pageSize).ToList();
                }
            }


            //Update page count
            if (totalRecords > 0)
            {
                if (pageSize > 0 && totalRecords > pageSize)
                {
                    grdView.PageCount = Convert.ToInt32(Math.Ceiling((double)totalRecords / pageSize));
                }
                else
                {
                    grdView.PageCount = 1;
                }
            }
            else
            {
                grdView.PageCount = 0;
            }

            //Update ZentityGridView data source
            grdView.DataSource = resultList;
            grdView.SortDataSource();
            grdView.DataBind();
        }
    }

    protected void Step2BackButton_Click(object sender, EventArgs e)
    {
        Wizard1.ActiveStepIndex = 0;
        this.ParserErrorsTxtBox.Text = String.Empty;
        this.MappingErrorsTxtBox.Text = String.Empty;
    }

    protected void Step2ImportButton_Click(object sender, EventArgs e)
    {

        // if resource does not exist then "import" operation would not be performed.
        if (!this._isResExist)
        {
            return;
        }
        ICollection<ScholarlyWork> resourcesToBeImported = null;
        ICollection<Guid> resourcesToBeCitesOnly = null;

        if (Session.Count > 0)
        {
            // Get resources to be imported in the Zentity
            resourcesToBeImported = (ICollection<ScholarlyWork>)Session[_bibtexImportResource];
            // Get resources for those only citations to be updated
            resourcesToBeCitesOnly = (ICollection<Guid>)Session[_bibtexResourceToBeCitedId];
        }

        if (resourcesToBeImported == null || resourcesToBeCitesOnly == null ||
            (resourcesToBeCitesOnly.Count == 0 && resourcesToBeImported.Count == 0))
        {
            this.DisplayCannotImportMessage(Resources.Resources.BibtexImportNoData);
            return;
        }

        DataSet resourceInfo = null;
        try
        {
            using (ResourceDataAccess resourceDAL = new ResourceDataAccess())
            {
                AuthenticatedToken token = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
                resourceInfo = resourceDAL.ImportBibTeX(token, _scholarlyWorkObj.Id, resourcesToBeImported,
                    resourcesToBeCitesOnly);
            }
        }
        catch (Exception ex)
        {
            this.DisplayCannotImportMessage(Resources.Resources.BibtexImportError + ex.Message +
                _htmlBreakLine + (ex.InnerException != null ? ex.InnerException.Message :
                String.Empty));
            return;

        }
        this.GridView1.DataSource = resourceInfo;
        this.GridView1.DataBind();

        if (resourcesToBeImported.Count == 0)
        {
            this.LabelMessage.Text = Resources.Resources.BibtexImportSuccessful;
            this.Wizard1.Visible = false;
        }
        else
        {
            this.LabelMessage.Text = Resources.Resources.BibtexImportSuccessful + Resources.Resources.BibtexImportUpdateNew;
        }
        this.LabelMessage.Visible = true;

        //Clean up and move to next step
        Wizard1.ActiveStepIndex = 2;
        Session.Remove(_bibtexImportResource);
        Session.Remove(_bibtexResourceToBeCitedId);
        Session.Remove(_bibtexCitedResource);
        Session.Remove(_bibtexResourceToBeCited);
    }

    protected void Step3FinishButton_Click(object sender, EventArgs e)
    {
        // if resource does not exist then "finish" operation would not be performed.
        if (!this._isResExist)
        {
            return;
        }
        Hashtable resourceIdList = new Hashtable();

        foreach (GridViewRow row in this.GridView1.Rows)
        {
            CheckBox checkbox = row.Cells[0].Controls[1] as CheckBox;
            Guid resourceId = new Guid(((HiddenField)(row.Cells[0].Controls[3])).Value);

            if (checkbox != null)
            {
                resourceIdList.Add(resourceId, checkbox.Checked ?
                    Resources.Resources.BibtexImportScopeExternal :
                    Resources.Resources.BibtexImportScopeInternal);
            }
        }

        try
        {
            using (ResourceDataAccess resourceDAL = new ResourceDataAccess())
            {
                resourceDAL.UpdateImportedResourceScope(resourceIdList);
            }

            DisplayResources(GridView1, null, false);

            this.LabelMessage.Text = Resources.Resources.BibtexImportUpdateSuccessful;
            this.LabelMessage.Visible = true;
            this.Wizard1.Visible = false;
        }
        catch (ZentityException ex)
        {
            this.LabelMessage.Visible = false;
            this.DisplayCannotImportMessage(Resources.Resources.BibtexImportUpdateError + ex.Message);
            return;
        }
    }

    #endregion

    #endregion
}
