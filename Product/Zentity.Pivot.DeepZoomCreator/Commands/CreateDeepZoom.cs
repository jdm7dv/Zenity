// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.Diagnostics;
using System.Globalization;
using Zentity.Services.Web;

namespace Zentity.DeepZoomCreator.Commands
{
    using System;
    using System.Collections.Specialized;
    using System.Text;


    /// <summary>
    /// Handles the createdeepzoom command line operation.
    /// </summary>
    class CreateDeepZoom : ICommand
    {
        /// <summary>
        /// The help message format string for the custom command
        /// </summary>
        private const string HelpMessageFormat = "\nSyntax : /operation:createdeepzoom\n" +
                                                 "         /{0}:<Instance ID>\n" +
                                                 "         /{1}:<Source Images Serialized File Path>\n";

        /// <summary>
        /// Switch name for the "instanceId" command line argument
        /// </summary>
        private const string ArgumentInstanceId = "instanceId";

        /// <summary>
        /// Switch name for the "sourceImageFilePath" command line argument
        /// </summary>
        private const string ArgumentSourceImageFilePath = "sourceImageFilePath";

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

                string filePath = commandData[ArgumentSourceImageFilePath].Trim();

                Console.WriteLine(string.Format("Operation    : Create Deep Zoom Images\n" +
                                                "InstanceID   : {0}\n" +
                                                "File Path    : {1}\n", instanceId, filePath, CultureInfo.InvariantCulture));

                ImageGenerator imageGen = new ImageGenerator(instanceId, filePath);
                imageGen.Generate();
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
                                 ArgumentSourceImageFilePath);
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

            if (!commandData.ContainsKey(ArgumentSourceImageFilePath))
            {
                exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                            "Missing mandatory parameter : [{0}]",
                                                            ArgumentSourceImageFilePath));
                parameterMissing = true;
            }
            else
            {
                if (commandData[ArgumentSourceImageFilePath].Trim().Length == 0)
                {
                    exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                "Missing value for mandatory parameter : [{0}]",
                                                ArgumentSourceImageFilePath));
                    parameterInvalid = true;
                }
                else if (!System.IO.File.Exists(commandData[ArgumentSourceImageFilePath].Trim()))
                {
                    exceptionMessage.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                              "The source image serialized file path was not found : [{0}]",
                                                              commandData[ArgumentSourceImageFilePath].Trim()));
                }
            }

            if (parameterMissing || parameterInvalid)
            {
                throw new ArgumentException(exceptionMessage.ToString());
            }
        }
    }
}
