using System;
using System.Linq;
using System.Numerics;
using NeoSharp.Crypto;
using NeoSharp.Script;
using NeoSharp.Serialization;
using NeoSharp.Utils;
using BinaryWriter = NeoSharp.Serialization.BinaryWriter;
using BinaryReader = NeoSharp.Serialization.BinaryReader;

namespace NeoSharp.Types
{
    /// <summary>
    /// A Hash160 is a 20 bytes long hash created from some data by first applying SHA-256 and then RIPEMD-160.
    /// These hashes are mostly used for obtaining the script hash of a smart contract or an account.
    /// </summary>
    public readonly struct Hash160 : IEquatable<Hash160>, IComparable<Hash160>, INeoSerializable
    {
        /// <summary>
        /// The hash is stored as a byte array in big-endian order.
        /// </summary>
        private readonly byte[] _hash;

        /// <summary>
        /// A zero-value hash.
        /// </summary>
        public static readonly Hash160 Zero = new(new byte[20]);

        /// <summary>
        /// The script hash as a hexadecimal string in big-endian order without the '0x' prefix.
        /// </summary>
        public string ToHex() => _hash?.ToHexString() ?? string.Empty;

        /// <summary>
        /// The size of a Hash160 in bytes.
        /// </summary>
        public int Size => 20;

        /// <summary>
        /// Constructs a new hash with 20 zero bytes.
        /// </summary>
        public Hash160()
        {
            _hash = new byte[20];
        }

        /// <summary>
        /// Constructs a new hash from the given byte array. The byte array must be in big-endian order and 160 bits long.
        /// </summary>
        /// <param name="hash">The hash in big-endian order</param>
        /// <exception cref="ArgumentException">Thrown if hash is not exactly 20 bytes</exception>
        public Hash160(byte[] hash)
        {
            if (hash == null || hash.Length != 20)
                throw new ArgumentException($"Hash must be 20 bytes long but was {hash?.Length ?? 0} bytes.");
            
            _hash = new byte[20];
            Array.Copy(hash, _hash, 20);
        }

        /// <summary>
        /// Constructs a new hash from the given hexadecimal string. The string must be in big-endian order and 160 bits long.
        /// </summary>
        /// <param name="hash">The hash in big-endian order as hex string</param>
        /// <exception cref="ArgumentException">Thrown if hash string is invalid</exception>
        public Hash160(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash string cannot be null or empty");

            if (!hash.IsValidHex())
                throw new ArgumentException("String argument is not hexadecimal.");

            var bytes = hash.FromHexString();
            if (bytes.Length != 20)
                throw new ArgumentException($"Hash must be 20 bytes long but was {bytes.Length} bytes.");

            _hash = bytes;
        }

        /// <summary>
        /// Returns the script hash as a byte array in big-endian order.
        /// </summary>
        /// <returns>The hash as byte array</returns>
        public byte[] ToArray() => (byte[])_hash.Clone();

        /// <summary>
        /// Returns the script hash as a byte array in little-endian order.
        /// </summary>
        /// <returns>The hash as byte array in little-endian order</returns>
        public byte[] ToLittleEndianArray()
        {
            var result = new byte[20];
            Array.Copy(_hash, result, 20);
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns the address corresponding to this script hash.
        /// </summary>
        /// <returns>The address string</returns>
        public string ToAddress() => _hash.ScriptHashToAddress();

        /// <summary>
        /// Creates a script hash from the given address.
        /// </summary>
        /// <param name="address">The address from which to derive the script hash</param>
        /// <returns>The script hash</returns>
        /// <exception cref="ArgumentException">Thrown if address is invalid</exception>
        public static Hash160 FromAddress(string address)
        {
            return new Hash160(address.AddressToScriptHash());
        }

        /// <summary>
        /// Parses a Hash160 from a hexadecimal string.
        /// </summary>
        /// <param name="hex">The hexadecimal string</param>
        /// <returns>The parsed Hash160</returns>
        /// <exception cref="ArgumentException">Thrown if hex string is invalid</exception>
        public static Hash160 Parse(string hex)
        {
            return new Hash160(hex);
        }

        /// <summary>
        /// Creates a script hash from the given script in byte array form.
        /// </summary>
        /// <param name="script">The script to calculate the script hash for</param>
        /// <returns>The script hash</returns>
        public static Hash160 FromScript(byte[] script)
        {
            var hash = Hash.Hash160(script);
            Array.Reverse(hash); // Convert to big-endian
            return new Hash160(hash);
        }

        /// <summary>
        /// Creates a script hash from the given script in hexadecimal string form.
        /// </summary>
        /// <param name="script">The script to calculate the script hash for</param>
        /// <returns>The script hash</returns>
        public static Hash160 FromScript(string script) => FromScript(script.FromHexString());

        /// <summary>
        /// Creates a script hash from the given public key.
        /// </summary>
        /// <param name="encodedPublicKey">The encoded public key</param>
        /// <returns>The script hash</returns>
        public static Hash160 FromPublicKey(byte[] encodedPublicKey)
        {
            return FromScript(ScriptBuilder.BuildVerificationScript(encodedPublicKey));
        }

        /// <summary>
        /// Creates a script hash from the given public keys for multi-sig.
        /// </summary>
        /// <param name="publicKeys">The public keys</param>
        /// <param name="signingThreshold">The signing threshold</param>
        /// <returns>The script hash</returns>
        public static Hash160 FromPublicKeys(ECPublicKey[] publicKeys, int signingThreshold)
        {
            return FromScript(ScriptBuilder.BuildVerificationScript(publicKeys, signingThreshold));
        }

        /// <summary>
        /// Serializes this hash to the given writer.
        /// </summary>
        /// <param name="writer">The binary writer</param>
        public void Serialize(Serialization.BinaryWriter writer)
        {
            var littleEndian = ToLittleEndianArray();
            writer.Write(littleEndian);
        }

        /// <summary>
        /// Deserializes a hash from the given reader.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public void Deserialize(Serialization.BinaryReader reader)
        {
            var bytes = reader.ReadBytes(20);
            Array.Reverse(bytes); // Convert from little-endian to big-endian
            Array.Copy(bytes, _hash, 20);
        }

        public bool Equals(Hash160 other)
        {
            if (_hash == null && other._hash == null) return true;
            if (_hash == null || other._hash == null) return false;
            return _hash.SequenceEqual(other._hash);
        }

        public override bool Equals(object? obj)
        {
            return obj is Hash160 hash && Equals(hash);
        }

        public override int GetHashCode()
        {
            return _hash?.GetSequenceHashCode() ?? 0;
        }

        public int CompareTo(Hash160 other)
        {
            if (_hash == null && other._hash == null) return 0;
            if (_hash == null) return -1;
            if (other._hash == null) return 1;
            
            var thisBigInt = new BigInteger(_hash, isUnsigned: true, isBigEndian: true);
            var otherBigInt = new BigInteger(other._hash, isUnsigned: true, isBigEndian: true);
            return thisBigInt.CompareTo(otherBigInt);
        }

        public override string ToString() => ToHex();

        public static bool operator ==(Hash160 left, Hash160 right) => left.Equals(right);
        public static bool operator !=(Hash160 left, Hash160 right) => !left.Equals(right);
        public static bool operator <(Hash160 left, Hash160 right) => left.CompareTo(right) < 0;
        public static bool operator >(Hash160 left, Hash160 right) => left.CompareTo(right) > 0;
        public static bool operator <=(Hash160 left, Hash160 right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Hash160 left, Hash160 right) => left.CompareTo(right) >= 0;

        public static implicit operator string(Hash160 hash) => hash.ToHex();
        public static explicit operator Hash160(string hex) => new(hex);
    }
}