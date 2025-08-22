using System;
using FluentAssertions;
using NeoSharp.Crypto;
using NeoSharp.Utils;
using NeoSharp.Tests.Helpers;
using Xunit;

namespace NeoSharp.Tests.Crypto
{
    /// <summary>
    /// Tests for Wallet Import Format (WIF) functionality.
    /// Converted from Swift NeoSwiftTests/unit/crypto/WIFTests.swift
    /// </summary>
    public class WIFTests
    {
        private const string ValidWif = "L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13A";
        private const string PrivateKeyHex = "9117f4bf9be717c9a90994326897f4243503accd06712162267e77f18b49c3a3";

        [Fact]
        public void ValidWifToPrivateKey_ShouldDecodeCorrectly()
        {
            // Act
            var (privateKey, compressed) = WIF.Decode(ValidWif);

            // Assert
            privateKey.Should().NotBeNull();
            HexExtensions.ToHexString(privateKey).Should().Be(PrivateKeyHex);
            compressed.Should().BeTrue();
        }

        [Fact]
        public void PrivateKeyToWif_ShouldEncodeCorrectly()
        {
            // Arrange
            var privateKeyBytes = TestConstants.HexToBytes(PrivateKeyHex);

            // Act
            var wif = WIF.Encode(privateKeyBytes, compressed: true);

            // Assert
            wif.Should().Be(ValidWif);
        }

        [Fact]
        public void RoundTrip_ShouldPreservePrivateKey()
        {
            // Arrange
            var originalPrivateKey = TestConstants.HexToBytes(PrivateKeyHex);

            // Act
            var wif = WIF.Encode(originalPrivateKey, compressed: true);
            var (decodedPrivateKey, _) = WIF.Decode(wif);

            // Assert
            decodedPrivateKey.Should().BeEquivalentTo(originalPrivateKey);
        }

        [Theory]
        [InlineData("L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13Ahc7S")] // Too large
        [InlineData("L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWML")] // Too small
        public void WronglySizedWifs_ShouldThrowException(string invalidWif)
        {
            // Act & Assert
            Action act = () => WIF.Decode(invalidWif);
            act.Should().Throw<WIFException>()
                .WithMessage("*format*");
        }

        [Fact]
        public void WrongFirstByteWif_ShouldThrowException()
        {
            // Arrange
            var base58Bytes = NeoSharp.Crypto.Base58.Decode(ValidWif);
            base58Bytes[0] = 0x81; // Change first byte from 0x80 to 0x81
            var wrongFirstByteWif = NeoSharp.Crypto.Base58.Encode(base58Bytes);

            // Act & Assert
            Action act = () => WIF.Decode(wrongFirstByteWif);
            act.Should().Throw<WIFException>()
                .WithMessage("*prefix*");
        }

        [Fact]
        public void WrongByte33Wif_ShouldThrowException()
        {
            // Arrange
            var base58Bytes = NeoSharp.Crypto.Base58.Decode(ValidWif);
            base58Bytes[33] = 0x00; // Change compressed flag from 0x01 to 0x00
            var wrongByte33Wif = NeoSharp.Crypto.Base58.Encode(base58Bytes);

            // Act & Assert
            Action act = () => WIF.Decode(wrongByte33Wif);
            act.Should().Throw<WIFException>()
                .WithMessage("*compressed*");
        }

        [Fact]
        public void InvalidChecksum_ShouldThrowException()
        {
            // Arrange
            var base58Bytes = NeoSharp.Crypto.Base58.Decode(ValidWif);
            base58Bytes[^1] = (byte)(base58Bytes[^1] ^ 0xFF); // Corrupt checksum
            var invalidChecksumWif = NeoSharp.Crypto.Base58.Encode(base58Bytes);

            // Act & Assert
            Action act = () => WIF.Decode(invalidChecksumWif);
            act.Should().Throw<WIFException>()
                .WithMessage("*checksum*");
        }

        [Fact]
        public void NullPrivateKey_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => WIF.Encode(null);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("privateKey");
        }

        [Theory]
        [InlineData(31)] // Too short
        [InlineData(33)] // Too long
        public void InvalidPrivateKeyLength_ShouldThrowWIFException(int length)
        {
            // Arrange
            var invalidPrivateKey = new byte[length];

            // Act & Assert
            Action act = () => WIF.Encode(invalidPrivateKey);
            act.Should().Throw<WIFException>()
                .WithMessage("*32 bytes*");
        }

        [Fact]
        public void NullWifString_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => WIF.Decode(null);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("wifString");
        }

        [Fact]
        public void EmptyWifString_ShouldThrowWIFException()
        {
            // Act & Assert
            Action act = () => WIF.Decode("");
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("wifString");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompressedFlag_ShouldBeHandledCorrectly(bool compressed)
        {
            // Arrange
            var privateKeyBytes = TestConstants.HexToBytes(PrivateKeyHex);

            // Act
            var wif = WIF.Encode(privateKeyBytes, compressed);
            var (decoded, _) = WIF.Decode(wif);

            // Assert
            decoded.Should().BeEquivalentTo(privateKeyBytes);
        }

        [Fact]
        public void UncompressedWif_ShouldDecodeCorrectly()
        {
            // Arrange - Create uncompressed WIF
            var privateKeyBytes = TestConstants.HexToBytes(PrivateKeyHex);
            var uncompressedWif = WIF.Encode(privateKeyBytes, compressed: false);

            // Act
            var (decoded, _) = WIF.Decode(uncompressedWif);

            // Assert
            decoded.Should().BeEquivalentTo(privateKeyBytes);
        }

        [Fact]
        public void IsValidWIF_WithValidWIF_ShouldReturnTrue()
        {
            // Act & Assert
            WIF.IsValid(ValidWif).Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("L25kgAQJXNHnhc7Sx9bomxxwVSMsZdkaNQ3m2VfHrnLzKWMLP13Ahc7S")] // Too long
        public void IsValidWIF_WithInvalidWIF_ShouldReturnFalse(string invalidWif)
        {
            // Act & Assert
            WIF.IsValid(invalidWif).Should().BeFalse();
        }
    }
}