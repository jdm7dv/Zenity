// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Node type of the Zentity search tree node.
    /// </summary>
    internal enum NodeType
    {
        /// <summary>
        /// 'And' logical operator.
        /// </summary>
        And,
        /// <summary>
        /// 'Or' logical operator.
        /// </summary>
        Or,
        /// <summary>
        /// Predicate operator.
        /// </summary>
        Predicate,
        /// <summary>
        /// Predicate node.
        /// </summary>
        PredicateNode,
        /// <summary>
        /// Comparison operators like =,&lt;, &lt;=, &gt;, &gt;=.
        /// </summary>
        ComparisonOperator,
        /// <summary>
        /// WordEqual operator. 
        /// Indicates OPERAND1 contains word equal to OPERAND2.
        /// </summary>
        WordEqual,
        /// <summary>
        /// WordStartsWith operator. 
        /// Indicates OPERAND1 contains word starting with OPERAND2.
        /// </summary>
        WordStartsWith,
        /// <summary>
        /// Open Bracket.
        /// </summary>
        OpenRoundBracket,
        /// <summary>
        /// Close Bracket.
        /// </summary>
        CloseRoundBracket,
        /// <summary>
        /// Not operator.
        /// </summary>
        Not,
        /// <summary>
        /// All Resources Node.
        /// </summary>
        AllResources
    }
}
