// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


using Zentity.Rdf.Concepts;

namespace Zentity.Core
{
    /// <summary>
    /// Defines the class for Rdf Context
    /// </summary>
    internal class Context
    {
        #region Member Variables
        Graph graph;
        ExecutionMode executionMode;
        ResourceType baseResourceType;
        XsdDataTypeCollection xsdDataTypeCollection;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the graph.
        /// </summary>
        /// <value>The graph.</value>
        internal Graph Graph
        {
            get { return graph; }
            set { graph = value; }
        }

        /// <summary>
        /// Gets or sets the XSD data type collection.
        /// </summary>
        /// <value>The XSD data type collection.</value>
        internal XsdDataTypeCollection XsdDataTypeCollection
        {
            get { return xsdDataTypeCollection; }
            set { xsdDataTypeCollection = value; }
        }

        /// <summary>
        /// Gets or sets the execution mode.
        /// </summary>
        /// <value>The execution mode.</value>
        internal ExecutionMode ExecutionMode
        {
            get { return executionMode; }
            set { executionMode = value; }
        }

        /// <summary>
        /// Gets or sets the type of the base resource.
        /// </summary>
        /// <value>The type of the base resource.</value>
        internal ResourceType BaseResourceType
        {
            get { return baseResourceType; }
            set { baseResourceType = value; }
        }

        #endregion
    }
}
