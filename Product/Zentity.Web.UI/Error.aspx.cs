// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Web.UI.WebControls;
using Zentity.Web.UI;
using Zentity.Web.UI.ToolKit;
using System.Web.UI;

public partial class Error : Page
{
    #region Properties

    public override String StyleSheetTheme
    {
        get
        {
            return Utility.GetStyleSheetTheme(base.StyleSheetTheme);
        }
    }

    #endregion Properties

    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        LocalizePage();
    }

    #endregion

    #region Methods

    private void LocalizePage()
    {
        if (Request.QueryString[Constants.ErrorQueryParam] != null)
        {
            string messageIdentifier = Request.QueryString[Constants.ErrorQueryParam].ToString();

            switch (messageIdentifier)
            {
                case Constants.ResourceNotFoundQueryParam:
                    MessageText.Text = Resources.Resources.ResourceNotFound;
                    break;
                case Constants.TagNotFoundQueryParam:
                    MessageText.Text = Resources.Resources.TagNotFound;
                    break;
                case Constants.CategoryNotFoundQueryParam:
                    MessageText.Text = Resources.Resources.CategoryNotFound;
                    break;
                case Constants.ConcurrentAccessQueryParam:
                    MessageText.Text = Resources.Resources.ConcurrentAccess;
                    break;
                case Constants.PageNotFoundQueryParam:
                    MessageText.Text = Resources.Resources.PageNotFoundMessage;
                    break;
                case Constants.TimeoutQueryParam:
                    MessageText.Text = Resources.Resources.TimeoutMessage;
                    break;
                case Constants.FormatExceptionQueryParam:
                    MessageText.Text = Resources.Resources.InvalidFormatMessage;
                    break;
                case Constants.UnauthorizedQueryParam:
                    {
                        if (Session[Constants.ExceptionMessageForCustomError] != null)
                        {
                            MessageText.Text = Session[Constants.ExceptionMessageForCustomError].ToString();
                            Session.Remove(Constants.ExceptionMessageForCustomError);
                        }
                        else
                        {
                            MessageText.Text = Resources.Resources.UnauthorizedAccessException;
                        }
                    }
                    break;
                default:
                    MessageText.Text = Resources.Resources.ErrorMessage;
                    break;
            }
        }
        else
        {
            MessageText.Text = Resources.Resources.ErrorMessage;
        }
    }

    #endregion
}