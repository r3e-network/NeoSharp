using System;
using System.IO;

namespace NeoSharp.Serialization
{
    /// <summary>
    /// Extension methods for System.IO.BinaryReader.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a variable-length integer.
        /// </summary>
        public static long ReadVarInt(this System.IO.BinaryReader reader, long max = long.MaxValue)
        {
            var first = reader.ReadByte();
            long value = first switch
            {
                0xFD => reader.ReadUInt16(),
                0xFE => reader.ReadUInt32(),
                0xFF => reader.ReadInt64(),
                _ => first
            };

            if (value > max)
                throw new FormatException($"Variable integer value {value} exceeds maximum {max}");

            return value;
        }

        /// <summary>
        /// Reads a variable-length byte array.
        /// </summary>
        public static byte[] ReadVarBytes(this System.IO.BinaryReader reader, int max = 0x1000000)
        {
            var length = (int)reader.ReadVarInt(max);
            return reader.ReadBytes(length);
        }
    }
}