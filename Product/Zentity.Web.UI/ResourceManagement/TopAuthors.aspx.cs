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
using System.Web.UI.WebControls;
using Zentity.Web.UI;
using Zentity.Security.Authentication;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI.ToolKit;
using Zentity.Core;
using System.Web.UI.HtmlControls;
using Zentity.Security.Authorization;
using Zentity.Security.AuthorizationHelper;

public partial class ResourceManagement_TopAuthors : ZentityBasePage
{
    #region Constants

    #region Private

    private const string _resourceDetailUrl = "ManageResource.aspx?ActiveTab=Summary&Id={0}";
    private const string _authoredWorks = "AuthoredWorks";
    private const int _authoredWorksCount = 10;
    private const int _pageSize = 10;
    private const int _maxChar = 40;
    private const string _topAuthorTableStyle = "TopAuthorContainer";
    private const string _div = "DIV";
    private const string _topAuthorDivStyle = "TopAuthorInternalDynDiv";

    #endregion

    #endregion

    #region Member Variables

    #region Private

    private Dictionary<Guid, IEnumerable<string>> _userPermissions = null;

    #endregion

    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        AuthenticatedToken authenToken = this.Session[Constants.AuthenticationTokenKey] as
                                                AuthenticatedToken;
        List<Contact> topAuthors = GetTopAuthors(authenToken, _pageSize);
        _userPermissions = GetPermissions(authenToken, topAuthors.Select(tuple => tuple as Resource).ToList());
        FillTable(topAuthors);
    }

    /// <summary>
    /// Get user permissions for the specified list of resources and their related resources.
    /// </summary>
    /// <param name="token">Authentication token</param>
    /// <param name="resources">List or resources</param>
    /// <returns>Mapping resource id and user permissons on the resource</returns>
    private Dictionary<Guid, IEnumerable<string>> GetPermissions(AuthenticatedToken token,
                                                                    IList<Resource> resources)
    {
        Dictionary<Guid, IEnumerable<string>> userPermissions = null;

        if (resources != null && token != null && resources.Count > 0)
        {
            IList<Resource> srcResources = resources.ToList();
            foreach (Resource res in resources)
            {
                //add author resources to source list
                Contact contact = res as Contact;
                if (contact != null && contact.AuthoredWorks != null && contact.AuthoredWorks.Count > 0)
                {
                    srcResources = srcResources.Union(contact.AuthoredWorks.Take(_authoredWorksCount).
                                                Select(tuple =>
                                                tuple as Resource).ToList()).ToList();
                }
            }

            var permissons = GetResourcePermissions(token, srcResources);

            if (permissons != null)
            {
                userPermissions = permissons.ToDictionary(tuple => tuple.Resource.Id,
                                                            tuple => tuple.Permissions);
            }

        }

        //This is a default case which indicates that user is not having any permission.
        if (userPermissions == null)
            userPermissions = new Dictionary<Guid, IEnumerable<string>>(); ;

        return userPermissions;
    }

    /// <summary>
    /// Gets user permission for the resources in the specified resource list.
    /// </summary>
    /// <param name="token">Authentication token</param>
    /// <param name="resources">List of resources</param>
    /// <returns>List of resources mapped with user permissions.</returns>
    public IEnumerable<ResourcePermissions<T>> GetResourcePermissions<T>(AuthenticatedToken token,
                                                                IList<T> resources) where T : Resource
    {
        using (ZentityContext context = new ZentityContext())
        {
            if (token != null && resources != null)
            {
                if (resources != null && resources.Count > 0 && token != null)
                    return resources.GetPermissions<T>(context, token);
            }
            return null;
        }
    }

    /// <summary>
    /// Returns list of latest added resources
    /// </summary>
    /// <param name="pageSize">Maximum No. of records to be fetched</param>
    /// <param name="token">Authenticated token</param>
    /// <returns></returns>
    public static List<Contact> GetTopAuthors(AuthenticatedToken token, int pageSize)
    {
        using (ResourceDataAccess dataAccess = new ResourceDataAccess(Utility.CreateContext()))
        {
            return dataAccess.GetTopAuthors(token, pageSize);
        }
    }

    /// <summary>
    /// Fill table with top authors.
    /// </summary>
    /// <param name="topAuthors">Collection of top author.</param>
    private void FillTable(List<Contact> topAuthors)
    {
        TableRow containerDivRow = new TableRow();
        containerDivRow.Attributes.Add("classs", "testPadding");
        TableCell containerDivCell = new TableCell();
        containerDivCell.ColumnSpan = 4;

        Table containerTable = CreateTable();
        int authorCnt = 0;
        foreach (Contact contact in topAuthors)
        {
            authorCnt++;
            TableRow row = new TableRow();

            TableCell contactTextCell = CreateTableCell(Unit.Percentage(15),
                                                        Resources.Resources.ZentityAuthor);
            TableCell contactValueCell = NewCreateTableCell(Unit.Percentage(35));

            string authorName = GetDataByType(contact);
            HyperLink hyperLink = null;
            // Check user have permission to see resource.
            if (_userPermissions.Keys.Contains(contact.Id))
            {
                hyperLink = Utility.CreateHyperLink(Utility.FitString(authorName, _maxChar),
                                                        string.Format(_resourceDetailUrl, contact.Id));
            }
            else
            {
                hyperLink = Utility.CreateHyperLink(Utility.FitString(authorName, _maxChar),
                                                        string.Empty);
            }
            contactValueCell.Controls.Add(hyperLink);

            TableCell emailCell = CreateTableCell(Unit.Percentage(15), Resources.Resources.ZentityEmail);

            TableCell emailValueCell = NewCreateTableCell(Unit.Percentage(35));
            Label label = new Label();
            label.Text = contact.Email;
            emailValueCell.Controls.Add(label);

            row.Controls.Add(contactTextCell);
            row.Controls.Add(contactValueCell);
            row.Controls.Add(emailCell);
            row.Controls.Add(emailValueCell);
            containerTable.Controls.Add(row);
          
            List<ScholarlyWork> scholaryWorks = contact.AuthoredWorks.Take(_authoredWorksCount).ToList();
            TableRow authorWorksRow = AddAuthorWorks(scholaryWorks, contact.Id);
            containerTable.Controls.Add(authorWorksRow);

            if (authorCnt < topAuthors.Count)
            {
                TableRow divRow = CreateDivRow();
                containerTable.Controls.Add(divRow);
            }
        }
        containerDivCell.Controls.Add(containerTable);
        containerDivRow.Controls.Add(containerDivCell);
        TopAuthorsTable.Controls.Add(containerDivRow);
    }

    /// <summary>
    /// If resource is of type Person then first name and last name will be returned
    //  otherwise title will be returned.
    /// </summary>
    /// <param name="contact"></param>
    /// <returns></returns>
    private string GetDataByType(Contact contact)
    {
        string authorName = string.Empty;
        if (contact is Person)
        {
            Person person = contact as Person;
            if (!string.IsNullOrEmpty(person.FirstName) || (
                                    !string.IsNullOrEmpty(person.LastName)))
            {
                authorName = HttpUtility.HtmlEncode(person.FirstName) + Constants.Space + HttpUtility.HtmlEncode(person.LastName);
            }
            else
            {
                authorName = ValidateTitle(contact.Title);
            }
        }
        else
        {
            authorName = ValidateTitle(contact.Title);
        }
        return authorName;
    }

    /// <summary>
    /// Create a row with div as child control and apply style on the div.
    /// </summary>
    /// <returns></returns>
    private TableRow CreateDivRow()
    {
        TableRow divRow = new TableRow();

        TableCell divcell = new TableCell();
        divcell.ColumnSpan = 4;

        HtmlGenericControl dynDiv = new HtmlGenericControl(_div);
        dynDiv.Attributes.Add(Constants.CssStyleClass, _topAuthorDivStyle);
        divcell.Controls.Add(dynDiv);
        divRow.Controls.Add(divcell);
        return divRow;
    }

    /// <summary>
    /// Validate resource's title.
    /// </summary>
    /// <param name="title">Title to be validated.</param>
    /// <returns>Validated title.</returns>
    private string ValidateTitle(string title)
    {
        if (string.IsNullOrEmpty(title) || title.Trim() == string.Empty)
        {
            title = Resources.Resources.ZentityNoTitle;
        }
        return HttpUtility.HtmlEncode(title);
    }

    /// <summary>
    /// Create Table cell with specified width and text.
    /// </summary>
    /// <param name="width">Cell's width in pixel.</param>
    /// <param name="text">Text to be displayed on the cell.</param>
    /// <returns>Instance of TableCell class.</returns>
    private static TableCell CreateTableCell(Unit width, string text)
    {
        TableCell tableCell = new TableCell();
        tableCell.Width = width;
        tableCell.HorizontalAlign = HorizontalAlign.Left;
        tableCell.Text = text;

        tableCell.VerticalAlign = VerticalAlign.Top;
        return tableCell;
    }

    /// <summary>
    /// Create Table cell with specified width and text.
    /// </summary>
    /// <param name="width">Cell's width in pixel.</param>
    /// <returns>Instance of TableCell class.</returns>
    private static TableCell NewCreateTableCell(Unit width)
    {
        TableCell tableCell = new TableCell();
        tableCell.Width = width;
        tableCell.HorizontalAlign = HorizontalAlign.Left;
        tableCell.VerticalAlign = VerticalAlign.Top;
        return tableCell;
    }

    /// <summary>
    /// Create a table and apply style on the table.
    /// </summary>
    /// <returns></returns>
    private Table CreateTable()
    {
        Table table = new Table();
        table.CellPadding = 0;
        table.CellSpacing = 0;
        table.Attributes.Add(Constants.CssStyleClass, _topAuthorTableStyle);
        return table;
    }

    /// <summary>
    /// Create space and add it to control.
    /// </summary>
    public static Literal CreateSpace()
    {
        Literal litSpace = new Literal();
        litSpace.Text = "," + "&nbsp; ";
        return litSpace;
    }

    /// <summary>
    /// Create table row and add authoredworks collection to the row.
    /// </summary>
    /// <param name="scholaryWorks"></param>
    /// <param name="enableMoreLink"></param>
    /// <returns></returns>
    private TableRow AddAuthorWorks(List<ScholarlyWork> authoredWorks, Guid contactId)
    {
        bool enableMoreLink = false;
        TableCell authorsWorksTextCell = CreateTableCell(Unit.Percentage(20), Resources.Resources.ZentityMostAuthoredWorks);

        TableCell authorsWorksValueCell = NewCreateTableCell(Unit.Percentage(80));
        authorsWorksValueCell.ColumnSpan = 3;

        int schWorkCnt = 0;
        foreach (ScholarlyWork schWork in authoredWorks)
        {
            Control control;
            if (_userPermissions.Keys.Contains(schWork.Id))
            {
                if (schWorkCnt > 0)
                {
                    Literal space = CreateSpace();
                    authorsWorksValueCell.Controls.Add(space);
                }
                schWorkCnt++;
                control = Utility.CreateHyperLink(Utility.FitString(ValidateTitle(schWork.Title), _maxChar),
                                    string.Format(_resourceDetailUrl, schWork.Id));

                authorsWorksValueCell.Controls.Add(control);

                
            }
        }

        if (schWorkCnt >= _authoredWorksCount && authoredWorks.Count >= _authoredWorksCount)
            enableMoreLink = true;

        if (enableMoreLink)
        {
            Literal space = CreateSpace();
            authorsWorksValueCell.Controls.Add(space);
            HyperLink hyperLink = Utility.CreateHyperLink(Resources.Resources.ZentityMore,
                                            string.Format(_resourceDetailUrl, contactId));
            authorsWorksValueCell.Controls.Add(hyperLink);
        }

        TableRow row = new TableRow();
        row.Controls.Add(authorsWorksTextCell);
        row.Controls.Add(authorsWorksValueCell);
        return row;

    }

    private Label CreateLabel(string title)
    {
        Label label = new Label();
        label.Text = title;
        return label;
    }
}