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
    /// <example>This example shows simple creation of TagToTagRelationship 
    /// and then how to retrieve the changes. Use AdministrationContext.EnableChangeHistory() 
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
    ///            ZentityContext context = new ZentityContext();
    ///
    ///            //Create a tag.
    ///            Tag csharp = new Tag { Uri = &quot;zentitySamples:tag:c-sharp&quot;, Name = &quot;C#&quot; };
    ///            Tag csharpMultiThreading = new Tag { Uri = &quot;zentitySamples:tag:c-sharp-multi-threading&quot;, Name = &quot;C#-Multi-Threading&quot; };
    ///
    ///            // Retrive contains Predicate
    ///            Predicate predicate = context.Predicates.First();
    ///
    ///
    ///            //Create TagToTag relationship.
    ///            TagToTagRelationship tagRelationship = new TagToTagRelationship();
    ///            tagRelationship.Predicate = predicate;
    ///            tagRelationship.SubjectTag = csharp;
    ///            tagRelationship.ObjectTag = csharpMultiThreading;
    ///
    ///            // Save off the changes.   
    ///            context.SaveChanges();
    ///
    ///            // Changeset entries are processed by a background 
    ///            // job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///
    ///                // Pick the last changeset.
    ///                ChangeSet changeSet = adminContext.ChangeSets.
    ///                    Include(&quot;TagToTagRelationshipChanges.Operation&quot;).
    ///                    OrderByDescending(tuple =&gt; tuple.DateCreated).First();
    ///
    ///                if (changeSet != null)
    ///                {
    ///                    Console.WriteLine(&quot;Changeset [{0}] created on [{1}]&quot;,
    ///                        changeSet.Id, changeSet.DateCreated);
    ///
    ///                    foreach (TagToTagRelationshipChange tc in changeSet.TagToTagRelationshipChanges)
    ///                    {
    ///                        Console.WriteLine(&quot;&quot;);
    ///                        tc.ChangeSetReference.Load();
    ///                        tc.OperationReference.Load();
    ///                        Console.WriteLine(&quot;ChangeSet              [{0}]&quot;, tc.ChangeSet.Id);
    ///                        Console.WriteLine(&quot;Operation              [{0}]&quot;, tc.Operation.Name);
    ///                        Console.WriteLine(&quot;TagToTagRelationshipId [{0}]&quot;, tc.TagToTagRelationshipId);
    ///                        Console.WriteLine(&quot;PreviousObjectTagId    [{0}]&quot;, tc.PreviousObjectTagId);
    ///                        Console.WriteLine(&quot;NextObjectTagId        [{0}]&quot;, tc.NextObjectTagId);
    ///                        Console.WriteLine(&quot;PreviousPredicateId    [{0}]&quot;, tc.PreviousPredicateId);
    ///                        Console.WriteLine(&quot;NextPredicateId        [{0}]&quot;, tc.NextPredicateId);
    ///                        Console.WriteLine(&quot;PreviousSubjectTagId   [{0}]&quot;, tc.PreviousSubjectTagId);
    ///                        Console.WriteLine(&quot;NextSubjectTagId       [{0}]&quot;, tc.NextSubjectTagId);
    ///
    ///                    }
    ///                }
    ///            }
    ///
    ///            // Update TagToTagRelationship properties.
    ///            predicate = context.Predicates.
    ///                        Where(tuple =&gt; tuple.Uri == &quot;urn:predicates:contains&quot;).
    ///                        First();
    ///            tagRelationship.Predicate = predicate;
    ///            tagRelationship.SubjectTag = csharpMultiThreading;
    ///            tagRelationship.ObjectTag = csharp;
    ///            context.SaveChanges();
    ///
    ///            // Sleep for a minute.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///
    ///            Console.WriteLine(&quot;\n----------- Update TagToTagRelationship -----------&quot;);
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///                TagToTagRelationshipChange tagRelationshipChange = adminContext.TagToTagRelationshipChanges.
    ///                                        Where(tuple =&gt; tuple.TagToTagRelationshipId == tagRelationship.Id).
    ///                                        OrderByDescending(tuple =&gt; tuple.ChangeSet.DateCreated).
    ///                                        FirstOrDefault();
    ///                if (tagRelationshipChange != null)
    ///                {
    ///                    tagRelationshipChange.ChangeSetReference.Load();
    ///                    tagRelationshipChange.OperationReference.Load();
    ///                    Console.WriteLine(&quot;ChangeSet              [{0}]&quot;, tagRelationshipChange.ChangeSet.Id);
    ///                    Console.WriteLine(&quot;Operation              [{0}]&quot;, tagRelationshipChange.Operation.Name);
    ///                    Console.WriteLine(&quot;TagToTagRelationshipId [{0}]&quot;, tagRelationshipChange.TagToTagRelationshipId);
    ///                    Console.WriteLine(&quot;PreviousObjectTagId    [{0}]&quot;, tagRelationshipChange.PreviousObjectTagId);
    ///                    Console.WriteLine(&quot;NextObjectTagId        [{0}]&quot;, tagRelationshipChange.NextObjectTagId);
    ///                    Console.WriteLine(&quot;PreviousPredicateId    [{0}]&quot;, tagRelationshipChange.PreviousPredicateId);
    ///                    Console.WriteLine(&quot;NextPredicateId        [{0}]&quot;, tagRelationshipChange.NextPredicateId);
    ///                    Console.WriteLine(&quot;PreviousSubjectTagId   [{0}]&quot;, tagRelationshipChange.PreviousSubjectTagId);
    ///                    Console.WriteLine(&quot;NextSubjectTagId       [{0}]&quot;, tagRelationshipChange.NextSubjectTagId);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class TagToTagRelationshipChange
    {
        // Making the constructor internal.
        internal TagToTagRelationshipChange()
        {
        }
    }
}
