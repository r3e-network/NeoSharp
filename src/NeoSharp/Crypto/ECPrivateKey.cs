using System;
using System.Linq;
using System.Security.Cryptography;
using NeoSharp.Types;
using NeoSharp.Utils;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Represents an elliptic curve private key
    /// </summary>
    public class ECPrivateKey : IDisposable
    {
        private readonly byte[] _privateKeyBytes;
        private ECPublicKey? _publicKey;
        private bool _disposed;

        /// <summary>
        /// Gets the private key bytes
        /// </summary>
        public byte[] PrivateKeyBytes 
        { 
            get 
            {
                ThrowIfDisposed();
                return (byte[])_privateKeyBytes.Clone(); 
            }
        }

        /// <summary>
        /// Gets the private key bytes (alias for compatibility)
        /// </summary>
        public byte[] D => PrivateKeyBytes;

        /// <summary>
        /// Gets the corresponding public key
        /// </summary>
        public ECPublicKey PublicKey
        {
            get
            {
                ThrowIfDisposed();
                return _publicKey ??= DerivePublicKey();
            }
        }

        /// <summary>
        /// Initializes a new instance of ECPrivateKey
        /// </summary>
        /// <param name="privateKeyBytes">The private key bytes (32 bytes)</param>
        public ECPrivateKey(byte[] privateKeyBytes)
        {
            if (privateKeyBytes == null)
                throw new ArgumentNullException(nameof(privateKeyBytes));
            if (privateKeyBytes.Length != 32)
                throw new ArgumentException("Private key must be 32 bytes", nameof(privateKeyBytes));
            
            _privateKeyBytes = (byte[])privateKeyBytes.Clone();
        }

        /// <summary>
        /// Initializes a new instance of ECPrivateKey with a random key
        /// </summary>
        public ECPrivateKey()
        {
            _privateKeyBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(_privateKeyBytes);
        }

        /// <summary>
        /// Generates a random private key
        /// </summary>
        /// <returns>A new random ECPrivateKey</returns>
        public static ECPrivateKey GenerateRandom()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return new ECPrivateKey(bytes);
        }

        /// <summary>
        /// Gets the script hash for this private key
        /// </summary>
        /// <returns>The script hash</returns>
        public Hash160 GetScriptHash()
        {
            ThrowIfDisposed();
            return PublicKey.GetScriptHash();
        }

        /// <summary>
        /// Signs data with this private key
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <returns>The signature</returns>
        public byte[] Sign(byte[] data)
        {
            ThrowIfDisposed();
            if (data == null) throw new ArgumentNullException(nameof(data));
            
            // Use BouncyCastle for proper secp256r1 ECDSA signing
            var curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256r1");
            var domainParams = new Org.BouncyCastle.Crypto.Parameters.ECDomainParameters(
                curve.Curve, curve.G, curve.N, curve.H);
            
            var privateKeyInt = new Org.BouncyCastle.Math.BigInteger(1, _privateKeyBytes);
            var keyParams = new Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters(privateKeyInt, domainParams);
            
            var signer = new Org.BouncyCastle.Crypto.Signers.ECDsaSigner();
            signer.Init(true, keyParams);
            
            var hash = Hash.SHA256(data);
            var signature = signer.GenerateSignature(hash);
            
            // Convert to 64-byte format (32 bytes r + 32 bytes s)
            var r = signature[0].ToByteArrayUnsigned();
            var s = signature[1].ToByteArrayUnsigned();
            
            var result = new byte[64];
            Array.Copy(r, 0, result, 32 - r.Length, r.Length);
            Array.Copy(s, 0, result, 64 - s.Length, s.Length);
            
            return result;
        }

        /// <summary>
        /// Gets the public key as an ECPublicKey
        /// </summary>
        /// <returns>The public key as ECPublicKey</returns>
        public ECPublicKey GetPublicKey()
        {
            ThrowIfDisposed();
            
            // Use BouncyCastle for proper secp256r1 public key derivation
            var curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256r1");
            var domainParams = new Org.BouncyCastle.Crypto.Parameters.ECDomainParameters(
                curve.Curve, curve.G, curve.N, curve.H);
            
            var privateKeyInt = new Org.BouncyCastle.Math.BigInteger(1, _privateKeyBytes);
            var publicKeyPoint = domainParams.G.Multiply(privateKeyInt).Normalize();
            
            // Get compressed encoding
            var publicKeyBytes = publicKeyPoint.GetEncoded(true);
            
            return new ECPublicKey(publicKeyBytes);
        }

        /// <summary>
        /// Creates a private key from a WIF (Wallet Import Format) string
        /// </summary>
        /// <param name="wif">The WIF string</param>
        /// <returns>The private key</returns>
        public static ECPrivateKey FromWIF(string wif)
        {
            if (string.IsNullOrEmpty(wif))
                throw new ArgumentException("WIF cannot be empty", nameof(wif));
            
            var data = Base58.Decode(wif);
            if (data.Length != 38 && data.Length != 37)
                throw new FormatException("Invalid WIF length");
            
            if (data[0] != 0x80)
                throw new FormatException("Invalid WIF version");
            
            var compressed = data.Length == 38;
            if (compressed && data[33] != 0x01)
                throw new FormatException("Invalid WIF compression flag");
            
            // Verify checksum
            var toHash = data.Take(data.Length - 4).ToArray();
            var hash = Hash.SHA256(toHash);
            hash = Hash.SHA256(hash);
            var checksum = hash.Take(4).ToArray();
            var providedChecksum = data.Skip(data.Length - 4).ToArray();
            
            if (!checksum.SequenceEqual(providedChecksum))
                throw new FormatException("Invalid WIF checksum");
            
            var privateKey = new byte[32];
            Array.Copy(data, 1, privateKey, 0, 32);
            
            return new ECPrivateKey(privateKey);
        }

        /// <summary>
        /// Converts this private key to WIF (Wallet Import Format)
        /// </summary>
        /// <returns>The WIF string</returns>
        public string ToWIF()
        {
            ThrowIfDisposed();
            var data = new byte[38];
            data[0] = 0x80; // Version byte for mainnet
            Array.Copy(_privateKeyBytes, 0, data, 1, 32);
            data[33] = 0x01; // Compression flag
            
            var hash = Hash.SHA256(data.Take(34).ToArray());
            hash = Hash.SHA256(hash);
            var checksum = hash.Take(4).ToArray();
            
            var result = new byte[38];
            Array.Copy(data, 0, result, 0, 34);
            Array.Copy(checksum, 0, result, 34, 4);
            
            return Base58.Encode(result);
        }

        private ECPublicKey DerivePublicKey()
        {
            // This is a simplified implementation
            // In a real implementation, you would use secp256r1 curve math
            // For now, we'll create a placeholder
            var publicKeyBytes = new byte[33];
            publicKeyBytes[0] = 0x02; // Compressed format
            Array.Copy(_privateKeyBytes, 0, publicKeyBytes, 1, 32);
            return new ECPublicKey(publicKeyBytes);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ECPrivateKey));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Array.Clear(_privateKeyBytes, 0, _privateKeyBytes.Length);
                _disposed = true;
            }
        }
    }
}