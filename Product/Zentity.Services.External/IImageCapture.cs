// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.External
{
    /// <summary>
    /// Provides the interface for custom implmentation of WebCapture using any third party library.
    /// </summary>
    public interface IImageCapture : System.IDisposable
    {
        /// <summary>
        /// Generates Custom Image.
        /// </summary>
        /// <param name="propertyValue">Property Vaue is fetched from the database for a particualar resource.</param>
        /// <param name="width">The width of the generated image.</param>
        /// <param name="height">The height of the generated image.</param>
        /// <returns>The path where the image is generated.</returns>
        string GenerateImage(object propertyValue, int width, int height);

        /// <summary>
        /// Gets or sets the working folder where all the images are generated.
        /// </summary>
        /// <value>The working folder path.</value>
        string WorkingDirectory
        {
            get; set;
        }
    }
}
