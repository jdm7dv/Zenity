// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

#region .NET Framework Class Namespace Imports

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

#endregion

#region Custom Namespace Imports

using Zentity.Platform.Properties;

#endregion

namespace Zentity.Platform
{
    /// <summary>
    /// Represents a date value class.
    /// </summary>
    internal class DateValue
    {
        #region Private Fields

        /// <summary>
        /// Valid special date values regular expression.
        /// </summary>
        private static string validSpecialDateValuesRegex;

        /// <summary>
        /// List of valid special date values.
        /// </summary>
        private static string[] regexResourceKeys =
            new string[] {
            SearchConstants.SEARCH_DATE_REGEX_TODAY,
            SearchConstants.SEARCH_DATE_REGEX_TOMORROW,
            SearchConstants.SEARCH_DATE_REGEX_YESTERDAY,
            SearchConstants.SEARCH_DATE_REGEX_THIS_WEEK,
            SearchConstants.SEARCH_DATE_REGEX_NEXT_WEEK,
            SearchConstants.SEARCH_DATE_REGEX_LAST_WEEK,
            SearchConstants.SEARCH_DATE_REGEX_PAST_WEEK,
            SearchConstants.SEARCH_DATE_REGEX_THIS_MONTH,
            SearchConstants.SEARCH_DATE_REGEX_NEXT_MONTH,
            SearchConstants.SEARCH_DATE_REGEX_LAST_MONTH,
            SearchConstants.SEARCH_DATE_REGEX_PAST_MONTH,
            SearchConstants.SEARCH_DATE_REGEX_THIS_YEAR,
            SearchConstants.SEARCH_DATE_REGEX_NEXT_YEAR,
            SearchConstants.SEARCH_DATE_REGEX_LAST_YEAR,
            SearchConstants.SEARCH_DATE_REGEX_PAST_YEAR
            };

        #endregion

        #region Properties

        /// <summary>
        /// Gets the start value of the date range.
        /// </summary>
        public string StartDate { get; private set; }

        /// <summary>
        /// Gets the end value of the date range.
        /// </summary>
        public string EndDate { get; private set; }

        /// <summary>
        /// Gets the end value of the date range + 1 day.
        /// </summary>
        public string NextDayOfEndDate { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateValue"/> class.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        private DateValue(DateTime startDate, DateTime endDate)
        {
            StartDate = GetDateString(startDate);
            EndDate = GetDateString(endDate);

            if (endDate.ToShortDateString() == DateTime.MaxValue.ToShortDateString())
            {
                NextDayOfEndDate = GetDateString(DateTime.MaxValue);
            }
            else
            {
                NextDayOfEndDate = GetDateString(endDate.AddDays(1));
            }
        }

        /// <summary>
        /// Initializes static members of the <see cref="DateValue"/> class.
        /// </summary>
        static DateValue()
        {
            StringBuilder regExBuilder = new StringBuilder();
            foreach (string regExResourceKey in regexResourceKeys)
            {
                regExBuilder.Append(SearchConstants.OPEN_ROUND_BRACKET)
                    .Append(Resources.ResourceManager.GetString(regExResourceKey, CultureInfo.CurrentCulture))
                    .Append(SearchConstants.CLOSE_ROUND_BRACKET)
                    .Append(SearchConstants.PIPE);
            }
            regExBuilder = regExBuilder.Remove(
                regExBuilder.Length - SearchConstants.PIPE.Length,
                SearchConstants.PIPE.Length);
            validSpecialDateValuesRegex = regExBuilder.ToString();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts the specified string representation of a date to its <see cref="DateValue"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a date to convert.</param>
        /// <param name="date"> When this method returns, <see cref="DateValue"/> contains the value 
        /// equivalent to the date contained in value, if the conversion succeeded, or null if the 
        /// conversion failed. The conversion fails if the value parameter is null, or does not 
        /// contain a valid string representation of a date. This parameter is passed un-initialized.</param>
        /// <returns>true if the value parameter was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string value, out DateValue date)
        {
            if (string.IsNullOrEmpty(value))
            {
                date = null;
                return false;
            }

            if (Regex.IsMatch(value, validSpecialDateValuesRegex, RegexOptions.IgnoreCase))
            {
                DateTime startDate;
                DateTime endDate;
                GetStartAndEndDates(value, out startDate, out endDate);
                date = new DateValue(startDate, endDate);
                return true;
            }

            return DateTimeTryParse(value, out date);
        }

        /// <summary>
        /// Converts the specified string representation of a date to its <see cref="DateValue"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a date to convert.</param>
        /// <returns><see cref="DateValue"/> equivalent.</returns>
        public static DateValue Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            if (Regex.IsMatch(value, validSpecialDateValuesRegex, RegexOptions.IgnoreCase))
            {
                DateTime startDate;
                DateTime endDate;
                GetStartAndEndDates(value, out startDate, out endDate);
                return new DateValue(startDate, endDate);
            }

            DateTime startDateTime = DateTime.Parse(value, CultureInfo.CurrentCulture);
            return new DateValue(startDateTime, startDateTime);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tries to parse the datetime.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="date">The date.</param>
        /// <returns>
        ///     <c>true</c> if the date time provided is valid; otherwise <c>false</c>.
        /// </returns>
        private static bool DateTimeTryParse(string value, out DateValue date)
        {
            DateTime startDateTime;
            if (DateTime.TryParse(value, out startDateTime))
            {
                date = new DateValue(startDateTime, startDateTime);
                return true;
            }

            date = null;
            return false;
        }

        /// <summary>
        /// Gets the date string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>String representation of the date.</returns>
        private static string GetDateString(DateTime dateTime)
        {
            return dateTime.ToString(SearchConstants.ISO_8601_DATE_FORMAT);
        }

        /// <summary>
        /// Gets the start and end dates.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        private static void GetStartAndEndDates(string value, out DateTime startDate, out DateTime endDate)
        {
            DateTime today = DateTime.Today;
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_TODAY, value))
            {
                startDate = endDate = today;
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_TOMORROW, value))
            {
                startDate = endDate = today.AddDays(1);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_YESTERDAY, value))
            {
                startDate = endDate = today.AddDays(-1);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_THIS_WEEK, value))
            {
                startDate = today.AddDays(-((int)today.DayOfWeek));
                endDate = startDate.AddDays(6);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_NEXT_WEEK, value))
            {
                startDate = today.AddDays(-((int)today.DayOfWeek)).AddDays(7);
                endDate = startDate.AddDays(6);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_LAST_WEEK, value) ||
               CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_PAST_WEEK, value))
            {
                startDate = today.AddDays(-((int)today.DayOfWeek)).AddDays(-7);
                endDate = startDate.AddDays(6);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_THIS_MONTH, value))
            {
                startDate = today.AddDays(-(today.Day - 1));
                endDate = startDate.AddMonths(1).AddDays(-1);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_NEXT_MONTH, value))
            {
                startDate = today.AddDays(-(today.Day - 1)).AddMonths(1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_LAST_MONTH, value) ||
                CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_PAST_MONTH, value))
            {
                startDate = today.AddDays(-(today.Day - 1)).AddMonths(-1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_THIS_YEAR, value))
            {
                startDate = today.AddDays(-(today.Day - 1)).AddMonths(-(today.Month - 1));
                endDate = startDate.AddYears(1).AddDays(-1);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_NEXT_YEAR, value))
            {
                startDate = today.AddDays(-(today.Day - 1)).AddMonths(-(today.Month - 1)).AddYears(1);
                endDate = startDate.AddYears(1).AddDays(-1);
                return;
            }
            if (CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_LAST_YEAR, value)
                || CheckRegularExpressionMatch(SearchConstants.SEARCH_DATE_REGEX_PAST_YEAR, value))
            {
                startDate = today.AddDays(-(today.Day - 1)).AddMonths(-(today.Month - 1)).AddYears(-1);
                endDate = startDate.AddYears(1).AddDays(-1);
                return;
            }
            // Default to today.
            startDate = endDate = today;
        }

        /// <summary>
        /// Checks the regular expression match.
        /// </summary>
        /// <param name="regExResourceKey">The reg ex resource key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the regular expression match passes; otherwise <c>false</c>.
        /// </returns>
        private static bool CheckRegularExpressionMatch(string regExResourceKey, string value)
        {
            return Regex.IsMatch(value,
                Resources.ResourceManager.GetString(regExResourceKey, CultureInfo.CurrentCulture),
                RegexOptions.IgnoreCase);
        }

        #endregion
    }
}