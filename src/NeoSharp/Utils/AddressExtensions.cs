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
        private const byte AddressVersion = 0x35; // Neo N3 address version

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
                
                if (addressBytes.Length < 25)
                    throw new ArgumentException("Invalid address length", nameof(address));

                // Extract payload and checksum (take only first 25 bytes if more present)
                var relevantBytes = addressBytes.Take(25).ToArray();
                var payload = relevantBytes.Take(21).ToArray();
                var checksum = relevantBytes.Skip(21).Take(4).ToArray();

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
            return Base58.Encode(data);
        }

        private static byte[] Base58Decode(string encoded)
        {
            return Base58.Decode(encoded);
        }
    }
}