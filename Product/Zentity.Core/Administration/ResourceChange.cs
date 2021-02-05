// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Administration
{
    /// <summary>Adds functionality to the ResourceChange entity class.</summary>
    /// <example>This example shows simple creation, updating and deletion of a 
    /// <see cref="Zentity.Core.Resource"/>. It then shows how to retrieve those changes. 
    /// Use <see cref="Zentity.Administration.AdministrationContext.EnableChangeHistory"/> 
    /// to enable the change history feature if not enabled already. 
    /// <code>
    ///using System;
    ///using System.Linq;
    ///using Zentity.Core;
    ///using System.Threading;
    ///using Zentity.Administration;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            //Create Resource
    ///            Guid resourceId = CreateResource();
    ///            PrintResource(resourceId);
    ///            RetriveLatestResourceChangeset(resourceId);
    ///
    ///            //Modify Resource
    ///            ModifyResource(resourceId);
    ///            PrintResource(resourceId);
    ///            RetriveLatestResourceChangeset(resourceId);
    ///
    ///            //Delete Resource 
    ///            DeleteResource(resourceId);
    ///            PrintResource(resourceId);
    ///            RetriveLatestResourceChangeset(resourceId);
    ///        }
    ///
    ///        static Guid CreateResource()
    ///        {
    ///            Guid resourceId;
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Resource resource = new Resource();
    ///                resourceId = resource.Id;
    ///                resource.Title = &quot;Create Title for SampleResource &quot;;
    ///                resource.Uri = &quot;urn:change-history-samples/resource/sample-resource&quot;;
    ///                context.AddToResources(resource);
    ///                context.SaveChanges();
    ///            }
    ///            //Changeset entries are processed by a background job that is invoked 
    ///            // every 10 seconds.Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///            return resourceId;
    ///        }
    ///
    ///        static void ModifyResource(Guid resourceId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Resource resource = context.Resources.
    ///                    Where(tuple =&gt; tuple.Id == resourceId).First();
    ///                resource.Title = &quot;Modify Title for SampleResource &quot;;
    ///                context.SaveChanges();
    ///            }
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///        }
    ///
    ///        static void DeleteResource(Guid resourceId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Resource resource = context.Resources.
    ///                    Where(tuple =&gt; tuple.Id == resourceId).First();
    ///                context.DeleteObject(resource);
    ///                context.SaveChanges();
    ///            }
    ///            Thread.Sleep(new TimeSpan(0, 0, 20));
    ///        }
    ///
    ///        static void PrintResource(Guid resourceId)
    ///        {
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                Resource resource = context.Resources.
    ///                    Where(tuple =&gt; tuple.Id == resourceId).FirstOrDefault();
    ///                PrintLine();
    ///                if (resource != null)
    ///                {
    ///                    Console.WriteLine(&quot;Id   [{0}]&quot;, resource.Id);
    ///                    Console.WriteLine(&quot;Name [{0}]&quot;, resource.Title);
    ///                    Console.WriteLine(&quot;Uri  [{0}]&quot;, resource.Uri);
    ///                }
    ///                else
    ///                    Console.WriteLine(&quot;Resource not found.&quot;);
    ///                PrintLine();
    ///            }
    ///        }
    ///
    ///        static void RetriveLatestResourceChangeset(Guid resourceId)
    ///        {
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///                ResourceChange resourceChange = adminContext.ResourceChanges.
    ///                    Where(tuple =&gt; tuple.ResourceId == resourceId).
    ///                    OrderByDescending(tuple =&gt; tuple.ChangeSet.DateCreated).
    ///                    FirstOrDefault();
    ///
    ///                if (resourceChange != null)
    ///                {
    ///                    Console.WriteLine(&quot;&quot;);
    ///                    resourceChange.ChangeSetReference.Load();
    ///                    resourceChange.OperationReference.Load();
    ///                    Console.WriteLine(&quot;Operation            [{0}]&quot;, resourceChange.Operation.Name);
    ///                    Console.WriteLine(&quot;ResourceId           [{0}]&quot;, resourceChange.ResourceId);
    ///                    Console.WriteLine(&quot;ResourceTypeFullName [{0}]&quot;, resourceChange.ResourceTypeFullName);
    ///                    Console.WriteLine(&quot;PropertyChanges    \n {0}&quot;, resourceChange.PropertyChanges);
    ///                    Console.WriteLine(&quot;&quot;);
    ///                }
    ///            }
    ///        }
    ///
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
    /// <remarks>
    /// The PropertyChanges property is a an XML document stored as a string value in the store.
    /// This document describes the changes in property values for a particular resource. The 
    /// document has the following schema.
    /// <code lang="xml">
    ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
    ///&lt;xs:schema attributeFormDefault=&quot;unqualified&quot; elementFormDefault=&quot;qualified&quot; xmlns:xs=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
    ///  &lt;xs:element name=&quot;PropertyChanges&quot;&gt;
    ///    &lt;xs:complexType&gt;
    ///      &lt;xs:sequence&gt;
    ///        &lt;xs:element maxOccurs=&quot;unbounded&quot; name=&quot;PropertyChange&quot;&gt;
    ///          &lt;xs:complexType&gt;
    ///            &lt;xs:sequence minOccurs=&quot;0&quot;&gt;
    ///              &lt;xs:element minOccurs=&quot;0&quot; name=&quot;PreviousValue&quot; type=&quot;xs:string&quot; /&gt;
    ///              &lt;xs:element minOccurs=&quot;0&quot; name=&quot;NextValue&quot; type=&quot;xs:string&quot; /&gt;
    ///            &lt;/xs:sequence&gt;
    ///            &lt;xs:attribute name=&quot;PropertyName&quot; type=&quot;xs:string&quot; use=&quot;required&quot; /&gt;
    ///            &lt;xs:attribute name=&quot;Changed&quot; type=&quot;xs:string&quot; use=&quot;optional&quot; /&gt;
    ///          &lt;/xs:complexType&gt;
    ///        &lt;/xs:element&gt;
    ///      &lt;/xs:sequence&gt;
    ///      &lt;xs:attribute name=&quot;ResourceId&quot; type=&quot;xs:string&quot; use=&quot;required&quot; /&gt;
    ///      &lt;xs:attribute name=&quot;ResourceTypeFullName&quot; type=&quot;xs:string&quot; use=&quot;required&quot; /&gt;
    ///    &lt;/xs:complexType&gt;
    ///  &lt;/xs:element&gt;
    ///&lt;/xs:schema&gt;
    /// </code>
    /// <para>Empty strings are treated separate from NULL values. An empty 'PreviousValue' or 
    /// 'NextValue' element represents empty string. Absence of these elements represents NULL 
    /// values.
    /// </para>
    /// For an insert operation, the ‘PreviousValue’ element is not present for the 
    /// ‘PropertyChange’ element. Absence of the ‘NextValue’ in this case represents that the 
    /// inserted value is NULL for the property. An example XML is shown below.
    /// <code lang="xml">
    ///&lt;PropertyChanges ResourceId=&quot;5A732A9F-8824-4BD1-AFF8-FB9A687B1C81&quot; ResourceTypeFullName=&quot;Zentity.ScholarlyWorks.Lecture&quot;&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Title&quot;&gt;
    ///    &lt;NextValue&gt;Lecture1&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateModified&quot;&gt;
    ///    &lt;NextValue&gt;Apr  2 2009 10:05PM&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Series&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;License&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateValidUntil&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Notes&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Image&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAdded&quot;&gt;
    ///    &lt;NextValue&gt;Apr  2 2009 10:05PM&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Audience&quot;&gt;
    ///    &lt;NextValue&gt;All .NET People&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Uri&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Abstract&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Venue&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Copyright&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Description&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateValidFrom&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Language&quot;&gt;
    ///    &lt;NextValue&gt;C#&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateEnd&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Scope&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateStart&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAvailableUntil&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAvailableFrom&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateCopyrighted&quot; /&gt;
    ///&lt;/PropertyChanges&gt;
    /// </code>
    /// <para>For delete operation, the ‘NextValue’ element is not present for the Property. 
    /// Absence of a ‘PreviousValue’ in this case represents that the property value was NULL 
    /// before deletion. An example is shown below.
    /// </para>
    /// <code lang="xml">
    ///&lt;PropertyChanges ResourceId=&quot;5A732A9F-8824-4BD1-AFF8-FB9A687B1C81&quot; ResourceTypeFullName=&quot;Zentity.ScholarlyWorks.Lecture&quot;&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Title&quot;&gt;
    ///    &lt;PreviousValue&gt;Lecture1&lt;/PreviousValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateModified&quot;&gt;
    ///    &lt;PreviousValue&gt;Apr  2 2009 10:05PM&lt;/PreviousValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Series&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;License&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateValidUntil&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Notes&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Image&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAdded&quot;&gt;
    ///    &lt;PreviousValue&gt;Apr  2 2009 10:05PM&lt;/PreviousValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Audience&quot;&gt;
    ///    &lt;PreviousValue&gt;All .NET People Who Knows C# Only&lt;/PreviousValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Uri&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Abstract&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Venue&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Copyright&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Description&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateValidFrom&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Language&quot;&gt;
    ///    &lt;PreviousValue&gt;C#&lt;/PreviousValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateEnd&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Scope&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateStart&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAvailableUntil&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAvailableFrom&quot; /&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateCopyrighted&quot; /&gt;
    ///&lt;/PropertyChanges&gt;
    /// </code>
    /// <para>For update operation, absence of ‘PreviousValue’ represents that the property value 
    /// was NULL before update and absence of ‘NextValue’ represents that the property value was 
    /// NULL after update. However, for BLOB properties (String and Binary with MaxLength = -1), 
    /// the ‘PreviousValue’ element may not be generated if the property is not changed. The 
    /// ‘Changed’ attribute on each ‘PropertyChange’ element is set to ‘True’ if the property has 
    /// undergone any change, otherwise the value of ‘Changed’ attribute is ‘False’. 
    /// </para>
    /// <code lang="xml">
    ///&lt;PropertyChanges ResourceId=&quot;DFF92785-6ADB-4B3A-90F0-A30D66E52B1A&quot; ResourceTypeFullName=&quot;Zentity.ScholarlyWorks.Lecture&quot;&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Title&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateModified&quot; Changed=&quot;True&quot;&gt;
    ///    &lt;PreviousValue&gt;Apr  6 2009 11:02AM&lt;/PreviousValue&gt;
    ///    &lt;NextValue&gt;Apr  6 2009 11:40AM&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Series&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;License&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateValidUntil&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Notes&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Image&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAdded&quot; Changed=&quot;False&quot;&gt;
    ///    &lt;PreviousValue&gt;Apr  6 2009 11:02AM&lt;/PreviousValue&gt;
    ///    &lt;NextValue&gt;Apr  6 2009 11:02AM&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Audience&quot; Changed=&quot;True&quot;&gt;
    ///    &lt;PreviousValue&gt;Basic level.&lt;/PreviousValue&gt;
    ///    &lt;NextValue&gt;new audience&lt;/NextValue&gt;
    ///  &lt;/PropertyChange&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Uri&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Abstract&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Venue&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Copyright&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Description&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateValidFrom&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Language&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateEnd&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;Scope&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateStart&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAvailableUntil&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateAvailableFrom&quot; Changed=&quot;False&quot;/&gt;
    ///  &lt;PropertyChange PropertyName=&quot;DateCopyrighted&quot; Changed=&quot;False&quot;/&gt;
    ///&lt;/PropertyChanges&gt;
    /// </code>
    /// </remarks>
    public partial class ResourceChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceChange"/> class.
        /// Making the constructor internal.
        /// </summary>
        internal ResourceChange()
        {
        }
    }
}
