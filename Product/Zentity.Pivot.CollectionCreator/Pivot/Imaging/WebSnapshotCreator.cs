// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Zentity.Services.External;
using Zentity.Services.Web;
using Zentity.CollectionCreator.Properties;

namespace Zentity.Pivot.Imaging
{
    /// <summary>
    /// WebSnapshotCreator creates bitmap images capturing a rendering of an Url.</summary>
    /// <remarks> 
    /// <para><b>NOTE:</b> This class creates an instance of the IE7 rendering engine, and is therefore very memory intensive! 
    /// Under low memory conditions, it is a known problem that images are sometimes not rendered as
    /// expected, so be sure to optimize memory use when using this class.</para>
    /// 
    /// <para>* bear in mind that the snapshot of the page is taken as soon as the document is finished rendering, so
    /// any animations or JavaScript which run after that time are likely not to be included.</para>
    /// </remarks>
    public class WebSnapshotCreator : ImageCreator, IImageCapture
    {
        /// <summary>
        /// The maximum width or height at which images may be rendered.
        /// </summary>
        public const int MaximumDimension = 5000;

        /// <summary>
        /// The default width at which images are rendered.
        /// </summary>
        public const int DefaultWidth = 1280;

        /// <summary>
        /// The default height at which images are rendered.
        /// </summary>
        public const int DefaultHeight = 1024;

        /// <summary>
        /// Initializes a new instance of the WebSnapshotCreator class.
        /// </summary>
        public WebSnapshotCreator()
        {
            m_lock = new Object();
            m_resetEvent = new AutoResetEvent(false);
            Thread workerThread = new Thread(RunWorkerThread);
            workerThread.Priority = ThreadPriority.Lowest;
            workerThread.IsBackground = true;
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();

            this.Height = DefaultHeight;
            this.Width = DefaultWidth;
            this.WorkingDirectory = Path.GetTempPath();
        }

        /// <summary>
        /// Gets or sets the height (in pixels) at which to render images.
        /// </summary>
        /// <remarks>
        /// By default, this is set to 1024 pixels.
        /// </remarks>
        public int Height
        {
            get { return m_height; }

            set
            {
                if (value < 1) throw new ArgumentException(string.Format(Messages.HeightMustbeAtleastOne, value));
                if (value > MaximumDimension)
                {
                    throw new ArgumentException(string.Format(Messages.HeightMustBeLessThan, MaximumDimension, value));
                }
                m_height = value;
            }
        }

        /// <summary>
        /// Gets or sets the width (in pixels) at which to render images.
        /// </summary>
        /// <remarks>
        /// By default, this is set to 1280 pixels.
        /// </remarks>
        public int Width
        {
            get { return m_width; }

            set
            {
                if (value < 1) throw new ArgumentException(string.Format(Messages.WidthMustbeAtleastOne, value));
                if (value > MaximumDimension)
                {
                    throw new ArgumentException(string.Format(Messages.WidthMustBeLessThan, MaximumDimension, value));
                }
                m_width = value;
            }
        }


        #region IImageCapture Members

        /// <summary>
        /// Generates Custom Image.
        /// </summary>
        /// <param name="propertyValue">Property Vaue is fetched from the database for a particualar resource.</param>
        /// <param name="width">The width of the generated image.</param>
        /// <param name="height">The height of the generated image.</param>
        /// <returns>The path where the image is generated.</returns>
        public string GenerateImage(object propertyValue, int width, int height)
        {
            this.Width = width;
            this.Height = height;

            if (propertyValue == null || !(propertyValue is Uri))
            {
                return string.Empty;
            }

            Uri websiteUri = propertyValue as Uri;
            String tempFile = Path.Combine(this.WorkingDirectory, Guid.NewGuid() + ".jpg");
            using (FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
            {
                if (this.CreateImage(websiteUri, fileStream))
                {
                    return tempFile;
                }
                else
                {
                    Globals.TraceMessage(TraceEventType.Warning, string.Empty, string.Format("Could not create an image for '{0}' in within {1} seconds", propertyValue, RenderTimeout.TotalSeconds));
                    return string.Empty;
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates a new image based upon a given item, and writes it to the provided output stream.
        /// </summary>
        /// <param name="websiteUri">The website URI.</param>
        /// <param name="outputStream">the stream to which the bitmap should be written</param>
        /// <returns>
        /// true if the image was successfully written
        /// </returns>
        private bool CreateImage(Uri websiteUri, Stream outputStream)
        {
            if (m_outputStream != null)
                throw new InvalidOperationException(Messages.ThreadAlreadyCreatingImage);

            m_outputStream = outputStream;

            bool createdImage = false;
            try
            {
                if (m_resetEvent != null)
                    m_resetEvent.WaitOne(RenderTimeout);

                lock (m_lock)
                {
                    if (m_dispatcher != null)
                    {
                        // Invode the webbrowser and wait for it to render the html page.
                        m_dispatcher.BeginInvoke((Action<Uri>) this.InvokeWebBrowser, websiteUri);

                        if (m_resetEvent != null)
                            m_resetEvent.WaitOne(RenderTimeout);

                        // Check if the image file has content or not.
                        createdImage = m_outputStream != null && m_outputStream.Length > 0;
                    }
                    else
                    {
                        Globals.TraceMessage(TraceEventType.Warning, string.Empty, Messages.DispatcherNull);
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.TraceMessage(TraceEventType.Error, string.Format(Messages.ExceptionInCreateImageThread, ex), string.Empty);
            }
            
            return createdImage;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_webBrowser", Justification="The m_webBrowser object is disposed in a dispatcher.")]
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (m_dispatcher != null)
                    {
                        if (m_webBrowser != null)
                        {
                            m_dispatcher.Invoke((Action) delegate()
                            {
                                m_webBrowser.Dispose();
                                m_webBrowser = null;
                            });
                        }
                        m_dispatcher.InvokeShutdown();
                        m_dispatcher = null;
                    }

                    if (m_resetEvent != null)
                    {
                        m_resetEvent.Dispose();
                    }
                }

                this.disposed = true;
            }

            // Call the dispose method of the base class
            base.Dispose(disposing);
        }

        /// <summary>
        /// Runs the worker thread.
        /// </summary>
        private void RunWorkerThread()
        {
            try
            {
                lock (m_lock)
                {
                    m_dispatcher = Dispatcher.CurrentDispatcher;
                }
                if (m_resetEvent != null)
                    m_resetEvent.Set();
                Dispatcher.Run();
            }
            catch (Exception e)
            {
                Globals.TraceMessage(TraceEventType.Error, string.Format(Messages.ExceptionInRenderingThread, e), string.Empty);
            }
        }

        /// <summary>
        /// Invokes the web browser and loads the given Url.
        /// </summary>
        /// <param name="websiteUri">The website URI to take snapshot of.</param>
        private void InvokeWebBrowser(Uri websiteUri)
        {
            try
            {
                if (m_webBrowser != null)
                {
                    m_webBrowser.Dispose();
                }

                m_webBrowser = new WebBrowser();
                m_webBrowser.DocumentCompleted += OnDocumentCompleted;
                m_webBrowser.ScriptErrorsSuppressed = true;
                m_webBrowser.ScrollBarsEnabled = false;
                
                m_webBrowser.Height = m_height;
                m_webBrowser.Width = m_width;
                m_webBrowser.AllowNavigation = true;
                m_webBrowser.Navigate(websiteUri);
            }
            catch (Exception ex)
            {
                Exception innerException = ex.InnerException ?? ex;
                Globals.TraceMessage(TraceEventType.Error, innerException.StackTrace, string.Format(Messages.ExceptionInvokingBrowser, innerException.Message));
            }
        }

        /// <summary>
        /// Called when [document loading has completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.Forms.WebBrowserDocumentCompletedEventArgs"/> instance containing the event data.</param>
        private void OnDocumentCompleted(Object sender, WebBrowserDocumentCompletedEventArgs args)
        {
            if (m_webBrowser.ReadyState == WebBrowserReadyState.Complete)
            {
                try
                {
                    using (Bitmap bitmap = new Bitmap(m_width, m_height))
                    {
                        m_webBrowser.DrawToBitmap(bitmap, new Rectangle(m_webBrowser.Location.X, m_webBrowser.Location.Y, m_webBrowser.Width, m_webBrowser.Height));

                        ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders()[JpegEncoderIndex];
                        using (EncoderParameters encoderParameters = new EncoderParameters(1))
                        {
                            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                            bitmap.Save(m_outputStream, codecInfo, encoderParameters);
                        }
                    }
                }
                catch (Exception e)
                {
                    Globals.TraceMessage(TraceEventType.Error, string.Format(Messages.ImageRenderingFailure, e), string.Empty);
                }

                if (m_resetEvent != null)
                    m_resetEvent.Set();
            }
        }

        private static readonly TimeSpan RenderTimeout = TimeSpan.FromMinutes(3);

        private const int JpegEncoderIndex = 1;

        private Object m_lock;

        private Dispatcher m_dispatcher;

        private WebBrowser m_webBrowser;

        private Stream m_outputStream;

        private int m_height;

        private int m_width;

        private bool disposed;

        private AutoResetEvent m_resetEvent;
    }
}
