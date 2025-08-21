using System;
using NeoSharp.Core;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Exception thrown when cryptographic signing operations fail
    /// </summary>
    public sealed class SignException : NeoSharpException
    {
        /// <summary>
        /// Initializes a new instance of the SignException class
        /// </summary>
        public SignException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SignException class with a specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        public SignException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SignException class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public SignException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a SignException for header out of range errors
        /// </summary>
        /// <param name="header">The invalid header value</param>
        /// <returns>A new SignException</returns>
        public static SignException HeaderOutOfRange(byte header) =>
            new($"Header {header} is out of valid range [27,34]");

        /// <summary>
        /// Creates a SignException for signature recovery failures
        /// </summary>
        /// <returns>A new SignException</returns>
        public static SignException RecoveryFailed() =>
            new("Failed to recover public key from signature");
    }
}