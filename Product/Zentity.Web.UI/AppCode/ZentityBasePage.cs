// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Zentity.Web.UI;
using Zentity.Web.UI.ToolKit;
using System.Globalization;

/// <summary>
/// Summary description for BasePage
/// </summary>
public class ZentityBasePage : Page
{
    #region Constants
    const string _errorPagePath = "~/Error.aspx?Error={0}";
    static string _errorPagePathWithUrl = "~/Error.aspx?" + Constants.ErrorQueryParam + "={0}&" + Constants.URL + "={1}";
    #endregion Constants

    #region Constructor

    /// <summary>
    /// Initializes the page.
    /// </summary>
    public ZentityBasePage()
    {
        this.Error += new EventHandler(ZentityBasePage_Error);
    }

    #endregion Constructor

    #region Properties

    public override string StyleSheetTheme
    {
        get
        {
            return Utility.GetStyleSheetTheme(base.StyleSheetTheme);
        }
    }

    #endregion Properties

    #region Event Handlers

    /// <summary>
    /// Handles unhandled exceptions.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    void ZentityBasePage_Error(object sender, EventArgs e)
    {
        Exception exception = Server.GetLastError();

        if (exception != null)
        {
            EntityNotFoundException exceptionType = exception as EntityNotFoundException;
            if (exceptionType != null)
            {
                switch (exceptionType.EntityType)
                {
                    case EntityType.Resource:
                        Response.Redirect(string.Format(CultureInfo.InvariantCulture,
                            _errorPagePath, Constants.ResourceNotFoundQueryParam));
                        break;
                    case EntityType.Tag:
                        Response.Redirect(string.Format(CultureInfo.InvariantCulture,
                            _errorPagePath, Constants.TagNotFoundQueryParam));
                        break;
                    case EntityType.Category:
                        Response.Redirect(string.Format(CultureInfo.InvariantCulture,
                            _errorPagePath, Constants.CategoryNotFoundQueryParam));
                        break;
                }
                Server.ClearError();
            }
            else if (exception is ConcurrentAccessException)
            {
                Response.Redirect(string.Format(CultureInfo.InvariantCulture,
                    _errorPagePath, Constants.ConcurrentAccessQueryParam));
                Server.ClearError();
            }
            else if (exception is TimeoutException)
            {
                Response.Redirect(string.Format(CultureInfo.InvariantCulture,
                    _errorPagePath, Constants.TimeoutQueryParam));
                Server.ClearError();
            }
            else if (exception is FormatException)
            {
                Response.Redirect(string.Format(CultureInfo.InvariantCulture,
                    _errorPagePath, Constants.FormatExceptionQueryParam));
                Server.ClearError();
            }
            else if (exception is UnauthorizedAccessException)
            {
                Session.Add(Constants.ExceptionMessageForCustomError, exception.Message);
                Response.Redirect(string.Format(CultureInfo.InvariantCulture,
                    _errorPagePathWithUrl, Constants.UnauthorizedQueryParam, HttpUtility.UrlEncode(Request.RawUrl)));
                Server.ClearError();
            }
        }
    }

    #endregion Event Handlers
}