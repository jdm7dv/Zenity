// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Security.Authentication;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Represents a control that can be used for viewing reports.
    /// </summary>
    /// <example>The code below is the source for ReportingView.aspx. It shows an example of using <see cref="ReportingView"/>.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="Zentity" %&gt;
    ///             
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///             &lt;head runat="server"&gt;
    ///                 &lt;title&gt;ReportingView Sample&lt;/title&gt;
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:ReportingView ID="ReportingView1" runat="server" Title="Recently Added Resources"
    ///                     ViewUrl="ResourceDetailView.aspx?Id={0}" ShowReportType="RecentlyAddedResources" 
    ///                     IsSecurityAwareControl="false"&gt;
    ///                 &lt;/Zentity:ReportingView&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [DefaultProperty("Text")]
    //[Designer(typeof(ReportingViewDesigner))]
    public class ReportingView : ZentityBase
    {
        #region Member Variables
        ResourceListView _resourceListView;
        #endregion

        #region Constants
        private const string _resourceListViewId = "resourceListViewId";
        const string _getReportTypeKey = "getReportTypeKey";
        const string _pageSizeViewStateKey = "PageSize";
        const int _defaultPageSize = 10;
        const string _dateAddedProperty = "DateAdded";
        const string _dateModifiedProperty = "DateModified";
        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the report type.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [Localizable(true)]
        public ReportType ShowReportType
        {
            get
            {
                return ViewState[_getReportTypeKey] != null ?
                    (ReportType)ViewState[_getReportTypeKey] : ReportType.RecentlyAddedResources;
            }
            set
            {
                ViewState[_getReportTypeKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of records to be displayed.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionTablePageSize")]
        [Localizable(true)]
        public int PageSize
        {
            get
            {
                return ViewState[_pageSizeViewStateKey] != null ? (int)ViewState[_pageSizeViewStateKey] : _defaultPageSize;
            }
            set
            {
                ViewState[_pageSizeViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets view link Url. Url format should be Url?Id={0}. {0} will be replaced by actual entity id.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionZentityGridViewViewUrl")]
        public string ViewUrl
        {
            get
            {
                return ResourceListView.ViewUrl;
            }
            set
            {
                ResourceListView.ViewUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the container.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ContainerStyle
        {
            get
            {
                return ResourceListView.ContainerStyle;
            }
        }


        /// <summary>
        /// Gets or sets style to be applied to the Title.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public new TableItemStyle TitleStyle
        {
            get
            {
                return ResourceListView.TitleStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the Separator that Separate a record.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle SeparatorStyle
        {
            get
            {
                return ResourceListView.SeparatorStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the All Items.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle ItemStyle
        {
            get
            {
                return ResourceListView.ItemStyle;
            }
        }

        /// <summary>
        /// Gets or sets style to be applied to the Alternate resource items.
        /// </summary>
        [ZentityCategory("CategoryStyles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle AlternateItemStyle
        {
            get
            {
                return ResourceListView.AlternateItemStyle;
            }
        }

        #endregion

        #region Private

        private ResourceListView ResourceListView
        {
            get
            {
                if (_resourceListView == null)
                {
                    _resourceListView = new ResourceListView();
                    _resourceListView.ID = _resourceListViewId;
                    _resourceListView.Width = Unit.Percentage(100);
                    _resourceListView.ShowFooter = false;
                    _resourceListView.ShowHeader = false;
                    _resourceListView.EnableDeleteOption = false;
                }

                return _resourceListView;
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Creates child controls layout.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.Controls.Add(ResourceListView);

            if (this.DesignMode)
            {
                List<Resource> dataSource = new List<Resource>();

                for (int Index = 0; Index < 5; Index++)
                {
                    Resource resource = new Resource();
                    resource.Title = GlobalResource.ResourceTitle;
                    resource.DateModified = DateTime.Now;
                    resource.DateAdded = DateTime.Now;
                    resource.Description = GlobalResource.ResourceDescription;
                    dataSource.Add(resource);
                }

                ResourceListView.DataSource.Clear();
                foreach (Resource resource in dataSource)
                    ResourceListView.DataSource.Add(resource);

                ResourceListView.DataBind();
            }
        }

        /// <summary>
        /// Initializes controls with the data.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            ResourceListView.Title = Title;

            switch (ShowReportType)
            {
                case ReportType.RecentlyAddedResources:
                    UpdateRecentlyAddedResourceView();
                    break;
                case ReportType.RecentlyModifiedResources:
                    UpdateRecentlyModifiedResourceView();
                    break;
                case ReportType.MostActiveAuthors:
                    UpdateMostActiveAuthorsView();
                    break;
            }

            ResourceListView.DataBind();

            base.OnPreRender(e);
        }

        /// <summary>
        /// Get user permissions for the specified list of resources and their related resources.
        /// </summary>
        /// <param name="token">Authentication token.</param>
        /// <param name="resources">List or resources.</param>
        /// <returns>Mapping resource id and user permissions on the resource.</returns>
        private Dictionary<Guid, IEnumerable<string>> GetPermissions(AuthenticatedToken token, IList<Resource> resources)
        {
            Dictionary<Guid, IEnumerable<string>> userPermissions = null;

            if (resources != null && token != null && resources.Count > 0)
            {
                IList<Resource> srcResources = resources.ToList();
                foreach (Resource res in resources)
                {
                    //add file resources to source list
                    if (res.Files != null && res.Files.Count > 0)
                    {
                        srcResources = srcResources.Union(res.Files.Select(tuple => tuple as Resource).ToList()).ToList();
                    }

                    //add author resources to source list
                    ScholarlyWork scholWork = res as ScholarlyWork;
                    if (scholWork != null && scholWork.Authors != null && scholWork.Authors.Count > 0)
                    {
                        srcResources = srcResources.Union(scholWork.Authors.Select(tuple => tuple as Resource).ToList()).ToList();
                    }
                }

                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    //Get user permission for all resources in the source list.
                    var permissons = dataAccess.GetResourcePermissions(token, srcResources);

                    if (permissons != null)
                    {
                        userPermissions = permissons.ToDictionary(tuple => tuple.Resource.Id, tuple => tuple.Permissions);
                    }
                }
            }

            //This is a default case which indicates that user is not having any permission.
            if (userPermissions == null)
                userPermissions = new Dictionary<Guid, IEnumerable<string>>(); ;

            return userPermissions;
        }

        private void UpdateRecentlyAddedResourceView()
        {
            IList<Resource> dataSource = null;
            Dictionary<Guid, IEnumerable<string>> userPermissions = null;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                if (!IsSecurityAwareControl)
                {
                    dataSource = dataAccess.GetLatestAddedResources(null, PageSize);
                }
                else
                {
                    if (this.AuthenticatedToken != null)
                    {
                        dataSource = dataAccess.GetLatestAddedResources(this.AuthenticatedToken, this.PageSize);
                        userPermissions = GetPermissions(this.AuthenticatedToken, dataSource);
                    }
                }
            }

            ResourceListView.DataSource.Clear();
            foreach (Resource resource in dataSource)
                ResourceListView.DataSource.Add(resource);

            ResourceListView.UserPermissions = userPermissions;
            ResourceListView.SortDirection = SortDirection.Descending;
            ResourceListView.SortExpression = _dateAddedProperty;
            ResourceListView.ShowDate = DateType.DateAdded;
            ResourceListView.SortDataSource(SortDirection.Descending, _dateAddedProperty);
            ResourceListView.DataBind();
        }

        private void UpdateRecentlyModifiedResourceView()
        {
            IList<Resource> dataSource = null;

            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                if (!IsSecurityAwareControl)
                {
                    dataSource = dataAccess.GetLatestModifiedResources(null, PageSize);
                }
                else
                {
                    if (this.AuthenticatedToken != null)
                    {
                        dataSource = dataAccess.GetLatestModifiedResources(this.AuthenticatedToken, this.PageSize);
                        GetPermissions(this.AuthenticatedToken, dataSource);
                    }
                }
            }
            ResourceListView.DataSource.Clear();
            foreach (Resource resource in dataSource)
                ResourceListView.DataSource.Add(resource);

            ResourceListView.SortDirection = SortDirection.Descending;
            ResourceListView.SortExpression = _dateModifiedProperty;
            ResourceListView.ShowDate = DateType.DateModified;
            ResourceListView.SortDataSource(SortDirection.Descending, _dateModifiedProperty);
            ResourceListView.DataBind();

        }

        private void UpdateMostActiveAuthorsView()
        {
            IList<Resource> dataSource = null;

            if (!IsSecurityAwareControl)
            {
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    dataSource = dataAccess.GetTopAuthors(null, PageSize).Select(tuple => tuple as Resource).ToList();
                }
            }
            else
            {
                if (this.AuthenticatedToken != null)
                {
                    using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                    {
                        dataSource = dataAccess.GetTopAuthors(this.AuthenticatedToken, PageSize).Select(tuple => tuple as Resource).ToList();
                    }
                }
            }
            ResourceListView.DataSource.Clear();
            foreach (Resource resource in dataSource)
                ResourceListView.DataSource.Add(resource);

            ResourceListView.DataBind();
        }

        #endregion
    }

    /// <summary>
    /// Provides list of report types.
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Recently added resources.
        /// </summary>
        RecentlyAddedResources,
        /// <summary>
        /// Recently modified resources.
        /// </summary>
        RecentlyModifiedResources,
        /// <summary>
        /// Most active authors.
        /// </summary>
        MostActiveAuthors
    }
}
