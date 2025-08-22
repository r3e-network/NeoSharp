using System;
using System.Text;
using FluentAssertions;
using NeoSharp.Tests.Helpers;
using Xunit;

namespace NeoSharp.Tests.Crypto
{
    /// <summary>
    /// Base64 tests converted from Swift NeoSwift Base64Tests.swift
    /// </summary>
    public class Base64Tests
    {
        private const string InputString = "150c14242dbf5e2f6ac2568b59b7822278d571b75f17be0c14242dbf5e2f6ac2568b59b7822278d571b75f17be13c00c087472616e736665720c14897720d8cd76f4f00abfa37c0edd889c208fde9b41627d5b5238";
        private const string OutputString = "FQwUJC2/Xi9qwlaLWbeCInjVcbdfF74MFCQtv14vasJWi1m3giJ41XG3Xxe+E8AMCHRyYW5zZmVyDBSJdyDYzXb08Aq/o3wO3YicII/em0FifVtSOA==";

        [Fact]
        public void Base64Encode_ForString_ShouldEncodeCorrectly()
        {
            // Arrange
            var inputBytes = TestConstants.HexToBytes(InputString);

            // Act
            var result = Convert.ToBase64String(inputBytes);

            // Assert
            result.Should().Be(OutputString);
        }

        [Fact]
        public void Base64Encode_ForBytes_ShouldEncodeCorrectly()
        {
            // Arrange
            var inputBytes = TestConstants.HexToBytes(InputString);

            // Act
            var result = Convert.ToBase64String(inputBytes);

            // Assert
            result.Should().Be(OutputString);
        }

        [Fact]
        public void Base64Decode_ShouldDecodeCorrectly()
        {
            // Act
            var decodedBytes = Convert.FromBase64String(OutputString);
            var result = TestConstants.BytesToHex(decodedBytes);

            // Assert
            result.Should().Be(InputString);
        }

        [Fact]
        public void Base64Encode_WithEmptyInput_ShouldReturnEmptyString()
        {
            // Arrange
            var emptyBytes = Array.Empty<byte>();

            // Act
            var result = Convert.ToBase64String(emptyBytes);

            // Assert
            result.Should().Be("");
        }

        [Fact]
        public void Base64Decode_WithEmptyInput_ShouldReturnEmptyArray()
        {
            // Act
            var result = Convert.FromBase64String("");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Base64Encode_WithUtf8String_ShouldEncodeCorrectly()
        {
            // Arrange
            var testString = "Hello, Neo!";
            var inputBytes = Encoding.UTF8.GetBytes(testString);

            // Act
            var encoded = Convert.ToBase64String(inputBytes);
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

            // Assert
            decoded.Should().Be(testString);
        }

        [Fact]
        public void Base64Decode_WithInvalidInput_ShouldThrowException()
        {
            // Arrange
            var invalidBase64 = "Invalid!Base64#String";

            // Act & Assert
            Assert.Throws<FormatException>(() => Convert.FromBase64String(invalidBase64));
        }

        [Theory]
        [InlineData("SGVsbG8gV29ybGQ=", "Hello World")]
        [InlineData("TmVv", "Neo")]
        [InlineData("QmxvY2tjaGFpbg==", "Blockchain")]
        public void Base64RoundTrip_ShouldPreserveData(string expectedBase64, string originalText)
        {
            // Arrange
            var originalBytes = Encoding.UTF8.GetBytes(originalText);

            // Act
            var encoded = Convert.ToBase64String(originalBytes);
            var decodedBytes = Convert.FromBase64String(encoded);
            var decodedText = Encoding.UTF8.GetString(decodedBytes);

            // Assert
            encoded.Should().Be(expectedBase64);
            decodedText.Should().Be(originalText);
        }
    }
}