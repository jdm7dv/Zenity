// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System.Drawing;

    internal abstract class ImageProviderBase
    {
        /// <summary>
        /// Return the size of the image
        /// </summary>
        public abstract System.Drawing.Size Size { get; }

        /// <summary>
        /// Draw the image into the given rectangle in the graphics context.
        /// Use the level parameter to draw different visuals for different levels, if desired.
        /// </summary>
        public abstract void Draw(Graphics g, Rectangle itemRectangle, int level);
    }

}
