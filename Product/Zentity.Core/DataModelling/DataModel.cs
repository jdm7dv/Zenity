// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Collections.Specialized;
using System.Data.Mapping;
using System.Xml;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Globalization;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.IO;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;
using System.Transactions;
using System.Threading;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Zentity.Core;
using System.Collections;

namespace Zentity.Core
{
    /// <summary>
    /// Represents the Zentity Data Model for a store.
    /// </summary>
    /// <remarks>Zentity data model is a way to define the domain-specific resource types and 
    /// associations in the system. Since Zentity is built on top of Entity Framework, we tried 
    /// to stay as close as possible to the Entity Data Model (EDM). Most of the concepts here 
    /// are very similar in nature to the Entity Data Model. Table below shows the correspondence 
    /// between the two models.
    /// <br/>
    /// <table border="1">
    /// <tr><td>Zentity Data Model (ZDM) Construct</td><td>Entity Data Model (EDM) Construct</td></tr>
    /// <tr><td>Resource ManagerType</td><td>Entity</td></tr>
    /// <tr><td>Scalar Property</td><td>Scalar Property</td></tr>
    /// <tr><td>Navigation Property</td><td>Navigation Property</td></tr>
    /// <tr><td>Association</td><td>Association</td></tr>
    /// </table>
    /// <br/>
    /// Methods in this class work on ZDM constructs and generate the EDM constructs in addition 
    /// to other items. The generated EDM, has all its entities deriving directly or indirectly 
    /// from ‘Resource’ entity in the Zentity Core EDM. There is a single ZDM associated with each 
    /// store. Multiple data model modules can be present in a ZDM. Figure below shows the Zentity
    /// Data Model elements.
    /// <br/>
    /// <img src="ZDM.bmp"/>
    /// <br/>
    /// </remarks>
    /// <example>Example below shows how to enumerate and update model elements.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Data.EntityClient;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                PrintModelElements(context);
    ///
    ///                // Update the model.
    ///                DataModelModule module = new DataModelModule { NameSpace = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;) };
    ///                context.DataModel.Modules.Add(module);
    ///
    ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
    ///                ResourceType resourceTypeScholarlyWork = new ResourceType { Name = &quot;ScholarlyWork&quot;, BaseType = resourceTypeResource };
    ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
    ///
    ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
    ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
    ///
    ///                // This method takes a few minutes to complete depending on the actions taken by 
    ///                // other modules (such as change history logging) in response to schema changes.
    ///                // Provide a sufficient timeout.
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///
    ///                // Print the model again.
    ///                PrintModelElements(context);
    ///            }
    ///        }
    ///
    ///        private static void PrintModelElements(ZentityContext context)
    ///        {
    ///            Console.WriteLine(&quot;Data Model details for store [{0}\\{1}], Version [{2}]&quot;,
    ///                ((EntityConnection)context.Connection).StoreConnection.DataSource,
    ///                ((EntityConnection)context.Connection).StoreConnection.Database,
    ///                context.GetConfiguration(&quot;ZentityVersion&quot;));
    ///
    ///            foreach (DataModelModule module in context.DataModel.Modules)
    ///            {
    ///                Console.WriteLine(&quot;Module: [{0}]&quot;, module.NameSpace);
    ///                foreach (ResourceType resourceType in module.ResourceTypes)
    ///                {
    ///                    Console.WriteLine(&quot;\tResourceType: [{0}]&quot;, resourceType.Name);
    ///                    foreach (ScalarProperty scalarProperty in resourceType.ScalarProperties)
    ///                    {
    ///                        Console.WriteLine(&quot;\t\tScalarProperty: [{0}]&quot;, scalarProperty.Name);
    ///                    }
    ///                    foreach (NavigationProperty navProperty in resourceType.NavigationProperties)
    ///                    {
    ///                        Console.WriteLine(&quot;\t\tScalarProperty: [{0}]&quot;, navProperty.Name);
    ///                    }
    ///                }
    ///                foreach (Association assoc in module.Associations)
    ///                {
    ///                    Console.WriteLine(&quot;\tAssociation: [{0}]&quot;, assoc.Name);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed partial class DataModel
    {
        #region Fields

        int maxDiscriminator;
        DataModelModuleCollection modules;
        ZentityContext parent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of modules that form the data model.
        /// </summary>
        public DataModelModuleCollection Modules
        {
            get
            {
                if (modules == null)
                {
                    this.Refresh();
                }

                return modules;
            }
        }

        /// <summary>
        /// Gets the parent context.
        /// </summary>
        public ZentityContext Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets or sets the max discriminator.
        /// </summary>
        /// <value>The max discriminator.</value>
        internal int MaxDiscriminator
        {
            get { return maxDiscriminator; }
            set { maxDiscriminator = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModel"/> class.
        /// </summary>
        /// <param name="parent">The parent ZentityContext instance.</param>
        internal DataModel(ZentityContext parent)
        {
            this.parent = parent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the Entity Framework artifacts for a set of modules. Artifacts can be generated
        /// only for synchronized data model modules.
        /// </summary>
        /// <param name="modulesToInclude">Namespaces of the modules to include in the generated 
        /// artifacts. If this parameter is null, all synchronized modules of the data model are 
        /// included in the list.</param>
        /// <returns>Entity Framework artifacts for a set of synchronized modules.</returns>
        /// <example> The example below shows how to generate the Entity Framework artifacts for a
        /// custom data model module.
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System;
        ///using System.Linq;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType { Name = &quot;ScholarlyWork&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    Uri = Guid.NewGuid().ToString(&quot;N&quot;),
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///                // Synchronize to alter the database schema.
        ///                // It is important to synchronize before we can generate the Entity Framework
        ///                // artifacts. The discriminator, table and column mappings are assigned while 
        ///                // synchronizing and cannot be computed in advance.
        ///                // This method takes a few minutes to complete depending on the actions taken by 
        ///                // other modules (such as change history logging) in response to schema changes.
        ///                // Provide a sufficient timeout.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate the Entity Framework artifacts for all the modules in the model.
        ///                EFArtifactGenerationResults results = context.DataModel.GenerateEFArtifacts(null);
        ///
        ///                // Dump the CSDLs.
        ///                foreach (var kvp in results.Csdls)
        ///                    kvp.Value.Save(kvp.Key + &quot;.csdl&quot;);
        ///
        ///                // Dump the consolidated SSDL.
        ///                results.Ssdl.Save(&quot;Consolidated.ssdl&quot;);
        ///
        ///                // Dump the consolidated MSL.
        ///                results.Msl.Save(&quot;Consolidated.msl&quot;);
        ///
        ///                // Generate the Entity Framework artifacts that can be used with just Core and 
        ///                // this module.
        ///                results = context.DataModel.GenerateEFArtifacts(namespaceName);
        ///
        ///                // Dump the CSDLs.
        ///                XmlDocument extendedCoreCsdl = results.Csdls.
        ///                    Where(tuple =&gt; tuple.Key == &quot;Zentity.Core&quot;).First().Value;
        ///                XmlDocument moduleCsdl = results.Csdls.
        ///                    Where(tuple =&gt; tuple.Key == namespaceName).First().Value;
        ///                extendedCoreCsdl.Save(&quot;ExtendedCore.csdl&quot;);
        ///                moduleCsdl.Save(&quot;Module.csdl&quot;);
        ///
        ///                // Dump the consolidated SSDL.
        ///                results.Ssdl.Save(&quot;Consolidated1.ssdl&quot;);
        ///
        ///                // Dump the consolidated MSL.
        ///                results.Msl.Save(&quot;Consolidated1.msl&quot;);
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// <br/>
        /// The generated CSDL for Zentity.Core namespace is an extended CSDL file that contains 
        /// additional AssociationSet elements for all the associations present in the input modules. 
        /// The snippet below shows the AssociationSet element for the association between ScholarlyWork 
        /// and Contact in the example above.
        /// <code lang="xml">
        ///&lt;AssociationSet Name=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3_ScholarlyWorkAuthoredByContact&quot; Association=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3.Namespacee60d0ce425e242228d1c80901cfa74c3_ScholarlyWorkAuthoredByContact&quot;&gt;
        ///  &lt;End Role=&quot;ScholarlyWork&quot; EntitySet=&quot;Resources&quot; /&gt;
        ///  &lt;End Role=&quot;Contact&quot; EntitySet=&quot;Resources&quot; /&gt;
        ///&lt;/AssociationSet&gt;
        /// </code>
        /// <br/>
        /// The generated CSDL for the custom module is shown below.
        /// <code lang="xml">
        ///&lt;Schema Namespace=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3&quot; Alias=&quot;Self&quot; xmlns=&quot;http://schemas.microsoft.com/ado/2006/04/edm&quot;&gt;
        ///  &lt;EntityType Name=&quot;ScholarlyWork&quot; BaseType=&quot;Zentity.Core.Resource&quot;&gt;
        ///    &lt;Property Name=&quot;CopyRight&quot; Type=&quot;String&quot; Nullable=&quot;true&quot; Unicode=&quot;true&quot; MaxLength=&quot;4000&quot; FixedLength=&quot;false&quot; /&gt;
        ///    &lt;NavigationProperty Name=&quot;Authors&quot; Relationship=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3.Namespacee60d0ce425e242228d1c80901cfa74c3_ScholarlyWorkAuthoredByContact&quot; FromRole=&quot;ScholarlyWork&quot; ToRole=&quot;Contact&quot; /&gt;
        ///  &lt;/EntityType&gt;
        ///  &lt;EntityType Name=&quot;Contact&quot; BaseType=&quot;Zentity.Core.Resource&quot;&gt;
        ///    &lt;Property Name=&quot;Email&quot; Type=&quot;String&quot; Nullable=&quot;true&quot; Unicode=&quot;true&quot; MaxLength=&quot;1024&quot; FixedLength=&quot;false&quot; /&gt;
        ///    &lt;NavigationProperty Name=&quot;AuthoredWorks&quot; Relationship=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3.Namespacee60d0ce425e242228d1c80901cfa74c3_ScholarlyWorkAuthoredByContact&quot; FromRole=&quot;Contact&quot; ToRole=&quot;ScholarlyWork&quot; /&gt;
        ///  &lt;/EntityType&gt;
        ///  &lt;Association Name=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3_ScholarlyWorkAuthoredByContact&quot;&gt;
        ///    &lt;End Role=&quot;ScholarlyWork&quot; Type=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3.ScholarlyWork&quot; Multiplicity=&quot;*&quot; /&gt;
        ///    &lt;End Role=&quot;Contact&quot; Type=&quot;Namespacee60d0ce425e242228d1c80901cfa74c3.Contact&quot; Multiplicity=&quot;*&quot; /&gt;
        ///  &lt;/Association&gt;
        ///&lt;/Schema&gt;
        /// </code>
        /// </example>
        /// <remarks>
        /// <para>
        /// This method ignores the Core module if specified in the parameter list. The input 
        /// module list is enhanced to include all the dependencies before processing. For example, 
        /// if a custom module defines a resource type that inherits from 
        /// Zentity.ScholarlyWorks.Book and Zentity.ScholarlyWorks is not present in the input 
        /// list, this method will include Zentity.ScholarlyWorks the list before proceeding.
        /// </para>
        /// <para>
        /// The result includes a list of CSDL documents per processed module (input and 
        /// dependencies). The CSDL corresponding to Zentity.Core is enhanced to also include 
        /// AssociationSet elements for all associations present in the processed modules. Next, 
        /// the result includes a consolidated SSDL document that contains the information of all 
        /// store entities and associations present in the processed modules. Finally, the result 
        /// contains a consolidated MSL document that contains the mapping information of all 
        /// processed modules.
        /// </para>
        /// <para>
        /// Note that the artifacts generated for a set of input modules are not equivalent to the 
        /// artifacts generated for each of them separately. For example, the artifacts generated 
        /// by the two cases below are not equivalent:
        /// <br/>
        /// Case 1: Invoking this method with ("Zentity.ScholarlyWorks", "Zentity.Samples.Museum")
        /// <br/>
        /// Case 2: Combined artifacts generated by invoking the method with 
        /// ("Zentity.ScholarlyWorks") and ("Zentity.Samples.Museum") separately
        /// <br/>
        /// You might get 'duplicate definition' errors while trying to use all the artifacts 
        /// generated in Case 2 together.
        /// </para>
        /// <para>
        /// <b>Multiple active module-sets per store not supported</b>
        /// <br/>
        /// Even though it is possible to generate artifacts for various combinations of modules 
        /// present in the model, using multiple module-sets on the same store from your 
        /// applications might raise errors. For example, consider a custom module M1 with 
        /// resource type RT1 that has discriminator value 100. App1 creates resources using 
        /// artifacts generated for M1 module and App2 uses artifacts embedded in Zentity.Core.dll 
        /// to enumerate resources on the same store. Now rows corresponding to RT1 in Core.Resource 
        /// table will have discriminator value as 100, but the artifacts used by App2 do not have
        /// any entity type defined that maps to this discriminator. So, resources retrieved by 
        /// App2 will not have any RT1 instances which may result into application errors.
        /// </para>
        /// </remarks>
        public EFArtifactGenerationResults GenerateEFArtifacts(params string[] modulesToInclude)
        {
            // Validate the model.
            this.Validate();

            // Verify all the modules to include are present in the model.
            List<string> inputNamespaces = (modulesToInclude == null ||
                modulesToInclude != null && modulesToInclude.Length == 0) ?
                this.Modules.Select(tuple => tuple.NameSpace).ToList() :
                modulesToInclude.ToList();
            
            var nameSpacesInGraph = this.Modules.Select(tuple => tuple.NameSpace);
            var absentModules = inputNamespaces.Except(nameSpacesInGraph);
            if (absentModules.Count() > 0)
            {
                var absentModule = absentModules.First();
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ExceptionAbsentModuleNamespace, absentModule));
            }
            
            // Validate that the modules in input list are synchronized.
            DataModel synchronizedModel;
            TableMappingCollection tableMappings;
            Dictionary<Guid, string> associationViewMappings;
            Dictionary<Guid, int> discriminators;
            GetSynchronizedModelAndMappings(out synchronizedModel, out tableMappings,
                out associationViewMappings, out discriminators);

            // We can generate artifacts only for the synchronized modules.
            DetectUnsynchronizedModules(synchronizedModel, this, inputNamespaces);

            EFArtifactGenerationResults results = new EFArtifactGenerationResults();
            Assembly zentityAssembly = typeof(Resource).Assembly;

            // Load initial values.
            results.Ssdl = PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                DataModellingResources.SSDLManifestResourceName);

            results.Csdls.Add(new KeyValuePair<string, XmlDocument>(DataModellingResources.ZentityCore,
                PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                DataModellingResources.CSDLManifestResourceName)));

            results.Msl = PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                DataModellingResources.MSLManifestResourceName);

            // Translate from namespaces to modules.
            List<DataModelModule> inputModules = new List<DataModelModule>();

            // Add dependencies.
            Queue<DataModelModule> moduleQueue = new Queue<DataModelModule>(this.Modules.Where(
                tuple => inputNamespaces.Contains(tuple.NameSpace)).Distinct());

            while (moduleQueue.Count > 0)
            {
                DataModelModule dependentModule = moduleQueue.Dequeue();
                if (!inputModules.Contains(dependentModule))
                    inputModules.Add(dependentModule);

                foreach (ResourceType resourceType in dependentModule.ResourceTypes.
                    Where(tuple => tuple.BaseType != null &&
                        !inputModules.Contains(tuple.BaseType.Parent)))
                {
                    moduleQueue.Enqueue(resourceType.BaseType.Parent);
                }
            }

            // Sort the modules. Updating MSL requires copying mapping fragments from base resource
            // types. So, we might get incorrect number of fragments if a derived resource type is
            // processed before base type.
            List<DataModelModule> sortedModules = SortModules(inputModules);

            for (int i = 0; i < sortedModules.Count(); i++)
            {
                DataModelModule item = sortedModules[i];

                // Ignore Core module.
                if (item.NameSpace == DataModellingResources.ZentityCore)
                    continue;

                // Update the SSDL.
                item.UpdateSsdl(results.Ssdl, tableMappings);

                // Create a new CSDL and add it to results.
                XmlDocument csdlDocument = new XmlDocument();
                XmlElement schemaElement = csdlDocument.CreateElement(DataModellingResources.Schema,
                    DataModellingResources.CSDLSchemaNameSpace);
                csdlDocument.AppendChild(schemaElement);
                Utilities.AddAttribute(schemaElement, DataModellingResources.Namespace, item.NameSpace);
                Utilities.AddAttribute(schemaElement, DataModellingResources.Alias, DataModellingResources.Self);
                results.Csdls.Add(new KeyValuePair<string, XmlDocument>(item.NameSpace,
                    csdlDocument));

                // Update the Core CSDL and the module specific CSDL.
                item.UpdateCsdls(results.Csdls.Where(tuple =>
                    tuple.Key == DataModellingResources.ZentityCore).First().Value, csdlDocument);

                // Update MSL.
                // NOTE: We are passing the tableMappings and the discriminators retrieved
                // from the synchronized model. We cannot rely on the in-memory graph here
                // since it might still not be refreshed. For example, if a user creates an
                // in memory model and generates sql scripts for it and then applies those
                // scripts to the database, the backend column mappings are updated but the
                // in memory model does not have any mapping information still. Thus, we pass
                // the mappings information that we got from the backend to MSL update procedures.
                string storageSchemaName = results.Ssdl[DataModellingResources.Schema].
                    Attributes[DataModellingResources.Namespace].Value;
                item.UpdateMsl(results.Msl, tableMappings, discriminators, storageSchemaName);
            }

            return results;
        }

        /// <summary>
        /// Returns the flattened Entity Framework artifacts for a set of modules. Artifacts can be 
        /// generated only for synchronized data model modules.
        /// </summary>
        /// <param name="modulesToInclude">Namespaces of the modules to include in the generated 
        /// artifacts. If this parameter is null, all synchronized modules of the data model are 
        /// included in the list.</param>
        /// <returns>Entity Framework artifacts for a set of synchronized modules.</returns>
        public EFArtifactGenerationResults GenerateFlattenedEFArtifacts(params string[] modulesToInclude)
        {
            // Validate the model.
            this.Validate();

            // Verify all the modules to include are present in the model.
            List<string> inputNamespaces = (modulesToInclude == null ||
                modulesToInclude != null && modulesToInclude.Length == 0) ?
                this.Modules.Select(tuple => tuple.NameSpace).ToList() :
                modulesToInclude.ToList();

            var nameSpacesInGraph = this.Modules.Select(tuple => tuple.NameSpace);
            var absentModules = inputNamespaces.Except(nameSpacesInGraph);
            if (absentModules.Count() > 0)
            {
                var absentModule = absentModules.First();
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ExceptionAbsentModuleNamespace, absentModule));
            }

            // Validate that the modules in input list are synchronized.
            DataModel synchronizedModel;
            TableMappingCollection tableMappings;
            Dictionary<Guid, string> associationViewMappings;
            Dictionary<Guid, int> discriminators;
            GetSynchronizedModelAndMappings(out synchronizedModel, out tableMappings,
                out associationViewMappings, out discriminators);

            // We can generate artifacts only for the synchronized modules.
            DetectUnsynchronizedModules(synchronizedModel, this, inputNamespaces);

            EFArtifactGenerationResults results = new EFArtifactGenerationResults();
            Assembly zentityAssembly = typeof(Resource).Assembly;

            // Load initial values.
            results.Ssdl = PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                DataModellingResources.SSDLManifestResourceName);

            results.Csdls.Add(new KeyValuePair<string, XmlDocument>(DataModellingResources.ZentityCore,
                PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                DataModellingResources.CSDLManifestResourceName)));

            results.Msl = PrepareXmlDocumentFromManifestResourceStream(zentityAssembly,
                DataModellingResources.MSLManifestResourceName);

            // Translate from namespaces to modules.
            List<DataModelModule> inputModules = new List<DataModelModule>();

            // Add dependencies.
            Queue<DataModelModule> moduleQueue = new Queue<DataModelModule>(this.Modules.Where(
                tuple => inputNamespaces.Contains(tuple.NameSpace)).Distinct());

            while (moduleQueue.Count > 0)
            {
                DataModelModule dependentModule = moduleQueue.Dequeue();
                if (!inputModules.Contains(dependentModule))
                    inputModules.Add(dependentModule);

                foreach (ResourceType resourceType in dependentModule.ResourceTypes.
                    Where(tuple => tuple.BaseType != null &&
                        !inputModules.Contains(tuple.BaseType.Parent)))
                {
                    moduleQueue.Enqueue(resourceType.BaseType.Parent);
                }
            }

            // Sort the modules. Updating MSL requires copying mapping fragments from base resource
            // types. So, we might get incorrect number of fragments if a derived resource type is
            // processed before base type.
            List<DataModelModule> sortedModules = SortModules(inputModules);

            for (int i = 0; i < sortedModules.Count(); i++)
            {
                DataModelModule item = sortedModules[i];

                // Ignore Core module.
                if (item.NameSpace == DataModellingResources.ZentityCore)
                    continue;

                // Update the SSDL.
                item.UpdateFlattenedSsdl(results.Ssdl, tableMappings);

                // Create a new CSDL and add it to results.
                XmlDocument csdlDocument = new XmlDocument();
                XmlElement schemaElement = csdlDocument.CreateElement(DataModellingResources.Schema, DataModellingResources.CSDLSchemaNameSpace);
                csdlDocument.AppendChild(schemaElement);
                Utilities.AddAttribute(schemaElement, DataModellingResources.Namespace, item.NameSpace);
                Utilities.AddAttribute(schemaElement, DataModellingResources.Alias, DataModellingResources.Self);
                results.Csdls.Add(new KeyValuePair<string, XmlDocument>(item.NameSpace, csdlDocument));

                // Update the Core CSDL and the module specific CSDL.
                item.UpdateFlattenedCsdls(results.Csdls.Where(tuple => tuple.Key == DataModellingResources.ZentityCore).First().Value, csdlDocument);

                // Update MSL.
                // NOTE: We are passing the tableMappings and the discriminators retrieved
                // from the synchronized model. We cannot rely on the in-memory graph here
                // since it might still not be refreshed. For example, if a user creates an
                // in memory model and generates sql scripts for it and then applies those
                // scripts to the database, the backend column mappings are updated but the
                // in memory model does not have any mapping information still. Thus, we pass
                // the mappings information that we got from the backend to MSL update procedures.
                string storageSchemaName = results.Ssdl[DataModellingResources.Schema].Attributes[DataModellingResources.Namespace].Value;
                item.UpdateFlattenedMsl(results.Msl, tableMappings, discriminators, storageSchemaName);
            }

            return results;
        }

        /// <summary>
        /// Generates a .NET assembly containing the types defined in the input modules. 
        /// </summary>
        /// <param name="outputAssemblyName">The security identity of the output assembly.</param>
        /// <param name="embedMetadataFilesAsResources">Whether to embed the Entity Framework 
        /// artifacts into the assembly.</param>
        /// <param name="modulesToEmbed">Namespaces of modules for which the Entity Framework 
        /// artifacts are to be embedded in the generated assembly. The method embeds artifacts
        /// for all modules if this parameter is null and embedMetadataFilesAsResources is true.
        /// </param>
        /// <param name="modulesToCompile">Namespaces of modules to compile. The method generates
        /// source code for all non-MsShipped modules if this parameter is null.</param>
        /// <param name="referencedAssemblies">The assemblies to be referenced for compilation.
        /// It is not required to reference the 'Zentity.Core' dll explicitly. The semantics of 
        /// this parameter are similar to ReferencedAssemblies property of 
        /// System.CodeDom.Compiler.CompilerParameters class.</param>
        /// <returns>The generated .NET assembly for custom resource types.</returns>
        /// <example> The example below shows how to generate and use an Extensions assembly. 
        /// Though the example uses reflection, we recommend compiling the client programs with
        /// references to the generated assembly to avoid reflection based code. The Entity 
        /// Framework artifacts generated to work with the assembly are embedded in the assembly 
        /// itself. Also, the custom resource type inherits from  Zentity.ScholarlyWorks.Contact 
        /// and Zentity.ScholarlyWorks is not specified in the modulesToCompile list. It is thus
        /// required to provide the path to Zentity.ScholarlyWorks assembly to the compiler.
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System.Reflection;
        ///using System;
        ///using System.Collections;
        ///using System.Linq;
        ///using Zentity.ScholarlyWorks;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        static string extensionsNamespace = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///        const string connectionStringFormat = @&quot;provider=System.Data.SqlClient;
        ///                metadata=res://{0}; provider connection string='Data Source=.;
        ///                Initial Catalog=Zentity;Integrated Security=True;MultipleActiveResultSets=True'&quot;;
        ///        const string extensionsAssemblyName = &quot;Extensions&quot;;
        ///
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, &quot;Zentity.Core&quot;)))
        ///            {
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = extensionsNamespace };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create a custom resource type.
        ///                ResourceType resourceTypeContact = context.DataModel.Modules[&quot;Zentity.ScholarlyWorks&quot;].ResourceTypes[&quot;Contact&quot;];
        ///                ResourceType resourceTypeEmployee = new ResourceType { Name = &quot;Employee&quot;, BaseType = resourceTypeContact };
        ///                module.ResourceTypes.Add(resourceTypeEmployee);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty salary = new ScalarProperty { Name = &quot;Salary&quot;, DataType = DataTypes.Int32 };
        ///                resourceTypeEmployee.ScalarProperties.Add(salary);
        ///
        ///                // Synchronize to alter the database schema.
        ///                // It is important to synchronize before we can generate the Entity Framework
        ///                // artifacts. The discriminator, table and column mappings are assigned while 
        ///                // synchronizing and cannot be computed in advance.
        ///                // This method takes a few minutes to complete depending on the actions taken by 
        ///                // other modules (such as change history logging) in response to schema changes.
        ///                // Provide a sufficient timeout.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate Extensions Assembly.
        ///                // Take note that we have embedded the generated Entity Framework artifacts in the assembly itself.
        ///                byte[] rawAssembly = context.DataModel.GenerateExtensionsAssembly(
        ///                    extensionsAssemblyName, true, new string[] { extensionsNamespace },
        ///                    new string[] { extensionsNamespace },
        ///                    new string[] { typeof(ScholarlyWork).Assembly.Location });
        ///
        ///                Assembly extensions = Assembly.Load(rawAssembly);
        ///
        ///                // Create some repository items using the generated assembly.
        ///                CreateRepositoryItems(extensions);
        ///
        ///                // Retrieve the created repository items.
        ///                FetchRepositoryItems(extensions);
        ///            }
        ///        }
        ///
        ///        private static void FetchRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Console.WriteLine(&quot;Getting Employees...&quot;);
        ///                Type resourceTypeEmployee = extensionsAssembly.GetType(extensionsNamespace + &quot;.Employee&quot;);
        ///                PropertyInfo pi = resourceTypeEmployee.GetProperty(&quot;Salary&quot;);
        ///                MethodInfo ofTypeMethod = context.Resources.GetType().GetMethod(&quot;OfType&quot;).
        ///                    MakeGenericMethod(resourceTypeEmployee);
        ///                var customTypeInstances = ofTypeMethod.Invoke(context.Resources, null);
        ///                foreach (Resource employee in (IEnumerable)customTypeInstances)
        ///                {
        ///                    Console.WriteLine(&quot;Id:[{0}], Salary:[{1}]&quot;, employee.Id,
        ///                        pi.GetValue(employee, null));
        ///                }
        ///            }
        ///        }
        ///
        ///        private static void CreateRepositoryItems(Assembly extensionsAssembly)
        ///        {
        ///            using (ZentityContext context = new ZentityContext(
        ///                string.Format(connectionStringFormat, extensionsAssemblyName)))
        ///            {
        ///                Type resourceTypeEmployee = extensionsAssembly.GetType(extensionsNamespace + &quot;.Employee&quot;);
        ///                var anEmployee = Activator.CreateInstance(resourceTypeEmployee);
        ///                PropertyInfo pi = resourceTypeEmployee.GetProperty(&quot;Salary&quot;);
        ///                pi.SetValue(anEmployee, 100000, null);
        ///
        ///                // Save the items to repository. 
        ///                context.AddToResources((Resource)anEmployee);
        ///                context.SaveChanges();
        ///
        ///                Console.WriteLine(&quot;Created Employee with Id:[{0}], Salary:[{1}]&quot;, ((Resource)anEmployee).Id, 100000);
        ///            }
        ///        }
        ///
        ///    }
        ///}
        /// </code>
        /// </example>
        /// <remarks>
        /// The method creates some intermediate files on disk and thus the application should have
        /// write access on current directory. A temporary assembly file is created on disk using 
        /// the outputAssemblyName parameter. Try reducing the length of this parameter if you are 
        /// seeing 'path too long' exceptions.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is disposed in finally block."), 
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public byte[] GenerateExtensionsAssembly(string outputAssemblyName, bool embedMetadataFilesAsResources, string[] modulesToEmbed, string[] modulesToCompile, string[] referencedAssemblies)
        {
            CSharpCodeProvider codeProvider = null;
            CompilerParameters compilerParameters = null;
            CompilerResults results = null;
            string notNullOutputAssemblyName = string.IsNullOrEmpty(outputAssemblyName) ?
                Utilities.GetGuidString() : outputAssemblyName;
            string temporaryDirectoryName = Utilities.GetGuidString();

            try
            {
                // Create a provider.
                codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() 
                {                 
                    {                     
                        DataModellingResources.CompilerVersion,                     
                        DataModellingResources.v4_0                    
                    } 
                });

                // Create compiler parameters.
                compilerParameters = new CompilerParameters();
                Assembly coreAssembly = typeof(Resource).Assembly;
                // Set the list of assemblies to reference.
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystem4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystemDataEntity4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystemRuntimeSerialization4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystemXml4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(coreAssembly.Location);

                // Add reference to other assemblies.
                if (referencedAssemblies != null)
                    foreach (String assemblyLocation in referencedAssemblies)
                    {
                        // Case sensitive comparison.
                        if (!Assembly.LoadFile(assemblyLocation).FullName.Equals(coreAssembly.FullName))
                            compilerParameters.ReferencedAssemblies.Add(assemblyLocation);
                    }
                // Set other options.
                compilerParameters.GenerateExecutable = false;
                compilerParameters.TreatWarningsAsErrors = false;
                compilerParameters.GenerateInMemory = false;
                compilerParameters.OutputAssembly = Path.Combine(temporaryDirectoryName,
                    notNullOutputAssemblyName + DataModellingResources.DotDll);

                // Dump the resources on disk and include them in the embedded resources list.
                // To avoid System.IO.PathTooLongException for the resource names, use a hash 
                // instead of actual names.
                Directory.CreateDirectory(temporaryDirectoryName);
                if (embedMetadataFilesAsResources)
                {
                    EFArtifactGenerationResults efArtifacts = new EFArtifactGenerationResults();
                    efArtifacts = GenerateEFArtifacts(modulesToEmbed);

                    // Embed all the CSDLs.
                    for (int i = 0; i < efArtifacts.Csdls.Count; i++)
                    {
                        XmlDocument xCSDL = efArtifacts.Csdls[i].Value;
                        string csdlFileName = Path.Combine(temporaryDirectoryName,
                            ComputeHash(efArtifacts.Csdls[i].Key) +
                            DataModellingResources.DotCsdl);
                        xCSDL.Save(csdlFileName);
                        compilerParameters.EmbeddedResources.Add(csdlFileName);
                    }

                    // Embed SSDL.
                    string ssdlFileName = Path.Combine(temporaryDirectoryName,
                        ComputeHash(notNullOutputAssemblyName) +
                        DataModellingResources.DotSsdl);
                    efArtifacts.Ssdl.Save(ssdlFileName);
                    compilerParameters.EmbeddedResources.Add(ssdlFileName);

                    // Embed MSL.
                    string mslFileName = Path.Combine(temporaryDirectoryName,
                        ComputeHash(notNullOutputAssemblyName) +
                        DataModellingResources.DotMsl);
                    efArtifacts.Msl.Save(mslFileName);
                    compilerParameters.EmbeddedResources.Add(mslFileName);
                }

                StringCollection sourceCode = new StringCollection();
                if (modulesToCompile != null)
                {
                    sourceCode = GenerateSourceCode(modulesToCompile);
                }
                else
                {
                    sourceCode.Add(DataModellingResources.DummyClassCode);
                }

                // There could be no source code to process sometimes. This happens when there is no 
                // custom type in the resource types collection.
                if (sourceCode.Count == 0)
                    return null;

                string[] sourceCodeArray = new string[sourceCode.Count];
                sourceCode.CopyTo(sourceCodeArray, 0);
                results = codeProvider.CompileAssemblyFromSource(compilerParameters, sourceCodeArray);
                if (results.Errors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in results.Errors)
                        sb.Append(error.ErrorText);

                    throw new ZentityException(sb.ToString());
                }

                // Create byte array from generated assembly.
                byte[] generatedAssembly = null;
                using (FileStream fs = new FileStream(results.PathToAssembly, FileMode.Open,
                    FileAccess.Read))
                {
                    generatedAssembly = new byte[fs.Length];
                    fs.Read(generatedAssembly, 0, (int)fs.Length);
                }

                return generatedAssembly;
            }
            finally
            {
                // Cleanup.
                if (Directory.Exists(temporaryDirectoryName))
                    Directory.Delete(temporaryDirectoryName, true);

                if (codeProvider != null)
                    codeProvider.Dispose();
            }
        }

        /// <summary>
        /// Generates a flattened .NET assembly containing the types defined in the input modules. 
        /// </summary>
        /// <param name="outputAssemblyName">The security identity of the output assembly.</param>
        /// <param name="embedMetadataFilesAsResources">Whether to embed the Entity Framework 
        /// artifacts into the assembly.</param>
        /// <param name="modulesToEmbed">Namespaces of modules for which the Entity Framework 
        /// artifacts are to be embedded in the generated assembly. The method embeds artifacts
        /// for all modules if this parameter is null and embedMetadataFilesAsResources is true.
        /// </param>
        /// <param name="modulesToCompile">Namespaces of modules to compile. The method generates
        /// source code for all non-MsShipped modules if this parameter is null.</param>
        /// <param name="referencedAssemblies">The assemblies to be referenced for compilation.
        /// It is not required to reference the 'Zentity.Core' dll explicitly. The semantics of 
        /// this parameter are similar to ReferencedAssemblies property of 
        /// System.CodeDom.Compiler.CompilerParameters class.</param>
        /// <returns>The generated .NET assembly for custom resource types.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="The object is disposed in finally block."),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public byte[] GenerateFlattenedExtensionsAssembly(string outputAssemblyName, bool embedMetadataFilesAsResources, string[] modulesToEmbed, string[] modulesToCompile, string[] referencedAssemblies)
        {
            CSharpCodeProvider codeProvider = null;
            CompilerParameters compilerParameters = null;
            CompilerResults results = null;
            string notNullOutputAssemblyName = string.IsNullOrEmpty(outputAssemblyName) ?
                Utilities.GetGuidString() : outputAssemblyName;
            string temporaryDirectoryName = Utilities.GetGuidString();

            try
            {
                // Create a provider.
                codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() 
                {                 
                    {                     
                        DataModellingResources.CompilerVersion,                     
                        DataModellingResources.v4_0                    
                    } 
                });

                // Create compiler parameters.
                compilerParameters = new CompilerParameters();
                Assembly coreAssembly = typeof(Resource).Assembly;
                // Set the list of assemblies to reference.
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystem4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystemDataEntity4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystemRuntimeSerialization4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(Assembly.Load(DataModellingResources.DllSystemXml4_0).Location);
                compilerParameters.ReferencedAssemblies.Add(coreAssembly.Location);

                // Add reference to other assemblies.
                if (referencedAssemblies != null)
                    foreach (String assemblyLocation in referencedAssemblies)
                    {
                        // Case sensitive comparison.
                        if (!Assembly.LoadFile(assemblyLocation).FullName.Equals(coreAssembly.FullName))
                            compilerParameters.ReferencedAssemblies.Add(assemblyLocation);
                    }
                // Set other options.
                compilerParameters.GenerateExecutable = false;
                compilerParameters.TreatWarningsAsErrors = false;
                compilerParameters.GenerateInMemory = false;
                compilerParameters.OutputAssembly = Path.Combine(temporaryDirectoryName,
                    notNullOutputAssemblyName + DataModellingResources.DotDll);
                StringCollection sourceCode = new StringCollection();

                // Dump the resources on disk and include them in the embedded resources list.
                // To avoid System.IO.PathTooLongException for the resource names, use a hash 
                // instead of actual names.
                Directory.CreateDirectory(temporaryDirectoryName);
                if (embedMetadataFilesAsResources)
                {
                    EFArtifactGenerationResults efArtifacts = new EFArtifactGenerationResults();
                    efArtifacts = GenerateFlattenedEFArtifacts(modulesToEmbed);

                    // Embed all the CSDLs.
                    for (int i = 0; i < efArtifacts.Csdls.Count; i++)
                    {
                        XmlDocument xCSDL = efArtifacts.Csdls[i].Value;
                        string csdlFileName = Path.Combine(temporaryDirectoryName,
                            ComputeHash(efArtifacts.Csdls[i].Key) +
                            DataModellingResources.DotCsdl);
                        xCSDL.Save(csdlFileName);
                        compilerParameters.EmbeddedResources.Add(csdlFileName);
                    }

                    // Embed SSDL.
                    string ssdlFileName = Path.Combine(temporaryDirectoryName,
                        ComputeHash(notNullOutputAssemblyName) +
                        DataModellingResources.DotSsdl);
                    efArtifacts.Ssdl.Save(ssdlFileName);
                    compilerParameters.EmbeddedResources.Add(ssdlFileName);

                    // Embed MSL.
                    string mslFileName = Path.Combine(temporaryDirectoryName,
                        ComputeHash(notNullOutputAssemblyName) +
                        DataModellingResources.DotMsl);
                    efArtifacts.Msl.Save(mslFileName);
                    compilerParameters.EmbeddedResources.Add(mslFileName);

                    string precompiledViewsSource = GeneratePrecompiledViews(efArtifacts);
                    if (!string.IsNullOrWhiteSpace(precompiledViewsSource))
                    {
                        sourceCode.Add(precompiledViewsSource);
                    }
                }


                if (modulesToCompile != null)
                {
                    sourceCode = GenerateFlattenedSourceCode(modulesToCompile);
                }
                else
                {
                    if (sourceCode.Count == 0)
                    {
                        sourceCode.Add(DataModellingResources.DummyClassCode);
                    }
                }

                // There could be no source code to process sometimes. This happens when there is no 
                // custom type in the resource types collection.
                if (sourceCode.Count == 0)
                    return null;

                string[] sourceCodeArray = new string[sourceCode.Count];
                sourceCode.CopyTo(sourceCodeArray, 0);
                results = codeProvider.CompileAssemblyFromSource(compilerParameters, sourceCodeArray);
                if (results.Errors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (CompilerError error in results.Errors)
                        sb.Append(error.ErrorText);

                    throw new ZentityException(sb.ToString());
                }

                // Create byte array from generated assembly.
                byte[] generatedAssembly = null;
                using (FileStream fs = new FileStream(results.PathToAssembly, FileMode.Open,
                    FileAccess.Read))
                {
                    generatedAssembly = new byte[fs.Length];
                    fs.Read(generatedAssembly, 0, (int) fs.Length);
                }

                return generatedAssembly;
            }
            finally
            {
                // Cleanup.
                if (Directory.Exists(temporaryDirectoryName))
                    Directory.Delete(temporaryDirectoryName, true);

                if (codeProvider != null)
                    codeProvider.Dispose();
            }
        }

        /// <summary>
        /// Generates the C# source code for the specified modules.
        /// </summary>
        /// <param name="modulesToInclude">A list of module namespaces to generate source code 
        /// for. If this parameter is null, all modules of the data model are included in 
        /// the list.</param>
        /// <returns>Source code for each module defined in the input parameter.</returns>
        /// <example> The example below shows how to generate the source code for a custom data
        /// model module.
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System.IO;
        ///using System.Text;
        ///using System.Collections.Specialized;
        ///using System;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType { Name = &quot;ScholarlyWork&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    Uri = Guid.NewGuid().ToString(&quot;N&quot;),
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///                // Synchronize to alter the database schema.
        ///                // It is important to synchronize before we can generate the Entity Framework
        ///                // artifacts. The discriminator, table and column mappings are assigned while 
        ///                // synchronizing and cannot be computed in advance.
        ///                // This method takes a few minutes to complete depending on the actions taken by 
        ///                // other modules (such as change history logging) in response to schema changes.
        ///                // Provide a sufficient timeout.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///
        ///                // Generate source code for just a single module.
        ///                using (StreamWriter writer = new StreamWriter(namespaceName + &quot;.cs&quot;))
        ///                {
        ///                    foreach (string str in context.DataModel.GenerateSourceCode(namespaceName))
        ///                        writer.WriteLine(str);
        ///                    writer.Close();
        ///                }
        ///
        ///                // Generate source code for all modules (except MsShipped).
        ///                using (StreamWriter writer = new StreamWriter(&quot;AllModules.cs&quot;))
        ///                {
        ///                    foreach (string str in context.DataModel.GenerateSourceCode(null))
        ///                        writer.WriteLine(str);
        ///                    writer.Close();
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// <br/>
        /// The snippet below presents an idea of the generated output.
        /// <code>
        ///...
        ///namespace Namespace7577e96f4c0c4540a679fe75340e76f1
        ///{
        ///    ...
        ///    public partial class ScholarlyWork : Zentity.Core.Resource
        ///    {
        ///        public static ScholarlyWork CreateScholarlyWork(global::System.Guid id)
        ///        {
        ///            ...
        ///        }
        ///        public string CopyRight
        ///        {
        ///            get
        ///            {
        ///                ...
        ///            }
        ///            set
        ///            {
        ///                ...
        ///            }
        ///        }
        ///        public global::System.Data.Objects.DataClasses.EntityCollection&lt;Contact&gt; Authors
        ///        {
        ///            get
        ///            {
        ///                ...
        ///            }
        ///            set
        ///            {
        ///                ...
        ///            }
        ///        }
        ///        ...
        ///    }
        ///
        ///    ...
        ///    public partial class Contact : Zentity.Core.Resource
        ///    {
        ///        public static Contact CreateContact(global::System.Guid id)
        ///        {
        ///            ...
        ///        }
        ///
        ///        public string Email
        ///        {
        ///            ...
        ///        }
        ///
        ///        public global::System.Data.Objects.DataClasses.EntityCollection&lt;ScholarlyWork&gt; AuthoredWorks
        ///        {
        ///            ...
        ///        }
        ///        ...
        ///    }
        ///}
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public StringCollection GenerateSourceCode(params string[] modulesToInclude)
        {
            // No need to Validate here. GenerateEFArtifacts does it for us.
            List<string> inputNamespaces = (modulesToInclude == null ||
                modulesToInclude != null && modulesToInclude.Length == 0) ?
                this.Modules.Select(tuple => tuple.NameSpace).ToList() :
                modulesToInclude.ToList();

            // Validate the input list.
            var nameSpaces = this.Modules.Select(tuple => tuple.NameSpace);
            var absentModules = inputNamespaces.Except(nameSpaces);
            if (absentModules.Count() > 0)
            {
                var absentModule = absentModules.First();
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ExceptionAbsentModuleNamespace, absentModule));
            }

            // Generate artifacts for all the modules in the model. This saves us from missing CSDL
            // references while generating source code. We filter out unwanted namespaces later.
            Dictionary<string, string> sourceCodeTracker = new Dictionary<string, string>();

            // Prepare a reference list of CSDLs.
            List<string> refCSDLs = new List<string>();
            // Add Core CSDL.
            refCSDLs.Add(Utilities.GetGuidString() + DataModellingResources.DotCsdl);

            // Create generator.
            EntityCodeGenerator generator =
                new EntityCodeGenerator(LanguageOption.GenerateCSharpCode);

            List<DataModelModule> sortedModules = SortModules(this.Modules.ToList());
            // Remove the Core module from sorted list. We cannot assume that the Core module is
            // the first module in sorted list. For example, if a module has zero resource types
            // it is independent of Core and thus can be sorted before Core.
            var coreModule = sortedModules.
                Where(tuple => tuple.NameSpace == DataModellingResources.ZentityCore).FirstOrDefault();
            if (coreModule != null)
                sortedModules.Remove(coreModule);

            try
            {
                for (int i = 0; i < sortedModules.Count(); i++)
                {
                    DataModelModule module = sortedModules[i];

                    // Generate CSDL for this module. We cannot move this artifact generation 
                    // outside this loop since, the consolidated CSDL changes with the inclusion 
                    // of every module. For example, if the new module has some associations, 
                    // the Core CSDL will have to host new AssociationSet elements. We generate 
                    // and update the new Core CSDL in each iteration.
                    EFArtifactGenerationResults results = this.GenerateEFArtifacts(
                        module.NameSpace);

                    // Update Core CSDL.
                    results.Csdls.Where(tuple => tuple.Key == DataModellingResources.ZentityCore).
                        First().Value.Save(refCSDLs[0]);

                    // Dump the CSDL.
                    string csdlFileName = Utilities.GetGuidString() + DataModellingResources.DotCsdl;
                    results.Csdls.Where(tuple => tuple.Key == module.NameSpace).
                        First().Value.Save(csdlFileName);

                    // Create an output file name.
                    string outputFileName = Utilities.GetGuidString() + DataModellingResources.DotCs;

                    // Generate class.
                    IList<EdmSchemaError> errors = generator.GenerateCode(csdlFileName, outputFileName, refCSDLs);

                    if (errors.Count > 0)
                    {
                        StringBuilder error = new StringBuilder();
                        foreach (EdmSchemaError e in errors)
                            error.Append(e.Message);

                        throw new ZentityException(error.ToString());
                    }

                    // Add this csdl to reference csdl list.
                    refCSDLs.Add(csdlFileName);

                    // Add the code to object layer.
                    using (StreamReader rdr = new StreamReader(outputFileName))
                    {
                        sourceCodeTracker.Add(module.NameSpace, rdr.ReadToEnd());
                    }

                    // Delete the output file.
                    System.IO.File.Delete(outputFileName);
                }

                StringCollection objectLayer = new StringCollection();

                // Strip off additional source code.
                foreach (KeyValuePair<string, string> kvp in sourceCodeTracker)
                {
                    if (inputNamespaces.Contains(kvp.Key) &&
                        !this.Modules.Where(module => module.NameSpace == kvp.Key).First().IsMsShipped)
                        objectLayer.Add(kvp.Value);
                }

                return objectLayer;
            }
            finally
            {
                // Cleanup - delete all referenced csdl files.
                foreach (string refCSDL in refCSDLs)
                    if (System.IO.File.Exists(refCSDL))
                        System.IO.File.Delete(refCSDL);
            }
        }

        /// <summary>
        /// Generates the flattened C# source code for the specified modules.
        /// The source code also includes prevompiled-views for queries in Entity Framework to perform better.
        /// </summary>
        /// <param name="modulesToInclude">A list of module namespaces to generate source code 
        /// for. If this parameter is null, all modules of the data model are included in 
        /// the list.</param>
        /// <returns>Source code for each module defined in the input parameter.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public StringCollection GenerateFlattenedSourceCode(params string[] modulesToInclude)
        {
            // No need to Validate here. GenerateEFArtifacts does it for us.
            List<string> inputNamespaces = (modulesToInclude == null ||
                modulesToInclude != null && modulesToInclude.Length == 0) ?
                this.Modules.Select(tuple => tuple.NameSpace).ToList() :
                modulesToInclude.ToList();

            // Validate the input list.
            var nameSpaces = this.Modules.Select(tuple => tuple.NameSpace);
            var absentModules = inputNamespaces.Except(nameSpaces);
            if (absentModules.Count() > 0)
            {
                var absentModule = absentModules.First();
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ExceptionAbsentModuleNamespace, absentModule));
            }

            // Generate artifacts for all the modules in the model. This saves us from missing CSDL
            // references while generating source code. We filter out unwanted namespaces later.
            Dictionary<string, string> sourceCodeTracker = new Dictionary<string, string>();

            // Prepare a reference list of CSDLs.
            List<string> refCSDLs = new List<string>();
            // Add Core CSDL.
            refCSDLs.Add(Utilities.GetGuidString() + DataModellingResources.DotCsdl);

            // Create generator.
            EntityCodeGenerator generator = new EntityCodeGenerator(LanguageOption.GenerateCSharpCode);

            List<DataModelModule> sortedModules = SortModules(this.Modules.ToList());
            // Remove the Core module from sorted list. We cannot assume that the Core module is
            // the first module in sorted list. For example, if a module has zero resource types
            // it is independent of Core and thus can be sorted before Core.
            var removeModuleList = sortedModules.Where(tuple => tuple.NameSpace == DataModellingResources.ZentityCore || !modulesToInclude.Contains(tuple.NameSpace, StringComparer.OrdinalIgnoreCase));
            if (removeModuleList != null)
            {
                while (removeModuleList.Count() > 0)
                {
                    sortedModules.Remove(removeModuleList.First());
                }
            }

            try
            {
                StringBuilder sbFileRelationships = new StringBuilder();

                for (int i = 0; i < sortedModules.Count(); i++)
                {
                    DataModelModule module = sortedModules[i];

                    // Generate CSDL for this module. We cannot move this artifact generation 
                    // outside this loop since, the consolidated CSDL changes with the inclusion 
                    // of every module. For example, if the new module has some associations, 
                    // the Core CSDL will have to host new AssociationSet elements. We generate 
                    // and update the new Core CSDL in each iteration.
                    EFArtifactGenerationResults results = this.GenerateFlattenedEFArtifacts(module.NameSpace);

                    // Update Core CSDL.
                    results.Csdls.Where(tuple => tuple.Key == DataModellingResources.ZentityCore).First().Value.Save(refCSDLs[0]);

                    // Dump the CSDL.
                    string csdlFileName = Utilities.GetGuidString() + DataModellingResources.DotCsdl;
                    results.Csdls.Where(tuple => tuple.Key == module.NameSpace).First().Value.Save(csdlFileName);

                    // Create an output file name.
                    string outputFileName = Utilities.GetGuidString() + DataModellingResources.DotCs;

                    // Generate class.
                    IList<EdmSchemaError> errors = generator.GenerateCode(csdlFileName, outputFileName, refCSDLs);

                    if (errors.Count > 0)
                    {
                        StringBuilder error = new StringBuilder();
                        foreach (EdmSchemaError e in errors)
                            error.Append(e.Message);

                        throw new ZentityException(error.ToString());
                    }

                    // Add this csdl to reference csdl list.
                    refCSDLs.Add(csdlFileName);

                    // Add the source code to object layer.
                    using (StreamReader rdr = new StreamReader(outputFileName))
                    {
                        sourceCodeTracker.Add(module.NameSpace, rdr.ReadToEnd());
                    }

                    foreach (var resourceType in module.ResourceTypes)
                    {
                        sbFileRelationships.AppendFormat(DataModellingResources.AssemblyFileRelationshipFormat, resourceType.Name, module.NameSpace);
                    }

                    // Delete the output file.
                    System.IO.File.Delete(outputFileName);
                }

                StringCollection objectLayer = new StringCollection();

                // Strip off additional source code.
                foreach (KeyValuePair<string, string> kvp in sourceCodeTracker)
                {
                    if (inputNamespaces.Any(nameSpace => kvp.Key.Contains(nameSpace)))
                        objectLayer.Add(kvp.Value);
                }

                if (sbFileRelationships.Length > 0)
                {
                    objectLayer.Add(sbFileRelationships.ToString());
                }

                //objectLayer.Add(@"[assembly: System.Data.Objects.DataClasses.EdmRelationshipAttribute(""Zentity.Core"", ""ResourceHasFile_AudioFile"", ""Audio"", System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.ScholarlyWorks.Audio), ""File"", System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(Zentity.Core.File))]");
                return objectLayer;
            }
            finally
            {
                // Cleanup - delete all referenced csdl files.
                foreach (string refCSDL in refCSDLs)
                    if (System.IO.File.Exists(refCSDL))
                        System.IO.File.Delete(refCSDL);
            }
        }

        /// <summary>
        /// Generates the precompiled views for Entity Framework.
        /// </summary>
        /// <param name="efArtifacts">The EF artifacts as generated by GenerateEFArtifacts or GenerateFlattenedEFArtifacts.</param>
        /// <returns>The source code for the precompiled view.</returns>
        public string GeneratePrecompiledViews(EFArtifactGenerationResults efArtifacts)
        {
            EntityViewGenerator viewGenerator = new EntityViewGenerator(LanguageOption.GenerateCSharpCode);
            string outputViewName = Utilities.GetGuidString() + DataModellingResources.DotCs;

            // Generation of Pre-Compiled Views
            // load the csdl
            XmlReader[] cReaders = efArtifacts.Csdls.Select(csdl => new XmlNodeReader(csdl.Value)).ToArray();
            IList<EdmSchemaError> cErrors = null;
            EdmItemCollection edmItemCollection = MetadataItemCollectionFactory.CreateEdmItemCollection(cReaders, out cErrors);

            // load the ssdl 
            XmlReader[] sReaders = { new XmlNodeReader(efArtifacts.Ssdl) };
            IList<EdmSchemaError> sErrors = null;
            StoreItemCollection storeItemCollection = MetadataItemCollectionFactory.CreateStoreItemCollection(sReaders, out sErrors);

            // load the msl
            XmlReader[] mReaders = { new XmlNodeReader(efArtifacts.Msl) };
            IList<EdmSchemaError> mErrors = null;
            StorageMappingItemCollection mappingItemCollection = MetadataItemCollectionFactory.CreateStorageMappingItemCollection(edmItemCollection, storeItemCollection, mReaders, out mErrors);

            // Generate view
            IList<EdmSchemaError> errorsView = viewGenerator.GenerateViews(mappingItemCollection, outputViewName);

            if (errorsView.Count > 0)
            {
                StringBuilder error = new StringBuilder();
                foreach (EdmSchemaError e in errorsView)
                    error.Append(e.Message);

                throw new ZentityException(error.ToString());
            }

            return System.IO.File.ReadAllText(outputViewName);
        }

        /// <summary>
        /// Reloads the data model information from backend. The 
        /// <see cref="Zentity.Core.DataModel.Modules"/> property is re-initialized with a new 
        /// instance of <see cref="Zentity.Core.DataModelModuleCollection"/> object.
        /// </summary>
        public void Refresh()
        {

            // We open a new connection here and do not reuse context connection. Using context 
            // connection might raise errors if there is an explicit transaction opened on it by 
            // a client. In that case, the ExecuteNonQuery here should use the same client 
            // transaction and it is difficult to get hold of the client initiated transaction here.
            using (SqlConnection storeConnection =
                new SqlConnection(this.Parent.StoreConnectionString))
            {
                DataModelLoader.Refresh(this, storeConnection);
            }
        }

        /// <summary>
        /// Returns the TSQL script to synchronize the in-memory data model with the backend.
        /// It is strongly recommended that you take a backup of your database before executing 
        /// these scripts and run the scripts in a transaction. Also, consider executing the
        /// <see cref="Zentity.Core.DataModel.Refresh()"/> method after executing the scripts to
        /// reload the mapping information from the backend.
        /// </summary>
        /// <returns>TSQL script to synchronize the in-memory data model with the backend.
        /// </returns>
        /// <example> The example below shows how to generate the scripts that synchronize the
        /// in-memory data model with the backend store in a transaction. 
        /// <code>
        ///using Zentity.Core;
        ///using System.Xml;
        ///using System.IO;
        ///using System.Text;
        ///using System.Collections.Specialized;
        ///using System.Transactions;
        ///using System.Data.EntityClient;
        ///using System.Configuration;
        ///using System.Data.SqlClient;
        ///using System.Data;
        ///using System;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                PrintModules(context);
        ///
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].
        ///                    ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType
        ///                {
        ///                    Name = &quot;ScholarlyWork&quot;,
        ///                    BaseType = resourceTypeResource
        ///                };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    Uri = Guid.NewGuid().ToString(&quot;N&quot;),
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///                // Get the synchronization scripts.
        ///                StringCollection scripts = context.DataModel.GetSynchronizationScripts();
        ///
        ///                // Generate script.
        ///                using (StreamWriter writer = new StreamWriter(&quot;Zentity.Samples.sql&quot;))
        ///                {
        ///                    foreach (string command in scripts)
        ///                    {
        ///                        writer.WriteLine(command);
        ///                        writer.WriteLine(&quot;GO&quot;);
        ///                    }
        ///                    writer.Close();
        ///                }
        ///
        ///                // IMPORTANT: If you intend to use the generated script, it is highly recommended 
        ///                // that you take a backup of your database and run these scripts in a transaction.
        ///                // The generated sql script invokes [Core].[AfterSchemaChanges] in the end which
        ///                // in turn invokes all the modules (e.g. ChangeHistory) that have registered to be
        ///                // notified of schema changes. The dependent modules might take too long to process
        ///                // the changes. So, if you are using a TransactionScope, assign a sufficiently 
        ///                // large timeout.
        ///                using (SqlConnection storeConnection = new SqlConnection(context.StoreConnectionString))
        ///                {
        ///                    storeConnection.Open();
        ///                    Console.WriteLine(&quot;Executing synchronization scripts...&quot;);
        ///                    SqlTransaction tx = storeConnection.BeginTransaction();
        ///                    foreach (string str in scripts)
        ///                    {
        ///                        using (SqlCommand cmd = new SqlCommand())
        ///                        {
        ///                            cmd.Connection = storeConnection;
        ///                            cmd.Transaction = tx;
        ///                            cmd.CommandTimeout = 120;
        ///                            cmd.CommandText = &quot;sp_executesql&quot;;
        ///                            cmd.CommandType = CommandType.StoredProcedure;
        ///
        ///                            SqlParameter param = cmd.CreateParameter();
        ///                            param.DbType = DbType.String;
        ///                            param.Direction = ParameterDirection.Input;
        ///                            param.ParameterName = &quot;Cmd&quot;;
        ///                            param.Size = -1;
        ///                            param.Value = str;
        ///                            cmd.Parameters.Add(param);
        ///                            cmd.ExecuteNonQuery();
        ///                        }
        ///                    }
        ///                    tx.Commit();
        ///                }
        ///
        ///                // It is recommended to discard ZentityContext after each schema change and reload
        ///                // it with new metadata. For now, we'll just print the datamodel modules in the 
        ///                // store.
        ///                context.DataModel.Refresh();
        ///                PrintModules(context);
        ///            }
        ///        }
        ///
        ///        private static void PrintModules(ZentityContext context)
        ///        {
        ///            Console.WriteLine(&quot;Datamodel modules :--&quot;);
        ///            foreach (DataModelModule module in context.DataModel.Modules)
        ///                Console.WriteLine(&quot;Module Id:[{0}], Namespace:[{1}], IsMsShipped:[{2}]&quot;,
        ///                    module.Id, module.NameSpace, module.IsMsShipped);
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        public StringCollection GetSynchronizationScripts()
        {
            TableMappingCollection tableMappings;
            Dictionary<Guid, int> discriminators;
            Dictionary<Guid, string> associationViewMappings;
            return GetSynchronizationScripts(out tableMappings, out associationViewMappings,
                out discriminators);
        }

        /// <summary>
        /// Synchronizes the in-memory data model with the backend and saves the data model 
        /// information in the backend. It is strongly recommended that you take a backup of your 
        /// database before invoking this method, discard the ZentityContext object immediately 
        /// after invoking this method and reinitialize it with new metadata.
        /// </summary>
        /// <remarks> This method sometimes takes few minutes to complete. Stop SQL Agent service 
        /// if you are experiencing too long wait times. 
        /// <para>
        /// During synchronization, following types of database objects are created/updated/deleted.
        /// <ul>
        /// <li>Resource Tables - These tables host the actual data for the resources. Each resource is 
        /// an instance of a resource type. New columns are added to resource table with the introduction 
        /// of new scalar properties in the data model. A resource table is designed to have a maximum of 
        /// around 290 columns. Once, a resource table reaches its maximum limit a new resource table is 
        /// created in the database. Data model metadata tables store the mapping information between 
        /// resource table columns and scalar properties. Each resource table has ‘Id’ and ‘Discriminator’ 
        /// columns.
        /// </li>
        /// <li>Association Views - These views are created on Core.Relationship table for each of the 
        /// defined association in the data model. A predicate is used as a filter for the view definition. 
        /// For example, the view definition for association ‘ResourceHasFile’ looks like the following
        /// <code lang="T-SQL">
        ///CREATE VIEW [Core].[ResourceHasFile]
        ///WITH SCHEMABINDING
        ///AS
        ///	SELECT [SubjectResourceId], [ObjectResourceId]
        ///	FROM [Core].[Relationship] T
        ///	WHERE [T].[PredicateId] = '818A93F5-25A9-4149-A8D2-19104A352DA0';
        /// </code>
        /// These views are mapped to associations in the data model.
        /// </li>
        /// <li>CUD Procedures - Each resource type in the data model has one procedure each for creating, 
        /// updating and deleting the resources of that type from the repository. Likewise, Many-To-Many, 
        /// Many-To-ZeroOrOne, ZeroOrOne-To-Many and ZeroOrOne-To-ZeroOrOne associations also have these 
        /// CUD procedures to manipulate the relationships in the repository.
        /// </li>
        /// <li>Data Model Metadata Tables - These table are updated after altering the database schema to
        /// reflect the latest data model information.
        /// </li>
        /// </ul>
        /// </para>
        /// <br/>
        /// Figure below shows the overall flow for creating custom types and associations in the system.
        /// <br/>
        /// <img src="OverallFlow.bmp"/>
        /// </remarks>
        /// <example> The following example shows how to make changes to the in-memory data
        /// model and then sychronize the changes with the backend.
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                string namespaceName = &quot;Namespace&quot; + Guid.NewGuid().ToString(&quot;N&quot;);
        ///
        ///                // Create a new module.
        ///                DataModelModule module = new DataModelModule { NameSpace = namespaceName };
        ///                context.DataModel.Modules.Add(module);
        ///
        ///                // Create the ScholarlyWork type.
        ///                ResourceType resourceTypeResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].
        ///                    ResourceTypes[&quot;Resource&quot;];
        ///                ResourceType resourceTypeScholarlyWork = new ResourceType
        ///                {
        ///                    Name = &quot;ScholarlyWork&quot;,
        ///                    BaseType = resourceTypeResource
        ///                };
        ///                module.ResourceTypes.Add(resourceTypeScholarlyWork);
        ///
        ///                // Create some Scalar Properties.
        ///                ScalarProperty copyRight = new ScalarProperty { Name = &quot;CopyRight&quot;, DataType = DataTypes.String, MaxLength = 4000 };
        ///                resourceTypeScholarlyWork.ScalarProperties.Add(copyRight);
        ///
        ///                // Create some Navigation Properties.
        ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
        ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
        ///
        ///                // Create the Contact type.
        ///                ResourceType resourceTypeContact = new ResourceType { Name = &quot;Contact&quot;, BaseType = resourceTypeResource };
        ///                module.ResourceTypes.Add(resourceTypeContact);
        ///                ScalarProperty email = new ScalarProperty { Name = &quot;Email&quot;, DataType = DataTypes.String, MaxLength = 1024 };
        ///                resourceTypeContact.ScalarProperties.Add(email);
        ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
        ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
        ///
        ///                // Add SamplesScholarlyWorkAuthoredByContact association.
        ///                // Association names should be unique across all the modules in the data model.
        ///                Association association = new Association
        ///                {
        ///                    Name = namespaceName + &quot;_ScholarlyWorkAuthoredByContact&quot;,
        ///                    Uri = Guid.NewGuid().ToString(&quot;N&quot;),
        ///                    SubjectNavigationProperty = authors,
        ///                    ObjectNavigationProperty = authoredWorks,
        ///                    SubjectMultiplicity = AssociationEndMultiplicity.Many,
        ///                    ObjectMultiplicity = AssociationEndMultiplicity.Many
        ///                };
        ///
        ///                // Synchronize the in-memory data model with the backend store.
        ///                // Database objects are created/updated/deleted during synchronization.
        ///                // This method takes a few minutes to complete depending on the actions taken by 
        ///                // other modules (such as change history logging) in response to schema changes.
        ///                // Provide a sufficient timeout.
        ///                context.CommandTimeout = 300;
        ///                context.DataModel.Synchronize();
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        public void Synchronize()
        {
            TableMappingCollection tableMappings;
            Dictionary<Guid, int> discriminators;
            Dictionary<Guid, string> associationViewMappings;
            StringCollection queries = GetSynchronizationScripts(out tableMappings,
                out associationViewMappings, out discriminators);

            // Apply changes in a transaction.
            // Create a new connection for synchronization. We do not share the EntityConnection.
            int commandTimeout = this.Parent.OperationTimeout;
            using (SqlConnection storeConnection =
                new SqlConnection(this.Parent.StoreConnectionString))
            {
                storeConnection.Open();
                using (SqlTransaction tran = storeConnection.BeginTransaction())
                {
                    foreach (string str in queries)
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = storeConnection;
                            cmd.Transaction = tran;
                            cmd.CommandTimeout = commandTimeout;
                            cmd.CommandText = DataModellingResources.Core_SpExecuteSql;
                            cmd.CommandType = CommandType.StoredProcedure;

                            SqlParameter param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.Direction = ParameterDirection.Input;
                            param.ParameterName = DataModellingResources.Cmd;
                            param.Size = -1;
                            param.Value = str;
                            cmd.Parameters.Add(param);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tran.Commit();
                }
            }

            UpdateModelMappings(tableMappings, discriminators, associationViewMappings);

            // NOTE: DO NOT Refresh() the data model here. It may break the clients that have
            // cached the data model items. For example, if an application has reference to
            // Publication resource type and we refresh the model here, the cached reference 
            // on client side becomes stale and might success into errors if used further.
        }

        /// <summary>
        /// Gets the resource item count.
        /// </summary>
        /// <param name="zentityContext">The Zentity context.</param>
        /// <param name="namespaceList">The namespace list.</param>
        /// <param name="totalUniqueResourceItemCount">The total unique resource item count.</param>
        /// <returns>
        /// List of fully qualified ResourceType names and their count
        /// </returns>
        public static IDictionary<string, int> GetResourceItemCount(ZentityContext zentityContext, IEnumerable<string> namespaceList, out int totalUniqueResourceItemCount)
        {
            // Argument Validations
            if (zentityContext == null)
            {
                throw new ArgumentNullException("zentityContext");
            }

            if (namespaceList == null)
            {
                throw new ArgumentNullException("namespaceList");
            }

            Dictionary<string, int> resourceItemCount = new Dictionary<string, int>();
            totalUniqueResourceItemCount = 0;

            if (namespaceList.Count() == 0)
            {
                return resourceItemCount;
            }

            // Create the connection to Zentity DataBase and call the stored procedure
            using (SqlConnection storeConnection = new SqlConnection(zentityContext.StoreConnectionString))
            {
                if (storeConnection.State == ConnectionState.Closed)
                    storeConnection.Open();

                using (SqlCommand cmd = storeConnection.CreateCommand())
                {
                    cmd.CommandText = DataModellingResources.Core_GetDataModelResourceItemCount;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = zentityContext.OperationTimeout;

                    SqlParameter inputParam = cmd.CreateParameter();
                    inputParam.DbType = DbType.String;
                    inputParam.Direction = ParameterDirection.Input;
                    inputParam.ParameterName = "@namespaceList";
                    inputParam.Size = -1;
                    inputParam.Value = string.Join<string>(",", namespaceList);
                    cmd.Parameters.Add(inputParam);

                    SqlParameter outputParam = cmd.CreateParameter();
                    outputParam.DbType = DbType.Int32;
                    outputParam.Direction = ParameterDirection.Output;
                    outputParam.ParameterName = "@totalItemCount";
                    cmd.Parameters.Add(outputParam);

                    using (SqlDataReader dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader != null && dataReader.HasRows)
                        {
                            // Read the item count table from the reader
                            while (dataReader.Read())
                            {
                                if (dataReader[DataModellingResources.DataModel] != null &&
                                    dataReader[DataModellingResources.ResourceType] != null &&
                                    !string.IsNullOrWhiteSpace(dataReader[DataModellingResources.DataModel].ToString()) &&
                                    !string.IsNullOrWhiteSpace(dataReader[DataModellingResources.ResourceType].ToString()))
                                {
                                    string resourceTypeFullName = Utilities.MergeSubNames(dataReader[DataModellingResources.DataModel].ToString(),
                                                                                          dataReader[DataModellingResources.ResourceType].ToString());
                                    resourceItemCount.Add(resourceTypeFullName, Convert.ToInt32(dataReader[DataModellingResources.ItemCount]));
                                }
                            }
                        }
                    }

                    // Read the totalItemCount from the output parameter
                    if (outputParam.Value != System.DBNull.Value)
                    {
                        totalUniqueResourceItemCount = Convert.ToInt32(outputParam.Value);
                    }
                }
            }

            return resourceItemCount;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>Returns the computed hash string</returns>
        private static string ComputeHash(string inputString)
        {
            using (MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider())
            {
                byte[] buffer = Encoding.Unicode.GetBytes(inputString);
                buffer = provider.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in buffer)
                    sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                return sb.ToString();
            }
        }

        /// <summary>
        /// Detects the unsynchronized modules.
        /// </summary>
        /// <param name="originalModel">The original model.</param>
        /// <param name="newModel">The new model.</param>
        /// <param name="inputNamespaces">The input namespaces.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void DetectUnsynchronizedModules(DataModel originalModel, DataModel newModel, List<string> inputNamespaces)
        {
            ModuleCollectionChange changes = ComputeDifferences(originalModel, newModel);

            // Again the logic below is based on IDs since the namespaces might have changed for
            // the modules. Once we have all the module IDs that have undergone some change, we
            // locate those modules in the new graph and prepare a list of namespaces from the
            // new model. In the end, we check if there are any namespaces in the prepared list
            // that are also present in the input list. If yes, this means that some of the input
            // namespaces have some pending changes and thus we cannot successfully generate the
            // mapping files for them.
            List<Guid> unsynchronizedModuleIds = new List<Guid>();
            unsynchronizedModuleIds.AddRange(
                changes.AddedAssociations.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedDataModelModules.Select(tuple => tuple.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedNavigationProperties.Select(tuple => tuple.Parent.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedResourceTypes.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.AddedScalarProperties.Select(tuple => tuple.Parent.Parent.Id));

            unsynchronizedModuleIds.AddRange(
                changes.DeletedAssociations.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedDataModelModules.Select(tuple => tuple.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedNavigationProperties.Select(tuple => tuple.Parent.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedResourceTypes.Select(tuple => tuple.Parent.Id));
            unsynchronizedModuleIds.AddRange(
                changes.DeletedScalarProperties.Select(tuple => tuple.Parent.Parent.Id));

            foreach (var kvp in changes.UpdatedAssociations)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Id);
            }
            foreach (var kvp in changes.UpdatedDataModelModules)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Id);
            }
            foreach (var kvp in changes.UpdatedNavigationProperties)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Parent.Id);
            }
            foreach (var kvp in changes.UpdatedResourceTypes)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Id);
            }
            foreach (var kvp in changes.UpdatedScalarProperties)
            {
                unsynchronizedModuleIds.Add(kvp.Key.Parent.Parent.Id);
                unsynchronizedModuleIds.Add(kvp.Value.Parent.Parent.Id);
            }

            List<string> unsynchronizedModuleNamespaces = newModel.Modules.
                Where(tuple => unsynchronizedModuleIds.Contains(tuple.Id)).
                Select(tuple => tuple.NameSpace).ToList();

            var aModule = inputNamespaces.Intersect(unsynchronizedModuleNamespaces).
                FirstOrDefault();
            if (aModule != null)
                throw new ZentityException(string.Format(CultureInfo.InvariantCulture,
                    DataModellingResources.ExceptionUnsynchronizedModule, aModule));
        }

        /// <summary>
        /// Gets the navigation property by id.
        /// </summary>
        /// <param name="inputModules">The input modules.</param>
        /// <param name="id">The id.</param>
        /// <returns>The searched navgation property</returns>
        private static NavigationProperty GetNavigationPropertyById(DataModelModuleCollection inputModules, Guid id)
        {
            var navigationProperties = inputModules.SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties).
                Where(tuple => tuple.Id == id);

            if (navigationProperties.Count() > 1)
                throw new ZentityException(DataModellingResources.ExceptionMultipleNavigationPropertyDetected);
            else
                return navigationProperties.First();
        }

        /// <summary>
        /// Gets the resource type by id.
        /// </summary>
        /// <param name="inputModules">The input modules.</param>
        /// <param name="id">The id.</param>
        /// <returns>The searched resource type</returns>
        private static ResourceType GetResourceTypeById(DataModelModuleCollection inputModules, Guid id)
        {
            var resourceTypes = inputModules.SelectMany(tuple => tuple.ResourceTypes).
                Where(tuple => tuple.Id == id);

            if (resourceTypes.Count() > 1)
                throw new ZentityException(DataModellingResources.ExceptionMultipleResourceTypeDetected);
            else
                return resourceTypes.First();
        }

        /// <summary>
        /// Gets the synchronization scripts.
        /// </summary>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="associationViewMappings">The association view mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <returns>A string collection of all synchronization strings</returns>
        private StringCollection GetSynchronizationScripts(out TableMappingCollection tableMappings, out Dictionary<Guid, string> associationViewMappings, out Dictionary<Guid, int> discriminators)
        {
            // Validate the in-memory data model graph.
            Validate();

            DataModel synchronizedModel;
            GetSynchronizedModelAndMappings(out synchronizedModel, out tableMappings,
                out associationViewMappings, out discriminators);

            // Verify that the MsShipped modules are not changed.
            ValidateMsShippedModuleChanges(synchronizedModel, this);

            // Generate synchronization script for the changes. The scripts alter the database 
            // schema and also update the data model information in the backend tables.
            StringCollection scripts = SqlScriptGenerator.GenerateScripts(
                synchronizedModel, this, tableMappings, associationViewMappings, discriminators);

            return scripts;
        }

        /// <summary>
        /// Gets the synchronized model and mappings.
        /// </summary>
        /// <param name="synchronizedModel">The synchronized model.</param>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="associationViewMappings">The association view mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        private void GetSynchronizedModelAndMappings(out DataModel synchronizedModel, out TableMappingCollection tableMappings, out Dictionary<Guid, string> associationViewMappings, out Dictionary<Guid, int> discriminators)
        {
            synchronizedModel = new DataModel(this.Parent);
            tableMappings = new TableMappingCollection();
            associationViewMappings = new Dictionary<Guid, string>();
            discriminators = new Dictionary<Guid, int>();
            XmlDocument xDocMappings = new XmlDocument();

            // Create a new model from backend information. Also, get the table mappings for
            // the data model items. We can use the same connection. Wrap these two information 
            // pulls in a transaction so that we get the mapping information that is in sync with
            // the metadata information. For example, if client A pulls the metadata information 
            // then client B updates the information and then client A again pulls the mappings 
            // information, the mapping information will not be consistent with the metadata 
            // information that client A had pulled earlier. This may not be required since this 
            // method will be called very infrequently and it is very unlikely that readers and
            // writers are active at the same time.
            // Do not specify the isolation level here else we might get errors if this method is
            // invoked from within a TransactionScope with a different isolation level.
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
            {
                using (SqlConnection storeConnection =
                    new SqlConnection(this.Parent.StoreConnectionString))
                {
                    DataModelLoader.Refresh(synchronizedModel, storeConnection);

                    if (storeConnection.State == ConnectionState.Closed)
                        storeConnection.Open();

                    using (SqlCommand cmd = storeConnection.CreateCommand())
                    {
                        cmd.CommandText = DataModellingResources.Core_GetTableModelMap;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = this.Parent.OperationTimeout;

                        SqlParameter param = cmd.CreateParameter();
                        param.DbType = DbType.String;
                        param.Direction = ParameterDirection.Output;
                        param.ParameterName = DataModellingResources.TableModelMap;
                        param.Size = -1;
                        cmd.Parameters.Add(param);
                        cmd.ExecuteNonQuery();

                        xDocMappings.LoadXml(param.Value.ToString());
                    }
                }
                scope.Complete();
            }

            // Create table mappings.
            tableMappings.LoadFromXml(xDocMappings);

            // Create association mappings.
            foreach (Association assoc in synchronizedModel.Modules.
                SelectMany(module => module.Associations))
                associationViewMappings.Add(assoc.Id, assoc.ViewName);

            // Create discriminators.
            foreach (ResourceType r in synchronizedModel.Modules.
                SelectMany(module => module.ResourceTypes))
                discriminators.Add(r.Id, r.Discriminator);
        }

        /// <summary>
        /// Prepares the XML document from manifest resource stream.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="manifestResourceName">Name of the manifest resource.</param>
        /// <returns>The xml document from the resource stream</returns>
        private static XmlDocument PrepareXmlDocumentFromManifestResourceStream(Assembly assembly, string manifestResourceName)
        {
            XmlDocument resource;
            using (Stream resourceStream = assembly.GetManifestResourceStream(manifestResourceName))
            {
                resource = new XmlDocument();
                resource.Load(resourceStream);
            }
            return resource;
        }

        /// <summary>
        /// Sorts the supplied list of ResourceTypes by hierarchy.
        /// </summary>
        /// <param name="list">The source list.</param>
        /// <returns>The sorted list</returns>
        internal static List<ResourceType> SortByHierarchy(List<ResourceType> list)
        {
            List<Guid> visitedList = new List<Guid>();
            List<Guid> idList = new List<Guid>();
            idList.AddRange(list.Select(tuple => tuple.Id));

            List<ResourceType> sortedList = new List<ResourceType>();

            // Get the roots. Root elements are those whose base types are not present in the list.
            sortedList.AddRange(list.Where(tuple => tuple.BaseType == null ||
                tuple.BaseType != null && !idList.Contains(tuple.BaseType.Id)));

            // Process each entry in the list and queue up the derived types in the end of list. 
            // Basically, we are traversing the inheritance tree in depth first order.
            for (int i = 0; i < sortedList.Count; i++)
            {
                ResourceType resourceType = sortedList[i];

                // Check for cycles in the inheritance hierarchy.
                if (visitedList.Contains(resourceType.Id))
                    throw new ZentityException(DataModellingResources.InvalidResourceTypesCyclicInheritance);

                // Enqueue derived types.
                foreach (ResourceType derivedType in list.Where(tuple => tuple.BaseType != null &&
                    tuple.BaseType.Id == resourceType.Id))
                    sortedList.Add(derivedType);

                // Mark as visited.
                visitedList.Add(resourceType.Id);
            }

            // If the final count of sorted list is not equal to the input list then it means that
            // there are some elements missed that have their base type present in the collection and 
            // which cannot be reached by the root nodes. This is only possible if there are cylces
            // of resource types in the list.
            if (sortedList.Count != list.Count)
                throw new ZentityException(DataModellingResources.InvalidResourceTypesCyclicInheritance);

            return sortedList;
        }

        /// <summary>
        /// Sorts the data model modules list.
        /// </summary>
        /// <param name="moduleList">The source data model module list.</param>
        /// <returns>The sorted data model module list</returns>
        private static List<DataModelModule> SortModules(List<DataModelModule> moduleList)
        {
            // Do a topological sort of modules (http://en.wikipedia.org/wiki/Topological_sort).

            // Prepare a directed graph. Each node in the dictionary holds the Id of a module
            // and the list of Ids of modules that depend on this module, basically a list of
            // incoming edges.
            Dictionary<Guid, List<Guid>> directedGraph = new Dictionary<Guid, List<Guid>>();

            // Pass1: Populate modules.
            foreach (DataModelModule module in moduleList)
                directedGraph.Add(module.Id, new List<Guid>());

            // Pass2: Set dependencies.
            //                Core
            //                ^  ^
            //                |  |
            //    ScholarlyWorks |
            //                ^  |
            //                |  |
            //               Museum
            foreach (DataModelModule dependentModule in moduleList)
            {
                Guid dependentModuleId = dependentModule.Id;
                foreach (ResourceType resourceType in dependentModule.ResourceTypes)
                {
                    // BaseType might be null. e.g. for Core.Resource. Also, ignore all the 
                    // resource types whose parent modules are not set.
                    if (resourceType.BaseType != null && resourceType.BaseType.Parent != null &&
                        resourceType.BaseType.Parent.Id != dependentModuleId)
                    {
                        Guid baseModuleId = resourceType.BaseType.Parent.Id;
                        // Ignore modules that are not present in the input list.
                        if (directedGraph.Keys.Contains(baseModuleId))
                        {
                            List<Guid> dependentModuleIds = directedGraph[baseModuleId];
                            // The module id might already be added to the dependency list.
                            if (!dependentModuleIds.Contains(dependentModuleId))
                                dependentModuleIds.Add(dependentModuleId);
                        }
                    }
                }
            }

            List<Guid> L = TopologicalSort(directedGraph);

            List<DataModelModule> sortedModules = new List<DataModelModule>();
            for (int i = L.Count - 1; i >= 0; i--)
            {
                sortedModules.Add(moduleList.Where(tuple => tuple.Id == L[i]).First());
            }

            return sortedModules;
        }

        /// <summary>
        /// Sorts the directed graph by topology.
        /// </summary>
        /// <param name="directedGraph">The directed graph.</param>
        /// <returns>The sorted list</returns>
        private static List<Guid> TopologicalSort(Dictionary<Guid, List<Guid>> directedGraph)
        {
            List<Guid> L = new List<Guid>();
            Queue<Guid> S = new Queue<Guid>();

            foreach (KeyValuePair<Guid, List<Guid>> kvp in directedGraph)
            {
                if (kvp.Value.Count == 0)
                    S.Enqueue(kvp.Key);
            }

            while (S.Count > 0)
            {
                Guid n = S.Dequeue();
                L.Add(n);
                //for each node m with an edge e from n to m do
                //    remove edge e from the graph
                //    if m has no other incoming edges then
                //        insert m into S
                List<Guid> baseModuleIds = directedGraph.Where(tuple => tuple.Value.Contains(n)).
                    Select(tuple => tuple.Key).ToList();
                foreach (Guid m in baseModuleIds)
                {
                    List<Guid> dependentModuleIds = directedGraph[m];
                    dependentModuleIds.Remove(n);
                    if (dependentModuleIds.Count == 0)
                        S.Enqueue(m);
                }
                // Removal of the edge includes removing entry from the dependents list
                // of all the nodes as well as removing the node itself.
                directedGraph.Remove(n);
            }

            if (directedGraph.Count > 0)
                throw new ZentityException(DataModellingResources.ExceptionCycleInDependencyGraph);
            return L;
        }

        /// <summary>
        /// Updates the model mappings.
        /// </summary>
        /// <param name="tableMappings">The table mappings.</param>
        /// <param name="discriminators">The discriminators.</param>
        /// <param name="associationViewMappings">The association view mappings.</param>
        private void UpdateModelMappings(TableMappingCollection tableMappings, Dictionary<Guid, int> discriminators, Dictionary<Guid, string> associationViewMappings)
        {
            // Update mappings for scalar properties.
            // NOTE: The mappings are not updated by the SqlScriptGenerator. It is not
            // suppose to cause any side effects to the passed models. It however, 
            // updates the passed in mappings to point to newer values.
            foreach (ScalarProperty scalarProperty in this.Modules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.ScalarProperties))
            {
                ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                    scalarProperty.Id);
                scalarProperty.TableName = columnMapping.Parent.TableName;
                scalarProperty.ColumnName = columnMapping.ColumnName;
            }

            // Update mappings for navigation properties.
            foreach (NavigationProperty navigationProperty in this.Modules.
                SelectMany(tuple => tuple.ResourceTypes).
                SelectMany(tuple => tuple.NavigationProperties))
            {
                ColumnMapping columnMapping = tableMappings.GetColumnMappingByPropertyId(
                    navigationProperty.Id);
                if (columnMapping != null)
                {
                    navigationProperty.TableName = columnMapping.Parent.TableName;
                    navigationProperty.ColumnName = columnMapping.ColumnName;
                }
            }

            // Update mappings for associations.
            foreach (Association association in this.Modules.
                SelectMany(tuple => tuple.Associations))
            {
                association.ViewName = associationViewMappings[association.Id];
            }

            // Update Discriminators.
            foreach (ResourceType type in this.modules.
                SelectMany(tuple => tuple.ResourceTypes))
            {
                type.Discriminator = discriminators[type.Id];
            }
        }

        #endregion
    }
}
