// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.IO;

namespace Zentity.Pivot.Imaging
{
    /// <summary>
    /// ImageCreator is an abstract parent class for objects which deal with creating images.
    /// </summary>
    /// <remarks>
    /// This class defines a number of functions for managing the working directory for those objects.
    /// </remarks>
    public abstract class ImageCreator : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the ImageCreator class.
        /// </summary>
        protected ImageCreator()
        {
            this.m_workingDirectory = Path.Combine(Path.GetTempPath(), this.GetType().Name + "-" + Guid.NewGuid());
            this.ShouldDeleteWorkingDirectory = true;
        }

        /// <summary>
        /// Gets or sets the working directory for this image creator.
        /// </summary>
        /// <remarks>
        /// Subclasses should keep whatever temporary results, cached files, or other working files they need in this
        /// directory. Any time this property is changed, the  <see cref="ShouldDeleteWorkingDirectory"/> property is
        /// set to false. By default, this property is set to a new, uniquely named directory within the current user's
        /// temp directory.
        /// </remarks>
        public String WorkingDirectory
        {
            get { return m_workingDirectory; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("WorkingDirectory cannot be null");
                if ((m_workingDirectory != null) && (m_shouldDeleteWorkingDirectory))
                {
                    if (Directory.Exists(m_workingDirectory) == true)
                    {
                        Directory.Delete(m_workingDirectory, true);
                    }
                }

                if (Directory.Exists(value) == false)
                {
                    Directory.CreateDirectory(value);
                }

                m_workingDirectory = value;
                this.ShouldDeleteWorkingDirectory = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this image creator should remove its working 
        /// directory when it is disposed.
        /// </summary>
        /// <remarks>
        /// By default, this is set to true, but any time the <see cref="WorkingDirectory"/> is changed, this property
        /// will be reset to false.
        /// </remarks>
        public bool ShouldDeleteWorkingDirectory
        {
            get { return m_shouldDeleteWorkingDirectory; }

            set { m_shouldDeleteWorkingDirectory = value; }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.ShouldDeleteWorkingDirectory)
                    {
                        Directory.Delete(this.WorkingDirectory, true);
                        this.ShouldDeleteWorkingDirectory = false;
                    }
                }
            }

            this.disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the ImageCreator class.
        /// </summary>
        ~ImageCreator()
        {
            Dispose(false);
        }

        private String m_workingDirectory;

        private bool m_shouldDeleteWorkingDirectory;

        private bool disposed;
    }
}
