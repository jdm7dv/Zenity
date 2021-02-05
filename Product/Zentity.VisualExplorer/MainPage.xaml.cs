// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using guanximap.utils;

    /// <summary>
    /// Code behind of MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl
    {
        /// <summary>
        /// Singleton instance of MainPage class
        /// </summary>
        private static MainPage instance;

        /// <summary>
        /// Initial Parameters
        /// </summary>
        private IDictionary<string, string> initParams;

        /// <summary>
        /// Initializes a new instance of the MainPage class
        /// </summary>
        /// <param name="dict">Dictionary of initial parameter values</param>
        public MainPage(IDictionary<string, string> dict)
        {
            this.InitializeComponent();
            instance = this;
            this.initParams = dict;
            this.SetInitParams();
            Controller.Instance.Init();
        }

        /// <summary>
        /// Gets or sets the singleton instance of MainPage class
        /// </summary>
        public static MainPage Instance
        {
            get
            {
                return instance;
            }

            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Set initial parameters
        /// </summary>
        public void SetInitParams()
        {
            ObjectUtils.initParamsToObject(this.initParams, Config.Instance);
        }
    }
}
