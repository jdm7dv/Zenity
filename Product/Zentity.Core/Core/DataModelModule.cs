// *********************************************************
// 
//     Copyright (c) Microsoft. All rights reserved.
//     This code is licensed under the Apache License, Version 2.0.
//     THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//     ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//     IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//     PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// 
// *********************************************************

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Linq;
using Zentity.Rdf.Xml;
using System.Xml.Linq;
using Zentity.Rdf.Concepts;

namespace Zentity.Core
{
    /// <summary>
    /// Represents a module of the complete data model.
    /// </summary>
    /// <example>
    /// <code>
    ///using Zentity.Core;
    ///using System.Xml;
    ///using System;
    ///using System.Text;
    ///using System.IO;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Console.WriteLine(&quot;Modules present in the model:&quot;);
    ///                foreach (DataModelModule module in context.DataModel.Modules)
    ///                {
    ///                    Console.WriteLine(&quot;\tModule Id: [{0}]&quot;, module.Id);
    ///                    Console.WriteLine(&quot;\t\tNameSpace: [{0}]&quot;, module.NameSpace);
    ///                    Console.WriteLine(&quot;\t\tUri: [{0}]&quot;, module.Uri);
    ///                    Console.WriteLine(&quot;\t\tIsMsShipped: [{0}]&quot;, module.IsMsShipped);
    ///                    Console.WriteLine(&quot;\t\tDescription: [{0}]&quot;, module.Description);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed class DataModelModule
    {
        #region Fields

        DataModel parent;
        string nameSpace;
        string uri;
        Guid id;
        string description;
        bool isMsShipped;
        ResourceTypeCollection resourceTypes;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the namespace of the data model module.
        /// </summary>
        public string NameSpace
        {
            get { return nameSpace; }
            set { nameSpace = value; }
        }

        /// <summary>
        /// Gets or sets the Uri of the data model module.
        /// </summary>
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        /// <summary>
        /// Gets the Id of the data model module.
        /// </summary>
        public Guid Id
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Gets or sets the Description of the data model module.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets the flag that indicates whether the module is shipped by Microsoft.
        /// </summary>
        public bool IsMsShipped
        {
            get { return isMsShipped; }
            internal set { isMsShipped = value; }
        }

        /// <summary>
        /// Gets the parent data model for the module.
        /// </summary>
        public DataModel Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /// <summary>
        /// Gets the resource types collection of the module.
        /// </summary>
        public ResourceTypeCollection ResourceTypes
        {
            get
            {
                return resourceTypes;
            }
        }

        /// <summary>
        /// Gets the associations present in this module. An association is considered to 
        /// be in the module if both its object and subject navigation properties are 
        /// present in the same module. Any dangling association (with no subject or object 
        /// navigation property) is not returned in this set.
        /// </summary>
        public IEnumerable<Association> Associations
        {
            get
            {
                List<Association> associations = new List<Association>();

                // Check each navigation property that is reachable from the 
                // DataModelModule --> ResourceType path.
                foreach (NavigationProperty navigationProperty in this.ResourceTypes.
                    SelectMany(tuple => tuple.NavigationProperties))
                {
                    // If navigation property has association.
                    if (navigationProperty.Association != null)
                    {
                        // Cache the association reference.
                        Association association = navigationProperty.Association;

                        // Locate the other end. It could be a null reference.
                        NavigationProperty otherEnd =
                            (navigationProperty.Direction == AssociationEndType.Subject) ?
                            association.ObjectNavigationProperty :
                            association.SubjectNavigationProperty;

                        // If not dangling.
                        if (otherEnd != null)
                        {
                            // Check that the other end belongs to this data model module.
                            // Avoid dangling navigation properties and resource types.
                            if (otherEnd.Parent != null && otherEnd.Parent.Parent != null &&
                                otherEnd.Parent.Parent == this)
                            {
                                // Avoid duplicate entries.
                                if (!associations.Contains(association))
                                    associations.Add(association);
                            }
                        }
                    }
                }

                return associations;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of data model module.
        /// </summary>
        public DataModelModule()
        {
            this.id = Guid.NewGuid();
            this.resourceTypes = new ResourceTypeCollection(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Parses object model defined in RDFS/XML to DataModelModule object.
        /// </summary>
        /// <param name="baseResourceType">Instance of Base ResourceType.</param>
        /// <param name="rdfUri">RDF/XML file Uri.</param>
        /// <param name="xsdUri">XSD file Uri.</param>
        /// <param name="namespaceName">User defined Namespace.</param>
        /// <param name="isStrictExecutionMode">Execution Mode (Strict/Loose).</param>
        /// <returns>Parsed object of DataModelModule class.</returns>
        /// <exception cref="Zentity.Rdf.Xml.RdfXmlParserException"/>
        /// <exception cref="Zentity.Core.RdfsException"/>
        /// <example> This example shows how to create a <see cref="Zentity.Core.DataModelModule"/>
        /// from an RDF Schema document. Though, the example uses reflection to show how to use the 
        /// generated assembly, we recommend creating clients that are compiled with references to 
        /// the extensions assembly to avoid the reflection based code.
        /// <br/>
        /// The listing below shows the contents of RDF Schema document. The example assumes that 
        /// this document is saved as 'C:\Sample.rdf'.
        /// <code lang="xml">
        ///&lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;!DOCTYPE rdf:RDF [
        ///  &lt;!ENTITY dt &quot;http://contoso.com/2008/&quot; &gt;
        ///]&gt;
        ///&lt;rdf:RDF
        ///   xmlns:rdf=&quot;http://www.w3.org/1999/02/22-rdf-syntax-ns#&quot;
        ///   xmlns:rdfs=&quot;http://www.w3.org/2000/01/rdf-schema#&quot;
        ///   xml:base=&quot;http://www.sample.org/&quot;&gt;
        ///
        ///  &lt;rdfs:Class rdf:about=&quot;Person&quot;&gt;
        ///  &lt;/rdfs:Class&gt;
        ///
        ///  &lt;rdfs:Class rdf:about=&quot;Employee&quot;&gt;
        ///    &lt;rdfs:subClassOf rdf:resource=&quot;Person&quot;/&gt;
        ///  &lt;/rdfs:Class&gt;
        ///
        ///  &lt;rdfs:Class rdf:about=&quot;Book&quot;&gt;
        ///  &lt;/rdfs:Class&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;AuthoredBy&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;Person&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Book&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;EditedBy&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;Person&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Book&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;Name&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;&amp;dt;string128&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Person&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;Designation&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;&amp;dt;string256&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Employee&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;//Book/Name&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;&amp;dt;string128&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Book&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///&lt;/rdf:RDF&gt;
        /// </code>
        /// <br/>
        /// The listing below shows the contents of XSD that contains additional type definitions
        /// used in the example. The example assumes that this document is saved as 'C:\Sample.xsd'.
        /// <code lang="xml">
        ///&lt;xs:schema
        ///  targetNamespace=&quot;http://contoso.com/2008/&quot;
        ///  xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///
        ///  &lt;xs:simpleType name=&quot;string128&quot; id=&quot;string128&quot;&gt;
        ///    &lt;xs:restriction base=&quot;xs:string&quot;&gt;
        ///      &lt;xs:maxLength value=&quot;128&quot;/&gt;
        ///    &lt;/xs:restriction&gt;
        ///  &lt;/xs:simpleType&gt;
        ///
        ///  &lt;xs:simpleType name=&quot;string256&quot; id=&quot;string256&quot;&gt;
        ///    &lt;xs:restriction base=&quot;xs:string&quot;&gt;
        ///      &lt;xs:maxLength value=&quot;256&quot;/&gt;
        ///    &lt;/xs:restriction&gt;
        ///  &lt;/xs:simpleType&gt;
        ///
        ///&lt;/xs:schema&gt;
        /// </code>
        /// <br/>
        /// Example:
        /// <code>
        ///using System;
        ///using System.Linq;
        ///using Zentity.Core;
        ///using System.Reflection;
        ///using System.Collections;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        static string nameSpace = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///        const string connectionStringFormat = @&quot;provider=System.Data.SqlClient;
        ///                metadata=res://{0}; provider connection string='Data Source=.;
        ///                Initial Catalog=Zentity;Integrated Security=True;MultipleActiveResultSets=True'&quot;;
        ///        const string rdfsPath = @&quot;C:\Sample.rdf&quot;;
        ///        const string dataTypesXsdPath = @&quot;C:\Sample.xsd&quot;;
        ///        static string extensionsAssemblyName = nameSpace;
        ///
        ///        public static void Main(string[] args)
        ///        {
        ///            Assembly extensions = SynchronizeAndGenerateAssembly();
        ///            CreateRepositoryItems(extensions);
        ///            FetchRepositoryItems(extensions);
        ///        }
        ///
        ///        private static void FetchRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Console.WriteLine(&quot;Getting books...&quot;);
        ///                Type resourceTypeBook = extensionsAssembly.GetType(nameSpace + &quot;.Book&quot;);
        ///                MethodInfo ofTypeMethod = context.Resources.GetType().GetMethod(&quot;OfType&quot;).
        ///                    MakeGenericMethod(resourceTypeBook);
        ///                var customTypeInstances = ofTypeMethod.Invoke(context.Resources, null);
        ///                foreach (Resource book in (IEnumerable)customTypeInstances)
        ///                    Console.WriteLine(book.Id);
        ///
        ///                Console.WriteLine(&quot;\nGetting persons...&quot;);
        ///                Type resourceTypePerson = extensionsAssembly.GetType(nameSpace + &quot;.Person&quot;);
        ///                ofTypeMethod = context.Resources.GetType().GetMethod(&quot;OfType&quot;).
        ///                    MakeGenericMethod(resourceTypePerson);
        ///                customTypeInstances = ofTypeMethod.Invoke(context.Resources, null);
        ///                foreach (Resource person in (IEnumerable)customTypeInstances)
        ///                    Console.WriteLine(person.Id);
        ///
        ///                Console.WriteLine(&quot;\nGetting AuthoredBy relationships...&quot;);
        ///                NavigationProperty authoredByProperty = context.DataModel.Modules[nameSpace].
        ///                    ResourceTypes[&quot;Book&quot;].NavigationProperties[&quot;AuthoredBy&quot;];
        ///                Predicate predicate = context.Predicates.
        ///                    Where(tuple =&gt; tuple.Id == authoredByProperty.Association.PredicateId).First();
        ///                predicate.Relationships.Load();
        ///                foreach (Relationship rel in predicate.Relationships)
        ///                    Console.WriteLine(&quot;[{0}] &lt;--{1}--&gt; [{2}]&quot;, rel.Subject.Id, predicate.Name,
        ///                        rel.Object.Id);
        ///            }
        ///        }
        ///
        ///        private static void CreateRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Type resourceTypeBook = extensionsAssembly.GetType(nameSpace + &quot;.Book&quot;);
        ///                PropertyInfo propertyAuthoredBy = resourceTypeBook.GetProperty(&quot;AuthoredBy&quot;);
        ///                Type resourceTypePerson = extensionsAssembly.GetType(nameSpace + &quot;.Person&quot;);
        ///
        ///                var aPerson = Activator.CreateInstance(resourceTypePerson);
        ///                var aBook = Activator.CreateInstance(resourceTypeBook);
        ///                var anotherBook = Activator.CreateInstance(resourceTypeBook);
        ///
        ///                // AuthoredBy is actually a an EntityCollection&lt;Person&gt;.
        ///                var instanceAuthoredBy = propertyAuthoredBy.GetValue(aBook, null);
        ///                Type entityCollectionOfPerson = instanceAuthoredBy.GetType();
        ///                // Get the &quot;Add&quot; method.
        ///                MethodInfo methodAdd = entityCollectionOfPerson.GetMethod(&quot;Add&quot;);
        ///                // Add author to book.
        ///                methodAdd.Invoke(instanceAuthoredBy, new object[] { aPerson });
        ///
        ///                instanceAuthoredBy = propertyAuthoredBy.GetValue(anotherBook, null);
        ///                methodAdd.Invoke(instanceAuthoredBy, new object[] { aPerson });
        ///
        ///                // Save the entities to repository. 
        ///                context.AddToResources((Resource)aPerson);
        ///                context.SaveChanges();
        ///            }
        ///        }
        ///
        ///        private static Assembly SynchronizeAndGenerateAssembly()
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, &quot;Zentity.Core&quot;)))
        ///            {
        ///                ResourceType defaultBaseResourceType = context.DataModel.
        ///                    Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
        ///
        ///                DataModelModule module = DataModelModule.CreateFromRdfs(defaultBaseResourceType,
        ///                    rdfsPath, dataTypesXsdPath, nameSpace, false);
        ///
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Synchronize to alter the database schema.
        ///                // This method sometimes takes a few minutes to complete depending on the actions
        ///                // taken by other modules (such as change history logging) in response to schema
        ///                // changes. Everything happens in a single transaction. Make sure that the timeout 
        ///                // values are set correct for the command and transaction. Transaction timeout is 
        ///                // controlled from App.Config, Web.Config and machine.config configuration files.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate the module assembly to use.
        ///                byte[] rawAssembly = context.DataModel.GenerateExtensionsAssembly(
        ///                    extensionsAssemblyName, true, new string[] { nameSpace },
        ///                    new string[] { nameSpace }, null);
        ///
        ///                return Assembly.Load(rawAssembly);
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        public static DataModelModule CreateFromRdfs(ResourceType baseResourceType, string rdfUri,
            string xsdUri, string namespaceName, bool isStrictExecutionMode)
        {
            //Validate parameters
            if (baseResourceType == null)
                throw new ArgumentNullException("baseResourceType", RdfsResources.ExceptionArgumentIsNull);
            if (string.IsNullOrEmpty(rdfUri))
                throw new ArgumentNullException("rdfUri", RdfsResources.ExceptionArgumentIsNull);
            if (string.IsNullOrEmpty(namespaceName))
                throw new ArgumentNullException("namespaceName", RdfsResources.ExceptionArgumentIsNull);

            XDocument rdfDocument =
                XDocument.Load(rdfUri, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            XDocument xsdDocument = !string.IsNullOrEmpty(xsdUri.Trim()) ?
                xsdDocument = XDocument.Load(xsdUri) : null;

            return CreateFromRdfs(baseResourceType, rdfDocument, xsdDocument,
                namespaceName, isStrictExecutionMode);
        }

        /// <summary>
        /// Parses object model defined in RDFS/XML to DataModelModule object.
        /// </summary>
        /// <param name="baseResourceType">Instance of Base ResourceType.</param>
        /// <param name="rdfDocument">RDF/XML Document.</param>
        /// <param name="xsdDocument">XSD Documet.</param>
        /// <param name="namespaceName">User defined Namespace.</param>
        /// <param name="isStrictExecutionMode">Execution Mode (Strict/Loose).</param>
        /// <returns>Parsed object of DataModelModule class.</returns>
        /// <exception cref="Zentity.Rdf.Xml.RdfXmlParserException"/>
        /// <exception cref="Zentity.Core.RdfsException"/>
        /// <example> This example shows how to create a <see cref="Zentity.Core.DataModelModule"/>
        /// from an RDF Schema document. Though, the example uses reflection to show how to use the 
        /// generated assembly, we recommend creating clients that are compiled with references to 
        /// the extensions assembly to avoid the reflection based code.
        /// <br/>
        /// The listing below shows the contents of RDF Schema document. The example assumes that 
        /// this document is saved as 'C:\Sample.rdf'.
        /// <code lang="xml">
        ///&lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;!DOCTYPE rdf:RDF [
        ///  &lt;!ENTITY dt &quot;http://contoso.com/2008/&quot; &gt;
        ///]&gt;
        ///&lt;rdf:RDF
        ///   xmlns:rdf=&quot;http://www.w3.org/1999/02/22-rdf-syntax-ns#&quot;
        ///   xmlns:rdfs=&quot;http://www.w3.org/2000/01/rdf-schema#&quot;
        ///   xml:base=&quot;http://www.sample.org/&quot;&gt;
        ///
        ///  &lt;rdfs:Class rdf:about=&quot;Person&quot;&gt;
        ///  &lt;/rdfs:Class&gt;
        ///
        ///  &lt;rdfs:Class rdf:about=&quot;Employee&quot;&gt;
        ///    &lt;rdfs:subClassOf rdf:resource=&quot;Person&quot;/&gt;
        ///  &lt;/rdfs:Class&gt;
        ///
        ///  &lt;rdfs:Class rdf:about=&quot;Book&quot;&gt;
        ///  &lt;/rdfs:Class&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;AuthoredBy&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;Person&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Book&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;EditedBy&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;Person&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Book&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;Name&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;&amp;dt;string128&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Person&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;Designation&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;&amp;dt;string256&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Employee&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///  &lt;rdf:Property rdf:about=&quot;//Book/Name&quot;&gt;
        ///    &lt;rdfs:range rdf:resource=&quot;&amp;dt;string128&quot;/&gt;
        ///    &lt;rdfs:domain rdf:resource=&quot;Book&quot;/&gt;
        ///  &lt;/rdf:Property&gt;
        ///
        ///&lt;/rdf:RDF&gt;
        /// </code>
        /// <br/>
        /// The listing below shows the contents of XSD that contains additional type definitions
        /// used in the example. The example assumes that this document is saved as 'C:\Sample.xsd'.
        /// <code lang="xml">
        ///&lt;xs:schema
        ///  targetNamespace=&quot;http://contoso.com/2008/&quot;
        ///  xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///
        ///  &lt;xs:simpleType name=&quot;string128&quot; id=&quot;string128&quot;&gt;
        ///    &lt;xs:restriction base=&quot;xs:string&quot;&gt;
        ///      &lt;xs:maxLength value=&quot;128&quot;/&gt;
        ///    &lt;/xs:restriction&gt;
        ///  &lt;/xs:simpleType&gt;
        ///
        ///  &lt;xs:simpleType name=&quot;string256&quot; id=&quot;string256&quot;&gt;
        ///    &lt;xs:restriction base=&quot;xs:string&quot;&gt;
        ///      &lt;xs:maxLength value=&quot;256&quot;/&gt;
        ///    &lt;/xs:restriction&gt;
        ///  &lt;/xs:simpleType&gt;
        ///
        ///&lt;/xs:schema&gt;
        /// </code>
        /// <br/>
        /// Example:
        /// <code>
        ///using System;
        ///using System.Linq;
        ///using Zentity.Core;
        ///using System.Reflection;
        ///using System.Collections;
        ///using System.Xml.Linq;
        ///
        ///namespace ZentitySamples
        ///{
        ///    class Program
        ///    {
        ///        const string nameSpace = &quot;Zentity.Samples&quot;;
        ///        const string connectionStringFormat = @&quot;provider=System.Data.SqlClient;
        ///                metadata=res://{0}; provider connection string='Data Source=.;
        ///                Initial Catalog=Zentity;Integrated Security=True;MultipleActiveResultSets=True'&quot;;
        ///        const string rdfsPath = @&quot;C:\Sample.rdf&quot;;
        ///        const string dataTypesXsdPath = @&quot;C:\Sample.xsd&quot;;
        ///        const string extensionsAssemblyName = &quot;Extensions&quot;;
        ///        const string extensionsAssemblyFileName = &quot;Extensions.dll&quot;;
        ///
        ///        static void Main(string[] args)
        ///        {
        ///            Assembly extensions = SynchronizeAndGenerateAssembly();
        ///            CreateRepositoryItems(extensions);
        ///            FetchRepositoryItems(extensions);
        ///        }
        ///
        ///        private static void FetchRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Console.WriteLine(&quot;Getting books...&quot;);
        ///                Type resourceTypeBook = extensionsAssembly.GetType(&quot;Zentity.Samples.Book&quot;);
        ///                MethodInfo ofTypeMethod = context.Resources.GetType().GetMethod(&quot;OfType&quot;).
        ///                    MakeGenericMethod(resourceTypeBook);
        ///                var customTypeInstances = ofTypeMethod.Invoke(context.Resources, null);
        ///                foreach (Resource book in (IEnumerable)customTypeInstances)
        ///                    Console.WriteLine(book.Id);
        ///
        ///                Console.WriteLine(&quot;\nGetting persons...&quot;);
        ///                Type resourceTypePerson = extensionsAssembly.GetType(&quot;Zentity.Samples.Person&quot;);
        ///                ofTypeMethod = context.Resources.GetType().GetMethod(&quot;OfType&quot;).
        ///                    MakeGenericMethod(resourceTypePerson);
        ///                customTypeInstances = ofTypeMethod.Invoke(context.Resources, null);
        ///                foreach (Resource person in (IEnumerable)customTypeInstances)
        ///                    Console.WriteLine(person.Id);
        ///
        ///                Console.WriteLine(&quot;\nGetting AuthoredBy relationships...&quot;);
        ///                NavigationProperty authoredByProperty = context.DataModel.Modules[nameSpace].
        ///                    ResourceTypes[&quot;Book&quot;].NavigationProperties[&quot;AuthoredBy&quot;];
        ///                Predicate predicate = context.Predicates.
        ///                    Where(tuple =&gt; tuple.Id == authoredByProperty.Association.PredicateId).First();
        ///                predicate.Relationships.Load();
        ///                foreach (Relationship rel in predicate.Relationships)
        ///                    Console.WriteLine(&quot;[{0}] &lt;--{1}--&gt; [{2}]&quot;, rel.Subject.Id, predicate.Name,
        ///                        rel.Object.Id);
        ///            }
        ///        }
        ///
        ///        private static void CreateRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Type resourceTypeBook = extensionsAssembly.GetType(&quot;Zentity.Samples.Book&quot;);
        ///                PropertyInfo propertyAuthoredBy = resourceTypeBook.GetProperty(&quot;AuthoredBy&quot;);
        ///                Type resourceTypePerson = extensionsAssembly.GetType(&quot;Zentity.Samples.Person&quot;);
        ///
        ///                var aPerson = Activator.CreateInstance(resourceTypePerson);
        ///                var aBook = Activator.CreateInstance(resourceTypeBook);
        ///                var anotherBook = Activator.CreateInstance(resourceTypeBook);
        ///
        ///                // AuthoredBy is actually a an EntityCollection&lt;Person&gt;.
        ///                var instanceAuthoredBy = propertyAuthoredBy.GetValue(aBook, null);
        ///                Type entityCollectionOfPerson = instanceAuthoredBy.GetType();
        ///                // Get the &quot;Add&quot; method.
        ///                MethodInfo methodAdd = entityCollectionOfPerson.GetMethod(&quot;Add&quot;);
        ///                // Add author to book.
        ///                methodAdd.Invoke(instanceAuthoredBy, new object[] { aPerson });
        ///
        ///                instanceAuthoredBy = propertyAuthoredBy.GetValue(anotherBook, null);
        ///                methodAdd.Invoke(instanceAuthoredBy, new object[] { aPerson });
        ///
        ///                // Save the entities to repository. 
        ///                context.AddToResources((Resource)aPerson);
        ///                context.SaveChanges();
        ///            }
        ///        }
        ///
        ///        private static Assembly SynchronizeAndGenerateAssembly()
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///            string.Format(connectionStringFormat, &quot;Zentity.Core&quot;)))
        ///            {
        ///                ResourceType defaultBaseResourceType = context.DataModel.
        ///                Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
        ///
        ///                XDocument rdfDocument = XDocument.Load(rdfsPath);
        ///                XDocument xsdDocument = XDocument.Load(dataTypesXsdPath);
        ///                DataModelModule module = DataModelModule.CreateFromRdfs(defaultBaseResourceType,
        ///                rdfDocument, xsdDocument, nameSpace, false);
        ///
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Synchronize to alter the database schema.
        ///                // This method sometimes takes a few minutes to complete depending on the actions
        ///                // taken by other modules (such as change history logging) in response to schema
        ///                // changes. Everything happens in a single transaction. Make sure that the timeout 
        ///                // values are set correct for the command and transaction. Transaction timeout is 
        ///                // controlled from App.Config, Web.Config and machine.config configuration files.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate the module assembly to use.
        ///                byte[] rawAssembly = context.DataModel.GenerateExtensionsAssembly(
        ///                extensionsAssemblyName, true, new string[] { nameSpace },
        ///                new string[] { nameSpace }, null);
        ///
        ///                return Assembly.Load(rawAssembly);
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        public static DataModelModule CreateFromRdfs(ResourceType baseResourceType, XDocument rdfDocument,
            XDocument xsdDocument, string namespaceName, bool isStrictExecutionMode)
        {
            //Validate parameters.
            if (baseResourceType == null)
                throw new ArgumentNullException("baseResourceType", RdfsResources.ExceptionArgumentIsNull);
            if (rdfDocument == null)
                throw new ArgumentNullException("rdfDocument", RdfsResources.ExceptionArgumentIsNull);
            if (string.IsNullOrEmpty(namespaceName))
                throw new ArgumentNullException("namespaceName", RdfsResources.ExceptionArgumentIsNull);


            ExecutionMode executionMode = isStrictExecutionMode ?
                ExecutionMode.Strict : ExecutionMode.Loose;

            //Parse Rdf/Xml to Graph.
            RdfXmlParser rdfParser = new RdfXmlParser(rdfDocument);
            Graph rdfGraph = rdfParser.Parse(true);

            //Create collection of xsd datatypes.
            XsdDataTypeCollection dataTypeCollection = null;
            if (xsdDocument != null)
                dataTypeCollection = new XsdDataTypeCollection(xsdDocument);
            else
                dataTypeCollection = new XsdDataTypeCollection();

            //Create context for current execution.
            Context context = new Context
            {
                Graph = rdfGraph,
                XsdDataTypeCollection = dataTypeCollection,
                ExecutionMode = executionMode,
                BaseResourceType = baseResourceType
            };

            //Create list of interpreters.
            List<RdfsInterpreter> interpreters = new List<RdfsInterpreter>();
            interpreters.Add(new PredicateInterpreter(context));
            interpreters.Add(new TypeInterpreter(context));
            interpreters.Add(new SubClassOfInterpreter(context));
            interpreters.Add(new DomainRangeInterpreter(context));

            //Create data model module object.
            DataModelModule dataModelModule = new DataModelModule
            {
                NameSpace = namespaceName,
                Uri = rdfDocument.BaseUri
            };

            //Execute interpreters.
            foreach (RdfsInterpreter interpreter in interpreters)
                interpreter.Interpret(dataModelModule);

            return dataModelModule;
        }
        #endregion

        #region Helper Methods

        internal void Validate()
        {
            // Validate Id.
            if (this.Id == Guid.Empty)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.InvalidIdEmpty));

            // Validate Uri.
            if (!string.IsNullOrEmpty(this.Uri) && this.Uri.Length > 1024)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Uri, 1024));

            // Validate Description.
            if (!string.IsNullOrEmpty(this.Description) && this.Description.Length > 4000)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Description, 4000));

            // Validate Namespace.
            if (string.IsNullOrEmpty(this.NameSpace))
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.InvalidStringPropertyCannotBeNullOrEmpty, CoreResources.Namespace,
                    this.ToString()));

            if (this.NameSpace.Length > 150)
                throw new ModelItemValidationException(string.Format(CultureInfo.InvariantCulture,
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Namespace, 150));

            // Validating the namespace for a valid C# namespace makes the validation process slow.
            // So we are not including that check here. This check is included at the DataModel
            // level.

            // Validate resourceTypes.
            this.ResourceTypes.Validate();
        }

        internal void UpdateSsdl(XmlDocument ssdlDocument, TableMappingCollection tableMappings)
        {
            // Process custom resource types.
            foreach (ResourceType resourceType in this.ResourceTypes)
                resourceType.UpdateSsdl(ssdlDocument, tableMappings);

            // Process custom associations.
            foreach (Association association in this.Associations)
                association.UpdateSsdl(ssdlDocument, tableMappings);
        }

        internal void UpdateCsdls(XmlDocument coreCsdl, XmlDocument moduleCsdl)
        {
            // Process Resource Types.
            foreach (ResourceType resourceType in this.ResourceTypes)
                resourceType.UpdateCsdl(coreCsdl, moduleCsdl);

            // Process Associations.
            foreach (Association association in this.Associations)
                association.UpdateCsdl(coreCsdl, moduleCsdl);
        }

        internal void UpdateMsl(XmlDocument mslDocument, TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators, string storageSchemaName)
        {
            // Sort the resource types. We copy over mapping fragments from base type  to the
            // derived type and thus it is required that the base type is processed before
            // derived types.
            List<ResourceType> sortedTypes = DataModel.SortByHierarchy(
                this.ResourceTypes.ToList());

            // Process Resource Types.
            foreach (ResourceType resourceType in sortedTypes)
                resourceType.UpdateMsl(mslDocument, tableMappings, discriminators, storageSchemaName);

            // Process Associations.
            foreach (Association association in this.Associations)
                association.UpdateMsl(mslDocument, tableMappings, storageSchemaName);
        }

        #endregion
    }
}
