using System;
using System.Collections.Generic;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Represents signature data containing V, R, and S components for ECDSA signatures
    /// </summary>
    public sealed class SignatureData : IEquatable<SignatureData>
    {
        /// <summary>
        /// The recovery ID component
        /// </summary>
        public byte V { get; }

        /// <summary>
        /// The R component as byte array
        /// </summary>
        public byte[] R { get; }

        /// <summary>
        /// The S component as byte array  
        /// </summary>
        public byte[] S { get; }

        /// <summary>
        /// Gets the concatenated R and S components (64 bytes total)
        /// </summary>
        public byte[] Concatenated
        {
            get
            {
                var result = new byte[64];
                Array.Copy(R, 0, result, 0, 32);
                Array.Copy(S, 0, result, 32, 32);
                return result;
            }
        }

        /// <summary>
        /// Initializes signature data with V, R, and S components
        /// </summary>
        /// <param name="v">The recovery ID</param>
        /// <param name="r">The R component (must be 32 bytes)</param>
        /// <param name="s">The S component (must be 32 bytes)</param>
        /// <exception cref="ArgumentException">Thrown when R or S is not 32 bytes</exception>
        public SignatureData(byte v, byte[] r, byte[] s)
        {
            if (r.Length != 32)
                throw new ArgumentException("R component must be exactly 32 bytes", nameof(r));
            if (s.Length != 32)
                throw new ArgumentException("S component must be exactly 32 bytes", nameof(s));

            V = v;
            R = new byte[32];
            S = new byte[32];
            Array.Copy(r, R, 32);
            Array.Copy(s, S, 32);
        }

        /// <summary>
        /// Initializes signature data from a 64-byte signature with V = 0
        /// </summary>
        /// <param name="signature">The 64-byte signature (32 bytes R + 32 bytes S)</param>
        /// <exception cref="ArgumentException">Thrown when signature is not 64 bytes</exception>
        public SignatureData(byte[] signature) : this(0, signature)
        {
        }

        /// <summary>
        /// Initializes signature data from a 64-byte signature with specified V
        /// </summary>
        /// <param name="v">The recovery ID</param>
        /// <param name="signature">The 64-byte signature (32 bytes R + 32 bytes S)</param>
        /// <exception cref="ArgumentException">Thrown when signature is not 64 bytes</exception>
        public SignatureData(byte v, byte[] signature)
        {
            if (signature.Length != 64)
                throw new ArgumentException("Signature must be exactly 64 bytes", nameof(signature));

            V = v;
            R = new byte[32];
            S = new byte[32];
            Array.Copy(signature, 0, R, 0, 32);
            Array.Copy(signature, 32, S, 0, 32);
        }

        /// <summary>
        /// Creates signature data from a byte array with V = 0
        /// </summary>
        /// <param name="signature">The signature byte array</param>
        /// <returns>The signature data</returns>
        public static SignatureData FromByteArray(byte[] signature) => 
            FromByteArray(0, signature);

        /// <summary>
        /// Creates signature data from a byte array with specified V
        /// </summary>
        /// <param name="v">The recovery ID</param>
        /// <param name="signature">The signature byte array</param>
        /// <returns>The signature data</returns>
        public static SignatureData FromByteArray(byte v, byte[] signature) => 
            new(v, signature);

        /// <summary>
        /// Determines whether this signature data equals another
        /// </summary>
        /// <param name="other">The other signature data to compare</param>
        /// <returns>True if the signature data are equal</returns>
        public bool Equals(SignatureData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return V == other.V && R.AsSpan().SequenceEqual(other.R) && S.AsSpan().SequenceEqual(other.S);
        }

        /// <summary>
        /// Determines whether this signature data equals another object
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object? obj) => Equals(obj as SignatureData);

        /// <summary>
        /// Gets the hash code for this signature data
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(V);
            hash.AddBytes(R);
            hash.AddBytes(S);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Returns a string representation of this signature data
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() => 
            $"SignatureData(V={V}, R={Convert.ToHexString(R)}, S={Convert.ToHexString(S)})";

        public static bool operator ==(SignatureData? left, SignatureData? right) => 
            EqualityComparer<SignatureData>.Default.Equals(left, right);

        public static bool operator !=(SignatureData? left, SignatureData? right) => 
            !(left == right);
    }
}