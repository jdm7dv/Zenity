// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Code behind of Default.master
    /// </summary>
    public partial class Redirect : System.Web.UI.Page
    {
        /// <summary>
        /// Holds the identifier of the resource
        /// </summary>
        private string resourceId = string.Empty;
        
        /// <summary>
        /// Holds the ResourceType of the resource
        /// </summary>
        private string resourceType = string.Empty;
        
        /// <summary>
        /// Holds the Pivot collection Uri
        /// </summary>
        private string collectionUri = string.Empty;
        
        /// <summary>
        /// Flag representing redirection to Zentity Web UI
        /// </summary>
        private string webUI = string.Empty;
        
        /// <summary>
        /// Flag representing redirection to PivotViewer
        /// </summary>
        private string webPivot = string.Empty;

        /// <summary>
        /// Page_Load event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Page_Load</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.ReadQueryString())
            {
                string url = string.Empty;
                if (!string.IsNullOrWhiteSpace(this.webUI))
                {
                    url = string.Format(
                                    "{0}/ResourceManagement/ManageResource.aspx?ActiveTab=Summary&Id={1}",
                                    ConfigurationManager.AppSettings[ConfigurationKeys.WebUIUri],
                                    this.resourceId);
                }
                else if (!string.IsNullOrWhiteSpace(this.webPivot))
                {
                    url = string.Format(
                                "~/Pivot/Viewer.aspx?{0}={1}&{2}={3}",
                                QueryStringKeys.ResourceId, 
                                this.resourceId,
                                QueryStringKeys.CollectionUri, 
                                Request.RequestContext.HttpContext.Server.UrlEncode(this.collectionUri));
                }

                if (!string.IsNullOrWhiteSpace(url))
                {
                    Response.Redirect(url, true);
                }
            }
            else
            {
                this.lblError.Visible = true;
            }
        }

        /// <summary>
        /// Helper method to initialize by reading querystring values
        /// </summary>
        /// <returns>true for a valid collection uri else false</returns>
        private bool ReadQueryString()
        {            
            if (!string.IsNullOrWhiteSpace(Request.QueryString[QueryStringKeys.WebUI]) ||
                !string.IsNullOrWhiteSpace(Request.QueryString[QueryStringKeys.WebPivot]))
            {
                this.webUI = Request.QueryString[QueryStringKeys.WebUI];
                this.webPivot = Request.QueryString[QueryStringKeys.WebPivot];
                if (!string.IsNullOrWhiteSpace(Request.QueryString[QueryStringKeys.ResourceId]))
                {
                    this.resourceId = Request.QueryString[QueryStringKeys.ResourceId];
                    if (!string.IsNullOrWhiteSpace(Request.QueryString[QueryStringKeys.ResourceType]))
                    {
                        this.resourceType = Request.QueryString[QueryStringKeys.ResourceType];
                        if (this.resourceType.Contains("."))
                        {
                            var dataModel = this.resourceType.Remove(this.resourceType.LastIndexOf('.'));
                            this.resourceType = this.resourceType.Replace(dataModel + ".", string.Empty);
                        }

                        if (!string.IsNullOrWhiteSpace(this.resourceType))
                        {
                            this.collectionUri = string.Format(
                                                    "{0}://{1}/VisualExplorer/{2}/{3}/{4}.cxml?Id=EQ.{5}",
                                                    Request.Url.Scheme,
                                                    Request.Url.Authority,
                                                    ConfigurationManager.AppSettings[ConfigurationKeys.PathPrefix],
                                                    Request.QueryString[QueryStringKeys.ResourceType],
                                                    this.resourceType,
                                                    this.resourceId);

                            return true;
                        }
                    }
                }            
            }

            return false;
        }
    }

    /// <summary>
    /// Contains all QueryString keys used
    /// </summary>
    public class QueryStringKeys
    {
        /// <summary>
        /// Denotes redirection to be done to Zentity WebUI
        /// </summary>
        public const string WebUI = "WebUI";
        
        /// <summary>
        /// Denotes redirection to be done to Pivot Viewer
        /// </summary>
        public const string WebPivot = "Pivot";
        
        /// <summary>
        /// Identifier of the resource
        /// </summary>
        public const string ResourceId = "Id";
        
        /// <summary>
        /// ResourceType of the resource
        /// </summary>
        public const string ResourceType = "Type";
        
        /// <summary>
        /// Collection Uri QueryString key
        /// </summary>
        public const string CollectionUri = "Uri";
    }

    /// <summary>
    /// Contains keys of all configurations
    /// </summary>
    public class ConfigurationKeys
    {
        /// <summary>
        /// Key to the Zentity Web UI Uri setting
        /// </summary>
        public const string WebUIUri = "ZentityWebUIUri";
        
        /// <summary>
        /// Key to the virtual directory name for Pivot collections
        /// </summary>
        public const string PathPrefix = "PathPrefix";
        
        /// <summary>
        /// Key to the physical path of Pivot collection
        /// </summary>
        public const string CollectionFilePath = "CollectionFilePath";
        
        /// <summary>
        /// Key to the Zentity Dataservice Uri setting
        /// </summary>
        public const string DataServiceUri = "ZentityDataServiceUri";
        
        /// <summary>
        /// Key to the ODataToCxmlTranslator Uri setting
        /// </summary>
        public const string ODataToCXmlTranslatorUri = "ODataToCXmlTranslatorUri";
    }
}