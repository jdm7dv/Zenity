// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    
    internal abstract class ImageBase : ImageProviderBase
    {
        public ImageBase()
        {
        }

        #region ImageProviderBase Members

        public override Size Size 
        {
            get 
            {
                EnsureIsSize();
                return m_size;
            }
        }

        public override void Draw(Graphics g, Rectangle itemRectangle, int level)
        {
            if(!EnsureIsLoaded())
            {
                return;
            }
            
            using (MemoryStream stream = new MemoryStream(m_imageData))
            using (Image image = Image.FromStream(stream))
            {
                Size scaledSize = ScaleToFillSize(image.Size, itemRectangle.Size);
                g.DrawImage(image, itemRectangle.X, itemRectangle.Y, scaledSize.Width, scaledSize.Height);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// If the image is a constant size, you may override this to set the size directly.
        /// </summary>
        protected virtual void EnsureIsSize()
        {
            EnsureIsLoaded();
        }

        protected virtual bool EnsureIsLoaded()
        {
            if (null == m_imageData)
            {
                using (Image image = MakeImage())
                {
                    if (image == null)
                    {
                        return false;
                    }
                    m_size = ScaleToFillSize(image.Size, GetTileSize());
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Save(stream, ImageFormat.Jpeg);
                        m_imageData = stream.ToArray();
                    }
                }
            }

            return true;
        }

        protected Size GetTileSize()
        {
            return new Size(256, 256);
        }

        protected abstract Image MakeImage();

        #endregion

        #region Private Methods

        internal static Size ScaleToFillSize(Size size, Size maxSize)
        {
            Size newSize = new Size();
            double aspectRatio = ((double)size.Width) / size.Height;
            if (aspectRatio > 1.0)
            {
                newSize.Width = maxSize.Width;
                newSize.Height = (int)((double)newSize.Width / aspectRatio);
            }
            else
            {
                newSize.Height = maxSize.Height;
                newSize.Width = (int)(newSize.Height * aspectRatio);
            }
            return newSize;
        }

        #endregion

        #region Private Fields
        
        Size m_size;
        byte[] m_imageData;

        #endregion
    }

}
