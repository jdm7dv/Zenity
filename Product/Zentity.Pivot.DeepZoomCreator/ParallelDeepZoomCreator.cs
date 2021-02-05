// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.DeepZoomTools;
using Zentity.DeepZoomCreator.PublishingProgressService;
using Zentity.Services.Web;

namespace Zentity.DeepZoomCreator
{
    /// <summary>
    /// Creates deep zoom images in parallel.
    /// </summary>
    internal class ParallelDeepZoomCreator
    {
        #region Public Constants

        /// <summary>
        /// Min threads per core to do the processing.
        /// </summary>
        public const int MinThreadsPerCore = 5;

        #endregion

        #region Private Fields

        private const String DziFileExtension = ".dzi";

        private String m_outputDirectory;

        private List<String> m_dziPaths;

        private int m_threadCount;

        private List<Thread> m_threadPool;

        private Queue<KeyValuePair<Guid, string>> m_sourceImagePathQueue;

        private volatile bool m_stopRequested;

        private volatile int m_itemCount;

        private ImageCreator imageCreator;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelDeepZoomCreator"/> class.
        /// </summary>
        public ParallelDeepZoomCreator()
        {
            m_outputDirectory = ".";
            m_dziPaths = new List<String>();
            m_threadPool = new List<Thread>();
            m_threadCount = Environment.ProcessorCount * MinThreadsPerCore;
            m_sourceImagePathQueue = new Queue<KeyValuePair<Guid, string>>();
            imageCreator = new ImageCreator();
        }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        public String OutputDirectory
        {
            get { return m_outputDirectory; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(Properties.Messages.OutputDirectoryNullError);
                if (Directory.Exists(value) == false)
                {
                    Directory.CreateDirectory(value);
                }
                m_outputDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets the progress service client.
        /// </summary>
        /// <value>The progress service client.</value>
        public IPublishingProgressService ProgressServiceClient
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the instance id.
        /// </summary>
        /// <value>The instance id.</value>
        public Guid InstanceId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the total images.
        /// </summary>
        /// <value>The total images.</value>
        public int TotalImages
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the dzi paths.
        /// </summary>
        /// <value>The dzi paths.</value>
        public List<String> DziPaths
        {
            get { return m_dziPaths; }
        }

        /// <summary>
        /// Gets or sets the thread count.
        /// </summary>
        /// <value>The thread count.</value>
        public int ThreadCount
        {
            get { return m_threadCount; }

            set
            {
                if (value < 1) throw new ArgumentException(string.Format(Properties.Messages.ThreadCountLessThanOne, value));
                m_threadCount = value;
            }
        }

        /// <summary>
        /// Submits the specified item id.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="sourceImagePath">The source image path.</param>
        public void Submit(Guid itemId, String sourceImagePath)
        {
            lock (m_sourceImagePathQueue)
            {
                var itemPair = new KeyValuePair<Guid, string>(itemId, sourceImagePath);
                if (m_sourceImagePathQueue.Contains(itemPair, new KeyValuePairComparer())) return;

                m_itemCount++;
                m_sourceImagePathQueue.Enqueue(itemPair);
                Monitor.Pulse(m_sourceImagePathQueue);
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            lock (m_threadPool)
            {
                if (m_threadPool.Count > 0) return;

                m_stopRequested = false;
                while (m_threadPool.Count < m_threadCount)
                {
                    Thread newThread = new Thread(OnThreadRun) { Name = "ParallelDeepZoomCreator-" + m_threadPool.Count };
                    newThread.IsBackground = true;
                    newThread.Start();

                    m_threadPool.Add(newThread);
                }
            }
        }

        /// <summary>
        /// Joins this instance.
        /// </summary>
        public void Join()
        {
            int lastReportedSize = int.MaxValue;
            while (true)
            {
                lock (m_dziPaths)
                {
                    int countRemaining = m_itemCount - m_dziPaths.Count;
                    if (countRemaining == 0) break;

                    if ((lastReportedSize - countRemaining) > 10)
                    {
                        lastReportedSize = countRemaining;
                    }
                    Monitor.Wait(m_dziPaths);
                }
            }

            m_stopRequested = true;
            lock (m_sourceImagePathQueue) { Monitor.PulseAll(m_sourceImagePathQueue); }
            lock (m_threadPool)
            {
                lastReportedSize = int.MaxValue;
                while (m_threadPool.Count > 0)
                {
                    if ((lastReportedSize - m_threadPool.Count) > 10)
                    {
                        lastReportedSize = m_threadPool.Count;
                    }
                    Monitor.Wait(m_threadPool);
                }
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            this.Join();
            m_threadPool.Clear();
            m_sourceImagePathQueue.Clear();
            m_dziPaths.Clear();
            m_stopRequested = false;
        }

        /// <summary>
        /// Called when [thread run].
        /// </summary>
        private void OnThreadRun()
        {
            KeyValuePair<Guid, string> currentImageItem = new KeyValuePair<Guid, string>();
            bool shouldResign = false;
            while ((m_stopRequested == false) && (shouldResign == false))
            {
                try
                {
                    lock (m_sourceImagePathQueue)
                    {
                        if (m_sourceImagePathQueue.Count == 0)
                        {
                            Monitor.Wait(m_sourceImagePathQueue);
                            continue;
                        }
                        currentImageItem = m_sourceImagePathQueue.Dequeue();
                    }

                    this.CreateDeepZoomImage(currentImageItem.Key, currentImageItem.Value);
                }
                catch (OutOfMemoryException)
                {
                    lock (m_threadPool)
                    {
                        if (m_threadPool.Count > 1)
                        {
                            shouldResign = true;
                        }
                    }
                    lock (m_sourceImagePathQueue)
                    {
                        m_sourceImagePathQueue.Enqueue(currentImageItem);
                        Monitor.Pulse(m_sourceImagePathQueue);
                    }
                    lock (m_dziPaths) { Monitor.Pulse(m_dziPaths); }
                }
                catch
                {
                    lock (m_dziPaths) { Monitor.Pulse(m_dziPaths); }
                }
            }

            lock (m_threadPool)
            {
                m_threadPool.Remove(Thread.CurrentThread);
                Monitor.Pulse(m_threadPool);
            }
        }

        /// <summary>
        /// Creates the deep zoom image.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="sourceImagePath">The source image path.</param>
        private void CreateDeepZoomImage(Guid itemId, String sourceImagePath)
        {
            String dziTargetPath = Path.Combine(m_outputDirectory, itemId + DziFileExtension);
            bool shouldCreateImage = true;
            lock (m_dziPaths)
            {
                shouldCreateImage = (m_dziPaths.Contains(dziTargetPath) == false);
                m_dziPaths.Add(dziTargetPath);
                try
                {
                    if (this.ProgressServiceClient != null)
                    {
                        ProgressCounter progCounter = new ProgressCounter {Total = this.TotalImages, Completed = m_dziPaths.Count};
                        this.ProgressServiceClient.ReportProgress(this.InstanceId, PublishStage.CreatingDeepZoomImages, progCounter);
                    }
                }
                catch (Exception ex)
                {
                    Globals.TraceMessage(TraceEventType.Error, ex.ToString(), ex.Message);
                }
                Globals.TraceMessage(TraceEventType.Information, Properties.Messages.StatusMessage, string.Format(Properties.Messages.ImageCreated, m_dziPaths.Count));
            }

            if (shouldCreateImage)
            {
                imageCreator.Create(sourceImagePath, dziTargetPath);
            }

            lock (m_dziPaths) { Monitor.Pulse(m_dziPaths); }
        }
    }

    /// <summary>
    /// Comparer for the custom KeyValuePair(Guid, string) class using Pivot Publishing service
    /// </summary>
    internal class KeyValuePairComparer : IEqualityComparer<KeyValuePair<Guid, string>>
    {
        #region IEqualityComparer<KeyValuePair<Guid,string>> Members

        /// <summary>
        /// Check for equality between two instances of KeyValuePair.
        /// </summary>
        /// <param name="x">The first instance.</param>
        /// <param name="y">The second instance.</param>
        /// <returns>Returns true if equal, otherwise false</returns>
        public bool Equals(KeyValuePair<Guid, string> x, KeyValuePair<Guid, string> y)
        {
            return x.Key == y.Key;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(KeyValuePair<Guid, string> obj)
        {
            return obj.Key.GetHashCode();
        }

        #endregion
    }
}
