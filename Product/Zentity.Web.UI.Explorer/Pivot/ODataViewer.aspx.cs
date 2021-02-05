// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Web.UI.Explorer.Pivot
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Code behind for ODataViewer.aspx
    /// </summary>
    public partial class ODataViewer : System.Web.UI.Page
    {
        /// <summary>
        /// Page_Load event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Page_Load event</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!string.IsNullOrWhiteSpace(Request.QueryString[QueryStringKeys.CollectionUri]))
                {
                    string uri = Request[QueryStringKeys.CollectionUri];
                    int index = uri.IndexOf("?src=", StringComparison.CurrentCultureIgnoreCase);
                    if (index > 0)
                    {
                        uri = uri.Substring(index + 5);
                    }

                    this.txtODataUrl.Text = uri;
                }
                else
                {
                    this.silverlightControlHost.Visible = false;
                    this.txtODataUrl.Text = ConfigurationManager.AppSettings[ConfigurationKeys.DataServiceUri];
                }
            }
        }

        /// <summary>
        /// Sumbit button Click event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Click event</param>
        protected void OnBtnSubmitClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.txtODataUrl.Text))
            {
                string uri = string.Format(
                        "{0}://{1}/{2}?src={3}",
                        Request.Url.Scheme,
                        Request.Url.Authority,
                        ConfigurationManager.AppSettings[ConfigurationKeys.ODataToCXmlTranslatorUri],
                        this.txtODataUrl.Text);
                uri = Server.UrlEncode(uri);
                Response.Redirect(string.Format("ODataViewer.aspx?{0}={1}", QueryStringKeys.CollectionUri, uri), true);
            }
        }
    }
}