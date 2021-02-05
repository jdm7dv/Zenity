// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Installer.CustomAction
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;
    using System.Linq;
    using Microsoft.Deployment.WindowsInstaller;
    using System.Net;


    public class CustomActions
    {
        private const string InstallPath = "InstallPath";
        private const string MachineName = "MachineName";
        private const string PortNumber = "PortNumber";
        private const string xmlNamespace = "http://schemas.microsoft.com/powershell/2004/04";
        private const string xPath = "/def:Objs/def:Obj/def:DCT";
        private const string prefix = "def";
        private const string fileName = "ZentityConsole.config";
        private const string folderName = "PowerShell scripts";
        private const string ZentityWebsiteDesc = "ZentityWebsiteDesc";
        private const string ZentityCaption = "Zentity Setup";

        [CustomAction]
        public static ActionResult WriteConfigEntries(Session session)
        {
            session.Log("Executing custom action to write entries to the config file");

            string installPath = session.CustomActionData[CustomActions.InstallPath];
            string machineName = session.CustomActionData[CustomActions.MachineName];
            string portNumber = session.CustomActionData[CustomActions.PortNumber];
            string websiteDesc = session.CustomActionData[CustomActions.ZentityWebsiteDesc];

            try
            {
                string filePath = Path.Combine(Path.Combine(installPath, folderName), fileName);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsMgr.AddNamespace(prefix, xmlNamespace);
                XmlNodeList nodes = xmlDoc.SelectSingleNode(xPath, nsMgr).ChildNodes;

                char[] delimiters = new char[] { '/' };

                foreach (XmlNode node in nodes)
                {
                    switch (node.FirstChild.InnerText)
                    {
                        case "ZentityWebsite":
                            {
                                node.LastChild.InnerText = websiteDesc;
                                break;
                            }
                        case "InstallPath":
                            {
                                node.LastChild.InnerText = installPath;
                                break;
                            }
                        default:
                            {
                                try
                                {
                                    if (!String.IsNullOrEmpty(machineName) && !String.IsNullOrEmpty(portNumber))
                                    {
                                        string[] values = node.LastChild.InnerText.Split(delimiters);
                                        node.LastChild.InnerText = String.Concat("http://", machineName, ":", portNumber, "/", values[values.Length - 2], "/", values[values.Length - 1]);
                                    }
                                }
                                catch (IndexOutOfRangeException)
                                {}
                                break;
                            }
                    }
                }

                xmlDoc.Save(filePath);
            }
            catch (Exception ex)
            {
                session.Log(ex.ToString());
                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult ValidateWebsiteDetails(Session session)
        {
            session.Log("Executing custom action to validate website details");
            session["VALIDZENTITYINPUT"] = string.Empty;
            string websiteName = session["ZENTITYWEBSITEDESC"];
            string machineName = session["MACHINENAME"];
            string portNumber = session["PORTNUMBER"];
            char[] notallowedCharacters = new char[] { '\\', '/', '?', ';', ':', '@', '&', '=', '+', '$', ',', '|', '<', '>', '!', '#', '%' };

            if (string.IsNullOrEmpty(websiteName))
            {
                MessageBox.Show("Zentity Website name cannot be empty.\nPlease enter a valid website name.", ZentityCaption);
                return ActionResult.Success;
            }
            else
            {
                if (websiteName.Length > 255)
                {
                    MessageBox.Show("Zentity Website name cannot be longer than 255 characters.\nPlease enter a valid website name.", ZentityCaption);
                    return ActionResult.Success;
                }

                bool invalidWebsiteName = false;
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
                    MessageBox.Show("Zentity Website name contains invalid and/or special characters.\nPlease enter a valid website name.", ZentityCaption);
                    return ActionResult.Success;
                }
            }

            if (string.IsNullOrEmpty(machineName))
            {
                MessageBox.Show("Zentity server name cannot be empty.\nPlease enter a valid server name.", ZentityCaption);
                return ActionResult.Success;
            }
            else
            {
                if (machineName.Length > 255)
                {
                    MessageBox.Show("Zentity server name cannot be longer than 255 characters.", ZentityCaption);
                    return ActionResult.Success;
                }
            }

            long zentityPortNumber = 0;
            if (string.IsNullOrEmpty(portNumber))
            {
                MessageBox.Show("Please enter a valid port number for Zentity website.", ZentityCaption);
                return ActionResult.Success;
            }
            else
            {
                if (!long.TryParse(portNumber, out zentityPortNumber))
                {
                    MessageBox.Show("Please enter a valid port number for Zentity website.", ZentityCaption);
                    return ActionResult.Success;
                }

                if (zentityPortNumber >= 65536 || zentityPortNumber <= 0)
                {
                    MessageBox.Show("Port number should be greater than 0 and lesser than 65536.\nPlease enter a valid port number for Zentity website.", ZentityCaption);
                    return ActionResult.Success;
                }
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.UseDefaultCredentials = true;
                    //try connecting the publishing service
                    using (Stream data = client.OpenRead(string.Format("http://{0}:{1}/Pivot/PublishingService?wsdl", machineName, zentityPortNumber)))
                    {
                        data.Close();
                        data.Dispose();
                    }

                    client.Dispose();
                }
            }
            catch (System.Net.WebException webException)
            {   
                if (webException.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    // verify machine name
                    MessageBox.Show("Could not resolve the machine name.\nPlease verify the server name with Zentity installed.", ZentityCaption);
                    return ActionResult.Success;
                }

                MessageBox.Show("Could not connect to Zentity Services.\nPlease verify the portnumber or server name with Zentity installed.", ZentityCaption);
                return ActionResult.Success;
            }
            catch (Exception)
            {
                MessageBox.Show("Could not connect to Zentity Services.\nPlease verify the portnumber or server name with Zentity installed.", ZentityCaption);
                return ActionResult.Success;
            }

            session["VALIDZENTITYINPUT"] = "1";
            session.Log("Exiting custom action to validate website details.");
            return ActionResult.Success;

        }
    }
}