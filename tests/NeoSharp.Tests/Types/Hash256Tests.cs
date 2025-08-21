using System;
using FluentAssertions;
using NeoSharp.Types;
using Xunit;

namespace NeoSharp.Tests.Types
{
    public class Hash256Tests
    {
        [Fact]
        public void Constructor_WithValidBytes_ShouldCreateHash()
        {
            // Arrange
            var bytes = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                bytes[i] = (byte)i;
            }

            // Act
            var hash = new Hash256(bytes);

            // Assert
            hash.Should().NotBeNull();
            hash.ToArray().Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void Constructor_WithInvalidLength_ShouldThrowException()
        {
            // Arrange
            var bytes = new byte[31]; // Invalid length

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Hash256(bytes));
        }

        [Fact]
        public void Parse_WithValidHexString_ShouldCreateHash()
        {
            // Arrange
            var hexString = "0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";

            // Act
            var hash = Hash256.Parse(hexString);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be(hexString);
        }

        [Fact]
        public void Parse_WithoutPrefix_ShouldCreateHash()
        {
            // Arrange
            var hexString = "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";

            // Act
            var hash = Hash256.Parse(hexString);

            // Assert
            hash.Should().NotBeNull();
            hash.ToString().Should().Be("0x" + hexString);
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

        [Fact]
        public void Equals_WithSameHash_ShouldReturnTrue()
        {
            // Arrange
            var hex = "0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";
            var hash1 = Hash256.Parse(hex);
            var hash2 = Hash256.Parse(hex);

            // Act & Assert
            hash1.Equals(hash2).Should().BeTrue();
            (hash1 == hash2).Should().BeTrue();
            (hash1 != hash2).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithDifferentHash_ShouldReturnFalse()
        {
            // Arrange
            var hash1 = Hash256.Parse("0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
            var hash2 = Hash256.Parse("0x201f1e1d1c1b1a191817161514131211100f0e0d0c0b0a090807060504030201");

            // Act & Assert
            hash1.Equals(hash2).Should().BeFalse();
            (hash1 == hash2).Should().BeFalse();
            (hash1 != hash2).Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_ShouldBeConsistent()
        {
            // Arrange
            var hex = "0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";
            var hash1 = Hash256.Parse(hex);
            var hash2 = Hash256.Parse(hex);

            // Act & Assert
            hash1.GetHashCode().Should().Be(hash2.GetHashCode());
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
        public void CompareTo_ShouldOrderCorrectly()
        {
            // Arrange
            var hash1 = Hash256.Parse("0x0000000000000000000000000000000000000000000000000000000000000001");
            var hash2 = Hash256.Parse("0x0000000000000000000000000000000000000000000000000000000000000002");
            var hash3 = Hash256.Parse("0x0000000000000000000000000000000000000000000000000000000000000002");

            // Act & Assert
            hash1.CompareTo(hash2).Should().BeLessThan(0);
            hash2.CompareTo(hash1).Should().BeGreaterThan(0);
            hash2.CompareTo(hash3).Should().Be(0);
        }
    }
}