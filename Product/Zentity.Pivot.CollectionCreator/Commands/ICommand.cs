// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.CollectionCreator.Commands
{
    using System.Collections.Specialized;

    /// <summary>
    /// Interface for defining custom commands for the tool.
    /// </summary>
    internal interface ICommand
    {
        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="commandName">The command name.</param>
        /// <param name="commandData">The key values collection.</param>
        /// <returns>Return code</returns>
        void Run(string commandName, StringDictionary commandData);

        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <returns>Help message string</returns>
        string GetHelpMessage();
    }
}
