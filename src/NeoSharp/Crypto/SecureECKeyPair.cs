using System;
using System.Threading.Tasks;
using NeoSharp.Wallet.NEP6;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// A secure version of ECKeyPair that uses SecureBytes for private key storage.
    /// Provides enhanced security for cryptographic operations by ensuring proper
    /// memory management and secure cleanup of sensitive key material.
    /// </summary>
    public sealed class SecureECKeyPair : IDisposable
    {
        private readonly SecureBytes _securePrivateKey;
        private bool _disposed = false;

        /// <summary>
        /// The public key component of this EC key pair.
        /// </summary>
        public ECPoint PublicKey { get; }

        /// <summary>
        /// Gets whether this SecureECKeyPair has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed || _securePrivateKey.IsDisposed;

        /// <summary>
        /// Initializes a new SecureECKeyPair with the provided secure private key and public key.
        /// </summary>
        /// <param name="securePrivateKey">The secure private key storage</param>
        /// <param name="publicKey">The corresponding public key</param>
        /// <exception cref="ArgumentNullException">Thrown when securePrivateKey or publicKey is null</exception>
        private SecureECKeyPair(SecureBytes securePrivateKey, ECPoint publicKey)
        {
            _securePrivateKey = securePrivateKey ?? throw new ArgumentNullException(nameof(securePrivateKey));
            PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        }

        /// <summary>
        /// Creates a SecureECKeyPair from a private key byte array.
        /// </summary>
        /// <param name="privateKey">The private key bytes</param>
        /// <returns>A new SecureECKeyPair instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when privateKey is null</exception>
        /// <exception cref="ArgumentException">Thrown when privateKey has invalid length</exception>
        /// <exception cref="CryptoSecurityException">Thrown when key pair generation fails</exception>
        public static SecureECKeyPair Create(byte[] privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            if (privateKey.Length != 32)
                throw new ArgumentException("Private key must be exactly 32 bytes", nameof(privateKey));

            try
            {
                var securePrivateKey = new SecureBytes(privateKey);
                var publicKey = GeneratePublicKeyFromPrivateKey(privateKey);
                return new SecureECKeyPair(securePrivateKey, publicKey);
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to create secure EC key pair from private key", ex);
            }
        }

        /// <summary>
        /// Creates a SecureECKeyPair from a SecureBytes private key.
        /// </summary>
        /// <param name="securePrivateKey">The secure private key storage</param>
        /// <returns>A new SecureECKeyPair instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when securePrivateKey is null</exception>
        /// <exception cref="CryptoSecurityException">Thrown when key pair generation fails</exception>
        public static SecureECKeyPair Create(SecureBytes securePrivateKey)
        {
            if (securePrivateKey == null)
                throw new ArgumentNullException(nameof(securePrivateKey));

            try
            {
                ECPoint publicKey = securePrivateKey.WithSecureAccess(privateKeyBytes =>
                    GeneratePublicKeyFromPrivateKey(privateKeyBytes.ToArray()));

                return new SecureECKeyPair(securePrivateKey, publicKey);
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to create secure EC key pair from SecureBytes", ex);
            }
        }

        /// <summary>
        /// Generates a fresh SecureECKeyPair using cryptographically secure random number generation.
        /// </summary>
        /// <returns>A new SecureECKeyPair with randomly generated keys</returns>
        /// <exception cref="CryptoSecurityException">Thrown when key generation fails</exception>
        public static SecureECKeyPair Generate()
        {
            try
            {
                var privateKeyBytes = new byte[32];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(privateKeyBytes);
                }

                return Create(privateKeyBytes);
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to generate secure EC key pair", ex);
            }
        }

        /// <summary>
        /// Creates a SecureECKeyPair from a WIF (Wallet Import Format) string.
        /// </summary>
        /// <param name="wifString">The WIF encoded private key</param>
        /// <returns>A new SecureECKeyPair instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when wifString is null</exception>
        /// <exception cref="WIFException">Thrown when WIF format is invalid</exception>
        /// <exception cref="CryptoSecurityException">Thrown when key pair generation fails</exception>
        public static SecureECKeyPair FromWIF(string wifString)
        {
            if (string.IsNullOrEmpty(wifString))
                throw new ArgumentNullException(nameof(wifString));

            try
            {
                var (privateKey, _) = wifString.FromWIF();
                return Create(privateKey);
            }
            catch (WIFException)
            {
                throw; // Re-throw WIF exceptions as-is
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to create secure EC key pair from WIF", ex);
            }
        }

        /// <summary>
        /// Creates a SecureECKeyPair from an encrypted NEP-2 private key.
        /// </summary>
        /// <param name="nep2String">The NEP-2 encrypted private key</param>
        /// <param name="password">The passphrase for decryption</param>
        /// <returns>A new SecureECKeyPair instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when nep2String or password is null</exception>
        /// <exception cref="NEP2Exception">Thrown when NEP-2 decryption fails</exception>
        /// <exception cref="CryptoSecurityException">Thrown when key pair generation fails</exception>
        public static SecureECKeyPair FromNEP2(string nep2String, string password)
        {
            if (string.IsNullOrEmpty(nep2String))
                throw new ArgumentNullException(nameof(nep2String));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            try
            {
                var keyPair = NeoSharp.Wallet.NEP2.Decrypt(nep2String, password, NeoSharp.Wallet.NEP6.ScryptParams.Default);
                return Create(keyPair.PrivateKeyBytes);
            }
            catch (NEP2Exception)
            {
                throw; // Re-throw NEP2 exceptions as-is
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to create secure EC key pair from NEP-2", ex);
            }
        }

        /// <summary>
        /// Performs a cryptographic operation with temporary access to the private key.
        /// The private key is only accessible during the operation and is securely cleared afterward.
        /// </summary>
        /// <typeparam name="T">The return type of the operation</typeparam>
        /// <param name="operation">The operation to perform with the private key</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        /// <exception cref="ArgumentNullException">Thrown when operation is null</exception>
        public T WithPrivateKey<T>(Func<byte[], T> operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            ThrowIfDisposed();
            
            return _securePrivateKey.WithSecureAccess(privateKeySpan => 
                operation(privateKeySpan.ToArray()));
        }

        /// <summary>
        /// Signs a message hash using this key pair's private key.
        /// </summary>
        /// <param name="messageHash">The hash to sign</param>
        /// <returns>The ECDSA signature</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        /// <exception cref="ArgumentNullException">Thrown when messageHash is null</exception>
        /// <exception cref="CryptoSecurityException">Thrown when signing fails</exception>
        public ECDSASignature Sign(byte[] messageHash)
        {
            if (messageHash == null)
                throw new ArgumentNullException(nameof(messageHash));

            ThrowIfDisposed();

            try
            {
                return WithPrivateKey(privateKey => 
                {
                    // Create temporary ECPrivateKey for signing
                    using var tempKey = new ECPrivateKey(privateKey);
                    return ECDSASignature.FromBytes(tempKey.Sign(messageHash));
                });
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to sign message with secure key pair", ex);
            }
        }

        /// <summary>
        /// Exports the private key in WIF (Wallet Import Format).
        /// </summary>
        /// <param name="compressed">Whether to use compressed format (default: true)</param>
        /// <returns>The WIF encoded private key</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        /// <exception cref="CryptoSecurityException">Thrown when WIF export fails</exception>
        public string ExportWIF(bool compressed = true)
        {
            ThrowIfDisposed();

            try
            {
                return WithPrivateKey(privateKey => privateKey.ToWIF(compressed));
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to export secure key pair as WIF", ex);
            }
        }

        /// <summary>
        /// Exports the private key encrypted using NEP-2 format.
        /// </summary>
        /// <param name="password">The passphrase for encryption</param>
        /// <param name="scryptParams">Optional Scrypt parameters (uses default if not provided)</param>
        /// <returns>The NEP-2 encrypted private key</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        /// <exception cref="ArgumentNullException">Thrown when password is null</exception>
        /// <exception cref="NEP2Exception">Thrown when NEP-2 encryption fails</exception>
        public string ExportNEP2(string password, NeoSharp.Wallet.NEP6.ScryptParams? scryptParams = null)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            ThrowIfDisposed();

            try
            {
                return WithPrivateKey(privateKey => 
                    NeoSharp.Wallet.NEP2.Encrypt(privateKey, password, scryptParams ?? NeoSharp.Wallet.NEP6.ScryptParams.Default));
            }
            catch (NEP2Exception)
            {
                throw; // Re-throw NEP2 exceptions as-is
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to export secure key pair as NEP-2", ex);
            }
        }

        /// <summary>
        /// Generates a public key from a private key using secp256r1 curve.
        /// </summary>
        /// <param name="privateKey">The private key bytes</param>
        /// <returns>The corresponding public key</returns>
        /// <exception cref="CryptoSecurityException">Thrown when public key generation fails</exception>
        private static ECPoint GeneratePublicKeyFromPrivateKey(byte[] privateKey)
        {
            try
            {
                // Use BouncyCastle for proper secp256r1 public key generation
                var curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256r1");
                var domainParams = new Org.BouncyCastle.Crypto.Parameters.ECDomainParameters(
                    curve.Curve, curve.G, curve.N, curve.H);
                
                // Convert private key to BigInteger
                var privateKeyInt = new Org.BouncyCastle.Math.BigInteger(1, privateKey);
                
                // Calculate public key point: Q = d * G (where d is private key, G is generator)
                var publicKeyPoint = curve.G.Multiply(privateKeyInt).Normalize();
                
                // Get compressed encoding (33 bytes)
                var encodedPublicKey = publicKeyPoint.GetEncoded(true);
                
                return new ECPoint(encodedPublicKey);
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.KeyGenerationError("Failed to generate public key from private key", ex);
            }
        }

        /// <summary>
        /// Throws ObjectDisposedException if this instance has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SecureECKeyPair));
        }

        /// <summary>
        /// Releases all resources used by the SecureECKeyPair.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _securePrivateKey?.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Finalizer to ensure cleanup if Dispose() wasn't called.
        /// </summary>
        ~SecureECKeyPair()
        {
            Dispose();
        }
    }
}