using System;
using System.Text.Json.Serialization;

namespace NeoSharp.Types
{
    /// <summary>
    /// Represents the different types of contract parameters supported by Neo
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContractParameterType : byte
    {
        /// <summary>
        /// Represents any type (null/void)
        /// </summary>
        Any = 0x00,

        /// <summary>
        /// Boolean type
        /// </summary>
        Boolean = 0x10,

        /// <summary>
        /// Signed integer type
        /// </summary>
        Integer = 0x11,

        /// <summary>
        /// Byte array type
        /// </summary>
        ByteArray = 0x12,

        /// <summary>
        /// String type
        /// </summary>
        String = 0x13,

        /// <summary>
        /// 160-bit hash type (typically for addresses/script hashes)
        /// </summary>
        Hash160 = 0x14,

        /// <summary>
        /// 256-bit hash type (typically for transaction/block hashes)
        /// </summary>
        Hash256 = 0x15,

        /// <summary>
        /// Public key type (33 bytes compressed format)
        /// </summary>
        PublicKey = 0x16,

        /// <summary>
        /// Digital signature type (64 bytes ECDSA signature)
        /// </summary>
        Signature = 0x17,

        /// <summary>
        /// Array type containing multiple parameters
        /// </summary>
        Array = 0x20,

        /// <summary>
        /// Map/dictionary type with key-value pairs
        /// </summary>
        Map = 0x21,

        /// <summary>
        /// Interop interface type for complex objects
        /// </summary>
        InteropInterface = 0x30,

        /// <summary>
        /// Void type (no return value)
        /// </summary>
        Void = 0xFF
    }

    /// <summary>
    /// Extension methods for ContractParameterType
    /// </summary>
    public static class ContractParameterTypeExtensions
    {
        /// <summary>
        /// Gets the JSON representation value for this parameter type
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <returns>The JSON string value</returns>
        public static string GetJsonValue(this ContractParameterType type)
        {
            return type switch
            {
                ContractParameterType.Any => "Any",
                ContractParameterType.Boolean => "Boolean",
                ContractParameterType.Integer => "Integer",
                ContractParameterType.ByteArray => "ByteArray",
                ContractParameterType.String => "String",
                ContractParameterType.Hash160 => "Hash160",
                ContractParameterType.Hash256 => "Hash256",
                ContractParameterType.PublicKey => "PublicKey",
                ContractParameterType.Signature => "Signature",
                ContractParameterType.Array => "Array",
                ContractParameterType.Map => "Map",
                ContractParameterType.InteropInterface => "InteropInterface",
                ContractParameterType.Void => "Void",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown contract parameter type")
            };
        }

        /// <summary>
        /// Parses a ContractParameterType from its JSON string value
        /// </summary>
        /// <param name="jsonValue">The JSON string value</param>
        /// <returns>The corresponding ContractParameterType</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON value is not recognized</exception>
        public static ContractParameterType FromJsonValue(string jsonValue)
        {
            return jsonValue switch
            {
                "Any" => ContractParameterType.Any,
                "Boolean" => ContractParameterType.Boolean,
                "Integer" => ContractParameterType.Integer,
                "ByteArray" => ContractParameterType.ByteArray,
                "String" => ContractParameterType.String,
                "Hash160" => ContractParameterType.Hash160,
                "Hash256" => ContractParameterType.Hash256,
                "PublicKey" => ContractParameterType.PublicKey,
                "Signature" => ContractParameterType.Signature,
                "Array" => ContractParameterType.Array,
                "Map" => ContractParameterType.Map,
                "InteropInterface" => ContractParameterType.InteropInterface,
                "Void" => ContractParameterType.Void,
                _ => throw new ArgumentException($"Unknown parameter type: {jsonValue}", nameof(jsonValue))
            };
        }

        /// <summary>
        /// Determines if this parameter type represents a collection
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <returns>True if the type is a collection (Array or Map)</returns>
        public static bool IsCollection(this ContractParameterType type)
        {
            return type == ContractParameterType.Array || type == ContractParameterType.Map;
        }

        /// <summary>
        /// Determines if this parameter type represents a hash
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <returns>True if the type is a hash (Hash160 or Hash256)</returns>
        public static bool IsHash(this ContractParameterType type)
        {
            return type == ContractParameterType.Hash160 || type == ContractParameterType.Hash256;
        }

        /// <summary>
        /// Determines if this parameter type represents binary data
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <returns>True if the type represents binary data</returns>
        public static bool IsBinaryData(this ContractParameterType type)
        {
            return type is ContractParameterType.ByteArray or 
                        ContractParameterType.Signature or 
                        ContractParameterType.PublicKey or
                        ContractParameterType.Hash160 or 
                        ContractParameterType.Hash256;
        }
    }
}