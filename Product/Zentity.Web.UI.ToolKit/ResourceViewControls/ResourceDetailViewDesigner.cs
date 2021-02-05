// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System.IO;
using System.Web.UI;
using System.Web.UI.Design;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class manages the design time rendering of <see cref="ResourceDetailView" /> control.
    /// </summary>
    public class ResourceDetailViewDesigner : ControlDesigner
    {
        #region Member variables

        #region Private

        ResourceDetailView _zentityResourceDetail;

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
            this._zentityResourceDetail = (ResourceDetailView)component;
        }

        /// <summary>
        /// Retrieves the HTML markup that is used to represent the <see cref="ZentityTable" /> 
        /// control at design time.
        /// </summary>
        /// <returns>HTML code for <see cref="ZentityTable" /> control.</returns>
        public override string GetDesignTimeHtml()
        {
            if (_zentityResourceDetail != null)
            {
                System.IO.StringWriter stringWriter = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                HtmlTextWriter textWriter = new HtmlTextWriter(stringWriter);

                _zentityResourceDetail.RenderControl(textWriter);

                return stringWriter.ToString();
            }

            return Constants.EmptyDivTag;
        }

        #endregion Public

        #endregion Methods
    }
}
