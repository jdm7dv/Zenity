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
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Zentity.Services.Web;
using Zentity.CollectionCreator.Properties;
using System.Globalization;

namespace Zentity.Pivot.Imaging
{
    /// <summary>
    /// HtmlImageCreator creates bitmap images capturing a rendering of an HTML template.</summary>
    /// <remarks>The template is expected to contain various tags which are replaced with the appropriate values taken
    /// from a Pivot item. Once the template is "instantiated", it will be rendered using an in-memory IE7 engine. That
    /// rendered version will then be saved to an appropriate output stream.
    /// <para>The HTML template may be any valid HTML document which can be rendered in IE7. This can include CSS,
    /// JavaScript, or any other technology used to render web pages*. In order to determine the size at which the final
    /// image is drawn, each template may include an HTML comment specifying the size: <c>&lt;!-- size: <i>width</i>,
    /// <i>height</i> --&gt;</c>. If this comment is not included, then a default size will be used. In order to include
    /// item-specific data in the image, you can use the following tags in your template:</para>
    /// 
    /// <list type="table">
    /// <listheader><term>Syntax</term><description>Description</description></listheader>
    /// <item><term>{<i>facet</i>}</term>
    /// <description>inserts the first value of the named facet</description></item>
    /// <item><term>{<i>facet</i>:<i>n</i>}</term>
    /// <description>inserts the n-th value of the named facet</description></item>
    /// <item><term>{<i>facet</i>:join:<i>delimter</i>}</term>
    /// <description>inserts all of the named facet's values, delimited by the given string</description></item>
    /// </list>
    /// 
    /// <para><b>NOTE:</b> This class creates an instance of the IE7 rendering engine, and is therefore very memory intensive! 
    /// Under low memory conditions, it is a known problem that images are sometimes not rendered as
    /// expected, so be sure to optimize memory use when using this class.</para>
    /// 
    /// <para>* bear in mind that the snapshot of the page is taken as soon as the document is finished rendering, so
    /// any animations or JavaScript which run after that time are likely not to be included.</para>
    /// </remarks>
    public sealed class HtmlImageCreator : ImageCreator
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
        /// Initializes a new instance of the HtmlImageCreator class.
        /// </summary>
        public HtmlImageCreator()
        {
            m_lock = new Object();
            m_resetEvent = new AutoResetEvent(false);

            Thread workerThread = new Thread(RunWorkerThread);
            workerThread.Priority = ThreadPriority.Lowest;
            workerThread.IsBackground = true;
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();
            
            this.HtmlTemplate = Zentity.CollectionCreator.Properties.Resources.DefaultHtmlTemplate;
            this.Height = DefaultHeight;
            this.Width = DefaultWidth;
        }

        /// <summary>
        /// Gets or sets the HTML template used to create images.
        /// </summary>
        /// <remarks>
        /// This property may not be empty or null. When changing this property, the current values of
        /// <see cref="Height"/> and <see cref="Width"/> will be overridden if the new template provides an appropriate
        /// "size" comment.  The <see cref="HtmlTemplatePath"/> will also be set to null. By default, this property is
        /// set to a simple template which displays the item's current image overlaid with the item's title along the
        /// bottom edge.
        /// </remarks>
        public String HtmlTemplate
        {
            get { return m_htmlTemplate; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("HtmlTemplate cannot be null");
                m_htmlTemplate = value;
                m_htmlTemplatePath = null;

                this.ReadSizeFromTemplate(value, ref m_width, ref m_height);
            }
        }

        /// <summary>
        /// Gets or sets the path to an HTML file which should be used as the template for this image creator.
        /// </summary>
        /// <remarks>
        /// When setting this property, the specified file will be loaded, and the <see cref="HtmlTemplate"/> property
        /// set to its contents.
        /// </remarks>
        public String HtmlTemplatePath
        {
            get { return m_htmlTemplatePath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("HtmlTemplatePath cannot be null");
                if (File.Exists(value) == false)
                {
                    throw new ArgumentException("HtmlTemplatePath does not exist: " + value);
                }

                this.HtmlTemplate = File.ReadAllText(value);
                m_htmlTemplatePath = value;
            }
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
                    throw new ArgumentException(string.Format(Messages.HeightMustBeLessThan, MaximumDimension, value, CultureInfo.InvariantCulture));
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

        /// <summary>
        /// Creates an image and assigns it to the given item.
        /// </summary>
        /// <param name="collectionItem">The collection item.</param>
        /// <returns>Temporary path of the image.</returns>
        /// <remarks>
        /// If the item already had an image, it is replaced with the new image. The new image refers to a newly create
        /// image file stored in this image creator's working directory. If the <see cref="ImageCreator.ShouldDeleteWorkingDirectory"/> property is true, then the image will also be deleted as
        /// soon as this image creator is disposed, so it will be necessary to ensure the image is copied to a new
        /// directory (and the item updated) before that happens.
        /// </remarks>
        [CLSCompliant(false)]
        public string CreateImage(Item collectionItem)
        {
            String tempFile = Path.Combine(this.WorkingDirectory, collectionItem.Id + ".jpg");
            using (FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
            {
                if (this.CreateImage(collectionItem, fileStream))
                {
                    return tempFile;
                }
                else
                {
                    Globals.TraceMessage(TraceEventType.Warning, string.Empty, string.Format(Messages.FailedCreatingImageInTime, collectionItem.Name, RenderTimeout.TotalSeconds, CultureInfo.InvariantCulture));
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Creates a new image based upon a given item, and writes it to the provided output stream.
        /// </summary>
        /// <param name="collectionItem">The collection item.</param>
        /// <param name="outputStream">the stream to which the bitmap should be written</param>
        /// <returns>
        /// true if the image was successfully written
        /// </returns>
        private bool CreateImage(Item collectionItem, Stream outputStream)
        {
            if (m_outputStream != null) 
                throw new InvalidOperationException(Messages.ThreadAlreadyCreatingImage);
            
            m_outputStream = outputStream;
            String documentHtml = this.InstantiateTemplate(collectionItem);
            string tempHtmlFile = Path.Combine(this.WorkingDirectory, collectionItem.Id + ".html");
            File.WriteAllText(tempHtmlFile, documentHtml, System.Text.Encoding.Unicode);

            bool createdImage = false;
            try
            {
                // Wait till the dispatcher object is obtained
                if (m_resetEvent != null)
                    m_resetEvent.WaitOne(RenderTimeout);

                lock (m_lock)
                {
                    if (m_dispatcher != null)
                    {
                        // Invode the webbrowser and wait for it to render the html page.
                        m_dispatcher.BeginInvoke((Action<String>) this.InvokeWebBrowser, tempHtmlFile);

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
                Globals.TraceMessage(TraceEventType.Error, string.Format(Messages.ExceptionInCreateImageThread, ex, CultureInfo.InvariantCulture), string.Empty);
            }
            finally
            {
                m_outputStream = null;
                if (File.Exists(tempHtmlFile))
                {
                    File.Delete(tempHtmlFile);
                }
            }

            return createdImage;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_webBrowser", Justification = "The m_webBrowser object is disposed in a dispatcher.")]
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
        /// Parses the text of an HTML template and returns the size specified (if any).
        /// </summary>
        /// <remarks>
        /// If a size comment was found, but the specified size could not be parsed, then warnings will be written to
        /// the Pauthor global log.
        /// </remarks>
        /// <param name="template">the HTML template to parse</param>
        /// <param name="width">updated with the specified width, if one was provided</param>
        /// <param name="height">updated with the specified height, if one was provided</param>
        public void ReadSizeFromTemplate(String template, ref int width, ref int height)
        {
            Match match = TemplateSizeRegex.Match(template);
            if (match.Success == false) return;

            if (Int32.TryParse(match.Groups[1].Value, out width) == false)
            {
                Globals.TraceMessage(TraceEventType.Warning, "HTML Template had an unexpected width: " + match.Groups[1].Value, "Invalid dimension in template");
            }

            if (Int32.TryParse(match.Groups[2].Value, out height) == false)
            {
                Globals.TraceMessage(TraceEventType.Warning, "HTML Template had an unexpected height: " + match.Groups[2].Value, "Invalid dimension in template");
            }
        }

        /// <summary>
        /// Creates a copy of this image creator's <see cref="HtmlTemplate"/> by replacing all the tags with appropriate
        /// values based upon a given image.
        /// </summary>
        /// <param name="item">the item containing the specific data to use</param>
        /// <returns>an HTML document with the given item's data</returns>
        [CLSCompliant(false)]
        public String InstantiateTemplate(Item item)
        {
            String text = m_htmlTemplate;
            text = Replace(text, "{id}", item.Id);
            text = Replace(text, "{name}", item.Name ?? "");
            text = Replace(text, "{href}", item.Href ?? "");
            text = Replace(text, "{description}", item.Description ?? "");
            text = Replace(text, "{type}", item.Type ?? "");

            return text;
        }

        /// <summary>
        /// Replaces the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="findText">The find text.</param>
        /// <param name="replaceText">The replace text.</param>
        /// <returns>Modified string after replacement.</returns>
        private static String Replace(String source, String findText, String replaceText)
        {
            String result = source;
            while (true)
            {
                int index = result.IndexOf(findText, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1) break;

                String foundText = result.Substring(index, findText.Length);
                result = result.Replace(foundText, replaceText);
            }
            return result;
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
        /// Invokes the web browser.
        /// </summary>
        /// <param name="htmlFilePath">The HTML file path.</param>
        private void InvokeWebBrowser(String htmlFilePath)
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
                m_webBrowser.Url = new Uri(htmlFilePath, UriKind.Absolute);
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

        private static readonly Regex TemplateSizeRegex = new Regex("<!--[\n\t ]*size:[\n\t ]*([0-9]+),[\n\t ]*([0-9]+)[\n\t ]*-->", RegexOptions.IgnoreCase);

        private static readonly TimeSpan RenderTimeout = TimeSpan.FromMinutes(1);

        private const int JpegEncoderIndex = 1;

        private Object m_lock;

        private Dispatcher m_dispatcher;

        private WebBrowser m_webBrowser;

        private Stream m_outputStream;

        private String m_htmlTemplate;

        private String m_htmlTemplatePath;

        private int m_height;

        private int m_width;

        private bool disposed;

        private AutoResetEvent m_resetEvent;
    }
}
