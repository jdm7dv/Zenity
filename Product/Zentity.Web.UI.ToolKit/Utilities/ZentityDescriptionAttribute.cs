// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace Zentity.Web.UI.ToolKit
{

    #region Using Namespace

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.ComponentModel;
    using Zentity.Web.UI.ToolKit.Resources;

    #endregion

    /// <summary>
    /// Class for displaying control properties description.
    /// </summary>
    internal sealed class ZentityDescriptionAttribute : DescriptionAttribute
    {
        #region Member Variables

        #region Private

        private bool _replaced;

        #endregion

        #endregion

        #region Constructors

        #region Internal

        internal ZentityDescriptionAttribute(string description)
            : base(description)
        {
        }

        #endregion

        #endregion

        #region Properties

        #region Public

        public override string Description
        {
            get
            {
                if (!this._replaced)
                {
                    this._replaced = true;
                    base.DescriptionValue = ZentityAttributeResource.ResourceManager.GetString(base.Description);
                }
                return base.Description;
            }
        }

        public override object TypeId
        {
            get
            {
                return typeof(DescriptionAttribute);
            }
        }

        #endregion Public

        #endregion Properties
    }

}
