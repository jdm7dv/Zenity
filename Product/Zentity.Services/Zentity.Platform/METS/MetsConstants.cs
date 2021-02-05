// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System.Collections.Generic;

namespace Zentity.Platform
{
    internal static class MetsConstants
    {
        #region Fields

        public const string MetsNamespace = "http://www.loc.gov/METS/";
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        public const string XlinkNamespace = "http://www.w3.org/1999/xlink";
        public const string DublinCoreNamespace = "http://purl.org/dc/elements/1.1/";

        public const string DublinCorePrefix = "DC";

        public const string MimeType = "MIMETYPE";
        public const string MdWrap = "mdWrap";
        public const string XmlData = "xmlData";
        public const string TextXml = "text/xml";
        public const string MdType = "MDTYPE";
        public const string File = "file";
        public const string Flocat = "FLocat";
        public const string Href = "href";
        public const string Id = "ID";
        public const string Div = "div";
        public const string Fptr = "fptr";
        public const string DmdId = "DMDID";
        public const string AdmId = "ADMID";
        public const string FileId = "FILEID";
        public const string DmdSec = "dmdSec";
        public const string RightsMD = "rightsMD";

        public const string Item = "Item";

        public const int FileSize = 2048;

        private static Dictionary<string, string> _dcProperties;

        public static Dictionary<string, string> DcProperties
        {
            get
            {
                if (null == _dcProperties)
                {
                    LoadDcProperty();
                }
                return MetsConstants._dcProperties;
            }
            private set { _dcProperties = value; }
        }

        private static void LoadDcProperty()
        {
            DcProperties = new Dictionary<string, string>();
            DcProperties.Add("date", "DateCreated");
            DcProperties.Add("type", "ResourceType");
        }

        #endregion
    }
}
