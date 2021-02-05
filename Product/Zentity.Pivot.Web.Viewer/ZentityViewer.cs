// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Pivot.Web
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Pivot;
    using Zentity.Pivot.Web.Viewer;

    /// <summary>
    /// ZentityViewer control: This contains a custom action to be shown on each tile.
    /// </summary>
    public class ZentityViewer : PivotViewer
    {
        /// <summary>
        /// Represents the identifier for the custom action
        /// </summary>
        public const string VisualExplorerActionId = "2710B03A-530E-490A-8CF2-628F125D9141";

        /// <summary>
        /// Represents the name of the custom action
        /// </summary>
        private const string VisualExplorer = "Visual Explorer";

        /// <summary>
        /// Overridden GetCustomActionsForItem method to add custom action
        /// </summary>
        /// <param name="itemId">Identifier of the tile in the collection</param>
        /// <returns>List of CustomAction loaded in the control</returns>
        protected override List<CustomAction> GetCustomActionsForItem(string itemId)
        {
            List<CustomAction> actions = base.GetCustomActionsForItem(itemId);
            if (actions == null)
            {
                actions = new List<CustomAction>();
            }

            CustomAction action = new CustomAction(
                                            VisualExplorer,
                                            new Uri(ViewerResources.VisualExplorerImageRelativePath, UriKind.Relative),
                                            ViewerResources.NavigateToVisualExplorer,
                                            VisualExplorerActionId);
            actions.Add(action);
            return actions;
        }
    }
}
