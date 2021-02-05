// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using Zentity.DeepZoomCreator.CommandLine;
using Zentity.DeepZoomCreator.Commands;
using Zentity.Services.Web;

namespace Zentity.DeepZoomCreator
{
    /// <summary>
    /// Applications Entry Point class
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Gets the max retries.
        /// </summary>
        /// <value>The max retries.</value>
        public static int MaxRetries
        {
            get
            {
                int retryVal = Globals.MaxRetryValue;
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["MaxRetries"]))
                {
                    int.TryParse(ConfigurationManager.AppSettings["MaxRetries"], out retryVal);
                }
                return retryVal;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="commandLineArgs">The command line args.</param>
        /// <returns>Return code.</returns>
        [STAThread]
        static int Main(string[] commandLineArgs)
        {
            if (commandLineArgs.Length == 0)
            {
                ShowConsoleMessage(string.Empty, true);
                return (int) ReturnCode.Invalid;
            }

            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);

            // Create a list of switch type and whether requires or not.
            Dictionary<SwitchType, bool> commandSwitchOption = new Dictionary<SwitchType, bool>();
            commandSwitchOption.Add(SwitchType.Operation, true);
            commandSwitchOption.Add(SwitchType.Help, true);

            // Create a list of switch commands as passed on the command line
            List<SwitchCommand> switchCommandList = new List<SwitchCommand>();

            // Processing all command arguments
            foreach (string argument in commandLineArgs)
            {
                // Check for a valid switch syntax
                if (!argument.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !argument.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    ShowConsoleMessage(string.Format(CultureInfo.InvariantCulture, "\nError : Invalid switch '{0}'. Please check help for valid switches.\n", argument), true);
                    return (int) ReturnCode.Invalid;
                }

                // Check for a valid switch command and value separator character
                string[] arrArgSplit = argument.Split(new char[] { ':', '=' }, 2);
                if (arrArgSplit.Length != 2)
                {
                    ShowConsoleMessage(string.Format(CultureInfo.InvariantCulture, "\nError : Invalid switch and value pattern '{0}'.\n", argument), false);
                    return (int) ReturnCode.Invalid;
                }

                // Create a valid switch command object. Show error if there is an problem.
                try
                {
                    SwitchCommand switchCommand = new SwitchCommand(arrArgSplit[0].Substring(1), arrArgSplit[1]);
                    switchCommandList.Add(switchCommand);
                }
                catch
                {
                    ShowConsoleMessage(string.Format(CultureInfo.InvariantCulture, "\nError : Invalid switch '{0}'\n", arrArgSplit[0].Substring(1)), true);
                    return (int) ReturnCode.Invalid;
                }
            }

            // Fetch the mandatory switches
            IEnumerable<SwitchType> mandatorySwitchTypes = commandSwitchOption.Where(switchOption => switchOption.Value).Select(switchOption => switchOption.Key);

            int countMandatory = (from SwitchCommand commandSwitch in switchCommandList
                                  join SwitchType switchType in mandatorySwitchTypes on commandSwitch.Switch equals switchType
                                  select commandSwitch.Switch).Count();

            if (countMandatory == 0)
            {
                ShowConsoleMessage(string.Format(CultureInfo.InvariantCulture, "\nError : Incorrect number of mandatory parameters.\n"), true);
                return (int) ReturnCode.Invalid;
            }

            return ProcessCommands(switchCommandList);
        }

        /// <summary>
        /// Shows the console message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="showHelp">if set to <c>true</c> [show help].</param>
        private static void ShowConsoleMessage(string message, bool showHelp)
        {
            if (!string.IsNullOrWhiteSpace(message) || showHelp)
            {
                // Display the user message on the console
                Console.WriteLine(message);

                // Show the help message if required.
                if (showHelp)
                {
                    Console.WriteLine(GetHelpMessage());
                }
            }
        }

        /// <summary>
        /// Shows the application help.
        /// </summary>
        /// <returns>Help message.</returns>
        private static string GetHelpMessage()
        {
            StringBuilder strbAppHelp = new StringBuilder();
            strbAppHelp.AppendFormat(CultureInfo.InvariantCulture,
                                     "Syntax:\n     {0} /operation:<operation name> [/help:<operation name>]",
                                     System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            return strbAppHelp.ToString();
        }

        /// <summary>
        /// Processes the commands.
        /// </summary>
        /// <param name="switchCommandList">The command switch list.</param>
        /// <returns>Return code</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is the top level method to handle the exception. The user needs to be gracefully shown the exception message.")]
        private static int ProcessCommands(IEnumerable<SwitchCommand> switchCommandList)
        {
            SwitchCommand helpSwitch = switchCommandList.FirstOrDefault(arg => arg.Switch == SwitchType.Help);
            SwitchCommand operationSwitch = switchCommandList.FirstOrDefault(arg => arg.Switch == SwitchType.Operation);
            ICommand commandDef;

            if (helpSwitch != null)
            {
                commandDef = CommandFactory.GetCommandDefinition(helpSwitch.CommandValue.Trim());

                if (commandDef != null)
                {
                    ShowConsoleMessage(commandDef.GetHelpMessage(), false);
                    return (int) ReturnCode.Success;
                }
            }
            else if (operationSwitch != null)
            {
                commandDef = CommandFactory.GetCommandDefinition(operationSwitch.CommandValue.Trim());

                if (commandDef != null)
                {
                    StringDictionary switchData = new StringDictionary();
                    foreach (SwitchCommand commandSwitch in switchCommandList)
                    {
                        if (commandSwitch.Switch == SwitchType.DataArgument)
                        {
                            switchData.Add(commandSwitch.CommandName, commandSwitch.CommandValue);
                        }
                    }

                    try
                    {
                        commandDef.Run(operationSwitch.CommandValue.Trim(), switchData);
                        return (int) ReturnCode.Success;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\nError Message :- \n{0}\n", ex.Message));
                        ShowConsoleMessage(commandDef.GetHelpMessage(), false);
                        return (int) ReturnCode.Error;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\nError   : Exception occurred. Check below for trace information"));
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\nMessage : {0}\n", ex.Message));
                        Console.WriteLine(new string('*', 80));
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine(new string('*', 80));
                        return (int) ReturnCode.Error;
                    }
                }
            }

            ShowConsoleMessage(string.Format(CultureInfo.InvariantCulture, "\nError : Invalid operation or operation not defined.\n"), true);
            return (int) ReturnCode.Invalid;
        }
    }
}
