// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System.ServiceModel.Syndication;

namespace Zentity.Platform
{
    /// <summary>
    /// Interface exposing the functions specific to Store add, update and delete.
    /// </summary>
    public interface IAtomPubStoreWriter
    {
        /// <summary>
        /// Creates a member resource in the specified collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection in which the member resource should be created.</param>
        /// <param name="atomEntry">An AtomEntryDocument that contains information about the member resource that should be created.</param>
        /// <returns>A SyndicationItem corresponding to the member resource that is created.</returns>
        SyndicationItem CreateMember(string collectionName, AtomEntryDocument atomEntry);

        /// <summary>
        /// Creates a new Resource.File for a specified resource of type collectionName in the repository.
        /// </summary>
        /// <param name="collectionName">The resource type.</param>
        /// <param name="mimeType">The MIME type of media.</param>
        /// <param name="media">The new File contents.</param>
        /// <param name="fileExtension">The media file extension.</param>
        /// <returns>A SyndicationItem that describes the newly created resource.</returns>
        SyndicationItem CreateMedia(string collectionName, string mimeType, byte[] media, string fileExtension);

        /// <summary>
        /// Updates the information for a member resource.
        /// </summary>
        /// <param name="collectionName">The type of the resource.</param>
        /// <param name="memberResourceId">The Id of the member resource.</param>
        /// <param name="atomEntry">An AtomEntryDocument that contains information about the member resource that should be updated.</param>
        /// <returns>The updated SyndicationItem.</returns>
        SyndicationItem UpdateMemberInfo(string collectionName, string memberResourceId, AtomEntryDocument atomEntry);

        /// <summary>
        /// Updates the media for a member resource.
        /// </summary>
        /// <param name="collectionName">The type of the resource.</param>
        /// <param name="memberResourceId">The Id of the member resource.</param>
        /// <param name="mimeType">The MIME type of media.</param>
        /// <param name="media">The new media.</param>
        /// <returns>The updated SyndicationItem.</returns>
        SyndicationItem UpdateMedia(string collectionName, string memberResourceId, string mimeType, byte[] media);

        /// <summary>
        /// Deletes a member resource from the specified collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection from which the member resource is to be deleted.</param>
        /// <param name="memberResourceId">The Id of the member resource that is to be deleted.</param>
        /// <returns>True if the operation succeeds, False otherwise.</returns>
        bool DeleteMember(string collectionName, string memberResourceId);

        /// <summary>
        /// Deletes the media resource from a member resource.
        /// </summary>
        /// <param name="collectionName">Represents the name of the requested collection.</param>
        /// <param name="memberResourceId">The Id of the member resource from which the media resource is to be deleted.</param>        
        /// <returns>True if the operation succeeds, False otherwise.</returns>
        bool DeleteMedia(string collectionName, string memberResourceId);
    }
}
