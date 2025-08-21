using System;
using System.Numerics;
using System.Security.Cryptography;
using NeoSharp.Types;
using NeoSharp.Utils;
using NeoSharp.Core;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Provides cryptographic signing and signature verification functionality for Neo
    /// </summary>
    public static class Sign
    {
        private const int LowerRealV = 27;

        /// <summary>
        /// Signs the SHA256 hash of a hexadecimal message with the private key
        /// </summary>
        /// <param name="message">The message to sign in hexadecimal format</param>
        /// <param name="keyPair">The key pair containing the private key</param>
        /// <returns>The signature data</returns>
        public static SignatureData SignHexMessage(string message, ECKeyPair keyPair)
        {
            return SignMessage(message.HexToBytes(), keyPair);
        }

        /// <summary>
        /// Signs the SHA256 hash of a UTF-8 string message with the private key
        /// </summary>
        /// <param name="message">The message to sign</param>
        /// <param name="keyPair">The key pair containing the private key</param>
        /// <returns>The signature data</returns>
        public static SignatureData SignMessage(string message, ECKeyPair keyPair)
        {
            return SignMessage(message.ToByteArray(), keyPair);
        }

        /// <summary>
        /// Signs the SHA256 hash of a byte array message with the private key
        /// </summary>
        /// <param name="message">The message to sign</param>
        /// <param name="keyPair">The key pair containing the private key</param>
        /// <returns>The signature data</returns>
        /// <exception cref="NeoSharpException">Thrown when signature recovery fails</exception>
        public static SignatureData SignMessage(byte[] message, ECKeyPair keyPair)
        {
            var messageHash = message.Sha256();
            var privateKey = keyPair.PrivateKey;
            var signatureBytes = privateKey.Sign(messageHash);
            var signature = ECDSASignature.FromBytes(signatureBytes);
            var recId = -1;

            // Find recovery ID
            for (var i = 0; i <= 3; i++)
            {
                var recovered = RecoverFromSignature(i, signature, messageHash);
                if (recovered != null && recovered.Equals(keyPair.PublicKey))
                {
                    recId = i;
                    break;
                }
            }

            if (recId == -1)
            {
                throw new NeoSharpException("Could not construct a recoverable key. This should never happen.");
            }

            var rBytes = signature.R.ToByteArray(isUnsigned: true, isBigEndian: true);
            var sBytes = signature.S.ToByteArray(isUnsigned: true, isBigEndian: true);

            // Pad to 32 bytes
            var r = new byte[32];
            var s = new byte[32];
            
            if (rBytes.Length <= 32)
                Array.Copy(rBytes, 0, r, 32 - rBytes.Length, rBytes.Length);
            else
                Array.Copy(rBytes, rBytes.Length - 32, r, 0, 32);
                
            if (sBytes.Length <= 32)
                Array.Copy(sBytes, 0, s, 32 - sBytes.Length, sBytes.Length);
            else
                Array.Copy(sBytes, sBytes.Length - 32, s, 0, 32);

            return new SignatureData((byte)(recId + 27), r, s);
        }

        /// <summary>
        /// Recovers the public key from a signature and message hash
        /// </summary>
        /// <param name="recId">The recovery ID (0-3)</param>
        /// <param name="signature">The ECDSA signature</param>
        /// <param name="messageHash">The hash of the original message</param>
        /// <returns>The recovered public key, or null if recovery failed</returns>
        public static ECPublicKey? RecoverFromSignature(int recId, ECDSASignature signature, byte[] messageHash)
        {
            if (recId < 0)
                throw new ArgumentException("Recovery ID must be positive", nameof(recId));
            if (signature.R.Sign <= 0)
                throw new ArgumentException("R must be positive", nameof(signature));
            if (signature.S.Sign <= 0)
                throw new ArgumentException("S must be positive", nameof(signature));
            if (messageHash.Length == 0)
                throw new ArgumentException("Message hash cannot be empty", nameof(messageHash));

            var curve = NeoSharp.Core.NeoConstants.Secp256r1Domain;
            var n = curve.N;
            var i = new BigInteger(recId / 2);
            var rBC = new Org.BouncyCastle.Math.BigInteger(signature.R.ToString());
            var iBC = new Org.BouncyCastle.Math.BigInteger(i.ToString());
            var x = rBC.Add(iBC.Multiply(n));

            if (x.CompareTo(curve.Curve.FieldSize) >= 0)
                return null;

            try
            {
                var point = DecompressKey(x, (recId & 1) == 1);
                var pointAtInfinity = point.Multiply(n);
                
                // Check if point * n = infinity (point of order n)
                if (!pointAtInfinity.IsInfinity)
                    return null;

                // Remove this old code as we're handling it with BouncyCastle below

                // Convert to BouncyCastle BigIntegers for calculations
                // rBC is already declared above
                var sBC = new Org.BouncyCastle.Math.BigInteger(signature.S.ToString());
                var eBC = new Org.BouncyCastle.Math.BigInteger(messageHash);
                var minusEBC = eBC.Negate().Mod(n);
                
                var rInv = rBC.ModInverse(n);
                var srInv = rInv.Multiply(sBC).Mod(n);
                var erInv = rInv.Multiply(minusEBC).Mod(n);

                var result = curve.G.Multiply(erInv).Add(point.Multiply(srInv));
                return new ECPublicKey(result.GetEncoded(true));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Decompresses a public key from its x-coordinate and y-bit
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="yBit">The y-coordinate bit</param>
        /// <returns>The decompressed ECPoint</returns>
        private static Org.BouncyCastle.Math.EC.ECPoint DecompressKey(Org.BouncyCastle.Math.BigInteger x, bool yBit)
        {
            var curve = NeoSharp.Core.NeoConstants.Secp256r1Domain.Curve;
            
            // Create the point using BouncyCastle's curve.CreatePoint
            var xBytes = x.ToByteArrayUnsigned();
            var compressed = new byte[33];
            compressed[0] = yBit ? (byte)0x03 : (byte)0x02;
            
            if (xBytes.Length <= 32)
                Array.Copy(xBytes, 0, compressed, 33 - xBytes.Length, xBytes.Length);
            else
                Array.Copy(xBytes, xBytes.Length - 32, compressed, 1, 32);

            return curve.DecodePoint(compressed);
        }

        /// <summary>
        /// Recovers the public key from a message and signature data
        /// </summary>
        /// <param name="message">The original message</param>
        /// <param name="signatureData">The signature data</param>
        /// <returns>The recovered public key</returns>
        /// <exception cref="ArgumentException">Thrown when signature format is invalid</exception>
        public static ECPublicKey SignedMessageToKey(byte[] message, SignatureData signatureData)
        {
            if (signatureData.R.Length != 32 || signatureData.S.Length != 32)
            {
                throw new ArgumentException($"{(signatureData.R.Length != 32 ? "R" : "S")} must be 32 bytes");
            }

            var header = signatureData.V;
            if (header < 27 || header > 34)
            {
                throw new SignException($"Header {header} out of range [27,34]");
            }

            var r = new BigInteger(signatureData.R, isUnsigned: true, isBigEndian: true);
            var s = new BigInteger(signatureData.S, isUnsigned: true, isBigEndian: true);
            var signature = new ECDSASignature(r, s);
            var messageHash = message.Sha256();
            var recId = header - 27;

            var key = RecoverFromSignature(recId, signature, messageHash);
            if (key == null)
            {
                throw new SignException("Failed to recover public key from signature");
            }

            return key;
        }

        /// <summary>
        /// Derives a public key from a private key
        /// </summary>
        /// <param name="privateKey">The private key</param>
        /// <returns>The corresponding public key</returns>
        public static ECPublicKey PublicKeyFromPrivateKey(ECPrivateKey privateKey)
        {
            return new ECPublicKey(PublicPointFromPrivateKey(privateKey).GetEncoded(true));
        }

        /// <summary>
        /// Derives a public key point from a private key
        /// </summary>
        /// <param name="privateKey">The private key</param>
        /// <returns>The corresponding public key point</returns>
        public static Org.BouncyCastle.Math.EC.ECPoint PublicPointFromPrivateKey(ECPrivateKey privateKey)
        {
            var keyBytes = privateKey.PrivateKeyBytes;
            var keyBC = new Org.BouncyCastle.Math.BigInteger(1, keyBytes);
            var order = NeoSharp.Core.NeoConstants.Secp256r1Domain.N;
            
            if (keyBC.CompareTo(order) >= 0)
            {
                keyBC = keyBC.Mod(order);
            }
            
            return NeoSharp.Core.NeoConstants.Secp256r1Domain.G.Multiply(keyBC);
        }

        /// <summary>
        /// Recovers the signer's script hash from a message and signature
        /// </summary>
        /// <param name="message">The signed message</param>
        /// <param name="signatureData">The signature data</param>
        /// <returns>The signer's script hash</returns>
        public static Hash160 RecoverSigningScriptHash(byte[] message, SignatureData signatureData)
        {
            var realV = GetRealV(signatureData.V);
            var sig = new SignatureData(realV, signatureData.R, signatureData.S);
            var key = SignedMessageToKey(message, sig);
            return Hash160.FromPublicKey(key.GetEncoded(true));
        }

        /// <summary>
        /// Gets the real V value from the signature V component
        /// </summary>
        /// <param name="v">The V component</param>
        /// <returns>The real V value</returns>
        private static byte GetRealV(byte v)
        {
            if (v == LowerRealV || v == LowerRealV + 1)
            {
                return v;
            }
            
            var realV = LowerRealV;
            var increment = v % 2 == 0 ? 1 : 0;
            return (byte)(realV + increment);
        }

        /// <summary>
        /// Verifies that a signature is appropriate for the given message and public key
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="signatureData">The signature data</param>
        /// <param name="publicKey">The public key</param>
        /// <returns>True if verification succeeds</returns>
        public static bool VerifySignature(byte[] message, SignatureData signatureData, ECPublicKey publicKey)
        {
            var r = new BigInteger(signatureData.R, isUnsigned: true, isBigEndian: true);
            var s = new BigInteger(signatureData.S, isUnsigned: true, isBigEndian: true);
            var signature = new ECDSASignature(r, s);
            
            return publicKey.Verify(message, signature.ToBytes());
        }

        /// <summary>
        /// Computes the modular inverse of a with respect to modulus m
        /// </summary>
        /// <param name="a">The value to invert</param>
        /// <param name="m">The modulus</param>
        /// <returns>The modular inverse</returns>
        private static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            if (a < 0) a = (a % m + m) % m;
            
            var g = GreatestCommonDivisor(a, m, out var x, out _);
            if (g != 1)
                throw new ArgumentException("Modular inverse does not exist");
                
            return (x % m + m) % m;
        }

        /// <summary>
        /// Extended Euclidean algorithm
        /// </summary>
        private static BigInteger GreatestCommonDivisor(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            if (a == 0)
            {
                x = 0;
                y = 1;
                return b;
            }
            
            var gcd = GreatestCommonDivisor(b % a, a, out var x1, out var y1);
            x = y1 - (b / a) * x1;
            y = x1;
            return gcd;
        }
    }
}