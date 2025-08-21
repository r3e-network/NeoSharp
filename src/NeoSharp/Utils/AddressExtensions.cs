using System;
using System.Linq;
using System.Security.Cryptography;
using NeoSharp.Crypto;

namespace NeoSharp.Utils
{
    /// <summary>
    /// Extension methods for address conversions
    /// </summary>
    public static class AddressExtensions
    {
        private const byte AddressVersion = 0x35;
        private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        /// <summary>
        /// Converts script hash to Neo address
        /// </summary>
        /// <param name="scriptHash">The script hash bytes</param>
        /// <returns>The Neo address</returns>
        public static string ScriptHashToAddress(this byte[] scriptHash)
        {
            if (scriptHash == null || scriptHash.Length != 20)
                throw new ArgumentException("Script hash must be 20 bytes", nameof(scriptHash));

            // Create the payload: version + script hash
            var payload = new byte[21];
            payload[0] = AddressVersion;
            Array.Copy(scriptHash, 0, payload, 1, 20);

            // Calculate checksum (first 4 bytes of double SHA256)
            var checksum = payload.Sha256().Sha256().Take(4).ToArray();

            // Combine payload and checksum
            var addressBytes = new byte[25];
            Array.Copy(payload, 0, addressBytes, 0, 21);
            Array.Copy(checksum, 0, addressBytes, 21, 4);

            // Encode to Base58
            return Base58Encode(addressBytes);
        }

        /// <summary>
        /// Converts Neo address to script hash
        /// </summary>
        /// <param name="address">The Neo address</param>
        /// <returns>The script hash bytes</returns>
        public static byte[] AddressToScriptHash(this string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));

            try
            {
                // Decode from Base58
                var addressBytes = Base58Decode(address);
                
                if (addressBytes.Length != 25)
                    throw new ArgumentException("Invalid address length", nameof(address));

                // Extract payload and checksum
                var payload = addressBytes.Take(21).ToArray();
                var checksum = addressBytes.Skip(21).ToArray();

                // Verify checksum
                var expectedChecksum = payload.Sha256().Sha256().Take(4).ToArray();
                if (!checksum.SequenceEqual(expectedChecksum))
                    throw new ArgumentException("Invalid address checksum", nameof(address));

                // Verify version
                if (payload[0] != AddressVersion)
                    throw new ArgumentException("Invalid address version", nameof(address));

                // Extract script hash
                return payload.Skip(1).ToArray();
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new ArgumentException("Invalid address format", nameof(address), ex);
            }
        }

        /// <summary>
        /// Converts Neo address to script hash (alias for AddressToScriptHash)
        /// </summary>
        /// <param name="address">The Neo address</param>
        /// <returns>The script hash as Hash160</returns>
        public static Types.Hash160 ToScriptHash(this string address)
        {
            return new Types.Hash160(address.AddressToScriptHash());
        }

        private static string Base58Encode(byte[] data)
        {
            if (data.Length == 0)
                return string.Empty;

            // Count leading zeros
            int leadingZeros = 0;
            while (leadingZeros < data.Length && data[leadingZeros] == 0)
                leadingZeros++;

            // Convert to big integer and encode
            var result = "";
            var value = System.Numerics.BigInteger.Zero;
            
            // Build big integer from bytes
            for (int i = 0; i < data.Length; i++)
            {
                value = value * 256 + data[i];
            }

            // Convert to Base58
            while (value > 0)
            {
                var remainder = (int)(value % 58);
                result = Base58Alphabet[remainder] + result;
                value /= 58;
            }

            // Add leading 1s for leading zeros
            result = new string('1', leadingZeros) + result;

            return result;
        }

        private static byte[] Base58Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return Array.Empty<byte>();

            // Count leading 1s
            int leadingOnes = 0;
            while (leadingOnes < encoded.Length && encoded[leadingOnes] == '1')
                leadingOnes++;

            // Convert from Base58
            var value = System.Numerics.BigInteger.Zero;
            foreach (char c in encoded)
            {
                var index = Base58Alphabet.IndexOf(c);
                if (index < 0)
                    throw new ArgumentException($"Invalid Base58 character: {c}");
                
                value = value * 58 + index;
            }

            // Convert to bytes
            var bytes = value.ToByteArray();
            
            // Remove unnecessary leading zero if present (BigInteger adds it for positive numbers)
            if (bytes.Length > 1 && bytes[^1] == 0)
                bytes = bytes.Take(bytes.Length - 1).ToArray();

            // Reverse to get big-endian
            Array.Reverse(bytes);

            // Add leading zeros for leading 1s
            var result = new byte[leadingOnes + bytes.Length];
            Array.Copy(bytes, 0, result, leadingOnes, bytes.Length);

            return result;
        }
    }
}