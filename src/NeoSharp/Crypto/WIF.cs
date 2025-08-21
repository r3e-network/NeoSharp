using System;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Wallet Import Format (WIF) utilities for encoding and decoding private keys.
    /// WIF is a standardized way to encode private keys for easy import/export between wallets.
    /// </summary>
    public static class WIF
    {
        /// <summary>
        /// Size of a private key in bytes (256 bits = 32 bytes).
        /// </summary>
        public const int PrivateKeySize = 32;

        /// <summary>
        /// WIF prefix byte for mainnet private keys.
        /// </summary>
        public const byte WifPrefix = 0x80;

        /// <summary>
        /// Suffix byte indicating compressed public key format.
        /// </summary>
        public const byte CompressedSuffix = 0x01;

        /// <summary>
        /// Expected length of a WIF string when decoded to bytes (38 bytes total).
        /// 1 (prefix) + 32 (private key) + 1 (compressed flag) + 4 (checksum) = 38 bytes
        /// </summary>
        public const int WifDecodedLength = 38;

        /// <summary>
        /// Encodes a private key to WIF format.
        /// </summary>
        /// <param name="privateKey">The 32-byte private key to encode</param>
        /// <param name="compressed">Whether the corresponding public key should be compressed (default: true)</param>
        /// <returns>WIF encoded private key string</returns>
        /// <exception cref="ArgumentNullException">Thrown when privateKey is null</exception>
        /// <exception cref="WIFException">Thrown when privateKey has invalid length</exception>
        public static string Encode(byte[] privateKey, bool compressed = true)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            if (privateKey.Length != PrivateKeySize)
                throw WIFException.InvalidFormat($"Private key must be exactly {PrivateKeySize} bytes, but was {privateKey.Length} bytes");

            try
            {
                // Build the extended key: prefix + private_key + compressed_flag
                var extendedKey = new byte[PrivateKeySize + 2]; // +1 for prefix, +1 for compressed flag
                extendedKey[0] = WifPrefix;
                privateKey.CopyTo(extendedKey, 1);
                
                if (compressed)
                {
                    extendedKey[PrivateKeySize + 1] = CompressedSuffix;
                }

                // Calculate checksum (first 4 bytes of double SHA-256)
                var hash = extendedKey.Hash256();
                var checksum = new byte[4];
                Array.Copy(hash, 0, checksum, 0, 4);

                // Create final WIF data: extended_key + checksum
                var wifData = new byte[extendedKey.Length + 4];
                extendedKey.CopyTo(wifData, 0);
                checksum.CopyTo(wifData, extendedKey.Length);

                return wifData.ToBase58();
            }
            catch (Exception ex) when (!(ex is WIFException))
            {
                throw WIFException.EncodingError("Failed to encode private key to WIF format", ex);
            }
        }

        /// <summary>
        /// Decodes a WIF string to extract the private key.
        /// </summary>
        /// <param name="wifString">The WIF encoded private key string</param>
        /// <returns>A tuple containing the private key bytes and compressed flag</returns>
        /// <exception cref="ArgumentNullException">Thrown when wifString is null</exception>
        /// <exception cref="WIFException">Thrown when WIF format is invalid</exception>
        public static (byte[] privateKey, bool compressed) Decode(string wifString)
        {
            if (string.IsNullOrEmpty(wifString))
                throw new ArgumentNullException(nameof(wifString));

            try
            {
                // Decode from Base58
                var decoded = wifString.FromBase58();
                if (decoded == null)
                    throw WIFException.InvalidFormat("Invalid Base58 encoding");

                // Validate length (37 bytes for uncompressed, 38 bytes for compressed)
                if (decoded.Length != 37 && decoded.Length != WifDecodedLength)
                    throw WIFException.InvalidFormat($"Invalid WIF length. Expected 37 or {WifDecodedLength} bytes, got {decoded.Length} bytes");

                // Validate prefix
                if (decoded[0] != WifPrefix)
                    throw WIFException.InvalidFormat($"Invalid WIF prefix. Expected 0x{WifPrefix:X2}, got 0x{decoded[0]:X2}");

                // Determine if compressed
                bool compressed = decoded.Length == WifDecodedLength;
                if (compressed && decoded[33] != CompressedSuffix)
                    throw WIFException.InvalidFormat($"Invalid compressed flag. Expected 0x{CompressedSuffix:X2}, got 0x{decoded[33]:X2}");

                // Extract components
                var dataLength = decoded.Length - 4; // Remove checksum length
                var data = new byte[dataLength];
                var providedChecksum = new byte[4];
                Array.Copy(decoded, 0, data, 0, dataLength);
                Array.Copy(decoded, dataLength, providedChecksum, 0, 4);

                // Validate checksum
                var calculatedChecksum = data.Hash256();
                for (int i = 0; i < 4; i++)
                {
                    if (providedChecksum[i] != calculatedChecksum[i])
                        throw WIFException.InvalidChecksum("WIF checksum validation failed");
                }

                // Extract private key (skip prefix, take 32 bytes)
                var privateKey = new byte[PrivateKeySize];
                Array.Copy(data, 1, privateKey, 0, PrivateKeySize);

                return (privateKey, compressed);
            }
            catch (WIFException)
            {
                throw; // Re-throw WIF exceptions as-is
            }
            catch (Exception ex)
            {
                throw WIFException.EncodingError("Failed to decode WIF string", ex);
            }
        }

        /// <summary>
        /// Validates if a string is a properly formatted WIF.
        /// </summary>
        /// <param name="wifString">The string to validate</param>
        /// <returns>True if valid WIF format; otherwise, false</returns>
        public static bool IsValid(string wifString)
        {
            try
            {
                Decode(wifString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the expected WIF string length for the given parameters.
        /// </summary>
        /// <param name="compressed">Whether the key represents a compressed public key</param>
        /// <returns>Expected WIF string length</returns>
        public static int GetExpectedLength(bool compressed)
        {
            // This is an approximation since Base58 length can vary slightly
            // due to leading zeros being encoded as '1' characters
            return compressed ? 52 : 51; // Typical lengths
        }
    }

    /// <summary>
    /// Extension methods for WIF encoding and decoding.
    /// </summary>
    public static class WIFExtensions
    {
        /// <summary>
        /// Converts a private key byte array to WIF format.
        /// </summary>
        /// <param name="privateKey">The private key bytes</param>
        /// <param name="compressed">Whether to use compressed public key format (default: true)</param>
        /// <returns>WIF encoded string</returns>
        /// <exception cref="WIFException">Thrown when private key format is invalid</exception>
        public static string ToWIF(this byte[] privateKey, bool compressed = true)
        {
            return WIF.Encode(privateKey, compressed);
        }

        /// <summary>
        /// Decodes a WIF string to extract the private key.
        /// </summary>
        /// <param name="wifString">The WIF encoded string</param>
        /// <returns>A tuple containing the private key bytes and compressed flag</returns>
        /// <exception cref="WIFException">Thrown when WIF format is invalid</exception>
        public static (byte[] privateKey, bool compressed) FromWIF(this string wifString)
        {
            return WIF.Decode(wifString);
        }

        /// <summary>
        /// Decodes a WIF string to extract only the private key bytes.
        /// </summary>
        /// <param name="wifString">The WIF encoded string</param>
        /// <returns>The private key bytes</returns>
        /// <exception cref="WIFException">Thrown when WIF format is invalid</exception>
        public static byte[] PrivateKeyFromWIF(this string wifString)
        {
            return WIF.Decode(wifString).privateKey;
        }

        /// <summary>
        /// Validates if a string is a properly formatted WIF.
        /// </summary>
        /// <param name="wifString">The string to validate</param>
        /// <returns>True if valid WIF format; otherwise, false</returns>
        public static bool IsValidWIF(this string wifString)
        {
            return WIF.IsValid(wifString);
        }
    }
}