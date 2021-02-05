namespace Zentity.Services.ServiceHost
{
    using System;

    partial class WebServiceHost
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                if (adminConfigServiceHost != null)
                    (adminConfigServiceHost as IDisposable).Dispose();
                if (configurationServiceHost != null)
                    (configurationServiceHost as IDisposable).Dispose();
                if (publishingServiceHost != null)
                    (publishingServiceHost as IDisposable).Dispose();
                if (dataModelServiceHost != null)
                    (dataModelServiceHost as IDisposable).Dispose();
                if (resourceTypeServiceHost != null)
                    (resourceTypeServiceHost as IDisposable).Dispose();
                if (publishingProgressServiceHost != null)
                    (publishingProgressServiceHost as IDisposable).Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ServiceName = "Zentity Publishing Service";
        }

        #endregion
    }
}
