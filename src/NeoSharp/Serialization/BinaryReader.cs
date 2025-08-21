using System;
using System.IO;
using System.Numerics;
using System.Text;
using NeoSharp.Crypto;
using NeoSharp.Script;
using NeoSharp.Utils;

namespace NeoSharp.Serialization
{
    /// <summary>
    /// Binary reader for Neo serialization format with little-endian byte order
    /// </summary>
    public sealed class BinaryReader : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly byte[] _buffer;
        private int _position;
        private int _marker = -1;
        private bool _disposed;

        /// <summary>
        /// Gets the current position in the stream
        /// </summary>
        public int Position => _position;

        /// <summary>
        /// Gets the number of bytes available to read
        /// </summary>
        public int Available => _buffer.Length - _position;

        /// <summary>
        /// Initializes a new BinaryReader with the specified byte array
        /// </summary>
        /// <param name="input">The input byte array</param>
        public BinaryReader(byte[] input)
        {
            _buffer = input ?? throw new ArgumentNullException(nameof(input));
            _stream = new MemoryStream(_buffer);
            _position = 0;
        }

        /// <summary>
        /// Initializes a new BinaryReader with a MemoryStream
        /// </summary>
        /// <param name="stream">The memory stream to read from</param>
        public BinaryReader(MemoryStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            
            _buffer = stream.ToArray();
            _stream = stream;
            _position = 0;
        }

        /// <summary>
        /// Marks the current position for later reset
        /// </summary>
        public void Mark()
        {
            _marker = _position;
        }

        /// <summary>
        /// Resets to the previously marked position
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no mark has been set</exception>
        public void Reset()
        {
            if (_marker == -1)
                throw new InvalidOperationException("No mark has been set");
                
            _position = _marker;
        }

        /// <summary>
        /// Reads a boolean value
        /// </summary>
        /// <returns>The boolean value</returns>
        /// <exception cref="EndOfStreamException">Thrown when there are insufficient bytes</exception>
        public bool ReadBoolean()
        {
            EnsureAvailable(1);
            return _buffer[_position++] == 1;
        }

        /// <summary>
        /// Reads a single byte
        /// </summary>
        /// <returns>The byte value</returns>
        /// <exception cref="EndOfStreamException">Thrown when there are insufficient bytes</exception>
        public byte ReadByte()
        {
            EnsureAvailable(1);
            return _buffer[_position++];
        }

        /// <summary>
        /// Reads a byte as an unsigned integer
        /// </summary>
        /// <returns>The unsigned integer value</returns>
        public int ReadUnsignedByte() => ReadByte();

        /// <summary>
        /// Reads the specified number of bytes
        /// </summary>
        /// <param name="length">The number of bytes to read</param>
        /// <returns>The byte array</returns>
        /// <exception cref="EndOfStreamException">Thrown when there are insufficient bytes</exception>
        public byte[] ReadBytes(int length)
        {
            if (length < 0)
                throw new ArgumentException("Length cannot be negative", nameof(length));
            if (length == 0)
                return Array.Empty<byte>();
                
            EnsureAvailable(length);
            var result = new byte[length];
            Array.Copy(_buffer, _position, result, 0, length);
            _position += length;
            return result;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer
        /// </summary>
        /// <returns>The unsigned integer value</returns>
        public ushort ReadUInt16()
        {
            var bytes = ReadBytes(2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a 16-bit signed integer
        /// </summary>
        /// <returns>The signed integer value</returns>
        public short ReadInt16()
        {
            var bytes = ReadBytes(2);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer
        /// </summary>
        /// <returns>The unsigned integer value</returns>
        public uint ReadUInt32()
        {
            var bytes = ReadBytes(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Reads a 32-bit signed integer
        /// </summary>
        /// <returns>The signed integer value</returns>
        public int ReadInt32()
        {
            var bytes = ReadBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reads a 64-bit signed integer
        /// </summary>
        /// <returns>The signed integer value</returns>
        public long ReadInt64()
        {
            var bytes = ReadBytes(8);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Reads an encoded EC point (compressed format)
        /// </summary>
        /// <returns>The encoded EC point bytes</returns>
        /// <exception cref="FormatException">Thrown when the point format is invalid</exception>
        public byte[] ReadEncodedECPoint()
        {
            var prefix = ReadByte();
            if (prefix != 0x02 && prefix != 0x03)
                throw new FormatException("Invalid EC point encoding");
                
            var point = new byte[33];
            point[0] = prefix;
            Array.Copy(ReadBytes(32), 0, point, 1, 32);
            return point;
        }

        /// <summary>
        /// Reads an EC point
        /// </summary>
        /// <returns>The EC point</returns>
        /// <exception cref="FormatException">Thrown when the point format is invalid</exception>
        public ECPoint ReadECPoint()
        {
            var prefix = ReadByte();
            byte[] encoded;
            
            switch (prefix)
            {
                case 0x00:
                    encoded = new byte[] { 0x00 };
                    break;
                case 0x02:
                case 0x03:
                    encoded = new byte[33];
                    encoded[0] = prefix;
                    Array.Copy(ReadBytes(32), 0, encoded, 1, 32);
                    break;
                case 0x04:
                    encoded = new byte[65];
                    encoded[0] = prefix;
                    Array.Copy(ReadBytes(64), 0, encoded, 1, 64);
                    break;
                default:
                    throw new FormatException($"Invalid EC point prefix: 0x{prefix:X2}");
            }
            
            return new ECPoint(encoded);
        }

        /// <summary>
        /// Reads a serializable object
        /// </summary>
        /// <typeparam name="T">The type of object to read</typeparam>
        /// <returns>The deserialized object</returns>
        public T ReadSerializable<T>() where T : INeoSerializable, new()
        {
            var obj = new T();
            obj.Deserialize(this);
            return obj;
        }

        /// <summary>
        /// Reads a list of serializable objects with variable-length encoding
        /// </summary>
        /// <typeparam name="T">The type of objects to read</typeparam>
        /// <returns>The list of objects</returns>
        public List<T> ReadSerializableListVarBytes<T>() where T : INeoSerializable, new()
        {
            var length = ReadVarInt(0x10000000);
            var list = new List<T>();
            var bytesRead = 0;
            var startPosition = _position;
            
            while (bytesRead < length)
            {
                var obj = ReadSerializable<T>();
                list.Add(obj);
                bytesRead = _position - startPosition;
            }
            
            return list;
        }

        /// <summary>
        /// Reads a list of serializable objects
        /// </summary>
        /// <typeparam name="T">The type of objects to read</typeparam>
        /// <returns>The list of objects</returns>
        public List<T> ReadSerializableList<T>() where T : INeoSerializable, new()
        {
            var count = ReadVarInt(0x10000000);
            var list = new List<T>();
            
            for (int i = 0; i < count; i++)
            {
                list.Add(ReadSerializable<T>());
            }
            
            return list;
        }

        /// <summary>
        /// Reads a variable-length byte array
        /// </summary>
        /// <returns>The byte array</returns>
        public byte[] ReadVarBytes() => ReadVarBytes(0x1000000);

        /// <summary>
        /// Reads a variable-length string
        /// </summary>
        /// <returns>The UTF-8 string</returns>
        public string ReadVarString()
        {
            var bytes = ReadVarBytes();
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Reads PUSHDATA operation data
        /// </summary>
        /// <returns>The data bytes</returns>
        /// <exception cref="FormatException">Thrown when not positioned at a PUSHDATA opcode</exception>
        public byte[] ReadPushData()
        {
            var opcode = (OpCode)ReadByte();
            var size = opcode switch
            {
                OpCode.PushData1 => ReadUnsignedByte(),
                OpCode.PushData2 => ReadInt16(),
                OpCode.PushData4 => ReadInt32(),
                _ => throw new FormatException("Current position does not contain a PUSHDATA opcode")
            };
            
            return ReadBytes(size);
        }

        /// <summary>
        /// Reads a variable-length byte array with maximum size limit
        /// </summary>
        /// <param name="max">The maximum allowed size</param>
        /// <returns>The byte array</returns>
        public byte[] ReadVarBytes(int max) => ReadBytes(ReadVarInt(max));

        /// <summary>
        /// Reads a variable-length integer
        /// </summary>
        /// <returns>The integer value</returns>
        public int ReadVarInt() => ReadVarInt(int.MaxValue);

        /// <summary>
        /// Reads a variable-length integer with maximum value limit
        /// </summary>
        /// <param name="max">The maximum allowed value</param>
        /// <returns>The integer value</returns>
        /// <exception cref="FormatException">Thrown when value exceeds maximum</exception>
        public int ReadVarInt(int max)
        {
            var first = ReadUnsignedByte();
            int value = first switch
            {
                0xFD => ReadInt16(),
                0xFE => ReadInt32(),
                0xFF => (int)ReadInt64(),
                _ => first
            };
            
            if (value > max)
                throw new FormatException($"Variable integer value {value} exceeds maximum {max}");
                
            return value;
        }

        /// <summary>
        /// Reads a PUSH string operation
        /// </summary>
        /// <returns>The UTF-8 string</returns>
        public string ReadPushString()
        {
            var bytes = ReadPushData();
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Reads a PUSH integer operation
        /// </summary>
        /// <returns>The integer value</returns>
        public int ReadPushInt()
        {
            var bigInt = ReadPushBigInt();
            if (bigInt > int.MaxValue || bigInt < int.MinValue)
                throw new OverflowException("PUSH integer value exceeds int range");
                
            return (int)bigInt;
        }

        /// <summary>
        /// Reads a PUSH big integer operation
        /// </summary>
        /// <returns>The BigInteger value</returns>
        /// <exception cref="FormatException">Thrown when opcode is not a PUSH integer</exception>
        public BigInteger ReadPushBigInt()
        {
            var opcode = (OpCode)ReadByte();
            
            // Handle small integers (PUSHM1 to PUSH16)
            if (opcode >= OpCode.PushM1 && opcode <= OpCode.Push16)
            {
                return (int)opcode - (int)OpCode.Push0;
            }
            
            // Handle larger integers
            var byteCount = opcode switch
            {
                OpCode.PushInt8 => 1,
                OpCode.PushInt16 => 2,
                OpCode.PushInt32 => 4,
                OpCode.PushInt64 => 8,
                OpCode.PushInt128 => 16,
                OpCode.PushInt256 => 32,
                _ => throw new FormatException($"Invalid PUSH integer opcode: {opcode}")
            };
            
            var bytes = ReadBytes(byteCount);
            return new BigInteger(bytes, isUnsigned: false, isBigEndian: false);
        }

        /// <summary>
        /// Ensures that the specified number of bytes are available
        /// </summary>
        /// <param name="count">The number of bytes required</param>
        /// <exception cref="EndOfStreamException">Thrown when insufficient bytes are available</exception>
        private void EnsureAvailable(int count)
        {
            if (Available < count)
                throw new EndOfStreamException($"Attempted to read {count} bytes, but only {Available} are available");
        }

        /// <summary>
        /// Releases all resources used by the BinaryReader
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _stream?.Dispose();
                _disposed = true;
            }
        }
    }
}