// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Zentity.Pivot;
using Zentity.Services.Web;
using Zentity.Services.Web.Pivot;

namespace Zentity.CollectionCreator.Commands
{
    using System;
    using System.Collections.Specialized;
    using System.Text;

    class UpdateCollection : ICommand
    {
        /// <summary>
        /// The help message format string for the custom command
        /// </summary>
        private const string HelpMessageFormat = "\nSyntax : /operation:updatecollection\n" +
                                                 "         /{0}:<Instance ID>\n" +
                                                 "         /{1}:<DataModel Namespace>\n" +
                                                 "         /{2}:<ResourceType Name>\n" +
                                                 "         /{3}:<Change Message File Path>";

        /// <summary>
        /// Switch name for the "instanceId" command line argument
        /// </summary>
        private const string ArgumentInstanceId = "instanceId";

        /// <summary>
        /// Switch name for the "modelNamespace" command line argument
        /// </summary>
        private const string ArgumentModelNamespace = "modelNamespace";

        /// <summary>
        /// Switch name for the "resourceType" command line argument
        /// </summary>
        private const string ArgumentResourceTypeName = "resourceType";

        /// <summary>
        /// Switch name for the "changeMessageFilePath" command line argument
        /// </summary>
        private const string ArgumentChangeMessageFilePath = "changeMessageFilePath";

        #region ICommand Members

        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="commandData">The key values collection of data.</param>
        public void Run(string commandName, StringDictionary commandData)
        {
            try
            {
                // Validate the arguments for this command
                ValidateDataArguments(commandData);

                // Get the InstanceID from the command arguments
                Guid instanceId;
                Guid.TryParse(commandData[ArgumentInstanceId].Trim(), out instanceId);
                Environment.SetEnvironmentVariable("InstanceID", "_" + instanceId, EnvironmentVariableTarget.Process);


                string modelNamespace = commandData[ArgumentModelNamespace].Trim();
                string resourceType = commandData[ArgumentResourceTypeName].Trim();
                string filePath = commandData[ArgumentChangeMessageFilePath].Trim();

                Console.WriteLine(string.Format("Operation    : Update Collection\n" +
                                                "InstanceID   : {0}\n" + 
                                                "Date Model   : {1}\n" + 
                                                "ResourceType : {2}\n" +
                                                "File Path    : {3}", instanceId, modelNamespace, resourceType, filePath), 
                                              CultureInfo.InvariantCulture);

                IEnumerable<ResourceChangeMessage> changeMessageList = ResourceChangeMessage.DeserializeResourceChangeMessageList(filePath);
                if (changeMessageList == null)
                {
                    throw new ArgumentException("changeMessageList", "Update Collection operation will be aborted.");
                }
                
                using (CollectionHelper collectionHelper = new CollectionHelper(instanceId, modelNamespace, resourceType))
                {
                    collectionHelper.UpdateCollection(changeMessageList);
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    { }
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <returns>Help message string</returns>
        public string GetHelpMessage()
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 HelpMessageFormat,
                                 ArgumentInstanceId,
                                 ArgumentModelNamespace,
                                 ArgumentResourceTypeName, 
                                 ArgumentChangeMessageFilePath);
        }

        #endregion

        /// <summary>
        /// Validates the data arguments.
        /// </summary>
        /// <param name="commandData">The key values collection of data.</param>
        private static void ValidateDataArguments(StringDictionary commandData)
        {
            StringBuilder exceptionMessage = new StringBuilder();
            bool parameterMissing = false;
            bool parameterInvalid = false;

            if (!commandData.ContainsKey(ArgumentInstanceId))
            {
                exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                            "Missing mandatory parameter : [{0}]",
                                                            ArgumentInstanceId));
                parameterMissing = true;
            }
            else
            {
                if (commandData[ArgumentInstanceId].Trim().Length == 0)
                {
                    exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                "Missing value for mandatory parameter : [{0}]",
                                                ArgumentInstanceId));
                    parameterInvalid = true;
                }
                else
                {
                    Guid instanceId;
                    if (!Guid.TryParse(commandData[ArgumentInstanceId].Trim(), out instanceId))
                    {
                        exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                                  "Invalid Guid value for parameter [{0}] : {1}",
                                                                  ArgumentInstanceId,
                                                                  commandData[ArgumentInstanceId].Trim()));
                        parameterInvalid = true;
                    }
                }
            }

            if (!commandData.ContainsKey(ArgumentModelNamespace))
            {
                exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, 
                                                            "Missing mandatory parameter : [{0}]",
                                                            ArgumentModelNamespace));
                parameterMissing = true;
            }
            else
            {
                if (commandData[ArgumentModelNamespace].Trim().Length == 0)
                {
                    exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, 
                                                "Missing value for mandatory parameter : [{0}]",
                                                ArgumentModelNamespace));
                    parameterInvalid = true;
                }
            }

            if (!commandData.ContainsKey(ArgumentResourceTypeName))
            {
                exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                            "Missing mandatory parameter : [{0}]",
                                                            ArgumentResourceTypeName));
                parameterMissing = true;
            }
            else
            {
                if (commandData[ArgumentResourceTypeName].Trim().Length == 0)
                {
                    exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                "Missing value for mandatory parameter : [{0}]",
                                                ArgumentResourceTypeName));
                    parameterInvalid = true;
                }
            }

            if (!commandData.ContainsKey(ArgumentChangeMessageFilePath))
            {
                exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                            "Missing mandatory parameter : [{0}]",
                                                            ArgumentChangeMessageFilePath));
                parameterMissing = true;
            }
            else
            {
                if (commandData[ArgumentChangeMessageFilePath].Trim().Length == 0)
                {
                    exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                "Missing value for mandatory parameter : [{0}]",
                                                ArgumentChangeMessageFilePath));
                    parameterInvalid = true;
                }
                else if (!System.IO.File.Exists(commandData[ArgumentChangeMessageFilePath].Trim()))
                {
                    exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                              "The change message file path was not found : [{0}]",
                                                              commandData[ArgumentChangeMessageFilePath].Trim()));
                }
            }

            if (parameterMissing || parameterInvalid)
            {
                throw new ArgumentException(exceptionMessage.ToString());
            }
        }
    }
}
