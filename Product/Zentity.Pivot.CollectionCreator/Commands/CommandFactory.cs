// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.CollectionCreator.Commands
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Factory class for instantiation ICommandDefinition instances
    /// </summary>
    internal static class CommandFactory
    {
        /// <summary>
        /// Gets the command definition.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <returns>Instance of the ICommandDefinition object</returns>
        internal static ICommand GetCommandDefinition(string commandName)
        {
            ICommand commandDefinition = null;
            string fullyQualifiedClassName = ConfigurationManager.AppSettings[commandName];
            if (!string.IsNullOrEmpty(fullyQualifiedClassName))
            {
                Type commandDefType = Type.GetType(fullyQualifiedClassName, false, true);
                if (commandDefType != null)
                {
                    commandDefinition = (ICommand) Activator.CreateInstance(commandDefType);
                }
            }

            return commandDefinition;
        }
    }
}
