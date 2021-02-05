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
    /// http://www.w3.org/TR/2004/REC-rdf-syntax-grammar-20040210/#nodeElementList production.
    /// </summary>
    internal sealed class ProductionNodeElementList : Production
    {
        #region Member Variables

        #region Private
        IEnumerable<EventElement> innerList;
        #endregion

        #endregion

        #region Constructor

        #region Internal
        /// <summary>
        /// Initializes a new instance of the ProductionNodeElementList class. 
        /// The constructor takes a collection of EventElement objects to initialize itself.
        /// </summary>
        /// <param name="nodeElementList">IEnumerable&lt;EventElement&gt; object to precess.</param>
        internal ProductionNodeElementList(IEnumerable<EventElement> nodeElementList)
        {
            if (nodeElementList == null)
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture,
                    Resources.MsgNullArgument, "nodeElementList"));

            this.innerList = nodeElementList;
        }
        #endregion

        #endregion

        #region Methods

        #region Internal
        /// <summary>
        /// Validates node element list based on following syntax rules, 
        /// <br/>
        /// ws* (nodeElement ws* )*
        /// </summary>
        /// <param name="outputGraph">Graph to add the generated triples.</param>
        internal override void Match(Graph outputGraph)
        {
            foreach (EventElement nodeElement in this.innerList)
            {
                ProductionNodeElement productionNodeElement =
                    new ProductionNodeElement(nodeElement);

                productionNodeElement.Match(outputGraph);
            }
        }
        #endregion

        #endregion
    }
}
