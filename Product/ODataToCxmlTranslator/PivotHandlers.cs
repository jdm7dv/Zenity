// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace ODataToCxmlTranslator
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Web;
    using System.Web.Caching;

    public class CxmlHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string src = context.Request.QueryString["src"];

            //Build the collection in response to any query parameters.
            //Collection collection = FileExplorerCollection.MakeCollection(src);
            Collection collection = ODataCollectionSource.MakeOdataCollection(src);

            string cacheKey = Guid.NewGuid().ToString("N") + ".dzc";

            collection.ImgBaseName = cacheKey;
            context.Cache[cacheKey] = collection;
            context.Cache.Add(cacheKey, collection, null, DateTime.MaxValue, new TimeSpan(1, 0, 0),
                              CacheItemPriority.High, null);
            context.Response.ContentType = "text/xml";
            CxmlSerializer.Serialize(context.Response.Output, collection);
        }

        #endregion
    }

    public class DzcHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //Retrieve the collection object from the cache and write it as a DZC.
            string cacheKey = context.Request.Url.Segments[context.Request.Url.Segments.Length - 1];
            Collection collection = (Collection)context.Cache[cacheKey];

            context.Response.ContentType = "text/xml";
            DzcSerializer.Serialize(context.Response.Output, collection);
        }

        #endregion

    }

    public class ImageHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            ImageRequest imageRequest = new ImageRequest(context.Request.Url);

            //Retrieve the collection object from the cache.
            string cacheKey = imageRequest.DzcName + ".dzc";
            Collection collection = (Collection)context.Cache[cacheKey];

            int imageDimensionCount = (1 << (DzcSerializer.DefaultMaxLevel - imageRequest.Level));
            int subImageDimension = DzcSerializer.DefaultTilePixelDimension / imageDimensionCount;
            int levelBitCount = DzcSerializer.DefaultMaxLevel - imageRequest.Level;

            int mortonRange;
            int mortonStart = MortonHelpers.LevelXYToMorton(imageRequest.Level, imageRequest.X, imageRequest.Y,
                DzcSerializer.DefaultMaxLevel, out mortonRange);

            using (Bitmap bitmap = new Bitmap(DzcSerializer.DefaultTilePixelDimension, DzcSerializer.DefaultTilePixelDimension))
            {
                float fontEmSize = (float)(24.0 / Math.Pow(2, levelBitCount));

                using (Graphics g = Graphics.FromImage(bitmap))
                using (Font font = new Font(FontFamily.GenericSansSerif, fontEmSize))
                {
                    //Draw a background across the entire tile.
                    Rectangle tileRect = Rectangle.FromLTRB(0, 0,
                                                            DzcSerializer.DefaultTilePixelDimension,
                                                            DzcSerializer.DefaultTilePixelDimension);
                    g.FillRectangle(Brushes.LemonChiffon, tileRect);

                    //Draw the sub-images into the tile
                    for (int y = 0; y < imageDimensionCount; ++y)
                        for (int x = 0; x < imageDimensionCount; ++x)
                        {
                            int itemIndex = mortonStart + MortonHelpers.XYToMorton(levelBitCount, x, y);

                            if (itemIndex < collection.Items.Count)
                            {
                                CollectionItem item = collection.Items[itemIndex];
                                Rectangle itemRect = new Rectangle(x * subImageDimension, y * subImageDimension,
                                                                   subImageDimension, subImageDimension);

                                item.ImageProvider.Draw(g, itemRect, imageRequest.Level);
                            }
                        }
                }

                context.Response.ContentType = "image/jpeg";
                ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders()[1];
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75L);
                bitmap.Save(context.Response.OutputStream, codecInfo, encoderParameters);
            }
        }

        #endregion
    }

}
