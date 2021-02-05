// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    /// <summary>
    /// Provides utility constants.
    /// </summary>
    internal static class SearchConstants
    {
        #region Parser And Interpreter Constants 

        internal const string AND = "AND";
        internal const string OR = "OR";
        internal const string ASTERIX = "*";
        internal const string SINGLE_QUOTE = "'";
        internal const string PIPE = "|";
        internal const string TWO_SINGLE_QUOTES = SINGLE_QUOTE + SINGLE_QUOTE;
        internal const string DOUBLE_QUOTE = "\"";
        internal const string TWO_DOUBLE_QUOTES = DOUBLE_QUOTE + DOUBLE_QUOTE;
        internal const string OPEN_ROUND_BRACKET = "(";
        internal const string CLOSE_ROUND_BRACKET = ")";
        internal const string SPACE = " ";
        internal const string WORDEQUAL = "WORDEQUAL";
        internal const string WORDSTARTSWITH = "WORDSTARTSWITH";
        internal const string COLON = ":";
        internal const string GREATER_THAN = ">";
        internal const string LESS_THAN = "<";
        internal const string GREATER_THAN_OR_EQUAL = ">=";
        internal const string LESS_THAN_OR_EQUAL = "<=";
        internal const string EQUAL_TO = "=";
        internal const string NOT_EQUAL_TO = "!=";
        internal const string COMMA = ",";
        internal const string DOT = ".";
        internal const int DEFAULT_MAX_RESULT_COUNT = 25;
        internal const string RESOURCETYPE = "RESOURCETYPE";
        internal const string DEFAULTRESOURCETYPE = "Zentity.Core.Resource";
        internal const string OPEN_BOX_BRACKET = "[";
        internal const string CLOSE_BOX_BRACKET = "]";
        internal const string UNDERSCORE = "_";
        internal const string ESCAPE_CHARACTER = "\\";
        internal const string FILERESOURCETYPE = "Zentity.Core.File";
        internal const string NOT = "NOT";

        #endregion 

        #region Date Regular Expression Resource Keys

        internal const string SEARCH_DATE_REGEX_TODAY = "SEARCH_DATE_REGEX_TODAY";
        internal const string SEARCH_DATE_REGEX_TOMORROW = "SEARCH_DATE_REGEX_TOMORROW";
        internal const string SEARCH_DATE_REGEX_YESTERDAY = "SEARCH_DATE_REGEX_YESTERDAY";
        internal const string SEARCH_DATE_REGEX_THIS_WEEK = "SEARCH_DATE_REGEX_THIS_WEEK";
        internal const string SEARCH_DATE_REGEX_NEXT_WEEK = "SEARCH_DATE_REGEX_NEXT_WEEK";
        internal const string SEARCH_DATE_REGEX_LAST_WEEK = "SEARCH_DATE_REGEX_LAST_WEEK";
        internal const string SEARCH_DATE_REGEX_PAST_WEEK = "SEARCH_DATE_REGEX_PAST_WEEK";
        internal const string SEARCH_DATE_REGEX_THIS_MONTH = "SEARCH_DATE_REGEX_THIS_MONTH";
        internal const string SEARCH_DATE_REGEX_NEXT_MONTH = "SEARCH_DATE_REGEX_NEXT_MONTH";
        internal const string SEARCH_DATE_REGEX_LAST_MONTH = "SEARCH_DATE_REGEX_LAST_MONTH";
        internal const string SEARCH_DATE_REGEX_PAST_MONTH = "SEARCH_DATE_REGEX_PAST_MONTH";
        internal const string SEARCH_DATE_REGEX_THIS_YEAR = "SEARCH_DATE_REGEX_THIS_YEAR";
        internal const string SEARCH_DATE_REGEX_NEXT_YEAR = "SEARCH_DATE_REGEX_NEXT_YEAR";
        internal const string SEARCH_DATE_REGEX_LAST_YEAR = "SEARCH_DATE_REGEX_LAST_YEAR";
        internal const string SEARCH_DATE_REGEX_PAST_YEAR = "SEARCH_DATE_REGEX_PAST_YEAR";

        #endregion 

        #region Xml Constants

        internal const string XML_FILE_EXTENSION = ".config";
        internal const string XMLNS_NAMESPACE = "urn:zentity";
        internal const string XML_TOKEN = "token";
        internal const string XML_MODULE = "module";
        internal const string XML_NAMESPACE = "namespace";
        internal const string XML_RESOURCETYPE = "resourceType";
        internal const string XML_NAME = "name";
        internal const string XML_PROPERTY = "property";
        internal const string XML_PREDICATE = "predicate";
        internal const string XML_REVERSERELATION = "reverseRelation";
        internal const string XML_DATATYPE = "dataType";

        #endregion 

        #region .NET Constants

        internal const string SYSTEM_TYPE_NAMESPACE_PREFIX = "System.";
        internal const string TRY_PARSE = "TryParse";
        internal const string ISO_8601_DATE_FORMAT = "yyyyMMdd";

        #endregion 

        #region T-SQL Constants

        internal const string TSQL_SP_EXECUTESQL = "sp_executesql";
        internal const string TSQL_INTERSECT = "INTERSECT";
        internal const string TSQL_UNION = "UNION";
        internal const string TSQL_EXCEPT = "EXCEPT";
        internal const string TSQL_RESOURCETYPEID_CRITERIA = " ResourceTypeId = '{0}' ";
        internal const string TSQL_RESOURCE_QUERY = "select sub.Id {0} from Core.Resource sub where ";
        internal const string TSQL_SUB = "sub.";
        internal const string TSQL_ID = "Id";
        internal const string TSQL_COMPARISON_CRITERIA = " {0} {1} N'{2}' ";
        internal const string TSQL_DATE_EQUAL_COMPARISON_CRITERIA = " {0} >= '{1}' AND {0} < '{2}' ";
        internal const string TSQL_LIKE_ESCAPE_CLAUSE = " ESCAPE '" + TSQL_LIKE_ESCAPE_CHAR + "' ";
        internal const string TSQL_WORDEQUAL_CONTAINS_CRITERIA = " CONTAINS({0}, N'\"{1}\"') ";
        internal const string TSQL_WORDEQUAL_LIKE_CRITERIA = " {0} LIKE N'%{1}%'" + TSQL_LIKE_ESCAPE_CLAUSE;
        internal const string TSQL_WORDSTARTSWITH_CONTAINS_CRITERIA = " CONTAINS({0}, N'\"{1}*\"') ";
        internal const string TSQL_WORDSTARTSWITH_LIKE_CRITERIA = " {0} LIKE N'%{1}%'" + TSQL_LIKE_ESCAPE_CLAUSE;
        internal const string TSQL_JOIN_QUERIES = "({0}) {1} ({2})";
        internal const string TSQL_PREDICATE_QUERY = "select distinct sub.Id {0} from Core.Resource sub, Core.Relationship rel, Core.Predicate pred where"
                    + " {1} AND rel.SubjectResourceId = sub.Id AND rel.PredicateId = pred.Id AND rel.ObjectResourceId IN ({2})";
        internal const string TSQL_REVERSE_PREDICATE_QUERY = "select distinct sub.Id {0} from Core.Resource sub, Core.Relationship rel, Core.Predicate pred where"
                    + " {1} AND rel.ObjectResourceId = sub.Id AND rel.PredicateId = pred.Id AND rel.SubjectResourceId IN ({2})";
        internal const string TSQL_PREDICATE_CRITERIA = " pred.Name = '{0}' ";
        internal const string TSQL_PAGING = "WITH PagingResources AS (select ROW_NUMBER() over(order by {0}) as RowNumber, MatchingResources.* from ({1}) as MatchingResources)"
                + "SELECT Id FROM PagingResources WHERE RowNumber BETWEEN {2} AND {3} ORDER BY RowNumber";
        internal const string TSQL_PAGING_PERCENTAGE_MATCH = 
            "WITH PagingResources AS (select ROW_NUMBER() over(order by " 
            + SearchConstants.TSQL_PERCENTAGE_MATCH 
            + SearchConstants.SPACE
            + SearchConstants.TSQL_DESC 
            + " ) as RowNumber, MatchingResources.* from ({0}) as MatchingResources)"
            + "SELECT Id, " 
            + SearchConstants.TSQL_PERCENTAGE_MATCH 
            + " FROM PagingResources WHERE RowNumber BETWEEN {1} AND {2} ORDER BY RowNumber";
        internal const string TSQL_SELECT_1 = "(select 1)";
        internal const string TSQL_TOTAL_COUNT = "select COUNT(Id) from ({0}) as MatchingResources";
        internal const string TSQL_LIKE_ESCAPE_CHAR = "!";
        internal const string TSQL_LIKE_ESCAPE_REGEX = "[_]|[%]|[[]|[]]|[" + SearchConstants.TSQL_LIKE_ESCAPE_CHAR + "]";
        internal const string TSQL_LIKE_ESCAPE_REGEX_REPLACE = SearchConstants.TSQL_LIKE_ESCAPE_CHAR + "$&";
        internal const string TSQL_CONTENT_QUERY_SORTING = "select ResourceId as Id {0} from Core.Content con, Core.Resource res where FREETEXT(con.Content, '{1}') and con.ResourceId = res.Id";
        internal const string TSQL_ASC = "asc";
        internal const string TSQL_DESC = "desc";
        internal const string TSQL_PLUS = "+";
        internal const string TSQL_CAST_LEN = "CAST(LEN({0}) as float) ";
        internal const string TSQL_PERCENTAGE_MATCH = "percentageMatch";
        internal const string TSQL_PERCENTAGE_MATCH_FORMULA =
            "CAST(({0})/({1})*100 as DEC(5,2)) as " + SearchConstants.TSQL_PERCENTAGE_MATCH + SearchConstants.SPACE;
        internal const string TSQL_AUTHORIZATION = "select MatchingResources.* from ( {0} ) as MatchingResources where {1}";
        
        #endregion 

        #region E-SQL Constants

        internal const string ESQL_RESOURCES = "select value r from OFTYPE(ZentityContext.Resources, {0}) as r";
        internal const string ESQL_RESOURCES_BY_ID_WHERE = " where r.Id IN {{ {0} }}";
        internal const string ESQL_CAST_GUID = "CAST('{0}' AS System.Guid)";
        internal const string ESQL_ORDER_BY = " order by r.{0} {1}";

        #endregion 

        #region Regular Expressions

        internal const string REGEX_SPACES = "\\A\\s+";
        // Expression for: '( )', '(AND/OR)', '(AND OR OR...)'
        internal const string REGEX_UNUSED_BLOCKS = "[(]\\s*((AND)\\s+|(OR)\\s+|(NOT)\\s+)*\\s*[)]|[(]\\s*((AND)\\s+|(OR)\\s+|(NOT)\\s+)*\\s*((AND)[)]|(OR)[)]|(NOT)[)])";
        // Expression for: 'AND/OR resourceType: ABC', 'resourceType: ABC'
        internal const string REGEX_RESOURCE_TYPE =
                    "[(]\\s*" + SearchConstants.RESOURCETYPE + "[:]\\s*[^\\s^(^)]+"
                    + "|"
                    + "[)]\\s*" + SearchConstants.RESOURCETYPE + "[:]\\s*[^\\s^(^)]+"
                    + "|"
                    + "\\A" + SearchConstants.RESOURCETYPE + "[:]\\s*[^\\s^(^)]+"
                    + "|"
                    + "\\s+" + SearchConstants.RESOURCETYPE + "[:]\\s*[^\\s^(^)]+";
        internal const string REGEX_UNUSED_LOGICAL_OPERATORS_AT_START = "\\A((AND)|(OR))\\s|\\A((AND)|(OR))\\s*[(]|\\A((AND)|(OR))$";
        internal const string REGEX_UNUSED_LOGICAL_AND_NOT_OPERATORS_AT_END = "\\s((AND)|(OR)|(NOT))$|[):]\\s*((AND)|(OR)|(NOT))$|\\A\\s*(NOT)\\s*$";
        internal const string REGEX_OPEN_BRACKET_FOLLOWED_BY_LOGICAL_OPERATOR = "[(]\\s*((AND)|(OR))\\s";
        internal const string REGEX_LOGICAL_AND_NOT_OPERATOR_FOLLOWED_BY_CLOSE_BRACKET = "\\s((AND)|(OR)|(NOT))\\s*[)]";
        internal const string REGEX_OPERATOR_FOLLOWED_OPERATOR = "\\s((AND)|(OR)|(NOT))\\s+((AND)|(OR))|\\A((AND)|(OR)|(NOT))\\s+((AND)|(OR))";
       
        #endregion 
    }
}