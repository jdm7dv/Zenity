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
    /// Class for displaying control properties in specified defined category sections
    /// </summary>
    internal sealed class ZentityCategoryAttribute : CategoryAttribute
    {
        #region Constructors

        #region Internal

        internal ZentityCategoryAttribute(string category)
            : base(category)
        {
        }

        #endregion

        #endregion

        #region Properties

        #region Public

        public override object TypeId
        {
            get
            {
                return typeof(CategoryAttribute);
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Protected

        protected override string GetLocalizedString(string value)
        {
            string localizedString = base.GetLocalizedString(value);
            if (localizedString == null)
            {
                localizedString = ZentityAttributeResource.ResourceManager.GetString(value);
            }
            return localizedString;
        }

        #endregion

        #endregion
    }
}
