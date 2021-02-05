// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Administration
{
    /// <summary>Adds functionality to the ResourcePropertyChange entity class.</summary>
    /// <example>This example shows simple creation of a <see cref="Zentity.Core.ResourceProperty"/> 
    /// and retrieving the changes. Use <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
    /// to enable the change history feature if not enabled already.
    /// <code>
    /// ///using System;
    ///using System.Collections.Generic;
    ///using System.Linq;
    ///using System.Text;
    ///using Zentity.Core;
    ///using System.Threading;
    ///using Zentity.Administration;
    ///using System.Collections.ObjectModel;
    ///namespace ChangeHistorySamples
    ///{
    ///    class ResourcePropertyChangeSample
    ///    {
    ///        static void Main(string[] args)
    ///        {
    ///            //Create Resource property
    ///            Guid resourcePropertyId = CreateResourceProperty();
    ///
    ///            PrintResourceProperty(resourcePropertyId);
    ///            RetriveResourcePropertyChanges();
    ///
    ///            //Modify Resource property
    ///            ModifyResourceProperty(resourcePropertyId);
    ///            PrintResourceProperty(resourcePropertyId);
    ///            RetriveResourcePropertyChanges();
    ///
    ///            //Delete Resource property 
    ///            DeleteResourceProperty(resourcePropertyId);
    ///            PrintResourceProperty(resourcePropertyId);
    ///            RetriveResourcePropertyChanges();
    ///
    ///            Console.Read();
    ///        }
    ///
    ///        static Guid CreateResourceProperty()
    ///        {
    ///            Guid resourcePropertyId;
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///
    ///                //Create Resource
    ///                Resource resource = new Resource();
    ///                resource.Title = &quot;Sample Resource&quot;;
    ///                resource.Uri = &quot;urn:change-history-samples/resource/sample-resource&quot;;
    ///
    ///                //Create Property
    ///                Property property = new Property();
    ///                property.Name = &quot;CreatedBy&quot;;
    ///                property.Uri = &quot;urn:change-history-samples/property/created-by&quot;;
    ///
    ///                ResourceProperty resProperty = new ResourceProperty();
    ///                resourcePropertyId = resProperty.Id;
    ///                resProperty.Resource = resource;
    ///
    ///                resProperty.Property = property;
    ///                resProperty.Value = DateTime.Now.ToString();
    ///
    ///                context.AddToResourceProperties(resProperty);
    ///                context.SaveChanges();
    ///
    ///            }
    ///            //Changeset entries are processed by a background 
    ///            //job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///            return resourcePropertyId;
    ///        }
    ///
    ///        static void ModifyResourceProperty(Guid resourcePropertyId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                ResourceProperty resProperty = context.ResourceProperties
    ///                                      .Where(tuple => tuple.Id == resourcePropertyId)
    ///                                      .First();
    ///                resProperty.Value = DateTime.Now.ToString() ;
    ///                context.SaveChanges();
    ///            }
    ///
    ///            //Changeset entries are processed by a background 
    ///            //job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///        }
    ///
    ///        static void DeleteResourceProperty(Guid resourcePropertyId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                ResourceProperty resProperty = context.ResourceProperties
    ///                                      .Where(tuple => tuple.Id == resourcePropertyId)
    ///                                      .First();
    ///                context.DeleteObject(resProperty);
    ///                context.SaveChanges();
    ///            }
    ///
    ///            //Changeset entries are processed by a background 
    ///            //job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///        }
    ///
    ///        static void PrintResourceProperty(Guid resourcePropertyId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                ResourceProperty resProperty = context.ResourceProperties
    ///                                      .Include(&quot;Property&quot;)
    ///                                      .Include(&quot;Resource&quot;)
    ///                                      .Where(tuple => tuple.Id == resourcePropertyId)
    ///                                      .FirstOrDefault();
    ///
    ///                PrintLine();
    ///                if (resProperty != null)
    ///                {
    ///                    Console.WriteLine(&quot;Resource Id    [{0}]&quot;, resProperty.Resource.Id);
    ///                    Console.WriteLine(&quot;Resource Title [{0}]&quot;, resProperty.Resource.Title);
    ///                    Console.WriteLine(&quot;Property Id    [{0}]&quot;, resProperty.Property.Id);
    ///                    Console.WriteLine(&quot;Property Name  [{0}]&quot;, resProperty.Property.Name);
    ///                    Console.WriteLine(&quot;Property Value [{0}]&quot;, resProperty.Value);
    ///                }
    ///                else
    ///                    Console.WriteLine(&quot;Oops!! resource property does not exist.&quot;);
    ///                PrintLine();
    ///            }
    ///
    ///        }
    ///        static void RetriveResourcePropertyChanges()
    ///        {
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///
    ///                // Pick the last changeset.
    ///                ChangeSet changeSet = adminContext.ChangeSets.
    ///                    Include(&quot;ResourcePropertyChanges.Operation&quot;).
    ///                    OrderByDescending(tuple => tuple.DateCreated).First();
    ///
    ///                if (changeSet != null)
    ///                {
    ///                    PrintLine();
    ///                    Console.WriteLine(&quot;Changeset [{0}] created on [{1}]&quot;,
    ///                        changeSet.Id, changeSet.DateCreated);
    ///
    ///                    foreach (ResourcePropertyChange resourcePropertyChange in changeSet.ResourcePropertyChanges)
    ///                    {
    ///                        Console.WriteLine(&quot;&quot;);
    ///                        resourcePropertyChange.ChangeSetReference.Load();
    ///                        resourcePropertyChange.OperationReference.Load();
    ///                        Console.WriteLine(&quot;Operation           [{0}]&quot;, resourcePropertyChange.Operation.Name);
    ///                        Console.WriteLine(&quot;ResourcePropertyId  [{0}]&quot;, resourcePropertyChange.ResourcePropertyId);
    ///                        Console.WriteLine(&quot;PreviousPropertyId  [{0}]&quot;, resourcePropertyChange.PreviousPropertyId);
    ///                        Console.WriteLine(&quot;NextPropertyId      [{0}]&quot;, resourcePropertyChange.NextPropertyId);
    ///                        Console.WriteLine(&quot;PreviousResourceId  [{0}]&quot;, resourcePropertyChange.PreviousResourceId);
    ///                        Console.WriteLine(&quot;NextResourceId      [{0}]&quot;, resourcePropertyChange.NextResourceId);
    ///                        Console.WriteLine(&quot;PreviousValue       [{0}]&quot;, resourcePropertyChange.PreviousValue);
    ///                        Console.WriteLine(&quot;NextValue           [{0}]&quot;, resourcePropertyChange.NextValue);
    ///                        Console.WriteLine(&quot;&quot;);
    ///                    }
    ///                }
    ///            }
    ///
    ///        }
    ///        static void PrintLine()
    ///        {
    ///            for (int i = 0; i &lt; 75; i++)
    ///                Console.Write(&quot;-&quot;);
    ///            Console.WriteLine(&quot;&quot;);
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ResourcePropertyChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePropertyChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal ResourcePropertyChange()
        {
        }
    }
}
