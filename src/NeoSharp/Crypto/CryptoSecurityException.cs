using System;
using System.Runtime.Serialization;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Exception thrown when cryptographic security operations fail.
    /// This includes secure memory operations, key generation, and other security-critical functions.
    /// </summary>
    [Serializable]
    public class CryptoSecurityException : Exception
    {
        /// <summary>
        /// Gets the error code associated with this security operation.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets the security level of this exception.
        /// </summary>
        public SecurityLevel Level { get; }

        /// <summary>
        /// Defines the security levels for cryptographic exceptions.
        /// </summary>
        public enum SecurityLevel
        {
            /// <summary>
            /// Low security impact - informational or recoverable error.
            /// </summary>
            Low,
            
            /// <summary>
            /// Medium security impact - potential security weakness.
            /// </summary>
            Medium,
            
            /// <summary>
            /// High security impact - critical security failure.
            /// </summary>
            High,
            
            /// <summary>
            /// Critical security impact - system compromise possible.
            /// </summary>
            Critical
        }

        /// <summary>
        /// Initializes a new instance of the CryptoSecurityException class.
        /// </summary>
        public CryptoSecurityException() : base("Cryptographic security operation failed.")
        {
            ErrorCode = "SECURITY_UNKNOWN";
            Level = SecurityLevel.Medium;
        }

        /// <summary>
        /// Initializes a new instance of the CryptoSecurityException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CryptoSecurityException(string message) : base(message)
        {
            ErrorCode = "SECURITY_ERROR";
            Level = SecurityLevel.Medium;
        }

        /// <summary>
        /// Initializes a new instance of the CryptoSecurityException class with specified parameters.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errorCode">A specific error code for this security operation.</param>
        /// <param name="level">The security level of this exception.</param>
        public CryptoSecurityException(string message, string errorCode, SecurityLevel level = SecurityLevel.Medium) : base(message)
        {
            ErrorCode = errorCode ?? "SECURITY_ERROR";
            Level = level;
        }

        /// <summary>
        /// Initializes a new instance of the CryptoSecurityException class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CryptoSecurityException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "SECURITY_INNER_EXCEPTION";
            Level = SecurityLevel.Medium;
        }

        /// <summary>
        /// Initializes a new instance of the CryptoSecurityException class with all parameters.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">A specific error code for this security operation.</param>
        /// <param name="level">The security level of this exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CryptoSecurityException(string message, string errorCode, SecurityLevel level, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode ?? "SECURITY_INNER_EXCEPTION";
            Level = level;
        }

        /// <summary>
        /// Initializes a new instance of the CryptoSecurityException class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected CryptoSecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ErrorCode = info?.GetString(nameof(ErrorCode)) ?? "SECURITY_DESERIALIZED";
            if (Enum.TryParse<SecurityLevel>(info?.GetString(nameof(Level)) ?? "Medium", out var level))
            {
                Level = level;
            }
            else
            {
                Level = SecurityLevel.Medium;
            }
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
                info.AddValue(nameof(Level), Level.ToString());
            }
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Creates a CryptoSecurityException for secure memory operation errors.
        /// </summary>
        /// <param name="message">The specific memory error message.</param>
        /// <param name="innerException">The underlying system exception.</param>
        /// <returns>A new CryptoSecurityException with appropriate error code and high security level.</returns>
        public static CryptoSecurityException SecureMemoryError(string message, Exception innerException = null)
        {
            return new CryptoSecurityException($"Secure memory operation failed: {message}", "SECURITY_MEMORY_ERROR", SecurityLevel.High, innerException);
        }

        /// <summary>
        /// Creates a CryptoSecurityException for key generation errors.
        /// </summary>
        /// <param name="message">The specific key generation error message.</param>
        /// <param name="innerException">The underlying cryptographic exception.</param>
        /// <returns>A new CryptoSecurityException with appropriate error code and critical security level.</returns>
        public static CryptoSecurityException KeyGenerationError(string message, Exception innerException = null)
        {
            return new CryptoSecurityException($"Key generation failed: {message}", "SECURITY_KEY_GENERATION_ERROR", SecurityLevel.Critical, innerException);
        }

        /// <summary>
        /// Creates a CryptoSecurityException for cryptographic validation errors.
        /// </summary>
        /// <param name="message">The specific validation error message.</param>
        /// <returns>A new CryptoSecurityException with appropriate error code and high security level.</returns>
        public static CryptoSecurityException ValidationError(string message)
        {
            return new CryptoSecurityException($"Cryptographic validation failed: {message}", "SECURITY_VALIDATION_ERROR", SecurityLevel.High);
        }

        /// <summary>
        /// Creates a CryptoSecurityException for access control violations.
        /// </summary>
        /// <param name="message">The specific access control error message.</param>
        /// <returns>A new CryptoSecurityException with appropriate error code and critical security level.</returns>
        public static CryptoSecurityException AccessViolation(string message)
        {
            return new CryptoSecurityException($"Security access violation: {message}", "SECURITY_ACCESS_VIOLATION", SecurityLevel.Critical);
        }

        /// <summary>
        /// Creates a CryptoSecurityException for data tampering detection.
        /// </summary>
        /// <param name="message">The specific tampering detection message.</param>
        /// <returns>A new CryptoSecurityException with appropriate error code and critical security level.</returns>
        public static CryptoSecurityException TamperingDetected(string message)
        {
            return new CryptoSecurityException($"Data tampering detected: {message}", "SECURITY_TAMPERING_DETECTED", SecurityLevel.Critical);
        }
    }
}