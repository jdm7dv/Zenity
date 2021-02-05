// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.Design;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class manages the design time rendering of EntitySearch control.
    /// </summary>
    public class EntitySearchDesigner : ControlDesigner
    {
        #region Member variables

        #region Private
        EntitySearch _resourceSearch;
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
            this._resourceSearch = (EntitySearch)component;
        }

        /// <summary>
        /// Retrieves the HTML markup that is used to represent the EntitySearch control at design
        /// time.
        /// </summary>
        /// <returns>HTML code for Association control.</returns>
        public override string GetDesignTimeHtml()
        {
            if (_resourceSearch != null)
            {
                StringWriter _cultureInfo = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                HtmlTextWriter _textWriter = new HtmlTextWriter(_cultureInfo);
                _resourceSearch.RenderControl(_textWriter);
                return _cultureInfo.ToString();
            }

            return "<div />";
        }


        #endregion Public

        #endregion Methods
    }
}
