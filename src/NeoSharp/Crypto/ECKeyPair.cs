using System;
using System.Numerics;
using NeoSharp.Types;
using NeoSharp.Utils;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Represents an elliptic curve key pair for Neo blockchain operations
    /// </summary>
    public class ECKeyPair : IDisposable
    {
        private ECPrivateKey _privateKey;
        private ECPublicKey? _publicKey;
        private bool _disposed;

        /// <summary>
        /// Gets the public key
        /// </summary>
        public ECPublicKey PublicKey
        {
            get
            {
                if (_publicKey == null)
                    _publicKey = _privateKey.GetPublicKey();
                return _publicKey;
            }
        }

        /// <summary>
        /// Gets the private key bytes
        /// </summary>
        public byte[] PrivateKeyBytes => _privateKey.PrivateKeyBytes;

        /// <summary>
        /// Gets the private key
        /// </summary>
        public ECPrivateKey PrivateKey => _privateKey;

        /// <summary>
        /// Initializes a new ECKeyPair from private key bytes
        /// </summary>
        /// <param name="privateKeyBytes">The private key bytes</param>
        public ECKeyPair(byte[] privateKeyBytes)
        {
            _privateKey = new ECPrivateKey(privateKeyBytes);
        }

        /// <summary>
        /// Initializes a new ECKeyPair from private key
        /// </summary>
        /// <param name="privateKey">The private key</param>
        public ECKeyPair(ECPrivateKey privateKey)
        {
            _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        }

        /// <summary>
        /// Gets the address for this key pair
        /// </summary>
        /// <returns>The Neo address</returns>
        public string GetAddress()
        {
            return GetScriptHash().ToAddress();
        }

        /// <summary>
        /// Gets the script hash for this key pair
        /// </summary>
        /// <returns>The script hash</returns>
        public Hash160 GetScriptHash()
        {
            return Hash160.FromPublicKey(PublicKey.GetEncoded(true));
        }

        /// <summary>
        /// Signs data with this key pair's private key
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <returns>The signature</returns>
        public byte[] Sign(byte[] data)
        {
            return _privateKey.Sign(data);
        }

        /// <summary>
        /// Exports this key pair as a WIF string
        /// </summary>
        /// <returns>The WIF string</returns>
        public string ToWIF()
        {
            return _privateKey.ToWIF();
        }

        #region Static Factory Methods

        /// <summary>
        /// Creates a new random EC key pair
        /// </summary>
        /// <returns>A new random key pair</returns>
        public static ECKeyPair CreateEcKeyPair()
        {
            var privateKey = new ECPrivateKey();
            return new ECKeyPair(privateKey);
        }

        /// <summary>
        /// Creates a key pair from a private key as BigInteger
        /// </summary>
        /// <param name="privateKey">The private key as BigInteger</param>
        /// <returns>The key pair</returns>
        public static ECKeyPair Create(BigInteger privateKey)
        {
            var keyBytes = privateKey.ToByteArray(isUnsigned: true, isBigEndian: true);
            
            // Ensure the key is 32 bytes
            if (keyBytes.Length < 32)
            {
                var padded = new byte[32];
                Array.Copy(keyBytes, 0, padded, 32 - keyBytes.Length, keyBytes.Length);
                keyBytes = padded;
            }
            else if (keyBytes.Length > 32)
            {
                var trimmed = new byte[32];
                Array.Copy(keyBytes, keyBytes.Length - 32, trimmed, 0, 32);
                keyBytes = trimmed;
            }
            
            return new ECKeyPair(keyBytes);
        }

        /// <summary>
        /// Creates a key pair from WIF (Wallet Import Format)
        /// </summary>
        /// <param name="wif">The WIF string</param>
        /// <returns>The key pair</returns>
        public static ECKeyPair FromWIF(string wif)
        {
            var privateKey = ECPrivateKey.FromWIF(wif);
            return new ECKeyPair(privateKey);
        }

        /// <summary>
        /// Creates a key pair from a hex string private key
        /// </summary>
        /// <param name="privateKeyHex">The private key as hex string</param>
        /// <returns>The key pair</returns>
        public static ECKeyPair FromHex(string privateKeyHex)
        {
            if (!privateKeyHex.IsValidHex())
                throw new ArgumentException("Invalid hex string", nameof(privateKeyHex));
            
            var keyBytes = privateKeyHex.FromHexString();
            return new ECKeyPair(keyBytes);
        }

        #endregion

        /// <summary>
        /// Disposes the key pair and clears sensitive data
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _privateKey?.Dispose();
                _privateKey = null!;
                _publicKey = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// Returns a string representation of this key pair
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"ECKeyPair(Address: {GetAddress()})"; 
        }
    }
}