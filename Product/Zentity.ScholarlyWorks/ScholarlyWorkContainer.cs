// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Globalization;

namespace Zentity.ScholarlyWorks
{
    /// <example>Example below shows a simple usage of the ScholarlyWorksContainer class.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using Zentity.ScholarlyWorks;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res://Zentity.ScholarlyWorks;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///
    ///            Guid grandParentId = Guid.Empty;
    ///
    ///            // Create a container hierarchy.
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                ScholarlyWorkContainer grandParent = new ScholarlyWorkContainer { Title = &quot;GrandParent&quot; };
    ///                ScholarlyWorkContainer parent = new ScholarlyWorkContainer { Title = &quot;Parent&quot; };
    ///                grandParent.ContainedWorks.Add(parent);
    ///                Lecture lecture = new Lecture { Title = &quot;Lecture&quot; };
    ///                parent.ContainedWorks.Add(lecture);
    ///                Journal journal = new Journal { Title = &quot;Journal&quot; };
    ///                parent.ContainedWorks.Add(journal);
    ///
    ///                context.AddToResources(grandParent);
    ///                context.SaveChanges();
    ///                grandParentId = grandParent.Id;
    ///            }
    ///
    ///            // Print the hierarchy.
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                context.MetadataWorkspace.LoadFromAssembly(typeof(ScholarlyWork).Assembly);
    ///                PrintSubTree(context, grandParentId, 0);
    ///            }
    ///        }
    ///
    ///        private static void PrintSubTree(ZentityContext context, Guid resourceId, int level)
    ///        {
    ///            Resource r = context.Resources.Where(tuple =&gt; tuple.Id == resourceId).First();
    ///            for (int i = 0; i &lt; level; i++)
    ///                Console.Write(&quot;\t&quot;);
    ///            Console.Write(&quot;Resource [{0}], Title [{1}]\n&quot;, r.Id, r.Title);
    ///            ScholarlyWorkContainer container = r as ScholarlyWorkContainer;
    ///            if (container != null)
    ///            {
    ///                container.ContainedWorks.Load();
    ///                foreach (ScholarlyWork sw in container.ContainedWorks)
    ///                    PrintSubTree(context, sw.Id, level + 1);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ScholarlyWorkContainer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnCountChanging(int? value)
        {
            if (value != null && value < 0)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, 
                    ScholarlyWorksResources.ValidationExceptionNegativeValue, 
                    ScholarlyWorksResources.Count));
        }
    }
}
