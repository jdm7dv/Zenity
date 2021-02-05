// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;

    public class ItemImage
    {
        #region Constructors, Finalizer and Dispose

        public ItemImage()
        {
        }

        public ItemImage(string imageFilePath)
        {
            this.ImageFilePath = imageFilePath;
        }

        public ItemImage(Uri imageUrl)
        {
            this.ImageUrl = imageUrl;
        }

        #endregion

        #region Public Properties

        public string ImageFilePath { get; set; }
        public Uri ImageUrl { get; set; }

        #endregion
    }
}
