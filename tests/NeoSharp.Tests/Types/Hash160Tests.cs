using System;
using FluentAssertions;
using NeoSharp.Tests.Helpers;
using NeoSharp.Types;
using Xunit;

namespace NeoSharp.Tests.Types
{
    public class Hash160Tests
    {
        [Fact]
        public void Constructor_WithValidBytes_ShouldCreateHash()
        {
            // Arrange
            var bytes = new byte[20];
            for (int i = 0; i < 20; i++)
            {
                bytes[i] = (byte)i;
            }

            // Act
            var hash = new Hash160(bytes);

            // Assert
            hash.Should().NotBeNull();
            hash.ToArray().Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void Constructor_WithInvalidLength_ShouldThrowException()
        {
            // Arrange
            var bytes = new byte[19]; // Invalid length

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Hash160(bytes));
        }

        [Fact]
        public void Parse_WithValidHexString_ShouldCreateHash()
        {
            // Arrange
            var hexString = "0x0102030405060708090a0b0c0d0e0f1011121314";

            // Act
            var hash = Hash160.Parse(hexString);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be(hexString);
        }

        [Fact]
        public void Parse_WithoutPrefix_ShouldCreateHash()
        {
            // Arrange
            var hexString = "0102030405060708090a0b0c0d0e0f1011121314";

            // Act
            var hash = Hash160.Parse(hexString);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be("0x" + hexString);
        }

        [Fact]
        public void Zero_ShouldReturnZeroHash()
        {
            // Act
            var zero = Hash160.Zero;

            // Assert
            zero.Should().NotBeNull();
            zero.ToArray().Should().OnlyContain(b => b == 0);
            zero.ToString().Should().Be("0x0000000000000000000000000000000000000000");
        }

        [Fact]
        public void Equals_WithSameHash_ShouldReturnTrue()
        {
            // Arrange
            var hex = "0x0102030405060708090a0b0c0d0e0f1011121314";
            var hash1 = Hash160.Parse(hex);
            var hash2 = Hash160.Parse(hex);

            // Act & Assert
            hash1.Equals(hash2).Should().BeTrue();
            (hash1 == hash2).Should().BeTrue();
            (hash1 != hash2).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithDifferentHash_ShouldReturnFalse()
        {
            // Arrange
            var hash1 = Hash160.Parse("0x0102030405060708090a0b0c0d0e0f1011121314");
            var hash2 = Hash160.Parse("0x1413121110100f0e0d0c0b0a0908070605040302");

            // Act & Assert
            hash1.Equals(hash2).Should().BeFalse();
            (hash1 == hash2).Should().BeFalse();
            (hash1 != hash2).Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_ShouldBeConsistent()
        {
            // Arrange
            var hex = "0x0102030405060708090a0b0c0d0e0f1011121314";
            var hash1 = Hash160.Parse(hex);
            var hash2 = Hash160.Parse(hex);

            // Act & Assert
            hash1.GetHashCode().Should().Be(hash2.GetHashCode());
        }

        [Theory]
        [InlineData("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5")] // NEO token
        [InlineData("0xd2a4cff31913016155e38e474a2c06d08be276cf")] // GAS token
        public void Parse_WithKnownContractHashes_ShouldSucceed(string contractHash)
        {
            // Act
            var hash = Hash160.Parse(contractHash);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be(contractHash);
        }

        [Fact]
        public void ToLittleEndianArray_ShouldReverseBytes()
        {
            // Arrange
            var hexString = "23ba2703c53263e8d6e522dc32203339dcd8eee9";
            var originalBytes = TestConstants.HexToBytes(hexString);
            var expectedReversed = new byte[originalBytes.Length];
            Array.Copy(originalBytes, expectedReversed, originalBytes.Length);
            Array.Reverse(expectedReversed);

            // Act
            var hash = Hash160.Parse(hexString);
            var littleEndianBytes = hash.ToLittleEndianArray();

            // Assert
            littleEndianBytes.Should().BeEquivalentTo(expectedReversed);
        }

        [Fact]
        public void FromAddress_WithValidAddress_ShouldCreateHash()
        {
            // Act
            var hash = Hash160.FromAddress(TestConstants.DefaultAccountAddress);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be("0x" + TestConstants.DefaultAccountScriptHash);
        }

        [Fact]
        public void ToAddress_ShouldReturnCorrectAddress()
        {
            // Arrange
            var hash = Hash160.Parse(TestConstants.DefaultAccountScriptHash);

            // Act
            var address = hash.ToAddress();

            // Assert
            address.Should().Be(TestConstants.DefaultAccountAddress);
        }

        [Fact]
        public void FromScript_WithValidScript_ShouldCreateHash()
        {
            // Arrange
            var scriptHex = "110c21026aa8fe6b4360a67a530e23c08c6a72525afde34719c5436f9d3ced759f939a3d110b41138defaf";

            // Act
            var hash = Hash160.FromScript(scriptHex);

            // Assert
            hash.Should().NotBeNull();
        }

        [Fact]
        public void FromPublicKey_WithValidPublicKey_ShouldCreateHash()
        {
            // Arrange
            var publicKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPublicKey);

            // Act
            var hash = Hash160.FromPublicKey(publicKeyBytes);

            // Assert
            hash.Should().NotBeNull();
        }

        [Fact]
        public void CompareTo_ShouldOrderCorrectly()
        {
            // Arrange
            var hash1 = Hash160.Parse("0102030405060708090a0b0c0d0e0f1011121314");
            var hash2 = Hash160.Parse("1413121110100f0e0d0c0b0a0908070605040302");

            // Act & Assert
            hash1.CompareTo(hash2).Should().BeLessThan(0);
            hash2.CompareTo(hash1).Should().BeGreaterThan(0);
            hash1.CompareTo(hash1).Should().Be(0);
        }

        [Fact]
        public void Size_ShouldReturn20()
        {
            // Arrange
            var hash = Hash160.Parse("0102030405060708090a0b0c0d0e0f1011121314");

            // Act & Assert
            hash.Size.Should().Be(20);
        }

        [Fact]
        public void Serialize_ShouldWriteLittleEndianBytes()
        {
            // Arrange
            var hexString = "23ba2703c53263e8d6e522dc32203339dcd8eee9";
            var originalBytes = TestConstants.HexToBytes(hexString);
            var expectedData = new byte[originalBytes.Length];
            Array.Copy(originalBytes, expectedData, originalBytes.Length);
            Array.Reverse(expectedData); // Should be little endian

            var hash = Hash160.Parse(hexString);
            using var stream = new System.IO.MemoryStream();
            using var writer = new NeoSharp.Serialization.BinaryWriter(stream);

            // Act
            hash.Serialize(writer);

            // Assert
            stream.ToArray().Should().BeEquivalentTo(expectedData);
        }

        [Fact]
        public void FromLittleEndianBytes_ShouldCreateHashCorrectly()
        {
            // Arrange
            var hexString = "23ba2703c53263e8d6e522dc32203339dcd8eee9";
            var originalBytes = TestConstants.HexToBytes(hexString);
            var littleEndianData = new byte[originalBytes.Length];
            Array.Copy(originalBytes, littleEndianData, originalBytes.Length);
            Array.Reverse(littleEndianData);

            // Act
            var hash = Hash160.FromLittleEndianBytes(littleEndianData);

            // Assert
            hash.ToString().Should().Be("0x" + hexString);
        }
    }
}