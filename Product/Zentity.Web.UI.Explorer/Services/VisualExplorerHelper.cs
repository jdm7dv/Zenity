// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Explorer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Linq;
    using GuanxiUI;
    using Zentity.Core;
    using Zentity.Services.Web;

    /// <summary>
    /// Helper method to get VisualExplorer graph and related metadata
    /// </summary>
    public class VisualExplorerHelper
    {
        /// <summary>
        /// Maximum number of levels to be traversed
        /// </summary>
        private static int MAXDEPTHCOUNTER = int.Parse(ConfigurationManager.AppSettings["MaxRelationDepth"]);
        
        /// <summary>
        /// Maximum number of subject nodes
        /// </summary>
        private static int MAXSUBJECTNODES = int.Parse(ConfigurationManager.AppSettings["MaxSubjectNodeCount"]);
        
        /// <summary>
        /// Maximum number of object nodes
        /// </summary>
        private static int MAXOBJECTNODES = int.Parse(ConfigurationManager.AppSettings["MaxObjectNodeCount"]);

        /// <summary>
        /// ZentityContext object to connect to Zentity store
        /// </summary>
        private ZentityContext context;
        
        /// <summary>
        /// VisualExplorerGraph object
        /// </summary>
        private VisualExplorerGraph graph;
        
        /// <summary>
        /// Edges in the graph
        /// </summary>
        private Collection<Edge> edges;
        
        /// <summary>
        /// Nodes in the graph
        /// </summary>
        private Collection<Node> nodes;

        /// <summary>
        /// Dictionary of unique nodes in the graph
        /// </summary>
        private Dictionary<Guid, string> uniqueNodes;
        
        /// <summary>
        /// List of ResourceTypes in the graph
        /// </summary>
        private List<string> resourceTypes = new List<string>();
        
        /// <summary>
        /// Collection of unique edges in the graph
        /// </summary>
        private Collection<string> uniqueEdges;
        
        /// <summary>
        /// Number of subject nodes
        /// </summary>
        private uint subjectNodeCount = 0;
        
        /// <summary>
        /// Number of object nodes
        /// </summary>
        private uint objectNodeCount = 0;

        /// <summary>
        /// Gets VisualExplorer graph by ResourceId
        /// </summary>
        /// <param name="id">ResourceId from Zentity store</param>
        /// <returns>VisualExplorerGraph object</returns>
        public VisualExplorerGraph GetVisualExplorerGraphByResourceId(string id)
        {
            Guid resourceId;
            if (Guid.TryParse(id, out resourceId))
            {
                try
                {
                    using (this.context = Zentity.Services.Web.Utilities.CreateZentityContext())
                    {
                        // 1. Get the resource with the specified identifier
                        Resource resource = this.context.Resources.Where(tuple => tuple.Id == resourceId).First();
                        this.graph = new VisualExplorerGraph();
                        this.graph.JSONGraph.query = resource.Title;
                        this.edges = new Collection<Edge>();
                        this.nodes = new Collection<Node>();
                        this.uniqueNodes = new Dictionary<Guid, string>(MAXOBJECTNODES + MAXSUBJECTNODES);
                        this.uniqueEdges = new Collection<string>();

                        Node node = new Node();
                        node.Id = resource.Id.ToString();
                        node.Name = resource.Title;
                        node.DisplayName = resource.Title;
                        node.Scale = 1.2;
                        this.uniqueNodes.Add(resource.Id, resource.GetType().ToString());
                        this.resourceTypes.Add(resource.GetType().ToString());

                        this.GetSubjectGraphForResource(resource, 0);
                        this.GetObjectGraphForResource(resource, 0);
                        this.GetGraphBetweenNodes(this.nodes);

                        // the root node has to be added last.
                        this.nodes.Add(node);
                        this.graph.ResourceMap = this.uniqueNodes;

                        NodeComparer nodeComparer = new NodeComparer();
                        Collection<Node> resultNodes = new Collection<Node>(this.nodes.Distinct(nodeComparer).ToList());
                        this.graph.JSONGraph.Nodes = resultNodes;
                        if (this.edges.Count == 0)
                        {
                            this.graph.JSONGraph.Edges = null;
                        }
                        else
                        {
                            EdgeComparer edgeComparer = new EdgeComparer();
                            Collection<Edge> resultEdges = new Collection<Edge>(this.edges.Distinct(edgeComparer).ToList());
                            this.graph.JSONGraph.Edges = resultEdges;
                        }

                        this.graph.JSONGraph.layout = false;                        
                        return this.graph;
                    }
                }
                catch (Exception exception)
                {
                    Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// Get metadata for the specified ResourceId
        /// </summary>
        /// <param name="id">ResourceId from Zentity store</param>
        /// <returns>Resource metadata</returns>
        public Dictionary<string, string> GetResourceMetadataByResourceId(string id)
        {
            Guid resourceId;
            if (Guid.TryParse(id, out resourceId))
            {
                try
                {
                    using (this.context = Zentity.Services.Web.Utilities.CreateZentityContext())
                    {
                        // 1. Get the resource with the specified identifier
                        Resource resource = this.context.Resources.Where(tuple => tuple.Id == resourceId).First();
                        Dictionary<string, string> metadata = new Dictionary<string, string>();
                        this.context.LoadProperty(resource, "ResourceProperties");
                        metadata.Add("Title", resource.Title);
                        metadata.Add("ResourceType", resource.GetType().ToString());
                        metadata.Add("LastUpdated", resource.DateAdded.ToString());
                        metadata.Add("DateModified", resource.DateModified.ToString());
                        metadata.Add("Description", resource.Description);
                        metadata.Add("Uri", resource.Uri);
                        metadata.Add("Id", id);
                        return metadata;
                    }
                }
                catch (Exception exception)
                {
                    Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// Get Resource relation by ResourceId
        /// </summary>
        /// <param name="subjectResourceId">Identifier of the subject resource</param>
        /// <param name="objectResourceId">Identifier of the object resource</param>
        /// <returns>Resource relation metadata</returns>
        public List<string> GetResourceRelationByResourceId(string subjectResourceId, string objectResourceId)
        {
            Guid subjectId, objectId;
            if (Guid.TryParse(subjectResourceId, out subjectId) && Guid.TryParse(objectResourceId, out objectId))
            {
                try
                {
                    using (this.context = Zentity.Services.Web.Utilities.CreateZentityContext())
                    {
                        // 1. Get the resource with the specified identifier
                        Resource subjectResource = this.context.Resources.Where(tuple => tuple.Id == subjectId).First();
                        this.context.LoadProperty(subjectResource, "RelationshipsAsSubject");
                        EntityCollection<Relationship> objectRelations = subjectResource.RelationshipsAsSubject;
                        List<string> result = new List<string>();
                        result.Add(subjectResource.Title);

                        foreach (Relationship relation in objectRelations)
                        {
                            this.context.LoadProperty(relation, "Object");
                            this.context.LoadProperty(relation, "Predicate");

                            if (relation.Object.Id == objectId)
                            {
                                Resource objectResource = this.context.Resources.Where(tuple => tuple.Id == relation.Object.Id).First();
                                result.Add(relation.Predicate.Name);
                                result.Add(objectResource.Title);
                                break;
                            }
                        }

                        return result;
                    }
                }
                catch (Exception exception)
                {
                    Globals.TraceMessage(TraceEventType.Error, exception.ToString(), exception.Message);
                }
            }

            return null;
        }
        
        /// <summary>
        /// Helper method to construct graph for nodes related as subject
        /// </summary>
        /// <param name="resource">Resource for which subjects are to be found</param>
        /// <param name="level">Depth level of graph</param>
        private void GetSubjectGraphForResource(Resource resource, int level)
        {
            // 2. Get all the navigation properties of the resource for a depth level configured
            if (level < MAXDEPTHCOUNTER)
            {
                this.context.LoadProperty(resource, "RelationshipsAsSubject");
                EntityCollection<Relationship> objectRelations = resource.RelationshipsAsSubject;
                foreach (Relationship relation in objectRelations)
                {
                    bool duplicateResource = false;

                    this.context.LoadProperty(relation, "Object");
                    this.context.LoadProperty(relation, "Predicate");
                    Resource resourceItem = this.context.Resources.Where(tuple => tuple.Id == relation.Object.Id).First();

                    // 3. create  a Node & Edge for the 2 nodes
                    Node node = new Node();
                    node.Id = resourceItem.Id.ToString();
                    node.Name = resourceItem.Title;
                    node.DisplayName = resourceItem.Title;
                    node.Scale = 1.0 - (0.5 * ((level + 1) / MAXDEPTHCOUNTER));
                    try
                    {
                        this.uniqueNodes.Add(resourceItem.Id, resourceItem.GetType().ToString());
                        this.nodes.Add(node);
                        this.resourceTypes.Add(resourceItem.GetType().ToString());
                    }
                    catch (ArgumentException)
                    {
                        duplicateResource = true;
                    }

                    Edge edge = new Edge();
                    edge.IsArrow = false;                    
                    edge.Node1 = resource.Id.ToString();
                    edge.Node2 = resourceItem.Id.ToString();
                    edge.Str = 1.0 - (0.5 * ((level + 1) / MAXDEPTHCOUNTER));
                    edge.Desc = relation.Predicate.Name;
                    this.edges.Add(edge);

                    this.subjectNodeCount += 1;
                    if (this.subjectNodeCount >= MAXSUBJECTNODES)
                    {
                        break;
                    }

                    // 4. Get all the navigation properties of the child with the designated level by calling self
                    if (!duplicateResource)
                    {
                        this.GetSubjectGraphForResource(resourceItem, level + 1);
                        this.GetObjectGraphForResource(resourceItem, level + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to construct graph for nodes related as Object
        /// </summary>
        /// <param name="resource">Resource for which objects are to be found</param>
        /// <param name="level">Depth level of graph</param>
        private void GetObjectGraphForResource(Resource resource, int level)
        {
            // 2. Get all the navigation properties of the resource for a depth level configured
            if (level < MAXDEPTHCOUNTER)
            {
                resource.RelationshipsAsObject.Load();
                foreach (Relationship relation in resource.RelationshipsAsObject)
                {
                    bool duplicateResource = false;

                    this.context.LoadProperty(relation, "Subject");
                    this.context.LoadProperty(relation, "Predicate");
                    Resource resourceItem = this.context.Resources.Where(tuple => tuple.Id == relation.Subject.Id).First();

                    // 3. create  a Node & Edge for the 2 nodes
                    Node node = new Node();
                    node.Id = resourceItem.Id.ToString();
                    node.Name = resourceItem.Title;
                    node.DisplayName = resourceItem.Title;
                    node.Scale = 1.0 - (0.5 * ((level + 1) / MAXDEPTHCOUNTER));

                    try
                    {
                        this.uniqueNodes.Add(resourceItem.Id, resourceItem.GetType().ToString());
                        this.nodes.Add(node);
                        this.resourceTypes.Add(resourceItem.GetType().ToString());
                    }
                    catch (ArgumentException)
                    {
                        duplicateResource = true;
                    }

                    Edge edge = new Edge();
                    edge.IsArrow = false;
                    edge.Node1 = resourceItem.Id.ToString();
                    edge.Node2 = resource.Id.ToString();
                    edge.Str = 1.0 - (0.5 * ((level + 1) / MAXDEPTHCOUNTER));
                    edge.Desc = relation.Predicate.Name;
                    this.edges.Add(edge);

                    this.objectNodeCount += 1;
                    if (this.objectNodeCount >= MAXOBJECTNODES)
                    {
                        break;
                    }

                    // 4. Get all the navigation properties of the child with the designated level by calling self
                    if (!duplicateResource)
                    {
                        this.GetSubjectGraphForResource(resourceItem, level + 1);
                        this.GetObjectGraphForResource(resourceItem, level + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to get graph between the nodes in the graph
        /// </summary>
        /// <param name="nodeCollection">List of nodes in graph</param>
        private void GetGraphBetweenNodes(IEnumerable<Node> nodeCollection)
        {
            if (nodeCollection == null)
            {
                return;
            }

            IEnumerable<Guid> nodeIds = from node in nodeCollection select Guid.Parse(node.Id);

            foreach (Node node in nodeCollection)
            {
                try
                {
                    Guid nodeId = Guid.Parse(node.Id);
                    IEnumerable<Relationship> relations = this.context.Relationships.Where(tuple => tuple.Subject.Id == nodeId
                                                                                && nodeIds.Contains(tuple.Object.Id));
                    if (relations != null && relations.Count() > 0)
                    {
                        foreach (Relationship relation in relations)
                        {
                            Edge edge = new Edge();
                            edge.IsArrow = false;
                            edge.Node1 = relation.Subject.Id.ToString();
                            edge.Node2 = relation.Object.Id.ToString();
                            edge.Str = 0.5;

                            if (relation.Predicate != null)
                            {
                                edge.Desc = relation.Predicate.Name;
                            }

                            this.edges.Add(edge);
                        }
                    }
                }
                catch (Exception)
                {
                    // absorbing all exceptions here since, entity framework will not be to load properties sometimes. 
                    // ex: relation.Predicate will be null or EntityCommandExecutionException will be thrown.
                }
            }
        }
    }

    /// <summary>
    /// VisualExplorer graph
    /// </summary>
    public class VisualExplorerGraph
    {
        /// <summary>
        /// Initializes a new instance of the VisualExplorerGraph class
        /// </summary>
        public VisualExplorerGraph()
        {
            this.JSONGraph = new JSONGraph();
            this.ResourceMap = new Dictionary<Guid, string>();
        }

        /// <summary>
        /// Gets or sets Dictionary of Resources and ResourceTypes
        /// </summary>
        public Dictionary<Guid, string> ResourceMap { get; set; }

        /// <summary>
        /// Gets or sets JSONGraph object
        /// </summary>
        public JSONGraph JSONGraph { get; set; }
    }

    /// <summary>
    /// Defines methods to support the comparison of Edges for equality
    /// </summary>
    internal class EdgeComparer : IEqualityComparer<Edge>
    {
        /// <summary>
        /// Determines whether the specified Edges are equal
        /// </summary>
        /// <param name="x">The first object of type Edge to compare</param>
        /// <param name="y">The second object of type Edge to compare</param>
        /// <returns> true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(Edge x, Edge y)
        {
            if (x != null && y != null &&
                x.Node1 == y.Node1 &&
                x.Node2 == y.Node2)
            {
                return true;
            }

            if (x != null && y != null &&
                x.Node1 == y.Node2 &&
                x.Node2 == y.Node1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a Edge type.
        /// </summary>
        /// <param name="obj">Edge object</param>
        /// <returns>A hash code for the current Edge</returns>
        public int GetHashCode(Edge obj)
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Defines methods to support the comparison of Nodes for equality
    /// </summary>
    internal class NodeComparer : IEqualityComparer<Node>
    {
        /// <summary>
        /// Determines whether the specified Nodes are equal
        /// </summary>
        /// <param name="x">The first object of type Node to compare</param>
        /// <param name="y">The second object of type Node to compare</param>
        /// <returns> true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(Node x, Node y)
        {
            if (x != null && y != null &&
                x.Id == y.Id)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a Node type.
        /// </summary>
        /// <param name="obj">Node object</param>
        /// <returns>A hash code for the current Node</returns>
        public int GetHashCode(Node obj)
        {
            return base.GetHashCode();
        }
    }
}
