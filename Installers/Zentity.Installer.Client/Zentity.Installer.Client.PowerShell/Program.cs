// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Zentity.Installer.Client.PowerShell
{
    public class Program
    {
        static void Main(string[] args)
        {
            string psinstallPath = Registry.LocalMachine.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\1\PowerShellEngine",
                                               @"C:\Windows\System32\WindowsPowerShell\v1.0",
                                               RegistryValueOptions.None) as string;

            if (!string.IsNullOrEmpty(psinstallPath))
            {
                byte[] buffer = Encoding.Default.GetBytes(Resources.powershell_exe);

                using (FileStream stream = File.Create(psinstallPath + @"\" + "powershell.exe.config", buffer.Count()))
                {
                    stream.Write(buffer, 0, buffer.Count());
                    stream.Close();
                }
            }
        }
    }
}
