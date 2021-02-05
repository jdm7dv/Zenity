// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.Security.Authentication;
using Zentity.Security.AuthorizationHelper;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control displays related downloads of the specified subject resource.
    /// </summary>
    [DefaultProperty("Text")]
    internal class RelatedDownloads : ZentityBase
    {
        #region Member Variables

        private Panel _titlePanel;
        private Panel _bodyPanel;
        private Label _titleLabel;
        private TableItemStyle _ItemStyle;
        private IEnumerable<string> _userPermissions;

        #endregion

        #region Constants

        private const string _titlePanelId = "titlePanelId";
        private const string _bodyPanelId = "bodyPanelId";
        private const string _titleLabelId = "titleLabelId";

        private const string _subjectResourceKey = "_subjectFileKey";
        private const string _userPermissionsKey = "userPermissionsKey";
        private const string _FilesProperty = "Files";

        private int _maxTitleCharWidth = 30;
        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets subject resource Id.
        /// </summary>
        public Resource SubjectResource
        {
            get { return ViewState[_subjectResourceKey] != null ? ViewState[_subjectResourceKey] as Resource : null; }
            set { ViewState[_subjectResourceKey] = value; }
        }

        /// <summary>
        /// Gets or sets style to be applied to the items.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ItemStyle
        {
            get
            {
                if (this._ItemStyle == null)
                {
                    this._ItemStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._ItemStyle).TrackViewState();
                    }
                }
                return this._ItemStyle;
            }
        }

        #endregion

        #region Internal

        internal IEnumerable<string> UserPermissions
        {
            get { return _userPermissions; }
            set { _userPermissions = value; }
        }

        #endregion

        #region Private

        private Panel TitlePanel
        {
            get
            {
                if (_titlePanel == null)
                {
                    _titlePanel = new Panel();
                    _titlePanel.Style.Add(HtmlTextWriterStyle.Margin, "5px");
                    _titlePanel.ID = _titlePanelId;
                    _titlePanel.HorizontalAlign = HorizontalAlign.Left;
                    _titlePanel.Controls.Add(TitleLabel);
                }

                return _titlePanel;
            }
        }

        private Panel BodyPanel
        {
            get
            {
                if (_bodyPanel == null)
                {
                    _bodyPanel = new Panel();
                    _bodyPanel.ID = _bodyPanelId;
                    _bodyPanel.Style.Add(HtmlTextWriterStyle.Margin, "10px");
                    _bodyPanel.HorizontalAlign = HorizontalAlign.Left;
                }

                return _bodyPanel;
            }

        }

        private Label TitleLabel
        {
            get
            {
                if (_titleLabel == null)
                {
                    _titleLabel = new Label();
                    _titleLabel.ID = _titleLabelId;
                    _titleLabel.Font.Bold = true;
                    _titleLabel.Text = GlobalResource.RelatedDownloadsLabelText;
                }

                return _titleLabel;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Protected

        /// <summary>
        /// Creates child controls layout.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Controls.Add(TitlePanel);
            this.Controls.Add(BodyPanel);
        }

        /// <summary>
        /// Populates related resources.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (SubjectResource != null)
            {
                NavigationProperty navigationProperty = GetNavigationProperty(SubjectResource);
                if (!PopulateBodyPanelWithRelatedDownloads(SubjectResource.Id, navigationProperty))
                {
                    this.Visible = false;
                }
            }
            ApplyStyles();

            base.OnLoad(e);
        }

        #endregion

        #region Private

        private bool PopulateBodyPanelWithRelatedDownloads(Guid fileId, NavigationProperty navigationProperty)
        {
            bool success = false;
            List<File> relatedResources = null;
            IDictionary<Guid, IEnumerable<string>> userPermissions = null;
            if (this.DesignMode)
            {
                relatedResources = GetDesignTimeRelatedFiles();
            }
            else
            {
                if (!IsSecurityAwareControl)
                {
                    relatedResources = GetRelatedFiles(fileId, navigationProperty);
                }
                else
                {
                    //If control is security aware the get resources as well as their respective user permissions.
                    IEnumerable<ResourcePermissions<File>> resourcesWithPermissions =
                        GetRelatedFiles(AuthenticatedToken, fileId, navigationProperty);
                    if (resourcesWithPermissions != null)
                    {
                        relatedResources = resourcesWithPermissions.Select(tuple => tuple.Resource).ToList();
                        userPermissions = resourcesWithPermissions.ToDictionary(tuple => tuple.Resource.Id, tuple => tuple.Permissions);
                    }
                }
            }
            if (relatedResources != null && relatedResources.Count > 0)
            {
                Table table = new Table();
                table.Width = Unit.Percentage(100);

                foreach (File file in relatedResources)
                {
                    //if control is security unaware OR if control is security aware and user is having 
                    //Read permission on the resource then only set the link.
                    if (!IsSecurityAwareControl ||
                        (userPermissions.Keys.Contains(file.Id) && userPermissions[file.Id].Contains(UserResourcePermissions.Read)))
                    {
                        using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                        {
                            string fileSize = dataAccess.GetUploadedFileSize(file);
                            if (!string.IsNullOrEmpty(fileSize))
                            {
                                TableRow tr = new TableRow();
                                TableCell td = new TableCell();
                                td.HorizontalAlign = HorizontalAlign.Left;
                                td.VerticalAlign = VerticalAlign.Top;
                                td.Width = Unit.Percentage(70);
                                LinkButton fileNameLink = new LinkButton();
                                fileNameLink.ID = file.Id.ToString();
                                fileNameLink.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(
                                    CoreHelper.UpdateEmptyTitle(CoreHelper.GetFileName(file.Title, file.FileExtension)),
                                    _maxTitleCharWidth));
                                fileNameLink.Click += new EventHandler(fileNameLink_Click);
                                td.Controls.Add(fileNameLink);
                                tr.Cells.Add(td);
                                td = new TableCell();
                                td.HorizontalAlign = HorizontalAlign.Left;
                                td.VerticalAlign = VerticalAlign.Top;
                                Label fileSizeLabel = new Label();
                                fileSizeLabel.Text = fileSize;
                                td.Controls.Add(fileSizeLabel);
                                tr.Cells.Add(td);

                                table.Rows.Add(tr);
                            }
                        }
                    }
                }
                BodyPanel.Controls.Add(table);

                success = true;
            }

            return success;
        }

        private void fileNameLink_Click(object sender, EventArgs e)
        {
            LinkButton lnkButton = sender as LinkButton;
            if (lnkButton != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    dataAccess.DownloadFile(new Guid(lnkButton.ID), this.Page.Response);
                }

            }
        }

        private List<File> GetRelatedFiles(Guid fileId, NavigationProperty navigationProperty)
        {
            List<File> relatedResources = null;

            if (fileId != Guid.Empty && navigationProperty != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    var resources = dataAccess.GetRelatedResources(fileId, navigationProperty, null, UserResourcePermissions.Read);
                    if (resources != null)
                        relatedResources = resources.Select(tuple => tuple as File).ToList();
                }
            }

            return relatedResources;
        }

        private IEnumerable<ResourcePermissions<File>> GetRelatedFiles(AuthenticatedToken token, Guid fileId,
            NavigationProperty navigationProperty)
        {
            IEnumerable<ResourcePermissions<File>> resPermissions = null;

            if (fileId != Guid.Empty && navigationProperty != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    var resources = dataAccess.GetRelatedResources(fileId, navigationProperty, null, UserResourcePermissions.Read);

                    if (resources != null)
                    {
                        List<File> relatedFiles = resources.Select(tuple => tuple as File).ToList();
                        resPermissions = dataAccess.GetResourcePermissions<File>(token, relatedFiles);
                    }
                }
            }
            return resPermissions;
        }

        private NavigationProperty GetNavigationProperty(Resource resource)
        {
            NavigationProperty navProperty = null;
            if (resource != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    navProperty = dataAccess.GetNavigationProperty(this.Page.Cache, resource.GetType().Name, _FilesProperty);
                }
            }

            return navProperty;
        }

        private static List<File> GetDesignTimeRelatedFiles()
        {
            List<File> relatedResources = new List<File>();
            relatedResources.Add(new File() { Title = "File1" });
            relatedResources.Add(new File() { Title = "File2" });
            relatedResources.Add(new File() { Title = "File3" });
            relatedResources.Add(new File() { Title = "File4" });

            return relatedResources;
        }

        private void ApplyStyles()
        {
            TitlePanel.ApplyStyle(TitleStyle);
            BodyPanel.ApplyStyle(ItemStyle);
        }

        #endregion

        #endregion
    }
}
