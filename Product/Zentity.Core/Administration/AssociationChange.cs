// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Administration
{
    /// <summary>Adds functionality to the AssociationChange entity class.</summary>
    /// <example>This example shows simple creation, updating and deletion of an 
    /// <see cref="Zentity.Core.Association"/>. It then shows how to retrieve the 
    /// repository changes. Use <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
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
    ///            Guid associationId;
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
    ///                NavigationProperty authors = new NavigationProperty { Name = &quot;Authors&quot; };
    ///                resourceTypeScholarlyWork.NavigationProperties.Add(authors);
    ///
    ///                // Create the Contact type.
    ///                ResourceType resourceTypeContact = new ResourceType
    ///                {
    ///                    Name = &quot;Contact&quot;,
    ///                    BaseType = resourceTypeResource
    ///                };
    ///                module.ResourceTypes.Add(resourceTypeContact);
    ///                NavigationProperty authoredWorks = new NavigationProperty { Name = &quot;AuthoredWorks&quot; };
    ///                resourceTypeContact.NavigationProperties.Add(authoredWorks);
    ///
    ///                // Add association. Association names should be unique across all the modules in the data model.
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
    ///                // This method takes a few minutes to complete depending on the actions taken by 
    ///                // other modules (such as change history logging) in response to schema changes.
    ///                // Provide a sufficient timeout.
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///                associationId = association.Id;
    ///                Console.WriteLine(&quot;Created association with Id: [{0}]&quot;, associationId);
    ///
    ///                // Update Association.
    ///                association.ObjectMultiplicity = AssociationEndMultiplicity.One;
    ///                context.DataModel.Synchronize();
    ///
    ///                // Delete Association.
    ///                association.SubjectNavigationProperty = null;
    ///                association.ObjectNavigationProperty = null;
    ///                resourceTypeScholarlyWork.NavigationProperties.Remove(authors);
    ///                resourceTypeContact.NavigationProperties.Remove(authoredWorks);
    ///                context.DataModel.Synchronize();
    ///            }
    ///
    ///            // Give the background job some time to process the changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            // Retrieve the changes.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                foreach (AssociationChange ac in context.AssociationChanges.
    ///                    Include(&quot;ChangeSet&quot;).Include(&quot;Operation&quot;).
    ///                    Where(tuple =&gt; tuple.AssociationId == associationId))
    ///                {
    ///                    Console.WriteLine(&quot;Changeset: [{0}] created on [{1}].&quot;,
    ///                        ac.ChangeSet.Id, ac.ChangeSet.DateCreated);
    ///                    Console.WriteLine(&quot;AssociationChange Operation:[{0}], &quot; +
    ///                        &quot;PreviousSubjectMultiplicity:[{1}], NextSubjectMultiplicity:[{2}], &quot; +
    ///                        &quot;PreviousObjectMultiplicity:[{3}], NextObjectMultiplicity:[{4}]&quot;,
    ///                        ac.Operation.Name, ac.PreviousSubjectMultiplicity, ac.NextSubjectMultiplicity,
    ///                        ac.PreviousObjectMultiplicity, ac.NextObjectMultiplicity);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class AssociationChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal AssociationChange()
        {
        }
    }
}
