using System;
using System.Linq;

namespace NeoSharp.Utils
{
    /// <summary>
    /// Extension methods for hex string operations.
    /// </summary>
    public static class HexExtensions
    {
        /// <summary>
        /// Converts a hex string to byte array.
        /// </summary>
        /// <param name="hex">The hex string.</param>
        /// <returns>The byte array.</returns>
        public static byte[] HexToBytes(this string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Array.Empty<byte>();
            
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);
            
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Invalid hex string length", nameof(hex));
            
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            
            return bytes;
        }

        /// <summary>
        /// Converts a byte array to hex string.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The hex string.</returns>
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
            
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        /// <summary>
        /// Converts a byte array to hex string with 0x prefix.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>The hex string with 0x prefix.</returns>
        public static string ToHexStringWithPrefix(this byte[] bytes)
        {
            return "0x" + bytes.ToHexString();
        }

        /// <summary>
        /// Checks if a string is valid hex.
        /// </summary>
        /// <param name="hex">The string to check.</param>
        /// <returns>True if valid hex, false otherwise.</returns>
        public static bool IsHex(this string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return false;
            
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);
            
            return hex.Length % 2 == 0 && hex.All(c => 
                (c >= '0' && c <= '9') || 
                (c >= 'a' && c <= 'f') || 
                (c >= 'A' && c <= 'F'));
        }

        /// <summary>
        /// Reverses a hex string (useful for endianness conversion).
        /// </summary>
        /// <param name="hex">The hex string.</param>
        /// <returns>The reversed hex string.</returns>
        public static string ReverseHex(this string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return string.Empty;
            
            var bytes = hex.HexToBytes();
            Array.Reverse(bytes);
            return bytes.ToHexString();
        }
    }
}