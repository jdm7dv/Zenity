// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Globalization;
using System.IO;
using Zentity.Platform.Properties;
using Zentity.Zip;

namespace Zentity.Platform
{
    internal static class ZipExtractor
    {
        #region Methods

        /// <summary>
        /// Unzips the Zip File contents. The zip file contents are extracted in the temp folder.
        /// </summary>
        /// <param name="zipFilePath">Path to the Zip file to be extracted.</param>
        /// <returns>Path to the directory where zip file contents are extracted.</returns>
        public static string UnzipFileContents(string zipFilePath)
        {
            // Get path to the default extraction location i.e. temp folder.
            string extractionLocation = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);

            return UnzipFileContents(zipFilePath, extractionLocation);
        }

        /// <summary>
        /// Unzips the Zip File contents.
        /// </summary>
        /// <param name="zipFilePath">Path to the Zip file to be extracted.</param>
        /// <param name="extractionLocation">Path to the folder where zip contents should be extracted.</param>
        /// <returns>Path to the directory where zip file contents are extracted.</returns>
        public static string UnzipFileContents(string zipFilePath, string extractionLocation)
        {
            if(string.IsNullOrEmpty(zipFilePath))
            {
                throw new ArgumentNullException("zipFilePath");
            }

            if(!System.IO.File.Exists(zipFilePath))
            {
                throw new ArgumentException(Resources.SWORD_FILE_PATH, "zipFilePath");
            }

            Stream inputStream = null;
            try
            {
                inputStream = System.IO.File.OpenRead(zipFilePath);

                string resourcePath = UnzipFileContents(inputStream, extractionLocation);

                return resourcePath;
            }
            finally
            {
                if(null != inputStream)
                {
                    inputStream.Close();
                }
            }
        }

        /// <summary>
        /// Unzips the ZIP file content. The zip file contents are extracted in the temp folder.
        /// </summary>
        /// <param name="zipFileStream">An input stream for the zip file.</param>
        /// <returns>Path to the directory where zip file contents are extracted.</returns>
        public static string UnzipFileContents(Stream zipFileStream)
        {
            // Get path to the default extraction location i.e. temp folder.
            string extractionLocation = System.IO.Path.GetTempPath();

            return UnzipFileContents(zipFileStream, extractionLocation);
        }

        /// <summary>
        /// Unzips the ZIP file content. The zip file contents are extracted in the temp folder.
        /// </summary>
        /// <param name="zipFileStream">An input stream for the zip file.</param>
        /// <param name="extractionLocation">The location at which zip file contents should be extracted.</param>
        /// <returns>Path to the directory where zip file contents are extracted.</returns>
        public static string UnzipFileContents(Stream zipFileStream, string extractionLocation)
        {
            if(null == zipFileStream)
            {
                throw new ArgumentNullException("zipFileStream");
            }

            if(!zipFileStream.CanRead)
            {
                throw new ArgumentException(Properties.Resources.SWORD_INVALID_INPUT_STREAM);
            }

            if(string.IsNullOrEmpty(extractionLocation))
            {
                throw new ArgumentNullException("extractionLocation");
            }

            if(!System.IO.Directory.Exists(extractionLocation))
            {
                throw new ArgumentException(Resources.SWORD_DIRECTORY_PATH, "extractionLocation");
            }

            // Append "\" if it is not there
            if(!extractionLocation.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                extractionLocation = extractionLocation + "\\";
            }

            string actualExtractionPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}",
                                               extractionLocation,
                                               Guid.NewGuid().ToString());

            // Unzip only file content from ZIP file to a new folder.
            Directory.CreateDirectory(actualExtractionPath);


            zipFileStream.Position = 0;

            // Unzip the file contents to specified extractionLocation
            ZipArchive archive = new ZipArchive(zipFileStream, FileAccess.Read);
            archive.CopyToDirectory("", actualExtractionPath);
            // archive.Close(); Don't close this, as it will close the input 'zipFileStream' as well.

            return actualExtractionPath;
        }

        #endregion
    }
}
