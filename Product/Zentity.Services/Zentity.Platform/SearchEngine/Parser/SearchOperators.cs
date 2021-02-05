// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Represents search operators.
    /// </summary>
    internal static class SearchOperators
    {
        #region Private Fields

        private static string[] conditionalOperators = 
            { 
                SearchConstants.GREATER_THAN_OR_EQUAL, 
                SearchConstants.LESS_THAN_OR_EQUAL, 
                SearchConstants.GREATER_THAN, 
                SearchConstants.LESS_THAN, 
                SearchConstants.EQUAL_TO,
                SearchConstants.NOT_EQUAL_TO,
            };

        private static string[] logicalOperators = 
            { 
                SearchConstants.AND, 
                SearchConstants.OR 
            };

        #endregion

        #region Properties

        /// <summary>
        /// Gets conditional operators.
        /// </summary>
        public static string[] ConditionalOperators
        {
            get { return conditionalOperators; }
        }

        /// <summary>
        /// Gets logical operators.
        /// </summary>
        public static string[] LogicalOperators
        {
            get { return logicalOperators; }
        }

        #endregion
    }
}