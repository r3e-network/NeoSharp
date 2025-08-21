using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using NeoSharp.Types;
using NeoSharp.Utils;

namespace NeoSharp.Utils
{
    /// <summary>
    /// Extension methods for various types
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Converts a hex string to byte array
        /// </summary>
        /// <param name="hex">The hex string</param>
        /// <returns>The byte array</returns>
        public static byte[] ToByteArray(this string hex)
        {
            return hex.FromHexString();
        }

        /// <summary>
        /// Converts a byte array to script hash
        /// </summary>
        /// <param name="script">The script byte array</param>
        /// <returns>The script hash</returns>
        public static Hash160 ToScriptHash(this byte[] script)
        {
            return Hash160.FromScript(script);
        }

        /// <summary>
        /// Converts a BigInteger to decimal representation
        /// </summary>
        /// <param name="value">The BigInteger value</param>
        /// <param name="decimals">Number of decimal places</param>
        /// <returns>The decimal value</returns>
        public static decimal ToDecimal(this BigInteger value, int decimals = 8)
        {
            if (decimals == 0)
                return (decimal)value;

            var divisor = BigInteger.Pow(10, decimals);
            var quotient = BigInteger.DivRem(value, divisor, out var remainder);
            
            var result = (decimal)quotient;
            if (remainder != 0)
            {
                var fractional = (decimal)remainder / (decimal)divisor;
                result += fractional;
            }
            
            return result;
        }

        /// <summary>
        /// Converts a long to decimal representation
        /// </summary>
        /// <param name="value">The long value</param>
        /// <param name="decimals">Number of decimal places</param>
        /// <returns>The decimal value</returns>
        public static decimal ToDecimal(this long value, int decimals = 8)
        {
            return ((BigInteger)value).ToDecimal(decimals);
        }


        /// <summary>
        /// Converts bytes to hexadecimal string with 0x prefix
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The hex string with 0x prefix</returns>
        public static string ToHexStringWithPrefix(this byte[] bytes)
        {
            return "0x" + bytes.ToHexString();
        }

        /// <summary>
        /// Checks if a string is valid hexadecimal
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if valid hex</returns>
        public static bool IsValidHex(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            // Remove 0x prefix if present
            if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                str = str[2..];

            // Must have even length
            if (str.Length % 2 != 0)
                return false;

            // Check if all characters are hex digits
            foreach (char c in str)
            {
                if (!((c >= '0' && c <= '9') || 
                      (c >= 'a' && c <= 'f') || 
                      (c >= 'A' && c <= 'F')))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts hex string to byte array
        /// </summary>
        /// <param name="hex">The hex string</param>
        /// <returns>The byte array</returns>
        public static byte[] FromHexString(this string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Array.Empty<byte>();

            // Remove 0x prefix if present
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex[2..];

            if (!hex.IsValidHex())
                throw new ArgumentException("Invalid hexadecimal string", nameof(hex));

            var result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            }

            return result;
        }

        /// <summary>
        /// Encodes bytes to Base64
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The Base64 string</returns>
        public static string Base64Encode(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decodes Base64 string to bytes
        /// </summary>
        /// <param name="base64">The Base64 string</param>
        /// <returns>The byte array</returns>
        public static byte[] Base64Decode(this string base64)
        {
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Gets sequence hash code for byte arrays
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The hash code</returns>
        public static int GetSequenceHashCode(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return 0;

            var hash = new HashCode();
            foreach (var b in bytes)
            {
                hash.Add(b);
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// Gets sequence hash code for arrays
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="items">The array</param>
        /// <returns>The hash code</returns>
        public static int GetSequenceHashCode<T>(this T[] items)
        {
            if (items == null || items.Length == 0)
                return 0;

            var hash = new HashCode();
            foreach (var item in items)
            {
                hash.Add(item);
            }
            return hash.ToHashCode();
        }
    }
}