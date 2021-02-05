// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This control displays the tags in a descending order 
    /// based on the count of reference by the resources.
    /// </summary>
    /// <example>The code below is the source for TagCloud.aspx. It shows an example of using <see cref="TagCloud"/>.
    ///     <code>
    ///         &lt;%@ Page Language="C#" AutoEventWireup="true" %&gt;
    ///         
    ///         &lt;%@ Register Assembly="Zentity.Web.UI.ToolKit" Namespace="Zentity.Web.UI.ToolKit"
    ///             TagPrefix="Zentity" %&gt;
    ///             
    ///         &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"&gt;
    ///         &lt;html xmlns="http://www.w3.org/1999/xhtml"&gt;
    ///             &lt;head runat="server"&gt;
    ///                 &lt;title&gt;TagCloud Sample&lt;/title&gt;
    ///         &lt;/head&gt;
    ///         &lt;body&gt;
    ///             &lt;form id="form1" runat="server"&gt;
    ///             &lt;div&gt;
    ///                 &lt;Zentity:TagCloud ID="TopTags1" runat="server" BorderWidth="1px" EnableViewState="False"
    ///                     MaximumFontSize="32" MinimumFontSize="10" MaximumTagsToFetch="20" 
    ///                     TagClickDestinationPageUrl="ResourceDetailView.aspx?Id={0}" IsSecurityAwareControl="false"&gt;
    ///                 &lt;/Zentity:TagCloud&gt;
    ///             &lt;/div&gt;
    ///             &lt;/form&gt;
    ///         &lt;/body&gt;
    ///         &lt;/html&gt;
    ///     </code>
    /// </example>
    [Designer(typeof(TagCloudDesigner))]
    public class TagCloud : ZentityBase
    {
        #region Member Variables

        #region Protected

        /// <summary>
        /// Represents a container table.
        /// </summary>
        protected Table _tagCloudTable = new Table();

        #endregion

        #endregion Member Variables

        #region Constants

        #region Private

        private const string _maxTagDisplayCountViewStateKey = "MaxTagDisplayCount";
        private const string _navigateURLViewStateKey = "NavigateURL";
        private const string _minFontSize = "MinimumFontSize";
        private const string _maxFontSize = "MaximumFontSize";
        private int _densityLevels;
        /// <summary>
        /// Used to store reference counts and related font sizes.
        /// </summary>
        private HybridDictionary _referenceFontSizes = new HybridDictionary();
        private const int _defaultMaxTagCount = 20;
        private int _overallMaxRefCount;
        private int _overallMinRefCount;
        private int _prevLevelRefCountRange;

        private const string _title = "Popular Tags";
        private const int _defalutMinFontSize = 12;
        private const int _defaultMaxFontSize = 32;
        private const double _twentyFivePercent = 0.25;
        private const int _two = 2;
        private const int _eigth = 8;
        private const int _titleRowIndex = 0;
        private const int _dataRowStartIndex = 1;
        private const string _space = "&nbsp; " + "&nbsp; ";
        private const string _fontSize = "font-size";
        private const string _name = "title";
        private const string _pixel = "px";

        #endregion

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Gets or sets count of maximum tags to be displayed.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionTagCloudMaximumTagsToFetch")]
        [Localizable(true)]
        public int MaximumTagsToFetch
        {
            get
            {
                return ViewState[_maxTagDisplayCountViewStateKey] != null ?
                                            (int)ViewState[_maxTagDisplayCountViewStateKey] : _defaultMaxTagCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(GlobalResource.MaximumTagsFetchException);
                }
                ViewState[_maxTagDisplayCountViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets navigation path.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionTagCloudTagClickDestinationPageUrl")]
        public string TagClickDestinationPageUrl
        {
            get
            {
                return ViewState[_navigateURLViewStateKey] != null ? (string)ViewState[_navigateURLViewStateKey] : string.Empty;
            }
            set
            {
                ViewState[_navigateURLViewStateKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets minimum font size for tags to be displayed.
        /// </summary>
        [ZentityCategory("CategoryPaging")]
        [ZentityDescription("DescriptionTagCloudMinimumFontSize")]
        [Localizable(true)]
        public int MinimumFontSize
        {
            get
            {
                return ViewState[_minFontSize] != null ? (int)ViewState[_minFontSize] : _defalutMinFontSize;
            }
            set
            {
                if (value > 0 && value < this.MaximumFontSize)
                {
                    ViewState[_minFontSize] = value;
                }
                else
                {
                    throw new ArgumentException(GlobalResource.MinimumFontSizeLessThanMaxFontSize);
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum font size for tags to be displayed.
        /// </summary>
        [ZentityCategory("Category_Paging")]
        [ZentityDescription("Description_TagCloud_MaximumFontSize")]
        [Localizable(true)]
        public int MaximumFontSize
        {
            get
            {
                return ViewState[_maxFontSize] != null ? (int)ViewState[_maxFontSize] : _defaultMaxFontSize;
            }
            set
            {
                if (value > 0 && value > this.MinimumFontSize)
                {
                    ViewState[_maxFontSize] = value;
                }
                else
                {
                    throw new ArgumentException(GlobalResource.MaximumFontSizeGreaterThanMinFontSize);
                }

            }
        }

        #endregion

        #endregion

        #region Methods

        #region Protected

        /// <summary>
        /// Applies style to header title and creates design time appearance of the control.
        /// </summary>
        /// <param name="writer">Instance of HtmlTextWriter that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode)
            {
                this.CreateDesignerData();
            }
            ApplyStyleToTitle();
            base.Render(writer);
        }

        /// <summary>
        /// Adds child controls to the control and fire PreRender event.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            if (Title == null)
            {
                Title = _title;
            }
            base.OnPreRender(e);
            AddChildControls();
        }

        #endregion

        #region Private

        /// <summary>
        /// Creates a title row for the control.
        /// </summary>
        private void CreateTitleRow()
        {
            TableHeaderRow tableHeader = new TableHeaderRow();
            tableHeader.Controls.Add(CreateHeaderCell(Title));
            _tagCloudTable.CellSpacing = 0;
            _tagCloudTable.Controls.Add(tableHeader);
        }

        /// <summary>
        /// Applies style to title.
        /// </summary>
        private void ApplyStyleToTitle()
        {
            TableRow tableTitle = (TableRow)_tagCloudTable.Controls[_titleRowIndex];
            ApplyRowStyle(tableTitle, this.TitleStyle);
        }

        /// <summary>
        /// Iterate through all the item cells and apply stem style.
        /// </summary>
        /// <param name="row"> Table row to be applied style </param>
        /// <param name="style">Style to be applied to table row</param>
        private static void ApplyRowStyle(TableRow row, TableItemStyle style)
        {
            for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
            {
                TableCell cell = row.Cells[cellIndex];
                cell.MergeStyle(style);
            }
        }

        /// <summary>
        /// This method fetch top X no of tags and add each as a 
        /// link in the control in descending order based on 
        /// the count of reference by the resource.
        /// </summary>
        private void AddChildControls()
        {
            this._tagCloudTable.Controls.Clear();
            this._tagCloudTable.Width = Unit.Percentage(100);
            this.CreateTitleRow();
            if (this.MaximumTagsToFetch == 0)
            {
                AddEmptyTableRow();
                return;
            }

            List<TagCloudEntry> tagCloudEntries = null;
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                if (IsSecurityAwareControl)
                {
                    if (AuthenticatedToken != null)
                    {
                        tagCloudEntries = dataAccess.GetTopTagsForTagCloud(this.MaximumTagsToFetch, AuthenticatedToken);
                    }
                }
                else
                {
                    tagCloudEntries = dataAccess.GetTopTagsForTagCloud(this.MaximumTagsToFetch, null);
                }
            }

            if (tagCloudEntries == null || tagCloudEntries.Count == 0)
            {
                AddEmptyTableRow();
                return;
            }

            // Sort tags by "Name" property of tag for displaying tags in ascending order
            tagCloudEntries = tagCloudEntries.OrderBy(tagCloudEntry => tagCloudEntry.Tag.Name).ToList();

            this._densityLevels = GetDensityLevel();

            // Save tag's font size with corresponding reference count
            SetTagsFontSize(this._densityLevels, tagCloudEntries);

            AttachDisplayData(tagCloudEntries);
        }

        private void AddEmptyTableRow()
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.Text = _space;
            row.Cells.Add(cell);
            this._tagCloudTable.Controls.Add(row);
            this.Controls.Add(this._tagCloudTable);
        }

        /// <summary>
        /// Save font size with corresponding reference count in collection.
        /// </summary>
        /// <param name="referenceCount">Reference count.</param>
        /// <param name="fontSize">Font size calculated for a tag.</param>
        private void SaveFontSize(int referenceCount, float fontSize)
        {
            if (!_referenceFontSizes.Contains(referenceCount))
            {
                _referenceFontSizes.Add(referenceCount, (int)Math.Round(fontSize));
            }
        }

        /// <summary>
        /// Attach table control which contains title header and hyperlinks to tag cloud control
        /// </summary>
        /// <param name="tagCloudEntries">List of tag cloud entries.</param>
        private void AttachDisplayData(List<TagCloudEntry> tagCloudEntries)
        {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            foreach (TagCloudEntry pair in tagCloudEntries)
            {
                cell.Controls.Add(CreateHyperLink(pair.Tag.Id.ToString(), pair.Tag.Name, pair.ReferenceCount, ResolveUrl(this.TagClickDestinationPageUrl)));
                cell.Controls.Add(CreateSpace());
            }
            row.Controls.Add(cell);
            this._tagCloudTable.Controls.Add(row);
            this.Controls.Add(this._tagCloudTable);
        }

        /// <summary>
        /// Gets density levels.
        /// </summary>
        /// <returns>Density level.</returns>
        private int GetDensityLevel()
        {
            // Density level decides the number of groups into which all top tags will be divided 
            // depending on popularity
            int densityLevels = (int)((this.MaximumFontSize - this.MinimumFontSize) * _twentyFivePercent) - 1;

            // Top tags will be divided at least into one group 
            if (densityLevels < 1)
            {
                densityLevels = 1;
            }

            return densityLevels;
        }

        /// <summary>
        /// Sets font size of tags depends upon number of resources which have been tagged.
        /// </summary>
        /// <param name="densityLevels">Number of density levels.</param>
        /// <param name="tagCloudEntries">List of tag cloud entries.</param>
        private void SetTagsFontSize(int densityLevels, List<TagCloudEntry> tagCloudEntries)
        {
            #region Font size calculation approach
            /* 
             
             1.	Calculate density level that decides the number of groups into which all top tags will be divided depending on popularity.
             2.	Density level is calculate using formula :
             
                25 % of (Maximum font –minimum font size)
             
             3.	Reference count range is calculate for each density level that decides maximum tag’s reference count will be mapped into density level.
             4.	Reference count calculates using formula :
             
                (Maximum reference count – Minimum Reference Count)/Density level.
             
                it will be calculate using "density level * Reference count range" for next level.
             
             5.	Font size range calculates for each density level that decides maximum font size can be set to tags in each density level. 
             6.	Font size range is calculate using formula :
             
                (Maximum font size – Minimum font size)/Density level.
             
                it will be calculate using "density level * Font size range" for next level.
             
            7.	Font size calculation logic for tags per density level

                a.	If tags were not uniformly distributed in density level then font size calculation logic is changed.
             
                b.	 Uniform distribution is calculate per density level using formula :
             
                    Any of tag’s reference count > (allotted reference count range/2) then distribution is uniform otherwise low.
             
                c.	Tag’s font size calculation logic depending upon distribution of tags in each level:

                    •	Calculation formula if distribution is proper :

                        Tag’s reference count * font Size Range) / allotted Reference Count Range) + 
                        (font Size Range * (current Density Level Count - 1)) +  Minimum Font Size)

                    •	If distribution is low then grouping tags once again so font size difference can also visualize to user. 
                        New grouping formula follows existing way of grouping data explained in step 2 with slightly changes:
                    
                        Formula: 
                    
                        25% of (allotted reference count range/2)
                                     Or 
                        1/8(allotted reference count range)
             
                        So now tag’s font size calculation formula is: 
             
                        Tag’s reference count * 8) / allotted Reference Count Range) + 
                        (font Size Range * (current Density Level Count - 1)) +  Minimum Font Size)
             */
            #endregion

            List<int> tagReferenceCount = new List<int>();
            foreach (TagCloudEntry tagCloudEntry in tagCloudEntries)
            {
                tagReferenceCount.Add(tagCloudEntry.ReferenceCount);
            }

            tagReferenceCount.Sort();
            List<int> tagsReferenceCountinCurrentDensityLevel = new List<int>();
            this._overallMinRefCount = tagReferenceCount.Min();
            this._overallMaxRefCount = tagReferenceCount.Max();

            int referenceCountRange = (int)Math.Ceiling((decimal)(this._overallMaxRefCount - this._overallMinRefCount) / densityLevels);

            int densityLevelCount = this._overallMinRefCount;
            int j = 0;
            int currentdensityLevel = 0;
            while (densityLevelCount < this._overallMaxRefCount)
            {
                if (j < tagReferenceCount.Count() && tagReferenceCount[j] <= densityLevelCount + referenceCountRange)
                {
                    tagsReferenceCountinCurrentDensityLevel.Add(tagReferenceCount[j]);
                    j++;
                }
                else
                {
                    currentdensityLevel++;
                    if (tagsReferenceCountinCurrentDensityLevel.Count > 0)
                    {
                        CalculateTagsDistributionPerLevel(tagsReferenceCountinCurrentDensityLevel, referenceCountRange,
                                                            currentdensityLevel);
                    }
                    tagsReferenceCountinCurrentDensityLevel.Clear();
                    densityLevelCount += referenceCountRange;
                }
            }

            // True if all top tags tagged with same number of resources
            if (referenceCountRange == 0)
            {
                foreach (int referenceCount in tagReferenceCount)
                {
                    this.SaveFontSize(referenceCount, this.MaximumFontSize);
                }
            }
        }

        /// <summary>
        /// Calculates distribution of tags in current density level.
        /// </summary>
        /// <param name="currentLevelTags">List of tags for that font size to be calculated.</param>
        /// <param name="allotedRefCountRange">Represents range of reference count decided for current density level.</param>
        /// <param name="currentDensityLevelCount">Represents current density level.</param>
        private void CalculateTagsDistributionPerLevel(List<int> currentLevelTags, int allotedRefCountRange,
                                                        int currentDensityLevelCount)
        {
            int currentLevelMaxRefCount = currentLevelTags.Max();

            this._prevLevelRefCountRange = this._overallMinRefCount + (allotedRefCountRange * (currentDensityLevelCount - 1));
            int differenceCount = (currentLevelMaxRefCount) - this._prevLevelRefCountRange;

            // Calculate font size range that decides maximum font size can be set to tags in each density level.
            int fontSizeRange = (int)((this.MaximumFontSize - this.MinimumFontSize) / this._densityLevels);

            // True if any of the reference count in current level greater than half of allotted reference count range
            // if true means distributions of tags is uniform otherwise low distribution 
            if (differenceCount > (allotedRefCountRange / _two))
            {
                CalculateFontSizes(currentLevelTags, fontSizeRange, currentDensityLevelCount
                                , allotedRefCountRange, false);
            }
            else
            {
                CalculateFontSizes(currentLevelTags, fontSizeRange, currentDensityLevelCount
                                 , allotedRefCountRange, true);
            }
        }

        /// <summary>
        /// Calculates font size for specified list of tag.
        /// </summary>
        /// <param name="currentLevelTags">List of tags for which font size will be calculated.</param>
        /// <param name="fontSizeRange">Represents range of font size decided for current density level.</param>
        /// <param name="currentDensityLevelCount">Represents current density level.</param>
        /// <param name="allotedRefCountRange">Represents range of reference count decided for current density level.</param>
        /// <param name="isLowerDistribution">Represents whether tags is uniformly distributed within density level on the 
        /// basis of tag's reference count.</param>
        private void CalculateFontSizes(List<int> currentLevelTags, int fontSizeRange, int currentDensityLevelCount
                                     , int allotedRefCountRange, bool isLowerDistribution)
        {
            foreach (int currentTagCount in currentLevelTags)
            {
                float fontSize;

                // Gets tag count within range of reference count range by removing reference count range of previous density level.
                int refCountWithinLevel = currentTagCount - this._prevLevelRefCountRange;

                // True if overall maximum reference count occurs
                if (currentTagCount == this._overallMaxRefCount)
                {
                    fontSize = this.MaximumFontSize;
                }
                else if (isLowerDistribution)
                {
                    fontSize = ((((float)(refCountWithinLevel * _eigth) / allotedRefCountRange) +
                                         ((fontSizeRange * (currentDensityLevelCount - 1)))) +
                                          this.MinimumFontSize);
                }
                else
                {
                    fontSize = (((float)(refCountWithinLevel * fontSizeRange) / allotedRefCountRange)
                                          + (fontSizeRange * (currentDensityLevelCount - 1))
                                          + this.MinimumFontSize);
                }
                if (fontSize > this.MaximumFontSize)
                {
                    fontSize = this.MaximumFontSize;
                }

                // Save font size with corresponding reference count in collection 
                SaveFontSize(currentTagCount, fontSize);
            }
        }

        /// <summary>
        /// Creates hyperlink with effect of cloud tags and add it to control.
        /// </summary>
        /// <param name="tagID">tagIDtagID</param>
        /// <param name="tagName">tagName</param>
        /// <param name="tagCount">tagCount</param>
        /// <param name="rootPath">rootPath</param>
        /// <returns>Instance of HyperLink</returns>
        private HyperLink CreateHyperLink(string tagID, string tagName, int tagCount, string rootPath)
        {
            HyperLink linkTag = new HyperLink();
            linkTag.NavigateUrl = string.Format(CultureInfo.InvariantCulture, rootPath, tagID);
            int fontSize = (int)_referenceFontSizes[tagCount];
            linkTag.Style.Add(_fontSize, fontSize + _pixel);
            linkTag.ToolTip = string.Format(CultureInfo.CurrentCulture, GlobalResource.TagCloudEntryToolTip, tagName, tagCount);
            linkTag.Attributes.Add(_name, tagName);
            linkTag.Text = HttpUtility.HtmlEncode(tagName);
            return linkTag;
        }

        /// <summary>
        /// Creates header cell with text and returns TableHeaderCell.
        /// </summary>
        /// <param name="text">text as string</param>
        /// <returns>returns TableHeaderCell</returns>
        private static TableHeaderCell CreateHeaderCell(string text)
        {
            TableHeaderCell cell = new TableHeaderCell();
            cell.Text = text;
            return cell;
        }

        /// <summary>
        /// Create space and add it to control.
        /// </summary>
        private static Literal CreateSpace()
        {
            Literal litSpace = new Literal();
            litSpace.Text = _space;
            return litSpace;
        }

        #region Designer View Code

        /// <summary>
        /// Creates dummy tags and binds to tag cloud control for supporting design time view.
        /// </summary>
        private void CreateDesignerData()
        {
            this._tagCloudTable.Controls.Clear();
            this._tagCloudTable.Width = Unit.Percentage(100);
            this.CreateTitleRow();

            string[] tagNameCollection = GlobalResource.TagNameCollection.Split(',');
            int[] fontSizeCollection = { 16, 20, 12, 24, 14, 10, 15, 18, 22, 28 };
            int count = 0;
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            while (count < tagNameCollection.Count())
            {
                cell.Controls.Add(CreateHyperLinkForDesignTime(tagNameCollection[count], fontSizeCollection[count]));
                cell.Controls.Add(CreateSpace());
                count++;
            }
            row.Controls.Add(cell);
            this._tagCloudTable.Controls.Add(row);
            this.Controls.Add(this._tagCloudTable);
        }

        /// <summary>
        /// Creates hyperlink with effect of cloud tags and add it to control.
        /// </summary>
        /// <param name="tagName">Name of tag</param>
        /// <param name="fontSize">font size for tag</param>
        /// <returns>Instance of HyperLink</returns>
        private static HyperLink CreateHyperLinkForDesignTime(string tagName, int fontSize)
        {

            HyperLink linkTag = new HyperLink();
            linkTag.ForeColor = System.Drawing.Color.Blue;
            linkTag.Style.Add(_fontSize, fontSize + _pixel);
            linkTag.Text = tagName;
            return linkTag;
        }

        #endregion #region Designer View Code

        #endregion

        #endregion
    }

    /// <summary>
    /// This class manages the design time rendering of Tag Cloud control.
    /// </summary>
    class TagCloudDesigner : ControlDesigner
    {
        #region Constants

        #region Private
        private const string _div = "<Div></Div>";
        #endregion Private

        #endregion Constants

        #region Member variables

        #region Private

        TagCloud _tagCloud;

        #endregion Private

        #endregion Member variables

        #region Methods

        #region Public

        /// <summary>
        /// Initializes the control designer and loads the specified component.
        /// </summary>
        /// <param name="component">The control being designed.</param>
        public override void Initialize(System.ComponentModel.IComponent component)
        {
            base.Initialize(component);
            this._tagCloud = (TagCloud)component;
        }

        /// <summary>
        /// Retrieves the HTML markup that is used to represent the control at design
        /// time.
        /// </summary>
        /// <returns>HTML code for control.</returns>
        public override string GetDesignTimeHtml()
        {
            if (this._tagCloud != null)
            {
                StringWriter sw = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                this._tagCloud.RenderControl(new HtmlTextWriter(sw));
                return sw.ToString();
            }
            return _div;
        }

        #endregion

        #endregion
    }
}
