// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





using System;
using System.Configuration;

namespace Zentity.Platform
{
    /// <summary>
    /// Provides factory methods that return instances of the configured IAtomPubStoreReader and IAtomPubStoreWriter.
    /// </summary>
    internal class AtomPubStoreFactory
    {
        /// <summary>
        /// Gets the configured IAtomPubStoreReader.
        /// </summary>
        /// <returns>An IAtomPubStoreReader instance.</returns>
        public static IAtomPubStoreReader GetAtomPubStoreReader(string baseAddress)
        {
            return GetInstence<IAtomPubStoreReader>("AtomPubStoreReaderName", baseAddress);
        }

        /// <summary>
        /// Gets the configured IAtomPubStoreWriter.
        /// </summary>
        /// <returns>An IAtomPubStoreWriter instance.</returns>
        public static IAtomPubStoreWriter GetAtomPubStoreWriter(string baseAddress)
        {
            return GetInstence<IAtomPubStoreWriter>("AtomPubStoreWriterName", baseAddress);
        }

        /// <summary>
        /// Gets the configured IAtomPubStoreWriter.
        /// </summary>
        /// <returns>An IAtomPubStoreWriter instance.</returns>
        public static IAtomPubStoreWriter GetSwordStoreWriter()
        {
            return GetInstence<IAtomPubStoreWriter>("SwordStoreWriterName");
        }

        /// <summary>
        /// Get the instance of IAtomPubStoreWriter or IAtomPubStoreReader.
        /// </summary>
        /// <typeparam name="T">Zentity Store type</typeparam>
        /// <param name="typeKey">Key name which is used in configuration file to get the type.</param>
        /// <param name="instanceParams">Custructor parameters of Store type.</param>
        /// <returns>Zentity Store type instance.</returns>
        private static T GetInstence<T>(string typeKey, params object[] instanceParams) where T : class
        {
            string storeName = ConfigurationManager.AppSettings[typeKey];
            Type writerType = Type.GetType(storeName, false);

            if(null != writerType)
            {
                return Activator.CreateInstance(writerType, instanceParams) as T;
            }
            else
            {
                return null;
            }
        }
    }
}
