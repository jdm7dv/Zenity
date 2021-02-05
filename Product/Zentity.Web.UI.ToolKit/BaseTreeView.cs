// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    ///  This class implements the basic structure for the tree view.
    /// </summary>
    [Designer(typeof(TreeViewDesigner))]
    public abstract class BaseTreeView : Zentity.Web.UI.ToolKit.ZentityBase
    {
        #region Constants

        #region Private
        const string _headerViewStateKey = "TreeViewHeader";
        const string _treeViewId = "treeViewId";
        const string _titlePanelId = "titlePanelId";
        const string _treeViewPanelId = "treeViewPanelId";
        const string _containerPanelId = "containerPanelId";
        private const string _headerMessageLabelId = "MessageLabelId";
        private const string _newLineHtmlTag = "<br />";
        #endregion Private

        #endregion Constants

        #region Private
        private TableItemStyle _headerStyle;
        private TableItemStyle _containerStyle;
        private TableItemStyle _treeViewContainerStyle;
        private TreeView _treeView;
        private Panel _containerPanel;
        private Panel _treeViewPanel;
        private Panel _titlePanel;
        private Label _headerLabel;
        Label _headerMessageLabel;
        #endregion Private

        #region Properties

        #region Public

        /// <summary>
        ///  Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of tree view caption.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         ZentityCategory("CategoryApperance"),
         ZentityDescription("DescriptionZentityTableHeaderStyle"),
         NotifyParentProperty(true),
         PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle TreeViewCaptionStyle
        {
            get
            {
                if (this._headerStyle == null)
                {
                    this._headerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._headerStyle).TrackViewState();
                    }
                }
                return this._headerStyle;
            }
        }


        /// <summary>
        ///  Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of tree view container.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         ZentityCategory("CategoryApperance"),
         NotifyParentProperty(true),
         PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle TreeViewContainerStyle
        {
            get
            {
                if (this._containerStyle == null)
                {
                    this._containerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._containerStyle).TrackViewState();
                    }
                }
                return this._containerStyle;
            }
        }


        /// <summary>
        ///  Gets a reference to the <see cref="TableItemStyle" /> object that allows you to set the appearance 
        /// of tree view.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         ZentityCategory("CategoryApperance"),
         NotifyParentProperty(true),
         PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle TreeViewStyle
        {
            get
            {
                if (this._treeViewContainerStyle == null)
                {
                    this._treeViewContainerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager)this._treeViewContainerStyle).TrackViewState();
                    }
                }
                return this._treeViewContainerStyle;
            }
        }

        /// <summary>
        ///  Gets a reference to the <see cref="TreeNodeStyle" /> object that allows you to get the appearance 
        /// of tree view nodes.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         NotifyParentProperty(true),
         PersistenceMode(PersistenceMode.InnerProperty)]
        public TreeNodeStyle TreeViewNodeStyle
        {
            get
            {
                return TreeView.NodeStyle;
            }
        }

        /// <summary>
        ///  Gets a reference to the <see cref="TreeNodeStyle" /> object that allows you to get the appearance 
        /// of selected node.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         NotifyParentProperty(true),
         PersistenceMode(PersistenceMode.InnerProperty)]
        public TreeNodeStyle TreeViewSelectedNodeStyle
        {
            get
            {
                return TreeView.SelectedNodeStyle;
            }
        }



        /// <summary>
        /// Gets or sets the tree view header.
        /// </summary>
        public virtual string TreeViewCaption
        {
            get
            {
                return ViewState[_headerViewStateKey] != null ?
                    ViewState[_headerViewStateKey].ToString() : string.Empty;
            }
            set
            {
                ViewState[_headerViewStateKey] = value;
            }
        }

        #endregion Public

        #region Protected

        /// <summary>
        /// Gets the tree view control.
        /// </summary>
        protected TreeView TreeView
        {
            get
            {
                if (_treeView == null)
                {
                    _treeView = new TreeView();
                    _treeView.ID = _treeViewId;
                    _treeView.CollapseImageToolTip = GlobalResource.TreeViewCollapseImageToolTip;
                    _treeView.ExpandImageToolTip = GlobalResource.TreeViewExpandImageToolTip;
                }

                return _treeView;
            }
        }

        /// <summary>
        /// Gets container panel.
        /// </summary>
        protected Panel ContainerPanel
        {
            get
            {
                if (_containerPanel == null)
                {
                    _containerPanel = new Panel();
                    _containerPanel.ID = _containerPanelId;
                    _containerPanel.Controls.Add(TitlePanel);
                    _containerPanel.Controls.Add(HeaderMessageLabel);
                    _containerPanel.Controls.Add(TreeViewPanel);
                }

                return _containerPanel;
            }
        }

        /// <summary>
        /// Gets TreeView container panel.
        /// </summary>
        protected Panel TreeViewPanel
        {
            get
            {
                if (_treeViewPanel == null)
                {
                    _treeViewPanel = new Panel();
                    _treeViewPanel.ID = _treeViewPanelId;
                    _treeViewPanel.Controls.Add(TreeView);
                }

                return _treeViewPanel;
            }
        }

        /// <summary>
        /// Gets header message label.
        /// </summary>
        protected Label HeaderMessageLabel
        {
            get
            {
                if (_headerMessageLabel == null)
                {
                    _headerMessageLabel = new Label();
                    _headerMessageLabel.ID = _headerMessageLabelId;
                    _headerMessageLabel.EnableViewState = false;
                }
                return _headerMessageLabel;
            }
        }

        /// <summary>
        /// Gets the title panel.
        /// </summary>
        protected Panel TitlePanel
        {
            get
            {
                if (_titlePanel == null)
                {
                    _titlePanel = new Panel();
                    _titlePanel.ID = _titlePanelId;
                    _headerLabel = new Label();
                    _titlePanel.Controls.Add(_headerLabel);
                }

                return _titlePanel;
            }
        }

        #endregion

        #endregion Properties

        /// <summary>
        /// Handles the Load event of the control.
        /// </summary>
        /// <param name="e">Event paramter</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (DesignMode)
            {
                CreateDesignTimeDataSource();
                TreeView.ExpandAll();
            }
            this.Controls.Add(ContainerPanel);
        }

        /// <summary>
        /// Adds child controls.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
           
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            TreeView.ShowLines = true;
            _headerLabel.Text = TreeViewCaption;
            if (DesignMode)
            {
                CreateDesignTimeDataSource();
            }
            else
            {
                GetDataSource();
                PopulateTree();
            }

            this.ContainerPanel.ApplyStyle(TreeViewContainerStyle);
            this.TreeViewPanel.ApplyStyle(TreeViewStyle);
            this.TitlePanel.ApplyStyle(TreeViewCaptionStyle);

            base.OnPreRender(e);
        }

        /// <summary>
        /// Creates the data to be displayed in the control at design time.
        /// </summary>
        private void CreateDesignTimeDataSource()
        {
            FillNodesInDesignMode();
        }

        /// <summary>
        /// Fetches the data to be displayed in the control.  
        /// </summary>
        protected abstract void GetDataSource();

        /// <summary>
        /// Populates tree view from data source.
        /// </summary>
        protected abstract void PopulateTree();
        
        /// <summary>
        /// Creates a node for the tree view control.
        /// </summary>
        /// <param name="nodeText">The text to be displayed for the node.</param>
        protected static TreeNode CreateNode(string nodeText)
        {
            TreeNode node = new TreeNode();
            node.Text = HttpUtility.HtmlEncode(nodeText);
            node.ToolTip = nodeText;
            return node;
        }
        
        /// <summary>
        /// Creates a node for the tree view control.
        /// </summary>
        /// <param name="nodeValue">Supplemental data about the node that is not displayed.</param>
        /// <param name="nodeText">The text to be displayed for the node.</param>
        protected static TreeNode CreateNode(string nodeValue, string nodeText)
        {
            TreeNode node = new TreeNode();
            node.Text = HttpUtility.HtmlEncode(CoreHelper.FitString(CoreHelper.UpdateEmptyTitle(nodeText), 40));
            node.ToolTip = nodeText;
            node.Value = nodeValue;
            return node;
        }

        /// <summary>
        /// Creates a table cell.
        /// </summary>
        /// <param name="child">Child control.</param>
        /// <returns>A table cell.</returns>
        protected static TableCell CreateCell(Control child)
        {
            TableCell cell = new TableCell();
            cell.Controls.Add(child);
            return cell;
        }

        /// <summary>
        /// Creates a table row.
        /// </summary>
        /// <param name="cell">Cell to add to the row.</param>
        /// <returns>Table row.</returns>
        protected static TableRow CreateRow(TableCell cell)
        {
            TableRow row = new TableRow();
            row.Cells.Add(cell);
            return row;
        }

        /// <summary>
        /// Creates a new line literal control.
        /// </summary>
        /// <returns>Literal control.</returns>
        protected static Literal CreateNewLine()
        {
            Literal literalNewLine = new Literal();
            literalNewLine.Text = _newLineHtmlTag;
            return literalNewLine;
        }

        private void FillNodesInDesignMode()
        {
            //TreeNode rootNode = new TreeNode();
            TreeNode rootNode = CreateNode(GlobalResource.DesignModecategoryNodeText);

            for (int index = 1; index < 4; index++)
            {
                rootNode.ChildNodes.Add(CreateNode(index.ToString(CultureInfo.CurrentCulture)));
            }
             
            TreeView.Nodes.Add(rootNode);
        }

    }
}