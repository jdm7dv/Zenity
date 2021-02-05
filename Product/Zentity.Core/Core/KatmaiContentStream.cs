// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


namespace Zentity.Core
{
    using System;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.IO;

    /// <summary>
    /// This class abstracts the stream contained by Core.File object for SQL2K8. 
    /// It is a simple wrapper over FileStream and has similar behavior.
    /// </summary>
    internal sealed class KatmaiContentStream : Stream
    {
        #region Fields

        int commandTimeout;
        bool disposed; // Automatically initialized to false.
        SqlConnection innerConnection;
        SqlFileStream innerStream;
        SqlTransaction innerTransaction;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return innerStream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return innerStream.CanSeek;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream can time out.
        /// </summary>
        public override bool CanTimeout
        {
            get
            {
                return innerStream.CanTimeout;
            }
        }

        /// <summary>
        ///  Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return innerStream.CanWrite;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return innerStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return innerStream.Position;
            }
            set
            {
                innerStream.Position = value;
            }
        }

        /// <summary>
        /// Gets or sets a value, in milliseconds, that determines how long the stream 
        /// will attempt to read before timing out.
        /// </summary>
        public override int ReadTimeout
        {
            get
            {
                return innerStream.ReadTimeout;
            }
            set
            {
                innerStream.ReadTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a value, in milliseconds, that determines how long the stream
        /// will attempt to write before timing out.
        /// </summary>
        public override int WriteTimeout
        {
            get
            {
                return innerStream.WriteTimeout;
            }
            set
            {
                innerStream.WriteTimeout = value;
            }
        }

        /// <summary>
        /// Gets the inner stream.
        /// </summary>
        /// <value>The inner stream.</value>
        internal SqlFileStream InnerStream
        {
            get { return innerStream; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KatmaiContentStream"/> class.
        /// </summary>
        /// <param name="storeConnectionString">The store connection string.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="resourceId">The resource id.</param>
        internal KatmaiContentStream(string storeConnectionString, int commandTimeout, Guid resourceId)
        {
            // Initialize inner connection.
            innerConnection = new SqlConnection(storeConnectionString);
            innerConnection.Open();

            // Initialize inner transaction.
            innerTransaction = innerConnection.BeginTransaction();
            this.commandTimeout = commandTimeout;

            // Get the content details.
            string sqlFilePath;
            byte[] transactionToken;
            Utilities.GetPathNameAndTransactionToken(innerConnection, innerTransaction,
                this.commandTimeout, resourceId, out sqlFilePath, out transactionToken);

            // If the returned path is null, do nothing.
            if (string.IsNullOrEmpty(sqlFilePath))
                return;

            // Create a stream.
            innerStream = new SqlFileStream(sqlFilePath, transactionToken,
                FileAccess.Read, FileOptions.SequentialScan, 0);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data read 
        /// from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read 
        /// is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular 
        /// asynchronous read request from other requests</param>
        /// <returns>An System.IAsyncResult that represents the asynchronous read, which could 
        /// still be pending.</returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in buffer from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the 
        /// write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular 
        /// asynchronous write request from other requests.</param>
        /// <returns>An System.IAsyncResult that represents the asynchronous write, which could
        /// still be pending.</returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to 
        /// finish.</param>
        /// <returns>The number of bytes read from the stream, between zero (0) and the number
        /// of bytes you requested. Streams return zero (0) only at the end of the stream, 
        /// otherwise, they should block until at least one byte is available.</returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return innerStream.EndRead(asyncResult);
        }

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O 
        /// request.</param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            innerStream.EndWrite(asyncResult);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to 
        /// the zentity store.
        /// </summary>
        public override void Flush()
        {
            innerStream.Flush();
        }

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with 
        /// the values between offset and (offset + count - 1) replaced by the bytes read from the 
        /// current source.</param>
        /// <param name="offset">The byte offset in array at which the read bytes will be 
        /// placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the 
        /// number of bytes requested if that number of bytes are not currently available, or 
        /// zero if the end of the stream is reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int ReadByte()
        {
            return innerStream.ReadByte();
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a 
        /// reference point for origin, using a value of type System.IO.SeekOrigin.</param>
        /// <returns>The position of the read cursor after seek operation</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current 
        /// position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from 
        /// buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin 
        /// copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void WriteByte(byte value)
        {
            innerStream.WriteByte(value);
        }

        // NOTE: We do not have to override Close() here. Stream.Close() calls 
        // Dispose(true) and we place all our logic in Dispose.

        #endregion

        #region Dispose

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Close inner stream.
                    if (innerStream != null)
                        innerStream.Close();

                    // complete the transaction.
                    if (innerTransaction != null)
                        innerTransaction.Commit();

                    // Close connection.
                    if (innerConnection != null)
                        innerConnection.Close();

                    base.Dispose(disposing);
                }
                innerStream = null;
                innerTransaction = null;
                innerConnection = null;

                disposed = true;
            }
        }

        #endregion
    }
}
