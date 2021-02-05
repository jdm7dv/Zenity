// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Administration
{
    /// <summary>Adds functionality to the DataModelModuleChange entity class.</summary>
    /// <example>This example shows simple creation and deletion of a data model module. 
    /// It then shows how to retrieve the repository changes. Use 
    /// <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
    /// to enable the change history feature if not enabled already. 
    /// <code>
    ///using System;
    ///using System.Linq;
    ///using Zentity.Administration;
    ///using Zentity.Core;
    ///using System.Threading;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            Guid moduleId;
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
    ///
    ///                // Synchronize to alter the database schema.                 
    ///                // This method takes a few minutes to complete depending on the actions taken by 
    ///                // other modules (such as change history logging) in response to schema changes.
    ///                // Provide a sufficient timeout.
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///                moduleId = module.Id;
    ///
    ///                // Delete the module.
    ///                context.DataModel.Modules.Remove(module);
    ///                context.DataModel.Synchronize();
    ///            }
    ///
    ///            // Give the background job some time to process changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            // Retrieve changes.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                foreach (DataModelModuleChange dmm in context.DataModelModuleChanges.
    ///                    Include(&quot;Changeset&quot;).Include(&quot;Operation&quot;).
    ///                    Where(tuple =&gt; tuple.DataModelModuleId == moduleId))
    ///                {
    ///                    Console.WriteLine(&quot;Changeset: [{0}] created on [{1}].&quot;,
    ///                        dmm.ChangeSet.Id, dmm.ChangeSet.DateCreated);
    ///                    Console.WriteLine(&quot;DataModelModuleChange ModuleId:[{0}], Operation[{1}]&quot;,
    ///                        dmm.DataModelModuleId, dmm.Operation.Name);
    ///
    ///                    foreach (ResourceTypeChange rtc in context.ResourceTypeChanges.
    ///                        Include(&quot;Changeset&quot;).Include(&quot;Operation&quot;).
    ///                        Where(tuple =&gt; tuple.ChangeSet.Id == dmm.ChangeSet.Id &amp;&amp;
    ///                        (tuple.PreviousDataModelModuleId == dmm.DataModelModuleId ||
    ///                        tuple.NextDataModelModuleId == dmm.DataModelModuleId)))
    ///                    {
    ///                        Console.WriteLine(&quot;ResourceTypeChange ResourceTypeId:[{0}], Operation:[{1}]&quot;,
    ///                            rtc.ResourceTypeId, rtc.Operation.Name);
    ///                    }
    ///
    ///                    foreach (AssociationChange ac in context.AssociationChanges.
    ///                        Include(&quot;Changeset&quot;).Include(&quot;Operation&quot;).
    ///                        Where(tuple =&gt; tuple.ChangeSet.Id == dmm.ChangeSet.Id))
    ///                    {
    ///                        Console.WriteLine(&quot;AssociationChange AssociationId:[{0}], Operation:[{1}]&quot;,
    ///                            ac.AssociationId, ac.Operation.Name);
    ///                    }
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class DataModelModuleChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataModelModuleChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal DataModelModuleChange()
        {
        }
    }
}
