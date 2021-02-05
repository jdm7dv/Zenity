// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.Platform;
using Zentity.ScholarlyWorks;
using Zentity.Security.Authentication;
using Zentity.Security.AuthorizationHelper;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control displays related resources of a subject resource.
    /// </summary>
    [DefaultProperty("Text")]
    internal class RelatedResourcesCloud : ZentityBase
    {
        #region Member Variables

        private Panel _titlePanel;
        private Panel _bodyPanel;
        private Panel _bibTexPanel;
        private Label _titleLabel;
        private Label _seperatorLabel;
        //TODO: Uncomment to show edit link.
        //private HyperLink _editLink;
        private TableItemStyle _ItemStyle;
        private LinkButton _bibTexImportLink;
        private LinkButton _bibTexExportLink;
        private IEnumerable<string> _userPermissions;

        #endregion

        #region Constants

        private const string _titlePanelId = "titlePanelId";
        private const string _bodyPanelId = "bodyPanelId";
        private const string _bibTexPanelId = "bibTexPanelId";
        private const string _titleLabelId = "titleLabelId";
        private const string _seperatorLabelId = "seperatorLabelId";
        //TODO: Uncomment to show edit link.
        //private const string _editLinkId = "editLinkId";
        private const string _bibTexImportLinkId = "bibTexImportLinkId";
        private const string _bibTexExportLinkId = "bibTexExportLinkId";

        private const string _subjectResourceKey = "subjectResourceKey";
        private const string _cloudTitleKey = "cloudTitleKey";
        private const string _navigationPropertyKey = "navigationPropertyKey";
        //TODO: Uncomment to show edit link.
        //private const string _editUrlKey = "editUrlKey";
        private const string _viewUrlKey = "viewUrlKey";
        private const string _bibTexImportUrlKey = "bibTexImportUrlKey";
        private const string _userPermissionsKey = "userPermissionsKey";
        private const string _editTagAssociationUrlKey = "editTagAssociationUrlKey";
        private const string _editCategoryAssociationUrlKey = "editCategotyAssociationUrlKey";


        private const string _CitesProperty = "Cites";
        const string _bibExtention = ".bib";
        const string _attachment = "attachment; filename=";
        const string _contentTypeOctetStream = "application/octet-stream";
        const string _responseHeader = "Content-Disposition";

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
        /// Gets or sets navigation property name.
        /// </summary>
        public string NavigationPropertyName
        {
            get { return ViewState[_navigationPropertyKey] != null ? (string)ViewState[_navigationPropertyKey] : string.Empty; }
            set { ViewState[_navigationPropertyKey] = value; }
        }

        //TODO: Uncomment to show edit link.
        ///// <summary>
        ///// Gets or set edit url.
        ///// </summary>
        //public string EditUrl
        //{
        //    get { return ViewState[_editUrlKey] != null ? (string)ViewState[_editUrlKey] : string.Empty; }
        //    set { ViewState[_editUrlKey] = value; }
        //}

        /// <summary>
        /// Gets or sets view url.
        /// </summary>
        public string ViewUrl
        {
            get { return ViewState[_viewUrlKey] != null ? (string)ViewState[_viewUrlKey] : string.Empty; }
            set { ViewState[_viewUrlKey] = value; }
        }

        /// <summary>
        /// Gets or sets BibTex import url.
        /// </summary>
        public string BibTexImportUrl
        {
            get { return ViewState[_bibTexImportUrlKey] != null ? (string)ViewState[_bibTexImportUrlKey] : string.Empty; }
            set { ViewState[_bibTexImportUrlKey] = value; }
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

                    Table table = new Table();
                    table.Width = Unit.Percentage(100);

                    TableRow tr = new TableRow();

                    TableCell td = new TableCell();
                    td.HorizontalAlign = HorizontalAlign.Left;
                    td.Controls.Add(TitleLabel);
                    tr.Cells.Add(td);

                    //TODO: Uncomment to show edit link.
                    //td = new TableCell();
                    //td.HorizontalAlign = HorizontalAlign.Right;
                    //td.Controls.Add(EditLink);
                    //tr.Cells.Add(td);

                    table.Rows.Add(tr);

                    _titlePanel.Controls.Add(table);
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

        private Panel BibTexPanel
        {
            get
            {
                if (_bibTexPanel == null)
                {
                    _bibTexPanel = new Panel();
                    _bibTexPanel.ID = _bibTexPanelId;
                    _bibTexPanel.Style.Add(HtmlTextWriterStyle.Margin, "5px");
                    _bibTexPanel.HorizontalAlign = HorizontalAlign.Left;

                    _bibTexPanel.Controls.Add(BibTexImportLink);
                    _bibTexPanel.Controls.Add(SeperatorLabel);
                    _bibTexPanel.Controls.Add(BibTexExportLink);
                }

                return _bibTexPanel;
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
                    _titleLabel.Text = string.IsNullOrEmpty(Title) ? Title : GlobalResource.DefaultRelatedResourceCloudText;
                }

                return _titleLabel;
            }
        }

        private Label SeperatorLabel
        {
            get
            {
                if (_seperatorLabel == null)
                {
                    _seperatorLabel = new Label();
                    _seperatorLabel.ID = _seperatorLabelId;
                    _seperatorLabel.Font.Bold = true;
                    _seperatorLabel.Text = "|";
                }

                return _seperatorLabel;
            }
        }

        //TODO: Uncomment to show edit link.
        //private HyperLink EditLink
        //{
        //    get
        //    {
        //        if (_editLink == null)
        //        {
        //            _editLink = new HyperLink();
        //            _editLink.ID = _editLinkId;
        //            _editLink.Text = GlobalResource.RelatedResourceEditLinkText;
        //        }

        //        return _editLink;
        //    }

        //}

        private LinkButton BibTexImportLink
        {
            get
            {
                if (_bibTexImportLink == null)
                {
                    _bibTexImportLink = new LinkButton();
                    _bibTexImportLink.ID = _bibTexImportLinkId;
                    _bibTexImportLink.Text = GlobalResource.BibTexImportLabel;
                    _bibTexImportLink.Click += new EventHandler(BibTexImportLink_Click);
                }

                return _bibTexImportLink;
            }
        }

        private LinkButton BibTexExportLink
        {
            get
            {
                if (_bibTexExportLink == null)
                {
                    _bibTexExportLink = new LinkButton();
                    _bibTexExportLink.ID = _bibTexExportLinkId;
                    _bibTexExportLink.Text = GlobalResource.BibTexExportLabel;
                    _bibTexExportLink.Click += new EventHandler(BibTexExportLink_Click);
                }

                return _bibTexExportLink;
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
            TitleLabel.Text = Title;

            if (SubjectResource != null)
            {
                NavigationProperty navigationProperty = GetNavigationProperty(SubjectResource);

                //TODO: Uncomment to show edit link.
                ////If this control is security unaware OR this control is security aware control and 
                ////user is having update permission on the resource then only enable Edit link.
                //if (!IsSecurityAwareControl || (UserPermissions != null && UserPermissions.Contains(UserResourcePermissions.Update)))
                //{
                //    if (navigationProperty != null)
                //    {
                //        if(!string.IsNullOrEmpty(EditUrl))
                //            EditLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, EditUrl, SubjectResource.Id, navigationProperty.Id);
                //    }
                //    else
                //    {
                //        if (!string.IsNullOrEmpty(EditUrl))
                //            EditLink.NavigateUrl = string.Format(CultureInfo.CurrentCulture, EditUrl, SubjectResource.Id);
                //    }
                //}
                //else
                //{
                //    EditLink.Visible = false;
                //}

                bool success = PopulateBodyPanelWithResourceLinks(SubjectResource.Id, navigationProperty);

                if (!success && !_CitesProperty.Equals(NavigationPropertyName))
                {
                    this.Visible = false;
                }
                else if (_CitesProperty.Equals(NavigationPropertyName))
                {
                    AddBibTexLinks();
                }
            }
            ApplyStyles();

            base.OnLoad(e);
        }

        #endregion

        #region Private

        private bool PopulateBodyPanelWithResourceLinks(Guid resourceId, NavigationProperty navigationProperty)
        {
            bool success = false;
            List<Resource> relatedResources = null;
            IDictionary<Guid, IEnumerable<string>> userPermissions = null;
            if (this.DesignMode)
            {
                relatedResources = GetDesignTimeRelatedResources();
            }
            else
            {
                if (!IsSecurityAwareControl)
                {
                    relatedResources = GetRelatedResources(resourceId, navigationProperty);
                }
                else
                {
                    //If control is security aware the get resources as well as their respective user permissions.
                    var resourcesWithPermissions = GetRelatedResources(AuthenticatedToken, resourceId, navigationProperty);
                    if (resourcesWithPermissions != null)
                    {
                        relatedResources = resourcesWithPermissions.Select(tuple => tuple.Resource).ToList();
                        userPermissions = resourcesWithPermissions.ToDictionary(tuple => tuple.Resource.Id, tuple => tuple.Permissions);
                    }
                }
            }
            if (relatedResources != null && relatedResources.Count > 0)
            {
                //Create a bulleted list with the related resource titles as items. 
                Panel container = CreateScrollableContainer();
                BulletedList list = CreateBulletedList();
                foreach (Resource res in relatedResources)
                {
                    AddResourceLinkToList(list, res);
                }
                container.Controls.Add(list);
                BodyPanel.Controls.Add(container);
                success = true;
            }
            return success;
        }

        private static Panel CreateScrollableContainer()
        {
            Panel container = new Panel();
            container.ScrollBars = ScrollBars.Vertical;
            container.Height = 45;
            return container;
        }

        private static BulletedList CreateBulletedList()
        {
            BulletedList list = new BulletedList();
            list.BulletStyle = BulletStyle.Square;
            list.DisplayMode = BulletedListDisplayMode.HyperLink;
            return list;
        }

        //Adds resource link to the list.
        private void AddResourceLinkToList(BulletedList list, Resource res)
        {
            string linkText = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(
                CoreHelper.GetTitleByResourceType(res)), _maxTitleCharWidth));
            string linkUrl = string.Empty;
            linkUrl = string.Format(CultureInfo.CurrentCulture, ViewUrl, res.Id);
            ListItem item = new ListItem(linkText, linkUrl);
            //TODO - The following attribute is added for showing tooltip - but it does not work.
            item.Attributes.Add("title", linkText);
            list.Items.Add(new ListItem(linkText, linkUrl));
        }

        private void AddBibTexLinks()
        {
            using (ResourceDataAccess resourceDAL = new ResourceDataAccess(this.CreateContext()))
            {
                if (!IsSecurityAwareControl || (UserPermissions != null && UserPermissions.Contains(UserResourcePermissions.Read)))
                {
                    ScholarlyWork scholarlyWorkObj = (ScholarlyWork)resourceDAL.GetScholarlyWorkWithCitedScholarlyWorks(SubjectResource.Id);

                    if (scholarlyWorkObj != null && scholarlyWorkObj.Cites.Count > 0)
                        BibTexExportLink.Visible = true;
                    else
                        BibTexExportLink.Visible = false;
                }

                if (!IsSecurityAwareControl || (UserPermissions != null && UserPermissions.Contains(UserResourcePermissions.Update)))
                    BibTexImportLink.Visible = true;
                else
                    BibTexImportLink.Visible = false;

                SeperatorLabel.Visible = (BibTexExportLink.Visible == false || BibTexImportLink.Visible == false) ? false : true;
            }

            this.Controls.Add(BibTexPanel);
        }

        private List<Resource> GetRelatedResources(Guid resourceId, NavigationProperty navigationProperty)
        {
            List<Resource> relatedResources = null;

            if (resourceId != Guid.Empty && navigationProperty != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    relatedResources = dataAccess.GetRelatedResources(resourceId, navigationProperty, null, UserResourcePermissions.Read);
                }
            }

            return relatedResources;
        }

        private IEnumerable<ResourcePermissions<Resource>> GetRelatedResources(AuthenticatedToken token, Guid resourceId,
            NavigationProperty navigationProperty)
        {
            IEnumerable<ResourcePermissions<Resource>> resPermissions = null;

            if (resourceId != Guid.Empty && navigationProperty != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    List<Resource> relatedResources = dataAccess.GetRelatedResources(resourceId, navigationProperty, null, UserResourcePermissions.Read);

                    if (relatedResources != null)
                        resPermissions = dataAccess.GetResourcePermissions(token, relatedResources);
                }
            }
            return resPermissions;
        }

        private NavigationProperty GetNavigationProperty(Resource resource)
        {
            NavigationProperty navProperty = null;
            if (!string.IsNullOrEmpty(NavigationPropertyName) && resource != null)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    navProperty = dataAccess.GetNavigationProperty(this.Page.Cache,
                    resource.GetType().Name, NavigationPropertyName);
                }
            }

            return navProperty;
        }

        private static List<Resource> GetDesignTimeRelatedResources()
        {
            List<Resource> relatedResources = new List<Resource>();
            relatedResources.Add(new Resource() { Title = "Resource1" });
            relatedResources.Add(new Resource() { Title = "Resource2" });
            relatedResources.Add(new Resource() { Title = "Resource3" });
            relatedResources.Add(new Resource() { Title = "Resource4" });

            return relatedResources;
        }

        private void ApplyStyles()
        {
            TitlePanel.ApplyStyle(TitleStyle);
            BodyPanel.ApplyStyle(ItemStyle);
            BibTexPanel.ApplyStyle(ItemStyle);
        }

        void BibTexImportLink_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(BibTexImportUrl) && this.SubjectResource != null)
            {
                this.Page.Response.Redirect(string.Format(CultureInfo.InvariantCulture, BibTexImportUrl, SubjectResource.Id));
            }
        }

        void BibTexExportLink_Click(object sender, EventArgs e)
        {
            if (SubjectResource == null)
                return;

            ScholarlyWork scholarlyWorkObj = null;
            using (ResourceDataAccess resourceDAL = new ResourceDataAccess(this.CreateContext()))
            {
                scholarlyWorkObj = (ScholarlyWork)resourceDAL.GetScholarlyWorkWithCitedScholarlyWorks(SubjectResource.Id);

                if (scholarlyWorkObj != null)
                {
                    ICollection<ScholarlyWork> citationsList = scholarlyWorkObj.Cites;
                    List<ScholarlyWork> citations = resourceDAL.GetAuthorizedResources<ScholarlyWork>
                        (AuthenticatedToken, UserResourcePermissions.Read, citationsList).ToList();
                    if (citations.Count > 0)
                    {
                        String fileNameToSend = scholarlyWorkObj.Id.ToString() + _bibExtention;
                        String value = _attachment + fileNameToSend;
                        this.Page.Response.ContentType = _contentTypeOctetStream;
                        this.Page.Response.AddHeader(_responseHeader, value);
                        this.Page.Response.Clear();
                        BibTeXConverter bibConverter = new BibTeXConverter(BibTeXParserBehavior.IgnoreParseErrors);

                        foreach (ScholarlyWork swork in citations)
                        {
                            swork.Authors.Load();
                            swork.Editors.Load();
                        }
                        bibConverter.Export(citations, this.Page.Response.OutputStream);
                        this.Page.Response.Flush();
                        this.Page.Response.End();
                    }
                    else
                    {
                        BibTexExportLink.Visible = false;
                        SeperatorLabel.Visible = false;
                    }

                }
            }

        }

        #endregion

        #endregion
    }
}
