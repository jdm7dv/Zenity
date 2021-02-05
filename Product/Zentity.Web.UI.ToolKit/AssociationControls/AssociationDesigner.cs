// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System.ComponentModel;
using System.IO;
using System.Web.UI;
using System.Web.UI.Design;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class manages the design time rendering of Association control.
    /// </summary>
    public class AssociationDesigner : ControlDesigner
    {
        #region Member variables

        #region Private
        Association _association;
        #endregion Private

        #endregion Member variables

        #region Methods

        #region Public

        /// <summary>
        /// Initializes the control designer and loads the specified component.
        /// </summary>
        /// <param name="component">The control being designed.</param>
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            this._association = (Association)component;
        }

        /// <summary>
        /// Retrieves the HTML markup that is used to represent the Association control at design
        /// time.
        /// </summary>
        /// <returns>HTML code for Association control.</returns>
        public override string GetDesignTimeHtml()
        {
            if (_association != null)
            {
                StringWriter _cultureInfo = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                HtmlTextWriter _textWriter = new HtmlTextWriter(_cultureInfo);
                _association.RenderControl(_textWriter);
                return _cultureInfo.ToString();
            }

            return "<div />";
        }

        #endregion Public

        #endregion Methods
    }
}