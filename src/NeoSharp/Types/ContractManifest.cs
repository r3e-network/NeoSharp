using System;
using System.Collections.Generic;

namespace NeoSharp.Types
{
    /// <summary>
    /// Represents a contract manifest
    /// </summary>
    public class ContractManifest
    {
        /// <summary>
        /// Gets or sets the contract name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the contract groups
        /// </summary>
        public ContractGroup[]? Groups { get; set; }

        /// <summary>
        /// Gets or sets the supported standards
        /// </summary>
        public string[]? SupportedStandards { get; set; }

        /// <summary>
        /// Gets or sets the contract ABI
        /// </summary>
        public ContractAbi? Abi { get; set; }

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public ContractPermission[]? Permissions { get; set; }

        /// <summary>
        /// Gets or sets the trusted contracts
        /// </summary>
        public Hash160[]? Trusts { get; set; }

        /// <summary>
        /// Gets or sets extra metadata
        /// </summary>
        public Dictionary<string, object>? Extra { get; set; }
    }

    /// <summary>
    /// Represents a contract group
    /// </summary>
    public class ContractGroup
    {
        /// <summary>
        /// Gets or sets the public key
        /// </summary>
        public string? PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the signature
        /// </summary>
        public string? Signature { get; set; }
    }

    /// <summary>
    /// Represents a contract ABI
    /// </summary>
    public class ContractAbi
    {
        /// <summary>
        /// Gets or sets the methods
        /// </summary>
        public ContractMethod[]? Methods { get; set; }

        /// <summary>
        /// Gets or sets the events
        /// </summary>
        public ContractEvent[]? Events { get; set; }
    }

    /// <summary>
    /// Represents a contract method
    /// </summary>
    public class ContractMethod
    {
        /// <summary>
        /// Gets or sets the method name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters
        /// </summary>
        public ContractMethodParameter[]? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the return type
        /// </summary>
        public ContractParameterType ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the offset
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets whether the method is safe
        /// </summary>
        public bool Safe { get; set; }
    }

    /// <summary>
    /// Represents a contract method parameter
    /// </summary>
    public class ContractMethodParameter
    {
        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the parameter type
        /// </summary>
        public ContractParameterType Type { get; set; }
    }

    /// <summary>
    /// Represents a contract event
    /// </summary>
    public class ContractEvent
    {
        /// <summary>
        /// Gets or sets the event name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters
        /// </summary>
        public ContractMethodParameter[]? Parameters { get; set; }
    }

    /// <summary>
    /// Represents a contract permission
    /// </summary>
    public class ContractPermission
    {
        /// <summary>
        /// Gets or sets the contract hash or group
        /// </summary>
        public object? Contract { get; set; }

        /// <summary>
        /// Gets or sets the allowed methods
        /// </summary>
        public string[]? Methods { get; set; }
    }
}