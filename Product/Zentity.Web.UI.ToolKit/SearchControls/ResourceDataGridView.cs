// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.ObjectModel;
using Zentity.Core;
using Zentity.Web.UI.ToolKit.Resources;
using System.Globalization;
using System.Collections;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Displays resources in a table where each column represents
    /// a scalar property of Resource class and each row represents a Resource entity.
    /// This control provides facility for view, edit and delete operations.
    /// </summary>
    /// <example>The code below is the source for ResourceDataGridView.aspx. 
    ///     It shows an example of using <see cref="ResourceDataGridView"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register assembly="Zentity.Web.UI.ToolKit" namespace="Zentity.Web.UI.ToolKit" 
    ///             TagPrefix="zentity" %&gt;
    ///         &lt;%@ Import Namespace="System.Collections.Generic" %&gt;
    ///         &lt;%@ Import Namespace="System.Linq" %&gt;
    ///         &lt;%@ Import Namespace="Zentity.Core" %&gt;
    ///         &lt;%@ Import Namespace="Zentity.ScholarlyWorks" %&gt;
    ///         &lt;%@ Assembly Name="System.Data.Entity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head runat="server"&gt;
    ///             &lt;title&gt;ResourceDataGridView Sample&lt;/title&gt;
    ///             
    ///             &lt;script runat="server"&gt;
    ///                 protected void Page_Load(object sender, EventArgs e)
    ///                 {
    ///                     RefreshDataSource();
    ///                 }
    ///                 
    ///                 private void RefreshDataSource()
    ///                 {
    ///                     using (ZentityContext context = new ZentityContext())
    ///                     {
    ///                         context.MetadataWorkspace.LoadFromAssembly(typeof(ScholarlyWork).Assembly);
    ///                         IEnumerable&lt;Resource&gt; resources = context.Resources;
    ///                         int totalRecords = resources.Count();
    ///                         
    ///                         int pageSize = ResourceDataGridView1.PageSize;
    ///                         int pageIndex = ResourceDataGridView1.PageIndex;
    ///                         
    ///                         int pageCount = 0;
    ///                         if (totalRecords != 0 &amp;&amp; pageSize != 0)
    ///                         {
    ///                             pageCount = totalRecords / pageSize;
    ///                         }
    ///                         ResourceDataGridView1.PageCount = pageCount;
    ///                         
    ///                         List&lt;Resource&gt; pagedResources = resources
    ///                             .Skip(pageIndex * pageSize)
    ///                             .Take(pageSize)
    ///                             .ToList();
    ///                             
    ///                         ResourceDataGridView1.DataSource = pagedResources;
    ///                         ResourceDataGridView1.DataBind();
    ///                     }
    ///                 }
    ///              &lt;/script&gt;
    ///              
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///                 &lt;zentity:ResourceDataGridView ID="ResourceDataGridView1" runat="server" AllowSorting="True"
    ///                     EnableDelete="True" ShowCommandColumns="False" AllowPaging="true" ShowFooter="false" 
    ///                     PageSize="10" ViewColumn="Title" ViewUrl="ResourceDetailView.aspx?Id={0}" Width="100%"&gt;
    ///                     &lt;DisplayColumns&gt;
    ///                         &lt;zentity:ZentityGridViewColumn ColumnName="Title" HeaderText="Title"&gt;
    ///                         &lt;/zentity:ZentityGridViewColumn&gt;
    ///                         &lt;zentity:ZentityGridViewColumn ColumnName="DateAdded" HeaderText="Date Added"&gt;
    ///                         &lt;/zentity:zentityGridViewColumn&gt;
    ///                         &lt;zentity:ZentityGridViewColumn ColumnName="DateModified" HeaderText="Date Modified"&gt;
    ///                         &lt;/zentity:ZentityGridViewColumn&gt;
    ///                         &lt;zentity:ZentityGridViewColumn ColumnName="Description" HeaderText="Description"&gt;
    ///                         &lt;/zentity:ZentityGridViewColumn&gt;
    ///                     &lt;/DisplayColumns&gt;
    ///                 &lt;/zentity:ResourceDataGridView&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    public class ResourceDataGridView : ZentityDataGridView
    {
        #region Constants

        #region Private

        private const string _namespaceZentityCore = "Zentity.Core.";
        private const string _defaultResourceType = "Resource";
        private const string _defaultResourceProperty = "Title";
        private const string _entityTypeViewStateKey = "EntityType";
        private const string _viewColumnNameViewStateKey = "ViewColumnName";

        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets entity type.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionZentityGridViewEntityType")]
        [DefaultValue(typeof(string), _defaultResourceType), Browsable(true), Localizable(true)]
        public override string EntityType
        {
            get
            {
                return ViewState[_entityTypeViewStateKey] != null ? ViewState[_entityTypeViewStateKey].ToString() : _defaultResourceType;


            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value", Resources.GlobalResource.MessageEntityTypeNull);
                }
                ViewState[_entityTypeViewStateKey] = value;



                if (this.DataSource != null)
                {
                    this.Columns.Clear();
                    this.DataSource = null;
                    this.DataBind();
                }
            }
        }

        /// <summary>
        /// Gets or sets column name to have a view link.
        /// </summary>
        [ZentityCategory("CategoryAppearance")]
        [ZentityDescription("DescriptionZentityGridViewViewColumnName")]
        [DefaultValue(typeof(string), _defaultResourceProperty), Browsable(true), Localizable(true)]
        public override string ViewColumn
        {
            get
            {
                return ViewState[_viewColumnNameViewStateKey] != null ?
                    ViewState[_viewColumnNameViewStateKey].ToString() : _defaultResourceProperty;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value", Resources.GlobalResource.MessageViewColumnNull);
                }

                ViewState[_viewColumnNameViewStateKey] = value;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Sorts DataSource according to SortDirection and SortExpression.
        /// </summary>
        public override void SortDataSource()
        {
            if (!string.IsNullOrEmpty(this.SortExpression))
            {
                IList resourceList = this.DataSource as IList;
                if (resourceList == null || resourceList.Count == 0)
                {
                    return;
                }
                System.Reflection.PropertyInfo propertyInfo = resourceList[0].GetType().GetProperty(this.SortExpression);
                if (propertyInfo == null)
                {
                    throw new ArgumentException(GlobalResource.ExceptionSortExpressionZentityGridView);
                }
                if (this.SortDirection == SortDirection.Ascending)
                {
                    resourceList = resourceList.OfType<Resource>().OrderBy(
                        zentityType => zentityType.GetType().InvokeMember(
                            this.SortExpression,
                            System.Reflection.BindingFlags.GetProperty, null,
                            zentityType, null, CultureInfo.InvariantCulture)
                        ).ToList();
                }
                else
                {
                    resourceList = resourceList.OfType<Resource>().OrderByDescending(
                        zentityType => zentityType.GetType().InvokeMember(
                            this.SortExpression,
                            System.Reflection.BindingFlags.GetProperty, null,
                            zentityType, null, CultureInfo.InvariantCulture)
                        ).ToList();
                }

                this.DataSource = resourceList;
            }
        }

        #endregion

        #region Protected

        /// <summary>
        /// Adds columns to be displayed in the column collection.
        /// </summary>
        protected override void AddDisplayColumns()
        {
            if (!this.DesignMode)
            {
                Collection<ScalarProperty> propertyList = null;
                using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
                {
                    propertyList = dataAccess.GetScalarProperties(this.Page.Cache, EntityType);
                }

                //Add View Column.
                if (!string.IsNullOrEmpty(ViewColumn))
                {
                    AddViewColumn(propertyList);
                }

                //If user column list is been provided.
                if (DisplayColumns != null && DisplayColumns.Count > 0)
                {
                    AddCustomColumns(propertyList);
                }
                else
                {
                    //Add default columns based on Resource type
                    AddDefaultColumns(propertyList);
                }
            }
            else
            {
                //Add View Column
                base.AddViewColumn(GlobalResource.TitleText, GlobalResource.TitleText,
                  GlobalResource.TitleText);
            }
        }

        /// <summary>
        /// Sets DataNavigateUrlFormatString property of editAssociation column.
        /// </summary>
        /// <param name="editAssociationColumn">HyperLinkField.</param>
        protected override void SetEditAssociationUrlFormat(HyperLinkField editAssociationColumn)
        {
            editAssociationColumn.DataNavigateUrlFormatString = string.Format(CultureInfo.InstalledUICulture, EditAssociationUrl, "{0}",
                CoreHelper.ExtractTypeName(EntityType), _defaultResourceType);
        }

        #endregion

        #region Private

        private void AddViewColumn(Collection<ScalarProperty> propertyList)
        {
            if (!ContainsScalarProperty(ViewColumn, propertyList))
            {
                throw new ArgumentException(Resources.GlobalResource.MessageViewColumnNotFound);
            }

            bool columnAdded = false;
            foreach (ZentityGridViewColumn column in DisplayColumns)
            {
                if (column.ColumnName.Equals(ViewColumn) &&
                    ContainsScalarProperty(column.ColumnName, propertyList))
                {
                    //Add View Column
                    base.AddViewColumn(column.HeaderText, column.ColumnName,
                      column.ColumnName);

                    columnAdded = true;
                    break;
                }
            }

            if (!columnAdded)
            {
                ScalarProperty scalarProperty = GetScalarProperty(ViewColumn, propertyList);

                if (scalarProperty != null)
                {
                    //Add View Column
                    base.AddViewColumn(scalarProperty.Name, scalarProperty.Name,
                      scalarProperty.Name);

                    //Remove column from list
                    propertyList.Remove(scalarProperty);
                }
            }
        }

        private void AddDefaultColumns(Collection<ScalarProperty> propertyList)
        {
            foreach (ScalarProperty scalarProperty in propertyList)
            {
                BoundField field = new BoundField();
                field.HeaderText = scalarProperty.Name;
                field.DataField = scalarProperty.Name;
                field.SortExpression = scalarProperty.Name;
                this.Columns.Add(field);
            }
        }

        private void AddCustomColumns(Collection<ScalarProperty> propertyList)
        {
            foreach (ZentityGridViewColumn column in DisplayColumns)
            {
                if (!column.ColumnName.Equals(ViewColumn) &&
                    ContainsScalarProperty(column.ColumnName, propertyList))
                {
                    BoundField field = new BoundField();
                    field.HeaderText = column.HeaderText;
                    field.DataField = column.ColumnName;
                    field.SortExpression = column.ColumnName;
                    this.Columns.Add(field);
                }
            }
        }

        private static ScalarProperty GetScalarProperty(string propertyName, Collection<ScalarProperty> propertyList)
        {
            ScalarProperty property = null;
            foreach (ScalarProperty currentProperty in propertyList)
            {
                if (currentProperty.Name.Equals(propertyName))
                {
                    property = currentProperty;
                    break;
                }
            }
            return property;
        }

        private static bool ContainsScalarProperty(string propertyName, Collection<ScalarProperty> propertyList)
        {
            bool success = false;
            foreach (ScalarProperty currentProperty in propertyList)
            {
                if (currentProperty.Name.Equals(propertyName))
                {
                    success = true;
                    break;
                }
            }
            return success;
        }

        private ZentityContext CreateContext()
        {
            ZentityContext context = null;
            if (string.IsNullOrEmpty(this.ConnectionString))
            {
                context = new ZentityContext();
            }
            else
            {
                context = new ZentityContext(this.ConnectionString);
            }
            return context;
        }

        #endregion

        #endregion
    }
}
