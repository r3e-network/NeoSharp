using System;
using NeoSharp.Types;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Represents an elliptic curve public key
    /// </summary>
    public class ECPublicKey : IEquatable<ECPublicKey>
    {
        private readonly byte[] _encodedBytes;

        /// <summary>
        /// Gets the encoded bytes of the public key
        /// </summary>
        public byte[] EncodedBytes => (byte[])_encodedBytes.Clone();

        /// <summary>
        /// Initializes a new instance of ECPublicKey
        /// </summary>
        /// <param name="encodedBytes">The encoded public key bytes (33 compressed or 65 uncompressed)</param>
        public ECPublicKey(byte[] encodedBytes)
        {
            if (encodedBytes == null)
                throw new ArgumentNullException(nameof(encodedBytes));
            
            if (encodedBytes.Length == 33)
            {
                // Already compressed
                _encodedBytes = (byte[])encodedBytes.Clone();
            }
            else if (encodedBytes.Length == 65)
            {
                // Uncompressed - need to compress
                if (encodedBytes[0] != 0x04)
                    throw new ArgumentException("Invalid uncompressed public key format", nameof(encodedBytes));
                
                // Extract x coordinate and determine y parity
                var x = encodedBytes[1..33];
                var y = encodedBytes[33..65];
                
                // Determine if y is even or odd
                bool isYEven = (y[31] & 1) == 0;
                
                // Create compressed format: 0x02 (even y) or 0x03 (odd y) + x coordinate
                _encodedBytes = new byte[33];
                _encodedBytes[0] = isYEven ? (byte)0x02 : (byte)0x03;
                Array.Copy(x, 0, _encodedBytes, 1, 32);
            }
            else
            {
                throw new ArgumentException("Public key must be 33 bytes (compressed) or 65 bytes (uncompressed)", nameof(encodedBytes));
            }
        }

        /// <summary>
        /// Creates an ECPublicKey from an ECPoint
        /// </summary>
        /// <param name="point">The ECPoint</param>
        /// <returns>The ECPublicKey</returns>
        public static ECPublicKey FromECPoint(ECPoint point)
        {
            return new ECPublicKey(point.GetEncoded());
        }

        /// <summary>
        /// Converts this public key to an ECPoint
        /// </summary>
        /// <returns>The ECPoint representation</returns>
        public ECPoint ToECPoint()
        {
            return new ECPoint(_encodedBytes);
        }

        /// <summary>
        /// Gets the encoded bytes of this public key
        /// </summary>
        /// <param name="compressed">Whether to return compressed format (always true for Neo)</param>
        /// <returns>The encoded public key bytes</returns>
        public byte[] GetEncoded(bool compressed = true)
        {
            return EncodedBytes;
        }

        /// <summary>
        /// Gets the script hash for this public key
        /// </summary>
        /// <returns>The script hash</returns>
        public Hash160 GetScriptHash()
        {
            return Hash160.FromPublicKey(_encodedBytes);
        }

        public bool Equals(ECPublicKey? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _encodedBytes.AsSpan().SequenceEqual(other._encodedBytes);
        }

        public override bool Equals(object? obj) => Equals(obj as ECPublicKey);

        public override int GetHashCode()
        {
            // Use a consistent hash code based on the content
            int hash = 17;
            unchecked
            {
                foreach (byte b in _encodedBytes)
                {
                    hash = hash * 31 + b.GetHashCode();
                }
            }
            return hash;
        }

        /// <summary>
        /// Verifies a signature against a message hash
        /// </summary>
        /// <param name="messageHash">The message hash that was signed</param>
        /// <param name="signature">The signature to verify</param>
        /// <returns>True if the signature is valid</returns>
        public bool Verify(byte[] messageHash, byte[] signature)
        {
            if (messageHash == null) throw new ArgumentNullException(nameof(messageHash));
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            try
            {
                // Use BouncyCastle for signature verification
                var curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256r1");
                var domainParams = new Org.BouncyCastle.Crypto.Parameters.ECDomainParameters(
                    curve.Curve, curve.G, curve.N, curve.H);

                // Decode the public key point
                var point = curve.Curve.DecodePoint(_encodedBytes);
                var pubKeyParams = new Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(point, domainParams);

                // Parse signature (assuming 64-byte format: 32 bytes r + 32 bytes s)
                if (signature.Length != 64)
                    return false;

                var r = new Org.BouncyCastle.Math.BigInteger(1, signature[0..32]);
                var s = new Org.BouncyCastle.Math.BigInteger(1, signature[32..64]);

                // Verify signature
                var verifier = new Org.BouncyCastle.Crypto.Signers.ECDsaSigner();
                verifier.Init(false, pubKeyParams);
                return verifier.VerifySignature(messageHash, r, s);
            }
            catch
            {
                return false;
            }
        }

        public static bool operator ==(ECPublicKey? left, ECPublicKey? right) => 
            left?.Equals(right) ?? right is null;

        public static bool operator !=(ECPublicKey? left, ECPublicKey? right) => 
            !(left == right);
    }
}