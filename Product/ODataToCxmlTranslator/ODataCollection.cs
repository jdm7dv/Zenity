// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;
    using System.Configuration;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Principal;
    using System.Web;
    using System.Xml.Linq;

    public class ODataCollectionSource
    {
        #region Constructors, Finalizer and Dispose

        public ODataCollectionSource()
        {
        }

        #endregion

        #region Public Methods

        public static Collection MakeOdataCollection(string odataUrl)
        {
            ODataFeedBuilder builder = new ODataFeedBuilder(odataUrl, HttpContext.Current.Request.Url.LocalPath);
            return builder.GetCollection();
        }

        #endregion
    }

    class ODataFeedBuilder
    {
        #region Constructors, Finalizer and Dispose

        public ODataFeedBuilder(string odataUrl, string collectionBaseUrl)
        {
            m_odataUrl = odataUrl;
            m_collectionBaseUrl = collectionBaseUrl;
        }

        #endregion

        #region Public Methods

        public Collection GetCollection()
        {
            m_root = DownloadOdata(m_odataUrl);
            if (IsAtomPub(m_root))
            {
                return new AtomPub(m_root, m_collectionBaseUrl).MakeCollection();
            }
            else if (IsAtom(m_root))
            {
                return MakeCollectionFromFeed(m_root);
            }

            throw new ApplicationException("Root element is not <feed> or <service>");
        }

        #endregion

        #region Private Static Methods

        private static XElement DownloadOdata(string url)
        {
            string odata;

            //For internal sites, e.g. Sharepoint 2010, we need to authenticate using the caller's identity
            // and also specify useDefaultCredentials in order to download data from the site.
            // Therefore in IIS, "Anonymous Authentication" must be disabled and "Windows Authentication" enabled.
            //However, the Pivot client claims "unauthorized" against IIS if Anonymous is not on.
            // The Silverlight control works correctly if only Windows authentication is on.

            IIdentity runningUser = HttpContext.Current.User.Identity;
            using (runningUser.IsAuthenticated ? ((WindowsIdentity)runningUser).Impersonate() : null)
            using (WebClient client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                client.Encoding = System.Text.Encoding.UTF8;
 
                //TODO: Evaluate whether to use async to release this thread back to the IIS threadpool while
                // waiting for download. For details, see http://msdn.microsoft.com/en-us/magazine/cc164128.aspx
                odata = client.DownloadString(url);
            }

            using (StringReader reader = new StringReader(odata))
            {
                return XElement.Load(reader);
            }
        }

        private static bool IsAtomPub(XElement root)
        {
            return "service" == root.Name.LocalName;
        }

        private static bool IsAtom(XElement root)
        {
            return "feed" == root.Name.LocalName;
        }

        private Collection MakeCollectionFromFeed(XElement root)
        {
            this.m_collection = new Collection();
            this.m_collection.Name = root.Element(nsAtom + "title").Value;
            if (!string.IsNullOrWhiteSpace(this.m_collection.Name))
                this.m_collection.HrefBase = root.Element(nsAtom + "id").Value.Replace(this.m_collection.Name, string.Empty);

            this.AddEntriesToCollection(this.m_collection, root);

            //See if there is continuation data:
            const int countMaxIterations_c = 5;
            int iterationCount = 0;
            string maxPageTraversalCount = ConfigurationManager.AppSettings["MaxPageTraversalCount"];
            if (!string.IsNullOrEmpty(maxPageTraversalCount))
            {
                if (Int32.TryParse(maxPageTraversalCount, out iterationCount))
                {
                    if (iterationCount <= 0)
                    {
                        iterationCount = countMaxIterations_c;
                    }
                }
                else
                {
                    iterationCount = countMaxIterations_c;
                }
            }
            else
            {
                iterationCount = countMaxIterations_c;
            }


            for (int i = 1; i < iterationCount; i++)
            {
                string href = GetLinkRelNext(root);
                if (string.IsNullOrEmpty(href))
                {
                    break;
                }

                //TODO: Load these ahead of time, asynchronously.
                root = DownloadOdata(href);
                this.AddEntriesToCollection(this.m_collection, root);
            }

            return this.m_collection;
        }

        private void AddEntriesToCollection(Collection collection, XElement root)
        {
            foreach (XElement element in root.Elements(nsAtom + "entry"))
            {
                string title = element.Element(nsAtom + "title").Value;

                var propertiesElement = FindPropertiesElement(element);
                var nameElement = propertiesElement.Element(nsData + "Title");
                var idElement = propertiesElement.Element(nsData + "Id");
                if (null != nameElement)
                {
                    title = nameElement.Value;
                }

                if (null != idElement && idElement.Value != null)
                {
                    collection.AddItem(idElement.Value,
                     title,
                     null, null, null,
                     this.Entry(element).ToArray());
                }
                else
                {
                    collection.AddItem(
                        title,
                        null, null, null,
                        this.Entry(element).ToArray());
                }
            }
        }

        private static string GetLinkRelNext(XElement root)
        {
            var linksRelNext = root.Elements(nsAtom + "link").Where(link => ("next" == link.Attribute("rel").Value));
            if (null != linksRelNext)
            {
                foreach (XElement link in linksRelNext)
                {
                    return link.Attribute("href").Value; //Just return the first one.
                }
            }
            return null;
        }

        private IEnumerable<Facet> Entry(XElement element)
        {
            foreach (var it in EntryProperties(element))
            {
                yield return it;
            }

            //Get the links off the entry
            List<FacetHyperlink> links = new List<FacetHyperlink>();
            foreach (var link in element.Elements(nsAtom + "link"))
            {
                string title = link.Attribute("title").Value;
                string href = link.Attribute("href").Value;

                if (!string.IsNullOrWhiteSpace(this.m_collection.HrefBase))
                    href = this.m_collection.HrefBase + href;

                links.Add(new FacetHyperlink(title, href));
            }
            if (links.Count > 0)
            {
                yield return new Facet("Links:", links.ToArray());
            }
        }

        private static IEnumerable<Facet> EntryProperties(XElement entry)
        {
            var propertiesElement = FindPropertiesElement(entry);
            if (null == propertiesElement)
            {
                yield break;
            }

            foreach (var property in propertiesElement.Descendants())
            {
                //TODO:Handle the property having child nodes.

                Facet facet = FacetFromProperty(property, entry);
                if (null != facet)
                {
                    yield return facet;
                }
            }
        }

        //TODO: Make this overridable by derived classes, or have a way to provide an alternate implementation.
        private static Facet FacetFromProperty(XElement property, XElement parentEntry)
        {
            string category = property.Name.LocalName;
            if (Facet.IsReservedCategory(category))
            {
                category += "_";
            }

            FacetType facetType = FacetType.Text;
            object value = null;
            try
            {
                value = PivotFacetFromOdataProperty(property, out facetType);
            }
            catch
            {
                var id = parentEntry.Element(nsAtom + "id");
                string idValue = (null == id) ? string.Empty : id.Value;

                Console.Error.WriteLine("id \"{0}\", property {1} contains bad value \"{2}\"",
                    idValue, category, property.Value);
            }

            return (null == value) ? null : new Facet(category, facetType, value);
        }


        private static XElement FindPropertiesElement(XElement element)
        {
            var propertiesElements = element.Descendants(nsMetadata + "properties");
            if (null != propertiesElements)
            {
                foreach (var el in propertiesElements)
                {
                    return el; //Obviously this only returns the first one. That's what we want.
                }
            }
            return null;
        }

        private static object PivotFacetFromOdataProperty(XElement prop, out FacetType facetType)
        {
            string textValue = prop.Value;

            XAttribute typeAttr = prop.Attribute(nsMetadata + "type");
            string attr = (null == typeAttr) ? null : typeAttr.Value;

            switch (attr)
            {
                default:
                    facetType = FacetType.Text;
                    if (string.IsNullOrEmpty(textValue))
                    {
                        return null;
                    }
                    return textValue;

                case "Edm.Int32":
                    facetType = FacetType.Number;
                    if (string.IsNullOrEmpty(textValue))
                    {
                        return null;
                    }
                    return int.Parse(textValue);

                case "Edm.DateTime":
                    facetType = FacetType.DateTime;
                    if (string.IsNullOrEmpty(textValue))
                    {
                        return null;
                    }
                    return DateTime.Parse(textValue);

                //TODO: Other Edm types
            }
        }

        #endregion

        #region Private Fields

        static readonly XNamespace nsAtom = "http://www.w3.org/2005/Atom";
        static readonly XNamespace nsData = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        static readonly XNamespace nsMetadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        string m_odataUrl;
        string m_collectionBaseUrl;
        XElement m_root;
        private Collection m_collection;

        #endregion
    }

    internal class AtomPub
    {
        #region Constructor

        public AtomPub(XElement root, string urlPivotServer)
        {
            m_root = root;
            m_urlPivotServer = urlPivotServer;
        }

        #endregion

        #region Public Methods

        public Collection MakeCollection()
        {
            ReadRoot();

            Collection collection = new Collection();
            collection.Name = m_baseUrl;

            foreach (XElement workspace in m_root.Elements(nsApp + "workspace"))
            {
                string workSpaceTitle = GetAtomTitle(workspace);

                foreach (XElement coll in workspace.Elements(nsApp + "collection"))
                {
                    string href = coll.Attribute("href").Value;
                    string title = GetAtomTitle(coll);
                    collection.AddItem(title, FormatHref(href), null, null);
                }
            }

            return collection;
        }

        #endregion

        #region Private Static Methods

        private void ReadRoot()
        {
            if (0 != string.Compare("service", m_root.Name.LocalName, true))
            {
                throw new ApplicationException("Root element must be \"service\"");
            }

            ReadRootBaseAttribute();
        }

        private void ReadRootBaseAttribute()
        {
            XAttribute attrBase = m_root.Attribute(XNamespace.Xml + "base");
            if (null != attrBase)
            {
                m_baseUrl = attrBase.Value;
            }
        }

        private string GetHrefBase()
        {
            return string.Format("{0}?src={1}", m_urlPivotServer, m_baseUrl);
        }

        private string GetAtomTitle(XElement element)
        {
            return element.Element(nsAtom + "title").Value;
        }

        private string FormatHref(string href)
        {
            return string.Format("{0}?src={1}{2}", m_urlPivotServer, m_baseUrl, href);
        }

        #endregion

        #region Private Fields

        static readonly XNamespace nsAtom = "http://www.w3.org/2005/Atom";
        static readonly XNamespace nsApp = "http://www.w3.org/2007/app";

        XElement m_root;
        string m_urlPivotServer;
        string m_baseUrl;

        #endregion
    }
}
