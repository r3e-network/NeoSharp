using System;
using System.IO;

namespace NeoSharp.Serialization
{
    /// <summary>
    /// Extension methods for System.IO.BinaryWriter.
    /// </summary>
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes a variable-length integer.
        /// </summary>
        public static void WriteVarInt(this System.IO.BinaryWriter writer, long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value < 0xFD)
            {
                writer.Write((byte)value);
            }
            else if (value <= 0xFFFF)
            {
                writer.Write((byte)0xFD);
                writer.Write((ushort)value);
            }
            else if (value <= 0xFFFFFFFF)
            {
                writer.Write((byte)0xFE);
                writer.Write((uint)value);
            }
            else
            {
                writer.Write((byte)0xFF);
                writer.Write(value);
            }
        }

        /// <summary>
        /// Writes a variable-length byte array.
        /// </summary>
        public static void WriteVarBytes(this System.IO.BinaryWriter writer, byte[] value)
        {
            writer.WriteVarInt(value.Length);
            writer.Write(value);
        }

        /// <summary>
        /// Gets the encoded size of a variable-length integer.
        /// </summary>
        public static int GetVarSize(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3;
            else if (value <= 0xFFFFFFFF)
                return 5;
            else
                return 9;
        }

        /// <summary>
        /// Gets the encoded size of a variable-length byte array.
        /// </summary>
        public static int GetVarSize(byte[] value)
        {
            return GetVarSize(value.Length) + value.Length;
        }
    }
}