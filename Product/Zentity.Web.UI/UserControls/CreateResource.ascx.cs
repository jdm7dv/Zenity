// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.Web.UI;
using Zentity.Web.UI.ToolKit;
using System.Globalization;
using System.Data;
using Zentity.Security.Authentication;

public partial class UserControls_CreateResource : System.Web.UI.UserControl
{
    #region Constants

    #region Public

    public const string Blank_Space = " ";

    #endregion Public

    #region Private

    private const string _contactRes = "Contact";
    private const string _personRes = "Person";
    private const string _organizationRes = "Organization";
    private const string _createResPage = "CreateResource.aspx";
    private const string _typeName = "TypeName";
    private const string _questionMark = "?";
    private const int _titleLength = 40;
    private const string _createResourceValidationGroup = "CreateResource";
    private const string _defaultResourceType = "Resource";
    private const string _resourceManagementUrl = "~/ResourceManagement/FindSimilarResources.aspx?Id=";

    #endregion Private

    #endregion Constants

    #region Member variables

    private string typeName = string.Empty;
    bool isEditMode;
    private bool _isResourceExist = true;
    private Guid _resourceId = Guid.Empty;

    #endregion

    /// <summary>
    /// Gets or sets resource type in Add mode.
    /// </summary>   
    public string ResourceType
    {
        get
        {
            return typeName;
        }
        set
        {
            typeName = value;
        }
    }

    /// <summary>
    /// Gets or sets Core.Resource id property.
    /// </summary>     
    public Guid ResourceId
    {
        get
        {
            return _resourceId;
        }
        set
        {
            _resourceId = value;
        }
    }

    #region Events

    public event EventHandler<EventArgs> OnResourceSave;

    #endregion

    #region Event Handlers

    protected void Page_Init(object sender, EventArgs e)
    {
        InitializeFieldVariables();
        if (!IsPostBack)
        {
            ChangeButtonLabels();
        }

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        #region Set authentication token
        this.resourceProperties.AuthenticatedToken = Session[Constants.AuthenticationTokenKey] as AuthenticatedToken;
        #endregion
    }

    protected void Submit_Click(object sender, EventArgs e)
    {
        // if resource does not exist then submit operation would not be performed.
        if (!this._isResourceExist)
        {
            return;
        }
        errorMessage.Text = string.Empty;
        errorMessage.Visible = false;
        if (Page.IsValid)
        {
            bool result;
            try
            {
                result = SaveResource();
            }
            catch (UpdateException)
            {
                resourceTable.Visible = false;
                errorMessage.Visible = true;
                errorMessage.Text = Resources.Resources.LectureImageSize;
                return;
            }
            catch (Exception)
            {
                throw;
            }
            if (result)
            {
                Resource resource = resourceProperties.GetResourceDetails();
                if (isEditMode)
                {
                    Utility.ShowMessage(errorMessage, string.Format(CultureInfo.CurrentCulture,
                        Resources.Resources.AlertResourceEdited, typeName,
                        Utility.FitString(resource.Title, 40)), false);
                }
                else
                {
                    Utility.ShowMessage(errorMessage, string.Format(CultureInfo.CurrentCulture,
                        Resources.Resources.AlertResourceAdded, typeName,
                        Utility.FitString(resource.Title, 40)), false);
                }
                if (OnResourceSave != null)
                    OnResourceSave(this, new EventArgs());

            }
            else
            {
                if (isEditMode)
                {
                    Utility.ShowMessage(errorMessage, string.Format(CultureInfo.InvariantCulture,
                         Resources.Resources.ResourceUpdateFailure, typeName), true);
                }
                else
                {
                    Utility.ShowMessage(errorMessage, string.Format(CultureInfo.InvariantCulture,
                        Resources.Resources.ResourceCreateFailure, typeName), true);
                }
            }
        }
    }

    protected void SimilarRecords_Click(object sender, EventArgs e)
    {
        errorMessage.Text = string.Empty;
        errorMessage.Visible = false;
        Resource resource = resourceProperties.GetResourceDetails();
        Session[resource.Id.ToString("D")] = resource;
        Server.Transfer(_resourceManagementUrl + resource.Id.ToString("D"));
    }

    #endregion

    #region Methods

    #region Private

    private void InitializeFieldVariables()
    {
        if (ResourceId != Guid.Empty)
        {
            isEditMode = true;
            ButtonSimilarMatch.Visible = false;
            resourceProperties.ControlMode = ResourcePropertiesOperationMode.Edit;
            resourceProperties.ResourceId = ResourceId;
        }
        else
        {
            resourceProperties.ControlMode = ResourcePropertiesOperationMode.Add;
            using (ResourceDataAccess dataAccess = new ResourceDataAccess())
            {
                ResourceType resTypeObj = dataAccess.GetResourceType(typeName);

                if (resTypeObj != null)
                {
                    resourceProperties.ResourceType = resTypeObj.Name;
                }
                else
                {
                    HideResourcePropertyControl(Resources.Resources.InvalidResourceType);
                    return;
                }
            }
        }

        // Handle scenario which is "a resource deleted by one user and another user operating on the resource".       
        using (ResourceDataAccess resDataAccess = new ResourceDataAccess())
        {
            if (isEditMode)
            {
                Resource resource = resDataAccess.GetResource(ResourceId);

                if (resource == null)
                {
                    this.HideResourcePropertyControl(Resources.Resources.ResourceNotFound);
                    this._isResourceExist = false;
                    return;
                }

                typeName = resource.GetType().Name;
                Page.Title = Resources.Resources.Edit + Blank_Space + Utility.FitString(Utility.UpdateEmptyTitle(resource.Title),
                    _titleLength) + "(" + typeName + ")";
            }
            else
            {
                Page.Title = Resources.Resources.Create + Blank_Space + Utility.FitString(typeName, _titleLength);
            }
        }

        resourceProperties.ResourceType = typeName;
        resourceProperties.Title = string.Format(CultureInfo.InvariantCulture,
            Resources.Resources.ResourceLabelResourcePropertiesText, typeName);
        resourceProperties.ValidationRequired = true;
        ButtonSimilarMatch.ValidationGroup = _createResourceValidationGroup;
        SubmitButton.ValidationGroup = _createResourceValidationGroup;
        resourceProperties.ValidationGroup = _createResourceValidationGroup;
    }

    /// <summary>
    ///     Hide resource property control and show error message.
    /// </summary>
    /// <param name="message">Error message</param>
    private void HideResourcePropertyControl(string message)
    {
        resourceTable.Visible = false;
        errorMessage.Visible = true;
        errorMessage.Text = message;

        if (isEditMode)
        {
            Page.Title = Resources.Resources.Edit;
        }
        else
        {
            Page.Title = Resources.Resources.Create;
        }
    }

    private void ChangeButtonLabels()
    {
        if (isEditMode)
        {
            SubmitButton.Text = Resources.Resources.ButtonUpdateText;
        }
        else
        {
            SubmitButton.Text = Resources.Resources.ButtonSaveText;
        }
    }

    private bool SaveResource()
    {
        bool result = false;

        result = resourceProperties.SaveResourceProperties();
        if (result)
        {
            ResourceId = resourceProperties.ResourceId;
        }

        return result;
    }

    #endregion

    #endregion
}
