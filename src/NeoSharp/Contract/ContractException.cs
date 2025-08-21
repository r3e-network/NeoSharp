using System;
using System.Collections.Generic;
using System.Linq;
using NeoSharp.Core;

namespace NeoSharp.Contract
{
    /// <summary>
    /// Exception thrown when contract operations fail
    /// </summary>
    public sealed class ContractException : NeoSharpException
    {
        /// <summary>
        /// Initializes a new instance of the ContractException class
        /// </summary>
        public ContractException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ContractException class with a specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        public ContractException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ContractException class with a specified error message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ContractException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a ContractException for invalid NNS names
        /// </summary>
        /// <param name="name">The invalid NNS name</param>
        /// <returns>A new ContractException</returns>
        public static ContractException InvalidNeoName(string name) =>
            new($"'{name}' is not a valid NNS name.");

        /// <summary>
        /// Creates a ContractException for invalid NNS roots
        /// </summary>
        /// <param name="root">The invalid NNS root</param>
        /// <returns>A new ContractException</returns>
        public static ContractException InvalidNeoNameServiceRoot(string root) =>
            new($"'{root}' is not a valid NNS root.");

        /// <summary>
        /// Creates a ContractException for unexpected return types
        /// </summary>
        /// <param name="actualType">The actual return type</param>
        /// <param name="expectedTypes">The expected return types</param>
        /// <returns>A new ContractException</returns>
        public static ContractException UnexpectedReturnType(string actualType, params string[] expectedTypes)
        {
            if (expectedTypes?.Length > 0)
            {
                return new ContractException($"Got stack item of type {actualType} but expected {string.Join(", ", expectedTypes)}.");
            }
            return new ContractException(actualType);
        }

        /// <summary>
        /// Creates a ContractException for unresolvable domain names
        /// </summary>
        /// <param name="domainName">The unresolvable domain name</param>
        /// <returns>A new ContractException</returns>
        public static ContractException UnresolvableDomainName(string domainName) =>
            new($"The provided domain name '{domainName}' could not be resolved.");
    }
}