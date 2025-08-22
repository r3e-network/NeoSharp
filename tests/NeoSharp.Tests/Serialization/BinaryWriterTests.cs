using System;
using System.Numerics;
using FluentAssertions;
using NeoSharp.Serialization;
using NeoSharp.Utils;
using NeoSharp.Tests.Helpers;
using Xunit;

namespace NeoSharp.Tests.Serialization
{
    /// <summary>
    /// Tests for Binary Writer functionality.
    /// Converted from Swift NeoSwiftTests/unit/serialization/BinaryWriterTests.swift
    /// </summary>
    public class BinaryWriterTests : IDisposable
    {
        private readonly NeoSharp.Serialization.BinaryWriter _writer;

        public BinaryWriterTests()
        {
            _writer = new NeoSharp.Serialization.BinaryWriter();
        }

        [Fact]
        public void WriteUInt32_ShouldWriteCorrectBytes()
        {
            // Maximum UInt32 value
            _writer.WriteUInt32(uint.MaxValue);
            TestAndReset(new byte[] { 0xff, 0xff, 0xff, 0xff });

            // Zero
            _writer.WriteUInt32(0);
            TestAndReset(new byte[] { 0, 0, 0, 0 });

            // Arbitrary value
            _writer.WriteUInt32(12345);
            TestAndReset(new byte[] { 0x39, 0x30, 0, 0 });
        }

        [Fact]
        public void WriteInt64_ShouldWriteCorrectBytes()
        {
            // Maximum Int64 value
            _writer.WriteInt64(long.MaxValue);
            TestAndReset(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f });

            // Minimum Int64 value
            _writer.WriteInt64(long.MinValue);
            TestAndReset(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 });

            // Zero
            _writer.WriteInt64(0);
            TestAndReset(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });

            // Arbitrary value
            _writer.WriteInt64(1234567890);
            TestAndReset(new byte[] { 0xd2, 0x02, 0x96, 0x49, 0x00, 0x00, 0x00, 0x00 });
        }

        [Fact]
        public void WriteUInt16_ShouldWriteCorrectBytes()
        {
            // Maximum UInt16 value
            _writer.WriteUInt16(ushort.MaxValue);
            TestAndReset(new byte[] { 0xff, 0xff });

            // Zero
            _writer.WriteUInt16(0);
            TestAndReset(new byte[] { 0, 0 });

            // Arbitrary value
            _writer.WriteUInt16(12345);
            TestAndReset(new byte[] { 0x39, 0x30 });
        }

        [Fact]
        public void WriteVarInt_ShouldEncodeCorrectly()
        {
            // v == 0, encode with one byte
            _writer.WriteVarInt(0);
            TestAndReset(new byte[] { 0 });

            // v == 0xfd - 1, encode with one byte
            _writer.WriteVarInt(252);
            TestAndReset(new byte[] { 0xfc });

            // v == 0xfd, encode with uint16
            _writer.WriteVarInt(253);
            TestAndReset(new byte[] { 0xfd, 0xfd, 0x00 });

            // v == 0xfd + 1, encode with uint16
            _writer.WriteVarInt(254);
            TestAndReset(new byte[] { 0xfd, 0xfe, 0x00 });

            // v == 0xffff - 1, encode with uint16
            _writer.WriteVarInt(65_534);
            TestAndReset(new byte[] { 0xfd, 0xfe, 0xff });

            // v == 0xffff, encode with uint16
            _writer.WriteVarInt(65_535);
            TestAndReset(new byte[] { 0xfd, 0xff, 0xff });

            // v == 0xffff + 1, encode with uint32
            _writer.WriteVarInt(65_536);
            TestAndReset(new byte[] { 0xfe, 0x00, 0x00, 0x01, 0x00 });

            // v == 0xffffffff - 1, encode with uint32
            _writer.WriteVarInt(4_294_967_294);
            TestAndReset(new byte[] { 0xfe, 0xfe, 0xff, 0xff, 0xff });

            // v == 0xffffffff, encode with uint32
            _writer.WriteVarInt(4_294_967_295);
            TestAndReset(new byte[] { 0xfe, 0xff, 0xff, 0xff, 0xff });

            // v == 0xffffffff + 1, encode with uint64
            _writer.WriteVarInt(4_294_967_296);
            TestAndReset(new byte[] { 0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 });
        }

        [Fact]
        public void WriteVarBytes_ShouldEncodeCorrectly()
        {
            // Small byte array
            var data = TestConstants.HexToBytes("010203");
            _writer.WriteVarBytes(data);
            TestAndReset(TestConstants.HexToBytes("03010203"));

            // Large byte array
            var hex = "00102030102030102030102030102030102030102030102030102030102030102031020301020301020301020301020301020301020301020301020301020301020310203010203010203010203010203010203010203010203010203010203010203102030102030102030102030102030102030102030102030102030102030102030010203010203010203010203010203010203010203010203010203010203010203102030102030102030102030102030102030102030102030102030102030102031020301020301020301020301020301020301020301020301020301020301020310203010203010203010203010203010203010203010203010203010203010203";
            var largeData = TestConstants.HexToBytes(hex);
            _writer.WriteVarBytes(largeData);
            var expected = TestConstants.HexToBytes("fd" + "0601" + hex);
            TestAndReset(expected);
        }

        [Fact]
        public void WriteVarString_ShouldEncodeCorrectly()
        {
            // Small string
            _writer.WriteVarString("hello, world!");
            TestAndReset(TestConstants.HexToBytes("0d68656c6c6f2c20776f726c6421"));

            // Large string
            var largeString = string.Concat(Enumerable.Repeat("hello, world!", 41));
            _writer.WriteVarString(largeString);
            var expectedPrefix = TestConstants.HexToBytes("fd1502");
            var stringBytes = System.Text.Encoding.UTF8.GetBytes(largeString);
            var expected = expectedPrefix.Concat(stringBytes).ToArray();
            TestAndReset(expected);
        }

        [Fact]
        public void WriteByte_ShouldWriteSingleByte()
        {
            _writer.WriteByte(0x42);
            TestAndReset(new byte[] { 0x42 });

            _writer.WriteByte(0x00);
            TestAndReset(new byte[] { 0x00 });

            _writer.WriteByte(0xFF);
            TestAndReset(new byte[] { 0xFF });
        }

        [Fact]
        public void WriteBytes_ShouldWriteByteArray()
        {
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            _writer.WriteBytes(data);
            TestAndReset(data);
        }

        [Fact]
        public void WriteBoolean_ShouldWriteBooleaneanAsBytes()
        {
            _writer.WriteBoolean(true);
            TestAndReset(new byte[] { 0x01 });

            _writer.WriteBoolean(false);
            TestAndReset(new byte[] { 0x00 });
        }

        [Fact]
        public void WriteNullableBytes_ShouldHandleNullValues()
        {
            // Null array
            _writer.WriteVarBytes(null);
            TestAndReset(new byte[] { 0x00 });

            // Empty array
            _writer.WriteVarBytes(Array.Empty<byte>());
            TestAndReset(new byte[] { 0x00 });
        }

        [Fact]
        public void ToArray_ShouldReturnAllWrittenData()
        {
            _writer.WriteByte(0x01);
            _writer.WriteUInt16(0x0203);
            _writer.WriteUInt32(0x04050607);

            var result = _writer.ToArray();
            result.Should().BeEquivalentTo(new byte[] { 0x01, 0x03, 0x02, 0x07, 0x06, 0x05, 0x04 });
        }

        [Fact]
        public void Reset_ShouldClearBuffer()
        {
            _writer.WriteByte(0x42);
            _writer.ToArray().Should().NotBeEmpty();

            _writer.Position = 0;
            _writer.ToArray().Should().BeEmpty();
        }

        [Fact]
        public void Length_ShouldReturnCorrectLength()
        {
            _writer.Length.Should().Be(0);

            _writer.WriteByte(0x42);
            _writer.Length.Should().Be(1);

            _writer.WriteUInt32(12345);
            _writer.Length.Should().Be(5);
        }

        [Fact]
        public void Dispose_ShouldNotThrowException()
        {
            Action act = () => _writer.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void WriteVarBytes_WithBigIntegerData_ShouldEncodeCorrectly()
        {
            // Test writing BigInteger data as byte array
            // Test with actual .NET BigInteger serialization behavior
            
            // Zero - .NET serializes BigInteger.Zero as [0x00]
            var zeroBigInt = BigInteger.Zero.ToByteArray();
            _writer.WriteBytes(zeroBigInt);
            TestAndReset(zeroBigInt);

            // Positive number - .NET serializes BigInteger(255) as [0xFF, 0x00] to maintain sign
            var positiveBigInt = new BigInteger(255).ToByteArray();
            _writer.WriteBytes(positiveBigInt);
            TestAndReset(positiveBigInt);

            // Negative number - .NET serializes BigInteger(-1) as [0xFF]
            var negativeBigInt = new BigInteger(-1).ToByteArray();
            _writer.WriteBytes(negativeBigInt);
            TestAndReset(negativeBigInt);

            // Large positive number - test actual serialization
            var largeBigInt = BigInteger.Parse("123456789012345678901234567890");
            var largeBytes = largeBigInt.ToByteArray();
            _writer.WriteBytes(largeBytes);
            var result = _writer.ToArray();
            result.Should().BeEquivalentTo(largeBytes);
            _writer.Position = 0;
        }

        private void TestAndReset(byte[] expected)
        {
            var actual = _writer.ToArray();
            actual.Should().BeEquivalentTo(expected);
            // Reset the stream position to start (simulate reset)
            _writer.Position = 0;
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}