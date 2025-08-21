using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Scrypt;
using NeoSharp.Crypto;
using NeoSharp.Utils;
using NeoSharp.Wallet.NEP6;

namespace NeoSharp.Wallet
{
    /// <summary>
    /// NEP-2 private key encryption implementation.
    /// </summary>
    public static class NEP2
    {
        private const int NEP2_PRIVATE_KEY_LENGTH = 39;
        private const byte NEP2_PREFIX_1 = 0x01;
        private const byte NEP2_PREFIX_2 = 0x42;
        private const byte NEP2_FLAG = 0xe0;

        /// <summary>
        /// Encrypts the private key of the given EC key pair following the NEP-2 standard.
        /// </summary>
        /// <param name="keyPair">The ECKeyPair to be encrypted</param>
        /// <param name="password">The password used to encrypt</param>
        /// <param name="scryptParams">The scrypt parameters used for encryption</param>
        /// <returns>The NEP-2 encrypted private key string</returns>
        public static string Encrypt(ECKeyPair keyPair, string password, NEP6.ScryptParams? scryptParams = null)
        {
            scryptParams ??= NEP6.ScryptParams.Default;
            return Encrypt(keyPair.PrivateKeyBytes, password, scryptParams, keyPair.GetAddress());
        }

        /// <summary>
        /// Encrypts a private key using NEP-2 format.
        /// </summary>
        /// <param name="privateKey">The private key bytes.</param>
        /// <param name="password">The password.</param>
        /// <param name="scryptParams">The scrypt parameters.</param>
        /// <param name="address">The address for verification (optional, will be calculated if not provided)</param>
        /// <returns>The NEP-2 encrypted private key string.</returns>
        public static string Encrypt(byte[] privateKey, string password, NEP6.ScryptParams scryptParams, string? address = null)
        {
            if (privateKey == null || privateKey.Length != 32)
                throw new ArgumentException("Private key must be 32 bytes", nameof(privateKey));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));
            
            // Get address hash for salt
            if (address == null)
            {
                var keyPair = new ECKeyPair(privateKey);
                address = keyPair.GetAddress();
            }
            
            var addressHash = GetAddressHash(address);
            
            // Derive key using scrypt
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var derivedKey = NeoSharp.Crypto.ScryptEncoder.CryptoScrypt(passwordBytes, addressHash, scryptParams.N, scryptParams.R, scryptParams.P, 64);
            
            // Split derived key
            var derivedKeyHalf1 = derivedKey.Take(32).ToArray();
            var derivedKeyHalf2 = derivedKey.Skip(32).Take(32).ToArray();
            
            // XOR private key with first half of derived key
            var xorKey = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                xorKey[i] = (byte)(privateKey[i] ^ derivedKeyHalf1[i]);
            }
            
            // Encrypt using AES
            var encrypted = AesEncrypt(xorKey, derivedKeyHalf2);
            
            // Build final encrypted key
            var nep2Key = new byte[NEP2_PRIVATE_KEY_LENGTH];
            nep2Key[0] = NEP2_PREFIX_1;
            nep2Key[1] = NEP2_PREFIX_2;
            nep2Key[2] = NEP2_FLAG;
            Array.Copy(addressHash, 0, nep2Key, 3, 4);
            Array.Copy(encrypted, 0, nep2Key, 7, 32);
            
            // Base58 encode with checksum
            return Base58CheckEncode(nep2Key);
        }

        /// <summary>
        /// Decrypts the given encrypted private key in NEP-2 format with the given password.
        /// </summary>
        /// <param name="nep2Key">The NEP-2 encrypted private key</param>
        /// <param name="password">The passphrase used for decryption</param>
        /// <param name="scryptParams">The scrypt parameters used for encryption</param>
        /// <returns>An EC key pair constructed from the decrypted private key</returns>
        public static ECKeyPair Decrypt(string nep2Key, string password, NEP6.ScryptParams? scryptParams = null)
        {
            scryptParams ??= NEP6.ScryptParams.Default;
            var privateKeyBytes = DecryptToBytes(nep2Key, password, scryptParams);
            return new ECKeyPair(privateKeyBytes);
        }

        /// <summary>
        /// Decrypts a NEP-2 encrypted private key to raw bytes.
        /// </summary>
        /// <param name="nep2Key">The NEP-2 encrypted key string.</param>
        /// <param name="password">The password.</param>
        /// <param name="scryptParams">The scrypt parameters.</param>
        /// <returns>The decrypted private key bytes.</returns>
        public static byte[] DecryptToBytes(string nep2Key, string password, NEP6.ScryptParams scryptParams)
        {
            if (string.IsNullOrEmpty(nep2Key))
                throw new ArgumentException("NEP-2 key cannot be empty", nameof(nep2Key));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));
            
            // Base58 decode
            var decoded = Base58CheckDecode(nep2Key);
            
            if (decoded.Length != NEP2_PRIVATE_KEY_LENGTH)
                throw new FormatException("Invalid NEP-2 key length");
            
            if (decoded[0] != NEP2_PREFIX_1 || decoded[1] != NEP2_PREFIX_2 || decoded[2] != NEP2_FLAG)
                throw new FormatException("Invalid NEP-2 key prefix");
            
            // Extract components
            var addressHash = new byte[4];
            Array.Copy(decoded, 3, addressHash, 0, 4);
            
            var encrypted = new byte[32];
            Array.Copy(decoded, 7, encrypted, 0, 32);
            
            // Derive key using scrypt
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var derivedKey = NeoSharp.Crypto.ScryptEncoder.CryptoScrypt(passwordBytes, addressHash, scryptParams.N, scryptParams.R, scryptParams.P, 64);
            
            // Split derived key
            var derivedKeyHalf1 = derivedKey.Take(32).ToArray();
            var derivedKeyHalf2 = derivedKey.Skip(32).Take(32).ToArray();
            
            // Decrypt using AES
            var decrypted = AesDecrypt(encrypted, derivedKeyHalf2);
            
            // XOR with first half of derived key to get private key
            var privateKey = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                privateKey[i] = (byte)(decrypted[i] ^ derivedKeyHalf1[i]);
            }
            
            // Verify by checking address hash
            var keyPair = new ECKeyPair(privateKey);
            var checkHash = GetAddressHash(keyPair.GetAddress());
            
            if (!addressHash.SequenceEqual(checkHash))
                throw new InvalidOperationException("Invalid password or corrupted key");
            
            return privateKey;
        }

        /// <summary>
        /// Verifies if a string is a valid NEP-2 encrypted key.
        /// </summary>
        /// <param name="nep2Key">The key to verify.</param>
        /// <returns>True if valid NEP-2 format, false otherwise.</returns>
        public static bool IsNEP2(string nep2Key)
        {
            if (string.IsNullOrEmpty(nep2Key))
                return false;
            
            try
            {
                var decoded = Base58CheckDecode(nep2Key);
                return decoded.Length == NEP2_PRIVATE_KEY_LENGTH &&
                       decoded[0] == NEP2_PREFIX_1 &&
                       decoded[1] == NEP2_PREFIX_2 &&
                       decoded[2] == NEP2_FLAG;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypts the private key of the given EC key pair with custom scrypt parameters.
        /// </summary>
        /// <param name="keyPair">The ECKeyPair to be encrypted</param>
        /// <param name="password">The password used to encrypt</param>
        /// <param name="n">The N parameter for scrypt</param>
        /// <param name="r">The R parameter for scrypt</param>
        /// <param name="p">The P parameter for scrypt</param>
        /// <returns>The NEP-2 encrypted private key string</returns>
        public static string Encrypt(ECKeyPair keyPair, string password, int n, int r, int p)
        {
            return Encrypt(keyPair, password, new NEP6.ScryptParams { N = n, R = r, P = p });
        }

        /// <summary>
        /// Gets the address hash for a given address (first 4 bytes of double SHA256)
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns>The address hash (4 bytes)</returns>
        private static byte[] GetAddressHash(string address)
        {
            var addressBytes = Encoding.UTF8.GetBytes(address);
            return addressBytes.SHA256().SHA256().Take(4).ToArray();
        }

        /// <summary>
        /// XORs private key bytes with derived key half for specified range
        /// </summary>
        /// <param name="privateKey">The private key bytes</param>
        /// <param name="derivedHalf">The derived key half</param>
        /// <param name="start">Start index</param>
        /// <param name="length">Length to XOR</param>
        /// <returns>XORed bytes</returns>
        private static byte[] XorPrivateKeyAndDerivedHalf(byte[] privateKey, byte[] derivedHalf, int start, int length)
        {
            var result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(privateKey[start + i] ^ derivedHalf[start + i]);
            }
            return result;
        }

        /// <summary>
        /// Performs AES encryption/decryption using ECB mode
        /// </summary>
        /// <param name="data">The data to encrypt/decrypt</param>
        /// <param name="key">The AES key</param>
        /// <param name="decrypt">True for decryption, false for encryption</param>
        /// <returns>The result bytes</returns>
        private static byte[] PerformAesCipher(byte[] data, byte[] key, bool decrypt)
        {
            var cipher = new BufferedBlockCipher(new Org.BouncyCastle.Crypto.Modes.EcbBlockCipher(new AesEngine()));
            cipher.Init(!decrypt, new KeyParameter(key));
            
            var output = new byte[cipher.GetOutputSize(data.Length)];
            var length = cipher.ProcessBytes(data, 0, data.Length, output, 0);
            cipher.DoFinal(output, length);
            
            return output.Take(data.Length).ToArray();
        }

        private static byte[] AesEncrypt(byte[] data, byte[] key)
        {
            return PerformAesCipher(data, key, false);
        }

        private static byte[] AesDecrypt(byte[] data, byte[] key)
        {
            return PerformAesCipher(data, key, true);
        }

        private static string Base58CheckEncode(byte[] data)
        {
            var checksum = data.SHA256().SHA256().Take(4).ToArray();
            var dataWithChecksum = data.Concat(checksum).ToArray();
            return NeoSharp.Utils.Base58.Encode(dataWithChecksum);
        }

        private static byte[] Base58CheckDecode(string encoded)
        {
            var decoded = NeoSharp.Utils.Base58.Decode(encoded);
            if (decoded.Length < 4)
                throw new FormatException("Invalid base58 check string");
            
            var data = decoded.Take(decoded.Length - 4).ToArray();
            var checksum = decoded.Skip(decoded.Length - 4).ToArray();
            var expectedChecksum = data.SHA256().SHA256().Take(4).ToArray();
            
            if (!checksum.SequenceEqual(expectedChecksum))
                throw new FormatException("Invalid base58 check string");
            
            return data;
        }
    }
}