// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Globalization;

namespace Zentity.Core
{
    /// <example>Example below shows how to perform basic Create, Read, Update and Delete (CRUD) operations on a resource.
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using System.Data.EntityClient;
    ///using System.Data;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        static Guid pubId = Guid.Empty;
    ///        static string connStr = @&quot;provider=System.Data.SqlClient;
    ///                metadata=res://Zentity.Core;
    ///                provider connection string='Data Source=.;
    ///                Initial Catalog=Zentity;Integrated Security=True;
    ///                MultipleActiveResultSets=True'&quot;;
    ///
    ///        public static void Main(string[] args)
    ///        {
    ///            CreateResource();
    ///            RetrieveUsingLinqToEntities();
    ///            UpdateResource();
    ///            RetrieveUsingEntitySQL();
    ///            DeleteResource();
    ///        }
    ///
    ///        private static void DeleteResource()
    ///        {
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Resource pub = context.Resources.Where(tuple =&gt; tuple.Id == pubId).FirstOrDefault();
    ///                context.DeleteObject(pub);
    ///                context.SaveChanges();
    ///
    ///                // Verify that the resource is deleted.
    ///                int i = context.Resources.Where(tuple =&gt; tuple.Id == pubId).Count();
    ///                Console.WriteLine(&quot;Count of resources with id [{0}] = [{1}]&quot;, pubId, i);
    ///            }
    ///        }
    ///
    ///        private static void RetrieveUsingEntitySQL()
    ///        {
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                EntityCommand cmd = (EntityCommand)context.Connection.CreateCommand();
    ///                cmd.CommandText = &quot;SELECT Pubs.Uri, Pubs.Title&quot; +
    ///                    &quot; FROM ZentityContext.Resources AS Pubs &quot; +
    ///                    &quot; WHERE Pubs.Id = @Id&quot;;
    ///                EntityParameter param = new EntityParameter(&quot;Id&quot;, DbType.Guid);
    ///                param.Direction = ParameterDirection.Input;
    ///                param.Value = pubId;
    ///                cmd.Parameters.Add(param);
    ///                if (context.Connection.State == System.Data.ConnectionState.Closed)
    ///                    context.Connection.Open();
    ///                EntityDataReader rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
    ///                rdr.Read();
    ///                Console.WriteLine(&quot;Resource details:&quot;);
    ///                Console.WriteLine(&quot;Uri: [{0}]&quot;, rdr.GetString(0));
    ///                Console.WriteLine(&quot;Title: [{0}]&quot;, rdr.GetString(1));
    ///            }
    ///        }
    ///
    ///        private static void UpdateResource()
    ///        {
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Resource pub = context.Resources.Where(tuple =&gt; tuple.Id == pubId).FirstOrDefault();
    ///                pub.Title = &quot;New Title&quot;;
    ///
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Updated title of the resource with id: [{0}]&quot;, pubId);
    ///            }
    ///        }
    ///
    ///        private static void RetrieveUsingLinqToEntities()
    ///        {
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                Resource pub = context.Resources.Where(tuple =&gt; tuple.Id == pubId).FirstOrDefault();
    ///                Console.WriteLine(&quot;Resource details:&quot;);
    ///                Console.WriteLine(&quot;Uri: [{0}]&quot;, pub.Uri);
    ///                Console.WriteLine(&quot;Title: [{0}]&quot;, pub.Title);
    ///            }
    ///        }
    ///
    ///        private static void CreateResource()
    ///        {
    ///            using (ZentityContext context = new ZentityContext(connStr))
    ///            {
    ///                // Create a new resource.     
    ///                Resource pub = new Resource();
    ///                pub.Title = &quot;Some paper on global warming.&quot;;
    ///                pub.Uri = &quot;urn:zentity-samples:publications:global-warming&quot;;
    ///
    ///                // Add the resource to context.     
    ///                context.AddToResources(pub);
    ///
    ///                // Save the context.     
    ///                context.SaveChanges();
    ///                pubId = pub.Id;
    ///                Console.WriteLine(&quot;Created Resource with id: [{0}]&quot;, pubId);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class Resource
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of Resource class with no arguements.
        /// </summary>
        public Resource()
        {
            // Automatically create a new id.
            this._Id = Guid.NewGuid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when title changing.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnTitleChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.Title)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, 
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Title, 
                    MaxLengths.Title));
        }

        /// <summary>
        /// Called when URI changing.
        /// </summary>
        /// <param name="value">The value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        partial void OnUriChanging(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > MaxLengths.Uri)
                throw new InvalidPropertyValueException(string.Format(CultureInfo.InvariantCulture, 
                    CoreResources.ValidationExceptionInvalidLength, CoreResources.Uri, 
                    MaxLengths.Uri));
        }

        #endregion
    }
}
