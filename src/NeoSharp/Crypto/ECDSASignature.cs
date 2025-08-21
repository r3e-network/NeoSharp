using System;
using System.Numerics;
using NeoSharp.Core;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Represents an ECDSA signature with R and S components
    /// </summary>
    public sealed class ECDSASignature : IEquatable<ECDSASignature>
    {
        /// <summary>
        /// The R component of the signature
        /// </summary>
        public BigInteger R { get; }

        /// <summary>
        /// The S component of the signature  
        /// </summary>
        public BigInteger S { get; }

        /// <summary>
        /// Returns true if the S component is "low", meaning it is below the half curve order
        /// </summary>
        public bool IsCanonical => S.CompareTo(NeoSharp.Core.NeoConstants.Secp256r1HalfCurveOrder) <= 0;

        /// <summary>
        /// Initializes a new ECDSA signature with the specified R and S values
        /// </summary>
        /// <param name="r">The R component</param>
        /// <param name="s">The S component</param>
        public ECDSASignature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        /// <summary>
        /// Creates an ECDSA signature from a 64-byte signature array
        /// </summary>
        /// <param name="signature">The 64-byte signature (32 bytes R + 32 bytes S)</param>
        /// <returns>The ECDSA signature</returns>
        /// <exception cref="ArgumentException">Thrown when signature length is invalid</exception>
        public static ECDSASignature FromBytes(byte[] signature)
        {
            if (signature.Length != 64)
                throw new ArgumentException("Signature must be exactly 64 bytes", nameof(signature));

            var r = new BigInteger(signature[0..32], isUnsigned: true, isBigEndian: true);
            var s = new BigInteger(signature[32..64], isUnsigned: true, isBigEndian: true);
            
            return new ECDSASignature(r, s);
        }

        /// <summary>
        /// Converts this signature to a 64-byte array
        /// </summary>
        /// <returns>The signature as byte array (32 bytes R + 32 bytes S)</returns>
        public byte[] ToBytes()
        {
            var result = new byte[64];
            var rBytes = R.ToByteArray(isUnsigned: true, isBigEndian: true);
            var sBytes = S.ToByteArray(isUnsigned: true, isBigEndian: true);

            // Ensure exactly 32 bytes for each component
            if (rBytes.Length <= 32)
                Array.Copy(rBytes, 0, result, 32 - rBytes.Length, rBytes.Length);
            else
                Array.Copy(rBytes, rBytes.Length - 32, result, 0, 32);

            if (sBytes.Length <= 32)
                Array.Copy(sBytes, 0, result, 64 - sBytes.Length, sBytes.Length);
            else
                Array.Copy(sBytes, sBytes.Length - 32, result, 32, 32);

            return result;
        }

        /// <summary>
        /// Determines whether this signature equals another signature
        /// </summary>
        /// <param name="other">The other signature to compare</param>
        /// <returns>True if the signatures are equal</returns>
        public bool Equals(ECDSASignature? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return R.Equals(other.R) && S.Equals(other.S);
        }

        /// <summary>
        /// Determines whether this signature equals another object
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object? obj) => Equals(obj as ECDSASignature);

        /// <summary>
        /// Gets the hash code for this signature
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode() => HashCode.Combine(R, S);

        /// <summary>
        /// Returns a string representation of this signature
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() => $"ECDSASignature(R={R:X}, S={S:X})";

        public static bool operator ==(ECDSASignature? left, ECDSASignature? right) => 
            EqualityComparer<ECDSASignature>.Default.Equals(left, right);

        public static bool operator !=(ECDSASignature? left, ECDSASignature? right) => 
            !(left == right);
    }
}