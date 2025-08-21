using System;
using System.Security.Cryptography;
using System.Text;
using NeoSharp.Utils;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Cryptographic hash function extensions for byte arrays and strings
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>
        /// Applies SHA-256 to the input and returns the result.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>The SHA256 hash value as byte array</returns>
        public static byte[] Sha256(this byte[] bytes)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        /// Applies SHA-256 twice to the input and returns the result.
        /// Neo uses the name 'hash256' for hashes created in this way.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>The hash value as byte array</returns>
        public static byte[] Hash256(this byte[] bytes)
        {
            using var sha256 = SHA256.Create();
            var first = sha256.ComputeHash(bytes);
            return sha256.ComputeHash(first);
        }

        /// <summary>
        /// RipeMD-160 hash function
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>The hash value as byte array</returns>
        public static byte[] Ripemd160(this byte[] bytes)
        {
            return Hash.RIPEMD160(bytes);
        }

        /// <summary>
        /// Performs a SHA256 followed by a RIPEMD160
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>The hash value as byte array</returns>
        public static byte[] Sha256ThenRipemd160(this byte[] bytes)
        {
            using var sha256 = SHA256.Create();
            var sha = sha256.ComputeHash(bytes);
            return sha.Ripemd160();
        }

        /// <summary>
        /// Generates the HMAC SHA-512 digest for the bytes with the given key
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <param name="key">The key</param>
        /// <returns>The hash value for the given input</returns>
        public static byte[] HmacSha512(this byte[] bytes, byte[] key)
        {
            using var hmac = new HMACSHA512(key);
            return hmac.ComputeHash(bytes);
        }

        /// <summary>
        /// Applies SHA-256 twice to the input and returns the result.
        /// Neo uses the name 'hash256' for hashes created in this way.
        /// </summary>
        /// <param name="str">The input string</param>
        /// <returns>The hash value as hexadecimal string</returns>
        public static string Hash256(this string str)
        {
            return str.ToByteArray().Hash256().ToHexString();
        }

        /// <summary>
        /// RipeMD-160 hash function
        /// </summary>
        /// <param name="str">The input string</param>
        /// <returns>The hash value as hexadecimal string</returns>
        public static string Ripemd160(this string str)
        {
            return str.ToByteArray().Ripemd160().ToHexString();
        }

        /// <summary>
        /// Performs a SHA256 followed by a RIPEMD160
        /// </summary>
        /// <param name="str">The input string</param>
        /// <returns>The hash value as hexadecimal string</returns>
        public static string Sha256ThenRipemd160(this string str)
        {
            return str.ToByteArray().Sha256ThenRipemd160().ToHexString();
        }

        /// <summary>
        /// Generates the HMAC SHA-512 digest for the string with the given key
        /// </summary>
        /// <param name="str">The input string</param>
        /// <param name="key">The key as string</param>
        /// <returns>The hash value as hexadecimal string</returns>
        public static string HmacSha512(this string str, string key)
        {
            return str.ToByteArray().HmacSha512(key.ToByteArray()).ToHexString();
        }
    }
}