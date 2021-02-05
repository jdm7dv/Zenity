// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Zentity.Core;
    using Zentity.Platform.Properties;

    /// <summary>
    /// Represents the tokenizer.
    /// </summary>
    internal class Tokenizer
    {
        #region Private Members

        private SearchTokens searchTokens;
        private bool hasUserSpecifiedResourceType;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Zentity.Platform.Tokenizer"/> class.
        /// </summary>
        /// <param name="searchTokens"><see cref="SearchTokens" /> instance to fetch 
        /// tokens with.</param>
        public Tokenizer(SearchTokens searchTokens)
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
        /// Splits the given input string into tokens.
        /// </summary>
        /// <param name="searchQuery">Input string to be tokenized.</param>
        /// <param name="resourceTypeFullName">Resource type full name.</param>
        /// <returns>List of tokens.</returns>
        public IEnumerable<TreeNode> GetTokens(string searchQuery,
            out string resourceTypeFullName)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                throw new ArgumentNullException("searchQuery");
            }

            resourceTypeFullName = FetchSubjectCriteriaResourceType(ref searchQuery);

            if (!String.IsNullOrEmpty(resourceTypeFullName))
            {
                hasUserSpecifiedResourceType = true;
            }
            else
            {
                hasUserSpecifiedResourceType = false;
                resourceTypeFullName = SearchConstants.DEFAULTRESOURCETYPE;
            }

            searchQuery = searchQuery.Trim();
            List<TreeNode> tokens = new List<TreeNode>();

            while (!String.IsNullOrEmpty(searchQuery))
            {
                if (StartsWithNotOperator(searchQuery))
                {
                    AddNotNodes(tokens, ref searchQuery);
                }
                else if (StartsWithLogicalOperator(searchQuery))
                {
                    HandleLogicalOperator(ref searchQuery, tokens);
                }
                else if (StartsWithBracket(searchQuery))
                {
                    HandleRoundBracket(ref searchQuery, tokens);
                }
                else if (StartsWithSpecialPropertyToken(searchQuery))
                {
                    tokens.AddRange(HandlePropertyCondition(
                        ref searchQuery, resourceTypeFullName,
                        ExpressionTokenType.SpecialToken));
                }
                else if (StartsWithPropertyToken(searchQuery, ref resourceTypeFullName))
                {
                    tokens.AddRange(HandlePropertyCondition(
                        ref searchQuery, resourceTypeFullName,
                        ExpressionTokenType.PropertyToken));
                }
                else if (StartsWithPredicateToken(searchQuery))
                {
                    tokens.AddRange(HandlePredicateCondition(ref searchQuery));
                }
                else // Starts with a value with/without quotes.
                {
                    tokens.AddRange(
                        HandleImplicitCondition(ref searchQuery, resourceTypeFullName));
                }
                searchQuery = searchQuery.Trim();
            }

            TrimDefaultLogicalOperator(tokens);
            RemoveConsecutiveNotTokens(tokens);
            // Adds the resource type to all subject properties.
            AttachResourceType(tokens, resourceTypeFullName);
            return tokens;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the conditional operator if the input starts with conditional operator.
        /// </summary>
        /// <param name="inputQuery">Input query.</param>
        /// <returns>Conditional operator.</returns>
        private static string ExtractConditionalOperatorFromValue(ref string inputQuery)
        {
            if (!String.IsNullOrEmpty(inputQuery))
            {
                foreach (string conditionalOperator in SearchOperators.ConditionalOperators)
                {
                    if (inputQuery.StartsWith(conditionalOperator, StringComparison.Ordinal))
                    {
                        inputQuery =
                            inputQuery
                            .Remove(0, conditionalOperator.Length)
                            .TrimStart(SearchConstants.SPACE[0]);
                        return conditionalOperator;
                    }
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Handles the implicit condition.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <returns>List of tree nodes.</returns>
        private IEnumerable<TreeNode> HandleImplicitCondition(ref string searchQuery, string resourceTypeFullName)
        {
            List<TreeNode> tokens = new List<TreeNode>();

            string token = GetFirstToken(ref searchQuery);

            tokens.Add(
                GetExpression(SearchConstants.ASTERIX, token,
                ExpressionTokenType.ImplicitPropertiesToken, resourceTypeFullName));
            // Add default operator.
            tokens.Add(new AndOperator());

            return tokens;
        }

        /// <summary>
        /// Handles the round bracket.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="tokens">The tokens.</param>
        private static void HandleRoundBracket(ref string searchQuery, List<TreeNode> tokens)
        {
            string token = GetFirstToken(ref searchQuery);

            if (String.Equals(token, SearchConstants.CLOSE_ROUND_BRACKET))
            {
                if (tokens.Count > 0)
                {
                    // Remove default logical operator.
                    if (tokens[tokens.Count - 1].Type == NodeType.And)
                    {
                        tokens.RemoveAt(tokens.Count - 1);
                    }
                }
                tokens.Add(new CloseRoundBracket());

                // Add default operator.
                tokens.Add(new AndOperator());
            }
            else
            {
                tokens.Add(new OpenRoundBracket());
            }
        }

        /// <summary>
        /// Handles the logical operator.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="tokens">The tokens.</param>
        private static void HandleLogicalOperator(ref string searchQuery, List<TreeNode> tokens)
        {
            string token = GetFirstToken(ref searchQuery);

            if (tokens.Count > 0)
            {
                // Remove default logical operator.
                if (tokens[tokens.Count - 1].Type == NodeType.And
                    || tokens[tokens.Count - 1].Type == NodeType.Or)
                {
                    tokens.RemoveAt(tokens.Count - 1);
                }
            }
            if (String.Equals(token, SearchConstants.AND))
            {
                tokens.Add(new AndOperator());
            }
            else //OR
            {
                tokens.Add(new OrOperator());
            }
        }

        /// <summary>
        /// Handles the property condition.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <param name="expressionTokenType">Type of the expression token.</param>
        /// <returns>List of tree nodes.</returns>
        private IEnumerable<TreeNode> HandlePropertyCondition(
                                                            ref string searchQuery, 
                                                            string resourceTypeFullName,
                                                            ExpressionTokenType expressionTokenType)
        {
            //Fetch property name.
            string propertyName =
                searchQuery.Substring(0,
                searchQuery.IndexOf(SearchConstants.COLON, StringComparison.Ordinal));

            searchQuery = searchQuery.Remove(0, propertyName.Length + 1).Trim();

            //Fetch property condition.
            string condition = GetPropertyCondition(ref searchQuery);

            if (String.IsNullOrEmpty(
                condition.Trim(
                SearchConstants.OPEN_ROUND_BRACKET[0],
                SearchConstants.CLOSE_ROUND_BRACKET[0],
                SearchConstants.SPACE[0])))
            {
                throw new SearchException(
                    String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SEARCH_VALUE_NOT_SPECIFIED, propertyName));
            }

            List<TreeNode> tokens = new List<TreeNode>();

            tokens.AddRange(
                TokenizePropertyCondition(
                    propertyName, condition, expressionTokenType,
                    resourceTypeFullName));
            // Add default operator.
            tokens.Add(new AndOperator());

            return tokens;
        }

        /// <summary>
        /// Removes the last token from the list if its a 'default' logical operator.
        /// </summary>
        /// <param name="tokens">Token list.</param>
        private static void TrimDefaultLogicalOperator(List<TreeNode> tokens)
        {
            if (tokens.Count > 0)
            {
                // Remove default logical operator, if occurred at the end.
                if (tokens[tokens.Count - 1].Type == NodeType.And)
                {
                    tokens.RemoveAt(tokens.Count - 1);
                }
            }
        }

        /// <summary>
        /// Handles the predicate condition.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>List of tree nodes.</returns>
        private IEnumerable<TreeNode> HandlePredicateCondition(ref string searchQuery)
        {
            string predicateName =
                searchQuery.Substring(0, searchQuery.IndexOf(SearchConstants.COLON[0]));
            searchQuery =
                searchQuery.Remove(0, predicateName.Length + 1).Trim(); // + 1 to trim ':'

            if (String.IsNullOrEmpty(searchQuery))
            {
                throw new SearchException(
                    String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SEARCH_PREDICATE_CONDITION_MISSING,
                    predicateName));
            }

            string notOperator = ExtractNotOperators(ref searchQuery);

            string conditionString = String.Empty;
            if (searchQuery.StartsWith(
                SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal))
            {
                conditionString += GetFirstBracketBlock(ref searchQuery);
            }
            else if (!searchQuery.StartsWith(
                SearchConstants.CLOSE_ROUND_BRACKET, StringComparison.Ordinal))
            {
                conditionString += SearchConstants.SPACE + GetFirstToken(ref searchQuery);
            }

            string resourceTypeFullName;

            IEnumerable<TreeNode> conditionTokens =
                GetTokens(notOperator + conditionString, out resourceTypeFullName);

            if (conditionTokens.Count() == 0)
            {
                throw new SearchException(
                    String.Format(CultureInfo.CurrentCulture,
                    Resources.SEARCH_PREDICATE_CONDITION_MISSING,
                    predicateName));
            }

            List<TreeNode> predicateConditionTokens = new List<TreeNode>();

            // Adding an opening bracket to put entire predicate condition inside '(' & ')'
            predicateConditionTokens.Add(new OpenRoundBracket());

            // Add default predicate node.
            predicateConditionTokens.Add(new PredicateNode(predicateName));

            // Add default predicate operator node.
            predicateConditionTokens.Add(new PredicateOperator());
            predicateConditionTokens.AddRange(conditionTokens);
            predicateConditionTokens.Add(new CloseRoundBracket());
            // Add default operator.
            predicateConditionTokens.Add(new AndOperator());
            return predicateConditionTokens;
        }

        /// <summary>
        /// Attaches the specified resource type only to property tokens, 
        /// if property token doesn't have one.
        /// </summary>
        /// <param name="tokenList">List of tokens.</param>
        /// <param name="resourceTypeFullName">Resource type full name.</param>
        private static void AttachResourceType(
            IEnumerable<TreeNode> tokenList, string resourceTypeFullName)
        {
            foreach (TreeNode token in tokenList)
            {
                AllResources allResourcesNode = token as AllResources;
                if (allResourcesNode != null)
                {
                    if (String.IsNullOrEmpty(allResourcesNode.ResourceTypeFullName))
                    {
                        allResourcesNode.ResourceTypeFullName = resourceTypeFullName;
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the first resource type and removes the rest from 
        /// subject criteria only.
        /// </summary>
        /// <param name="conditionString">Search Query.</param>
        /// <returns>Resource Type.</returns>
        private string FetchSubjectCriteriaResourceType(
            ref string conditionString)
        {
            // Replace Quoted blocks with Guids
            Dictionary<Guid, string> quotedConditions =
                        FetchQuotedConditions(conditionString);

            conditionString =
                ReplaceConditionsWithGuid(conditionString, quotedConditions,
                SearchConstants.SPACE, SearchConstants.SPACE);

            // Replace Predicate blocks with Guids so that 
            // resource type of object criteria does not get trimmed.
            Dictionary<Guid, string> predicateConditions =
                FetchPredicateConditions(ref conditionString);

            conditionString =
                ReplaceConditionsWithGuid(conditionString, predicateConditions,
                SearchConstants.OPEN_ROUND_BRACKET, SearchConstants.CLOSE_ROUND_BRACKET);

            string resourceType = FetchResourceType(ref conditionString);

            conditionString =
                ReplaceGuidWithConditions(conditionString, predicateConditions,
                SearchConstants.OPEN_ROUND_BRACKET, SearchConstants.CLOSE_ROUND_BRACKET);

            conditionString =
                ReplaceGuidWithConditions(conditionString, quotedConditions,
                SearchConstants.SPACE, String.Empty);

            conditionString = CleanInputQuery(conditionString);
            
            if (!String.IsNullOrEmpty(resourceType))
            {
                // Note: for cases like resourceType: "ABC", quoted string "ABC" 
                // would get replaced by a guid. Hence replace it with original value.
                foreach (KeyValuePair<Guid, string> key in quotedConditions)
                {
                    if (String.Equals(key.Key.ToString(), resourceType))
                    {
                        resourceType = quotedConditions[new Guid(resourceType)];
                        resourceType = TrimQuotes(resourceType, false, true);
                        break;
                    }
                }

                string usersSpecifiedResourceType = resourceType;
                // Get resource type full name.
                resourceType = searchTokens.FetchResourceTypeFullName(resourceType);

                IEnumerable<string> excludedResourceTypeFullNames =
                        SearchTokens.FetchExcludedResourceTypeFullNames();

                if (excludedResourceTypeFullNames.Contains(resourceType.ToLower()))
                {
                    throw new SearchException(
                        String.Format(CultureInfo.CurrentCulture,
                        Resources.SEARCH_INVALID_RESOURCETYPE, usersSpecifiedResourceType));
                }

                return resourceType;
            }

            return String.Empty;
        }

        /// <summary>
        /// Fetches the predicate conditions.
        /// </summary>
        /// <param name="conditionString">The condition string.</param>
        /// <returns>Dictionary of predicate conditions.</returns>
        private Dictionary<Guid, string> FetchPredicateConditions(ref string conditionString)
        {
            StringBuilder newCondition = new StringBuilder();

            Dictionary<Guid, string> predicateConditions = new Dictionary<Guid, string>();
            while (!String.IsNullOrEmpty(conditionString))
            {
                Match spaceMatch = Regex.Match(conditionString, SearchConstants.REGEX_SPACES);
                if (spaceMatch.Success)
                {
                    newCondition.Append(spaceMatch.Value);
                    // Remove spaces from front.
                    conditionString = conditionString.Substring(spaceMatch.Value.Length);
                }

                if (StartsWithPredicateToken(conditionString))
                {
                    StringBuilder predicateCondition = new StringBuilder();

                    predicateCondition.Append(
                        conditionString
                        .Substring(0, conditionString.IndexOf(SearchConstants.COLON[0]) + 1));

                    conditionString = conditionString.Remove(0, predicateCondition.Length);

                    // Add spaces from input to output string.
                    spaceMatch = Regex.Match(conditionString, SearchConstants.REGEX_SPACES);
                    if (spaceMatch.Success)
                    {
                        predicateCondition.Append(spaceMatch.Value);
                        conditionString =
                            conditionString.Substring(spaceMatch.Value.Length);
                    }

                    string predicateCriteria = String.Empty;
                    string notOperator = ExtractNotOperators(ref conditionString);

                    if (conditionString.StartsWith(
                        SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal))
                    {
                        predicateCriteria = GetFirstBracketBlock(ref conditionString);
                    }
                    else if (!conditionString.StartsWith(
                        SearchConstants.CLOSE_ROUND_BRACKET, StringComparison.Ordinal))
                    {
                        predicateCriteria = GetFirstToken(ref conditionString);
                    }

                    if (!String.IsNullOrEmpty(notOperator))
                    {
                        predicateCondition.Append(notOperator + SearchConstants.SPACE);
                    }
                    predicateCondition.Append(predicateCriteria);

                    newCondition.Append(predicateCondition.ToString());
                    predicateConditions.Add(Guid.NewGuid(), predicateCondition.ToString());
                }
                else if (StartsWithSpecialPropertyToken(conditionString))
                {
                    // Fetch property name.
                    string propertyName =
                        conditionString.Substring(0,
                        conditionString.IndexOf(SearchConstants.COLON, StringComparison.Ordinal));

                    newCondition.Append(propertyName + SearchConstants.COLON);

                    conditionString = conditionString.Remove(0, propertyName.Length + 1);

                    // Add spaces from input to output string.
                    spaceMatch = Regex.Match(conditionString, SearchConstants.REGEX_SPACES);
                    if (spaceMatch.Success)
                    {
                        newCondition.Append(spaceMatch.Value);
                        conditionString =
                            conditionString.Substring(spaceMatch.Value.Length);
                    }
                }
                else
                {
                    newCondition.Append(GetFirstToken(ref conditionString));
                }
            }
            conditionString = newCondition.ToString();
            return predicateConditions;
        }

        /// <summary>
        /// Replaces the GUID with conditions.
        /// </summary>
        /// <param name="conditionString">The condition string.</param>
        /// <param name="guidConditionPairs">The GUID condition pairs.</param>
        /// <param name="paddedStartCharacter">The padded start character.</param>
        /// <param name="paddedEndCharacter">The padded end character.</param>
        /// <returns>Conditional string with the GUID's replaced with conditions.</returns>
        private static string ReplaceGuidWithConditions(
                                                    string conditionString, 
                                                    Dictionary<Guid, string> guidConditionPairs,
                                                    string paddedStartCharacter, 
                                                    string paddedEndCharacter)
        {
            if (!String.IsNullOrEmpty(conditionString))
            {
                foreach (KeyValuePair<Guid, string> guidConditionPair in guidConditionPairs)
                {
                    conditionString =
                        conditionString
                        .Replace(paddedStartCharacter + guidConditionPair.Key.ToString() + paddedEndCharacter,
                        guidConditionPair.Value);
                }
            }
            return conditionString;
        }

        /// <summary>
        /// Replaces the conditions with GUID.
        /// </summary>
        /// <param name="conditionString">The condition string.</param>
        /// <param name="guidConditionPairs">The GUID condition pairs.</param>
        /// <param name="padStartCharacter">The pad start character.</param>
        /// <param name="padEndCharacter">The pad end character.</param>
        /// <returns>Condition string with the conditions replaced by GUID's.</returns>
        private static string ReplaceConditionsWithGuid(
                                                    string conditionString, 
                                                    Dictionary<Guid, string> guidConditionPairs, 
                                                    string padStartCharacter, 
                                                    string padEndCharacter)
        {

            if (!String.IsNullOrEmpty(conditionString))
            {
                foreach (KeyValuePair<Guid, string> guidConditionPair in guidConditionPairs)
                {
                    conditionString =
                        conditionString
                        .Replace(guidConditionPair.Value,
                        padStartCharacter + guidConditionPair.Key.ToString() + padEndCharacter);
                }
            }
            return conditionString;
        }

        /// <summary>
        /// Removes empty brackets blocks like '( )' and '( LOGICAL_OERATOR )'
        /// and also AND/OR from start of the input query.
        /// </summary>
        /// <param name="inputQuery">Search query.</param>
        /// <returns>Search query with out empty blocks.</returns>
        private static string CleanInputQuery(string inputQuery)
        {
            Dictionary<Guid, string> quotedConditions = FetchQuotedConditions(inputQuery);

            inputQuery = ReplaceConditionsWithGuid(inputQuery, quotedConditions,
                String.Empty, String.Empty);

            inputQuery = RemoveUnusedBracketBlocks(inputQuery);

            // Remove AND/OR from start.
            inputQuery = RemoveUnusedLogicalAndNotOperators(inputQuery);

            inputQuery = ReplaceGuidWithConditions(inputQuery, quotedConditions,
                                String.Empty, String.Empty);

            return inputQuery;
        }

        /// <summary>
        /// Fetches the quoted conditions.
        /// </summary>
        /// <param name="inputQuery">The input query.</param>
        /// <returns>Dictionary of quoted conditions.</returns>
        private static Dictionary<Guid, string> FetchQuotedConditions(string inputQuery)
        {
            string tempInputQuery = inputQuery;
            Dictionary<Guid, string> quotedConditions = new Dictionary<Guid, string>();
            while (!String.IsNullOrEmpty(tempInputQuery))
            {
                string quotedString = Utility.GetQuotedString(ref tempInputQuery);
                if (!String.IsNullOrEmpty(quotedString))
                {
                    quotedConditions.Add(Guid.NewGuid(), quotedString);
                }
                else
                {
                    // if query starts with \' or \" then remove quote along 
                    // with its escape character.
                    if (tempInputQuery.Length >= 2 && tempInputQuery.StartsWith(
                        SearchConstants.ESCAPE_CHARACTER, StringComparison.Ordinal))
                    {
                        if (Char.Equals(tempInputQuery[1], SearchConstants.DOUBLE_QUOTE[0])
                            || Char.Equals(tempInputQuery[1], SearchConstants.SINGLE_QUOTE[0]))
                        {
                            tempInputQuery = tempInputQuery.Remove(0, 2);
                        }
                        else
                        {
                            tempInputQuery = tempInputQuery.Remove(0, 1);
                        }
                    }
                    else if(tempInputQuery.Length > 0)
                    {
                        tempInputQuery = tempInputQuery.Remove(0, 1);
                    }
                }
            }
            return quotedConditions;
        }

        /// <summary>
        /// Removes the unused bracket blocks.
        /// </summary>
        /// <param name="inputQuery">The input query.</param>
        /// <returns>Query with the bracket blocks removed.</returns>
        private static string RemoveUnusedBracketBlocks(string inputQuery)
        {
            // Removes'( )', '(AND/OR/NOT)', '(AND OR OR NOT...)'
            if (!String.IsNullOrEmpty(inputQuery))
            {
                Match match;
                do
                {
                    match = Regex.Match(inputQuery, SearchConstants.REGEX_UNUSED_BLOCKS);

                    inputQuery =
                        Regex.Replace(inputQuery, SearchConstants.REGEX_UNUSED_BLOCKS,
                        String.Empty).Trim();
                } while (match.Success);
            }
            return inputQuery;
        }

        /// <summary>
        /// Fetches the first resource type and removes the rest from inout query.
        /// </summary>
        /// <param name="inputQuery">Input query.</param>
        /// <returns>Resource Type.</returns>
        private static string FetchResourceType(ref string inputQuery)
        {
            string resourceType = String.Empty;
            bool isFirstOccurence = true;
            while (!String.IsNullOrEmpty(inputQuery))
            {
                // Get 1st resource type.
                Match match =
                    Regex.Match(inputQuery, SearchConstants.REGEX_RESOURCE_TYPE,
                    RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    if (isFirstOccurence)
                    {
                        // Extract resource type.
                        resourceType =
                            match.Value.Substring(
                            match.Value.IndexOf(SearchConstants.COLON, StringComparison.Ordinal) + 1);

                        resourceType = TrimQuotes(resourceType, false, true);
                            isFirstOccurence = false;
                    }

                    
                    // Trim resource type conditions from the input query.
                    if (match.Value.StartsWith(SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal)
                        || match.Value.StartsWith(SearchConstants.CLOSE_ROUND_BRACKET, StringComparison.Ordinal)
                        || match.Value.StartsWith(SearchConstants.SPACE, StringComparison.Ordinal))
                    {
                        inputQuery = inputQuery.Remove(match.Index + 1 , match.Value.Length - 1);
                    }
                    else
                    {
                        inputQuery = inputQuery.Remove(match.Index, match.Value.Length);
                    }
                }
                else
                {
                    break;
                }
            }
            return resourceType;
        }

        /// <summary>
        /// Removes the unused logical and NOT operators.
        /// </summary>
        /// <param name="inputQuery">The input query.</param>
        /// <returns>Query with logical and NOT operators removed.</returns>
        private static string RemoveUnusedLogicalAndNotOperators(string inputQuery)
        {
            bool matchFound;
            do
            {
                matchFound = false;

                // Remove from start.
                matchFound |= RemoveOperatorsFromStart(ref inputQuery);

                // Remove from end.
                matchFound |= RemoveOperatorsFromEnd(ref inputQuery);

                // Replace '(AND' and '(OR' with '('
                matchFound |= Replace(ref inputQuery,
                    SearchConstants.REGEX_OPEN_BRACKET_FOLLOWED_BY_LOGICAL_OPERATOR,
                    SearchConstants.OPEN_ROUND_BRACKET);

                // Replace 'AND)' , 'OR)' and 'NOT)' with ')'
                matchFound |= Replace(ref inputQuery,
                    SearchConstants.REGEX_LOGICAL_AND_NOT_OPERATOR_FOLLOWED_BY_CLOSE_BRACKET,
                    SearchConstants.CLOSE_ROUND_BRACKET);

                // Merge operators.
                matchFound |= MergeOperators(ref inputQuery);

            } while (!String.IsNullOrEmpty(inputQuery) && matchFound);

            return inputQuery;
        }

        /// <summary>
        /// Merges operators. i.e. AND OR = OR, OR OR = OR, NOT AND = AND etc.
        /// </summary>
        /// <param name="inputQuery">Input Query.</param>
        /// <returns>true, if atleast one match found else false.</returns>
        private static bool MergeOperators(ref string inputQuery)
        {
            if (String.IsNullOrEmpty(inputQuery))
            {
                return false;
            }
            Match match;
            bool matchFound = false;
            do
            {
                match = Regex.Match(inputQuery,
                    SearchConstants.REGEX_OPERATOR_FOLLOWED_OPERATOR);

                if (match.Success)
                {
                    inputQuery =
                        inputQuery.Remove(match.Index, match.Length).Trim();
                    // i.e. 'OR AND', 'NOT AND', 'AND AND' replace with 'AND'
                    if (match.Value.EndsWith(SearchConstants.AND, StringComparison.Ordinal))
                    {
                        inputQuery =
                            inputQuery.Insert(match.Index, SearchConstants.SPACE + SearchConstants.AND + SearchConstants.SPACE).Trim();
                    }
                    else
                    {
                        inputQuery =
                            inputQuery.Insert(match.Index, SearchConstants.SPACE + SearchConstants.OR + SearchConstants.SPACE).Trim();
                    }
                    matchFound = true;
                }

            } while (match.Success);
            return matchFound;
        }

        /// <summary>
        /// Within a specified input string, replaces all strings that match a specified
        /// regular expression with a specified replacement string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="replacement">The replacement string.</param>
        /// <returns>true, if atleast one match found else false.</returns>
        private static bool Replace(ref string input, string pattern, string replacement)
        {
            if (String.IsNullOrEmpty(input))
            {
                return false;
            }
            Match match;
            bool matchFound = false;
            do
            {
                match = Regex.Match(input, pattern);
                if (match.Success)
                {
                    input = Regex.Replace(input, pattern, replacement).Trim();
                    matchFound = true;
                }
            } while (match.Success);

            return matchFound;
        }

        /// <summary>
        /// Removes unnecessary logical operators and 'NOT' operator from the end of the input.
        /// </summary>
        /// <param name="inputQuery">Input Query.</param>
        /// <returns>true, if atleast one match found else false.</returns>
        private static bool RemoveOperatorsFromEnd(ref string inputQuery)
        {
            if (String.IsNullOrEmpty(inputQuery))
            {
                return false;
            }
            Match match;
            bool matchFound = false;
            do
            {
                match = Regex.Match(inputQuery,
                    SearchConstants.REGEX_UNUSED_LOGICAL_AND_NOT_OPERATORS_AT_END);
                if (match.Success)
                {
                    inputQuery =
                        inputQuery.Remove(match.Index, match.Length).Trim();
                    if (match.Value.Contains(SearchConstants.CLOSE_ROUND_BRACKET))
                    {
                        // For '...)AND' ,'...)OR' and '...)NOT'
                        inputQuery += SearchConstants.CLOSE_ROUND_BRACKET;
                    }
                    else if (match.Value.Contains(SearchConstants.COLON))
                    {
                        // For '...:AND' ,'...:OR' and '...:NOT'
                        inputQuery += SearchConstants.COLON;
                    }
                    matchFound = true;
                }
            } while (match.Success);

            return matchFound;
        }

        /// <summary>
        /// Removes unnecessary logical operators from the start of the input.
        /// </summary>
        /// <param name="inputQuery">Input Query.</param>
        /// <returns>true, if atleast one match found else false.</returns>
        private static bool RemoveOperatorsFromStart(ref string inputQuery)
        {
            if (String.IsNullOrEmpty(inputQuery))
            {
                return false;
            }
            Match match;
            bool matchFound = false;
            do
            {
                match = Regex.Match(inputQuery,
                    SearchConstants.REGEX_UNUSED_LOGICAL_OPERATORS_AT_START);
                if (match.Success)
                {
                    inputQuery =
                        inputQuery.Remove(match.Index, match.Length).Trim();
                    if (match.Value.Contains(SearchConstants.OPEN_ROUND_BRACKET))
                    {
                        // For 'AND(...' and 'OR(...'
                        inputQuery =
                            inputQuery.Insert(match.Index, SearchConstants.OPEN_ROUND_BRACKET).Trim();
                    }
                    matchFound = true;
                }
            } while (match.Success);

            return matchFound;
        }

        /// <summary>
        /// Tokenizes the property condition.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyCondition">The property condition.</param>
        /// <param name="expressionTokenType">Type of the expression token.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <returns>List of tree nodes.</returns>
        private IEnumerable<TreeNode> TokenizePropertyCondition(
                                                            string propertyName, 
                                                            string propertyCondition,
                                                            ExpressionTokenType expressionTokenType,
                                                            string resourceTypeFullName)
        {
            List<TreeNode> tokens = new List<TreeNode>();

            while (!String.IsNullOrEmpty(propertyCondition))
            {
                propertyCondition = propertyCondition.Trim();

                if (StartsWithLogicalOperator(propertyCondition))
                {
                    HandleLogicalOperator(ref propertyCondition, tokens);
                }
                else if (StartsWithBracket(propertyCondition))
                {
                    HandleRoundBracket(ref propertyCondition, tokens);
                }
                else // If starts with a value.
                {
                    // If 'propertyCondition' starts with 'NOT'
                    AddNotNodes(tokens, ref propertyCondition);

                    if (propertyCondition.StartsWith(
                        SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal))
                    {
                        tokens.AddRange(
                            TokenizePropertyCondition(
                            propertyName,
                            GetFirstBracketBlock(ref propertyCondition),
                            expressionTokenType, resourceTypeFullName));
                    }
                    else
                    {
                        string value = FetchFirstCondition(ref propertyCondition);

                        // To treat '!=' operator same as that on 'NOT' operator.
                        if (value.StartsWith(SearchConstants.NOT_EQUAL_TO, StringComparison.Ordinal)
                            && !String.Equals(value, SearchConstants.NOT_EQUAL_TO, StringComparison.Ordinal)
                            )
                        {
                            value = value.Remove(0, SearchConstants.NOT_EQUAL_TO.Length);
                            value = SearchConstants.NOT + SearchConstants.SPACE + SearchConstants.EQUAL_TO + SearchConstants.SPACE + value;
                            AddNotNodes(tokens, ref value);
                        }
                        tokens.Add(
                                GetExpression(propertyName, value, expressionTokenType, resourceTypeFullName));
                    }
                    // Add default operator.
                    tokens.Add(new AndOperator());
                }
            }
            TrimDefaultLogicalOperator(tokens);
            return tokens;
        }

        /// <summary>
        /// Adds NOT nodes.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="value">The value.</param>
        private static void AddNotNodes(List<TreeNode> tokens, ref string value)
        {
            string notOperator = ExtractNotOperators(ref value);

            if (!String.IsNullOrEmpty(notOperator))
            {
                tokens.Add(new AllResources());
                tokens.Add(new NotOperator());
            }
        }

        /// <summary>
        /// Gets the property condition in the query.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>Property condition in the query.</returns>
        private static string GetPropertyCondition(ref string searchQuery)
        {
            string notOperator = ExtractNotOperators(ref searchQuery);

            string condition = String.Empty;
            if (searchQuery.StartsWith(
                SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal))
            {
                condition = GetFirstBracketBlock(ref searchQuery);
            }
            else
            {
                condition = FetchFirstCondition(ref searchQuery);

                condition = SearchConstants.OPEN_ROUND_BRACKET
                    + condition
                    + SearchConstants.CLOSE_ROUND_BRACKET;
            }

            return notOperator + condition;
        }

        /// <summary>
        /// Fetches the first conditional clause in the query.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>The first conditional string in the query.</returns>
        private static string FetchFirstCondition(ref string searchQuery)
        {
            // NOTE: condition may or may not be with operator e.g. '>= 10/10/2008'
            // To handle cases like 'xyz', '=xyz', '="eric gamma"', '= xyz', '= "eric gamma"'.

            string condition = String.Empty;

            string conditonalOperator =
                ExtractConditionalOperatorFromValue(ref searchQuery);

            // Append operator + operand => value.
            string operand = GetFirstToken(ref searchQuery);

            if (!SearchOperators.LogicalOperators.Contains(operand) &&
                !String.Equals(operand, SearchConstants.OPEN_ROUND_BRACKET) &&
                !String.Equals(operand, SearchConstants.CLOSE_ROUND_BRACKET) &&
                !String.Equals(operand, SearchConstants.NOT)
                )
            {
                // Check to avoid unnecessary space
                if (!String.IsNullOrEmpty(conditonalOperator))
                {
                    condition = conditonalOperator + SearchConstants.SPACE + operand;
                }
                else
                {
                    condition = operand;
                }
            }
            else // Add back to searchQuery.
            {
                searchQuery = operand + SearchConstants.SPACE + searchQuery;
                condition = conditonalOperator;
            }
            return condition.Trim();
        }

        /// <summary>
        /// Fetches 'NOT' operator from start of the input query.
        /// </summary>
        /// <param name="input">Input query.</param>
        /// <returns>'NOT' if there are odd numbers of 'NOT' at the start of input query, 
        /// else returns empty string. 
        /// And trims the 'NOT's from the start of the input query.</returns>
        private static string ExtractNotOperators(ref string input)
        {
            int notOperatorCount = 0;
            while (StartsWithNotOperator(input))
            {
                notOperatorCount++;
                input =
                    input.Remove(0, SearchConstants.NOT.Length).Trim();
            }
            // In case of even numbers of 'NOT', they cancel each other
            if (notOperatorCount % 2 != 0)
            {
                return SearchConstants.NOT;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="expressionTokenType">Type of the expression token.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <returns>The <see cref="TreeNode"/> type.</returns>
        private TreeNode GetExpression(
                                    string field, 
                                    string value,
                                    ExpressionTokenType expressionTokenType, 
                                    string resourceTypeFullName)
        {
            Expression expression;
            // Default data type
            DataTypes fieldDataType = DataTypes.String;

            if (expressionTokenType == ExpressionTokenType.ImplicitPropertiesToken)
            {
                if (IsValueWithinQuotes(value))
                {
                    expression = new WordEqualExpression();
                }
                else
                {
                    expression = new WordStartsWithExpression();
                }
            }
            else
            {
                if (expressionTokenType == ExpressionTokenType.SpecialToken)
                {
                    fieldDataType =
                        SearchTokens.FetchSpecialTokenDataType(field);
                }
                else if (expressionTokenType == ExpressionTokenType.PropertyToken)
                {
                    ScalarProperty scalarProperty =
                        searchTokens.FetchPropertyToken(field, resourceTypeFullName);
                    fieldDataType = scalarProperty.DataType;
                }

                string conditionalOperator = GetConditionalOperator(ref value);

                expression =
                    GetExpressionObjectByDataType(value, fieldDataType, conditionalOperator);
            }

            expression.TokenType = expressionTokenType;
            expression.ExpressionToken = field;
            expression.DataType = fieldDataType;
            //Note: Set this with final derived resource type in GetTokens
            expression.ResourceTypeFullName = String.Empty;

            // Trim quotes before adding value to expression.
            expression.Value = TrimQuotes(value, true, false);
            return expression;
        }

        /// <summary>
        /// Gets the conditional operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Conditional operator.</returns>
        private static string GetConditionalOperator(ref string value)
        {
            string conditionalOperator =
                ExtractConditionalOperatorFromValue(ref value);

            // If value contains only operator then treat the conditional operator as value.
            if (!String.IsNullOrEmpty(conditionalOperator) && String.IsNullOrEmpty(value))
            {
                value = conditionalOperator;
                conditionalOperator = String.Empty;
            }
            return conditionalOperator;
        }

        /// <summary>
        /// Gets the type of the expression object by data.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="fieldDataType">Type of the field data.</param>
        /// <param name="conditionalOperator">The conditional operator.</param>
        /// <returns>The <see cref="Expression"/> type.</returns>
        private static Expression GetExpressionObjectByDataType(
                                                            string value, 
                                                            DataTypes fieldDataType, 
                                                            string conditionalOperator)
        {
            Expression expression;
            switch (fieldDataType)
            {
                case DataTypes.String:
                    {
                        expression = HandleStringDataType(value, conditionalOperator);
                        break;
                    }
                default: // Except for String type other would be treated with (=, >=, <=, >, <)
                    {
                        if (String.IsNullOrEmpty(conditionalOperator))
                        {
                            expression = new ComparisonExpression(SearchConstants.EQUAL_TO);
                        }
                        else
                        {
                            expression = new ComparisonExpression(conditionalOperator);
                        }
                        break;
                    }
            }
            return expression;
        }

        /// <summary>
        /// Handles the type of the string data.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="conditionalOperator">The conditional operator.</param>
        /// <returns>The <see cref="Expression"/> type.</returns>
        private static Expression HandleStringDataType(string value, string conditionalOperator)
        {
            Expression expression;
            if (IsValueWithinQuotes(value))
            {
                if (String.IsNullOrEmpty(conditionalOperator))
                {
                    expression = new WordEqualExpression();
                }
                else
                {
                    expression = new ComparisonExpression(conditionalOperator);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(conditionalOperator))
                {
                    expression = new WordStartsWithExpression();
                }
                else
                {
                    expression = new ComparisonExpression(conditionalOperator);
                }
            }
            return expression;
        }

        /// <summary>
        /// Trims the quotes from the given string.
        /// </summary>
        /// <param name="value">The value to trim.</param>
        /// <param name="replaceEscapeCharacter">if set to <c>true</c> replaces the escape character.</param>
        /// <param name="trimValueSpaces">if set to <c>true</c> [trim value spaces].</param>
        /// <returns>Trimmed string.</returns>
        private static string TrimQuotes(
                                        string value, 
                                        bool replaceEscapeCharacter, 
                                        bool trimValueSpaces)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            if (IsValueWithinQuotes(value))
            {
                string quote = value[0].ToString();
                value = value.Substring(1, value.Length - 2);
                if (replaceEscapeCharacter)
                {
                    value = 
                        value.Replace(SearchConstants.ESCAPE_CHARACTER + quote, quote);
                }
            }
            if (trimValueSpaces)
            {
                return value.Trim();
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Determines whether the specified value is within quotes.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is within quotes; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValueWithinQuotes(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if ((
                    (value.StartsWith(SearchConstants.DOUBLE_QUOTE, StringComparison.Ordinal) &&
                    value.EndsWith(SearchConstants.DOUBLE_QUOTE, StringComparison.Ordinal))
                    ||
                    (value.StartsWith(SearchConstants.SINGLE_QUOTE, StringComparison.Ordinal) &&
                    value.EndsWith(SearchConstants.SINGLE_QUOTE, StringComparison.Ordinal))
                    )
                    &&
                    value.Length >= 2)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Gets the first token in the query.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>The first token in the query.</returns>
        private static string GetFirstToken(ref string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return String.Empty;
            }

            string firstToken = Utility.GetQuotedString(ref searchQuery);

            // If searchQuery doesn't start with quotes.
            if (String.IsNullOrEmpty(firstToken))
            {
                firstToken = GetFirstWordOrBracket(ref searchQuery);
            }

            return firstToken;
        }

        /// <summary>
        /// Gets the first word or bracket.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>The first word or bracket.</returns>
        private static string GetFirstWordOrBracket(ref string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return String.Empty;
            }
            searchQuery = searchQuery.TrimStart(SearchConstants.SPACE[0]);

            if (String.IsNullOrEmpty(searchQuery))
            {
                return String.Empty;
            }

            string firstToken;

            if (searchQuery.StartsWith(
                SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal))
            {
                firstToken = SearchConstants.OPEN_ROUND_BRACKET;
            }
            else if (searchQuery.StartsWith(
                SearchConstants.CLOSE_ROUND_BRACKET, StringComparison.Ordinal))
            {
                firstToken = SearchConstants.CLOSE_ROUND_BRACKET;
            }
            else
            {
                firstToken = searchQuery.Split(
                                new String[] 
                                    { 
                                        SearchConstants.SPACE, 
                                        SearchConstants.OPEN_ROUND_BRACKET, 
                                        SearchConstants.CLOSE_ROUND_BRACKET 
                                    },
                                StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            }

            searchQuery = searchQuery.Remove(0, firstToken.Length);
            return firstToken;
        }

        /// <summary>
        /// Gets string starting from 1st open round bracket till its 
        /// matching closing bracket.
        /// </summary>
        /// <param name="searchQuery">Search query.</param>
        /// <returns>Bracket block.</returns>
        private static string GetFirstBracketBlock(ref string searchQuery)
        {
            searchQuery = searchQuery.TrimStart(SearchConstants.SPACE[0]);
            if (!String.IsNullOrEmpty(searchQuery)
                && searchQuery.StartsWith(
                SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal))
            {

                Stack<string> parenthesisStack = new Stack<string>();
                StringBuilder conditionString = new StringBuilder();

                while (!String.IsNullOrEmpty(searchQuery))
                {
                    string token = String.Empty;
                    if (searchQuery.StartsWith(SearchConstants.DOUBLE_QUOTE, StringComparison.Ordinal)
                        || searchQuery.StartsWith(SearchConstants.SINGLE_QUOTE, StringComparison.Ordinal))
                    {
                        token = Utility.GetQuotedString(ref searchQuery);
                        conditionString.Append(token);
                    }
                    // Discard quoted characters.
                    if (String.IsNullOrEmpty(token) && !String.IsNullOrEmpty(searchQuery))
                    {
                        token = searchQuery[0].ToString();
                        conditionString.Append(token);
                        searchQuery = searchQuery.Remove(0, 1);

                        if (String.Equals(token, SearchConstants.OPEN_ROUND_BRACKET))
                        {
                            parenthesisStack.Push(SearchConstants.OPEN_ROUND_BRACKET);
                        }
                        else if (String.Equals(token, SearchConstants.CLOSE_ROUND_BRACKET))
                        {
                            parenthesisStack.Pop();
                            if (parenthesisStack.Count == 0)
                            {
                                return conditionString.ToString();
                            }
                        }
                    }
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Finds out if the query starts with a special property token.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>
        ///     <c>true</c> if the query starts with a special property token, <c>false</c> otherwise.
        /// </returns>
        private static bool StartsWithSpecialPropertyToken(string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return false;
            }
            if (searchQuery.Contains(SearchConstants.COLON))
            {
                string token =
                    searchQuery.Substring(
                    0, searchQuery.IndexOf(SearchConstants.COLON, StringComparison.Ordinal));
                bool isSpecialToken = SearchTokens.IsSpecialToken(token);
                if (isSpecialToken)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds out if the query starts with a property token.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <returns>
        ///     <c>true</c> if the query starts with a property token, <c>false</c> otherwise.
        /// </returns>
        private bool StartsWithPropertyToken(string searchQuery, ref string resourceTypeFullName)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return false;
            }
            if (searchQuery.Contains(SearchConstants.COLON))
            {
                string token =
                    searchQuery.Substring(
                    0, searchQuery.IndexOf(SearchConstants.COLON, StringComparison.Ordinal));
                ScalarProperty scalarProperty;

                if (hasUserSpecifiedResourceType)
                {
                    scalarProperty =
                        searchTokens.FetchPropertyToken(token, resourceTypeFullName);
                }
                else //Infer
                {
                    scalarProperty = searchTokens.FetchPropertyToken(token);
                    if (scalarProperty == null)
                    {
                        return false;
                    }
                    //  Compare resource types to find out the derived one.
                    int comparisonResult =
                        searchTokens.CompareResourceTypes(
                            resourceTypeFullName, scalarProperty.Parent.FullName);
                    if (comparisonResult < 0) // New resource type is derived from previous _predicateConditionResourceTypeFullName
                    {
                        IEnumerable<string> excludedResourceTypeFullNames =
                            SearchTokens.FetchExcludedResourceTypeFullNames();

                        if (!excludedResourceTypeFullNames
                            .Contains(scalarProperty.Parent.FullName.ToLower()))
                        {
                            resourceTypeFullName = scalarProperty.Parent.FullName;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (comparisonResult == 0)
                    {
                        // Then don't consider this token as a property.
                        return false;
                    }
                }
                if (scalarProperty != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds out if the query starts with a predicate token.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>
        ///     <c>true</c> if the query starts with a predicate token, <c>false</c> otherwise.
        /// </returns>
        private bool StartsWithPredicateToken(string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return false;
            }
            if (searchQuery.Contains(SearchConstants.COLON))
            {
                string token =
                    searchQuery.Substring(
                    0, searchQuery.IndexOf(SearchConstants.COLON, StringComparison.Ordinal));
                IEnumerable<PredicateToken> predicateTokens =
                    searchTokens.FetchPredicateToken(token);
                if (predicateTokens.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds out if the query starts with a bracket.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>
        ///     <c>true</c> if the query starts with a bracket, <c>false</c> otherwise.
        /// </returns>
        private static bool StartsWithBracket(string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return false;
            }
            if (searchQuery.StartsWith(
                SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal)
                || searchQuery.StartsWith(
                SearchConstants.CLOSE_ROUND_BRACKET, StringComparison.Ordinal))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds out if the query starts with a logical operator.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>
        ///     <c>true</c> if the query starts with a logical operator, <c>false</c> otherwise.
        /// </returns>
        private static bool StartsWithLogicalOperator(string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return false;
            }
            foreach (string logicalOperator in SearchOperators.LogicalOperators)
            {
                if (searchQuery.StartsWith(
                    logicalOperator + SearchConstants.SPACE, StringComparison.Ordinal)
                    || searchQuery.StartsWith(
                    logicalOperator + SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal)
                    || searchQuery.StartsWith(
                    logicalOperator + SearchConstants.CLOSE_ROUND_BRACKET, StringComparison.Ordinal)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds out if the query starts with the NOT operator.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>
        ///     <c>true</c> if the query starts with the NOT operator, <c>false</c> otherwise.
        /// </returns>
        private static bool StartsWithNotOperator(string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
            {
                return false;
            }
            if (String.Equals(searchQuery, SearchConstants.NOT, StringComparison.Ordinal))
            {
                return true;
            }
            if (searchQuery.StartsWith(
                SearchConstants.NOT + SearchConstants.SPACE, StringComparison.Ordinal)
                || searchQuery.StartsWith(
                SearchConstants.NOT + SearchConstants.OPEN_ROUND_BRACKET, StringComparison.Ordinal)
                || searchQuery.StartsWith(
                SearchConstants.NOT + SearchConstants.CLOSE_ROUND_BRACKET, StringComparison.Ordinal)
                )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the consecutive NOT tokens.
        /// </summary>
        /// <param name="tokens">List of tokens.</param>
        private static void RemoveConsecutiveNotTokens(List<TreeNode> tokens)
        {
            if (tokens != null)
            {
                int tokenCount = tokens.Count;
                for (int i = 1; i < tokens.Count - 2; i++)
                {
                    if (tokens[i].Type == NodeType.Not && tokens[i - 1].Type == NodeType.AllResources
                        && tokens[i + 2].Type == NodeType.Not && tokens[i + 1].Type == NodeType.AllResources)
                    {
                        tokens.RemoveRange(i - 1, 4);
                        // Set counter to start.
                        i = 1;
                    }
                }
            }
        }

        #endregion
    }
}