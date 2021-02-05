// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    /// <summary>
    /// Contains all default configurations required by VisualExplorer
    /// </summary>
    public class Config
    {        
        /// <summary>
        /// Default edge thickness
        /// </summary>
        public const double EdgeThickness = 2.0;

        /// <summary>
        /// Default font size
        /// </summary>
        public const double FontSize = 10.0;

        /// <summary>
        /// Default edge click behavior
        /// </summary>
        public const bool IsEdgeClickable = true;

        /// <summary>
        /// Default graph behavior
        /// </summary>
        public const bool IsInteractive = true;

        /// <summary>
        /// Default Control Bar setting
        /// </summary>
        public const bool IsShowControlBar = true;

        /// <summary>
        /// Default Header setting
        /// </summary>
        public const bool IsShowHeader = false;

        /// <summary>
        /// Default Node radius
        /// </summary>
        public const double Radius = 25.0;

        /// <summary>
        /// Default Node scale
        /// </summary>
        public const double Scale = 1.0;

        /// <summary>
        /// Represents the singleton instance of Config class
        /// </summary>
        private static Config instance = new Config();        

        /// <summary>
        /// Gets the singleton instance of Config class
        /// </summary>
        public static Config Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
