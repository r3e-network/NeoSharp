using System;
using System.IO;
using NeoSharp.Serialization;

namespace NeoSharp.Transaction
{
    /// <summary>
    /// Represents a witness in a transaction.
    /// </summary>
    public class Witness : INeoSerializable
    {
        /// <summary>
        /// Gets or sets the invocation script.
        /// </summary>
        public byte[] InvocationScript { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets or sets the verification script.
        /// </summary>
        public byte[] VerificationScript { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets the size of the witness.
        /// </summary>
        public int Size => BinaryWriterExtensions.GetVarSize(InvocationScript.Length) + InvocationScript.Length +
                          BinaryWriterExtensions.GetVarSize(VerificationScript.Length) + VerificationScript.Length;

        /// <summary>
        /// Initializes a new instance of the Witness class.
        /// </summary>
        public Witness()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Witness class.
        /// </summary>
        /// <param name="invocationScript">The invocation script.</param>
        /// <param name="verificationScript">The verification script.</param>
        public Witness(byte[] invocationScript, byte[] verificationScript)
        {
            InvocationScript = invocationScript ?? throw new ArgumentNullException(nameof(invocationScript));
            VerificationScript = verificationScript ?? throw new ArgumentNullException(nameof(verificationScript));
        }

        public void Serialize(Serialization.BinaryWriter writer)
        {
            writer.WriteVarBytes(InvocationScript);
            writer.WriteVarBytes(VerificationScript);
        }

        public void Deserialize(Serialization.BinaryReader reader)
        {
            InvocationScript = reader.ReadVarBytes();
            VerificationScript = reader.ReadVarBytes();
        }
    }
}