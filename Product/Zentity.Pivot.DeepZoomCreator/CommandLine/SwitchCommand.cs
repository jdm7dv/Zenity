// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.DeepZoomCreator.CommandLine
{
    using System;

    /// <summary>
    /// CommandSwitch to handle comannd line arguments
    /// </summary>
    public class SwitchCommand
    {
        /// <summary>
        /// Holds the switch type
        /// </summary>
        private readonly SwitchType commandSwitch;

        /// <summary>
        /// Holds the switch command
        /// </summary>
        private readonly string commandName;

        /// <summary>
        /// Holds the switch value
        /// </summary>
        private readonly string commandValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchCommand"/> class.
        /// </summary>
        /// <param name="switchCommand">The switch command.</param>
        /// <param name="switchValue">The switch value.</param>
        [
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "This is required for syntax matching for command line parameters."),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The parsing of enum will cause an exception if the user enters wrong value. The default enum is set in this case.")
        ]
        public SwitchCommand(string switchCommand, string switchValue)
        {
            this.commandSwitch = SwitchType.DataArgument;
            try
            {
                this.commandSwitch = (SwitchType) Enum.Parse(typeof(SwitchType), switchCommand, true);
            }
            catch { }
            this.commandName = switchCommand.ToLowerInvariant();
            this.commandValue = switchValue.Replace("\"", string.Empty);
        }

        /// <summary>
        /// Gets the switch type.
        /// </summary>
        /// <value>The switch type.</value>
        public SwitchType Switch
        {
            get
            {
                return this.commandSwitch;
            }
        }

        /// <summary>
        /// Gets the switch command as string.
        /// </summary>
        /// <value>The switch command.</value>
        public string CommandName
        {
            get
            {
                return this.commandName;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string CommandValue
        {
            get
            {
                return this.commandValue;
            }
        }
    }
}
