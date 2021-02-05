// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Rdf.Concepts
{
    /// <summary>
    /// This abstract class represents a node that can be a subject 
    /// (http://www.w3.org/TR/2004/REC-rdf-concepts-20040210/#dfn-subject) of a triple. 
    /// Take note that literals cannot be a subject of a triple.
    /// </summary>
    public abstract class Subject:Node
    {
    }
}
