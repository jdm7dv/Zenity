// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Code behind of PopUpWindow.xaml
    /// </summary>
    public partial class PopUpWindow : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the PopUpWindow class
        /// </summary>
        public PopUpWindow()
        {
            this.InitializeComponent();
            this.closeBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.CloseBtn_MouseLeftButtonUp);
            this.KeyUp += new KeyEventHandler(this.OnPopUpWindowKeyUp);
            this.SubjectResourceTitle = string.Empty;
            this.ObjectResourceTitle = string.Empty;
            this.PropertyTitle = string.Empty;
        }
        
        /// <summary>
        /// Gets or sets title of the Subject resource
        /// </summary>
        public string SubjectResourceTitle
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets name of the relationship
        /// </summary>
        public string PropertyTitle
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets title of the Object resource
        /// </summary>
        public string ObjectResourceTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Load text to the labels in PopUpWindow
        /// </summary>
        public void LoadText()
        {
            this.txtobjectResourceTitle.Text = this.ObjectResourceTitle;
            this.txtpropertyTitle.Text = this.PropertyTitle;
            this.txtsubjectResourceTitle.Text = this.SubjectResourceTitle;
        }
        
        /// <summary>
        /// KeyUp Event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data of KeyUp event</param>
        private void OnPopUpWindowKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Visibility = Visibility.Collapsed;
                    break;
            }
        }       

        /// <summary>
        /// MouseLeftButtonUp Event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data of MouseLeftButtonUp event</param>
        private void CloseBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
