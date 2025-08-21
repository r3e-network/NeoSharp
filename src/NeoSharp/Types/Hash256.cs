using System;
using System.Linq;
using System.Numerics;
using NeoSharp.Crypto;
using NeoSharp.Serialization;
using NeoSharp.Utils;
using BinaryWriter = NeoSharp.Serialization.BinaryWriter;
using BinaryReader = NeoSharp.Serialization.BinaryReader;

namespace NeoSharp.Types
{
    /// <summary>
    /// A Hash256 is a 32 bytes long hash created from some data by applying SHA-256.
    /// These hashes are typically used for block hashes, transaction hashes, and Merkle tree nodes.
    /// </summary>
    public readonly struct Hash256 : IEquatable<Hash256>, IComparable<Hash256>, INeoSerializable
    {
        /// <summary>
        /// The hash is stored as a byte array in big-endian order.
        /// </summary>
        private readonly byte[] _hash;

        /// <summary>
        /// A zero-value hash.
        /// </summary>
        public static readonly Hash256 Zero = new(new byte[32]);

        /// <summary>
        /// The hash as a hexadecimal string in big-endian order without the '0x' prefix.
        /// </summary>
        public string ToHex() => _hash?.ToHexString() ?? string.Empty;

        /// <summary>
        /// The size of a Hash256 in bytes.
        /// </summary>
        public int Size => 32;

        /// <summary>
        /// Constructs a new hash with 32 zero bytes.
        /// </summary>
        public Hash256()
        {
            _hash = new byte[32];
        }

        /// <summary>
        /// Constructs a new hash from the given byte array. The byte array must be in big-endian order and 256 bits long.
        /// </summary>
        /// <param name="hash">The hash in big-endian order</param>
        /// <exception cref="ArgumentException">Thrown if hash is not exactly 32 bytes</exception>
        public Hash256(byte[] hash)
        {
            if (hash == null || hash.Length != 32)
                throw new ArgumentException($"Hash must be 32 bytes long but was {hash?.Length ?? 0} bytes.");
            
            _hash = new byte[32];
            Array.Copy(hash, _hash, 32);
        }

        /// <summary>
        /// Constructs a new hash from the given hexadecimal string. The string must be in big-endian order and 256 bits long.
        /// </summary>
        /// <param name="hash">The hash in big-endian order as hex string</param>
        /// <exception cref="ArgumentException">Thrown if hash string is invalid</exception>
        public Hash256(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash string cannot be null or empty");

            if (!hash.IsValidHex())
                throw new ArgumentException("String argument is not hexadecimal.");

            var bytes = hash.FromHexString();
            if (bytes.Length != 32)
                throw new ArgumentException($"Hash must be 32 bytes long but was {bytes.Length} bytes.");

            _hash = bytes;
        }

        /// <summary>
        /// Returns the hash as a byte array in big-endian order.
        /// </summary>
        /// <returns>The hash as byte array</returns>
        public byte[] ToArray() => (byte[])_hash.Clone();

        /// <summary>
        /// Returns the hash as a byte array in little-endian order.
        /// </summary>
        /// <returns>The hash as byte array in little-endian order</returns>
        public byte[] ToLittleEndianArray()
        {
            var result = new byte[32];
            Array.Copy(_hash, result, 32);
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Parses a Hash256 from a hexadecimal string
        /// </summary>
        /// <param name="hex">The hexadecimal string</param>
        /// <returns>The parsed Hash256</returns>
        public static Hash256 Parse(string hex)
        {
            return new Hash256(hex);
        }

        /// <summary>
        /// Creates a hash from the given data by applying SHA-256.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The hash</returns>
        public static Hash256 FromData(byte[] data)
        {
            var hash = Hash.SHA256(data);
            return new Hash256(hash);
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
        /// <returns>The deserialized hash</returns>
        public void Deserialize(Serialization.BinaryReader reader)
        {
            var bytes = reader.ReadBytes(32);
            Array.Reverse(bytes); // Convert from little-endian to big-endian
            Array.Copy(bytes, _hash, 32);
        }

        public bool Equals(Hash256 other)
        {
            if (_hash == null && other._hash == null) return true;
            if (_hash == null || other._hash == null) return false;
            return _hash.SequenceEqual(other._hash);
        }

        public override bool Equals(object? obj)
        {
            return obj is Hash256 hash && Equals(hash);
        }

        public override int GetHashCode()
        {
            return _hash?.GetSequenceHashCode() ?? 0;
        }

        public int CompareTo(Hash256 other)
        {
            if (_hash == null && other._hash == null) return 0;
            if (_hash == null) return -1;
            if (other._hash == null) return 1;
            
            var thisBigInt = new BigInteger(_hash, isUnsigned: true, isBigEndian: true);
            var otherBigInt = new BigInteger(other._hash, isUnsigned: true, isBigEndian: true);
            return thisBigInt.CompareTo(otherBigInt);
        }

        public override string ToString() => ToHex();

        public static bool operator ==(Hash256 left, Hash256 right) => left.Equals(right);
        public static bool operator !=(Hash256 left, Hash256 right) => !left.Equals(right);
        public static bool operator <(Hash256 left, Hash256 right) => left.CompareTo(right) < 0;
        public static bool operator >(Hash256 left, Hash256 right) => left.CompareTo(right) > 0;
        public static bool operator <=(Hash256 left, Hash256 right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Hash256 left, Hash256 right) => left.CompareTo(right) >= 0;

        public static implicit operator string(Hash256 hash) => hash.ToHex();
        public static explicit operator Hash256(string hex) => new(hex);
    }
}