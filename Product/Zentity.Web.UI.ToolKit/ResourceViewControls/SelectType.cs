// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Security.Authentication;
using Zentity.Web.UI.ToolKit.Resources;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Custom control which displays the resource types tree view.
    /// </summary>
    public class SelectType : BaseTreeView
    {
        #region Constants

        private const string _headerViewStateKey = "TreeViewHeader";
        private string _resourceType;
        private string _resourceHierarchy;

        #endregion Constants

        /// <summary>
        /// Event handler for resource type selection.
        /// </summary>
        public event EventHandler<EventArgs> OnResourceTypeSelected;

        /// <summary>
        /// Gets or sets the tree view header.
        /// </summary>
        public override string TreeViewCaption
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

        /// <summary>
        /// Gets or sets the name of the base type for resource type hierarchy
        /// </summary>
        public string ResourceHierarchy
        {
            get
            {
                return _resourceHierarchy;
            }
            set
            {
                _resourceHierarchy = value;
                PopulateTree();
            }
        }

        /// <summary>
        /// Gets the selected resource type
        /// </summary>
        public string ResourceType
        {
            get
            {
                return _resourceType;
            }
        }


        /// <summary>
        /// Creates the layout of child controls.
        /// </summary>
        /// <param name="e">Event Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ContainerPanel.Controls.Remove(TreeViewPanel);

            ContainerPanel.Style.Add(HtmlTextWriterStyle.Width, "100%");
            
            ContainerPanel.Controls.Add(TreeViewPanel);

            base.HeaderMessageLabel.Visible = false;
            base.TreeView.SelectedNodeChanged += new EventHandler(ResourceTypesTreeView_SelectedNodeChanged);
            
        }

        /// <summary>
        /// Overrides abstract method GetDataSource of the BaseTreeView.
        /// </summary>
        protected override void GetDataSource()
        {
            //nothing to implement
        }

        /// <summary>
        /// Populates tree with resource type hierarchy
        /// </summary>
        protected override void PopulateTree()
        {
            base.TreeView.Nodes.Clear();
            List<ResourceType> resTypes = GetResourceTypeList();

            if (string.IsNullOrEmpty(ResourceHierarchy))
            {
                List<ResourceType> parentResTypes = resTypes.Where(tuple => tuple.BaseType == null)
                    .ToList();
                foreach (ResourceType parentResType in parentResTypes)
                {
                    RemoveTypeFromList(resTypes, parentResType);
                    AddBranch(resTypes, parentResType);
                }
            }
            else
            {
                ResourceType resType = GetResourceTypeFromName(resTypes);
                if (resType != null)
                {
                    resTypes = resTypes.Where(tuple => tuple.BaseType != null).ToList();
                    AddBranch(resTypes, resType);
                }

            }
        }

        //Removes the given type from the given list.
        private static void RemoveTypeFromList(List<ResourceType> resTypes, ResourceType parentResType)
        {
            ResourceType typeToRemove = resTypes.Where(tuple => tuple.Name == parentResType.Name)
                .FirstOrDefault();
            if (typeToRemove != null)
            {
                resTypes.Remove(typeToRemove);
            }
        }

        //Adds resource type and its child nodes to the tree
        private void AddBranch(List<ResourceType> resTypes, ResourceType resType)
        {
            TreeNode parentNode = new TreeNode(resType.Name, resType.Id.ToString());
            base.TreeView.Nodes.Add(parentNode);
            CreateChildNodes(parentNode, resTypes, resType);
        }

        //Gets the resource type from its full name or name set in the ResourceHierarchy property
        private ResourceType GetResourceTypeFromName(List<ResourceType> resTypes)
        {
            ResourceType resType = null;
            if (ResourceHierarchy.Contains("."))
            {
                resType = resTypes.Where(tuple =>
                    tuple.FullName == ResourceHierarchy).FirstOrDefault();
            }
            else
            {
                resType = resTypes.Where(tuple =>
                    tuple.Name == ResourceHierarchy).FirstOrDefault();
            }
            return resType;
        }

        //Adds child nodes of the given parent type to the given tree node.
        private void CreateChildNodes(TreeNode parentNode, List<ResourceType> typesList,
         ResourceType parentType)
        {
            List<ResourceType> childTypes = typesList.Where(tuple => tuple.BaseType.Name == parentType.Name)
                .ToList();
            foreach (ResourceType childType in childTypes)
            {
                TreeNode childNode = new TreeNode(childType.Name, childType.Id.ToString());
                parentNode.ChildNodes.Add(childNode);
                CreateChildNodes(childNode, typesList, childType);
            }
        }

        private List<ResourceType> GetResourceTypeList()
        {
            List<ResourceType> resTypeList = null;
            using (ResourceDataAccess dataAccess = new ResourceDataAccess(base.CreateContext()))
            {
                resTypeList = dataAccess.GetResourceTypes().ToList();
                resTypeList = resTypeList.Where(tuple => tuple.Name != "Identity").ToList();
                resTypeList = resTypeList.Where(tuple => tuple.Name != "Group").ToList();
                resTypeList = resTypeList.Where(tuple => tuple.Name != "CategoryNode").ToList();
            }
            if (string.IsNullOrEmpty(ResourceHierarchy))
            {
                resTypeList = resTypeList.Where(tuple => tuple.Name != "Tag").ToList();
                resTypeList = resTypeList.Where(tuple => tuple.Name != "CategoryNode").ToList();
                resTypeList = resTypeList.Where(tuple => tuple.Name != "Contact").ToList();
                resTypeList = resTypeList.Where(tuple => tuple.Name != "Person").ToList();
                resTypeList = resTypeList.Where(tuple => tuple.Name != "Organization").ToList();
            }
            else
            {
                resTypeList = resTypeList.Where(tuple => tuple.Name != "Tag").ToList();
            }
            return resTypeList;
        }

        /// <summary>
        /// Handles SelectedNodeChanged event. Raises the OnResourceTypeSelected event of this control.
        /// </summary>
        /// <param name="sender">Object which raised this event</param>
        /// <param name="e">Event arguments.</param>
        protected void ResourceTypesTreeView_SelectedNodeChanged(object sender, EventArgs e)
        {
            _resourceType = base.TreeView.SelectedNode.Text;
            if (OnResourceTypeSelected != null)
                OnResourceTypeSelected(this, new EventArgs());
        }
    }
}
