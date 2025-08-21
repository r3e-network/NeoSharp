using System;
using System.Runtime.Serialization;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Exception thrown when NEP-2 password encryption/decryption operations fail.
    /// NEP-2 is a standard for passphrase-protected private keys.
    /// </summary>
    [Serializable]
    public class NEP2Exception : Exception
    {
        /// <summary>
        /// Gets the error code associated with this NEP-2 operation.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the NEP2Exception class.
        /// </summary>
        public NEP2Exception() : base("NEP-2 operation failed.")
        {
            ErrorCode = "NEP2_UNKNOWN";
        }

        /// <summary>
        /// Initializes a new instance of the NEP2Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NEP2Exception(string message) : base(message)
        {
            ErrorCode = "NEP2_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the NEP2Exception class with a specified error message and error code.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">A specific error code for this NEP-2 operation.</param>
        public NEP2Exception(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode ?? "NEP2_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the NEP2Exception class with a specified error message and a reference to the inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public NEP2Exception(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "NEP2_INNER_EXCEPTION";
        }

        /// <summary>
        /// Initializes a new instance of the NEP2Exception class with a specified error message, error code, and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">A specific error code for this NEP-2 operation.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public NEP2Exception(string message, string errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode ?? "NEP2_INNER_EXCEPTION";
        }

        /// <summary>
        /// Initializes a new instance of the NEP2Exception class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected NEP2Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ErrorCode = info?.GetString(nameof(ErrorCode)) ?? "NEP2_DESERIALIZED";
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
        /// Creates a NEP2Exception for invalid format errors.
        /// </summary>
        /// <param name="message">The specific format error message.</param>
        /// <returns>A new NEP2Exception with appropriate error code.</returns>
        public static NEP2Exception InvalidFormat(string message)
        {
            return new NEP2Exception($"Invalid NEP-2 format: {message}", "NEP2_INVALID_FORMAT");
        }

        /// <summary>
        /// Creates a NEP2Exception for invalid passphrase errors.
        /// </summary>
        /// <param name="message">The specific passphrase error message.</param>
        /// <returns>A new NEP2Exception with appropriate error code.</returns>
        public static NEP2Exception InvalidPassphrase(string message)
        {
            return new NEP2Exception($"Invalid passphrase: {message}", "NEP2_INVALID_PASSPHRASE");
        }

        /// <summary>
        /// Creates a NEP2Exception for encryption/decryption errors.
        /// </summary>
        /// <param name="message">The specific encryption error message.</param>
        /// <param name="innerException">The underlying cryptographic exception.</param>
        /// <returns>A new NEP2Exception with appropriate error code.</returns>
        public static NEP2Exception EncryptionError(string message, Exception innerException = null)
        {
            return new NEP2Exception($"NEP-2 encryption error: {message}", "NEP2_ENCRYPTION_ERROR", innerException);
        }
    }
}