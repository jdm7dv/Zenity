// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.IO;
    using System.Text;

    #region StringWriterWithEncoding Class
    internal class StringWriterWithEncoding : StringWriter
        {
            Encoding encoding;

            /// <summary>
            /// Initializes a new instance of the <see cref="StringWriterWithEncoding"/> class.
            /// </summary>
            /// <param name="builder">The builder.</param>
            /// <param name="encoding">The encoding.</param>
            public StringWriterWithEncoding(StringBuilder builder, Encoding encoding)
                : base(builder)
            {
                this.encoding = encoding;
            }

            /// <summary>
            /// Gets the <see cref="T:System.Text.Encoding"/> in which the output is written.
            /// </summary>
            /// <value></value>
            /// <returns>The Encoding in which the output is written.</returns>
            public override Encoding Encoding 
            { 
              get
                {
                    return encoding;
                } 
            }
        }
     #endregion
}
