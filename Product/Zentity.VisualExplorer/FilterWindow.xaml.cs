// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.VisualExplorer
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Zentity.VisualExplorer.ColorPicker;

    /// <summary>
    /// FilterCheckBoxClicked delegate for Clicked event
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="args">Event data for Clicked event</param>
    public delegate void FilterCheckBoxClicked(object sender, RoutedEventArgs args);

    /// <summary>
    /// Code behind for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : ChildWindow
    {
        /// <summary>
        /// Initializes a new instance of the FilterWindow class
        /// </summary>
        public FilterWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Relations CheckBox Clicked event
        /// </summary>
        public event FilterCheckBoxClicked FilterRelationsCheckBoxClicked;

        /// <summary>
        /// ResourceTypes CheckBox Clicked event
        /// </summary>
        public event FilterCheckBoxClicked FilterEntityCheckBoxClicked;

        /// <summary>
        /// Relations CheckBox Clicked event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Clicked event</param>
        private void OnRelationsCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (this.FilterRelationsCheckBoxClicked != null)
            {
                this.FilterRelationsCheckBoxClicked(sender, e);
            }
        }

        /// <summary>
        /// ResourceTypes CheckBox Clicked event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for Clicked event</param>
        private void OnEntityCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (this.FilterEntityCheckBoxClicked != null)
            {
                this.FilterEntityCheckBoxClicked(sender, e);
            }
        }

        /// <summary>
        /// ColorPicker SelectionChanged event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event data for SelectionChanged event</param>
        private void OnColorPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string color = ((Item)e.AddedItems[0]).Colors.Substring(1, 8);
            byte a = System.Convert.ToByte(color.Substring(0, 2), 16);
            byte r = System.Convert.ToByte(color.Substring(2, 2), 16);
            byte g = System.Convert.ToByte(color.Substring(4, 2), 16);
            byte b = System.Convert.ToByte(color.Substring(6, 2), 16);

            ((ComboBox)sender).Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }
    }
}

