// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Administration
{
    /// <summary>Adds functionality to the ResourceTypeChange entity class.</summary>
    /// <example>This example shows simple creation of a <see cref="Zentity.Core.ResourceType"/>. 
    /// It then shows how to retrieve those changes. Use 
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
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                DataModelModule module = new DataModelModule { NameSpace = &quot;ResourceTypeChangeSample&quot; };
    ///                context.DataModel.Modules.Add(module);
    ///                ResourceType rtResource = context.DataModel.Modules[&quot;Zentity.Core&quot;].ResourceTypes[&quot;Resource&quot;];
    ///
    ///                // Create a new resource type. 
    ///                ResourceType newBaseType = new ResourceType
    ///                {
    ///                    BaseType = rtResource,
    ///                    Name = &quot;NewBase&quot;,
    ///                    Uri = &quot;urn:zentity-samples:resource-type:new-base&quot;
    ///                };
    ///
    ///                // Add some scalar properties to it. 
    ///                ScalarProperty baseProp1 = new ScalarProperty(&quot;Prop1&quot;, DataTypes.String);
    ///                baseProp1.MaxLength = -1;
    ///                newBaseType.ScalarProperties.Add(baseProp1);
    ///                ScalarProperty baseProp2 = new ScalarProperty(&quot;Prop2&quot;, DataTypes.Int32);
    ///                newBaseType.ScalarProperties.Add(baseProp2);
    ///
    ///                // Add base type to context. 
    ///                module.ResourceTypes.Add(newBaseType);
    ///
    ///                // Create another resource type. Make sure that the base type reffered
    ///                // in the resource type declaration is already present in the resource
    ///                // type collection.
    ///                ResourceType newDerivedType = new ResourceType
    ///                {
    ///                    BaseType = newBaseType,
    ///                    Name = &quot;NewDerived&quot;,
    ///                    Uri = &quot;urn:zentity-samples:resource-type:new-derived&quot;
    ///                };
    ///                ScalarProperty derivedProp1 = new ScalarProperty(&quot;Prop3&quot;, DataTypes.String);
    ///                derivedProp1.MaxLength = 1024;
    ///                newDerivedType.ScalarProperties.Add(derivedProp1);
    ///
    ///                module.ResourceTypes.Add(newDerivedType);
    ///
    ///                // This method takes a few minutes to complete depending on the actions taken by 
    ///                // other modules (such as change history logging) in response to schema changes.
    ///                // Provide a sufficient timeout.
    ///                context.CommandTimeout = 300;
    ///                context.DataModel.Synchronize();
    ///            }
    ///
    ///            // Give the background job some time to process the changes.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///
    ///            // Retrieve the changes.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                foreach (ResourceTypeChange rtc in context.ResourceTypeChanges.Include(&quot;ChangeSet&quot;).
    ///                    Where(tuple =&gt; tuple.Operation.Name == &quot;Insert&quot;))
    ///                {
    ///                    Console.WriteLine(&quot;ResourceType: [{0}] created on [{1}].&quot;,
    ///                        rtc.NextName, rtc.ChangeSet.DateCreated);
    ///                    
    ///                    // Print the scalar properties.
    ///                    // Make sure you use MultipleActiveResultSets = true in the connection string.
    ///                    foreach (ScalarPropertyChange spc in context.ScalarPropertyChanges.Include(&quot;ChangeSet&quot;).
    ///                        Where(tuple =&gt; tuple.ChangeSet.Id == rtc.ChangeSet.Id &amp;&amp; 
    ///                            tuple.NextResourceTypeId == rtc.ResourceTypeId))
    ///                    {
    ///                        Console.WriteLine(&quot;Scalar Property Name: [{0}], DataType: [{1}], Nullable: [{2}]&quot;,
    ///                            spc.NextName, spc.NextDataType, spc.NextNullable);
    ///                    }
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ResourceTypeChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceTypeChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal ResourceTypeChange()
        {
        }
    }
}
