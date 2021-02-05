// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Code behind for NoResultPopUpWindow.xaml
    /// </summary>
    public partial class NoResultPopUpWindow : UserControl
    {
        /// <summary>
        ///  Initializes a new instance of the NoResultPopUpWindow class
        /// </summary>
        public NoResultPopUpWindow()
        {
            this.InitializeComponent();         
        }

        /// <summary>
        /// Close Button click event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Click event</param>
        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
