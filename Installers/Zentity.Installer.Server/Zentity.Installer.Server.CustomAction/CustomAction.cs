// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using System.Collections.Generic;

namespace Zentity.Installer.CustomAction
{
    public class CustomActions
    {
        private const string GroupExistsMessage = "The specified local group already exists.";
        private static string caption = "Zentity Setup";
        private static WindowsImpersonationContext impersonationContext;

        [CustomAction]
        public static ActionResult Run(Session session)
        {
            session.Log("Begin Run");
            ActionResult result = ActionResult.Success;

            // Give a default value to the caption.
            caption = "Zentity Setup";

            try
            {
                // Determine which action to execute.
                string actionToExecute = session["CATOEXECUTE"];

                switch (actionToExecute)
                {
                    case "ValidateDbCreationPerm":
                        ValidateDbCreationPerm(session, caption);
                        break;
                    case "FetchDbDefaultFolderPath":
                        FetchDbDefaultFolderPath(session, caption);
                        break;
                    case "ValidateDBFilesFolderPath":
                        ValidateDBFilesFolderPath(session, caption);
                        break;
                    case "ValidateDBFileStream":
                        ValidateDBFileStream(session, caption);
                        break;
                    case "ValidateDBFileStreamFolderPath":
                        ValidateDBFileStreamFolderPath(session, caption);
                        break;
                    case "ValidateAppPoolCredentials":
                        ValidateAppPoolCredentials(session, caption);
                        break;
                    case "ValidateIISDialogInput":
                        ValidateIISDialogInput(session, caption);
                        break;
                    case "ValidateNotificationServiceDialogInput":
                        ValidateNotificationServiceDialogInput(session, caption);
                        break;
                    case "ValidatePivotServiceDialogInput":
                        ValidatePivotServiceDialogInput(session, caption);
                        break;
                    case "CopyWebConfigToCollectionsVirtualDirectory":
                        CopyWebConfigToCollectionsVirtualDirectory(session);
                        break;
                    default:
                        MessageBox.Show("Fatal Error: Managed CA not specified", caption, MessageBoxButtons.OK);
                        session.Log("Fatal Error: Managed CA not specified");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error has occurred. Please check the log file." + ex.ToString(), caption, MessageBoxButtons.OK);
                session.Log(ex.ToString());
            }

            return result;
        }

        private static void ValidatePivotServiceDialogInput(Session session, string caption)
        {
            session["VALIDPIVOTSERVICEDIALOG"] = string.Empty;
            ValidateAppPoolCredentials(session, caption);

            if (session["VALIDCREDENTIALS"] == "1")
            {
                string collectionPath = session["PIVOTSERVICESSHAREPATH"];
                if (string.IsNullOrEmpty(collectionPath))
                {
                    MessageBox.Show("Pivot collection share path cannot be empty.\nPlease enter a valid physical folder path.", caption, MessageBoxButtons.OK);
                    return;
                }
                else
                {
                    if (!Directory.Exists(collectionPath))
                    {
                        MessageBox.Show("Pivot collection share path does not exist.\nPlease enter a valid physical folder path.", caption, MessageBoxButtons.OK);
                        return;
                    }
                }

                session["VALIDPIVOTSERVICEDIALOG"] = "1";
            }
        }

        private static void ValidateNotificationServiceDialogInput(Session session, string caption)
        {
            session["VALIDNOTIFICATIONSERVICEDIALOG"] = string.Empty;
            ValidateAppPoolCredentials(session, caption);

            if (session["VALIDCREDENTIALS"] == "1")
            {
                string batchSize = session["BATCHSIZE"];
                string timeout = session["TIMEOUT"];

                if (string.IsNullOrEmpty(batchSize) ||
                    string.IsNullOrEmpty(timeout))
                {
                    MessageBox.Show("Batch Size and/or Timeout cannot be empty. \nPlease enter valid Batch Size and/or Timeout values.", caption, MessageBoxButtons.OK);
                    return;
                }

                long batchSizeValue, timeoutValue;
                if (!(long.TryParse(batchSize, out batchSizeValue) &&
                    long.TryParse(timeout, out timeoutValue)))
                {
                    MessageBox.Show("Batch Size and/or Timeout values are not valid numbers. \nPlease enter valid Batch Size and/or Timeout values.", caption, MessageBoxButtons.OK);
                    return;
                }

                if (batchSizeValue <= 0 ||
                    timeoutValue <= 0)
                {
                    MessageBox.Show("Batch Size and/or Timeout values should be greater than zero. \nPlease enter valid Batch Size and/or Timeout values.", caption, MessageBoxButtons.OK);
                    return;
                }

                session["VALIDNOTIFICATIONSERVICEDIALOG"] = "1";
            }
        }

        private static void ValidateIISDialogInput(Session session, string caption)
        {
            session["VALIDZENTITYINPUT"] = string.Empty;
            string websiteName = session["ZENTITYIISWEBSITENAME"];
            string portNumber = session["ZENTITYIISWEBSITEPORT"];
            string callbackPortNumber = session["ZENTITYCALLBACKPORT"];

            if (string.IsNullOrEmpty(websiteName))
            {
                MessageBox.Show("Zentity Website name cannot be empty.\nPlease enter a valid website name", caption, MessageBoxButtons.OK);
                return;
            }
            else
            {
                if (websiteName.Length > 255)
                {
                    MessageBox.Show("Zentity Website name cannot be longer than 255 characters.\nPlease enter a valid website name", caption, MessageBoxButtons.OK);
                    return;
                }

                bool invalidWebsiteName = false;
                char[] notallowedCharacters = new char[] { '\\', '/', '?', ';', ':', '@', '&', '=', '+', '$', ',', '|', '<', '>', '!', '#', '%' };

                foreach (char c in websiteName.ToCharArray())
                {
                    if (notallowedCharacters.Contains(c))
                    {
                        invalidWebsiteName = true;
                        break;
                    }
                }

                if (invalidWebsiteName)
                {
                    MessageBox.Show("Zentity Website name contains invalid and/or special characters.\nPlease enter a valid website name", caption, MessageBoxButtons.OK);
                    return;
                }
            }

            long zentityPortNumber = 0;
            if (string.IsNullOrEmpty(portNumber))
            {
                MessageBox.Show("Please enter a valid port number for Zentity website.", caption, MessageBoxButtons.OK);
                return;
            }
            else
            {
                if (!long.TryParse(portNumber, out zentityPortNumber))
                {
                    MessageBox.Show("Please enter a valid port number for Zentity website.", caption, MessageBoxButtons.OK);
                    return;
                }

                if (zentityPortNumber >= 65536 || zentityPortNumber <= 0)
                {
                    MessageBox.Show("Port number should be greater than 0 and lesser than 65536.\nPlease enter a valid port number for Zentity website.", caption, MessageBoxButtons.OK);
                    return;
                }
            }

            long zentityProgressServicePortNumber = 0;
            if (string.IsNullOrEmpty(callbackPortNumber))
            {
                MessageBox.Show("Please enter a valid port number for the Progress reporting service.", caption, MessageBoxButtons.OK);
                return;
            }
            else
            {
                if (!long.TryParse(callbackPortNumber, out zentityProgressServicePortNumber))
                {
                    MessageBox.Show("Please enter a valid port number for the Progress reporting service.", caption, MessageBoxButtons.OK);
                    return;
                }

                if (zentityProgressServicePortNumber >= 65536 || zentityProgressServicePortNumber <= 0)
                {
                    MessageBox.Show("Port number should be greater than 0 and lesser than 65536.\nPlease enter a valid port number for the Progress reporting service.", caption, MessageBoxButtons.OK);
                    return;
                }
            }

            if (zentityPortNumber == zentityProgressServicePortNumber)
            {
                MessageBox.Show("Port numbers for Zentity Website and Progress reporting service cannot be same.\nPlease enter a valid port number for the Progress reporting service.", caption, MessageBoxButtons.OK);
                return;
            }

            session["VALIDZENTITYINPUT"] = "1";
        }

        #region ValidateDbCreationPerm

        private static void ValidateDbCreationPerm(Session context, string caption)
        {
            // Set "No" by default.
            // Setting a property value to an empty string removes it from the Installer's property collection.
            context["HASDBCREATIONPERM"] = string.Empty;

            string sqlServerInstanceName = context["SQLSERVERINSTANCENAME"];
            string sqlServerDatabaseName = context["SQLSERVERDATABASENAME"];

            if (string.IsNullOrEmpty(sqlServerInstanceName) ||
                string.IsNullOrEmpty(sqlServerInstanceName.Trim()))
            {
                MessageBox.Show("Please enter a valid SQL Server instance name.", caption, MessageBoxButtons.OK);
                return;
            }

            string errorMessage = ValidateDbCreationPerm(sqlServerInstanceName, "WIN", null, null, sqlServerDatabaseName, true);
            if (null == errorMessage)
            {
                context["HASDBCREATIONPERM"] = "1";
            }
            else
            {
                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK);
                context.Log(errorMessage);
            }
        }

        private static string ValidateDbCreationPerm(string instanceName, string authenticationType, string userId, string password, string databaseName, bool ErrorIfDbExists)
        {
            string errorMessage = null;
            string connectionString = null;

            connectionString = GetDBConnectionString(instanceName, authenticationType, userId, password, "tempdb");
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                SqlCommand sqlCommand = new SqlCommand("select IS_SRVROLEMEMBER('dbcreator')", conn);
                conn.Open();
                int dbCreationPermission = (int)sqlCommand.ExecuteScalar();
                if (dbCreationPermission != 1)
                {
                    throw new InvalidOperationException("User does not have create permissions on the database server");
                }
            }
            catch (SqlException)
            {
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            catch (InvalidOperationException)
            {
                // An InvalidOperationException is thrown if there is a '\' error. eg. '.\sqlexpress\'
                errorMessage += "Please ensure that you have database create permissions on the specified database server.\n";
            }
            catch (Exception)
            {
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            finally
            {
                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    SqlConnection.ClearPool(conn);
                    conn.Close();
                    conn.Dispose();
                }
            }

            if (null == errorMessage)
            {
                // Test connection and database
                if (databaseName != null && databaseName.Trim() != String.Empty)
                {
                    connectionString = GetDBConnectionString(instanceName, authenticationType, userId, password, databaseName);

                    try
                    {
                        conn = new SqlConnection(connectionString);
                        conn.Open();

                        if (ErrorIfDbExists)
                        {
                            errorMessage += String.Format("You cannot continue. Please backup your existing database '{0}' and delete it before attempting to create a new database.\n", databaseName);
                        }
                    }
                    catch (SqlException ex)
                    {
                        // correct behavior will skip error if the db should not exist
                        if (!ErrorIfDbExists)
                            throw ex;
                    }
                    finally
                    {
                        if (conn != null && conn.State != ConnectionState.Closed)
                        {
                            SqlConnection.ClearPool(conn);
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                }
            }

            return errorMessage;
        }

        private static string ValidateDbAccessPerm(string instanceName, string authenticationType, string userId, string password)
        {
            string errorMessage = null;
            string connectionString = null;

            connectionString = GetDBConnectionString(instanceName, "WIN", null, null, "tempdb");
            SqlConnection conn = null;

            try
            {
                if (ImpersonateZentityUser(userId, password))
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();
                }
                else
                {
                    errorMessage = "Please enter a valid username and password.";
                }
            }
            catch (SqlException)
            {
                errorMessage += "Please ensure that the user has access permissions to the database server.\n";
            }
            catch (InvalidOperationException)
            {
                // An InvalidOperationException is thrown if there is a '\' error. eg. '.\sqlexpress\'
                errorMessage += "Please ensure that the user has access permissions to the database server.\n";
            }
            catch (Exception)
            {
                errorMessage += "Please ensure that the user has access permissions to the database server.\n";
            }
            finally
            {
                UndoImpersonation();
                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    SqlConnection.ClearPool(conn);
                    conn.Close();
                    conn.Dispose();
                }
            }

            return errorMessage;
        }

        #endregion

        #region FetchDbDefaultFolderPath

        private static void FetchDbDefaultFolderPath(Session context, string caption)
        {
            // Set "No" by default.
            // Setting a property value to an empty string removes it from the Installer's property collection.
            context["UPDATEDSQLSERVERFILEPATHS"] = string.Empty;

            string sqlServerInstanceName = context["SQLSERVERINSTANCENAME"];

            string path;
            string errorMessage = FetchDbDefaultFolderPath(sqlServerInstanceName, "WIN", null, null, out path);

            if (null == errorMessage)
            {
                context["SQLSERVERFILEPATHS"] = path;
                context["UPDATEDSQLSERVERFILEPATHS"] = "1";
            }
            else
            {
                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK);
                context.Log(errorMessage);
            }
        }

        public static string FetchDbDefaultFolderPath(string instanceName, string authenticationType, string userId, string password, out string path)
        {
            string errorMessage = null;
            string connectionString = null;
            path = null;

            connectionString = GetDBConnectionString(instanceName, authenticationType, userId, password, "master");

            string query = "SELECT SUBSTRING(physical_name, 0, (LEN(physical_name) - LEN('master.mdf') + 1)) FROM sys.database_files WHERE name = 'master'";
            SqlCommand command = new SqlCommand();

            // Assume that there is a valid connection.
            try
            {
                command.Connection = new SqlConnection(connectionString);
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.Connection.Open();
                path = (string)command.ExecuteScalar();
            }
            catch (SqlException)
            {
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            catch (InvalidOperationException)
            {
                // An InvalidOperationException is thrown if there is a '\' error. eg. '.\sqlexpress\'
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            catch (Exception)
            {
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            finally
            {
                if (command != null && command.Connection != null && command.Connection.State != ConnectionState.Closed)
                {
                    SqlConnection.ClearPool(command.Connection);
                    command.Connection.Close();
                    command.Connection.Dispose();

                }
            }

            return errorMessage;
        }

        #endregion

        #region ValidateDBFilesFolderPath

        private static void ValidateDBFilesFolderPath(Session context, string caption)
        {
            // Set "No" by default.
            // Setting a property value to an empty string removes it from the Installer's property collection.
            context["VALIDDBFILESFOLDERPATH"] = string.Empty;

            string sqlServerInstanceName = context["SQLSERVERINSTANCENAME"];

            string filePaths = context["SQLSERVERFILEPATHS"];
            string errorMessage = ValidateDBFilesFolderPath(sqlServerInstanceName, "WIN", null, null, filePaths);

            if (null == errorMessage)
            {
                context["VALIDDBFILESFOLDERPATH"] = "1";
            }
            else
            {
                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK);
                context.Log(errorMessage);
            }
        }

        private static string ValidateDBFilesFolderPath(string instanceName, string authenticationType, string userId, string password, string filePaths)
        {
            string errorMessage = null;
            string connectionString = null;

            connectionString = GetDBConnectionString(instanceName, authenticationType, userId, password, "tempdb");

            string fileIsADirectory = null;

            if (!string.IsNullOrEmpty(filePaths))
            {
                string query = "xp_fileexist";
                SqlCommand command = new SqlCommand();

                // Assume that there is a valid connection.
                try
                {
                    command.Connection = new SqlConnection(connectionString);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@filename", filePaths));
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        fileIsADirectory = reader.GetByte(1).ToString();
                    }
                }
                catch (SqlException)
                {
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                catch (InvalidOperationException)
                {
                    // An InvalidOperationException is thrown if there is a '\' error. eg. '.\sqlexpress\'
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                catch (Exception)
                {
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                finally
                {
                    if (command != null && command.Connection != null && command.Connection.State != ConnectionState.Closed)
                    {
                        SqlConnection.ClearPool(command.Connection);
                        command.Connection.Close();
                        command.Connection.Dispose();
                    }
                }
            }

            if (fileIsADirectory != "1")
            {
                errorMessage = "Please ensure that the folder for database files exists on the SQL Server instance machine and you have access to it.\n";
            }

            return errorMessage;
        }

        #endregion

        #region ValidateDBFileStream

        private static void ValidateDBFileStream(Session context, string caption)
        {
            // Set "No" by default.
            // Setting a property value to an empty string removes it from the Installer's property collection.
            context["DBHASFILESTREAM"] = string.Empty;

            string sqlServerInstanceName = context["SQLSERVERINSTANCENAME"];

            string errorMessage = ValidateDBFileStream(sqlServerInstanceName, "WIN", null, null);

            if (null == errorMessage)
            {
                context["DBHASFILESTREAM"] = "1";
            }
            else
            {
                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK);
                context.Log(errorMessage);
            }
        }

        private static string ValidateDBFileStream(string instanceName, string authenticationType, string userId, string password)
        {
            string errorMessage = null;
            string connectionString = null;

            connectionString = GetDBConnectionString(instanceName, authenticationType, userId, password, "tempdb");

            string query = "select value_in_use from sys.configurations where name='filestream access level'";
            SqlCommand command = new SqlCommand();

            int fileStreamStatus = 0;
            // Assume that there is a valid connection.
            try
            {
                command.Connection = new SqlConnection(connectionString);
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.Connection.Open();
                object scalar = command.ExecuteScalar();
                if (scalar != null)
                {
                    fileStreamStatus = (int)scalar;
                }
            }
            catch (SqlException)
            {
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            catch (InvalidOperationException)
            {
                // An InvalidOperationException is thrown if there is a '\' error. eg. '.\sqlexpress\'
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            catch (Exception)
            {
                errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
            }
            finally
            {
                if (command != null && command.Connection != null && command.Connection.State != ConnectionState.Closed)
                {
                    SqlConnection.ClearPool(command.Connection);
                    command.Connection.Close();
                    command.Connection.Dispose();
                }
            }

            if (fileStreamStatus == 0)
            {
                errorMessage = "You cannot continue. Please enable FILESTREAM on the Sql Server Instance. \n";
                errorMessage += "For instructions on how to do so, please visit http://msdn.microsoft.com/en-us/library/cc645923.aspx \n";
            }

            return errorMessage;
        }

        #endregion

        #region ValidateDBFileStreamFolderPath

        private static void ValidateDBFileStreamFolderPath(Session context, string caption)
        {
            // Set "No" by default.
            // Setting a property value to an empty string removes it from the Installer's property collection.
            context["VALIDDBFILESTREAMFOLDER"] = string.Empty;

            string sqlServerInstanceName = context["SQLSERVERINSTANCENAME"];


            string fileStreamFolder = context["FILESTREAMFOLDER"];
            string contentFolder = context["FILESTREAMFOLDERNAME"];

            if (string.IsNullOrEmpty(fileStreamFolder))
            {
                MessageBox.Show("Please enter a valid physical path");
                return;
            }

            string errorMessage = ValidateDBFileStreamFolderPath(sqlServerInstanceName, "WIN", null, null, fileStreamFolder);
            if (null == errorMessage)
            {
                errorMessage = ValidateContentFolderDoesntExists(sqlServerInstanceName, "WIN", null, null, Path.Combine(fileStreamFolder, contentFolder));
                if (null == errorMessage)
                {
                    if (fileStreamFolder[fileStreamFolder.Length - 1] != '\\')
                        context["FILESTREAMFOLDER"] = fileStreamFolder + "\\";

                    context["VALIDDBFILESTREAMFOLDER"] = "1";
                }
                else
                {
                    MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK);
                    context.Log(errorMessage);
                }
            }
            else
            {
                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK);
                context.Log(errorMessage);
            }
        }

        private static string ValidateDBFileStreamFolderPath(string instanceName, string authenticationType, string userId, string password, string fileStreamFolder)
        {
            string errorMessage = null;
            string connectionString = null;

            connectionString = GetDBConnectionString(instanceName, authenticationType, userId, password, "tempdb");

            string fileIsADirectory = null;
            if (!string.IsNullOrEmpty(fileStreamFolder))
            {
                string query = "xp_fileexist";
                SqlCommand command = new SqlCommand();

                // Assume that there is a valid connection.
                try
                {
                    command.Connection = new SqlConnection(connectionString);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@filename", fileStreamFolder));
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        fileIsADirectory = reader.GetByte(1).ToString();
                    }
                }
                catch (SqlException)
                {
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                catch (InvalidOperationException)
                {
                    // An InvalidOperationException is thrown if there is a '\' error. eg. '.\sqlexpress\'
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                catch (Exception)
                {
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                finally
                {
                    if (command != null && command.Connection != null && command.Connection.State != ConnectionState.Closed)
                    {
                        SqlConnection.ClearPool(command.Connection);
                        command.Connection.Close();
                        command.Connection.Dispose();

                    }
                }
            }

            if (fileIsADirectory != "1")
            {
                errorMessage = "Please ensure that the folder for content exists on the SQL Server instance machine and you have access to it.\n";
            }

            return errorMessage;
        }

        private static string ValidateContentFolderDoesntExists(string instanceName, string authenticationType, string userId, string password, string contentFolder)
        {
            string errorMessage = null;
            string connectionString = null;

            connectionString = GetDBConnectionString(instanceName, authenticationType, userId, password, "tempdb");

            string fileIsADirectory = null;
            if (!string.IsNullOrEmpty(contentFolder))
            {
                string query = "xp_fileexist";
                SqlCommand command = new SqlCommand();

                // Assume that there is a valid connection.
                try
                {
                    command.Connection = new SqlConnection(connectionString);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@filename", contentFolder));
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        fileIsADirectory = reader.GetByte(1).ToString();
                    }
                }
                catch (SqlException)
                {
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                catch (InvalidOperationException)
                {
                    // An InvalidOperationException is thrown if there is a '\' error. eg. '.\sqlexpress\'
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                catch (Exception)
                {
                    errorMessage += "Please ensure that the SQL Server instance exists and that you have access to it.\n";
                }
                finally
                {
                    if (command != null && command.Connection != null && command.Connection.State != ConnectionState.Closed)
                    {
                        SqlConnection.ClearPool(command.Connection);
                        command.Connection.Close();
                        command.Connection.Dispose();

                    }
                }
            }

            if (fileIsADirectory == "1")
            {
                errorMessage = "Please ensure that the content folder (" + contentFolder + ") doesn't exists on the SQL Server instance machine.\n";
            }

            return errorMessage;
        }

        #endregion

        #region ValidateAppPoolCredentials

        private static void ValidateAppPoolCredentials(Session context, string caption)
        {
            // Set "No" by default.
            // Setting a property value to an empty string removes it from the Installer's property collection.
            context["VALIDAPPPOOLCREDENTIALS"] = string.Empty;
            context["VALIDCREDENTIALS"] = "0";

            string sqlServerInstanceName = context["SQLSERVERINSTANCENAME"];
            string user = context["IDENTITYUSER"];
            string password = context["IDENTITYPASSWORD"];

            string username, domain;
            string errorMessage = ValidateUsername(user, out username, out domain);

            if (string.IsNullOrEmpty(errorMessage))
            {
                string qualifiedUsername;
                errorMessage = ValidateAppPoolCredentials(username, domain, password, out qualifiedUsername);

                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage += ValidateDbAccessPerm(sqlServerInstanceName, string.Empty, user, password);

                if (string.IsNullOrEmpty(errorMessage))
                {
                    context["VALIDCREDENTIALS"] = "1";
                    context["IDENTITYUSER"] = qualifiedUsername;
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, caption, MessageBoxButtons.OK);
                context.Log(errorMessage);
            }
        }

        private static string ValidateUsername(string user, out string username, out string domain)
        {
            bool success = false;
            string errorMessage = null;
            username = null;
            domain = null;

            int indexOf = user.IndexOf('\\');
            int lastIndexOf = user.LastIndexOf('\\');

            // No slash or more than one slash.
            if (indexOf != -1 && indexOf == lastIndexOf)
            {
                domain = user.Remove(indexOf);
                username = user.Substring(indexOf + 1);

                // Blank username or domain name.
                if (domain.Trim() != string.Empty && username.Trim() != string.Empty)
                {
                    success = true;
                }
            }

            if (success == false)
            {
                errorMessage = "Please enter the User Name in the form: DomainName\\UserName OR MachineName\\UserName.";
            }

            return errorMessage;
        }

        private static string ValidateAppPoolCredentials(string username, string domain, string password, out string qualifiedUsername)
        {
            string errorMessage = null;
            qualifiedUsername = null;

            IntPtr token = IntPtr.Zero;
            WindowsIdentity user = null;
            bool success;
            try
            {
                success = LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out token);
                if (success)
                {
                    user = new WindowsIdentity(token);
                    qualifiedUsername = user.Name;
                }
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (user != null)
                {
                    user.Dispose();
                }
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }

            if (success == false)
            {
                errorMessage = "Please enter a valid username and password.";
            }

            return errorMessage;
        }

        #region Interop

        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_PROVIDER_DEFAULT = 0;

        [DllImport("advapi32.dll", SetLastError = false)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken
            );

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        #endregion

        #endregion

        #region CreateLocalUserGroups

        /// <summary>
        /// Creates the ZentityAdministrators and ZentityUsers local groups.
        /// </summary>
        /// <param name="session">Installation session object.</param>
        [CustomAction]
        public static ActionResult CreateLocalUserGroups(Session session)
        {
            ActionResult result = ActionResult.Success;

            try
            {
                CreateLocalUserGroups(session.CustomActionData.Values);
            }
            catch (Exception ex)
            {
                result = ActionResult.Failure;
                MessageBox.Show("Unable to create Zentity Local User Groups.", caption, MessageBoxButtons.OK);
                session.Log(ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// Creates a local user group with the given group name.
        /// </summary>
        /// <param name="groupNames">The names of groups that needs to be created.</param>
        private static void CreateLocalUserGroups(ICollection<string> groupNames)
        {
            foreach (var groupName in groupNames)
            {
                PrincipalContext context = new PrincipalContext(ContextType.Machine);

                GroupPrincipal group = GroupPrincipal.FindByIdentity(context, groupName);

                if (group == null)
                {
                    group = new GroupPrincipal(context);
                    group.Name = groupName;
                    group.IsSecurityGroup = true;
                    group.Save();
                }
            }
        }

        #endregion

        #region Add Users to Groups

        /// <summary>
        /// Adds users to Zentity groups.
        /// </summary>
        /// <param name="session">Installation session object.</param>
        [CustomAction]
        public static ActionResult AddUsersToGroups(Session session)
        {
            ActionResult result = ActionResult.Success;

            try
            {
                IEnumerable<string> groups = session.CustomActionData.Values.Take(2);
                IEnumerable<KeyValuePair<string, string>> credentials = GetUserCredentials(session.CustomActionData);

                AddUsersToGroups(groups, credentials, caption);
            }
            catch (Exception ex)
            {
                result = ActionResult.Failure;

                MessageBox.Show("Unable to add users to Zentity Groups", caption, MessageBoxButtons.OK);
                session.Log(ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// Adds distinct Zentity users to the collection.
        /// </summary>
        /// <param name="customActionData">The installer CustomActionData</param>
        /// <returns>Distinct Zentity users</returns>
        private static IEnumerable<KeyValuePair<string, string>> GetUserCredentials(CustomActionData customActionData)
        {
            Dictionary<string, string> credentials = new Dictionary<string, string>();

            credentials.Add(customActionData["appPoolUser"], customActionData["appPoolUserPassword"]);

            if (!credentials.Keys.Contains(customActionData["pivotUser"]))
            {
                credentials.Add(customActionData["pivotUser"], customActionData["pivotUserPassword"]);
            }

            if (!credentials.Keys.Contains(customActionData["notificationUser"]))
            {
                credentials.Add(customActionData["notificationUser"], customActionData["notificationUserPassword"]);
            }

            return credentials.Distinct();
        }

        /// <summary>
        /// Adds users to zentity groups.
        /// </summary>
        /// <param name="zentityGroups">Zentity local groups.</param>
        /// <param name="zentityUsers">The users that need to be added to groups.</param>
        /// <param name="caption">Zentity message box caption text.</param>
        private static void AddUsersToGroups(IEnumerable<string> zentityGroups, IEnumerable<KeyValuePair<string, string>> zentityUsers, string caption)
        {
            foreach (var zentityUser in zentityUsers)
            {
                PrincipalContext userContext = null;
                UserPrincipal userPrincipal = null;

                try
                {
                    if (ImpersonateZentityUser(zentityUser.Key, zentityUser.Value))
                    {
                        userContext = new PrincipalContext(ContextType.Domain);
                        userPrincipal = UserPrincipal.FindByIdentity(userContext, zentityUser.Key);

                        UndoImpersonation();

                        if (userPrincipal != null)
                        {
                            AddUserToGroups(userPrincipal, zentityGroups, caption);
                        }
                    }
                }
                catch (COMException) { /*occurs when local account is used to search on domain */ }
                catch (PrincipalServerDownException) { /*occurs in workgroup mode */}
                catch (Exception) { }
                finally
                {
                    UndoImpersonation();
                }

                if (userPrincipal == null)
                {
                    userContext = new PrincipalContext(ContextType.Machine);
                    userPrincipal = UserPrincipal.FindByIdentity(userContext, zentityUser.Key);

                    if (userPrincipal != null)
                    {
                        AddUserToGroups(userPrincipal, zentityGroups, caption);
                    }
                }

                if (userPrincipal == null)
                {                    
                    throw new ArgumentException("Unable to resolve the Zentity user.");
                }
            }
        }

        /// <summary>
        /// Adds the user to Zentity groups.
        /// </summary>
        /// <param name="userPrincipal">UserPrincipal object of the user.</param>
        /// <param name="zentityGroups">Groups to which user needs to be added.</param>
        /// <param name="caption">Zentity message box caption text.</param>
        private static void AddUserToGroups(UserPrincipal userPrincipal, IEnumerable<string> zentityGroups, string caption)
        {
            PrincipalContext context = new PrincipalContext(ContextType.Machine);

            foreach (var zentityGroup in zentityGroups)
            {
                GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(context, zentityGroup);

                if (groupPrincipal != null)
                {
                    //check if user is already a member
                    if (!groupPrincipal.Members.Contains(userPrincipal))
                    {
                        groupPrincipal.Members.Add(userPrincipal);
                        groupPrincipal.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Impersonate the Zentity Domain user.
        /// </summary>
        /// <param name="zentityUser">Name of the Domain user.</param>
        /// <param name="password">User Password.</param>
        /// <returns>true if impersonation was successful.</returns>
        private static bool ImpersonateZentityUser(string zentityUser, string password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            string userName = zentityUser;
            string domain = String.Empty;

            if (RevertToSelf())
            {
                if (zentityUser.IndexOf(@"\") > 0)
                {
                    string[] parts = zentityUser.Split(new string[] { @"\" }, StringSplitOptions.None);

                    if (parts != null && parts.Length == 2)
                    {
                        userName = parts[1];
                        domain = parts[0];
                    }
                }

                if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
                        LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }

            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
            return false;
        }

        /// <summary>
        /// Undo the impersonation.
        /// </summary>
        private static void UndoImpersonation()
        {
            if (impersonationContext != null)
            {
                impersonationContext.Undo();
            }
        }

        #endregion

        private static void CopyWebConfigToCollectionsVirtualDirectory(Session session)
        {
            string collectionsVirtualDirectortyPath = session["PIVOTSERVICESSHAREPATH"];

            if (!string.IsNullOrEmpty(collectionsVirtualDirectortyPath))
            {
                try
                {
                    byte[] buffer = Encoding.Default.GetBytes(Resource.web);

                    using (FileStream stream = File.Create(collectionsVirtualDirectortyPath + @"\" + "web.config", buffer.Count()))
                    {
                        stream.Write(buffer, 0, buffer.Count());
                        stream.Close();
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                throw new ArgumentNullException("Value for collections virtual directory cannot be empty");
            }
        }

        private static string GetDBConnectionString(string instanceName, string authenticationType, string userId, string password, string databaseName)
        {
            string connectionString = string.Empty;
            if (authenticationType == "WIN")
            {
                //try to connect to SQL server
                connectionString = "Server=" + instanceName + ";Database=" + databaseName + ";Trusted_Connection=Yes;";
            }
            else
            {
                connectionString = "Server=" + instanceName + ";Database=" + databaseName + ";User Id=" + userId + "; password= " + password + ";";
            }
            return connectionString;
        }
    }
}
