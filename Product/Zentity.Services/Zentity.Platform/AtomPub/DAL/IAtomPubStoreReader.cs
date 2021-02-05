// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System.IO;
using System.ServiceModel.Syndication;

namespace Zentity.Platform
{
    /// <summary>
    /// Interface exposing the functions specific to Store retrieval.
    /// </summary>
    public interface IAtomPubStoreReader
    {
        /// <summary>
        /// Gets the names of the AtomPub collections available in the store.
        /// </summary>
        /// <returns>A list of collection names.</returns>
        string[] GetCollectionNames();

        /// <summary>
        /// Gets the member resources in a collection.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="skip">The number of member resources to skip from the start.</param>
        /// <param name="count">The number of member resources to return.</param>
        /// <returns>A SyndicationFeed of member resources in the collection</returns>
        SyndicationFeed GetMembers(string collectionName, long skip, long count);

        /// <summary>
        /// Gets the number of resources present in a collection.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>Number of members present in the specified collection.</returns>
        long GetMembersCount(string collectionName);

        /// <summary>
        /// Gets a member resource.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">The Id of the member resource.</param>
        /// <returns>A SyndicationItem corresponding to the member resource Id. If no matching member resource is found, null is returned.</returns>
        SyndicationItem GetMember(string collectionName, string memberResourceId);

        /// <summary>
        /// Checks if the member of specified type and id is present in the given collection.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">Id of the member.</param>
        /// <returns>True if member is present in the given collection, else false.</returns>
        bool IsMemberPresent(string collectionName, string memberResourceId);

        /// <summary>
        /// Checks if the member of specified type and id is present in the given collection.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">Id of the member.</param>
        /// <returns>True if media is present for the given member, else false.</returns>
        bool IsMediaPresentForMember(string collectionName, string memberResourceId);

        /// <summary>
        /// Gets a media contents of a member resource.
        /// </summary>
        /// <param name="collectionName">Name of the target collection.</param>
        /// <param name="memberResourceId">The Id of the member resource.</param>
        /// <param name="outStream">A stream containing the media corresponding to the specified resource. If no matching media is found, null is returned.</param>        
        void GetMedia(string collectionName, string memberResourceId, Stream outStream);
    }
}
