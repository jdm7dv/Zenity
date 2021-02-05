// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zentity.Platform
{
	internal static class BibTeXHelper
	{
		public const string BT_ENTRY_VARIABLE_TYPE = "STRING";
		public const string BT_ENTRY_COMMENT_TYPE = "COMMENT";
		public const string BT_ENTRY_AT = "@";
		public const string BT_ENTRY_LEFT_BRACE = "{";
		public const string BT_ENTRY_RIGHT_BRACE = "}";
		public const string BT_ENTRY_COMMA = ",";
		public const string BT_ENTRY_EQUALTO = " = ";
		public const string BT_ENTRY_EQUAL = "=";
		public const string BT_ENTRY_QUOTATION = "\"";
		public const string BT_ENTRY_CONCATENATION = "#";
		public const string BT_ENTRY_LEFT_PARENTHESIS = "(";
		public const string BT_ENTRY_RIGHT_PARENTHESIS = ")";
		public const string BT_ENTRY_TAB = "\t";
		public const char BT_CHAR_ENTRY_LEFT_BRACE = '{';
		public const char BT_CHAR_ENTRY_RIGHT_BRACE = '}';
		public const char BT_CHAR_ENTRY_QUOTATION = '"';
		public const char BT_CHAR_ENTRY_NEWLINE = '\n';
		public const char BT_CHAR_ENTRY_CARRIAGERETURN = '\r';



		public static bool IsValidEntyName(string name)
		{
			Regex seprators = new Regex(Properties.Resources.REGEXP_VALID_ENTRY_NAME
										, RegexOptions.IgnoreCase);
			return !seprators.IsMatch(name);
		}

		/// <summary>
		/// Checks for valid property name.
		/// </summary>
		/// <param name="name">string name to be checked</param>
		/// <returns>true - if the name is valid</returns>
		public static bool IsValidPropertyName(string name)
		{
			Regex seprators = new Regex(Properties.Resources.REGEXP_VALID_PROPERTY_NAME
										, RegexOptions.IgnoreCase);
			return !seprators.IsMatch(name);
		}

		/// <summary>
		/// Validates the key for the BibTeX entry
		/// </summary>
		/// <param name="key">key to be validated</param>
		/// <returns>Return true if valid key otherwise false</returns>
		public static bool IsValidKey(string key)
		{
			Regex seprators = new Regex(Properties.Resources.REGEXP_VALID_KEY
										, RegexOptions.IgnoreCase);
			return !seprators.IsMatch(key);
		}
	}
}
