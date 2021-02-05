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

namespace Zentity.Administration
{
    /// <example>This example shows how to create custom resource types and retrieve changes done to the catalog.
    /// Use AdministrationContext.EnableChangeHistory() to enable the change history feature if not enabled already. 
    /// Make sure to use MultipleActiveResultSets = true in the AdministrationContext connection string.
    ///<code lang="xml">
    ///  &lt;connectionStrings&gt;
    ///    &lt;add name=&quot;ZentityContext&quot; 
    ///         connectionString=&quot;provider=System.Data.SqlClient;
    ///         metadata=res://Zentity;
    ///         provider connection string='Data Source=.;
    ///         Initial Catalog=Zentity;Integrated Security=True;
    ///         MultipleActiveResultSets=True'&quot;
    ///         providerName=&quot;System.Data.EntityClient&quot; /&gt;
    ///    &lt;add name=&quot;AdministrationContext&quot;
    ///         connectionString=&quot;provider=System.Data.SqlClient;
    ///         metadata=res://Zentity;
    ///         provider connection string='Data Source=.;
    ///         Initial Catalog=Zentity;Integrated Security=True;
    ///         MultipleActiveResultSets=True'&quot;
    ///         providerName=&quot;System.Data.EntityClient&quot; /&gt;
    ///  &lt;/connectionStrings&gt;
    ///</code>
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
    ///                // Create a new resource type. 
    ///                ResourceTypeInfo newBaseType = new ResourceTypeInfo
    ///                {
    ///                    BaseType = context.ResourceTypes[&quot;Zentity.Core.Publication&quot;],
    ///                    NameSpace = &quot;ZentitySamples&quot;,
    ///                    Name = &quot;NewBase&quot;,
    ///                    Uri = &quot;zentitySamples:resource-type:new-base&quot;
    ///                };
    ///
    ///                // Add some scalar properties to it. 
    ///                ScalarProperty baseProp1 = new ScalarProperty(&quot;Prop1&quot;, DataTypes.String);
    ///                baseProp1.MaxLength = -1;
    ///                newBaseType.Properties.Add(baseProp1);
    ///                ScalarProperty baseProp2 = new ScalarProperty(&quot;Prop2&quot;, DataTypes.Int32);
    ///                newBaseType.Properties.Add(baseProp2);
    ///
    ///                // Add base type to context. 
    ///                context.ResourceTypes.Add(newBaseType);
    ///
    ///                // Create another resource type. Make sure that the base type reffered
    ///                // in the resource type declaration is already present in the resource
    ///                // type collection.
    ///                ResourceTypeInfo newDerivedType = new ResourceTypeInfo
    ///                {
    ///                    BaseType = context.ResourceTypes[&quot;ZentitySamples.NewBase&quot;],
    ///                    NameSpace = &quot;ZentitySamples&quot;,
    ///                    Name = &quot;NewDerived&quot;,
    ///                    Uri = &quot;zentitySamples:resource-type:new-derived&quot;
    ///                };
    ///                ScalarProperty derivedProp1 = new ScalarProperty(&quot;Prop3&quot;, DataTypes.String);
    ///                derivedProp1.MaxLength = 1024;
    ///                newDerivedType.Properties.Add(derivedProp1);
    ///
    ///                context.ResourceTypes.Add(newDerivedType);
    ///
    ///                // Save off the changes to backend. 
    ///                context.ResourceTypes.Synchronize();
    ///            }
    ///
    ///            // Give the background job some time to process the changes.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///
    ///            // Retrieve the changes.
    ///            using (AdministrationContext context = new AdministrationContext())
    ///            {
    ///                foreach (ResourceTypeCatalogChange rtc in context.ResourceTypeCatalogChanges.Include(&quot;ChangeSet&quot;).
    ///                    Where(tuple =&gt; tuple.Operation.Name == &quot;Insert&quot;))
    ///                {
    ///                    Console.WriteLine(&quot;ResourceType: [{0}] created on [{1}].&quot;,
    ///                        rtc.NextNamespace + &quot;.&quot; + rtc.NextName, rtc.ChangeSet.DateCreated);
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
    ///
    ///            // Cleanup.
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                ResourceTypeInfo newBaseType = context.ResourceTypes[&quot;ZentitySamples.NewBase&quot;];
    ///                ResourceTypeInfo newDerivedType = context.ResourceTypes[&quot;ZentitySamples.NewDerived&quot;];
    ///
    ///                // Remove the derived types before the base type. 
    ///                context.ResourceTypes.Remove(newDerivedType);
    ///                context.ResourceTypes.Remove(newBaseType);
    ///
    ///                // Save off the changes to backend. 
    ///                context.ResourceTypes.Synchronize();
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ResourceTypeCatalogChange
    {
        // Making the constructor internal.
        internal ResourceTypeCatalogChange()
        {
        }
    }
}
