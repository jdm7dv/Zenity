// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Zentity.Rdf.Concepts;

    /// <summary>
    /// This class represents the 
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#propertyEltList production.
    /// </summary>
    internal sealed class ProductionPropertyElementList : Production
    {
        #region Member Variables

        #region Private
        IEnumerable<EventElement> innerList;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionPropertyElementList class. 
        /// The constructor takes an Collection of EventElement to initialize itself.
        /// </summary>
        /// <param name="propertyElementlist">IEnumerable&lt;EventElement&gt; object to process.</param>
        internal ProductionPropertyElementList(IEnumerable<EventElement> propertyElementlist)
        {
            if (propertyElementlist == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "propertyElementlist"));

            innerList = propertyElementlist;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Maches production,
        /// <br/>
        /// ws* (nodeElement ws* )*
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            foreach (EventElement nodeElement in this.innerList)
            {
                ProductionPropertyElement productionPropertyElement =
                    new ProductionPropertyElement(nodeElement);

                productionPropertyElement.Match(outputGraph);
            }
        }
        #endregion

        #endregion
    }
}
