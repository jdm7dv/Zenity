// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    internal class FacetCategory
    {
        #region Constructors, Finalizer and Dispose

        public FacetCategory(string name, FacetType type)
        {
            this.Name = name;
            this.FacetType = type;

            this.IsShowInFacetPane = true;
            this.IsShowInInfoPane = true;
            this.IsTextFilter = true;
        }

        #endregion

        #region Public Properties

        public string Name { get; private set; }
        public FacetType FacetType { get; set; }
        public string DisplayFormat { get; set; }
        public bool IsShowInFacetPane { get; set; }
        public bool IsShowInInfoPane { get; set; }
        public bool IsTextFilter { get; set; }

        #endregion
    }
}
