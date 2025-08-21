using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Provides cryptographic hash functions used in Neo.
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// Computes SHA256 hash.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The SHA256 hash.</returns>
        public static byte[] SHA256(byte[] data)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(data);
        }

        /// <summary>
        /// Computes SHA256 hash of a string.
        /// </summary>
        /// <param name="data">The string to hash.</param>
        /// <returns>The SHA256 hash.</returns>
        public static byte[] SHA256(string data)
        {
            return SHA256(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Computes double SHA256 hash (SHA256(SHA256(data))).
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The double SHA256 hash.</returns>
        public static byte[] DoubleSHA256(byte[] data)
        {
            return SHA256(SHA256(data));
        }

        /// <summary>
        /// Computes RIPEMD160 hash.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The RIPEMD160 hash.</returns>
        public static byte[] RIPEMD160(byte[] data)
        {
            var digest = new RipeMD160Digest();
            digest.BlockUpdate(data, 0, data.Length);
            var output = new byte[digest.GetDigestSize()];
            digest.DoFinal(output, 0);
            return output;
        }

        /// <summary>
        /// Computes Hash160 (RIPEMD160(SHA256(data))).
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The Hash160 result.</returns>
        public static byte[] Hash160(byte[] data)
        {
            return RIPEMD160(SHA256(data));
        }

        /// <summary>
        /// Computes Hash256 (double SHA256).
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The Hash256 result.</returns>
        public static byte[] Hash256(byte[] data)
        {
            return DoubleSHA256(data);
        }

        /// <summary>
        /// Computes Murmur3 hash.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <param name="seed">The seed value.</param>
        /// <returns>The Murmur3 hash.</returns>
        public static uint Murmur3(byte[] data, uint seed = 0)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            const int r1 = 15;
            const int r2 = 13;
            const uint m = 5;
            const uint n = 0xe6546b64;

            uint hash = seed;
            int dataLength = data.Length;
            int blockCount = dataLength / 4;

            for (int i = 0; i < blockCount; i++)
            {
                uint k = BitConverter.ToUInt32(data, (int)(i * 4));

                k *= c1;
                k = RotateLeft(k, r1);
                k *= c2;

                hash ^= k;
                hash = RotateLeft(hash, r2);
                hash = hash * m + n;
            }

            uint remainingBytes = 0;
            int remainingOffset = blockCount * 4;
            int remainingLength = dataLength - remainingOffset;

            switch (remainingLength)
            {
                case 3:
                    remainingBytes ^= (uint)data[remainingOffset + 2] << 16;
                    goto case 2;
                case 2:
                    remainingBytes ^= (uint)data[remainingOffset + 1] << 8;
                    goto case 1;
                case 1:
                    remainingBytes ^= data[remainingOffset];
                    remainingBytes *= c1;
                    remainingBytes = RotateLeft(remainingBytes, (int)r1);
                    remainingBytes *= c2;
                    hash ^= remainingBytes;
                    break;
            }

            hash ^= (uint)dataLength;
            hash ^= hash >> 16;
            hash *= 0x85ebca6b;
            hash ^= hash >> 13;
            hash *= 0xc2b2ae35;
            hash ^= hash >> 16;

            return hash;
        }

        /// <summary>
        /// Computes HMAC-SHA256.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data to authenticate.</param>
        /// <returns>The HMAC-SHA256.</returns>
        public static byte[] HMACSHA256(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// Computes HMAC-SHA512.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data to authenticate.</param>
        /// <returns>The HMAC-SHA512.</returns>
        public static byte[] HMACSHA512(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA512(key);
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// Computes SHA3-256 hash.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The SHA3-256 hash.</returns>
        public static byte[] SHA3_256(byte[] data)
        {
            var digest = new Sha3Digest(256);
            digest.BlockUpdate(data, 0, data.Length);
            var output = new byte[digest.GetDigestSize()];
            digest.DoFinal(output, 0);
            return output;
        }

        /// <summary>
        /// Computes Keccak256 hash.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The Keccak256 hash.</returns>
        public static byte[] Keccak256(byte[] data)
        {
            var digest = new KeccakDigest(256);
            digest.BlockUpdate(data, 0, data.Length);
            var output = new byte[digest.GetDigestSize()];
            digest.DoFinal(output, 0);
            return output;
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        /// <summary>
        /// Computes Murmur32 hash for a string (alias for Murmur3 with UTF8 encoding)
        /// </summary>
        /// <param name="text">The text to hash</param>
        /// <param name="seed">The seed value (default 0)</param>
        /// <returns>The Murmur32 hash</returns>
        public static uint Murmur32(string text, uint seed = 0)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            return Murmur3(bytes, seed);
        }
    }
}