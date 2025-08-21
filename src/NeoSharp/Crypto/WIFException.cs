using System;
using System.Runtime.Serialization;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Exception thrown when Wallet Import Format (WIF) operations fail.
    /// WIF is a standard format for encoding private keys for easy import/export.
    /// </summary>
    [Serializable]
    public class WIFException : Exception
    {
        /// <summary>
        /// Gets the error code associated with this WIF operation.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the WIFException class.
        /// </summary>
        public WIFException() : base("WIF operation failed.")
        {
            ErrorCode = "WIF_UNKNOWN";
        }

        /// <summary>
        /// Initializes a new instance of the WIFException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public WIFException(string message) : base(message)
        {
            ErrorCode = "WIF_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the WIFException class with a specified error message and error code.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">A specific error code for this WIF operation.</param>
        public WIFException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode ?? "WIF_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the WIFException class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WIFException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "WIF_INNER_EXCEPTION";
        }

        /// <summary>
        /// Initializes a new instance of the WIFException class with a specified error message, error code, and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">A specific error code for this WIF operation.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WIFException(string message, string errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode ?? "WIF_INNER_EXCEPTION";
        }

        /// <summary>
        /// Initializes a new instance of the WIFException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected WIFException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ErrorCode = info?.GetString(nameof(ErrorCode)) ?? "WIF_DESERIALIZED";
        }

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue(nameof(ErrorCode), ErrorCode);
            }
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Creates a WIFException for invalid format errors.
        /// </summary>
        /// <param name="message">The specific format error message.</param>
        /// <returns>A new WIFException with appropriate error code.</returns>
        public static WIFException InvalidFormat(string message)
        {
            return new WIFException($"Invalid WIF format: {message}", "WIF_INVALID_FORMAT");
        }

        /// <summary>
        /// Creates a WIFException for checksum validation errors.
        /// </summary>
        /// <param name="message">The specific checksum error message.</param>
        /// <returns>A new WIFException with appropriate error code.</returns>
        public static WIFException InvalidChecksum(string message)
        {
            return new WIFException($"Invalid WIF checksum: {message}", "WIF_INVALID_CHECKSUM");
        }

        /// <summary>
        /// Creates a WIFException for encoding/decoding errors.
        /// </summary>
        /// <param name="message">The specific encoding error message.</param>
        /// <param name="innerException">The underlying encoding exception.</param>
        /// <returns>A new WIFException with appropriate error code.</returns>
        public static WIFException EncodingError(string message, Exception innerException = null)
        {
            return new WIFException($"WIF encoding error: {message}", "WIF_ENCODING_ERROR", innerException);
        }
    }
}