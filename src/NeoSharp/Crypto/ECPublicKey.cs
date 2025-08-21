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
        /// <param name="encodedBytes">The encoded public key bytes</param>
        public ECPublicKey(byte[] encodedBytes)
        {
            if (encodedBytes == null)
                throw new ArgumentNullException(nameof(encodedBytes));
            if (encodedBytes.Length != 33)
                throw new ArgumentException("Public key must be 33 bytes (compressed)", nameof(encodedBytes));
            
            _encodedBytes = (byte[])encodedBytes.Clone();
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

        public override int GetHashCode() => _encodedBytes.GetHashCode();

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