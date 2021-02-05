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
    /// <example>This example shows a simple creation and updating of a Tag.
    /// It then shows how to retrieve the changes. Use AdministrationContext.EnableChangeHistory() 
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
    ///            Tag csharp = new Tag { Uri = &quot;zentitySamples:tag:c-sharp&quot;, Name = &quot;C#&quot;, Description = &quot;Multi-Threading&quot; };
    ///
    ///            // Save off the changes.   
    ///            context.AddToTags(csharp);
    ///            context.SaveChanges();
    ///
    ///            // Changeset entries are processed by a background 
    ///            // job that is invoked every 10 seconds.
    ///            // Wait for a while to let it complete.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///                // Pick the last changeset.
    ///                ChangeSet changeSet = adminContext.ChangeSets.
    ///                    Include(&quot;TagChanges.Operation&quot;).
    ///                    OrderByDescending(tuple =&gt; tuple.DateCreated).First();
    ///
    ///                if (changeSet != null)
    ///                {
    ///                    Console.WriteLine(&quot;Changeset [{0}] created on [{1}]&quot;,
    ///                        changeSet.Id, changeSet.DateCreated);
    ///
    ///                    foreach (TagChange tc in changeSet.TagChanges)
    ///                    {
    ///                        Console.WriteLine(&quot;Tag [{0}] underwent a change of type [{1}].&quot;,
    ///                            tc.TagId, tc.Operation.Name);
    ///                    }
    ///                }
    ///            }
    ///
    ///            // Update Tag properties.
    ///            csharp.Name = &quot;CSharp&quot;;
    ///            csharp.Description = &quot;Using C# implement 'Multi-Threading'&quot;;
    ///            csharp.Uri = &quot;zentity:tag:c-sharp&quot;;
    ///            context.SaveChanges();
    ///
    ///            // Sleep for a minute.
    ///            Thread.Sleep(new TimeSpan(0, 1, 0));
    ///
    ///            using (AdministrationContext adminContext = new AdministrationContext())
    ///            {
    ///                TagChange tagChanges = adminContext.TagChanges.
    ///                                        Where(tuple =&gt; tuple.TagId == csharp.Id).
    ///                                        OrderByDescending(tuple =&gt; tuple.ChangeSet.DateCreated).
    ///                                        FirstOrDefault();
    ///                if (tagChanges != null)
    ///                {
    ///                    tagChanges.ChangeSetReference.Load();
    ///                    tagChanges.OperationReference.Load();
    ///                    Console.WriteLine(&quot;ChangeSet          : {0}&quot;, tagChanges.ChangeSet.Id);
    ///                    Console.WriteLine(&quot;Operation          : {0}&quot;, tagChanges.Operation.Name);
    ///                    Console.WriteLine(&quot;TagId              : {0}&quot;, tagChanges.TagId);
    ///                    Console.WriteLine(&quot;PreviousName       : {0}&quot;, tagChanges.PreviousName);
    ///                    Console.WriteLine(&quot;NextName           : {0}&quot;, tagChanges.NextName);
    ///                    Console.WriteLine(&quot;PreviousDescription: {0}&quot;, tagChanges.PreviousDescription);
    ///                    Console.WriteLine(&quot;NextDescription    : {0}&quot;, tagChanges.NextDescription);
    ///                    Console.WriteLine(&quot;PreviousUri        : {0}&quot;, tagChanges.PreviousUri);
    ///                    Console.WriteLine(&quot;NextUri            : {0}&quot;, tagChanges.NextUri);
    ///                }
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class TagChange
    {
        // Making the constructor internal.
        internal TagChange()
        {
        }
    }
}
