// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the parser.
    /// </summary>
    internal class Parser
    {
        #region Private Members

        private SearchTokens searchTokens;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Platform.Parser"/> class.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch tokens with.</param>
        public Parser(SearchTokens searchTokens)
        {
            if (searchTokens == null)
            {
                throw new ArgumentNullException("searchTokens");
            }
            this.searchTokens = searchTokens;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Parses input string and Zentity search tree.
        /// </summary>
        /// <param name="inputQuery">Search query.</param>
        /// <param name="resourceTypeFullName">Resource type full name.</param>
        /// <returns>Root node of Zentity search tree.</returns>
        public TreeNode Parse(string inputQuery, out string resourceTypeFullName)
        {
            if (String.IsNullOrEmpty(inputQuery))
            {
                throw new ArgumentNullException("inputQuery");
            }

            Utility.ValidateInputQuery(inputQuery);
            Tokenizer tokenizer = new Tokenizer(searchTokens);
            IEnumerable<TreeNode> tokens = tokenizer.GetTokens(inputQuery, out resourceTypeFullName);

            if (tokens.Count() == 0)
            {
                AllResources allResources = new AllResources();
                allResources.ResourceTypeFullName = resourceTypeFullName;
                return allResources;
            }
            else
            {
                Stack<TreeNode> postfixTokens = GetPostfix(tokens);
                return CreateTree(null, postfixTokens);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the specified token is operator.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// 	<c>true</c> if the specified token is operator; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsOperator(TreeNode token)
        {
            if (token.Type == NodeType.And
                || token.Type == NodeType.Or
                || token.Type == NodeType.Predicate
                || token.Type == NodeType.Not)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the postfix.
        /// </summary>
        /// <param name="tokenList">The token list.</param>
        /// <returns>A treenode stack.</returns>
        private static Stack<TreeNode> GetPostfix(IEnumerable<TreeNode> tokenList)
        {
            Stack<TreeNode> postfixStack = new Stack<TreeNode>();
            Stack<TreeNode> operatorStack = new Stack<TreeNode>();

            foreach (TreeNode token in tokenList)
            {
                if (IsOperator(token))
                {
                    HandleOperatorForPostfix(postfixStack, operatorStack, token);
                }
                else if (token.Type == NodeType.OpenRoundBracket)
                {
                    operatorStack.Push(token);
                }
                else if (token.Type == NodeType.CloseRoundBracket)
                {
                    PopOperatorsFromOperatorStack(postfixStack, operatorStack);
                }
                else // Operand i.e. Expression or Predicate Node or AllResourcesNode.
                {
                    postfixStack.Push(token);
                }
            }
            // Pop operator stack and push operators on Postfix stack.
            while (operatorStack.Count != 0)
            {
                postfixStack.Push(operatorStack.Pop());
            }
            return postfixStack;
        }

        /// <summary>
        /// Handles the operator for postfix.
        /// </summary>
        /// <param name="postfixStack">The postfix stack.</param>
        /// <param name="operatorStack">The operator stack.</param>
        /// <param name="token">The token.</param>
        private static void HandleOperatorForPostfix(
                                                    Stack<TreeNode> postfixStack, 
                                                    Stack<TreeNode> operatorStack, 
                                                    TreeNode token)
        {
            if (operatorStack.Count != 0 &&
                (GetPrecedence(token) <= GetPrecedence(operatorStack.Peek())))
            {
                while (operatorStack.Count != 0 &&
                (GetPrecedence(token) <= GetPrecedence(operatorStack.Peek())))
                {
                    postfixStack.Push(operatorStack.Pop());
                }
            }

            operatorStack.Push(token);
        }

        /// <summary>
        /// Pops the operators from operator stack.
        /// </summary>
        /// <param name="postfixStack">The postfix stack.</param>
        /// <param name="operatorStack">The operator stack.</param>
        private static void PopOperatorsFromOperatorStack(Stack<TreeNode> postfixStack, Stack<TreeNode> operatorStack)
        {
            while (operatorStack.Count != 0)
            {
                TreeNode stackTop = operatorStack.Pop();
                if (!(stackTop.Type == NodeType.OpenRoundBracket))
                {
                    postfixStack.Push(stackTop);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the precedence of the operator.
        /// </summary>
        /// <param name="operatorToken">The operator token.</param>
        /// <returns>Operator precedence.</returns>
        private static int GetPrecedence(TreeNode operatorToken)
        {
            if (operatorToken.Type == NodeType.And)
            {
                return (int)OperatorPrecedence.LogicalOperatorAnd;
            }
            if (operatorToken.Type == NodeType.Or)
            {
                return (int)OperatorPrecedence.LogicalOperatorOr;
            }
            if (operatorToken.Type == NodeType.Predicate)
            {
                return (int)OperatorPrecedence.PredicateOperator;
            }
            if (operatorToken.Type == NodeType.Not)
            {
                return (int)OperatorPrecedence.NotOperator;
            }
            return -1;
        }

        /// <summary>
        /// Creates the tree.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="postfixStack">The postfix stack.</param>
        /// <returns>A <see cref="TreeNode"/>.</returns>
        private static TreeNode CreateTree(TreeNode root, Stack<TreeNode> postfixStack)
        {
            if (root == null && postfixStack.Count != 0)
            {
                root = postfixStack.Pop();
                CreateTree(root, postfixStack);
            }
            else
            {
                CompoundOperator compoundRoot = root as CompoundOperator;
                if (compoundRoot != null)
                {
                    if (compoundRoot.RightChild == null && postfixStack.Count != 0)
                    {
                        TreeNode node = postfixStack.Pop();
                        if (node != null)
                        {
                            compoundRoot.RightChild = node;
                            CreateTree(node, postfixStack);
                        }
                    }
                    if (compoundRoot.LeftChild == null && postfixStack.Count != 0)
                    {
                        TreeNode node = postfixStack.Pop();
                        if (node != null)
                        {
                            compoundRoot.LeftChild = node;
                            CreateTree(node, postfixStack);
                        }
                    }
                }
            }
            return root;
        }

        #endregion

        #region Private Enums

        /// <summary>
        /// Represents the operator precedence.
        /// </summary>
        private enum OperatorPrecedence
        {
            /// <summary>
            /// Predicate operator.
            /// </summary>
            PredicateOperator = 1,

            /// <summary>
            /// Logical operator OR.
            /// </summary>
            LogicalOperatorOr = 2,

            /// <summary>
            /// Logical operator AND.
            /// </summary>
            LogicalOperatorAnd = 3,

            /// <summary>
            /// NOT operator.
            /// </summary>
            NotOperator = 4,
        }

        #endregion
    }
}