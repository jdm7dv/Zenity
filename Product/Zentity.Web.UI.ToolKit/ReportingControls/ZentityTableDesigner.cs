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
    /// This class manages the design time rendering of <see cref="ZentityTable" /> control.
    /// </summary>
    public class ZentityTableDesigner : ControlDesigner
    {
        #region Member variables

        #region Private

        ZentityTable _zentityTable;

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
            this._zentityTable = (ZentityTable)component;
        }

        /// <summary>
        /// Retrieves the HTML markup that is used to represent the <see cref="ZentityTable" /> 
        /// control at design time.
        /// </summary>
        /// <returns>HTML code for <see cref="ZentityTable" /> control.</returns>
        public override string GetDesignTimeHtml()
        {
            if (_zentityTable != null)
            {
                StringWriter stringWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                HtmlTextWriter textWriter = new HtmlTextWriter(stringWriter);

                _zentityTable.RenderControl(textWriter);

                return stringWriter.ToString();
            }

            return Constants.EmptyDivTag;
        }

        #endregion Public

        #endregion Methods
    }
}