// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.VisualExplorer.ColorPicker
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    // To significantly reduce the sample data footprint in your production application, you can set
    // the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class ColorPickerDataSource { }
#else

    /// <summary>
    /// Represents the datasource for the ColorPicker control
    /// </summary>
    public class ColorPickerDataSource : INotifyPropertyChanged
    {
        /// <summary>
        /// Holds the list of cells within ColorPicker control
        /// </summary>
        private ItemCollection collection = new ItemCollection();

        /// <summary>        
        /// Initializes a new instance of the ColorPickerDataSource class.
        /// </summary>
        public ColorPickerDataSource()
        {
            try
            {
                System.Uri resourceUri = new System.Uri("/Zentity.VisualExplorer;component/ColorPicker/ColorPickerDataSource.xaml", System.UriKind.Relative);
                if (System.Windows.Application.GetResourceStream(resourceUri) != null)
                {
                    System.Windows.Application.LoadComponent(this, resourceUri);
                }
            }
            catch (System.Exception)
            {
            }
        }

        /// <summary>
        /// PropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Gets the list of cells within ColorPicker control
        /// </summary>
        public ItemCollection Collection
        {
            get
            {
                return this.collection;
            }
        }

        /// <summary>
        /// PropertyChanged Event handler
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Represents a cell within the ColorPicker control
    /// </summary>
    public class Item : INotifyPropertyChanged
    {
        /// <summary>
        /// Holds the value of the color selected
        /// </summary>
        private string colors = string.Empty;

        /// <summary>
        /// PropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the value of the color selected
        /// </summary>
        public string Colors
        {
            get
            {
                return this.colors;
            }

            set
            {
                if (this.colors != value)
                {
                    this.colors = value;
                    this.OnPropertyChanged("Colors");
                }
            }
        }

        /// <summary>
        /// PropertyChanged event handler
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Collection of cells representing the color picker
    /// </summary>
    public class ItemCollection : ObservableCollection<Item>
    {
    }
#endif
}
