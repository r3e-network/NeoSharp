using System;
using FluentAssertions;
using NeoSharp.Tests.Helpers;
using NeoSharp.Types;
using Xunit;

namespace NeoSharp.Tests.Types
{
    /// <summary>
    /// Hash256 tests converted from Swift NeoSwift Hash256Tests.swift
    /// </summary>
    public class Hash256Tests
    {
        [Fact]
        public void Parse_WithValidHexString_ShouldCreateHash()
        {
            // Test with prefix
            var hash1 = Hash256.Parse("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            hash1.ToString().Should().Be("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");

            // Test without prefix
            var hash2 = Hash256.Parse("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            hash2.ToString().Should().Be("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
        }

        [Fact]
        public void Constructor_WithInvalidHex_ShouldThrowException()
        {
            // Invalid hex with odd length
            Action act1 = () => Hash256.Parse("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21ae");
            act1.Should().Throw<ArgumentException>().WithMessage("*hexadecimal*");

            // Invalid hex with non-hex character
            Action act2 = () => Hash256.Parse("g804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");
            act2.Should().Throw<ArgumentException>().WithMessage("*hexadecimal*");

            // Invalid length - too short
            Action act3 = () => Hash256.Parse("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a2");
            act3.Should().Throw<ArgumentException>().WithMessage("*32 bytes*");

            // Invalid length - too long
            Action act4 = () => Hash256.Parse("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a12");
            act4.Should().Throw<ArgumentException>().WithMessage("*32 bytes*");
        }

        [Fact]
        public void Constructor_WithValidBytes_ShouldCreateHash()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var bytes = TestConstants.HexToBytes(hexString);

            // Act
            var hash = new Hash256(bytes);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be("0x" + hexString);
        }

        [Fact]
        public void ToLittleEndianArray_ShouldReverseBytes()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var originalBytes = TestConstants.HexToBytes(hexString);
            var expectedReversed = new byte[originalBytes.Length];
            Array.Copy(originalBytes, expectedReversed, originalBytes.Length);
            Array.Reverse(expectedReversed);

            // Act
            var hash = Hash256.Parse(hexString);
            var littleEndianBytes = hash.ToLittleEndianArray();

            // Assert
            littleEndianBytes.Should().BeEquivalentTo(expectedReversed);
        }

        [Fact]
        public void Serialize_ShouldWriteLittleEndianBytes()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var originalBytes = TestConstants.HexToBytes(hexString);
            var expectedData = new byte[originalBytes.Length];
            Array.Copy(originalBytes, expectedData, originalBytes.Length);
            Array.Reverse(expectedData); // Should be little endian

            var hash = Hash256.Parse(hexString);
            using var stream = new System.IO.MemoryStream();
            using var writer = new NeoSharp.Serialization.BinaryWriter(stream);

            // Act
            hash.Serialize(writer);

            // Assert
            stream.ToArray().Should().BeEquivalentTo(expectedData);
        }

        [Fact]
        public void Deserialize_ShouldCreateHashFromLittleEndianBytes()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var originalBytes = TestConstants.HexToBytes(hexString);
            var littleEndianData = new byte[originalBytes.Length];
            Array.Copy(originalBytes, littleEndianData, originalBytes.Length);
            Array.Reverse(littleEndianData);

            // Act
            var hash = Hash256.FromLittleEndianBytes(littleEndianData);

            // Assert
            hash.ToString().Should().Be("0x" + hexString);
        }

        [Fact]
        public void Equals_ShouldWorkCorrectly()
        {
            // Arrange
            var bytes1 = TestConstants.HexToBytes("1aa274391ab7127ca6d6b917d413919000ebee2b14974e67b49ac62082a904b8");
            Array.Reverse(bytes1); // Make little endian
            var bytes2 = TestConstants.HexToBytes("b43034ab680d646f8b6ca71647aa6ba167b2eb0b3757e545f6c2715787b13272");
            Array.Reverse(bytes2); // Make little endian

            var hash1 = new Hash256(bytes1);
            var hash2 = new Hash256(bytes2);
            var hash3 = Hash256.Parse("0xb804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");

            // Act & Assert
            hash1.Should().NotBe(hash2);
            hash1.Should().Be(hash1);
            hash1.Should().Be(hash3);
            hash1.GetHashCode().Should().Be(hash3.GetHashCode());
        }

        [Fact]
        public void CompareTo_ShouldOrderCorrectly()
        {
            // Arrange
            var bytes1 = TestConstants.HexToBytes("1aa274391ab7127ca6d6b917d413919000ebee2b14974e67b49ac62082a904b8");
            Array.Reverse(bytes1);
            var bytes2 = TestConstants.HexToBytes("b43034ab680d646f8b6ca71647aa6ba167b2eb0b3757e545f6c2715787b13272");
            Array.Reverse(bytes2);

            var hash1 = new Hash256(bytes1);
            var hash2 = new Hash256(bytes2);
            var hash3 = Hash256.Parse("0xf4609b99e171190c22adcf70c88a7a14b5b530914d2398287bd8bb7ad95a661c");

            // Act & Assert
            hash1.CompareTo(hash2).Should().BeGreaterThan(0);
            hash3.CompareTo(hash1).Should().BeGreaterThan(0);
            hash3.CompareTo(hash2).Should().BeGreaterThan(0);
        }

        [Fact]
        public void Size_ShouldReturn32()
        {
            // Arrange
            var hash = Hash256.Parse("b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a");

            // Act & Assert
            hash.Size.Should().Be(32);
        }

        [Fact]
        public void Zero_ShouldReturnZeroHash()
        {
            // Act
            var zero = Hash256.Zero;

            // Assert
            zero.Should().NotBeNull();
            zero.ToArray().Should().OnlyContain(b => b == 0);
            zero.ToString().Should().Be("0x0000000000000000000000000000000000000000000000000000000000000000");
        }

        [Theory]
        [InlineData("0x668e0c1f9870f61d45b8a91c77585e468c22c1216c5d6f941b971c3655974d31")] // Example block hash
        [InlineData("0xf782c7fde78c1c2b2021d1c0c8f17236d1c68690b96ad884587871997ae823f6")] // Example transaction hash
        public void Parse_WithRealHashes_ShouldSucceed(string hashString)
        {
            // Act
            var hash = Hash256.Parse(hashString);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be(hashString);
        }

        [Fact]
        public void FromData_ShouldCreateHashFromData()
        {
            // Arrange
            var testData = System.Text.Encoding.UTF8.GetBytes("Hello, NeoSharp!");

            // Act
            var hash = Hash256.FromData(testData);

            // Assert
            hash.Should().NotBeNull();
            hash.Size.Should().Be(32);
            hash.ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void FromLittleEndianBytes_ShouldCreateHashCorrectly()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var originalBytes = TestConstants.HexToBytes(hexString);
            var littleEndianData = new byte[originalBytes.Length];
            Array.Copy(originalBytes, littleEndianData, originalBytes.Length);
            Array.Reverse(littleEndianData);

            // Act
            var hash = Hash256.FromLittleEndianBytes(littleEndianData);

            // Assert
            hash.ToString().Should().Be("0x" + hexString);
        }

        [Fact]
        public void ToArray_ShouldReturnBigEndianBytes()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var expectedBytes = TestConstants.HexToBytes(hexString);

            // Act
            var hash = Hash256.Parse(hexString);
            var actualBytes = hash.ToArray();

            // Assert
            actualBytes.Should().BeEquivalentTo(expectedBytes);
        }

        [Fact]
        public void ImplicitStringConversion_ShouldReturnHexString()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";
            var hash = Hash256.Parse(hexString);

            // Act
            string converted = hash;

            // Assert
            converted.Should().Be(hexString);
        }

        [Fact]
        public void ExplicitHashConversion_ShouldCreateFromHexString()
        {
            // Arrange
            var hexString = "b804a98220c69ab4674e97142beeeb00909113d417b9d6a67c12b71a3974a21a";

            // Act
            var hash = (Hash256)hexString;

            // Assert
            hash.ToString().Should().Be("0x" + hexString);
        }
    }
}