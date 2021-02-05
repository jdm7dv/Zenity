// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System.Web;
using System.Web.SessionState;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    ///     Handler for create binary response for image.
    /// </summary>
    public class ImageHandler : IHttpHandler, IRequiresSessionState
    {
        #region Member variables

        #region Constatnt

        private string _thumbnailImage = "ThumbnailImage";

        #endregion

        #endregion

        #region Methods

        #region Public
        /// <summary>
        /// Gets a value indicating whether another request can use the IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Creates a binary response for image.
        /// </summary>
        /// <param name="context">An object of <see cref="System.Web.HttpContext"/>.</param>
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session[_thumbnailImage] != null)
            {
                byte[] imgData = (byte[])context.Session[_thumbnailImage];
                context.Response.BinaryWrite(imgData);
                context.Session[_thumbnailImage] = null;
            }
        }
        #endregion

        #endregion

    }
}
