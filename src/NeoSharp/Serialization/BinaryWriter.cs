using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using NeoSharp.Crypto;
using NeoSharp.Script;
using NeoSharp.Types;
using NeoSharp.Utils;

namespace NeoSharp.Serialization
{
    /// <summary>
    /// Binary writer for Neo serialization format with little-endian byte order
    /// </summary>
    public sealed class BinaryWriter : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly System.IO.BinaryWriter _writer;
        private bool _disposed;

        /// <summary>
        /// Gets the current length of the written data
        /// </summary>
        public long Length => _stream.Length;

        /// <summary>
        /// Gets the current position in the stream
        /// </summary>
        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        /// <summary>
        /// Initializes a new BinaryWriter
        /// </summary>
        public BinaryWriter()
        {
            _stream = new MemoryStream();
            _writer = new System.IO.BinaryWriter(_stream, Encoding.UTF8, leaveOpen: true);
        }

        /// <summary>
        /// Initializes a new BinaryWriter with initial capacity
        /// </summary>
        /// <param name="capacity">The initial capacity</param>
        public BinaryWriter(int capacity)
        {
            _stream = new MemoryStream(capacity);
            _writer = new System.IO.BinaryWriter(_stream, Encoding.UTF8, leaveOpen: true);
        }

        /// <summary>
        /// Initializes a new BinaryWriter with an existing stream
        /// </summary>
        /// <param name="stream">The memory stream to write to</param>
        public BinaryWriter(MemoryStream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _writer = new System.IO.BinaryWriter(_stream, Encoding.UTF8, leaveOpen: true);
        }

        /// <summary>
        /// Writes a boolean value
        /// </summary>
        /// <param name="value">The boolean value</param>
        public void WriteBoolean(bool value)
        {
            _writer.Write(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Writes a single byte
        /// </summary>
        /// <param name="value">The byte value</param>
        public void WriteByte(byte value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Writes a byte array
        /// </summary>
        /// <param name="value">The byte array</param>
        public void WriteBytes(byte[] value)
        {
            if (value?.Length > 0)
                _writer.Write(value);
        }

        /// <summary>
        /// Writes a byte array (alias for WriteBytes)
        /// </summary>
        /// <param name="value">The byte array</param>
        public void Write(byte[] value)
        {
            WriteBytes(value);
        }

        /// <summary>
        /// Writes a 16-bit signed integer
        /// </summary>
        /// <param name="value">The integer value</param>
        public void WriteInt16(short value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Writes a 16-bit unsigned integer
        /// </summary>
        /// <param name="value">The integer value</param>
        public void WriteUInt16(ushort value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Writes a 32-bit signed integer
        /// </summary>
        /// <param name="value">The integer value</param>
        public void WriteInt32(int value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer
        /// </summary>
        /// <param name="value">The integer value</param>
        public void WriteUInt32(uint value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Writes a 64-bit signed integer
        /// </summary>
        /// <param name="value">The integer value</param>
        public void WriteInt64(long value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Writes a 64-bit unsigned integer
        /// </summary>
        /// <param name="value">The integer value</param>
        public void WriteUInt64(ulong value)
        {
            _writer.Write(value);
        }

        /// <summary>
        /// Writes an EC point in compressed format
        /// </summary>
        /// <param name="point">The EC point</param>
        public void WriteECPoint(ECPoint point)
        {
            var encoded = point.GetEncoded(compressed: true);
            WriteBytes(encoded);
        }

        /// <summary>
        /// Writes a serializable object
        /// </summary>
        /// <param name="serializable">The serializable object</param>
        public void WriteSerializable(INeoSerializable serializable)
        {
            serializable.Serialize(this);
        }

        /// <summary>
        /// Writes a collection of serializable objects
        /// </summary>
        /// <param name="collection">The collection of serializable objects</param>
        public void WriteSerializableCollection<T>(IReadOnlyCollection<T> collection) 
            where T : INeoSerializable
        {
            WriteVarInt(collection.Count);
            foreach (var item in collection)
            {
                item.Serialize(this);
            }
        }

        /// <summary>
        /// Writes a variable-length byte array
        /// </summary>
        /// <param name="value">The byte array</param>
        public void WriteVarBytes(byte[] value)
        {
            WriteVarInt(value?.Length ?? 0);
            if (value?.Length > 0)
                WriteBytes(value);
        }

        /// <summary>
        /// Writes a variable-length string
        /// </summary>
        /// <param name="value">The UTF-8 string</param>
        public void WriteVarString(string value)
        {
            var bytes = value != null ? Encoding.UTF8.GetBytes(value) : Array.Empty<byte>();
            WriteVarBytes(bytes);
        }

        /// <summary>
        /// Writes a variable-length integer
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when value is negative</exception>
        public void WriteVarInt(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative");
                
            if (value < 0xFD)
            {
                WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                WriteByte(0xFD);
                WriteUInt16((ushort)value);
            }
            else if (value <= uint.MaxValue)
            {
                WriteByte(0xFE);
                WriteUInt32((uint)value);
            }
            else
            {
                WriteByte(0xFF);
                WriteUInt64((ulong)value);
            }
        }

        /// <summary>
        /// Writes a BigInteger as a variable-length signed integer
        /// </summary>
        /// <param name="value">The BigInteger value</param>
        public void WriteVarBigInteger(BigInteger value)
        {
            var bytes = value.ToByteArray(isUnsigned: false, isBigEndian: false);
            WriteVarBytes(bytes);
        }

        /// <summary>
        /// Writes PUSHDATA operation for the specified data
        /// </summary>
        /// <param name="data">The data to write</param>
        public void WritePushData(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                WriteByte((byte)OpCode.Push0);
                return;
            }

            if (data.Length <= byte.MaxValue)
            {
                WriteByte((byte)OpCode.PushData1);
                WriteByte((byte)data.Length);
            }
            else if (data.Length <= ushort.MaxValue)
            {
                WriteByte((byte)OpCode.PushData2);
                WriteUInt16((ushort)data.Length);
            }
            else
            {
                WriteByte((byte)OpCode.PushData4);
                WriteUInt32((uint)data.Length);
            }
            
            WriteBytes(data);
        }

        /// <summary>
        /// Writes a PUSH string operation
        /// </summary>
        /// <param name="value">The string value</param>
        public void WritePushString(string value)
        {
            var bytes = value != null ? Encoding.UTF8.GetBytes(value) : Array.Empty<byte>();
            WritePushData(bytes);
        }

        /// <summary>
        /// Writes a PUSH integer operation
        /// </summary>
        /// <param name="value">The integer value</param>
        public void WritePushInt(int value)
        {
            WritePushBigInteger(new BigInteger(value));
        }

        /// <summary>
        /// Writes a PUSH big integer operation
        /// </summary>
        /// <param name="value">The BigInteger value</param>
        public void WritePushBigInteger(BigInteger value)
        {
            // Handle small integers with dedicated opcodes
            if (value >= -1 && value <= 16)
            {
                var opcode = (OpCode)((int)OpCode.Push0 + (int)value);
                WriteByte((byte)opcode);
                return;
            }

            // Convert to bytes and determine size
            var bytes = value.ToByteArray(isUnsigned: false, isBigEndian: false);
            
            OpCode pushOpCode = bytes.Length switch
            {
                1 => OpCode.PushInt8,
                2 => OpCode.PushInt16,
                <= 4 => OpCode.PushInt32,
                <= 8 => OpCode.PushInt64,
                <= 16 => OpCode.PushInt128,
                <= 32 => OpCode.PushInt256,
                _ => throw new ArgumentException($"BigInteger too large: {bytes.Length} bytes", nameof(value))
            };

            WriteByte((byte)pushOpCode);
            
            // Pad to the required size
            var paddedBytes = new byte[pushOpCode switch
            {
                OpCode.PushInt8 => 1,
                OpCode.PushInt16 => 2,
                OpCode.PushInt32 => 4,
                OpCode.PushInt64 => 8,
                OpCode.PushInt128 => 16,
                OpCode.PushInt256 => 32,
                _ => throw new InvalidOperationException("Invalid push opcode")
            }];

            Array.Copy(bytes, paddedBytes, Math.Min(bytes.Length, paddedBytes.Length));
            WriteBytes(paddedBytes);
        }

        /// <summary>
        /// Gets the written data as a byte array
        /// </summary>
        /// <returns>The byte array</returns>
        public byte[] ToArray()
        {
            _writer.Flush();
            return _stream.ToArray();
        }

        /// <summary>
        /// Flushes the underlying stream
        /// </summary>
        public void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Seeks to the specified position
        /// </summary>
        /// <param name="offset">The offset</param>
        /// <param name="origin">The seek origin</param>
        /// <returns>The new position</returns>
        public long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        /// <summary>
        /// Releases all resources used by the BinaryWriter
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _writer?.Dispose();
                _stream?.Dispose();
                _disposed = true;
            }
        }
    }
}