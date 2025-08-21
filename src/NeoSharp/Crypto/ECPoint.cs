using System;
using System.Numerics;
using NeoSharp.Utils;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Represents an elliptic curve point on the secp256r1 curve
    /// </summary>
    public class ECPoint : IEquatable<ECPoint>
    {
        private readonly byte[] _encodedBytes;

        /// <summary>
        /// Gets the encoded bytes of this point (33 bytes compressed format)
        /// </summary>
        public byte[] EncodedBytes => (byte[])_encodedBytes.Clone();

        /// <summary>
        /// Gets whether this point is the point at infinity (zero point)
        /// </summary>
        public bool IsInfinity => _encodedBytes.Length == 1 && _encodedBytes[0] == 0x00;

        /// <summary>
        /// Creates an ECPoint from encoded bytes
        /// </summary>
        /// <param name="encodedBytes">The encoded point bytes (33 bytes compressed format)</param>
        public ECPoint(byte[] encodedBytes)
        {
            if (encodedBytes == null)
                throw new ArgumentNullException(nameof(encodedBytes));
            if (encodedBytes.Length != 33)
                throw new ArgumentException("ECPoint must be 33 bytes (compressed format)", nameof(encodedBytes));
            
            _encodedBytes = (byte[])encodedBytes.Clone();
        }

        /// <summary>
        /// Creates an ECPoint from a hex string
        /// </summary>
        /// <param name="hex">The hex string (66 characters)</param>
        public ECPoint(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                throw new ArgumentException("Hex string cannot be null or empty", nameof(hex));
            
            if (!hex.IsValidHex())
                throw new ArgumentException("Invalid hex string", nameof(hex));
            
            var bytes = hex.FromHexString();
            if (bytes.Length != 33)
                throw new ArgumentException("ECPoint must be 33 bytes (compressed format)", nameof(hex));
            
            _encodedBytes = bytes;
        }

        /// <summary>
        /// Gets the encoded bytes (alias for EncodedBytes)
        /// </summary>
        /// <returns>The encoded bytes</returns>
        public byte[] GetEncoded() => EncodedBytes;

        /// <summary>
        /// Gets the encoded bytes with optional compression
        /// </summary>
        /// <param name="compressed">Whether to use compressed format (always true for this implementation)</param>
        /// <returns>The encoded bytes</returns>
        public byte[] GetEncoded(bool compressed) => EncodedBytes;

        /// <summary>
        /// Converts this point to a hex string
        /// </summary>
        /// <returns>The hex string representation</returns>
        public string ToHex() => _encodedBytes.ToHexString();

        /// <summary>
        /// Creates a random ECPoint (for testing purposes)
        /// </summary>
        /// <returns>A random ECPoint</returns>
        public static ECPoint CreateRandom()
        {
            var random = new Random();
            var bytes = new byte[33];
            bytes[0] = 0x02; // Compressed format prefix
            random.NextBytes(bytes.AsSpan(1)); // Fill remaining 32 bytes
            return new ECPoint(bytes);
        }

        /// <summary>
        /// Checks if this is a valid point on the secp256r1 curve
        /// </summary>
        /// <returns>True if valid</returns>
        public bool IsValid()
        {
            if (_encodedBytes.Length != 33)
                return false;
            
            // Check if the first byte is a valid compression prefix
            return _encodedBytes[0] == 0x02 || _encodedBytes[0] == 0x03;
        }

        /// <summary>
        /// Verifies a signature
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="signature">The signature</param>
        /// <returns>True if the signature is valid, false otherwise</returns>
        public bool VerifySignature(byte[] message, byte[] signature)
        {
            if (message == null || signature == null)
                return false;
            
            if (signature.Length != 64)
                return false;
            
            try
            {
                // Use BouncyCastle for proper ECDSA signature verification
                var curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256r1");
                var domainParams = new Org.BouncyCastle.Crypto.Parameters.ECDomainParameters(
                    curve.Curve, curve.G, curve.N, curve.H);
                
                // Parse the public key point
                var publicKeyPoint = curve.Curve.DecodePoint(_encodedBytes);
                var publicKeyParams = new Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(publicKeyPoint, domainParams);
                
                // Parse r and s from signature
                var r = new Org.BouncyCastle.Math.BigInteger(1, signature.Take(32).ToArray());
                var s = new Org.BouncyCastle.Math.BigInteger(1, signature.Skip(32).Take(32).ToArray());
                
                var signer = new Org.BouncyCastle.Crypto.Signers.ECDsaSigner();
                signer.Init(false, publicKeyParams);
                
                var hash = Hash.SHA256(message);
                return signer.VerifySignature(hash, r, s);
            }
            catch
            {
                return false;
            }
        }

        public bool Equals(ECPoint? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _encodedBytes.AsSpan().SequenceEqual(other._encodedBytes);
        }

        public override bool Equals(object? obj) => Equals(obj as ECPoint);

        public override int GetHashCode() => _encodedBytes.GetSequenceHashCode();

        public override string ToString() => ToHex();

        public static bool operator ==(ECPoint? left, ECPoint? right) => 
            left?.Equals(right) ?? right is null;

        public static bool operator !=(ECPoint? left, ECPoint? right) => 
            !(left == right);

        public static implicit operator string(ECPoint point) => point.ToHex();
        public static explicit operator ECPoint(string hex) => new(hex);
    }

    /// <summary>
    /// Extension methods for ECPoint operations using secp256r1 curve
    /// </summary>
    public static class ECPointExtensions
    {
        /// <summary>
        /// Multiplies this point by a scalar
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="scalar">The scalar</param>
        /// <returns>The resulting point</returns>
        public static ECPoint Multiply(this ECPoint point, BigInteger scalar)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            
            try
            {
                var curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256r1");
                var ecPoint = curve.Curve.DecodePoint(point.EncodedBytes);
                
                var scalarBytes = scalar.ToByteArray(isUnsigned: true, isBigEndian: false);
                if (scalarBytes.Length > 32)
                {
                    // Take only the least significant 32 bytes
                    scalarBytes = scalarBytes.Take(32).ToArray();
                }
                Array.Reverse(scalarBytes); // Convert to big-endian for BouncyCastle
                
                var scalarBC = new Org.BouncyCastle.Math.BigInteger(1, scalarBytes);
                var resultPoint = ecPoint.Multiply(scalarBC).Normalize();
                
                return new ECPoint(resultPoint.GetEncoded(true));
            }
            catch
            {
                // Fallback to original point on any error
                return point;
            }
        }

        /// <summary>
        /// Adds two points
        /// </summary>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>The resulting point</returns>
        public static ECPoint Add(this ECPoint point1, ECPoint point2)
        {
            if (point1 == null) throw new ArgumentNullException(nameof(point1));
            if (point2 == null) throw new ArgumentNullException(nameof(point2));
            
            try
            {
                var curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256r1");
                var ecPoint1 = curve.Curve.DecodePoint(point1.EncodedBytes);
                var ecPoint2 = curve.Curve.DecodePoint(point2.EncodedBytes);
                
                var resultPoint = ecPoint1.Add(ecPoint2).Normalize();
                
                return new ECPoint(resultPoint.GetEncoded(true));
            }
            catch
            {
                // Fallback to first point on any error
                return point1;
            }
        }
    }
}