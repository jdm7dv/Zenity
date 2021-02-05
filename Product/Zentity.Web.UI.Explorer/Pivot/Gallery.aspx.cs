// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.Pivot
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web.UI.WebControls;

    /// <summary>
    /// Code behind class of Gallery.aspx
    /// </summary>
    public partial class Gallery : System.Web.UI.Page
    {
        /// <summary>
        /// Holds the Virtual Directory path configured for Collections output folder
        /// </summary>
        private static string pathPrefix = ConfigurationManager.AppSettings[ConfigurationKeys.PathPrefix];

        /// <summary>
        /// Get all published Pivot collection 
        /// </summary>
        /// <returns>returns list of PublishingCollectionItem</returns>
        public IEnumerable<PublishingCollectionItem> GetAllPivotCollectionItems()
        {
            try
            {
                PublishingCollectionItems pivotCollectionItems = PivotCollectionHelper.GetAllPivotCollectionItemsForPivot(this.Context.Request);
                Session[Properties.Messages.PivotCollectionItemsForSorting] = pivotCollectionItems;
                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                ViewState.Add(Properties.Messages.SortingColumn, "ResourceType");
                return pivotCollectionItems.OrderBy(p => p.ResourceType);
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                this.errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
                return null;
            }
            catch (System.TimeoutException)
            {
                this.errorMessageSpan.InnerText = Properties.Messages.ConnectionTimedOut;
                this.errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
                return null;
            }
            catch (System.ServiceModel.FaultException exception)
            {
                Zentity.Services.Web.Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, exception.ToString(), exception.Message);
                this.errorMessageSpan.InnerText = Properties.Messages.ZentityServerAccessError;
                this.errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
                return null;
            }
            catch (System.Exception exception)
            {
                Zentity.Services.Web.Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, exception.ToString(), exception.Message);
                this.errorMessageSpan.InnerText = Properties.Messages.ZentityServerAccessError;
                this.errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
                return null;
            }
        }

        /// <summary>
        /// Overridden OnInit method
        /// </summary>
        /// <param name="e">Event data for OnInit event</param>
        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
            base.OnInit(e);
        }

        /// <summary>
        /// Overridden Page_Load method
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Page_Load event</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            collectionsGrid.Sorting += new System.Web.UI.WebControls.GridViewSortEventHandler(this.CollectionsGrid_Sorting);
            collectionsGrid.RowDataBound += new System.Web.UI.WebControls.GridViewRowEventHandler(this.CollectionsGrid_RowDataBound);

            if (!IsPostBack)
            {
                try
                {
                    this.collectionsGrid.DataSource = this.GetAllPivotCollectionItems();
                    this.collectionsGrid.DataBind();
                }
                catch (System.ServiceModel.EndpointNotFoundException)
                {
                    this.errorSummaryContainer.Visible = true;
                    this.collectionsGrid.Visible = false;
                }
                catch (System.TimeoutException)
                {
                    this.errorMessageSpan.InnerText = Properties.Messages.ConnectionTimedOut;
                    this.collectionsGrid.Visible = false;
                    this.errorSummaryContainer.Visible = true;
                }
                catch (System.ServiceModel.FaultException exception)
                {
                    Zentity.Services.Web.Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, exception.ToString(), exception.Message);
                    this.errorMessageSpan.InnerText = Properties.Messages.ZentityServerAccessError;
                    this.collectionsGrid.Visible = false;
                    this.errorSummaryContainer.Visible = true;
                }
                catch (System.Exception exception)
                {
                    Zentity.Services.Web.Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, exception.ToString(), exception.Message);
                    this.errorMessageSpan.InnerText = Properties.Messages.ZentityServerAccessError;
                    this.errorSummaryContainer.Visible = true;
                    this.collectionsGrid.Visible = false;
                }
            }
        }

        /// <summary>
        /// RowDataBound event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for RowDataBound event</param>
        private void CollectionsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.Footer)
            {
                var itemCollection = collectionsGrid.DataSource as IEnumerable<PublishingCollectionItem>;
                if (itemCollection != null)
                {
                    PublishingCollectionItem pubItem = itemCollection.FirstOrDefault();
                    if (pubItem != default(PublishingCollectionItem))
                    {
                        e.Row.Cells[2].Text = pubItem.TotalNoOfResources.ToString();
                        int noOfElementsCount = 0;
                        itemCollection.ToList().ForEach(p => noOfElementsCount = noOfElementsCount + p.TotalNoOfElements);
                        e.Row.Cells[4].Text = noOfElementsCount.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Sorting Event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Sorting event</param>
        private void CollectionsGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                IEnumerable<PublishingCollectionItem> pivotCollectionItems = (IEnumerable<PublishingCollectionItem>)Session[Properties.Messages.PivotCollectionItemsForSorting];
                if (pivotCollectionItems == null || pivotCollectionItems.Count() == 0)
                {
                    pivotCollectionItems = this.GetAllPivotCollectionItems();
                }

                if (pivotCollectionItems != null)
                {
                    //// Sorting Algorithm
                    switch (e.SortExpression)
                    {
                        case "DataModel":
                            if (ViewState[Properties.Messages.SortingColumn].ToString() == e.SortExpression)
                            {
                                if (ViewState[Properties.Messages.SortingDirection].ToString() == Properties.Messages.Ascending)
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderByDescending(p => p.DataModel);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Descending);
                                }
                                else
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.DataModel);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                                }
                            }
                            else
                            {
                                pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.DataModel);
                                ViewState.Add(Properties.Messages.SortingColumn, "DataModel");
                                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                            }

                            break;

                        case "ResourceType":
                            if (ViewState[Properties.Messages.SortingColumn].ToString() == e.SortExpression)
                            {
                                if (ViewState[Properties.Messages.SortingDirection].ToString() == Properties.Messages.Ascending)
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderByDescending(p => p.ResourceType);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Descending);
                                }
                                else
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.ResourceType);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                                }
                            }
                            else
                            {
                                pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.ResourceType);
                                ViewState.Add(Properties.Messages.SortingColumn, "ResourceType");
                                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                            }

                            break;

                        case "Collection":
                            if (ViewState[Properties.Messages.SortingColumn].ToString() == e.SortExpression)
                            {
                                if (ViewState[Properties.Messages.SortingDirection].ToString() == Properties.Messages.Ascending)
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderByDescending(p => p.Collection.First().Name);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Descending);
                                }
                                else
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.Collection.First().Name);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                                }
                            }
                            else
                            {
                                pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.Collection.First().Name);
                                ViewState.Add(Properties.Messages.SortingColumn, "Collection");
                                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                            }

                            break;

                        case "NumberofResources":
                            if (ViewState[Properties.Messages.SortingColumn].ToString() == e.SortExpression)
                            {
                                if (ViewState[Properties.Messages.SortingDirection].ToString() == Properties.Messages.Ascending)
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderByDescending(p => p.NumberOfResources);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Descending);
                                }
                                else
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.NumberOfResources);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                                }
                            }
                            else
                            {
                                pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.NumberOfResources);
                                ViewState.Add(Properties.Messages.SortingColumn, "NumberofResources");
                                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                            }

                            break;

                        case "NumberOfElements":
                            if (ViewState[Properties.Messages.SortingColumn].ToString() == e.SortExpression)
                            {
                                if (ViewState[Properties.Messages.SortingDirection].ToString() == Properties.Messages.Ascending)
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderByDescending(p => p.Collection.First().NumberOfElements);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Descending);
                                }
                                else
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.Collection.First().NumberOfElements);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                                }
                            }
                            else
                            {
                                pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.Collection.First().NumberOfElements);
                                ViewState.Add(Properties.Messages.SortingColumn, "NumberOfElements");
                                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                            }

                            break;

                        case "Status":
                            if (ViewState[Properties.Messages.SortingColumn].ToString() == e.SortExpression)
                            {
                                if (ViewState[Properties.Messages.SortingDirection].ToString() == Properties.Messages.Ascending)
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderByDescending(p => p.Status).ThenBy(p => p.ResourceType);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Descending);
                                }
                                else
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.Status);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                                }
                            }
                            else
                            {
                                pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.Status);
                                ViewState.Add(Properties.Messages.SortingColumn, "Status");
                                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                            }

                            break;

                        case "LastUpdated":
                            if (ViewState[Properties.Messages.SortingColumn].ToString() == e.SortExpression)
                            {
                                if (ViewState[Properties.Messages.SortingDirection].ToString() == Properties.Messages.Ascending)
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderByDescending(p => p.LastUpdated);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Descending);
                                }
                                else
                                {
                                    pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.LastUpdated);
                                    ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                                }
                            }
                            else
                            {
                                pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.LastUpdated);
                                ViewState.Add(Properties.Messages.SortingColumn, "LastUpdated");
                                ViewState.Add(Properties.Messages.SortingDirection, Properties.Messages.Ascending);
                            }

                            break;

                        default:
                            pivotCollectionItems = pivotCollectionItems.OrderBy(p => p.ResourceType);
                            break;
                    }
                }
                else
                {
                    this.errorSummaryContainer.Visible = true;
                }

                this.collectionsGrid.DataSource = pivotCollectionItems;
                this.collectionsGrid.DataBind();
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                this.errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
            }
            catch (System.TimeoutException)
            {
                this.errorMessageSpan.InnerText = Properties.Messages.ConnectionTimedOut;
                this.errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
            }
            catch (System.ServiceModel.FaultException exception)
            {
                Zentity.Services.Web.Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, exception.ToString(), exception.Message);
                this.errorMessageSpan.InnerText = Properties.Messages.ZentityServerAccessError;
                errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
            }
            catch (System.Exception exception)
            {
                Zentity.Services.Web.Globals.TraceMessage(System.Diagnostics.TraceEventType.Error, exception.ToString(), exception.Message);
                this.errorMessageSpan.InnerText = Properties.Messages.ZentityServerAccessError;
                this.errorSummaryContainer.Visible = true;
                this.collectionsGrid.Visible = false;
            }
        }
    }
}