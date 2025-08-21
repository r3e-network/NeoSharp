using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NeoSharp.Crypto;
using NeoSharp.Wallet;
using NeoSharp.Utils;

namespace NeoSharp.Types
{
    /// <summary>
    /// Represents a contract parameter for smart contract invocations
    /// </summary>
    public sealed class ContractParameter : IEquatable<ContractParameter>
    {
        /// <summary>
        /// The parameter name (optional)
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// The parameter type
        /// </summary>
        public ContractParameterType Type { get; }

        /// <summary>
        /// The parameter value
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Initializes a new contract parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="type">The parameter type</param>
        /// <param name="value">The parameter value</param>
        public ContractParameter(string? name, ContractParameterType type, object? value = null)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Initializes a new contract parameter without a name
        /// </summary>
        /// <param name="type">The parameter type</param>
        /// <param name="value">The parameter value</param>
        public ContractParameter(ContractParameterType type, object? value = null)
            : this(null, type, value)
        {
        }

        /// <summary>
        /// Creates an Any parameter
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Any(object? value = null) =>
            new(ContractParameterType.Any, value);

        /// <summary>
        /// Creates a String parameter
        /// </summary>
        /// <param name="value">The string value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter String(string value) =>
            new(ContractParameterType.String, value);

        /// <summary>
        /// Creates a ByteArray parameter from bytes
        /// </summary>
        /// <param name="value">The byte array</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter ByteArray(byte[] value) =>
            new(ContractParameterType.ByteArray, value);

        /// <summary>
        /// Creates a ByteArray parameter from hex string
        /// </summary>
        /// <param name="value">The hex string</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter ByteArray(string value)
        {
            if (!value.IsValidHex())
                throw new ArgumentException("Argument is not a valid hex string", nameof(value));
            
            return new ContractParameter(ContractParameterType.ByteArray, value.FromHexString());
        }

        /// <summary>
        /// Creates a ByteArray parameter from UTF-8 string
        /// </summary>
        /// <param name="value">The string value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter ByteArrayFromString(string value) =>
            new(ContractParameterType.ByteArray, System.Text.Encoding.UTF8.GetBytes(value));

        /// <summary>
        /// Creates a ContractParameter from an object value
        /// </summary>
        /// <param name="value">The object value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter FromObject(object? value)
        {
            return value switch
            {
                null => new ContractParameter(ContractParameterType.Any, null),
                bool b => Boolean(b),
                int i => Integer(i),
                long l => Integer(l),
                string s => String(s),
                byte[] bytes => ByteArray(bytes),
                Hash160 hash160 => ByteArray(hash160.ToArray()),
                Hash256 hash256 => ByteArray(hash256.ToArray()),
                _ => String(value.ToString() ?? "")
            };
        }

        /// <summary>
        /// Creates a Signature parameter from bytes
        /// </summary>
        /// <param name="value">The signature bytes</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Signature(byte[] value)
        {
            if (value.Length != NeoConstants.SignatureSize)
                throw new ArgumentException($"Signature must be {NeoConstants.SignatureSize} bytes, but was {value.Length}", nameof(value));
            
            return new ContractParameter(ContractParameterType.Signature, value);
        }

        /// <summary>
        /// Creates a Signature parameter from signature data
        /// </summary>
        /// <param name="value">The signature data</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Signature(SignatureData value) =>
            Signature(value.Concatenated);

        /// <summary>
        /// Creates a Signature parameter from hex string
        /// </summary>
        /// <param name="value">The hex string</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Signature(string value)
        {
            if (!value.IsValidHex())
                throw new ArgumentException("Argument is not a valid hex string", nameof(value));
            
            return Signature(value.HexToBytes());
        }

        /// <summary>
        /// Creates a Boolean parameter
        /// </summary>
        /// <param name="value">The boolean value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Boolean(bool value) =>
            new(ContractParameterType.Boolean, value);

        /// <summary>
        /// Creates an Integer parameter from int
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Integer(int value) =>
            new(ContractParameterType.Integer, value);

        /// <summary>
        /// Creates an Integer parameter from byte
        /// </summary>
        /// <param name="value">The byte value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Integer(byte value) =>
            Integer((int)value);

        /// <summary>
        /// Creates an Integer parameter from BigInteger
        /// </summary>
        /// <param name="value">The BigInteger value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Integer(BigInteger value) =>
            new(ContractParameterType.Integer, value);

        /// <summary>
        /// Creates a Hash160 parameter from account
        /// </summary>
        /// <param name="account">The account</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Hash160(Account account) =>
            new(ContractParameterType.Hash160, account.ScriptHash);

        /// <summary>
        /// Creates a Hash160 parameter from hash
        /// </summary>
        /// <param name="value">The Hash160 value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Hash160(Hash160 value) =>
            new(ContractParameterType.Hash160, value);

        /// <summary>
        /// Creates a Hash256 parameter from hash
        /// </summary>
        /// <param name="value">The Hash256 value</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Hash256(Hash256 value) =>
            new(ContractParameterType.Hash256, value);

        /// <summary>
        /// Creates a Hash256 parameter from bytes
        /// </summary>
        /// <param name="value">The hash bytes</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Hash256(byte[] value) =>
            new(ContractParameterType.Hash256, new Hash256(value));

        /// <summary>
        /// Creates a Hash256 parameter from hex string
        /// </summary>
        /// <param name="value">The hex string</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Hash256(string value) =>
            new(ContractParameterType.Hash256, new Hash256(value));

        /// <summary>
        /// Creates a PublicKey parameter from bytes
        /// </summary>
        /// <param name="value">The public key bytes</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter PublicKey(byte[] value)
        {
            if (value.Length != NeoConstants.PublicKeySizeCompressed)
                throw new ArgumentException($"Public key must be {NeoConstants.PublicKeySizeCompressed} bytes, but was {value.Length}", nameof(value));
            
            return new ContractParameter(ContractParameterType.PublicKey, value);
        }

        /// <summary>
        /// Creates a PublicKey parameter from hex string
        /// </summary>
        /// <param name="value">The hex string</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter PublicKey(string value) =>
            PublicKey(value.HexToBytes());

        /// <summary>
        /// Creates a PublicKey parameter from ECPublicKey
        /// </summary>
        /// <param name="value">The public key</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter PublicKey(ECPublicKey value) =>
            PublicKey(value.GetEncoded(true));

        /// <summary>
        /// Creates an Array parameter
        /// </summary>
        /// <param name="values">The array elements</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Array(params object[] values)
        {
            var parameters = new ContractParameter[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                parameters[i] = MapToContractParameter(values[i]);
            }
            return new ContractParameter(ContractParameterType.Array, parameters);
        }

        /// <summary>
        /// Creates a Map parameter
        /// </summary>
        /// <param name="values">The map entries</param>
        /// <returns>A new contract parameter</returns>
        public static ContractParameter Map(IDictionary<object, object> values)
        {
            if (!values.Any())
                throw new ArgumentException("At least one map entry is required", nameof(values));

            var map = new Dictionary<ContractParameter, ContractParameter>();
            foreach (var kvp in values)
            {
                var key = MapToContractParameter(kvp.Key);
                var value = MapToContractParameter(kvp.Value);
                
                if (key.Type == ContractParameterType.Array || key.Type == ContractParameterType.Map)
                    throw new ArgumentException("Map keys cannot be of type Array or Map");
                
                map[key] = value;
            }
            
            return new ContractParameter(ContractParameterType.Map, map);
        }

        /// <summary>
        /// Maps an object to the appropriate contract parameter type
        /// </summary>
        /// <param name="value">The object to map</param>
        /// <returns>The contract parameter</returns>
        public static ContractParameter MapToContractParameter(object? value)
        {
            return value switch
            {
                null => Any(null),
                ContractParameter cp => cp,
                bool b => Boolean(b),
                byte bt => Integer(bt),
                int i => Integer(i),
                BigInteger bi => Integer(bi),
                byte[] bytes => ByteArray(bytes),
                string s => String(s),
                Hash160 h160 => Hash160(h160),
                Hash256 h256 => Hash256(h256),
                Account acc => Hash160(acc),
                ECPublicKey pk => PublicKey(pk),
                SignatureData sig => Signature(sig),
                object[] array => Array(array),
                IDictionary<object, object> dict => Map(dict),
                _ => throw new ArgumentException($"Cannot map type {value.GetType().Name} to contract parameter", nameof(value))
            };
        }

        /// <summary>
        /// Determines whether this parameter equals another
        /// </summary>
        /// <param name="other">The other parameter to compare</param>
        /// <returns>True if the parameters are equal</returns>
        public bool Equals(ContractParameter? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Type == other.Type && Equals(Value, other.Value);
        }

        /// <summary>
        /// Determines whether this parameter equals another object
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object? obj) => Equals(obj as ContractParameter);

        /// <summary>
        /// Gets the hash code for this parameter
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode() => HashCode.Combine(Name, Type, Value);

        /// <summary>
        /// Returns a string representation of this parameter
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() => 
            $"ContractParameter(Name={Name}, Type={Type}, Value={Value})";

        public static bool operator ==(ContractParameter? left, ContractParameter? right) => 
            EqualityComparer<ContractParameter>.Default.Equals(left, right);

        public static bool operator !=(ContractParameter? left, ContractParameter? right) => 
            !(left == right);

        /// <summary>
        /// Converts the contract parameter to JSON format for RPC calls.
        /// </summary>
        /// <returns>A dictionary representing the JSON format.</returns>
        public Dictionary<string, object> ToJson()
        {
            var json = new Dictionary<string, object>
            {
                ["type"] = Type.ToString()
            };

            switch (Type)
            {
                case ContractParameterType.Boolean:
                    json["value"] = Value is bool b ? b : bool.Parse(Value?.ToString() ?? "false");
                    break;
                case ContractParameterType.Integer:
                    json["value"] = Value?.ToString() ?? "0";
                    break;
                case ContractParameterType.ByteArray:
                    json["value"] = Value is byte[] bytes ? Convert.ToBase64String(bytes) : Value?.ToString() ?? "";
                    break;
                case ContractParameterType.String:
                    json["value"] = Value?.ToString() ?? "";
                    break;
                case ContractParameterType.Hash160:
                    json["value"] = Value is Hash160 h160 ? h160.ToString() : Value?.ToString() ?? "";
                    break;
                case ContractParameterType.Hash256:
                    json["value"] = Value is Hash256 h256 ? h256.ToString() : Value?.ToString() ?? "";
                    break;
                case ContractParameterType.PublicKey:
                    json["value"] = Value is byte[] pk ? pk.ToHexString() : Value?.ToString() ?? "";
                    break;
                case ContractParameterType.Signature:
                    json["value"] = Value is byte[] sig ? Convert.ToBase64String(sig) : Value?.ToString() ?? "";
                    break;
                case ContractParameterType.Array:
                    if (Value is IEnumerable<ContractParameter> array)
                    {
                        json["value"] = array.Select(p => p.ToJson()).ToList();
                    }
                    else if (Value is ContractParameter[] cpArray)
                    {
                        json["value"] = cpArray.Select(p => p.ToJson()).ToList();
                    }
                    else
                    {
                        json["value"] = new List<object>();
                    }
                    break;
                case ContractParameterType.Map:
                    if (Value is IDictionary<ContractParameter, ContractParameter> map)
                    {
                        json["value"] = map.Select(kvp => new Dictionary<string, object>
                        {
                            ["key"] = kvp.Key.ToJson(),
                            ["value"] = kvp.Value.ToJson()
                        }).ToList();
                    }
                    else
                    {
                        json["value"] = new List<object>();
                    }
                    break;
                case ContractParameterType.InteropInterface:
                    json["value"] = Value?.ToString() ?? "";
                    break;
                case ContractParameterType.Any:
                case ContractParameterType.Void:
                default:
                    json["value"] = null;
                    break;
            }

            return json;
        }
    }
}