// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.Pivot
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Zentity.Services.Web;

    /// <summary>
    /// Code behind of Viewer.aspx
    /// </summary>
    public partial class Viewer : System.Web.UI.Page
    {
        /// <summary>
        /// Page_Load event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Page_Load event</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string encodedcollectionUri = Request.QueryString[QueryStringKeys.CollectionUri];
                string collectionUri = Request.RequestContext.HttpContext.Server.UrlDecode(encodedcollectionUri);
                string resourceId = Request.QueryString[QueryStringKeys.ResourceId];

                string[] collectionUriParts = collectionUri.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);
                var dataset = PivotCollectionHelper.GetAllPivotCollectionItems(Request);
                var dataModel = dataset.SingleOrDefault(tuple => tuple.Name == collectionUriParts[4]);

                if (dataModel == null || dataModel.IsNameNull())
                {
                    throw new ArgumentException(Properties.Messages.CollectionNotFound);
                }
            }
            catch (ThreadAbortException)
            { 
            }
            catch (Exception exception)
            {
                this.MainBody.Visible = false;
                this.MainBody.Disabled = true;
                this.errorSummaryContainer.Visible = true;

                Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
            }
        }
    }
}