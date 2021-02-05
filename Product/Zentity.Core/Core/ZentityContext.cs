// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using System;
using System.Security.Permissions;
using System.IO;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Configuration;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Data.Objects.DataClasses;
using System.Collections;

namespace Zentity.Core
{
    /// <example>Example below shows a simple usage of ZentityContext. For the sample to work, add 
    /// an application configuration file with ConnectionString similar to the following.
    /// <code language="XML">
    ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
    ///&lt;configuration&gt;
    ///  &lt;connectionStrings&gt;
    ///    &lt;add name=&quot;ZentityContext&quot;
    ///         connectionString=&quot;provider=System.Data.SqlClient;
    ///         metadata=res://Zentity.Core;
    ///         provider connection string='Data Source=.;
    ///         Initial Catalog=Zentity;Integrated Security=True;
    ///         MultipleActiveResultSets=True'&quot;
    ///         providerName=&quot;System.Data.EntityClient&quot; /&gt;
    ///  &lt;/connectionStrings&gt;
    ///&lt;/configuration&gt;
    /// </code>
    /// <br/>
    /// <code>
    ///using Zentity.Core;
    ///using System;
    ///using System.Linq;
    ///using System.Data.EntityClient;
    ///
    ///namespace ZentitySamples
    ///{
    ///    public class Program
    ///    {
    ///        public static void Main(string[] args)
    ///        {
    ///            // Creating a context from the default connection string provided in the application configuration. 
    ///            using (ZentityContext context = new ZentityContext())
    ///            {
    ///                // Create a Resource. 
    ///                Resource v = new Resource { Uri = &quot;urn:zentity-samples:video:aVideo&quot; };
    ///                context.AddToResources(v);
    ///                context.SaveChanges();
    ///                Console.WriteLine(&quot;Created Resource resource with Uri: [{0}]&quot;, v.Uri);
    ///            }
    ///
    ///            // Creating the context using a connection string.
    ///            string connectionString = &quot;metadata=res://Zentity.Core;&quot; +
    ///                &quot;provider=System.Data.SqlClient;&quot; +
    ///                &quot;provider connection string='Data Source=.;&quot; +
    ///                &quot;Initial Catalog=Zentity;&quot; +
    ///                &quot;Integrated Security=True;&quot; +
    ///                &quot;Pooling=False;&quot; +
    ///                &quot;MultipleActiveResultSets=True'&quot;;
    ///            EntityConnection conn = new EntityConnection(connectionString);
    ///            using (ZentityContext context = new ZentityContext(conn))
    ///            {
    ///                // Retrieve all Resources 
    ///                Console.WriteLine(&quot;All Resources in the repository:&quot;);
    ///                foreach (Resource v in context.Resources)
    ///                    Console.WriteLine(&quot;\t\t&quot; + v.Uri);
    ///            }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public partial class ZentityContext
    {
        #region Fields

        DataModel dataModel;
        SavingChangesHandler savingChangesHandler;
        SqlConnection validationConnection;
        object validationConnectionLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data model of this context.
        /// </summary>
        public DataModel DataModel
        {
            get
            {
                if (dataModel == null)
                    dataModel = new DataModel(this);
                return dataModel;
            }
        }

        /// <summary>
        /// Represents a query against the store for all File objects.
        /// </summary>
        public ObjectQuery<File> Files
        {
            get { return this.Resources.OfType<File>(); }
        }

        ///// <summary>
        ///// Represents a query against the store for all File objects.
        ///// </summary>
        //public ObjectQuery<Core.File> Files
        //{
        //    get { return this.Resources.OfType<Core.File>(); }
        //}

        /// <summary>
        /// Gets a connection string to the backend SQL Server 2008 database.
        /// </summary>
        public string StoreConnectionString
        {
            get
            {
                // Compute the store connection string from context connection string.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder(this.Connection.ConnectionString);

                if (string.IsNullOrEmpty(entityBuilder.ProviderConnectionString))
                {
                    string entityConnectionString =
                        ConfigurationManager.ConnectionStrings[entityBuilder.Name].ConnectionString;
                    entityBuilder = new EntityConnectionStringBuilder(entityConnectionString);
                }

                return entityBuilder.ProviderConnectionString;
            }
        }

        /// <summary>
        /// Gets the operation timeout.
        /// </summary>
        /// <value>The operation timeout.</value>
        internal int OperationTimeout
        {
            get
            {
                return this.CommandTimeout.HasValue ? (int)this.CommandTimeout : 120;
            }
        }

        #endregion

        #region Constructors

        // NOTE: We are not invoking this.ZentityContext() or any other autogenerated constructors 
        // here. this.ZentityContext() calls OnContextCreated in the end. Invoking it instead of 
        // base constructor would cause the invocation of OnContextCreated before loading of the 
        // assemblies which is not the correct semantics of OnContextCreated. It should be the last
        // operation done by the constructor.

        /// <summary>
        /// Initializes a new ZentityContext object using the connection string found in the 
        /// 'ZentityContext' section of the application configuration file followed by loading
        /// additional .NET type information from the input assemblies.
        /// </summary>
        /// <param name="assembliesToLoadFrom">A list of assemblies to load the additional 
        /// .NET type information after context creation.</param>
        /// <remarks>Invoking this constructor has the same effect as invoking the parameterless
        /// constructor and then calling System.Data.Metadata.Edm.MetadataWorkspace.LoadFromAssembly
        /// passing each of the input assemblies as parameter.
        /// </remarks>
        public ZentityContext(params Assembly[] assembliesToLoadFrom)
            : base("name=ZentityContext", "ZentityContext")
        {
            foreach (Assembly assembly in assembliesToLoadFrom)
                this.MetadataWorkspace.LoadFromAssembly(assembly);

            this.OnContextCreated();
        }

        /// <summary>
        /// Initialize a new ZentityContext object from the input connection string followed by 
        /// loading additional .NET type information from the input assemblies.
        /// </summary>
        /// <param name="connectionString">Connection string to initialize the context.</param>
        /// <param name="assembliesToLoadFrom">A list of assemblies to load the additional 
        /// .NET type information after context creation.</param>
        /// <remarks>Invoking this constructor has the same effect as invoking ZentityContext(string)
        /// and then calling System.Data.Metadata.Edm.MetadataWorkspace.LoadFromAssembly passing 
        /// each of the input assemblies as parameter.
        /// </remarks>
        public ZentityContext(string connectionString, params Assembly[] assembliesToLoadFrom) :
            base(connectionString, "ZentityContext")
        {
            foreach (Assembly assembly in assembliesToLoadFrom)
                this.MetadataWorkspace.LoadFromAssembly(assembly);

            this.OnContextCreated();
        }

        /// <summary>
        /// Initialize a new ZentityContext object from the input entity connection followed by 
        /// loading additional .NET type information from the input assemblies.
        /// </summary>
        /// <param name="connection"><see cref="System.Data.EntityClient.EntityConnection"/> 
        /// to initialize the context.</param>
        /// <param name="assembliesToLoadFrom">A list of assemblies to load the additional 
        /// .NET type information after context creation.</param>
        /// <remarks>Invoking this constructor has the same effect as invoking 
        /// ZentityContext(System.Data.EntityClient.EntityConnection)
        /// and then calling System.Data.Metadata.Edm.MetadataWorkspace.LoadFromAssembly passing 
        /// each of the input assemblies as parameter.
        /// </remarks>
        public ZentityContext(global::System.Data.EntityClient.EntityConnection connection, params Assembly[] assembliesToLoadFrom) :
            base(connection, "ZentityContext")
        {
            foreach (Assembly assembly in assembliesToLoadFrom)
                this.MetadataWorkspace.LoadFromAssembly(assembly);

            this.OnContextCreated();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an instance of a File resource in the repository and uploads binary content 
        /// from the input path. File creation and upload operations are performed within a 
        /// <see cref="System.Transactions.TransactionScope"/> whose timeout is controlled by
        /// application configuration and machine configuration files.
        /// </summary>
        /// <typeparam name="T">A type that derives from Core.File.</typeparam>
        /// <param name="inputFilePath">Input file path.</param>
        /// <returns>An instance of type T</returns>
        /// <example>
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///using System.Linq;
        ///using System.IO;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            Guid dataFileGuid = Guid.Empty;
        ///            string filePath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        ///
        ///            // Create a sample file. 
        ///            string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
        ///            using (FileStream fin = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write, 1))
        ///            {
        ///                using (StreamWriter writer = new StreamWriter(fin))
        ///                {
        ///                    writer.Write(content);
        ///                }
        ///            }
        ///
        ///            // Create datafile. 
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File df = context.CreateFile&lt;Zentity.Core.File&gt;(filePath);
        ///                dataFileGuid = df.Id;
        ///                Console.WriteLine(&quot;Created file with content: [{0}]&quot;, content);
        ///            }
        ///
        ///            // Download content of a datafile. 
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File df = context.Files.Where(tuple =&gt; tuple.Id == dataFileGuid).First();
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    context.DownloadFileContent(df, stream);
        ///                    StreamReader rdr = new StreamReader(stream);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    Console.WriteLine(&quot;Downloaded file. Content: [{0}]&quot;, rdr.ReadToEnd());
        ///                }
        ///            }
        ///
        ///            // Cleanup. 
        ///            System.IO.File.Delete(filePath);
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter"), SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public T CreateFile<T>(string inputFilePath) where T : Core.File, new()
        {
            // Create the file and upload content in a single transaction.
            using (TransactionScope scope = new TransactionScope())
            {
                // Create File resource.
                T df = new T();

                // Create a new context. Do not add the newly created file to 'this' context. Else,
                // we might also accidentally save other changes while doing a SaveChanges below.
                using (ZentityContext context = new ZentityContext((EntityConnection)this.Connection))
                {
                    context.AddToResources(df);
                    context.SaveChanges();
                    context.UploadFileContent(df, inputFilePath);
                    context.Detach(df);
                }
                scope.Complete();
                this.Attach(df);
                return df;
            }
        }

        /// <summary>
        /// Creates an instance of a File resource in the repository and uploads binary content 
        /// from the input stream. File creation and upload operations are performed within a 
        /// <see cref="System.Transactions.TransactionScope"/> whose timeout is controlled by
        /// application configuration and machine configuration files.
        /// </summary>
        /// <typeparam name="T">A type that derives from Core.File.</typeparam>
        /// <param name="inputStream">Input stream.</param>
        /// <returns>An instance of type T</returns>
        /// <example>
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///using System.Linq;
        ///using System.IO;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            Guid dataFileGuid = Guid.Empty;
        ///
        ///            // Create the datafile. 
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    StreamWriter writer = new StreamWriter(stream);
        ///                    writer.AutoFlush = true;
        ///                    string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
        ///                    writer.Write(content);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    Zentity.Core.File df = context.CreateFile&lt;Zentity.Core.File&gt;(stream);
        ///                    dataFileGuid = df.Id;
        ///                    Console.WriteLine(&quot;Created File with content: [{0}]&quot;, content);
        ///                }
        ///            }
        ///
        ///            // Download content of a datafile. 
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File df = context.Files.Where(tuple =&gt; tuple.Id == dataFileGuid).First();
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    context.DownloadFileContent(df, stream);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    StreamReader rdr = new StreamReader(stream);
        ///                    Console.WriteLine(&quot;Downloaded file. Content is: [{0}]&quot;, rdr.ReadToEnd());
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter"), SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public T CreateFile<T>(Stream inputStream) where T : Core.File, new()
        {
            // Create the file and upload content in a single transaction.
            using (TransactionScope scope = new TransactionScope())
            {
                // Create File resource.
                T df = new T();

                // Create a new context. Do not add the newly created file to 'this' context. Else,
                // we might also accidentally save other changes while doing a SaveChanges below.
                using (ZentityContext context = new ZentityContext((EntityConnection)this.Connection))
                {
                    context.AddToResources(df);
                    context.SaveChanges();
                    context.UploadFileContent(df, inputStream);
                    // Detach from new context and attach to this.
                    context.Detach(df);
                }
                scope.Complete();
                this.Attach(df);
                return df;
            }
        }

        /// <summary>
        /// Uploads binary content for a <see cref="Zentity.Core.File"/> object.
        /// </summary>
        /// <param name="datafile">Target zentity file.</param>
        /// <param name="inputStream">Input stream to fetch content from.</param>
        /// <remarks>
        /// This method is used both for creating and updating the binary content of a 
        /// <see cref="Zentity.Core.File"/> object. Content can be uploaded only to an existing 
        /// file in the repository.This version of Zentity supports uploading content to only SQL 
        /// 2K8. The content is uploaded to a FILESTREAM column. SQL Server Native Client must be 
        /// installed to use this method. All FILESTREAM operations require a transaction to be 
        /// present, so this method also creates a new transaction with READCOMMITTED isolation
        /// level. The method opens a new connection for each upload instead of sharing the 
        /// context connection.
        /// <br/>
        /// The example below shows how to do a simple upload.
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///using System.Linq;
        ///using System.IO;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            Guid fileId = Guid.Empty;
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                // Create a Zentity file.    
        ///                Zentity.Core.File file = new Zentity.Core.File();
        ///                file.Uri = &quot;urn:zentity-samples:file:lecture-mp3&quot;;
        ///
        ///                // Add the file to context.    
        ///                context.AddToResources(file);
        ///
        ///                // Save off the id.
        ///                fileId = file.Id;
        ///
        ///                // Save the context.    
        ///                context.SaveChanges();
        ///
        ///                // Now upload the actual binary content of the file.    
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    StreamWriter writer = new StreamWriter(stream);
        ///                    writer.AutoFlush = true;
        ///                    string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
        ///                    writer.Write(content);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    context.UploadFileContent(file, stream);
        ///                    Console.WriteLine(&quot;Uploaded content: [{0}]&quot;, content);
        ///                }
        ///            }
        ///            // Fetch the file details from the repository.  
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    context.DownloadFileContent((Zentity.Core.File)file, stream);
        ///                    StreamReader reader = new StreamReader(stream);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    Console.WriteLine(&quot;Downloaded content: [{0}]&quot;, reader.ReadToEnd());
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void UploadFileContent(Core.File datafile, Stream inputStream)
        {
            // Uploads always happen on a new sql connection. We don't share connections, neither
            // do we use context EntityConnection. Using EntityConnection also required us to take 
            // care of contentions in multi-threaded scenarios. For example, two threads using 
            // same Context trying to invoke this method. Both get the same connection. Now 
            // Thread1 closes the connection (since it opened it) while Thread2 is still using it. 
            // This will cause Thread2 to raise errors.
            // Sharing of SqlConnections is not allowed because we cannot open multiple 
            // transactions on the same connection.
            using (SqlConnection conn = new SqlConnection(this.StoreConnectionString))
            {
                conn.Open();

                if (datafile == null)
                    throw new ArgumentNullException("datafile");
                if (!DataFileExists(conn, this.OperationTimeout, datafile.Id))
                    throw new ArgumentException(CoreResources.FileNotFoundForUpload, "datafile");
                if (inputStream == null)
                    throw new ArgumentNullException("inputStream");
                if (!inputStream.CanRead)
                    throw new ArgumentException(CoreResources.InvalidInputStream, "inputStream");

                UploadToSql2k8(conn, this.OperationTimeout, datafile.Id, inputStream);
            }
        }

        /// <summary>
        /// Uploads the content of a file.
        /// </summary>
        /// <param name="datafile">Target zentity file.</param>
        /// <param name="inputFilePath">Input file path to fetch content from.</param>
        /// <example>
        /// Example below shows how to do a simple upload.
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///using System.Linq;
        ///using System.IO;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            Guid fileId = Guid.Empty;
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                // Create a Zentity file.    
        ///                Zentity.Core.File file = new Zentity.Core.File();
        ///                file.Uri = &quot;urn:zentity-samples:file:lecture-mp3&quot;;
        ///
        ///                // Add the file to context.    
        ///                context.AddToResources(file);
        ///
        ///                // Save off the id.
        ///                fileId = file.Id;
        ///
        ///                // Save the context.    
        ///                context.SaveChanges();
        ///
        ///                // Create a sample file to upload. 
        ///                string filePath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        ///                string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
        ///                using (FileStream fin = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write, 1))
        ///                {
        ///                    using (StreamWriter writer = new StreamWriter(fin))
        ///                    {
        ///                        writer.Write(content);
        ///                        writer.Close();
        ///                    }
        ///                    fin.Close();
        ///                }
        ///
        ///                context.UploadFileContent(file, filePath);
        ///                Console.WriteLine(&quot;Uploaded content: [{0}]&quot;, content);
        ///
        ///                // Cleanup. 
        ///                System.IO.File.Delete(filePath);
        ///            }
        ///
        ///            // Fetch the file details from the repository.  
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
        ///                string filePath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        ///                context.DownloadFileContent(file, filePath, true);
        ///                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        ///                {
        ///                    using (StreamReader rdr = new StreamReader(fs))
        ///                    {
        ///                        Console.WriteLine(&quot;Downloaded content: [{0}]&quot;, rdr.ReadToEnd());
        ///                        rdr.Close();
        ///                    }
        ///                    fs.Close();
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void UploadFileContent(Core.File datafile, string inputFilePath)
        {
            // Again, we don't share EntityConnection or SqlConnection here. See other overload
            // for reasons.
            using (SqlConnection conn = new SqlConnection(this.StoreConnectionString))
            {
                conn.Open();
                if (datafile == null)
                    throw new ArgumentNullException("datafile");
                if (!DataFileExists(conn, this.OperationTimeout, datafile.Id))
                    throw new ArgumentException(CoreResources.FileNotFoundForUpload, "datafile");
                if (string.IsNullOrEmpty(inputFilePath))
                    throw new ArgumentException(CoreResources.InvalidInputPathNullOrEmtpy,
                        "inputFilePath");
                if (!System.IO.File.Exists(inputFilePath))
                    throw new ArgumentException(CoreResources.InvalidInputPathNonExisting,
                        "inputFilePath");

                // Open the file with buffersize = 1. This will, in effect, disable any .NET 
                // buffering. Also, enable the Sequential hint.
                using (FileStream fin = new FileStream(inputFilePath, FileMode.Open,
                    FileAccess.Read, FileShare.Read, 1, FileOptions.SequentialScan))
                {
                    UploadToSql2k8(conn, this.OperationTimeout, datafile.Id, fin);
                }
            }
        }

        /// <summary>
        /// Downloads the content of a file to an output stream.
        /// </summary>
        /// <param name="datafile">Source zentity file.</param>
        /// <param name="outputStream">Target output stream.</param>
        /// <example>
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///using System.Linq;
        ///using System.IO;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            Guid fileId = Guid.Empty;
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                // Create a Zentity file.    
        ///                Zentity.Core.File file = new Zentity.Core.File();
        ///                file.Uri = &quot;urn:zentity-samples:file:lecture-mp3&quot;;
        ///
        ///                // Add the file to context.    
        ///                context.AddToResources(file);
        ///
        ///                // Save off the id.
        ///                fileId = file.Id;
        ///
        ///                // Save the context.    
        ///                context.SaveChanges();
        ///
        ///                // Now upload the actual binary content of the file.    
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    StreamWriter writer = new StreamWriter(stream);
        ///                    writer.AutoFlush = true;
        ///                    string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
        ///                    writer.Write(content);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    context.UploadFileContent(file, stream);
        ///                    Console.WriteLine(&quot;Uploaded content: [{0}]&quot;, content);
        ///                }
        ///            }
        ///            // Fetch the file details from the repository.  
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    context.DownloadFileContent((Zentity.Core.File)file, stream);
        ///                    StreamReader reader = new StreamReader(stream);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    Console.WriteLine(&quot;Downloaded content: [{0}]&quot;, reader.ReadToEnd());
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void DownloadFileContent(Core.File datafile, Stream outputStream)
        {
            using (SqlConnection conn = new SqlConnection(this.StoreConnectionString))
            {
                conn.Open();

                if (datafile == null)
                    throw new ArgumentNullException("datafile");
                if (!DataFileExists(conn, this.OperationTimeout, datafile.Id))
                    throw new ArgumentException(CoreResources.FileNotFoundForDownload, "datafile");
                if (outputStream == null)
                    throw new ArgumentNullException("outputStream");
                if (!outputStream.CanWrite)
                    throw new ArgumentException(CoreResources.InvalidOutputStream, "outputStream");

                DownloadFromSql2k8(conn, this.OperationTimeout, datafile.Id, outputStream);
            }
        }

        /// <summary>
        /// Downloads the content of a file to a target location.
        /// </summary>
        /// <param name="datafile">Source zentity file.</param>
        /// <param name="outputFilePath">Target output file path.</param>
        /// <param name="overWrite">If true, overwrites the target file if it exists. If false, raises exception if the target file exists.</param>
        /// <example>
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///using System.Linq;
        ///using System.IO;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            Guid fileId = Guid.Empty;
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                // Create a zentity file.    
        ///                Zentity.Core.File file = new Zentity.Core.File();
        ///                file.Uri = &quot;urn:zentity-samples:file:lecture-mp3&quot;;
        ///
        ///                // Add the file to context.    
        ///                context.AddToResources(file);
        ///
        ///                // Save off the id.
        ///                fileId = file.Id;
        ///
        ///                // Save the context.    
        ///                context.SaveChanges();
        ///
        ///                // Create a sample file to upload. 
        ///                string filePath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        ///                string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
        ///                using (FileStream fin = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write, 1))
        ///                {
        ///                    using (StreamWriter writer = new StreamWriter(fin))
        ///                    {
        ///                        writer.Write(content);
        ///                        writer.Close();
        ///                    }
        ///                    fin.Close();
        ///                }
        ///
        ///                context.UploadFileContent(file, filePath);
        ///                Console.WriteLine(&quot;Uploaded content: [{0}]&quot;, content);
        ///
        ///                // Cleanup. 
        ///                System.IO.File.Delete(filePath);
        ///            }
        ///
        ///            // Fetch the file details from the repository.  
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
        ///                string filePath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());
        ///                context.DownloadFileContent(file, filePath, true);
        ///                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        ///                {
        ///                    using (StreamReader rdr = new StreamReader(fs))
        ///                    {
        ///                        Console.WriteLine(&quot;Downloaded content: [{0}]&quot;, rdr.ReadToEnd());
        ///                        rdr.Close();
        ///                    }
        ///                    fs.Close();
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void DownloadFileContent(Core.File datafile, string outputFilePath, bool overWrite)
        {
            using (SqlConnection conn = new SqlConnection(this.StoreConnectionString))
            {
                conn.Open();
                if (datafile == null)
                    throw new ArgumentNullException("datafile");
                if (!DataFileExists(conn, this.OperationTimeout, datafile.Id))
                    throw new ArgumentException(CoreResources.FileNotFoundForDownload, "datafile");
                if (string.IsNullOrEmpty(outputFilePath))
                    throw new ArgumentException(CoreResources.InvalidOutputPathNullOrEmpty,
                        "outputFilePath");

                using (FileStream outputStream = new FileStream(
                    outputFilePath, overWrite ? FileMode.Create : FileMode.CreateNew,
                    FileAccess.Write, FileShare.Write, 1, FileOptions.SequentialScan))
                {
                    DownloadFromSql2k8(conn, this.OperationTimeout, datafile.Id, outputStream);
                }
            }
        }

        /// <summary>
        /// Gets a stream for reading the content of a data file.
        /// </summary>
        /// <param name="datafile">The input data file to read.</param>
        /// <returns>A stream to read the content from. If no content is present, returns null.</returns>
        /// <example>
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///using System.Linq;
        ///using System.IO;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            Guid fileId = Guid.Empty;
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                // Create a Zentity file.    
        ///                Zentity.Core.File file = new Zentity.Core.File();
        ///                file.Uri = &quot;urn:zentity-samples:file:lecture-mp3&quot;;
        ///
        ///                // Add the file to context.    
        ///                context.AddToResources(file);
        ///                fileId = file.Id;
        ///                context.SaveChanges();
        ///
        ///                // Now upload the actual binary content of the file.    
        ///                using (MemoryStream stream = new MemoryStream())
        ///                {
        ///                    StreamWriter writer = new StreamWriter(stream);
        ///                    writer.AutoFlush = true;
        ///                    string content = &quot;Content-&quot; + Guid.NewGuid().ToString();
        ///                    writer.Write(content);
        ///                    stream.Seek(0, SeekOrigin.Begin);
        ///                    context.UploadFileContent(file, stream);
        ///                    Console.WriteLine(&quot;Uploaded content: [{0}]&quot;, content);
        ///                }
        ///            }
        ///
        ///            // Fetch the file details from the repository.  
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Zentity.Core.File file = context.Files.Where(tuple =&gt; tuple.Id == fileId).FirstOrDefault();
        ///                using (Stream contentStream = context.GetContentStream(file))
        ///                {
        ///                    StreamReader reader = new StreamReader(contentStream);
        ///                    Console.WriteLine(&quot;Downloaded content: [{0}]&quot;, reader.ReadToEnd());
        ///                    contentStream.Close();
        ///                }
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public Stream GetContentStream(Core.File datafile)
        {
            if (datafile == null)
                throw new ArgumentNullException("datafile");

            return GetSql2k8ContentStream(this.StoreConnectionString, this.OperationTimeout,
                datafile.Id);
        }

        /// <summary>
        /// Gets the value of a configuration parameter for zentity.
        /// </summary>
        /// <param name="configName">Name of the configuration parameter.</param>
        /// <returns>The configuration value if found, NULL otherwise.</returns>
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        ///using Zentity.Core;
        ///using System;
        ///
        ///namespace ZentitySamples
        ///{
        ///    public class Program
        ///    {
        ///        public static void Main(string[] args)
        ///        {
        ///            using (ZentityContext context = new ZentityContext())
        ///            {
        ///                Console.WriteLine(&quot;ZentityVersion = [{0}]&quot;, context.GetConfiguration(&quot;ZentityVersion&quot;));
        ///            }
        ///        }
        ///    }
        ///}
        /// </code>
        /// </example>
        public string GetConfiguration(string configName)
        {
            if (string.IsNullOrEmpty(configName))
                throw new ArgumentException(CoreResources.InvalidConfigNameNullOrEmpty, "configName");

            SqlParameter paramConfigName = new SqlParameter
            {
                DbType = DbType.String,
                ParameterName = CoreResources.ConfigName,
                Size = 256,
                Value = configName
            };

            SqlParameter paramConfigValue = new SqlParameter
            {
                DbType = DbType.String,
                Direction = ParameterDirection.Output,
                ParameterName = CoreResources.ConfigValue,
                Size = 4000,
                Value = configName
            };

            // Don't use EntityConnection. We will have to take care of contentions in 
            // multi-threaded scenarios. For example, two threads using same Context trying
            // to invoke this method. Both get the same connection. Now Thread1 closes the
            // connection (since it opened it) while Thread2 is still using it. This will
            // cause Thread2 to raise errors.
            using (SqlConnection conn = new SqlConnection(this.StoreConnectionString))
            {
                conn.Open();
                Utilities.ExecuteNonQuery(conn, null, this.OperationTimeout,
                    CoreResources.Core_GetConfigurationValue, CommandType.StoredProcedure,
                    paramConfigName, paramConfigValue);
            }

            return paramConfigValue.Value == DBNull.Value ? null :
                paramConfigValue.Value.ToString();
        }

        /// <summary>
        /// Updates a resource in the context with data from the data source, clears all its 
        /// related ends and then reloads the related ends again.
        /// </summary>
        /// <param name="resource">The resource to be refreshed.</param>
        /// <remarks>
        /// Entity Framework provided 'Refresh' methods do not drop the relationship entries that 
        /// were removed from the store out-of-band. For example, in the snippet below deleting a
        /// Core.Relationship object also causes the deletion of 'ResourceHasFile' relationship
        /// between the resource and file from the store. But we cannot refresh that information 
        /// using Entity Framework provided 'Refresh' methods.
        /// <code title="Refresh Problem">
        ///using (ZentityContext context = new ZentityContext())
        ///{
        ///    Resource r1 = new Resource { Title = &quot;resource&quot; };
        ///    File f = new File { Title = &quot;file&quot; };
        ///    r1.Files.Add(f);
        ///    context.AddToResources(r1);
        ///    context.SaveChanges();  // Creating a relationship between r1 and f also creates a 
        ///                            // Core.Relationship in the backend but that has to be 
        ///                            // explicitly loaded in the context.
        ///    Debug.Assert(r1.Files.Count == 1);
        ///    Debug.Assert(r1.RelationshipsAsSubject.Count == 0);
        ///
        ///    // Load the created Core.Relationship objects.
        ///    r1.RelationshipsAsSubject.Load();
        ///    Debug.Assert(r1.Files.Count == 1);
        ///    Debug.Assert(r1.RelationshipsAsSubject.Count == 1);
        ///
        ///    // Remove Core.Relationship object.
        ///    context.DeleteObject(r1.RelationshipsAsSubject.First());
        ///    context.SaveChanges();  // Deleting the Core.Relationship object also removes the 
        ///                            // 'ResourceHasFile' relationship between r1 and f in the
        ///                            // backend but that information has to be explitly loaded
        ///                            // in the context.
        ///    Debug.Assert(r1.Files.Count == 1);
        ///    Debug.Assert(r1.RelationshipsAsSubject.Count == 0);
        ///
        ///    // Reloading store information using Entity Framework provided 'Refresh' methods
        ///    // does not work.
        ///    context.Refresh(RefreshMode.StoreWins, r1);
        ///    Debug.Assert(r1.Files.Count == 1); // The file count should have been zero here.
        ///}
        /// </code>
        /// To resolve the above issue, we have provided this overload of Refresh method that first
        /// invokes Refresh(RefreshMode.StoreWins, resource) on the input resource to refresh it.
        /// It then clears all its related ends. Finally, this method invokes 
        /// Load(MergeOption.OverwriteChanges) on each of the related ends. The snippet below shows 
        /// its usage.
        /// <code title="Refresh Solution">
        ///using (ZentityContext context = new ZentityContext())
        ///{
        ///    Resource r1 = new Resource { Title = &quot;resource&quot; };
        ///    File f = new File { Title = &quot;file&quot; };
        ///    r1.Files.Add(f);
        ///    context.AddToResources(r1);
        ///    context.SaveChanges();
        ///
        ///    Debug.Assert(r1.Files.Count == 1);
        ///    Debug.Assert(r1.RelationshipsAsSubject.Count == 0);
        ///
        ///    r1.RelationshipsAsSubject.Load();
        ///    Debug.Assert(r1.Files.Count == 1);
        ///    Debug.Assert(r1.RelationshipsAsSubject.Count == 1);
        ///
        ///    context.DeleteObject(r1.RelationshipsAsSubject.First());
        ///    context.SaveChanges();
        ///
        ///    Debug.Assert(r1.Files.Count == 1);
        ///    Debug.Assert(r1.RelationshipsAsSubject.Count == 0);
        ///
        ///    // Reloading store information using this method.
        ///    context.Refresh(r1, false);
        ///
        ///    Debug.Assert(r1.Files.Count == 0); // The file count is zero as it should be.
        ///
        ///    // Add an explicit Core.Relationship object.
        ///    Relationship rel = new Relationship
        ///    {
        ///        Subject = r1,
        ///        Object = f,
        ///        Predicate = context.Predicates.
        ///        Where(tuple =&gt; tuple.Name == &quot;ResourceHasFile&quot;).First()
        ///    };
        ///    context.AddToRelationships(rel);
        ///    context.SaveChanges();  // This should also create a 'ResourceHasFile' entity 
        ///                            // framework relationship in the store. But, we have to
        ///                            // load that information explicitly into the context.
        ///    Debug.Assert(r1.Files.Count == 0);
        ///
        ///    // Load the related files.
        ///    context.Refresh(r1, true);  // We could also have used r1.Files.Load();
        ///    Debug.Assert(r1.Files.Count == 1);
        ///}
        /// </code>
        /// </remarks>
        public void Refresh(Resource resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            // First step is to refresh resource using the EF provided methods.
            // This removes the resource from object graph if it is deleted from store,
            // or raises appropriate exceptions if it in added state etc.
            this.Refresh(RefreshMode.StoreWins, resource);

            // No need to proceed further if the resource is removed from object graph 
            // after previous Refresh.
            if (resource.EntityKey == null)
                return;

            // Next, clear all the related ends.
            RelationshipManager mgr = ((IEntityWithRelationships)resource).RelationshipManager;
            var relatedEnds = mgr.GetAllRelatedEnds().ToList();
            for (int i = 0; i < relatedEnds.Count; i++)
            {
                IRelatedEnd relatedEnd = relatedEnds[i];
                // XXXToMany associations. RelatedEnd is EntityCollection<T>.
                if (relatedEnd is IEnumerable)
                {
                    MethodInfo mi = relatedEnd.GetType().GetMethod("Clear");
                    mi.Invoke(relatedEnd, null);
                }
                // XXXToOne or XXXToZeroOrOne associations. RelatedEnd is EntityReference<T>.
                else
                {
                    PropertyInfo pi = relatedEnd.GetType().GetProperty("Value");
                    pi.SetValue(relatedEnd, null, null);
                }
            }
            // Accept changes.
            foreach (ObjectStateEntry entry in this.ObjectStateManager.
                GetObjectStateEntries(EntityState.Deleted).
                Where(tuple => tuple.IsRelationship &&
                    ((EntityKey)tuple.OriginalValues[0] == resource.EntityKey ||
                    (EntityKey)tuple.OriginalValues[1] == resource.EntityKey)))
                entry.AcceptChanges();

            // Finally, reload all the related ends.

            foreach (RelatedEnd end in relatedEnds)
                end.Load(MergeOption.OverwriteChanges);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Uploads a binary stream as FileStream in Sql Server 2008 and above.
        /// </summary>
        /// <param name="storeConnection">The store connection.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="resourceId">The resource id.</param>
        /// <param name="inputStream">The input stream.</param>
        private static void UploadToSql2k8(SqlConnection storeConnection, int commandTimeout, Guid resourceId, Stream inputStream)
        {
            // Do uploads in a transaction. All FILESTREAM operations require a transaction to be 
            // present. We were observing invalid handle exceptions after around 10 minutes while 
            // using TransactionScope. So, we replaced that code with an explicit BeginTransaction
            // here. Also, create a new connection for upload. We cannot share connections here 
            // since SqlConnection does not support parallel transactions.
            using (SqlTransaction transaction = storeConnection.BeginTransaction(
                System.Data.IsolationLevel.ReadCommitted))
            {
                // Update content.
                string sqlFilePath;
                byte[] transactionToken;

                Utilities.GetPathNameAndTransactionToken(storeConnection, transaction,
                    commandTimeout, resourceId, out sqlFilePath, out transactionToken);

                // Open up the sql stream to write to.
                using (SqlFileStream destBlob = new SqlFileStream(sqlFilePath, transactionToken,
                    FileAccess.Write, FileOptions.SequentialScan, 1))
                {
                    CopyStream(inputStream, destBlob);
                }

                // Commit the transaction.
                transaction.Commit();
            }
        }

        /// <summary>
        /// Downloads a binary stream from FileStream content in Sql Server 2008 and above.
        /// </summary>
        /// <param name="storeConnection">The store connection.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="resourceId">The resource id.</param>
        /// <param name="outputStream">The output stream.</param>
        private static void DownloadFromSql2k8(SqlConnection storeConnection, int commandTimeout, Guid resourceId, Stream outputStream)
        {
            using (SqlTransaction transaction = storeConnection.BeginTransaction())
            {
                // Get the content details.
                string sqlFilePath;
                byte[] transactionToken;

                Utilities.GetPathNameAndTransactionToken(storeConnection, transaction,
                    commandTimeout, resourceId, out sqlFilePath, out transactionToken);

                // Create a stream from handle and copy it to output stream.
                using (SqlFileStream inputStream = new SqlFileStream(sqlFilePath, transactionToken,
                    FileAccess.Read, FileOptions.SequentialScan, 0))
                {
                    CopyStream(inputStream, outputStream);
                }

                // Commit transaction.
                transaction.Commit();
            }
        }

        /// <summary>
        /// Gets the content stream from a Sql Server's FileStream content column.
        /// </summary>
        /// <param name="storeConnectionString">The store connection string.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="resourceId">The resource id.</param>
        /// <returns>The output content stream</returns>
        private static Stream GetSql2k8ContentStream(string storeConnectionString, int commandTimeout, Guid resourceId)
        {
            KatmaiContentStream cs = new KatmaiContentStream(storeConnectionString,
                commandTimeout, resourceId);
            if (cs.InnerStream == null || cs.InnerStream.Length == 0)
            {
                cs.Dispose();
                return null;
            }
            else
                return cs;
        }

        /// <summary>
        /// Copies the stream from source to destination stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="destinationStream">The destination stream.</param>
        private static void CopyStream(Stream sourceStream, Stream destinationStream)
        {
            // Set the end of destination stream.
            if (destinationStream.CanSeek && destinationStream.CanWrite)
                destinationStream.SetLength(sourceStream.Length);

            // A buffer of 60K.
            byte[] buffer = new byte[60 * 1024];
            int bytesRead;

            // Read one buffer, write one buffer.
            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                destinationStream.Write(buffer, 0, bytesRead);
        }

        /// <summary>
        /// Checks if Data file exists for a given entity.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="fileId">The file id.</param>
        /// <returns>true if the Data File exists in the database</returns>
        private static bool DataFileExists(SqlConnection conn, int commandTimeout, Guid fileId)
        {
            SqlParameter paramId = new SqlParameter
            {
                DbType = DbType.Guid,
                ParameterName = CoreResources.Id,
                Value = fileId
            };

            var result = Utilities.ExecuteScalar(conn, null, commandTimeout,
                CoreResources.CmdFileExists, CommandType.Text, paramId);

            if (result != null && Convert.ToInt32(result, CultureInfo.InvariantCulture) == 1)
                return true;

            return false;
        }

        /// <summary>
        /// Called when context instance is created.
        /// </summary>
        partial void OnContextCreated()
        {
            this.validationConnection = new SqlConnection(this.StoreConnectionString);
            this.CommandTimeout = this.Connection.ConnectionTimeout;

            // Set up the handler for SaveChanges.
            savingChangesHandler = new SavingChangesHandler(this);
            this.SavingChanges += savingChangesHandler.Handle;
        }

        #endregion
    }
}