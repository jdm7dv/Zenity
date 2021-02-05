// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.Xml;
    using System.Xml;
    using System.Xml.Linq;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// Represents the http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#section-element-node event.
    /// </summary>
    internal sealed class EventElement
    {
        #region Member Variables

        #region Private
        private XElement innerElement;
        RDFUriReference uri;
        int liCounter = 1;
        Subject subject;
        List<EventAttribute> attributes;
        List<EventElement> children;
        EventElement parent;
        IXmlLineInfo lineInfo;
        #endregion

        #endregion

        #region Constructors

        #region Internal

        /// <summary>
        /// Initializes a new instance of the EventElement class. 
        /// The constructor takes an XElement to initialize itself. 
        /// The XElement is the implementation of “element information item” 
        /// referred by the W3C specification.
        /// </summary>
        /// <param name="element">Instance of XElement class</param>
        /// <exception cref="System.UriFormatException">Format of uriString is invalid.</exception>
        /// <exception cref="System.ArgumentNullException">Given XElement object is nill.</exception>
        internal EventElement(XElement element)
        {
            if(element == null)
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "element"));

            innerElement = element;
            lineInfo = innerElement as IXmlLineInfo;
            try
            {
                uri = new RDFUriReference(innerElement.Name.NamespaceName + innerElement.Name.LocalName);

                if (!uri.InnerUri.IsWellFormedOriginalString())
                    throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgEventElementInvalidUri, LineInfo);
            }
            catch (UriFormatException ex)
            {
                throw new RdfXmlParserException(ex, Constants.ErrorMessageIds.MsgEventElementInvalidUri, LineInfo);
            }

            attributes = GetAttributes();
            children = GetChildren();
        }

        /// <summary>
        ///  Initializes a new instance of the EventElement class. 
        ///  The constructor takes an RDFUriReference to initialize itself.
        /// </summary>
        /// <param name="localName">Local Name.</param>
        /// <param name="nameSpace">Name space.</param>
        /// <param name="children">Child Elements</param>
        /// <exception cref="System.ArgumentNullException">uriString is null.</exception>
        /// <exception cref="System.UriFormatException">Format of uriString is invalid.</exception>
        internal EventElement(string localName, string nameSpace, IEnumerable<EventElement> children)
        {
            if (string.IsNullOrEmpty(nameSpace))
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, Resources.MsgNullArgument, "nameSpace"));

            innerElement = new XElement(XName.Get(localName, nameSpace));
            lineInfo = innerElement as IXmlLineInfo;

            try
            {
                uri = new RDFUriReference(innerElement.Name.NamespaceName + innerElement.Name.LocalName);
                if(!uri.InnerUri.IsWellFormedOriginalString())
                    throw new RdfXmlParserException(Constants.ErrorMessageIds.MsgEventElementInvalidUri, LineInfo);
            }
            catch (UriFormatException ex)
            {
                throw new RdfXmlParserException(ex, Constants.ErrorMessageIds.MsgEventElementInvalidUri, LineInfo);
            }

            attributes = GetAttributes();
            this.AddChildren(children);
        }

        #endregion

        #endregion

        #region Properties

        #region Internal

        /// <summary>
        /// Gets base Uri of the element.
        /// </summary>
        internal string BaseUri
        {
            get
            {
                return this.GetBaseUri();
            }

        }

        /// <summary>
        /// Gets Attribute of the element
        /// </summary>
        internal IEnumerable<EventAttribute> Attributes
        {
            get
            {
                return attributes;
            }
        }

        /// <summary>
        /// Gets or sets Uri of the element.
        /// </summary>
        internal RDFUriReference Uri
        {
            get
            {
                return uri;
            }
            set
            {
                uri = value;
            }
        }

        /// <summary>
        /// Gets language set for the element. If no value is given from the attributes, 
        /// the value is set to the value of the language accessor on the parent event 
        /// (either a Root Event or an Element Event), which may be the empty string.
        /// </summary>
        internal string Language
        {
            get
            {
                return this.GetLanguage(innerElement);
            }
        }

        /// <summary>
        /// Gets or sets the Parent of the element.
        /// </summary>
        internal EventElement Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        /// <summary>
        /// Gets children elements of the element.
        /// </summary>
        internal IEnumerable<EventElement> Children
        {
            get
            {
                return children;
            }
        }

        /// <summary>
        /// Gets or sets element Identifier.
        /// </summary>
        internal Subject Subject
        {
            get
            {
                return subject;
            }
            set
            {
                subject = value;
            }
        }

        /// <summary>
        /// Gets text event assigned to element.
        /// </summary>
        internal string StringValue
        {
            get
            {
                return GetTextEvent();
            }
        }

        /// <summary>
        /// Gets line information of the element in the RDF Xml.
        /// </summary>
        internal string LineInfo
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, Resources.LineInfo, lineInfo.LineNumber, lineInfo.LinePosition);
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Returns the current XElement as string.
        /// </summary>
        /// <returns>
        /// Returns a string that represents XElement.
        /// </returns>
        public override string ToString()
        {
            return innerElement.ToString();
        }

        #endregion

        #region Internal

        /// <summary>
        /// Applies Expansion rules.
        /// </summary>
        /// <returns>a new RDFUriReference with current li-counter</returns>
        internal RDFUriReference ExpandList()
        {
            //List Expansion Rules:
            //For the given element e, create a new RDF URI reference 
            //u := concat("http://www.w3.org/1999/02/22-rdf-syntax-ns#_", e.li-counter), 
            //increment the e.li-counter property by 1 and return u.

            RDFUriReference u = new RDFUriReference(Constants.RdfNamespace + "_" + liCounter);

            liCounter++;

            return u;
        }

        #endregion

        #region Private


        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <returns>Returns a string representation of the BaseUri</returns>
        private string GetBaseUri()
        {
            //TODO: Revisit logic to handle intermediate BaseUri declarations
            XAttribute xmlBaseAttr = FindXmlBaseAttribute(innerElement);

            if (xmlBaseAttr != null)
            {
                return xmlBaseAttr.Value;
            }
            else
            {
                return GetDefaultBaseUri();
            }
        }

        /// <summary>
        /// Finds the XML base attribute for a specified XElement.
        /// </summary>
        /// <param name="element">The XElement.</param>
        /// <returns>Returns the XAttribute.</returns>
        private XAttribute FindXmlBaseAttribute(XElement element)
        {
            if (element != null)
            {
                XAttribute xmlBaseAttr = element.Attributes().Where(
                tuple => tuple.Name.NamespaceName == Constants.XmlNamespace &&
                tuple.Name.LocalName == Constants.XmlBase).FirstOrDefault();

                if (xmlBaseAttr != null)
                {
                    return xmlBaseAttr;
                }
                else
                {
                    return FindXmlBaseAttribute(element.Parent);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns>Returns a List of Event Attributes</returns>
        private List<EventAttribute> GetAttributes()
        {
            // Made from the value of element information item property [attributes] which is a 
            // set of attribute information items. If this set contains an attribute information 
            // item xml:lang ( [namespace name] property with the value 
            // "http://www.w3.org/XML/1998/namespace" and [local name] property value "lang") it 
            // is removed from the set of attribute information items and the ·language· accessor 
            // is set to the [normalized-value] property of the attribute information item. All 
            // remaining reserved XML Names (See Name in XML 1.0) are now removed from the set. 
            // These are, all attribute information items in the set with property [prefix] 
            // beginning with xml (case independent comparison) and all attribute information 
            // items with [prefix] property having no value and which have [local name] beginning 
            // with xml (case independent comparison) are removed. Note that the [base URI] 
            // accessor is computed by XML Base before any xml:base attribute information item is 
            // deleted. The remaining set of attribute information items are then used to construct 
            // a new set of Attribute Events which is assigned as the value of this accessor.

            List<EventAttribute> attributeList = new List<EventAttribute>();

            var xmlAttrs = innerElement.Attributes().
                Where(tuple =>
                !string.IsNullOrEmpty(tuple.Name.NamespaceName) &&
                !tuple.Name.NamespaceName.Equals(Constants.XmlNamespace) &&
                !tuple.IsNamespaceDeclaration);

            string expandedName = null;

            foreach (XAttribute xmlAttr in xmlAttrs)
            {
                if (ResolveReference(xmlAttr.Value, out expandedName))
                {
                    XAttribute expandedAttr = new XAttribute(xmlAttr.Name, expandedName);                    
                    attributeList.Add(new EventAttribute(expandedAttr, this));
                }
                else
                {
                    attributeList.Add(new EventAttribute(xmlAttr, this));
                }
            }

            return attributeList;
        }

        /// <summary>
        /// Resolves the reference for a given value.
        /// </summary>
        /// <param name="referenceValue">The reference value.</param>
        /// <param name="expandedName">Expanded Name.</param>
        /// <returns>Returns true if referenceValue is valid or else false.</returns>
        private bool ResolveReference(string referenceValue, out string expandedName)
        {
            expandedName = null;

            string[] xmlNameParts = referenceValue.Split(':');

            if (xmlNameParts.Length == 1)  // not a QName
                return false;

            if (xmlNameParts.Length > 2) //Invalid QName
                throw new ArgumentException("Invalid Value : " + referenceValue);

            string xmlNs = xmlNameParts[0];
            string xmlName = xmlNameParts[1];

            List<NamespaceDefinition> namespaceDefs = new List<NamespaceDefinition>();

            GetNamespacesInScope(innerElement, namespaceDefs);

            foreach (NamespaceDefinition ndef in namespaceDefs)
            {
                if (ndef.NamespacePrefix.Equals(xmlNs, StringComparison.OrdinalIgnoreCase))
                {
                    if (ndef.NamespaceUri.EndsWith("/") ||
                        ndef.NamespaceUri.EndsWith("#"))
                    {                        
                        expandedName = ndef.NamespaceUri + xmlName;
                    }
                    else
                    {
                        expandedName = ndef.NamespaceUri + "/" + xmlName;
                    }
                        
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <returns>Returns a list of Event Element</returns>
        private List<EventElement> GetChildren()
        {
            List<EventElement> childrenList = new List<EventElement>();

            IEnumerable<XElement> eventlementList =
                innerElement.Elements().
                Where(tuple => !string.IsNullOrEmpty(tuple.Name.NamespaceName));

            foreach (XElement element in eventlementList)
            {
                EventElement child = new EventElement(element);
                child.Parent = this;
                childrenList.Add(child);
            }

            return childrenList;
        }

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <param name="element">The XElement.</param>
        /// <returns>Returns a string representing the language.</returns>
        private string GetLanguage(XElement element)
        {
            if (element != null)
            {
                XAttribute langAttr = element.Attributes().
                   Where(tuple => 
                       tuple.Name.NamespaceName == Constants.XmlNamespace &&
                       tuple.Name.LocalName == Constants.XmlLang).FirstOrDefault();


                if (langAttr != null)
                    return langAttr.Value;
                else if (element.Parent != null)
                {
                    return GetLanguage(element.Parent);

                }
            }

            return null;
        }

        /// <summary>
        /// Gets the text event.
        /// </summary>
        /// <returns>Returns a string.</returns>
        private string GetTextEvent()
        {
            string str = "";

            foreach (XNode node in innerElement.Nodes())
            {
                //TODO: Possible If cases need to be identified here.
                if (node.NodeType != XmlNodeType.Comment && node.NodeType != XmlNodeType.ProcessingInstruction)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        str += GetNormalizedXmlString(node as XElement);
                    }
                    else
                    {
                        str += node.ToString().Trim();
                    }
                }
            }

            return str;
        }

        /// <summary>
        /// Adds the children elements.
        /// </summary>
        /// <param name="elements">List of Event Element.</param>
        private void AddChildren(IEnumerable<EventElement> elements)
        {
            if (elements != null)
            {
                foreach (EventElement element in elements)
                {
                    innerElement.Add(element.innerElement);
                }

                children = GetChildren();
            }
        }

        /// <summary>
        /// Gets the default base URI.
        /// </summary>
        /// <returns>Returns the default base URI.</returns>
        private static string GetDefaultBaseUri()
        {
            string defaultBaseUri = ConfigurationManager.AppSettings[Constants.DefaultBaseUriKey];
            if (string.IsNullOrEmpty(defaultBaseUri))
                defaultBaseUri = Constants.DefaultBaseUri;

            return defaultBaseUri;
        }

        /// <summary>
        /// Gets the normalized XML string.
        /// </summary>
        /// <param name="element">The XElement.</param>
        /// <returns>Returns the normalized xml string.</returns>
        private static string GetNormalizedXmlString(XElement element)
        {
            string xmlLiteralValue = element.ToString();
            if (element != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlLiteralValue);
                XmlDsigC14NTransform t2 = new XmlDsigC14NTransform();
                t2.LoadInput(xmlDoc);
                var w = t2.GetOutput() as MemoryStream;
                byte[] buffer = new byte[w.Length];
                w.Read(buffer, 0, (int)w.Length);
                xmlLiteralValue = System.Text.Encoding.Default.GetString(buffer);
            }

            return xmlLiteralValue;
        }
        
        /// <summary>
        /// Gets the collection of NamespaceAlias-NamespaceUri in scope at the specified Xml element.
        /// </summary>
        /// <param name="element">The XElement.</param>
        /// <param name="definitions">The namespace definitions.</param>
        private static void GetNamespacesInScope(XElement element, ICollection<NamespaceDefinition> definitions)
        {
            List<NamespaceDefinition> namespaceDefs =
                element.Attributes()
                 .Where(tuple => tuple.IsNamespaceDeclaration)
                 .Select((tuple) => new NamespaceDefinition()
                     {
                         NamespacePrefix = tuple.Name.LocalName,
                         NamespaceUri = tuple.Value
                     }
                 ).ToList();

            foreach (NamespaceDefinition localDef in namespaceDefs)
            {
                if (definitions.Contains(localDef, new NamespacePrefixComparer()))
                {
                    foreach (NamespaceDefinition globalDef in definitions)
                    {
                        if (globalDef.NamespacePrefix.Equals(localDef.NamespacePrefix))
                        {
                            globalDef.NamespacePrefix = localDef.NamespacePrefix;
                        }
                    }
                }
                else
                {
                    definitions.Add(localDef);
                }
            }

            if (element.Parent != null)
                GetNamespacesInScope(element.Parent, definitions);

        }

        #endregion

        #endregion

        #region Private Classes

        /// <summary>
        /// Namespace definition
        /// </summary>
        private class NamespaceDefinition
        {
            /// <summary>
            /// Gets or sets the namespace prefix.
            /// </summary>
            /// <value>The namespace prefix.</value>
            public string NamespacePrefix { get; set; }
            /// <summary>
            /// Gets or sets the namespace URI.
            /// </summary>
            /// <value>The namespace URI.</value>
            public string NamespaceUri { get; set; }
        }

        /// <summary>
        /// Namespace Prefix Comparer
        /// </summary>
        private class NamespacePrefixComparer : IEqualityComparer<NamespaceDefinition>
        {
            #region IEqualityComparer<NamespaceDefinition> Members

            /// <summary>
            /// Checks the namespace prefix of the two namespace definition.
            /// </summary>
            /// <param name="x">Subject Namespce definition.</param>
            /// <param name="y">Object Namespace definition.</param>
            /// <returns>Returns true if both subject and objects namespace prefix are equal or else false.</returns>
            public bool Equals(NamespaceDefinition x, NamespaceDefinition y)
            {
                return x.NamespacePrefix.Equals(y.NamespacePrefix);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The namespace definition object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(NamespaceDefinition obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        #endregion
    }
}
