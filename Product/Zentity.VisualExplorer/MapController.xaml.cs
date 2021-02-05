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
    /// Code behing of MapController.xaml
    /// </summary>
    public partial class MapController : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MapController class
        /// </summary>
        public MapController()
        {
            this.InitializeComponent();
            this.arrowBtn.MouseEnter += new MouseEventHandler(this.OnArrowBtnMouseEnter);
            this.arrowBtn.MouseLeave += new MouseEventHandler(this.OnArrowBtnMouseLeave);
            this.btn_Close.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseBtnMouseLeftButtonUp);
            this.btn_Open.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenBtnMouseLeftButtonUp);
        }

        /// <summary>
        /// MouseEnter event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseEnter event</param>
        private void OnArrowBtnMouseEnter(object sender, MouseEventArgs e)
        {
            Canvas.SetTop(this.arrowImage, -49.0);
        }

        /// <summary>
        /// MouseLeave event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeave event</param>
        private void OnArrowBtnMouseLeave(object sender, MouseEventArgs e)
        {
            Canvas.SetTop(this.arrowImage, 0.0);
        }

        /// <summary>
        /// Open Button MouseLeftButtonUp event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonUp event</param>
        private void OnOpenBtnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.pnlOpen.Visibility = Visibility.Visible;
            this.pnlClose.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Close Button MouseLeftButtonUp event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for MouseLeftButtonUp event</param>
        private void OnCloseBtnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.pnlOpen.Visibility = Visibility.Collapsed;
            this.pnlClose.Visibility = Visibility.Visible;
        }
    }
}
