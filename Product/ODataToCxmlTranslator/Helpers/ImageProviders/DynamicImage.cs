// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal class DynamicImage : ImageBase
    {
        #region Constructors, Finalizer and Dispose

        public DynamicImage(string name)
        {
            m_title = name;
        }

        #endregion

        #region Protected Methods

        protected override Image MakeImage()
        {
            return DrawBitmap(imageSize_c.Width, imageSize_c.Height, m_title);
        }

        #endregion

        #region Private Methods

        private static Bitmap DrawBitmap(int width, int height, string title)
        {
            Bitmap bitmap = new Bitmap(width, height);
            try
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    Rectangle rect = new Rectangle(0, 0, width, height);
                    DrawBackground(g, rect);
                    DrawContent(g, rect, title);
                }
            }
            catch
            {
                bitmap.Dispose();
                bitmap = null;
            }
            return bitmap;
        }

        private static void DrawBackground(Graphics g, Rectangle rect)
        {
            g.FillRectangle(Brushes.WhiteSmoke, rect);
        }

        private static void DrawContent(Graphics g, Rectangle rect, string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                using (Font titleFont = new Font(FontFamily.GenericSansSerif, 20.0f, FontStyle.Regular))
                {
                    // Get space occupied by title.
                    SizeF titleSize = g.MeasureString(title, titleFont, rect.Width);

                    Rectangle headerRect = new Rectangle(0, 0, rect.Width, (int)titleSize.Height + 5);
                    g.FillRectangle(Brushes.Gray, headerRect);
                    StringFormat stringFormat = new StringFormat(StringFormatFlags.LineLimit) { Trimming = StringTrimming.EllipsisWord };
                    g.DrawString(title, titleFont, Brushes.White, rect, stringFormat);
                    
                    int fotterRectHeight = rect.Height * 15 / 100;
                    if (titleSize.Height < rect.Height - fotterRectHeight)
                    {
                        // Draw fotter
                        Rectangle fotterRectangle = new Rectangle(0, rect.Height - fotterRectHeight, rect.Width, fotterRectHeight);
                        g.FillRectangle(Brushes.LightGray, fotterRectangle);
                    }
                }
            }
        }

        #endregion

        #region Private Fields

        static readonly Size imageSize_c = new Size(300, 300);

        string m_title;

        #endregion
    }
}
