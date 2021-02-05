// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Zentity.Platform;
using Zentity.Web.UI;
using System.Globalization;
using Zentity.Security.Authorization;

namespace Zentity.Web.UI
{
    /// <summary>
    /// This class contains common methods
    /// </summary>
    public static class Utility
    {
        #region Constants
        private const string _2yearFormatS = "yy";
        private const string _2yearFormatC = "YY";
        private const string _yearChar = "y";
        public const string DefaultPageSize = "10";
        #endregion

        #region Properties

        // Set Connection time out for BIG DB
        private static int timeoutInterval;
        public static int ConnectionTimeout
        {
            get
            {
                if (timeoutInterval <= 0)
                {
                    string actualConnectionString = string.Empty;
                    ConnectionStringSettingsCollection collection = ConfigurationManager.ConnectionStrings;
                    foreach (ConnectionStringSettings connection in collection)
                    {
                        if ((!connection.Name.Equals(Resources.Resources.ZentityLocalSqlServer, StringComparison.Ordinal)) && connection.ProviderName.Equals(Resources.Resources.EntityClientProvider, StringComparison.Ordinal))
                        {
                            actualConnectionString = connection.ConnectionString.Split(new string[] { Resources.Resources.ConnectionStringProvider }, StringSplitOptions.RemoveEmptyEntries).Last();
                            actualConnectionString = actualConnectionString.Trim('\'').Trim('\"').Trim();
                            break;
                        }
                    }

                    bool timeoutSpecified = false;
                    if (actualConnectionString.Contains(Resources.Resources.ConnectionStringTimeout) ||
                        actualConnectionString.Contains(Resources.Resources.ConnectionStringConnectTimeout))
                    {
                        timeoutSpecified = true;
                    }

                    using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(actualConnectionString))
                    {
                        timeoutInterval = connection.ConnectionTimeout;
                        if (!timeoutSpecified && timeoutInterval <= 300)
                        {
                            timeoutInterval = 300;
                        }
                    }
                }
                return timeoutInterval;
            }
        }

        #endregion

        #region Methods

        #region Public

        public static List<string> GetPageIndexRange()
        {
            List<string> pageIndex = new List<string>(Resources.Resources.PageIndexRange.Split(','));
            return pageIndex;
        }

        /// <summary>
        /// Create space and add it to control.
        /// </summary>
        public static Literal CreateSpace()
        {
            Literal litSpace = new Literal();
            litSpace.Text = Constants.HtmlSpace;
            return litSpace;
        }

        /// <summary>
        /// Apply style on the row on basis of existing rows in table.
        /// </summary>
        /// <param name="tableRow"> Table row to be applied style </param>
        /// <param name="count">Number of rows exist in table</param>
        public static void ApplyRowStyle(TableRow tableRow, int count)
        {
            if (count != 0 && (count % 2) == 0)
            {
                tableRow.Attributes.Add(Constants.CssStyleClass, Constants.AlternateRowStyle);
            }
            else
            {
                tableRow.Attributes.Add(Constants.CssStyleClass, Constants.RowStyle);
            }
        }

        /// <summary>
        /// Creates table cell.
        /// </summary>
        /// <returns></returns>
        public static TableCell CreateTableCell()
        {
            TableCell tableCell = new TableCell();
            tableCell.BorderStyle = System.Web.UI.WebControls.BorderStyle.Inset;
            tableCell.BorderWidth = System.Web.UI.WebControls.Unit.Pixel(1);
            return tableCell;
        }

        /// <summary>
        /// Create hyperlink with providing inputs
        /// </summary>
        /// <param name="linkText">Text to be displayed on link</param>
        /// <param name="navigateUrl">Navigate Url for link</param>
        /// <returns>Instance of HyperLink</returns>
        public static HyperLink CreateHyperLink(string linkText, string navigateUrl)
        {
            HyperLink hyperLink = new HyperLink();
            hyperLink.NavigateUrl = navigateUrl;

            // Encode Html tag into string and assign to hyper link
            hyperLink.Text = HttpUtility.HtmlEncode(linkText); ;
            return hyperLink;
        }

        /// <summary>
        ///     Returns current date format.
        /// </summary>
        /// <returns>current date format</returns>
        public static string GetDateFormat()
        {
            CultureInfo current = CultureInfo.CurrentCulture;
            string _dateTimeFormat = current.DateTimeFormat.ShortDatePattern;

            //If year format is yy or YY then replave it with yyyy or YYYY respectively.
            int startIndex = _dateTimeFormat.IndexOf(_yearChar, StringComparison.InvariantCultureIgnoreCase);
            int endIndex = _dateTimeFormat.LastIndexOf(_yearChar, StringComparison.InvariantCultureIgnoreCase);
            if ((endIndex - startIndex) == 1)
            {
                _dateTimeFormat = _dateTimeFormat.Replace(_2yearFormatS, _2yearFormatS + _2yearFormatS);
                _dateTimeFormat = _dateTimeFormat.Replace(_2yearFormatC, _2yearFormatC + _2yearFormatC);
            }
            return _dateTimeFormat;
        }

        /// <summary>
        ///     Return string with end with "..." if string is big.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximumChars"></param>
        /// <returns></returns>
        public static string FitString(string value, int maximumChars)
        {
            if (maximumChars < 1)
            {
                return string.Empty;
            }
            string stringToCompress = value;
            if (stringToCompress.Length > maximumChars)
            {
                string strResult = stringToCompress.Substring(0, maximumChars);
                strResult += "...";
                return (strResult);
            }
            else
            {
                return value;
            }
        }

        public static void InitializeDropDownList(ListControl dropDown, object dataSource, string textField, string valueField)
        {
            if (dropDown != null)
            {
                dropDown.DataSource = dataSource;

                if (!string.IsNullOrEmpty(valueField))
                {
                    dropDown.DataValueField = valueField;
                }

                if (!string.IsNullOrEmpty(textField))
                {
                    dropDown.DataTextField = textField;
                }

                dropDown.DataBind();

                ListItemCollection items = dropDown.Items;
                foreach (ListItem item in items)
                {
                    item.Attributes.Add(Resources.Resources.DropDownItemTitle, item.Text);
                }
            }
        }

        /// <summary>
        /// method to fill PageSizeDropDown
        /// </summary>
        public static void FillPageSizeDropDownList(ListControl pageSizeDropDownList)
        {
            Utility.InitializeDropDownList(pageSizeDropDownList, Utility.GetPageIndexRange(), null, null);
            pageSizeDropDownList.SelectedValue = DefaultPageSize;
        }

        public static string GetLinkTag(string navigationUrl, string displayText)
        {
            StringBuilder linkTag = new StringBuilder();
            linkTag.Append("<a href=");
            linkTag.Append("\"");
            linkTag.Append(navigationUrl);
            linkTag.Append("\">");
            linkTag.Append(HttpUtility.HtmlEncode(displayText));
            linkTag.Append("</a>");

            return linkTag.ToString();
        }

        /// <summary>
        /// Updates Resource's title with default value if title is null or empty string.
        /// </summary>
        /// <param name="resources">Collection of resource.</param>
        public static void UpdateResourcesEmptyTitle<T>(IEnumerable<T> resources) where T : Resource
        {
            IEnumerable<T> emptyTitleResources = resources.Where(res => res.Title == null || res.Title.Trim() == string.Empty).AsEnumerable();

            foreach (T res in emptyTitleResources)
            {
                res.Title = Resources.Resources.ResourceEmptyTitle;
            }
        }

        /// <summary>
        /// Updates provided title with default value if title contains null or empty string.
        /// </summary>
        /// <param name="resources">Resource's title.</param>
        public static string UpdateEmptyTitle(string title)
        {
            return title == null || title.Trim() == string.Empty ? Resources.Resources.ResourceEmptyTitle : title;
        }

        /// <summary>
        /// Set default file name if title contains null or empty string.
        /// </summary>
        /// <param name="resources">Resource's title.</param>
        public static string GetFileName(string title)
        {
            return title == null || title.Trim() == string.Empty ? Resources.Resources.DefaultFilelName : title;
        }

        public static ZentityContext CreateContext()
        {
            ZentityContext context = new ZentityContext();
            context.CommandTimeout = Utility.ConnectionTimeout;
            LoadMetadata(context);
            return context;
        }

        public static void LoadMetadata(ZentityContext context)
        {
            context.MetadataWorkspace.LoadFromAssembly(System.Reflection.Assembly.GetAssembly
              (typeof(ScholarlyWorkItem)));
            context.MetadataWorkspace.LoadFromAssembly(System.Reflection.Assembly.GetAssembly
              (typeof(Identity)));
        }

        public static TableCell NewCreateTableCell(Unit width)
        {
            TableCell tableCell = new TableCell();
            tableCell.Width = width;
            tableCell.HorizontalAlign = HorizontalAlign.Left;
            return tableCell;
        }

        public static void ShowMessage(Label errorLabel, string message, bool isError)
        {
            if (isError)
                errorLabel.ForeColor = System.Drawing.Color.Red;
            else
                errorLabel.ForeColor = System.Drawing.Color.Black;

            if (!string.IsNullOrEmpty(message))
            {
                message = "<br />" + message + "<br /><br />";
                errorLabel.Visible = true;
            }
            else
            {
                errorLabel.Visible = false;
            }
            errorLabel.Text = message;
        }

        public static string GetStyleSheetTheme(string currentValue)
        {
            if (HttpContext.Current.Session[Constants.SessionThemeName] == null)
            {
                HttpContext.Current.Session.Add(Constants.SessionThemeName, currentValue);
                return currentValue;
            }
            else
            {
                return ((string)HttpContext.Current.Session[Constants.SessionThemeName]);
            }
        }

        #endregion

        #endregion
    }
}