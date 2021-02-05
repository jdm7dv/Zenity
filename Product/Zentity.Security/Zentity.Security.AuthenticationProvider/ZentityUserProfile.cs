// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Security.AuthenticationProvider
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Zentity.Security.AuthenticationProvider.PasswordManagement;

    /// <summary>
    /// This class stores information of a Zentity user except his credentials.
    /// </summary>
    public class ZentityUserProfile
    {
        #region Properties

        /// <summary>
        /// Email of the user.
        /// </summary>
        private string email;

        /// <summary>
        /// Login name of for the user.
        /// </summary>
        private string logOnName;

        /// <summary>
        /// Security question.
        /// </summary>
        private string securityQuestion;

        /// <summary>
        /// Answer to security question.
        /// </summary>
        private string answer;

        /// <summary>
        /// Unique identifier for the user with given Log on Name.
        /// </summary>
        private Guid id;

        /// <summary>
        /// Account status.
        /// </summary>
        private string accountStatus;

        /// <summary>
        /// Date time at which user account was created.
        /// </summary>
        private DateTime? dateCreated;

        /// <summary>
        /// Date time at which user account / profile was last modified.
        /// </summary>
        private DateTime? dateModified;

        /// <summary>
        /// Date time at which the password was created / last updated.
        /// </summary>
        private DateTime? passwordCreationDate;

        /// <summary>
        /// Gets or sets the first name of the user. Required property
        /// </summary>
        public string FirstName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the middle name of the user. Optional property
        /// </summary>
        public string MiddleName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last name of the user. Optional property
        /// </summary>
        public string LastName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the email of the user. Required property. 
        /// </summary>
        /// <remarks>The setter validates whether the email string is in valid format</remarks>
        public string Email
        {
            get
            {
                return this.email;
            }

            set
            {
                if (ValidateEmail(value))
                {
                    this.email = value;
                }
                else
                {
                    throw new ArgumentException(ConstantStrings.EmailFormatExceptionMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the city to which the user belongs. Optional property. 
        /// </summary>
        /// <remarks>Setter does not validate input</remarks>
        public string City
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the state to which the user belongs. Optional property.
        /// </summary>
        /// <remarks>Setter does not validate input</remarks>
        public string State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the country to which the user belongs. Optional property. 
        /// </summary>
        /// <remarks>Setter does not validate input</remarks>
        public string Country
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique login name for the user. 
        /// </summary>
        /// <exception cref="System.ArgumentException">If the login name is not in valid format the setter throws ArgumentException</exception>
        public string LogOnName
        {
            get
            {
                return this.logOnName;
            }

            protected internal set
            {
                if (ValidateLogOnName(value))
                {
                    this.logOnName = value;
                }
                else
                {
                    throw new ArgumentException(ConstantStrings.LoginFormatExceptionMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the security question
        /// </summary>
        public string SecurityQuestion
        {
            get
            {
                return this.securityQuestion;
            }

            set
            {
                //// Parameter validation
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                this.securityQuestion = value.ToUpper(CultureInfo.CurrentUICulture);
            }
        }

        /// <summary>
        /// Gets or sets answer. 
        /// </summary>
        public string Answer
        {
            get
            {
                return this.answer;
            }

            set
            {
                //// Input validation
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                this.answer = HashingUtility.GenerateHash(value.ToUpper(CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Gets or sets the unique identifier for the user with given LogOnName
        /// </summary>
        public Guid Id
        {
            get
            {
                return this.id;
            }

            protected internal set
            {
                //// Input Validation
                if (value == Guid.Empty)
                {
                    throw new ArgumentNullException("value");
                }

                this.id = value;
            }
        }

        /// <summary>
        /// Gets or sets account status
        /// </summary>
        public string AccountStatus
        {
            get
            {
                return this.accountStatus;
            }

            protected internal set
            {
                //// Input Validation
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                this.accountStatus = value;
            }
        }

        /// <summary>
        /// Gets or sets the date time at which user account was created, or null if the 
        /// date is not retrieved from authentication store.
        /// </summary>
        /// <remarks>This property will be set to the value stored in database after
        /// FillUserProperties() method is called.</remarks>
        public DateTime? DateCreated
        {
            get
            {
                return this.dateCreated;
            }

            protected internal set
            {
                //// Input Validation
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.dateCreated = value;
            }
        }

        /// <summary>
        /// Gets or sets the date time at which user account / profile was last modified, 
        /// or null if the date is not retrieved from authentication store.
        /// </summary>
        /// <remarks>This property will be set to the value stored in database 
        /// after FillUserProperties() method is called.</remarks>
        public DateTime? DateModified
        {
            get
            {
                return this.dateModified;
            }

            protected internal set
            {
                //// Input Validation
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.dateModified = value;
            }
        }

        /// <summary>
        /// Gets or sets the the date time at which the password was created / last 
        /// updated, or null if the date is not retrieved from authentication store.
        /// </summary>
        /// <remarks>This property will be set to the value stored in database 
        /// after FillUserProperties() method is called.</remarks>
        public DateTime? PasswordCreationDate
        {
            get
            {
                return this.passwordCreationDate;
            }

            protected internal set 
            {
                //// Input Validation
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.passwordCreationDate = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates whether the login name sent as a parameter starts with an 
        /// alphabet and contains no special characters except underscore and hyphen
        /// or is a valid email id.
        /// </summary>
        /// <param name="logOnName">LogOnName of the user</param>
        /// <returns>True if log on name is valid</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when this method is called with null or empty string parameter.</exception>
        internal static bool ValidateLogOnName(string logOnName)
        {
            //// Input Validation
            if (string.IsNullOrEmpty(logOnName))
            {
                throw new ArgumentNullException("logOnName");
            }

            //// Check whether the login name is a valid, 
            //// and contains no special characters except "_" or "." or "-"
            if (Char.IsLetter(logOnName[0]))
            {
                if (!ContainsSpecialChar(logOnName))
                {
                    return true;
                }
            }

            //// If logonName is found invalid in the first check, 
            //// check for email id. Logon name can be a valid email too.
            return ValidateEmail(logOnName);
        }

        /// <summary>
        /// Sets a pre-hashed answer
        /// </summary>
        /// <param name="hashedAnswer">Answer in hashed form</param>
        protected internal void SetHashedAnswer(string hashedAnswer)
        {
            this.answer = hashedAnswer;
        }

        /// <summary>
        /// Checks whether the input string contains any special character except _ and - and .
        /// </summary>
        /// <param name="input">Input to be verified</param>
        /// <returns>System.Boolen; <c>true</c> if the input contains special characters, <c>false</c> otherwise.</returns>
        private static bool ContainsSpecialChar(string input)
        {
            string specialChars = "~`!@#$%^&*()+={}[]|\\/?><,:;\"'";
            foreach (char ch in input)
            {
                if (specialChars.Contains(ch))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Validates whether the parameter is of the form a@b.c
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>True if email string is in correct format</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when this method is called with null or empty string parameter.</exception>
        private static bool ValidateEmail(string email)
        {
            //// Input Validation
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            //// Check whether the email is of the form a@b.c
            Regex emailExpression = new Regex(@"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
            return emailExpression.IsMatch(email);
        }

        #endregion
    }
}
