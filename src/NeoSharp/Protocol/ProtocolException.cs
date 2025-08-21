using System;
using NeoSharp.Core;

namespace NeoSharp.Protocol
{
    /// <summary>
    /// Exception thrown when protocol-related errors occur
    /// </summary>
    public class ProtocolException : NeoSharpException
    {
        /// <summary>
        /// Initializes a new instance of the ProtocolException class
        /// </summary>
        /// <param name="message">The error message</param>
        public ProtocolException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProtocolException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ProtocolException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}