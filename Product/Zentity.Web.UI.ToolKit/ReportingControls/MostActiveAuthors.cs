// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI.WebControls;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    ///     This class inherits <see cref="ZentityTable"/> class to display a list of most active
    ///     authors.
    /// </summary>
    /// <example>
    ///     The code below is the source for MostActiveAuthors.aspx. 
    ///     It shows an example of using <see cref="MostActiveAuthors"/> control.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit" 
    ///             TagPrefix="Zentity" %&gt;
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///         &lt;head id="Head1" runat="server"&gt;
    ///             &lt;title&gt;MostActiveAuthors Sample&lt;/title&gt;
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="mainForm" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:MostActiveAuthors ID="MostActiveAuthors1" runat="server" TitleDestinationPageUrl="ResourceDetailView.aspx?Id={0}"
    ///                     PageSize="10" EnableViewState="False" BorderWidth="1px" IsSecurityAwareControl="false"&gt;
    ///                 &lt;/Zentity:MostActiveAuthors&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    public class MostActiveAuthors : ZentityTable
    {
        #region Constants

        const string _titleDestinationPageUrlViewStateKey = "TitleDestinationPageUrl";
        const string _titleHeaderViewStateKey = "TitleHeader";
        const string _firstNameHeaderViewStateKey = "FirstNameHeader";
        const string _middleNameHeaderViewStateKey = "MiddleNameHeader";
        const string _lastNameHeaderViewStateKey = "LastNameHeader";
        const string _emailHeaderViewStateKey = "EmailHeader";

        #endregion Constants

        #region Member variables

        #region Private

        private List<Contact> _personList = new List<Contact>();

        #endregion Private

        #endregion Member variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="MostActiveAuthors" /> class.
        /// </summary>
        public MostActiveAuthors()
        {
            this.Title = GlobalResource.TitleMostActiveAuthors;
        }

        #endregion Constructor

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets the Url of the page that the user is shown after clicking the title of the author.
        /// If the Url contains '{0}', it will be replaced by the id of the author.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionMostActiveAuthorsTitleUrl")]
        public string TitleDestinationPageUrl
        {
            get
            {
                return ViewState[_titleDestinationPageUrlViewStateKey] != null ?
                    (string)ViewState[_titleDestinationPageUrlViewStateKey] : Constants.Hash;
            }
            set
            {
                ViewState[_titleDestinationPageUrlViewStateKey] = value;
            }

        }

        /// <summary>
        /// Gets or sets the title column header.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionMostActiveAuthorsTitleHeader")]
        [Localizable(true)]
        public string TitleHeader
        {
            get
            {
                return ViewState[_titleHeaderViewStateKey] != null ?
                    ViewState[_titleHeaderViewStateKey].ToString() : GlobalResource.TitleText;
            }
            set
            {
                ViewState[_firstNameHeaderViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the first name column header.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionMostActiveAuthorsFirstNameHeader")]
        [Localizable(true)]
        public string FirstNameHeader
        {
            get
            {
                return ViewState[_firstNameHeaderViewStateKey] != null ?
                    ViewState[_firstNameHeaderViewStateKey].ToString() : GlobalResource.PersonFirstNameDefaultHeader;
            }
            set
            {
                ViewState[_firstNameHeaderViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the middle name column header.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionMostActiveAuthorsMiddleNameHeader")]
        [Localizable(true)]
        public string MiddleNameHeader
        {
            get
            {
                return ViewState[_middleNameHeaderViewStateKey] != null ?
                    ViewState[_middleNameHeaderViewStateKey].ToString() : GlobalResource.PersonMiddleNameDefaultHeader;
            }
            set
            {
                ViewState[_middleNameHeaderViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name column header.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionMostActiveAuthorsLastNameHeader")]
        [Localizable(true)]
        public string LastNameHeader
        {
            get
            {
                return ViewState[_lastNameHeaderViewStateKey] != null ?
                    ViewState[_lastNameHeaderViewStateKey].ToString() : GlobalResource.PersonLastNameDefaultHeader;
            }
            set
            {
                ViewState[_lastNameHeaderViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the email column header.
        /// </summary>
        [ZentityCategory("CategoryBehavior")]
        [ZentityDescription("DescriptionMostActiveAuthorsEmailHeader")]
        [Localizable(true)]
        public string EmailHeader
        {
            get
            {
                return ViewState[_emailHeaderViewStateKey] != null ?
                    ViewState[_emailHeaderViewStateKey].ToString() : GlobalResource.PersonEmailDefaultHeader;
            }
            set
            {
                ViewState[_emailHeaderViewStateKey] = value;
            }
        }
        #endregion Public

        #endregion Properties

        #region Methods

        #region Protected

        /// <summary>
        /// Fetches list of most active authors.
        /// </summary>
        protected override void GetDataSource()
        {
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(this.CreateContext()))
            {
                if (!IsSecurityAwareControl)
                    this._personList.AddRange(dataAccess.GetTopAuthors(null, PageSize));
                else
                {
                    if (this.AuthenticatedToken != null)
                    {
                        this._personList.AddRange(dataAccess.GetTopAuthors(this.AuthenticatedToken, PageSize));
                    }
                }
            }
        }

        /// <summary>
        /// Creates the data to be displayed in the control at design time.
        /// </summary>
        protected override void CreateDesignTimeDataSource()
        {
            for (int index = 0; index < Constants.DesignTimeDummyDataRowCount; index++)
            {
                Person person = new Person();
                person.Title = GlobalResource.DesignTimeDummyDataString;
                person.FirstName = GlobalResource.DesignTimeDummyDataString;
                person.LastName = GlobalResource.DesignTimeDummyDataString;
                person.Email = GlobalResource.DesignTimeDummyDataString;
                _personList.Add(person);
            }
        }

        /// <summary>
        /// Creates header for the table.
        /// </summary>
        protected override void CreateHeader()
        {
            TableHeaderRow tableHeader = new TableHeaderRow();
            tableHeader.Controls.Add(CreateHeaderCell(TitleHeader));
            tableHeader.Controls.Add(CreateHeaderCell(FirstNameHeader));
            tableHeader.Controls.Add(CreateHeaderCell(LastNameHeader));
            tableHeader.Controls.Add(CreateHeaderCell(EmailHeader));
            _resourceTable.Controls.Add(tableHeader);
        }

        /// <summary>
        /// Creates rows for the table.
        /// </summary>
        protected override void CreateRows()
        {
            foreach (Contact contact in _personList)
            {
                TableRow row = new TableRow();
                row.Controls.Add(CreateHyperlinkCell(contact.Title, string.Format(CultureInfo.InvariantCulture,
                    TitleDestinationPageUrl, contact.Id)));

                Person person = contact as Person;
                if (person != null)
                {
                    row.Controls.Add(CreateTextCell(person.FirstName));
                    row.Controls.Add(CreateTextCell(person.LastName));
                }
                else
                {
                    row.Controls.Add(CreateTextCell(string.Empty));
                    row.Controls.Add(CreateTextCell(string.Empty));
                }
                row.Controls.Add(CreateTextCell(contact.Email));
                _resourceTable.Controls.Add(row);
            }
        }

        #endregion Protected


        #endregion Methods
    }
}