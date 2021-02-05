// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System;

    /// <summary>
    /// Parse author name according to 3 BibTeX formats explained in 
    /// Tame the BeaST: The B to X of BibTEX by Nicolas Markey,
    /// which can be found at ftp://ftp.tex.ac.uk/tex-archive/info/bibtex/tamethebeast/ttb_en.pdf 
    /// </summary>
    class BibTeXAuthorNameParser
    {
        #region Public

        /// <summary>
        /// Initializes a new instance of the <see cref="BibTeXAuthorNameParser"/> class.
        /// </summary>
        /// <param name="authorName">Name of the author.</param>
        public BibTeXAuthorNameParser(string authorName)
        {
            completeName = authorName.Trim();
        }

        /// <summary>
        /// Parses this instance.
        /// </summary>
        public void Parse()
        {
            //Which format?
            string[] nameparts = completeName.Split(',');
            switch(nameparts.Length)
            {
                case 2:
                    ParseSecondFormat(nameparts[1], nameparts[0]);
                    break;
                case 3:
                    ParseThirdFormat(nameparts[1], nameparts[2], nameparts[0]);
                    break;
                default:
                    ParseFirstFormat(completeName, true);
                    break;
            }
        }

        #region Properties

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public string FirstName
        {
            get
            {
                return (title + " " + firstName).Trim();
            }
        }

        /// <summary>
        /// Gets the name of the middle.
        /// </summary>
        /// <value>The name of the middle.</value>
        public string MiddleName
        {
            get
            {
                return middleName;
            }
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public string LastName
        {
            get
            {
                return lastName;
            }
        }

        #endregion

        #endregion

        #region Private

        #region Member Variables

        private string firstName = "";
        private string middleName = ""; //aka. Von
        private string lastName = "";
        private string title = ""; //aka. Jr.
        private string completeName = "";

        #endregion

        /// <summary>
        /// Parses the first format.
        /// </summary>
        /// <param name="authorName">Name of the author.</param>
        /// <param name="isThreePartName">if set to <c>true</c> [is three part name].</param>
        private void ParseFirstFormat(string authorName, bool isThreePartName)
        {
            //Format: First von Last
            string[] nameparts = authorName.Split(new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            if(nameparts.Length == 0)
                return;
            int firstSmallLetterWord = -1;
            int lastSmallLetterWord = -1;
            for(int i = 0; i < nameparts.Length - 1; ++i)
            {
                if(BeginsWithSmallCase(nameparts[i]))
                {
                    if(firstSmallLetterWord != -1)
                        lastSmallLetterWord = i;
                    else
                    {
                        firstSmallLetterWord = lastSmallLetterWord = i;
                    }
                }
            }
            //Special case when there is no 'Von' part
            if(firstSmallLetterWord == -1 && lastSmallLetterWord == -1)
            {
                //In case its a two worded name, lets ignore the 'von' part
                firstSmallLetterWord = nameparts.Length - 1;
                lastSmallLetterWord = firstSmallLetterWord - 1;

                //In case its a three worded name, lets deduce the middle 
                // name as the second part
                if(nameparts.Length == 3 && isThreePartName == true)
                {
                    firstSmallLetterWord = 1;
                    lastSmallLetterWord = 1;
                }
            }
            for(int i = 0; i < nameparts.Length; ++i)
            {
                if(i < firstSmallLetterWord)
                    firstName += nameparts[i] + " ";
                if(i >= firstSmallLetterWord && i <= lastSmallLetterWord)
                    middleName += nameparts[i] + " ";
                if(i > lastSmallLetterWord)
                    lastName += nameparts[i] + " ";
            }
            firstName = firstName.Trim();
            middleName = middleName.Trim();
            lastName = lastName.Trim();
        }

        /// <summary>
        /// Parses the second format.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="remainingName">Name of the remaining.</param>
        private void ParseSecondFormat(string first, string remainingName)
        {
            //Format: von Last, First
            //Lets treat the reamining part as first format name
            ParseFirstFormat(remainingName, false);

            //We already have first name as namepartsComma[1]
            //In first format middle name is empty if we dont 
            // have a single lower case word, which means we 
            // need to treat the firstname as a part of lastname
            if(string.IsNullOrEmpty(middleName.Trim()))
            {
                // eg. De La Fontaine, Jean 
                // ParseFirstFormat('De La Fontaine') => 
                //  FName = 'De La'and LName = 'Fontaine' 
                //  so in 2nd format LName = 'De La Fontaine'
                lastName = firstName + " " + lastName;
            }
            else
            {
                // eg. De la Fontaine, Jean 
                // ParseFirstFormat('De la Fontaine') => 
                //  FName = 'De',MName = 'la' amd LName = 'Fontaine' 
                //  so in 2nd format MName = 'De la'
                middleName = firstName + " " + middleName;
            }
            firstName = first;

            firstName = firstName.Trim();
            middleName = middleName.Trim();
            lastName = lastName.Trim();
        }

        /// <summary>
        /// Parses the third format.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="remainingName">Name of the remaining.</param>
        private void ParseThirdFormat(string title, string firstName, string remainingName)
        {
            //Format: von Last, Jr, First
            ParseSecondFormat(firstName, remainingName);
            title = title.Trim();
        }

        /// <summary>
        /// Determines if the word begins the with small case.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>
        ///     <c>true</c> if the word begins the with small case; otherwise <c>false</c>.
        /// </returns>
        private static bool BeginsWithSmallCase(string word)
        {
            return (word[0] >= 'a' && word[0] <= 'z');
        }

        #endregion
    }
}
