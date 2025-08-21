using System;
using System.Numerics;
using System.Text;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Base58 and Base58Check encoding/decoding utilities.
    /// Base58 is a binary-to-text encoding scheme used in cryptocurrencies
    /// to create human-readable addresses and keys.
    /// </summary>
    public static class Base58
    {
        /// <summary>
        /// Length of checksum appended to Base58Check encoded strings.
        /// </summary>
        private const int ChecksumLength = 4;

        /// <summary>
        /// Base58 alphabet excluding 0, O, I, and l to avoid visual ambiguity.
        /// </summary>
        private static readonly string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        /// <summary>
        /// Lookup table for fast Base58 character to index conversion.
        /// </summary>
        private static readonly int[] AlphabetMap;

        /// <summary>
        /// Base of the Base58 encoding (58).
        /// </summary>
        private static readonly BigInteger Base = new(58);

        /// <summary>
        /// Static constructor to initialize the alphabet mapping.
        /// </summary>
        static Base58()
        {
            AlphabetMap = new int[128];
            Array.Fill(AlphabetMap, -1); // Initialize with invalid values

            for (int i = 0; i < Alphabet.Length; i++)
            {
                AlphabetMap[Alphabet[i]] = i;
            }
        }

        /// <summary>
        /// Encode bytes to Base58 string.
        /// </summary>
        /// <param name="data">The bytes to encode</param>
        /// <returns>Base58 encoded string</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length == 0)
                return string.Empty;

            // Convert to BigInteger (treating as big-endian)
            var value = new BigInteger(data.AsSpan(), isUnsigned: true, isBigEndian: true);

            // Encode using Base58
            var result = new StringBuilder();
            while (value > 0)
            {
                value = BigInteger.DivRem(value, Base, out var remainder);
                result.Insert(0, Alphabet[(int)remainder]);
            }

            // Add leading zeros as '1' characters
            foreach (var b in data)
            {
                if (b == 0)
                    result.Insert(0, '1');
                else
                    break;
            }

            return result.ToString();
        }

        /// <summary>
        /// Decode Base58 string to bytes.
        /// </summary>
        /// <param name="encoded">The Base58 encoded string</param>
        /// <returns>Decoded bytes, or null if decoding failed</returns>
        public static byte[]? Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            try
            {
                // Count leading '1's
                var leadingOnes = 0;
                foreach (var c in encoded)
                {
                    if (c == '1')
                        leadingOnes++;
                    else
                        break;
                }

                // Convert from Base58 to BigInteger
                var value = BigInteger.Zero;
                var multiplier = BigInteger.One;

                for (int i = encoded.Length - 1; i >= leadingOnes; i--)
                {
                    var c = encoded[i];
                    
                    // Validate character and get its value
                    if (c >= 128 || AlphabetMap[c] == -1)
                        return null; // Invalid character

                    value += multiplier * AlphabetMap[c];
                    multiplier *= Base;
                }

                // Convert BigInteger back to bytes
                var bytes = value == 0 ? Array.Empty<byte>() : value.ToByteArray(isUnsigned: true, isBigEndian: true);
                
                // Add leading zeros for leading '1's
                if (leadingOnes > 0)
                {
                    var result = new byte[leadingOnes + bytes.Length];
                    bytes.CopyTo(result, leadingOnes);
                    return result;
                }

                return bytes;
            }
            catch
            {
                return null; // Decoding failed
            }
        }

        /// <summary>
        /// Encode bytes to Base58Check string with checksum validation.
        /// </summary>
        /// <param name="data">The bytes to encode</param>
        /// <returns>Base58Check encoded string</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        public static string EncodeCheck(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Calculate checksum
            var checksum = CalculateChecksum(data);

            // Combine data and checksum
            var dataWithChecksum = new byte[data.Length + ChecksumLength];
            data.CopyTo(dataWithChecksum, 0);
            checksum.CopyTo(dataWithChecksum, data.Length);

            return Encode(dataWithChecksum);
        }

        /// <summary>
        /// Decode Base58Check string and validate checksum.
        /// </summary>
        /// <param name="encoded">The Base58Check encoded string</param>
        /// <returns>Decoded bytes without checksum, or null if decoding or validation failed</returns>
        public static byte[]? DecodeCheck(string encoded)
        {
            var decoded = Decode(encoded);
            if (decoded == null || decoded.Length < ChecksumLength)
                return null;

            // Split data and checksum
            var dataLength = decoded.Length - ChecksumLength;
            var data = new byte[dataLength];
            var checksum = new byte[ChecksumLength];

            Array.Copy(decoded, 0, data, 0, dataLength);
            Array.Copy(decoded, dataLength, checksum, 0, ChecksumLength);

            // Validate checksum
            var calculatedChecksum = CalculateChecksum(data);
            if (!checksum.AsSpan().SequenceEqual(calculatedChecksum.AsSpan()))
                return null;

            return data;
        }

        /// <summary>
        /// Calculate checksum for Base58Check encoding.
        /// Uses double SHA-256 and takes the first 4 bytes.
        /// </summary>
        /// <param name="data">The data to calculate checksum for</param>
        /// <returns>4-byte checksum</returns>
        private static byte[] CalculateChecksum(byte[] data)
        {
            var hash = data.Hash256();
            var checksum = new byte[ChecksumLength];
            Array.Copy(hash, 0, checksum, 0, ChecksumLength);
            return checksum;
        }

        /// <summary>
        /// Validates if a string is a valid Base58 encoded string.
        /// </summary>
        /// <param name="input">The string to validate</param>
        /// <returns>True if valid Base58; otherwise, false</returns>
        public static bool IsValid(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true; // Empty string is valid

            foreach (var c in input)
            {
                if (c >= 128 || AlphabetMap[c] == -1)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a string is a valid Base58Check encoded string.
        /// </summary>
        /// <param name="input">The string to validate</param>
        /// <returns>True if valid Base58Check; otherwise, false</returns>
        public static bool IsValidCheck(string input)
        {
            return DecodeCheck(input) != null;
        }
    }

    /// <summary>
    /// Extension methods for Base58 encoding/decoding.
    /// </summary>
    public static class Base58Extensions
    {
        /// <summary>
        /// Encode byte array to Base58 string.
        /// </summary>
        /// <param name="data">The bytes to encode</param>
        /// <returns>Base58 encoded string</returns>
        public static string ToBase58(this byte[] data)
        {
            return Base58.Encode(data);
        }

        /// <summary>
        /// Encode byte array to Base58Check string.
        /// </summary>
        /// <param name="data">The bytes to encode</param>
        /// <returns>Base58Check encoded string</returns>
        public static string ToBase58Check(this byte[] data)
        {
            return Base58.EncodeCheck(data);
        }

        /// <summary>
        /// Decode Base58 string to byte array.
        /// </summary>
        /// <param name="encoded">The Base58 encoded string</param>
        /// <returns>Decoded bytes, or null if decoding failed</returns>
        public static byte[]? FromBase58(this string encoded)
        {
            return Base58.Decode(encoded);
        }

        /// <summary>
        /// Decode Base58Check string to byte array.
        /// </summary>
        /// <param name="encoded">The Base58Check encoded string</param>
        /// <returns>Decoded bytes without checksum, or null if decoding or validation failed</returns>
        public static byte[]? FromBase58Check(this string encoded)
        {
            return Base58.DecodeCheck(encoded);
        }
    }
}