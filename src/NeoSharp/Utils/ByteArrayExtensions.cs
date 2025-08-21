using System;
using System.Linq;
using NeoSharp.Crypto;

namespace NeoSharp.Utils
{
    /// <summary>
    /// Extension methods for byte array operations.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Computes SHA256 hash of the byte array.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The SHA256 hash.</returns>
        public static byte[] SHA256(this byte[] data)
        {
            return Hash.SHA256(data);
        }

        /// <summary>
        /// Computes RIPEMD160 hash of the byte array.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The RIPEMD160 hash.</returns>
        public static byte[] RIPEMD160(this byte[] data)
        {
            return Hash.RIPEMD160(data);
        }

        /// <summary>
        /// Computes Hash160 (RIPEMD160(SHA256)) of the byte array.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The Hash160 hash.</returns>
        public static byte[] Hash160(this byte[] data)
        {
            return Hash.Hash160(data);
        }

        /// <summary>
        /// Computes Hash256 (double SHA256) of the byte array.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The Hash256 hash.</returns>
        public static byte[] Hash256(this byte[] data)
        {
            return Hash.Hash256(data);
        }

        /// <summary>
        /// Concatenates two byte arrays.
        /// </summary>
        /// <param name="first">The first array.</param>
        /// <param name="second">The second array.</param>
        /// <returns>The concatenated array.</returns>
        public static byte[] Concat(this byte[] first, byte[] second)
        {
            if (first == null) return second ?? Array.Empty<byte>();
            if (second == null) return first;
            
            var result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }

        /// <summary>
        /// Takes a specified number of bytes from the beginning of the array.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="count">The number of bytes to take.</param>
        /// <returns>The taken bytes.</returns>
        public static byte[] Take(this byte[] array, int count)
        {
            if (array == null || count <= 0) return Array.Empty<byte>();
            if (count >= array.Length) return (byte[])array.Clone();
            
            var result = new byte[count];
            Array.Copy(array, result, count);
            return result;
        }

        /// <summary>
        /// Skips a specified number of bytes from the beginning of the array.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="count">The number of bytes to skip.</param>
        /// <returns>The remaining bytes.</returns>
        public static byte[] Skip(this byte[] array, int count)
        {
            if (array == null || count >= array.Length) return Array.Empty<byte>();
            if (count <= 0) return (byte[])array.Clone();
            
            var result = new byte[array.Length - count];
            Array.Copy(array, count, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Reverses the byte array.
        /// </summary>
        /// <param name="array">The array to reverse.</param>
        /// <returns>The reversed array.</returns>
        public static byte[] Reverse(this byte[] array)
        {
            if (array == null || array.Length <= 1) return array;
            
            var result = (byte[])array.Clone();
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Converts the byte array to a hex string.
        /// </summary>
        /// <param name="array">The byte array.</param>
        /// <returns>The hex string.</returns>
        public static string ToHex(this byte[] array)
        {
            return array?.ToHexString() ?? string.Empty;
        }

        /// <summary>
        /// Checks if two byte arrays are equal.
        /// </summary>
        /// <param name="a">The first array.</param>
        /// <param name="b">The second array.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public static bool ArrayEquals(this byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            
            return a.SequenceEqual(b);
        }

        /// <summary>
        /// Gets the bit length of a byte array
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The bit length</returns>
        public static int GetBitLength(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return 0;

            // Find the most significant byte
            int lastNonZeroByte = bytes.Length - 1;
            while (lastNonZeroByte >= 0 && bytes[lastNonZeroByte] == 0)
                lastNonZeroByte--;

            if (lastNonZeroByte < 0)
                return 0;

            // Count bits in the most significant byte
            byte mostSignificantByte = bytes[lastNonZeroByte];
            int bitLength = (lastNonZeroByte + 1) * 8;

            // Subtract leading zero bits from the most significant byte
            if (mostSignificantByte != 0)
            {
                int leadingZeros = 0;
                for (int i = 7; i >= 0; i--)
                {
                    if ((mostSignificantByte & (1 << i)) != 0)
                        break;
                    leadingZeros++;
                }
                bitLength -= leadingZeros;
            }

            return bitLength;
        }
    }
}