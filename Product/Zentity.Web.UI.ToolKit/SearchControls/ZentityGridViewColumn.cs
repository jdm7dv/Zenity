// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.UI;
using System.Security.Permissions;
using System.Web.UI.WebControls;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// This class maintains information about the columns to be displayed in the GridView.
    /// </summary>
    [Serializable]
    [ParseChildren(false), ToolboxItem(false), Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false), DefaultProperty("Text"), ControlBuilder(typeof(TableCellControlBuilder))]
    public class ZentityGridViewColumn
    {
        #region Member Variables
        private string _columnName;
        private string _headerText;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets column name.
        /// </summary>
        [DefaultValue(""), NotifyParentProperty(true)]
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        /// <summary>
        /// Gets or sets header text.
        /// </summary>
        [DefaultValue(""), NotifyParentProperty(true)]
        public string HeaderText
        {
            get { return _headerText; }
            set { _headerText = value; }
        }
        #endregion

        #region Constructors & finalizers

        /// <summary>
        /// Initializes a new instance of <see cref="ZentityGridViewColumn"/>.
        /// </summary>
        public ZentityGridViewColumn()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ZentityGridViewColumn"/>.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="headerText">Header text.</param>
        public ZentityGridViewColumn(string columnName, string headerText)
        {
            _columnName = columnName;
            _headerText = headerText;
        }

        #endregion

        #region Medhods

        /// <summary>
        /// Returns headerText if not null else base type function.
        /// </summary>
        /// <returns>Header Text.</returns>
        public override string ToString()
        {
            if(!string.IsNullOrEmpty(_headerText))
                return _headerText;

            return base.ToString();
        }

        /// <summary>
        /// Duplicate the current ZentityGridViewColumn object.
        /// </summary>
        /// <returns>An instance of <see cref="ZentityGridViewColumn"/>.</returns>
        public ZentityGridViewColumn Clone()
        {
            ZentityGridViewColumn col = new ZentityGridViewColumn();
            col.HeaderText = _headerText;
            col.ColumnName = _columnName;

            return col;
        }
        #endregion
    }
}
