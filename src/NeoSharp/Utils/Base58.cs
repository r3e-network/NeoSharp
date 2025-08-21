using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace NeoSharp.Utils
{
    /// <summary>
    /// Base58 encoding/decoding implementation.
    /// </summary>
    public static class Base58
    {
        private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private static readonly BigInteger Base = new BigInteger(58);

        /// <summary>
        /// Encodes a byte array to Base58 string.
        /// </summary>
        /// <param name="data">The data to encode.</param>
        /// <returns>The Base58 encoded string.</returns>
        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (data.Length == 0)
                return string.Empty;
            
            // Count leading zeros
            int leadingZeros = 0;
            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                leadingZeros++;
            }
            
            // Convert to big integer (add 0x00 to ensure positive)
            var bytes = new byte[data.Length + 1];
            Array.Copy(data, 0, bytes, 1, data.Length);
            Array.Reverse(bytes, 1, data.Length);
            var value = new BigInteger(bytes);
            
            // Convert to base58
            var result = new StringBuilder();
            while (value > 0)
            {
                var remainder = value % Base;
                value /= Base;
                result.Insert(0, Alphabet[(int)remainder]);
            }
            
            // Add leading '1's for each leading zero byte
            for (int i = 0; i < leadingZeros; i++)
            {
                result.Insert(0, '1');
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Decodes a Base58 string to byte array.
        /// </summary>
        /// <param name="encoded">The Base58 encoded string.</param>
        /// <returns>The decoded byte array.</returns>
        public static byte[] Decode(string encoded)
        {
            if (encoded == null)
                throw new ArgumentNullException(nameof(encoded));
            
            if (encoded.Length == 0)
                return Array.Empty<byte>();
            
            // Count leading '1's
            int leadingOnes = 0;
            for (int i = 0; i < encoded.Length && encoded[i] == '1'; i++)
            {
                leadingOnes++;
            }
            
            // Convert from base58
            BigInteger value = 0;
            BigInteger power = 1;
            
            for (int i = encoded.Length - 1; i >= 0; i--)
            {
                int digit = Alphabet.IndexOf(encoded[i]);
                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character: {encoded[i]}");
                
                if (i < encoded.Length - 1)
                {
                    value += digit * power;
                    power *= Base;
                }
                else
                {
                    value = digit;
                }
            }
            
            // Convert to bytes
            var bytes = value.ToByteArray();
            
            // Remove sign byte if present
            if (bytes.Length > 1 && bytes[bytes.Length - 1] == 0)
            {
                Array.Resize(ref bytes, bytes.Length - 1);
            }
            
            // Reverse to big-endian
            Array.Reverse(bytes);
            
            // Add leading zeros
            if (leadingOnes > 0)
            {
                var result = new byte[leadingOnes + bytes.Length];
                Array.Copy(bytes, 0, result, leadingOnes, bytes.Length);
                return result;
            }
            
            return bytes;
        }

        /// <summary>
        /// Checks if a string is valid Base58.
        /// </summary>
        /// <param name="encoded">The string to check.</param>
        /// <returns>True if valid Base58, false otherwise.</returns>
        public static bool IsValid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return true;
            
            return encoded.All(c => Alphabet.Contains(c));
        }
    }
}