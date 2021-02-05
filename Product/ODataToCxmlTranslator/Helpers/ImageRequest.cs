// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;
    using System.Text.RegularExpressions;

    internal class ImageRequest
    {
        #region Constructors, Finalizer and Dispose

        public ImageRequest(Uri url)
        {
            Match match = s_matcher.Match(url.AbsolutePath);

            if (match.Groups.Count != 5)
            {
                throw new ArgumentException();
            }

            string firstPart = match.Groups[1].Value;
            int lastSlash = firstPart.LastIndexOf('/');
            if(lastSlash >= 0)
            {
                firstPart = firstPart.Substring(lastSlash + 1);
            }
            this.DzcName = firstPart;

            this.Level = int.Parse(match.Groups[2].Value);
            this.X = int.Parse(match.Groups[3].Value);
            this.Y = int.Parse(match.Groups[4].Value);

        }

        #endregion

        #region Public Properties

        public string DzcName { get; private set; }
        public int Level { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        #endregion

        #region Private Fields

        static readonly Regex s_matcher = new Regex("(.*)_files/(.*)/(.*)_(.*).jpg", RegexOptions.Compiled
            | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        #endregion
    }
}
