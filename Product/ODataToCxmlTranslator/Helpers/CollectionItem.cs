// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System.Collections.Generic;

    internal class CollectionItem
    {
        #region Constructors, Finalizer and Dispose

        public CollectionItem()
        {
        }

        #endregion

        #region Public Properties
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public ImageProviderBase ImageProvider { get; private set; }
        public ICollection<Facet> FacetValues { get; set; }

        #endregion

        #region Public Methods

        public void SetImage(ItemImage image)
        {
            this.ImageProvider = new DynamicImage(this.Name);
        }

        #endregion
    }
}
