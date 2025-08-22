using System;
using System.Collections.Generic;
using System.Numerics;
using FluentAssertions;
using NeoSharp.Script;
using NeoSharp.Serialization;
using NeoSharp.Utils;
using NeoSharp.Tests.Helpers;
using Xunit;

namespace NeoSharp.Tests.Serialization
{
    /// <summary>
    /// Tests for Binary Reader functionality.
    /// Converted from Swift NeoSwiftTests/unit/serialization/BinaryReaderTests.swift
    /// </summary>
    public class BinaryReaderTests
    {
        [Fact]
        public void ReadPushDataBytes_ShouldReadCorrectlyBasedOnPrefix()
        {
            var prefixCountMap = new Dictionary<string, int>
            {
                { "0c01", 1 },
                { "0cff", 255 },
                { "0d0001", 256 },
                { "0d0010", 4096 },
                { "0e00000100", 65536 }
            };

            foreach (var (prefix, count) in prefixCountMap)
            {
                var bytes = new byte[count];
                Array.Fill(bytes, (byte)0x01);
                var input = TestConstants.HexToBytes(prefix).Concat(bytes).ToArray();
                ReadPushDataBytesAndAssert(input, bytes);
            }
        }

        [Fact]
        public void ReadPushData_WithInvalidOpCode_ShouldThrowException()
        {
            var data = TestConstants.HexToBytes("4b")
                .Concat(new byte[] { 0x01 })
                .Concat(TestConstants.HexToBytes("0000"))
                .ToArray();

            Action act = () =>
            {
                using var reader = new NeoSharp.Serialization.BinaryReader(data);
                reader.ReadPushData();
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*PUSHDATA*");
        }

        [Fact]
        public void ReadPushDataString_ShouldReadStringsCorrectly()
        {
            // Empty string
            ReadPushDataStringAndAssert(TestConstants.HexToBytes("0c00"), "");

            // Single character
            ReadPushDataStringAndAssert(TestConstants.HexToBytes("0c0161"), "a");

            // Large string (10000 null bytes)
            var bytes = new byte[10000];
            var input = TestConstants.HexToBytes("0e10270000").Concat(bytes).ToArray();
            var expectedString = System.Text.Encoding.UTF8.GetString(bytes);
            ReadPushDataStringAndAssert(input, expectedString);
        }

        [Fact]
        public void ReadPushDataBigInteger_ShouldReadNumbersCorrectly()
        {
            // Zero (PUSH0)
            ReadPushDataIntegerAndAssert(new byte[] { (byte)OpCode.Push0 }, BigInteger.Zero);

            // One (PUSH1)
            ReadPushDataIntegerAndAssert(new byte[] { (byte)OpCode.Push1 }, BigInteger.One);

            // Negative one (PUSHM1)
            ReadPushDataIntegerAndAssert(new byte[] { (byte)OpCode.PushM1 }, new BigInteger(-1));

            // Sixteen (PUSH16)
            ReadPushDataIntegerAndAssert(new byte[] { (byte)OpCode.Push16 }, new BigInteger(16));
        }

        [Fact]
        public void ReadUInt32_ShouldReadCorrectValues()
        {
            ReadUInt32AndAssert(new byte[] { 0xff, 0xff, 0xff, 0xff }, 4_294_967_295);
            ReadUInt32AndAssert(new byte[] { 0x01, 0x00, 0x00, 0x00 }, 1);
            ReadUInt32AndAssert(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0);
            ReadUInt32AndAssert(new byte[] { 0x8c, 0xae, 0x00, 0x00, 0xff }, 44_684);
        }

        [Fact]
        public void ReadInt64_ShouldReadCorrectValues()
        {
            ReadInt64AndAssert(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, long.MinValue);
            ReadInt64AndAssert(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f }, long.MaxValue);
            ReadInt64AndAssert(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0);
            ReadInt64AndAssert(new byte[] { 0x11, 0x33, 0x22, 0x8c, 0xae, 0x00, 0x00, 0x00, 0xff }, 749_675_361_041);
        }

        [Fact]
        public void ReadByte_ShouldReadSingleByte()
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(new byte[] { 0x42, 0xFF, 0x00 });
            
            reader.ReadByte().Should().Be(0x42);
            reader.ReadByte().Should().Be(0xFF);
            reader.ReadByte().Should().Be(0x00);
        }

        [Fact]
        public void ReadBytes_ShouldReadSpecifiedNumberOfBytes()
        {
            var input = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            
            var result = reader.ReadBytes(3);
            result.Should().BeEquivalentTo(new byte[] { 0x01, 0x02, 0x03 });
            
            var remaining = reader.ReadBytes(2);
            remaining.Should().BeEquivalentTo(new byte[] { 0x04, 0x05 });
        }

        [Fact]
        public void ReadUInt16_ShouldReadCorrectValues()
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(new byte[] { 0x39, 0x30, 0xff, 0xff, 0x00, 0x00 });
            
            reader.ReadUInt16().Should().Be(12345);
            reader.ReadUInt16().Should().Be(65535);
            reader.ReadUInt16().Should().Be(0);
        }

        [Fact]
        public void ReadVarInt_ShouldReadVariableIntegers()
        {
            // Single byte value
            ReadVarIntAndAssert(new byte[] { 0xfc }, 252);
            
            // Two byte value
            ReadVarIntAndAssert(new byte[] { 0xfd, 0xfd, 0x00 }, 253);
            
            // Four byte value
            ReadVarIntAndAssert(new byte[] { 0xfe, 0x00, 0x00, 0x01, 0x00 }, 65536);
            
            // Eight byte value
            ReadVarIntAndAssert(new byte[] { 0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 }, 4_294_967_296);
        }

        [Fact]
        public void ReadVarBytes_ShouldReadVariableBytes()
        {
            var input = TestConstants.HexToBytes("03010203");
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            
            var result = reader.ReadVarBytes();
            result.Should().BeEquivalentTo(TestConstants.HexToBytes("010203"));
        }

        [Fact]
        public void ReadVarString_ShouldReadVariableString()
        {
            var input = TestConstants.HexToBytes("0d68656c6c6f2c20776f726c6421");
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            
            var result = reader.ReadVarString();
            result.Should().Be("hello, world!");
        }

        [Fact]
        public void ReadBoolean_ShouldReadBooleaneanValues()
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(new byte[] { 0x01, 0x00, 0xFF });
            
            reader.ReadBoolean().Should().BeTrue();
            reader.ReadBoolean().Should().BeFalse();
            reader.ReadBoolean().Should().BeTrue(); // Non-zero is true
        }

        [Fact]
        public void Position_ShouldTrackCurrentPosition()
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(new byte[] { 0x01, 0x02, 0x03, 0x04 });
            
            reader.Position.Should().Be(0);
            reader.ReadByte();
            reader.Position.Should().Be(1);
            reader.ReadUInt16();
            reader.Position.Should().Be(3);
        }

        [Fact]
        public void Available_ShouldReturnRemainingBytes()
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(new byte[] { 0x01, 0x02, 0x03, 0x04 });
            
            reader.Available.Should().Be(4);
            reader.ReadByte();
            reader.Available.Should().Be(3);
            reader.ReadBytes(2);
            reader.Available.Should().Be(1);
        }

        [Fact]
        public void ReadBeyondEnd_ShouldThrowException()
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(new byte[] { 0x01 });
            
            reader.ReadByte(); // This should work
            
            Action act = () => reader.ReadByte(); // This should fail
            act.Should().Throw<EndOfStreamException>();
        }

        [Fact]
        public void ReadBigInteger_ShouldReadCorrectly()
        {
            // Test various BigInteger values
            var testCases = new Dictionary<byte[], BigInteger>
            {
                { new byte[] { 0x00 }, BigInteger.Zero },
                { new byte[] { 0x01 }, BigInteger.One },
                { new byte[] { 0xFF }, new BigInteger(-1) },
                { new byte[] { 0xFF, 0x00 }, new BigInteger(255) }
            };

            foreach (var (input, expected) in testCases)
            {
                using var reader = new NeoSharp.Serialization.BinaryReader(input);
                var result = reader.ReadBigInteger();
                result.Should().Be(expected);
            }
        }

        private void ReadUInt32AndAssert(byte[] input, uint expected)
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            reader.ReadUInt32().Should().Be(expected);
        }

        private void ReadInt64AndAssert(byte[] input, long expected)
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            reader.ReadInt64().Should().Be(expected);
        }

        private void ReadVarIntAndAssert(byte[] input, ulong expected)
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            reader.ReadVarInt().Should().Be((int)expected);
        }

        private void ReadPushDataBytesAndAssert(byte[] input, byte[] expected)
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            var result = reader.ReadPushData();
            result.Should().BeEquivalentTo(expected);
        }

        private void ReadPushDataStringAndAssert(byte[] input, string expected)
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            var result = reader.ReadPushString();
            result.Should().Be(expected);
        }

        private void ReadPushDataIntegerAndAssert(byte[] input, BigInteger expected)
        {
            using var reader = new NeoSharp.Serialization.BinaryReader(input);
            var result = reader.ReadPushBigInt();
            result.Should().Be(expected);
        }
    }
}