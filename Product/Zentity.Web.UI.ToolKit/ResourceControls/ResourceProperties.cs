// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Objects.DataClasses;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{ // TODO : Implement Precision and Base for decimal types
    /// <summary>
    ///     This class inherits <see cref="ZentityTable"/> class to display a list 
    ///     of resource properties.
    ///     Custom control for generating resource properties as per resource type.
    /// </summary>
    /// <example>
    ///     The following example shows how to use the <see cref="ResourceProperties"/> control for generating resource properties as per resource type.
    ///     To configure the <see cref="ResourceProperties"/> control, add the following section to web configuration file.
    ///     <code lang="xml">
    ///         &lt;configuration&gt;
    ///           &lt;configSections&gt;
    ///             &lt;!-- Resource properties control configuration. --&gt;
    ///             &lt;section name="resourcePropertiesSettings" type="Zentity.Web.UI.ToolKit.ResourcePropertiesConfigSection, Zentity.Web.UI.ToolKit" /&gt;
    ///           &lt;/configSections&gt;
    ///           &lt;!-- Configures the resource properties control. --&gt;
    ///           &lt;resourcePropertiesSettings&gt;
    ///             &lt;requiredProperties&gt;
    ///               &lt;add name="Title" class="Zentity.Core.Resource" /&gt;
    ///               &lt;add name="GroupName" class="Zentity.Security.Authorization.Group" /&gt;
    ///             &lt;/requiredProperties&gt;
    ///             &lt;emailProperties regularExpression="^([^@\s]+)@((?:[-a-z0-9]+\.)+[a-z]{2,})$"&gt;
    ///               &lt;add name="From" class="Zentity.ScholarlyWorks.Email" /&gt;
    ///               &lt;add name="To" class="Zentity.ScholarlyWorks.Email" /&gt;
    ///               &lt;add name="Email" class="Zentity.ScholarlyWorks.Contact" /&gt;
    ///             &lt;/emailProperties&gt;
    ///             &lt;dateRangeProperties&gt;
    ///               &lt;add name="DateAvailableFrom" endDateName="DateAvailableUntil" class="Zentity.ScholarlyWorks.ScholarlyWork" /&gt;
    ///               &lt;add name="DateValidFrom" endDateName="DateValidUntil" class="Zentity.ScholarlyWorks.ScholarlyWork" /&gt;
    ///               &lt;add name="DateStart" endDateName="DateEnd" class="Zentity.ScholarlyWorks.Lecture" /&gt;
    ///             &lt;/dateRangeProperties&gt;
    ///             &lt;readOnlyProperties&gt;
    ///               &lt;add name="DayPublished" class="Zentity.ScholarlyWorks.Publication" /&gt;
    ///               &lt;add name="MonthPublished" class="Zentity.ScholarlyWorks.Publication" /&gt;
    ///               &lt;add name="YearPublished" class="Zentity.ScholarlyWorks.Publication" /&gt;
    ///               &lt;add name="DateAdded" class="Zentity.Core.Resource" /&gt;
    ///               &lt;add name="DateModified" class="Zentity.Core.Resource" /&gt;
    ///             &lt;/readOnlyProperties&gt;
    ///             &lt;imageProperties regularExpression="^.+\.(jpg|JPG|gif|GIF|png|PNG|bmp|BMP){1}$"&gt;
    ///               &lt;add name="Image" class="Zentity.ScholarlyWorks.Lecture" /&gt;
    ///             &lt;/imageProperties&gt;
    ///             &lt;excludedProperties&gt;
    ///               &lt;add name="Id" class="Zentity.Core.Resource" /&gt;
    ///             &lt;/excludedProperties&gt;
    ///             &lt;orderedProperties&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.Person" order="FirstName,MiddleName,LastName" /&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.Lecture" order="DateStart,DateEnd" /&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.ScholarlyWork" order="DateAvailableFrom,DateAvailableUntil" /&gt;
    ///               &lt;add class="Zentity.ScholarlyWorks.ScholarlyWork" order="DateValidFrom,DateValidUntil" /&gt;
    ///             &lt;/orderedProperties&gt;
    ///           &lt;/resourcePropertiesSettings&gt;
    ///         &lt;/configuration&gt;
    ///     </code>
    ///     
    ///     The code below is the source for ResourceProperties.aspx. 
    ///     It shows an example of using <see cref="ResourceProperties"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head id="Head1" runat="server"&gt;
    ///             &lt;title&gt;ResourceProperties Sample&lt;/title&gt;
    ///             
    ///             &lt;script runat="server"&gt;
    ///                 protected void Save_Click(object sender, EventArgs e)
    ///                 {
    ///                     bool success = ResourceProperties1.SaveResourceProperties();
    ///                     if(success)
    ///                     {
    ///                         StatusLabel.Text = "Resource added successfully. Resource id: " + ResourceProperties1.ResourceId;
    ///                     }
    ///                     else
    ///                     {
    ///                         StatusLabel.Text = "Resource could not be added.";
    ///                     }
    ///                 }
    ///             &lt;/script&gt;
    ///             
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="mainForm" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;zentity:ResourceProperties id="ResourceProperties1" runat="server" 
    ///                     ResourceType="Resource" IsSecurityAwareControl="false"&gt;
    ///                 &lt;/zentity:ResourceProperties&gt;
    ///                 &lt;br /&gt;
    ///                 &lt;asp:Button ID="SaveButton" runat="server" Text="Save" OnClick="Save_Click" /&gt;
    ///                 &lt;br /&gt;
    ///                 &lt;asp:Label ID="StatusLabel" runat="server" /&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    public class ResourceProperties : ZentityTable
    {
        #region Constants

        #region Private

        private const int _imageWidth = 32;
        private const int _imageHeight = 32;
        private const string _thumbnailImage = "ThumbnailImage";
        private const string _javascriptFile = "JavascriptFile";
        private const string _resourcePropertyScriptPath = "Zentity.Web.UI.ToolKit.ResourceControls.ResourcePropertyScript.js";
        private const string _noImagePath = "Zentity.Web.UI.ToolKit.Image.NoImage.JPG";
        private const string _thumbnailCallbackUrl = "ThumbnailCallback.ashx?ResourceId=";
        private const string _calendarImage = "Zentity.Web.UI.ToolKit.Image.Calendar_scheduleHS.png";
        private const int _minimumDate = 1899;
        private const int _maximumYearDate = -7920;
        private const int _maximumMonthDate = -6;
        private const int _maximumDayDate = -25;
        private const string _newLine = "</br>";
        private const string _space = " ";
        private const string _star = "*";
        private const string _htmlSpace = "&nbsp;";
        private const string _commonJavascriptFile = "CommonJavascriptFile";
        private const string _commonScriptPath = "Zentity.Web.UI.ToolKit.Scripts.CommonScript.js";
        private const string _onBlurEvent = "onblur";
        private const string _CauseValidationFunction = "javascript: CauseValidations('{0}');";

        #endregion

        #endregion

        #region Member Variables

        #region Private

        private string _resourceType;
        //private ResourceType _resourceType;
        private Guid _resourceId = Guid.Empty;
        private Resource _resource;
        private string _styleClass;
        private bool _validationRequired = true;
        private string _validationGroup;
        private string _validCharactersRegularExpression;
        private ResourcePropertiesOperationMode _controlMode;
        private int? _maxCharsForTextBox;

        private Collection<ScalarProperty> _scalarProperties;
        private Hashtable _propertyNameValue;
        private TableItemStyle _labelStyle;
        private TableItemStyle _controlStyle;
        private byte[] _fullLectureImage;
        private FileUpload fileUploadControl;
        private CheckBox checkboxDeleteFile;

        #endregion

        #endregion

        #region Enums

        #region Public

        
        #endregion

        #endregion

        #region Properties

        #region Public


        /// <summary>
        /// Gets or sets resource type in Add mode.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceResourceType")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
                _resourceType = value;

                if (string.IsNullOrEmpty(_resourceType) && ControlMode == ResourcePropertiesOperationMode.Add)
                {
                    _resourceType = ResourcePropertyConstants.ResourceName;
                }

                if (!IsValidResourceType() && ControlMode == ResourcePropertiesOperationMode.Add)
                {
                    throw new ArgumentException(GlobalResource.InvalidResourceType);
                }

            }
        }




        /// <summary>
        /// Gets or sets Core.Resource id property.
        /// </summary> 
        [Browsable(false)]
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

        /// <summary>
        /// Gets or sets whether validation is required for child controls.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceValidationRequired")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool ValidationRequired
        {
            get
            {
                return _validationRequired;
            }
            set
            {
                _validationRequired = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the validation group assigned to the child validation controls.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceValidationGroup")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ValidationGroup
        {
            get
            {
                return _validationGroup;
            }
            set
            {
                _validationGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets the regular expression that determines the pattern used to validate a field.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceValidCharsRegex")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ValidCharactersRegularExpression
        {
            get
            {
                return _validCharactersRegularExpression;
            }
            set
            {
                _validCharactersRegularExpression = value;
            }
        }

        /// <summary>
        /// Gets or sets style class for control.
        /// </summary>
        [Bindable(true)]
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionResourceCss")]
        [DefaultValue("")]
        [Localizable(true)]
        public override string CssClass
        {
            get
            {
                return _styleClass;
            }
            set
            {
                _styleClass = value;
            }
        }

        /// <summary>
        /// Gets or sets the control’s operating mode.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionResourceControlMode")]
        [DefaultValue("Add")]
        [Localizable(true)]
        public ResourcePropertiesOperationMode ControlMode
        {
            get
            {
                return _controlMode;
            }
            set
            {
                _controlMode = value;
            }

        }


        /// <summary>
        /// Gets Style defined for Labels.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle LabelStyle
        {
            get
            {
                if (this._labelStyle == null)
                {
                    this._labelStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._labelStyle).TrackViewState();
                    }
                }
                return this._labelStyle;
            }
        }

        /// <summary>
        ///     Gets Style defined for Labels.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ZentityCategory("CategoryApperance"),
        ZentityDescription("DescriptionZentityTableRowStyle"),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ControlsStyle
        {
            get
            {
                if (this._controlStyle == null)
                {
                    this._controlStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._controlStyle).TrackViewState();
                    }
                }
                return this._controlStyle;
            }
        }

        /// <summary>
        /// Gets or sets the maximum property size for which a textbox is rendered. 
        /// If the property size is greater than this value, a text area will be rendered.
        /// </summary>
        public int? MaxCharsForTextBox
        {
            get
            {
                if (_maxCharsForTextBox == null)
                {
                    return ResourcePropertyConstants.TextMultiLineMin;
                }
                return _maxCharsForTextBox;
            }
            set
            {
                _maxCharsForTextBox = value;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Creates/saves resource with values from the input controls.
        /// </summary>
        /// <returns>true, if successful; else false.</returns>
        public bool SaveResourceProperties()
        {
            if (!this.Page.IsValid)
                return false;
            return SaveResourceProperties(GetResourceDetails(), base.CreateContext());
        }

        /// <summary>
        /// Creates/saves resource with values from the input controls.
        /// </summary>
        /// <param name="resource">Resource object to be saved.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance.</param>
        /// <returns>true, if successful; else false.</returns>
        public bool SaveResourceProperties(Resource resource, ZentityContext context)
        {
            bool result = false;

            if (resource != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(context))
                {

                    result = dataAccess.SaveResource(ControlMode != ResourcePropertiesOperationMode.Add ? true : false,
                               resource);
                    if (result && IsSecurityAwareControl && ControlMode == ResourcePropertiesOperationMode.Add)
                    {
                        GrantOwnership(resource.Id);
                    }
                }

                if (result)
                {
                    ResourceId = resource.Id;
                    result = UploadFileContent(resource);
                }
            }
            return result;
        }

        /// <summary>
        /// Clears all the child property controls.
        /// </summary>
        public void ClearControls()
        {
            if (this != null)
            {
                _scalarProperties = new Collection<ScalarProperty>();
                GetScalarProperties(null);

                foreach (ScalarProperty property in _scalarProperties)
                {
                    ResetControl(this.FindControl(GetControlId(property.Name)), property);
                }
            }
        }

        /// <summary>
        /// Clears the specified child control.
        /// </summary>
        /// <param name="ctrl">A control to be cleared.</param>
        /// <param name="property">Property, which the specified control belongs to.</param>
        private static void ResetControl(Control ctrl, ScalarProperty property)
        {
            if (ctrl != null)
            {
                switch (property.DataType)
                {
                    case DataTypes.Binary:
                        break;
                    case DataTypes.Boolean:
                        CheckBox chkBox = ctrl as CheckBox;
                        if (chkBox != null)
                        {
                            chkBox.Checked = false;
                        }
                        break;
                    default:
                        TextBox txt = ctrl as TextBox;
                        if (txt != null)
                        {
                            txt.Text = string.Empty;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Updates resource object properties from respective property control data.
        /// </summary>        
        /// <returns>A resource with values from the input controls.</returns>
        public Resource GetResourceDetails()
        {
            Resource resourceToReturn = null;

            if (this._scalarProperties.Count == 0)
            {
                this.GetScalarProperties(null);

            }
            if (this._scalarProperties.Count > 0)
            {
                Type resourceSystemType = GetTypeOfResource();

                if (resourceSystemType != null)
                {
                    if (_resource == null)
                    {
                        Object resourceObj = Activator.CreateInstance(resourceSystemType, null);

                        _resource = resourceObj as Resource;
                        if (ResourceId != null && ResourceId != Guid.Empty)
                        {
                            _resource = GetResourceData(ResourceId);
                        }
                    }

                    PropertyInfo[] propertiesCollection = resourceSystemType.GetProperties();
                    Control ctrlProperty = null;
                    foreach (PropertyInfo prop in propertiesCollection)
                    {
                        ctrlProperty = this.FindControl(GetControlId(prop.Name));
                        ScalarProperty property = GetScalarPropertyByName(prop.Name);

                        if (ctrlProperty != null && property != null)
                        {
                            object propertyValue = GetValue(ctrlProperty, prop);
                            prop.SetValue(_resource, propertyValue, null);
                        }
                    }

                    if (IsInHierarchy(ResourcePropertyConstants.ResourceFileTypeFullName, false))
                    {
                        Zentity.Core.File fileResource = (Zentity.Core.File)_resource;
                        string fileExtension = GetFileExtension();
                        if (!string.IsNullOrEmpty(fileExtension))
                        {
                            fileResource.FileExtension = fileExtension;
                        }
                        else if (checkboxDeleteFile != null)
                        {
                            if (checkboxDeleteFile.Checked)
                            {
                                fileResource.FileExtension = string.Empty;
                            }
                        }
                    }

                    resourceToReturn = _resource;
                }
            }
            return resourceToReturn;
        }

        /// <summary>
        /// This method is used by RenderImage() for create Thumbnail of the Image.
        /// </summary>
        /// <returns>true for Thumbnail.</returns>
        public bool ThumbnailCallback()
        {
            return true;
        }

        /// <summary>
        /// Populates control properties with value.
        /// </summary>
        public void PopulateControls()
        {
            foreach (ScalarProperty property in _scalarProperties)
            {
                if (property.DataType == DataTypes.String)
                {
                    TextBox txt = this.FindControl(GetControlId(property.Name)) as TextBox;
                    if (_propertyNameValue.ContainsKey(property.Name))
                    {
                        txt.Text = Convert.ToString(_propertyNameValue[property.Name], CultureInfo.CurrentCulture);
                    }
                }
            }
        }

        /// <summary>
        /// Populate HashTable with scalar properties and value.
        /// </summary>
        /// <param name="resource">Resource object.</param>
        public void PopulateHashtable(Resource resource)
        {
            _propertyNameValue = new Hashtable();
            _propertyNameValue.Clear();

            foreach (ScalarProperty property in _scalarProperties)
            {
                Type resourceSystemType = GetTypeOfResource();

                if (resourceSystemType != null)
                {
                    PropertyInfo[] propertiesCollection = resourceSystemType.GetProperties();

                    foreach (PropertyInfo prop in propertiesCollection)
                    {
                        if (prop.Name == property.Name)
                        {
                            _propertyNameValue.Add(property.Name, prop.GetValue(resource, null));
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a resource with specified id.
        /// </summary>
        /// <param name="resourceId">Id of the resource to be deleted.</param>
        public void DeleteResource(Collection<Guid> resourceId)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                dataAccess.DeleteResourcesCategory(resourceId);
            }
        }

        /// <summary>
        /// Gets the category node with specified id.
        /// </summary>
        /// <param name="categoryNodeId">Id of the category node to be fetched.</param>
        /// <returns>Category node with specified id.</returns>
        public CategoryNode GetCategoryNode(Guid categoryNodeId)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                return dataAccess.GetCategoryNode(categoryNodeId);
            }
        }

        /// <summary>
        /// Grants ownership to specified resource.
        /// </summary>
        /// <param name="resourceId">Id of the resource.</param>
        /// <returns>true, if successful; else false.</returns>
        public bool GrantOwnership(Guid resourceId)
        {
            if (resourceId != Guid.Empty)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    return dataAccess.GrantDefaultOwnership(this.AuthenticatedToken, resourceId);
                }
            }

            return false;
        }

        #endregion

        #region protected

        /// <summary>
        /// Registers client script with the parent page and loads the control.
        /// </summary>
        /// <param name="e">Event argument.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Register javascriptScript.js file
            this.Page.ClientScript.RegisterClientScriptInclude(
                this.GetType(),
                _commonJavascriptFile,
                Page.ClientScript.GetWebResourceUrl(this.GetType(), _commonScriptPath));

            base.OnLoad(e);
        }

        /// <summary>
        /// Creates child control of resource property control.
        /// </summary>
        protected override void CreateRows()
        {
            if (IsValidResourceType())
            {
                RenderControls();

                if (ControlMode != ResourcePropertiesOperationMode.Read)
                {
                    InitializeControlInEditMode();
                }
            }
        }

        /// <summary>
        ///  Creates header for the control.
        /// </summary>
        protected override void CreateHeader()
        {
            TableHeaderRow row = new TableHeaderRow();
            row.Controls.Add(CreateHeaderCell(string.Empty));
            row.Controls.Add(CreateHeaderCell(string.Empty));
            this._resourceTable.Controls.Add(row);
        }

        /// <summary>
        ///  Initialize the scalar property.
        /// </summary>
        protected override void GetDataSource()
        {
            if (string.IsNullOrEmpty(Title))
            {
                Title = GlobalResource.ResourcePropertiesTitle;
            }
            this.Page.ClientScript.RegisterClientScriptInclude(this.GetType(), _javascriptFile, Page.ClientScript.GetWebResourceUrl(this.GetType(), _resourcePropertyScriptPath));
            _scalarProperties = new Collection<ScalarProperty>();

            if (ControlMode != ResourcePropertiesOperationMode.Add)
            {
                if (IsSecurityAwareControl && this.ResourceId != Guid.Empty)
                {
                    //Check whether user is having a update permission on a given resource.
                    using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                    {
                        if (!dataAccess.AuthorizeUser(this.AuthenticatedToken, UserResourcePermissions.Update, this.ResourceId))
                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                                GlobalResource.UnauthorizedAccessException, UserResourcePermissions.Update));
                    }
                }

                _resource = this.GetResourceData(this.ResourceId);
                if (_resource != null)
                {
                    ResourceType = _resource.GetType().Name;
                    GetScalarProperties(null);
                }
            }
            else
            {
                if (IsSecurityAwareControl)
                {
                    //Check whether user is having a Create permission.
                    using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                    {
                        if (!dataAccess.HasCreatePermission(this.AuthenticatedToken))
                            throw new UnauthorizedAccessException(string.Format(CultureInfo.InvariantCulture,
                                GlobalResource.UnauthorizedAccessExceptionCreate, UserResourcePermissions.Create));
                    }
                }

                GetScalarProperties(null);
            }
        }

        /// <summary>
        /// Creates the data to be displayed in the control at design time.
        /// </summary>
        protected override void CreateDesignTimeDataSource()
        {
            _scalarProperties = new Collection<ScalarProperty>();
            GetScalarProperties(null);
        }

        /// <summary>
        /// Registers javascript Validation Method to Textbox objects in the resource table.
        /// </summary>
        /// <param name="e">Event argument.</param>
        protected override void OnPreRender(EventArgs e)
        {
            RegisterValidationMethod();
            SetCursor();
            base.OnPreRender(e);
        }

        #endregion

        #region Private

        private void SetCursor()
        {
            if (ControlMode != ResourcePropertiesOperationMode.Read && _resourceTable.Rows.Count > 1
                && _resourceTable.Rows[1].Cells.Count > 1 && _resourceTable.Rows[1].Cells[1].Controls.Count > 0)
            {
                _resourceTable.Rows[1].Cells[1].Controls[0].Focus();
            }
        }

        /// <summary>
        /// Register CauseValidations() JS method for 
        /// onblur event on TextBox objects in the Resource Table.
        /// </summary>
        private void RegisterValidationMethod()
        {
            if (_resourceTable != null && _resourceTable.Rows.Count > 0)
            {
                foreach (TableRow tr in _resourceTable.Rows)
                {
                    TextBox txtControl = FindTextBoxControl(tr);
                    if (txtControl != null)
                    {
                        Collection<BaseValidator> validators = FindValidatorControls(tr);
                        if (validators.Count > 0)
                        {
                            string validatorIds = validators[0].ClientID;
                            for (int i = 1; i < validators.Count; i++)
                            {
                                validatorIds += "," + validators[i].ClientID;
                            }

                            txtControl.Attributes.Add(_onBlurEvent,
                              string.Format(CultureInfo.InvariantCulture, _CauseValidationFunction, validatorIds));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns TaxtBox object in the row.
        /// </summary>
        /// <param name="tr">TableRow object.</param>
        /// <returns>TaxtBox object if fount else null.</returns>
        private static TextBox FindTextBoxControl(TableRow tr)
        {
            TextBox txtControl = null;
            if (tr != null)
            {
                foreach (TableCell cell in tr.Cells)
                {
                    foreach (Control ctr in cell.Controls)
                    {
                        txtControl = ctr as TextBox;
                        if (txtControl != null)
                        {
                            break;
                        }
                    }
                }
            }

            return txtControl;
        }

        /// <summary>
        /// Finds Validators in a rows.
        /// </summary>
        /// <param name="tr">TableRow object.</param>
        /// <returns>Collection of validator objects.</returns>
        private static Collection<BaseValidator> FindValidatorControls(TableRow tr)
        {
            Collection<BaseValidator> validatorCollection = new Collection<BaseValidator>();
            BaseValidator validator = null;
            if (tr != null)
            {
                foreach (TableCell cell in tr.Cells)
                {
                    foreach (Control ctr in cell.Controls)
                    {
                        validator = ctr as BaseValidator;
                        if (validator != null)
                        {
                            validatorCollection.Add(validator);
                        }
                    }
                }
            }

            return validatorCollection;
        }


        /// <summary>
        /// Returns file extension for File object.
        /// </summary>
        /// <returns>File Extension</returns>
        private string GetFileExtension()
        {
            ScalarProperty uploadFileProperty = GetScalarPropertyByName(ResourcePropertyConstants.ResourceFileUploadName);
            if (uploadFileProperty == null)
                return string.Empty;

            FileUpload fileUpldCtrl = this.FindControl(GetControlId(ResourcePropertyConstants.ResourceFileUploadName))
                as FileUpload;
            if (fileUpldCtrl == null)
                return string.Empty;

            return Path.GetExtension(fileUpldCtrl.FileName);
        }

        /// <summary>
        /// Initialize control in edit and UploadFileControl property with file upload control.
        /// </summary>
        private void InitializeControlInEditMode()
        {
            if (this.ControlMode != ResourcePropertiesOperationMode.Read && this.ValidationRequired)
            {
                BindPropertyKeyPressEvent();
            }

        }
        /// <summary>
        /// Checks if specified resource type is valid
        /// </summary>
        /// <returns> Boolean value indicating child controls to be rendered or not </returns>
        private bool IsValidResourceType()
        {
            return (this.IsResourceTypeSpecified() && this.IsSpecifiedResourceTypeExist());
        }

        /// <summary>
        /// Checks if resource type is provided
        /// </summary>
        /// <returns> Boolean value indicating whether resource type is specified or not </returns>
        private bool IsResourceTypeSpecified()
        {
            return (!string.IsNullOrEmpty(this.ResourceType));
        }

        /// <summary>
        /// Checks if resource type specified is valid or not
        /// </summary>
        /// <returns> Boolean value indicating whether resource type is valid resource type or not </returns>
        private bool IsSpecifiedResourceTypeExist()
        {
            return ((GetResourceTypeInfo(ResourceType) != null));
        }

        /// <summary>
        /// Renders the child property control in html table
        /// </summary>
        /// <returns> returns html table as control object  </returns>
        private void RenderControls()
        {
            ReorderResourceFields();

            if (_scalarProperties.Count > 0)
            {
                PopulatePropertyNameValue();

                foreach (ScalarProperty property in _scalarProperties)
                {
                    this._resourceTable.Controls.Add(GetRow(property));
                }

                if (this.ControlMode == ResourcePropertiesOperationMode.Edit)
                {
                    if (IsInHierarchy(ResourcePropertyConstants.ResourceFileTypeFullName, false))
                    {
                        System.IO.Stream stream = null;
                        using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
                        {
                            stream = dataAccess.GetContentStream(this.ResourceId);
                        }
                        if (!(stream != null && stream.Length > 0))
                        {
                            checkboxDeleteFile.Visible = false;
                        }
                    }
                }

                this.Style.Add("width", GlobalResource.CssControlWidth100Percent);
            }
        }

        /// <summary>
        ///  Populates propertyNameValue hash table with property name and value.
        /// </summary>
        private void PopulatePropertyNameValue()
        {
            if (this.ControlMode != ResourcePropertiesOperationMode.Add)
            {
                _propertyNameValue = new Hashtable();
                _propertyNameValue.Clear();

                foreach (ScalarProperty property in _scalarProperties)
                {
                    Type resourceSystemType = GetTypeOfResource();

                    if (resourceSystemType != null)
                    {
                        PropertyInfo[] propertiesCollection = resourceSystemType.GetProperties();

                        foreach (PropertyInfo prop in propertiesCollection)
                        {
                            if (prop.Name == property.Name)
                            {
                                _propertyNameValue.Add(property.Name, prop.GetValue(_resource, null));
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uploads physical file associated with File  type object.
        /// </summary>        
        /// <param name="resourceToUpload">File object that is associated with physical file.</param>
        /// <returns>true if successfully uploaded.</returns>
        private bool UploadFileContent(Resource resourceToUpload)
        {
            if (_resourceType != "File")
                return true;

            ScalarProperty uploadFileProperty = GetScalarPropertyByName(ResourcePropertyConstants.ResourceFileUploadName);
            if (uploadFileProperty == null)
                return false;

            FileUpload fileUpldCtrl = this.FindControl(GetControlId(ResourcePropertyConstants.ResourceFileUploadName))
                as FileUpload;

            if (fileUpldCtrl == null)
                return false;

            Zentity.Core.File fileObj = resourceToUpload as Zentity.Core.File;

            if (fileUpldCtrl.HasFile)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
                {
                    dataAccess.UploadFileContent(fileObj, fileUpldCtrl.FileContent);
                    TextBox txtExt = (TextBox)this.FindControl(GetControlId(ResourcePropertyConstants.ResourceFileTypeExtension));
                    txtExt.Text = Path.GetExtension(fileUpldCtrl.FileName);
                    if (checkboxDeleteFile != null)
                        checkboxDeleteFile.Visible = true;
                }
            }
            else if (checkboxDeleteFile != null
                && checkboxDeleteFile.Checked)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
                {
                    dataAccess.UploadFileContent(fileObj, Stream.Null);
                }
                TextBox txtExt = (TextBox)this.FindControl(GetControlId(ResourcePropertyConstants.ResourceFileTypeExtension));
                txtExt.Text = string.Empty;
                if (checkboxDeleteFile != null)
                {
                    checkboxDeleteFile.Checked = false;
                    checkboxDeleteFile.Visible = false;
                }
            }
            //else??

            return true;
        }

        /// <summary>
        /// Returns type of Resource with full name.
        /// </summary>
        /// <returns></returns>
        private Type GetTypeOfResource()
        {
            Type resourceSystemType = null;

            if (_resource == null)
            {
                Core.ResourceType type = GetResourceTypeInfo(ResourceType);
                Assembly zentityAssembly = Assembly.Load(CoreHelper.ExtractNamespace(type.FullName));
                resourceSystemType = zentityAssembly.GetType(type.FullName);
            }
            else
            {
                resourceSystemType = _resource.GetType();
            }
            return resourceSystemType;
        }

        /// <summary>
        /// Gets specific scalar property from collection.
        /// </summary>
        /// <param name="name"> Name of scalar property to be retrieved</param>
        /// <returns> Instance of scalar property</returns>
        private ScalarProperty GetScalarPropertyByName(string name)
        {
            ScalarProperty typeProperty = null;
            try
            {
                typeProperty = _scalarProperties.Where(property => property.Name == name).First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            return typeProperty;
        }

        /// <summary>
        /// Retrieves ResourceTypeInfo object.
        /// </summary>
        /// <param name="typeName"> Name by which ResourceTypeInfo object to be retrieved </param>
        /// <returns> Instance of expected ResourceTypeInfo object </returns>
        private ResourceType GetResourceTypeInfo(string typeName)
        {
            if (this.DesignMode)
            {
                return new ResourceType();
            }

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                return dataAccess.GetResourceType(typeName);
            }
        }

        /// <summary>
        /// Binds text boxes with KeyPress event to restrict no of input characters.
        /// </summary>        
        private void BindPropertyKeyPressEvent()
        {
            foreach (ScalarProperty property in _scalarProperties)
            {
                if (property.DataType == DataTypes.String && property.MaxLength > 0)
                {
                    TextBox txt = this.FindControl(GetControlId(property.Name)) as TextBox;
                    if (txt != null && txt.TextMode == TextBoxMode.MultiLine && txt.MaxLength > 0)
                    {
                        txt.Attributes.Add("onkeypress",
                            "return MultilineTextValidate(" + txt.ClientID + "," + txt.MaxLength + ")");
                        txt.Attributes.Add("onkeyup",
                            "return MultilineTextValidate(" + txt.ClientID + "," + txt.MaxLength + ")");
                    }
                }
            }
        }


        /// <summary>
        /// Retrieves value from property control.
        /// </summary>
        /// <param name="valueRetrievalControl"> controls from which value to be retrieved </param>
        ///<param name="propertyInfo"> type property info object</param>
        /// <returns> property value </returns>
        private object GetValue(Control valueRetrievalControl, PropertyInfo propertyInfo)
        {
            object value = null;

            ScalarProperty property = GetScalarPropertyByName(propertyInfo.Name);

            if (property == null) return value;

            if (property.DataType == DataTypes.Binary)
            {
                FileUpload fileUploadCtrl = valueRetrievalControl as FileUpload;
                if (fileUploadCtrl.HasFile)
                {
                    value = fileUploadCtrl.FileBytes;
                }
                else
                {
                    if (_resource != null && propertyInfo != null)
                    {
                        value = propertyInfo.GetValue(_resource, null);
                        if (checkboxDeleteFile != null)
                        {
                            if (checkboxDeleteFile.Checked)
                            {
                                value = null;
                            }
                        }
                    }
                }
            }
            else if (property.DataType == DataTypes.Boolean)
            {
                CheckBox chkCtrl = valueRetrievalControl as CheckBox;
                value = chkCtrl.Checked;
            }
            else
            {
                value = GetValue((TextBox)valueRetrievalControl, property.DataType);
            }
            return value;
        }

        /// <summary>
        ///  Returns max string length for string other data type Validate data.
        /// </summary>
        /// <param name="valueRetrievalTextBox">textbox for get text</param>
        /// <param name="dataType">Data type</param>
        /// <returns>Object</returns>
        private static object GetValue(TextBox valueRetrievalTextBox, DataTypes dataType)
        {
            object value = null;

            valueRetrievalTextBox.Text = valueRetrievalTextBox.Text.Trim();

            if (dataType == DataTypes.String)
            {
                value = ReturnMaxLengthString(valueRetrievalTextBox);
            }
            else if (dataType == DataTypes.DateTime)
            {
                DateTime date = DateTime.MinValue;
                date = ValidateDate(valueRetrievalTextBox.Text);
                if (date != DateTime.MinValue)
                {
                    value = date;
                }
            }
            else
            {
                value = ValidateData(dataType, valueRetrievalTextBox.Text);
            }

            return value;
        }

        /// <summary>
        /// Adds range validators for property control as per data type to table cell.
        /// </summary>
        ///<param name="controlToValidate"> Id of control to be validated </param>
        ///<param name="dataType"> data type of control to be validated </param>
        ///<param name="cell"> Table cell to which validators to be added</param>
        /// <returns> Instance of base validator type </returns>
        private void AddRangeValidator(string controlToValidate, DataTypes dataType, TableCell cell)
        {
            BaseValidator validator = null;
            RangeValidator rngValidator = null;
            CustomValidator customValidator = null;

            switch (dataType)
            {
                case DataTypes.DateTime:
                    rngValidator = new RangeValidator();
                    rngValidator.Type = ValidationDataType.Date;
                    rngValidator.MinimumValue = DateTime.MinValue.Date.AddYears(_minimumDate).ToString(CoreHelper.GetDateFormat(), CultureInfo.InvariantCulture);
                    rngValidator.MaximumValue = DateTime.MaxValue.Date.AddYears(_maximumYearDate).AddMonths(_maximumMonthDate).AddDays(_maximumDayDate).ToString(CoreHelper.GetDateFormat(), CultureInfo.InvariantCulture);
                    rngValidator.ErrorMessage = _newLine + string.Format(CultureInfo.CurrentCulture, GlobalResource.InvalidDateErrorMessage, CoreHelper.GetDateFormat());
                    rngValidator.Display = ValidatorDisplay.Dynamic;
                    validator = rngValidator;
                    break;
                case DataTypes.Int64:
                    customValidator = new CustomValidator();
                    customValidator.ClientValidationFunction = ResourcePropertyConstants.LongTypeRangeValidatorScript;
                    customValidator.ErrorMessage = _newLine + GlobalResource.InvalidNumberErrorMessage;
                    validator = customValidator;
                    break;
                case DataTypes.Int32:
                    rngValidator = new RangeValidator();
                    rngValidator.Type = ValidationDataType.Integer;
                    rngValidator.MinimumValue = int.MinValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.MaximumValue = int.MaxValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.ErrorMessage = _newLine + GlobalResource.InvalidNumberErrorMessage;
                    validator = rngValidator;
                    break;
                case DataTypes.Int16:
                    rngValidator = new RangeValidator();
                    rngValidator.Type = ValidationDataType.Integer;
                    rngValidator.MinimumValue = short.MinValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.MaximumValue = short.MaxValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.ErrorMessage = _newLine + GlobalResource.InvalidNumberErrorMessage;
                    validator = rngValidator;
                    break;
                case DataTypes.Decimal:
                    customValidator = new CustomValidator();
                    customValidator.ClientValidationFunction = ResourcePropertyConstants.DecimalTypeRangeValidatorScript;
                    customValidator.ErrorMessage = _newLine + GlobalResource.InvalidNumberErrorMessage;
                    validator = customValidator;
                    break;
                case DataTypes.Double:
                    rngValidator = new RangeValidator();
                    rngValidator.Type = ValidationDataType.Double;
                    rngValidator.MinimumValue = double.MinValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.MaximumValue = double.MaxValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.ErrorMessage = _newLine + GlobalResource.InvalidNumberErrorMessage;
                    validator = rngValidator;
                    break;
                case DataTypes.Single:
                    rngValidator = new RangeValidator();
                    rngValidator.Type = ValidationDataType.Double;
                    rngValidator.MinimumValue = Single.MinValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.MaximumValue = Single.MaxValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.ErrorMessage = _newLine + GlobalResource.InvalidNumberErrorMessage;
                    validator = rngValidator;
                    break;
                case DataTypes.Byte:
                    rngValidator = new RangeValidator();
                    rngValidator.Type = ValidationDataType.Integer;
                    rngValidator.MinimumValue = Byte.MinValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.MaximumValue = Byte.MaxValue.ToString(CultureInfo.CurrentCulture);
                    rngValidator.ErrorMessage = _newLine + GlobalResource.InvalidNumberErrorMessage;
                    validator = rngValidator;
                    break;
                default:
                    break;
            }

            if (validator != null)
            {
                validator.ID = GetControlId(controlToValidate) +
                    ResourcePropertyConstants.RangeValidator;
                validator.ValidationGroup = ValidationGroup;
                validator.ControlToValidate = GetControlId(controlToValidate);
                validator.Display = ValidatorDisplay.Dynamic;
                cell.Controls.Add(validator);
            }
        }

        /// <summary>
        /// Adds Compare validators for property control to table cell.
        /// </summary>
        /// <param name="controlToValidate">Id of control to be validated </param>
        /// <param name="controlToComapre">Id of control to be compare </param>
        /// <param name="cell">Table cell to which validators to be added</param>
        private void AddDateCompareValidator(string controlToValidate, string controlToComapre, TableCell cell)
        {
            CompareValidator validator = new CompareValidator();
            validator.ID = GetControlId(controlToValidate) + ResourcePropertyConstants.CompareValidator;
            validator.ValidationGroup = ValidationGroup;
            validator.ControlToValidate = GetControlId(controlToComapre);
            validator.Type = ValidationDataType.Date;
            validator.Display = ValidatorDisplay.Dynamic;
            validator.ControlToCompare = GetControlId(controlToValidate);
            validator.ErrorMessage = _newLine + controlToValidate + _space + GlobalResource.InvalidDateRangeErrorMessage + _space + controlToComapre;
            validator.Operator = ValidationCompareOperator.GreaterThan;
            cell.Controls.Add(validator);

        }

        /// <summary>
        /// Gets required field validator for property control.
        /// </summary>
        ///<param name="controlToValidate"> Id of control to be validated </param>
        ///<param name="isNullable"> determines whether property requires required field validator </param>
        ///<param name="cell"> Table cell to which validators to be added</param>
        /// <returns> Instance of base validator </returns>
        private void AddRequiredFieldValidator(string controlToValidate, bool isNullable, TableCell cell)
        {
            if (!isNullable)
            {
                RequiredFieldValidator requiredField = new RequiredFieldValidator();
                requiredField.ID = this.GetControlId(controlToValidate + ResourcePropertyConstants.RequiredFieldValidator);
                requiredField.ControlToValidate = GetControlId(controlToValidate);
                requiredField.ValidationGroup = this.ValidationGroup;
                requiredField.Display = ValidatorDisplay.Dynamic;
                requiredField.ErrorMessage = _newLine + string.Format(CultureInfo.CurrentCulture, GlobalResource.Requiredproperty, controlToValidate);
                cell.Controls.Add(requiredField);
            }
        }

        /// <summary>
        /// Adds regular expression validator for property control as per data type to table cell.
        /// </summary>        
        ///<param name="controlToValidate"> Id of control to be validated </param>
        ///<param name="dataType"> data type of control to be validated </param> 
        ///<param name="cell"> Table cell to which validators to be added</param>
        /// <returns> Instance of base validator type </returns>
        private void AddRegExValidator(string controlToValidate, DataTypes dataType, TableCell cell)
        {
            RegularExpressionValidator regExValidator = null;
            if (dataType == DataTypes.String &&
                !string.IsNullOrEmpty(this.ValidCharactersRegularExpression))
            {
                regExValidator = new RegularExpressionValidator();
                regExValidator.Display = ValidatorDisplay.Dynamic;
                regExValidator.ErrorMessage = _newLine + GlobalResource.RegularExpressionError;
                regExValidator.ValidationExpression = ValidCharactersRegularExpression;
                regExValidator.ID = GetControlId(controlToValidate) + ResourcePropertyConstants.RegularExpressionValidator;
                regExValidator.ValidationGroup = ValidationGroup;
                regExValidator.ControlToValidate = GetControlId(controlToValidate);

                cell.Controls.Add(regExValidator);
            }
        }

        /// <summary>
        ///  Adds regular field validator for property control.
        /// </summary>
        /// <param name="controlToValidate">Id of control to be validated</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <param name="validationExpression">validation expression</param>
        /// <param name="cell">Table cell to which validators to be added</param>
        private void AddRegularExpressionValidator(string controlToValidate, string errorMessage, string validationExpression, TableCell cell)
        {
            RegularExpressionValidator regExValidator = new RegularExpressionValidator();
            regExValidator.Display = ValidatorDisplay.Dynamic;
            regExValidator.ErrorMessage = _newLine + errorMessage;
            regExValidator.ValidationExpression = validationExpression;
            regExValidator.ID = GetControlId(controlToValidate) + ResourcePropertyConstants.RegularExpressionValidator;
            regExValidator.ValidationGroup = ValidationGroup;
            regExValidator.ControlToValidate = GetControlId(controlToValidate);

            cell.Controls.Add(regExValidator);
        }

        /// <summary>
        /// Adds Custom validator for File type property control.
        /// </summary>
        /// <param name="controlToValidate">Id of control to be validated</param>
        /// <param name="cell">Table cell to which validators to be added</param>
        private void AddValidFileCustomvalidator(string controlToValidate, TableCell cell)
        {
            CustomValidator customValidator = new CustomValidator();
            customValidator.ServerValidate += new ServerValidateEventHandler(CheckUploadedFile);
            customValidator.ID = GetControlId(controlToValidate) + ResourcePropertyConstants.CustomValidator;
            customValidator.ControlToValidate = GetControlId(controlToValidate);
            customValidator.ErrorMessage = GlobalResource.InvalidFileUploadPath;
            customValidator.ValidationGroup = this.ValidationGroup;
            customValidator.Display = ValidatorDisplay.Dynamic;
            cell.Controls.Add(customValidator);
        }


        /// <summary>
        /// Ordering scalar properties.
        /// </summary>
        private void ReorderResourceFields()
        {
            string[] renderingOrder = null;
            if (ControlMode == ResourcePropertiesOperationMode.Read)
            {
                renderingOrder = GlobalResource.RenderingOrder1.Split(',');
            }
            else
            {
                renderingOrder = GlobalResource.RenderingOrder2.Split(',');
            }

            ScalarProperty property = null;
            for (int i = 0; i < renderingOrder.Length; i++)
            {
                property = GetScalarPropertyByName(Convert.ToString(renderingOrder.GetValue(i),
                    CultureInfo.InvariantCulture));
                ReorderResourceField(i, property);
            }
        }

        /// <summary>
        /// Shuffles the rendering order of property controls.
        /// </summary>
        /// <param name="indexToInsert"> New rendering position </param>
        /// <param name="property"> Instance of scalar property to be shuffled </param>
        private void ReorderResourceField(int indexToInsert, ScalarProperty property)
        {
            if (property != null)
            {
                _scalarProperties.Remove(property);
                _scalarProperties.Insert(indexToInsert, property);
            }
        }

        /// <summary>
        /// Populates generic list of scalar property with scalar property collections 
        /// from each resource type in the hierarchy .
        /// </summary>
        private void GetScalarProperties(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                if (string.IsNullOrEmpty(this.ResourceType))
                {
                    this.ResourceType = ResourcePropertyConstants.ResourceName;
                }
                type = this.ResourceType;
            }
            else
            {
                type = CoreHelper.ExtractTypeName(type);
            }

            Core.ResourceType typeInfo = null;
            if (DesignMode)
            {
                typeInfo = GetResourceTypeForDesignMode(type);
                foreach (ScalarProperty property in typeInfo.ScalarProperties)
                    _scalarProperties.Add(property);
            }
            else
            {
                ResourceType typeObj = GetResourceTypeInfo(type);

                Collection<ScalarProperty> scalarPropertiesCache = new Collection<ScalarProperty>();
                scalarPropertiesCache = CoreHelper.GetScalarProperties(this.Page.Cache, typeObj);

                foreach (ScalarProperty property in scalarPropertiesCache)
                    _scalarProperties.Add(property);

                if (IsInHierarchy(ResourcePropertyConstants.ResourceFileTypeFullName, false))
                {
                    ScalarProperty uploadFileProperty = GetUploadFileScalarProperty();
                    if (uploadFileProperty != null)
                    {
                        _scalarProperties.Add(uploadFileProperty);
                    }
                }

                ArrangePropertiesByOrder();
                FilterPropertiesByType();
            }
        }

        /// <summary>
        /// Arranges order of scalar properties.
        /// </summary>
        private void ArrangePropertiesByOrder()
        {
            ResourcePropertiesConfigSection resourceConfigSection =
                              (ResourcePropertiesConfigSection)ConfigurationManager.GetSection(ResourcePropertyConstants.ResourcePropertySetting);

            if (resourceConfigSection != null)
            {
                if (resourceConfigSection.OrderedProperties != null)
                {
                    for (int Index = 0; Index < resourceConfigSection.OrderedProperties.Count; Index++)
                    {
                        if (IsInHierarchy(resourceConfigSection.OrderedProperties[Index].Class, false))
                        {
                            string[] property = (resourceConfigSection.OrderedProperties[Index].Order).Split(',');
                            for (int Count = 0; Count < property.Length; Count++)
                            {
                                if (GetScalarPropertyByName(property[Count]) == null)
                                {
                                    return;
                                }
                            }

                            for (int Count = 1; Count < property.Length; Count++)
                            {
                                ScalarProperty orderProperty = GetScalarPropertyByName(property[Count]);
                                int index = GetIndexByPropertyName(property[Count]);
                                _scalarProperties.RemoveAt(index);
                                _scalarProperties.Insert(GetIndexByPropertyName(property[0]) + Count, orderProperty);

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns Index of scalar property from scalar property list.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Index</returns>
        private int GetIndexByPropertyName(string propertyName)
        {
            int Index = 0;
            for (Index = 0; Index < _scalarProperties.Count; Index++)
            {
                if (propertyName == _scalarProperties[Index].Name)
                {
                    break;
                }
            }
            return Index;
        }

        /// <summary>
        /// Returns resource typeInfo with all property collection.
        /// </summary>
        /// <param name="type">resource type name</param>
        /// <returns>type of resource info</returns>
        private static ResourceType GetResourceTypeForDesignMode(string type)
        {
            ResourceType typeObj = null;

            Type objectType = GetObjectTypForDesignMode(type);

            if (objectType == null)
            {
                objectType = typeof(Resource);
            }
            PropertyInfo[] properties = objectType.GetProperties();

            typeObj = new ResourceType();
            typeObj.BaseType = null;
            properties = objectType.GetProperties().
                Where(property => property.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).Length == 1).ToArray();
            for (int index = 0; index < properties.Length; index++)
            {
                typeObj.ScalarProperties.Add(new ScalarProperty(properties[index].Name,
                    CheckDataType(properties[index].PropertyType)));
            }
            
            typeObj.Name = type;
           
            return typeObj;
        }

        private static Type GetObjectTypForDesignMode(string typeName)
        {
            Type objectType = null;

            if (typeName.Contains("."))
            {
                objectType = Type.GetType(typeName);
            }
            else
            {
                objectType = Type.GetType(ResourcePropertyConstants.CoreAssemblyName +
                    ResourcePropertyConstants.Dot + typeName + "," + ResourcePropertyConstants.CoreAssemblyName);
                if (objectType == null)
                    objectType = Type.GetType(ResourcePropertyConstants.ScholarlyAssemblyName +
                        ResourcePropertyConstants.Dot + typeName + "," + ResourcePropertyConstants.ScholarlyAssemblyName);
            }
          
            return objectType;
        }

        /// <summary>
        ///     Returns the DataType for specific property.
        /// </summary>
        /// <param name="property">property DataType.</param>
        /// <returns>Data type of input property.</returns>
        private static DataTypes CheckDataType(Type property)
        {
            if (property == typeof(DateTime?))
            {
                return DataTypes.DateTime;
            }
            else if (property == typeof(Int32?))
            {
                return DataTypes.Int32;
            }
            else if (property == typeof(Int64?))
            {
                return DataTypes.Int64;
            }
            else if (property == typeof(Int16?))
            {
                return DataTypes.Int16;
            }
            else if (property == typeof(Guid))
            {
                return DataTypes.Guid;
            }
            else if (property == typeof(Boolean?))
            {
                return DataTypes.Boolean;
            }
            else
            {
                return DataTypes.String;
            }
        }

        /// <summary>
        /// Creates scalar property for upload file when resource type is file.
        /// </summary>
        /// <returns> Instance of scalar property </returns>
        private ScalarProperty GetUploadFileScalarProperty()
        {
            ScalarProperty uploadFileProperty = null;
            if (this.ControlMode != ResourcePropertiesOperationMode.Read)
            {
                uploadFileProperty = new ScalarProperty(ResourcePropertyConstants.ResourceFileUploadName
                    , DataTypes.Binary);
                uploadFileProperty.Nullable = true;
            }
            return uploadFileProperty;
        }


        /// <summary>
        /// Filters scalar properties as per resource type and read only mode.
        /// </summary>
        private void FilterPropertiesByType()
        {
            FilterExcludedProperties();
            if (this.ControlMode != ResourcePropertiesOperationMode.Read)
                FilterReadOnlyProperties();
        }

        /// <summary>
        ///     Filter the excluded property that specify in web.config file.
        /// </summary>
        private void FilterExcludedProperties()
        {
            ResourcePropertiesConfigSection resourceConfigSection =
                (ResourcePropertiesConfigSection)ConfigurationManager.GetSection(ResourcePropertyConstants.ResourcePropertySetting);

            if (resourceConfigSection != null)
            {
                for (int Index = 0; Index < resourceConfigSection.ExcludedProperties.Count; Index++)
                {
                    if (IsInHierarchy(resourceConfigSection.ExcludedProperties[Index].Class, true))
                    {
                        ScalarProperty property = GetScalarPropertyByName(resourceConfigSection.ExcludedProperties[Index].Name);
                        _scalarProperties.Remove(property);
                    }
                }
            }
        }

        /// <summary>
        /// Filters the read only properties that are specified in web.config file.
        /// </summary>
        public void FilterReadOnlyProperties()
        {
            ResourcePropertiesConfigSection resourceConfigSection =
                (ResourcePropertiesConfigSection)ConfigurationManager.GetSection(ResourcePropertyConstants.ResourcePropertySetting);

            if (resourceConfigSection != null)
            {
                for (int Index = 0; Index < resourceConfigSection.ReadOnlyProperties.Count; Index++)
                {
                    if (IsInHierarchy(resourceConfigSection.ReadOnlyProperties[Index].Class, true))
                    {
                        ScalarProperty property = GetScalarPropertyByName(resourceConfigSection.ReadOnlyProperties[Index].Name);
                        _scalarProperties.Remove(property);
                    }
                }
            }

        }

        /// <summary>
        /// Returns image control for lecture resource as control.
        /// </summary>        
        /// <returns> Instance of image as control object </returns>
        private static Control RenderImageControlForLecture()
        {
            System.Web.UI.WebControls.ImageButton lectureImage = new System.Web.UI.WebControls.ImageButton();
            return lectureImage;
        }

        /// <summary>
        /// Creates new table row object
        /// </summary>
        /// <param name="typeProperty"> Instance of scalar property </param>        
        /// <returns> Instance of table row </returns>
        private TableRow GetRow(ScalarProperty typeProperty)
        {
            TableRow row = null;

            if (typeProperty != null)
            {
                row = new TableRow();
                TableCell leftCell = new TableCell();
                TableCell rightCell = new TableCell();

                leftCell = CreateLabelCell(GetControlId(typeProperty.Name + ResourcePropertyConstants.Label), typeProperty.Name, LabelStyle);
                row.Cells.Add(leftCell);


                if (this.ControlMode == ResourcePropertiesOperationMode.Read)
                {
                    rightCell = GetRowForReadMode(typeProperty);
                }
                else
                {
                    rightCell = GetRowForEditMode(typeProperty);

                    if (this.ValidationRequired)
                    {
                        AddRequiredFieldValidator(typeProperty.Name, typeProperty.Nullable, rightCell);
                        if (!typeProperty.Nullable)
                        {
                            leftCell.Controls.Add(AddRequiredPropertyLabel());
                        }

                        AddRangeValidator(typeProperty.Name, typeProperty.DataType, rightCell);

                        AddRegExValidator(typeProperty.Name, typeProperty.DataType, rightCell);

                        if (!DesignMode)
                        {

                            ResourcePropertiesConfigSection resourceConfigSection =
                                (ResourcePropertiesConfigSection)ConfigurationManager.GetSection(ResourcePropertyConstants.ResourcePropertySetting);

                            if (resourceConfigSection != null)
                            {
                                if (resourceConfigSection.RequiredProperties[typeProperty.Name] != null)
                                    if (resourceConfigSection.RequiredProperties[typeProperty.Name].Name == typeProperty.Name)
                                    {
                                        if (resourceConfigSection.RequiredProperties[typeProperty.Name].Class != null)
                                        {
                                            if (IsInHierarchy(resourceConfigSection.RequiredProperties[typeProperty.Name].Class, true))
                                            {
                                                AddRequiredFieldValidator(typeProperty.Name, false, rightCell);
                                                leftCell.Controls.Add(AddRequiredPropertyLabel());
                                            }
                                        }
                                    }

                                if (resourceConfigSection.EmailProperties[typeProperty.Name] != null)
                                    if (resourceConfigSection.EmailProperties[typeProperty.Name].Name == typeProperty.Name)
                                    {
                                        if (IsInHierarchy(resourceConfigSection.EmailProperties[typeProperty.Name].Class, false))
                                            AddRegularExpressionValidator(typeProperty.Name, GlobalResource.InvalidEmail, resourceConfigSection.EmailProperties.RegularExpression, rightCell);
                                    }

                                for (int Index = 0; Index < resourceConfigSection.DateRangeProperties.Count; Index++)
                                {
                                    if ((resourceConfigSection.DateRangeProperties[Index].EndDateName == typeProperty.Name))
                                    {
                                        if (IsInHierarchy(resourceConfigSection.DateRangeProperties[Index].Class, true))
                                            AddDateCompareValidator(resourceConfigSection.DateRangeProperties[Index].Name, resourceConfigSection.DateRangeProperties[Index].EndDateName, rightCell);
                                        break;
                                    }
                                }

                                if (resourceConfigSection.ImageProperties[typeProperty.Name] != null)
                                    if (resourceConfigSection.ImageProperties[typeProperty.Name].Name == typeProperty.Name)
                                    {
                                        if (IsInHierarchy(resourceConfigSection.ImageProperties[typeProperty.Name].Class, true))
                                            AddRegularExpressionValidator(typeProperty.Name, GlobalResource.InvalidFileType, resourceConfigSection.ImageProperties.RegularExpression, rightCell);
                                    }
                            }
                        }
                    }
                }
                row.Cells.Add(rightCell);
            }
            return row;
        }

        private static Label AddRequiredPropertyLabel()
        {
            Label requiredProperty = new Label();
            requiredProperty.Text = _star;
            requiredProperty.ForeColor = System.Drawing.Color.Red;
            return requiredProperty;
        }

        /// <summary>
        ///     Check created resource is in actualClass Hierarchy.
        /// </summary>
        /// <param name="actualClass">actualClass for check in Hierarchy</param>
        /// <param name="IsInUpperHierarchy">check in upper Hierarchy</param>
        /// <returns>Boolean if actual class in Hierarchy</returns>
        private bool IsInHierarchy(string actualClass, bool IsInUpperHierarchy)
        {
            Type resourceType = GetTypeOfResource();
            Object resourceObj = null;
            if (resourceType != null)
            {
                resourceObj = Activator.CreateInstance(resourceType, null);
            }

            bool result = false;
            string namespaceName = CoreHelper.ExtractNamespace(actualClass);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                Type classType = GetTypeOfResource(namespaceName, actualClass);
                Resource classObj = Activator.CreateInstance(classType, null) as Resource;

                MethodInfo methodInfo = this.GetType().GetMethod("IsOfType",
                BindingFlags.NonPublic | BindingFlags.Static);

                if (methodInfo != null)
                {
                    result = (bool)methodInfo.MakeGenericMethod(classObj.GetType()).
                            Invoke(this, new object[] { resourceObj });

                    if (IsInUpperHierarchy)
                    {
                        if (!result)
                        {
                            result = (bool)methodInfo.MakeGenericMethod(resourceObj.GetType()).
                                Invoke(this, new object[] { classObj });

                        }
                    }
                }

            }
            return result;
        }

        /// <summary>
        ///  Check object in Hierarchy.
        /// </summary>
        /// <typeparam name="T">created resource</typeparam>
        /// <param name="resTypeObj">resource to check</param>
        /// <returns>Bool if object in Hierarchy</returns>
        private static bool IsOfType<T>(Object resTypeObj)
        {
            if (resTypeObj is T)
                return true;

            else return false;
        }

        /// <summary>
        /// Gets the type of Resource with full name.
        /// </summary>
        /// <param name="namespaceName">Namespace to which the class belongs to.</param>
        /// <param name="className">Class name</param>
        /// <returns>Type of resource with full name</returns>
        private static Type GetTypeOfResource(string namespaceName, string className)
        {
            Type resourceSystemType = null;

            Assembly zentityAssembly = Assembly.Load(namespaceName);

            resourceSystemType = zentityAssembly.GetType(className);

            return resourceSystemType;
        }

        /// <summary>
        /// Gets a table cell for containing property control in read mode.
        /// </summary>
        /// <param name="typeProperty"> associated scalar property </param>
        /// <returns> table cell containing property control </returns>
        private TableCell GetRowForReadMode(ScalarProperty typeProperty)
        {
            TableCell rightCell = new TableCell();

            Control propertyControl = null;

            if (IsImageRender(typeProperty.Name) &&
                        typeProperty.DataType == DataTypes.Binary)
            {
                propertyControl = RenderImageControlForLecture();
            }
            else
            {
                propertyControl = new Label();
            }

            propertyControl.ID = GetControlId(typeProperty.Name);
            rightCell.Controls.Add(propertyControl);

            PopulateResourceDetails(typeProperty, rightCell);

            return rightCell;
        }

        /// <summary>
        ///     Check is image need to render.
        /// </summary>
        /// <returns>true if images needs to render</returns>
        private bool IsImageRender(string propertyName)
        {
            bool result = false;
            if (!DesignMode)
            {
                ResourcePropertiesConfigSection resourceConfigSection =
                    (ResourcePropertiesConfigSection)ConfigurationManager.GetSection(ResourcePropertyConstants.ResourcePropertySetting);

                if (resourceConfigSection != null)
                {
                    for (int Index = 0; Index < resourceConfigSection.ImageProperties.Count; Index++)
                    {
                        if (resourceConfigSection.ImageProperties[Index].Name == propertyName)
                        {
                            if (IsInHierarchy(resourceConfigSection.ImageProperties[Index].Class, true))
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets a table cell for containing property control in create/edit mode.
        /// </summary>
        /// <param name="typeProperty"> associated scalar property </param>
        /// <returns> table cell containing property control </returns>
        private TableCell GetRowForEditMode(ScalarProperty typeProperty)
        {
            TableCell rightCell = new TableCell();

            // Not possible to optimize as done with GetRowForReadMode method
            if (typeProperty.DataType == DataTypes.Binary)
            {
                fileUploadControl = new FileUpload();
                fileUploadControl.ID = GetControlId(typeProperty.Name);
                fileUploadControl.Attributes.Add("contentEditable", "false");
                rightCell.Controls.Add(fileUploadControl);
                AddValidFileCustomvalidator(typeProperty.Name, rightCell);

                if (this.ControlMode != ResourcePropertiesOperationMode.Add)
                {
                    checkboxDeleteFile = new CheckBox();
                    checkboxDeleteFile.ID = GetControlId(typeProperty.Name) + ResourcePropertyConstants.CheckBox;
                    checkboxDeleteFile.Text = GlobalResource.DeleteFile;
                    rightCell.Controls.Add(checkboxDeleteFile);
                }

            }
            else if (typeProperty.DataType == DataTypes.Boolean)
            {
                CheckBox chkBox = new CheckBox();
                chkBox.ID = GetControlId(typeProperty.Name);
                rightCell.Controls.Add(chkBox);
            }
            else
            {
                TextBox txtControl = new TextBox();
                txtControl.ID = GetControlId(typeProperty.Name);

                txtControl.CssClass = this.ControlsStyle.CssClass;
                txtControl.ApplyStyle(ControlsStyle);

                rightCell.Controls.Add(txtControl);

                if (!(typeProperty.DataType == DataTypes.DateTime))
                {
                    if (typeProperty.MaxLength > 0)
                    {
                        txtControl.MaxLength = typeProperty.MaxLength;
                    }
                    if (typeProperty.MaxLength == ResourcePropertyConstants.TextMultiLineBase ||
                        typeProperty.MaxLength > MaxCharsForTextBox)
                    {
                        txtControl.TextMode = TextBoxMode.MultiLine;
                        txtControl.Rows = ResourcePropertyConstants.TextRowSpan;
                    }
                }
                if (typeProperty.Name == ResourcePropertyConstants.ResourceFileTypeExtension)
                    txtControl.Enabled = false;
            }

            PopulateResourceDetails(typeProperty, rightCell);

            return rightCell;
        }

        /// <summary>
        ///     Check enter file exist on file system and set IsValid property for validator.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Event argument</param>
        private void CheckUploadedFile(Object sender, ServerValidateEventArgs args)
        {
            //FileInfo file = new FileInfo(fileUploadControl.PostedFile);
            if (fileUploadControl.HasFile)
            {
                args.IsValid = true;
            }
            else
            {
                args.IsValid = false;
            }
        }

        /// <summary>
        /// Populates property control with respective resource object property value.
        /// </summary>
        /// <param name="typeProperty">associated scalar property</param>
        /// <param name="rightCell">table cell containing control</param>
        private void PopulateResourceDetails(ScalarProperty typeProperty, TableCell rightCell)
        {
            object value = null;
            if (this.ControlMode != ResourcePropertiesOperationMode.Add)
            {
                value = _propertyNameValue[typeProperty.Name];

                if (this.ControlMode != ResourcePropertiesOperationMode.Read)
                {
                    SetValue(typeProperty.DataType, rightCell.Controls[0], value);
                }
                else
                {
                    SetReadOnlyValue(typeProperty.DataType, rightCell.Controls[0], value);
                }
            }
        }

        /// <summary>
        /// Sets property control with value from respective resource obj property.
        /// </summary>
        /// <param name="dataType"> data type of associated property </param>
        /// <param name="controlToUpdate"> Property control to be set with respective value </param>
        /// <param name="value"> Value to be set in control to be updated </param>
        private static void SetValue(DataTypes dataType,
            Control controlToUpdate,
             object value)
        {
            Control fldControl = controlToUpdate;

            if (dataType == DataTypes.Boolean)
            {
                CheckBox chkCtrl = fldControl as CheckBox;

                if (chkCtrl != null)
                {
                    chkCtrl.Checked = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                TextBox txtCtrl = fldControl as TextBox;

                if (txtCtrl != null)
                {
                    if (dataType == DataTypes.DateTime)
                    {
                        DateTime date = TransformToDate(value);
                        if (date != DateTime.MinValue)
                        {
                            txtCtrl.Text = date.ToString(CoreHelper.GetDateFormat(), CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        txtCtrl.Text = Convert.ToString(value, CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        /// <summary>
        /// Sets property control with value from respective resource obj property.
        /// </summary>
        /// <param name="dataType"> data type of associated property </param>
        ///<param name="controlToUpdate"> Instance of property control to be updated  </param>
        ///<param name="value"> Value to be assigned to property control  </param>
        private void SetReadOnlyValue(DataTypes dataType,
            Control controlToUpdate, object value)
        {

            if (dataType == DataTypes.Binary)
            {
                System.Web.UI.WebControls.ImageButton imgControl = controlToUpdate as System.Web.UI.WebControls.ImageButton;
                if (imgControl != null)
                {
                    RenderImage(imgControl, value);
                }
            }
            else
            {
                Label lblCtrl = (Label)controlToUpdate;
                if (lblCtrl == null) return;
                if (dataType == DataTypes.Boolean)
                {
                    bool parsedValue = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                    if (parsedValue)
                    {
                        lblCtrl.Text = ResourcePropertyConstants.BooleanTrue;
                    }
                    else
                    {
                        lblCtrl.Text = ResourcePropertyConstants.BooleanFalse;
                    }
                }
                else if (dataType == DataTypes.DateTime)
                {
                    DateTime date = TransformToDate(value);
                    if (date != DateTime.MinValue)
                    {
                        lblCtrl.Text = date.ToString(CoreHelper.GetDateFormat(), CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        lblCtrl.Text = Convert.ToString(value, CultureInfo.InvariantCulture);
                    }
                }
                else if (dataType == DataTypes.String)
                {
                    lblCtrl.Text = HttpUtility.HtmlEncode(Convert.ToString(value, CultureInfo.InvariantCulture));
                }
                else
                {
                    lblCtrl.Text = Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Returns string as per text controls max length property.
        /// </summary>
        /// <param name="txtCtrl"> Text control whose contents to be returned </param>
        /// <returns> String value </returns>
        private static string ReturnMaxLengthString(TextBox txtCtrl)
        {
            string text = string.Empty;
            if (txtCtrl.MaxLength < 1)
            {
                text = txtCtrl.Text;
            }
            else
            {
                if (txtCtrl.Text.Length > txtCtrl.MaxLength)
                {
                    text = txtCtrl.Text.Substring(0, txtCtrl.MaxLength);
                }
                else
                {
                    text = txtCtrl.Text;
                }
            }
            return text;
        }

        /// <summary>
        /// Sets image url of image control.
        /// </summary>
        /// <param name="imgCtrl"> Image control whose image url to be set </param>
        /// <param name="value"> Image to be rendered on server file system and attached to image control </param>
        private void RenderImage(System.Web.UI.WebControls.ImageButton imgCtrl, object value)
        {
            if (value == null)
            {
                imgCtrl.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), _noImagePath);
                imgCtrl.Attributes.Add("onclick", "return false");
                imgCtrl.Style.Add("cursor", "default");
                return;
            }
            _fullLectureImage = (byte[])value;

            System.Drawing.Image htmlImage = ByteArrayToImage(_fullLectureImage);
            System.Drawing.Image thumnailImage = htmlImage.GetThumbnailImage(_imageWidth, _imageHeight, new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
            this.Page.Session[_thumbnailImage] = ImageToByteArray(thumnailImage);

            imgCtrl.ImageUrl = _thumbnailCallbackUrl + this.ResourceId;
            imgCtrl.Click += new ImageClickEventHandler(imgCtrl_Click);
            imgCtrl.ToolTip = GlobalResource.AlternateThumbImageText;
        }

        /// <summary>
        ///     Returns System.Drawing.Image from byte array.
        /// </summary>
        /// <param name="byteArrayIn">array of byte</param>
        /// <returns>System.Drawing.Image</returns>
        private static System.Drawing.Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream memoryStream = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(memoryStream);
            return returnImage;
        }

        /// <summary>
        ///     Returns byte array from System.Drawing.Image.
        /// </summary>
        /// <param name="imageIn">System.Drawing.Image</param>
        /// <returns>array of byte</returns>
        private static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream memoryStream = new MemoryStream();
            imageIn.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return memoryStream.ToArray();
        }

        /// <summary>
        ///     Click event redirect to the full image.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event argument</param>
        private void imgCtrl_Click(object sender, ImageClickEventArgs e)
        {
            this.Page.Response.BinaryWrite(_fullLectureImage);
        }

        /// <summary>
        /// Validates incoming data as per datatype specified.
        /// </summary>
        /// <param name="typeOfData"> Datatype </param>        
        /// <param name="value"> Value to be validated </param>
        /// <returns> Validated value </returns>
        private static object ValidateData(DataTypes typeOfData, object value)
        {
            object validValue = null;
            try
            {
                switch (typeOfData)
                {
                    case DataTypes.String:
                        validValue = System.Convert.ToString(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Int64:
                        validValue = System.Convert.ToInt64(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Int32:
                        validValue = System.Convert.ToInt32(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Int16:
                        validValue = System.Convert.ToInt16(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Decimal:
                        validValue = System.Convert.ToDecimal(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Double:
                        validValue = System.Convert.ToDouble(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Single:
                        validValue = System.Convert.ToSingle(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Byte:
                        validValue = System.Convert.ToByte(value, CultureInfo.CurrentCulture);
                        break;
                    case DataTypes.Boolean:
                        validValue = System.Convert.ToBoolean(value, CultureInfo.CurrentCulture);
                        break;
                }
            }
            catch (FormatException)
            {

            }
            return validValue;
        }

        /// <summary>
        /// Transforms object value to datetime.
        /// </summary>        
        /// <param name="value"> value to be transformed </param>
        /// <returns> transformed datetime value </returns>
        private static DateTime TransformToDate(object value)
        {
            DateTime temp = DateTime.MinValue;
            try
            {
                temp = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
            }
            return temp;
        }

        /// <summary>
        /// Validates value as datetime.
        /// </summary>
        /// <param name="value"> value to be validated </param>
        /// <returns> validated datetime value </returns>
        private static DateTime ValidateDate(object value)
        {
            DateTime date = DateTime.MinValue;
            bool success = DateTime.TryParse(System.Convert.ToString(value, CultureInfo.InvariantCulture),
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out date);
            if (success)
            {
                return date;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        private string GetControlId(string name)
        {
            if (!string.IsNullOrEmpty(this.ID))
            {
                return this.ID + name;
            }

            return name;

        }

        /// <summary>
        /// Gets the resource with specified id.
        /// </summary>
        /// <param name="resourceId">Id of the resource to be fetched.</param>
        /// <returns>Resource having the specified id.</returns>
        public Resource GetResourceData(Guid resourceId)
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                return dataAccess.GetResource(resourceId);
            }
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Helps in determining in which mode control is operating   
    /// </summary>
    public enum ResourcePropertiesOperationMode
    {
        /// <summary>
        /// Sets control in add mode
        /// </summary>
        Add,
        /// <summary>
        /// Sets control in edit mode
        /// </summary>
        Edit,
        /// <summary>
        /// Sets control in read mode
        /// </summary>
        Read
    }
}
