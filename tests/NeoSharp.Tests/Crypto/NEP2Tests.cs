using System;
using FluentAssertions;
using NeoSharp.Crypto;
using NeoSharp.Tests.Helpers;
using NeoSharp.Utils;
using NeoSharp.Wallet;
using Xunit;

namespace NeoSharp.Tests.Crypto
{
    /// <summary>
    /// NEP-2 wallet encryption/decryption tests
    /// Converted from Swift NEP2Tests.swift
    /// Tests the NEP-2 standard for encrypted private keys
    /// </summary>
    public class NEP2Tests
    {
        [Fact]
        public void TestDecryptWithDefaultScryptParams()
        {
            // Test decryption with default scrypt parameters
            var decrypted = NEP2.Decrypt(
                "6PYNuCdKFYG8m7hYDHdoEdmWnrkV9ETs6qqTHUsRUZ7yQvzZrCCtDxSf9D", // Correct encrypted key
                TestConstants.DefaultAccountPassword);

            var expectedPrivateKey = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            decrypted.PrivateKeyBytes.Should().BeEquivalentTo(expectedPrivateKey);
        }

        [Fact]
        public void TestDecryptWithNonDefaultScryptParams()
        {
            // Test decryption with custom scrypt parameters
            var scryptParams = new NeoSharp.Wallet.NEP6.ScryptParams { N = 256, R = 1, P = 1 };
            var encrypted = "6PYNuCdKEF9T4CEgfpfzUeQPYgeFgFTvm9UryhWw1rYUxjXvMBKWTcfXqD"; // Correct encrypted key
            
            var decrypted = NEP2.Decrypt(encrypted, TestConstants.DefaultAccountPassword, scryptParams);
            
            var expectedPrivateKey = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            decrypted.PrivateKeyBytes.Should().BeEquivalentTo(expectedPrivateKey);
        }

        [Fact]
        public void TestEncryptWithDefaultScryptParams()
        {
            // Create key pair from test private key
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);

            // Encrypt with default parameters
            var encrypted = NEP2.Encrypt(keyPair, TestConstants.DefaultAccountPassword);

            // Should produce valid NEP-2 format
            encrypted.Should().StartWith("6P");
            encrypted.Should().HaveLength(58);

            // Should be able to decrypt back
            var decrypted = NEP2.Decrypt(encrypted, TestConstants.DefaultAccountPassword);
            decrypted.PrivateKeyBytes.Should().BeEquivalentTo(privateKeyBytes);
        }

        [Fact]
        public void TestEncryptWithNonDefaultScryptParams()
        {
            // Test encryption with custom scrypt parameters
            var scryptParams = new NeoSharp.Wallet.NEP6.ScryptParams { N = 256, R = 1, P = 1 };
            var expected = "6PYNuCdKEF9T4CEgfpfzUeQPYgeFgFTvm9UryhWw1rYUxjXvMBKWTcfXqD"; // Correct encrypted key
            
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);
            
            var encrypted = NEP2.Encrypt(keyPair, TestConstants.DefaultAccountPassword, scryptParams);
            
            encrypted.Should().Be(expected);
        }

        [Fact]
        public void TestRoundTripEncryptionDecryption()
        {
            // Test that encrypt -> decrypt produces original key
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var originalKeyPair = new ECKeyPair(privateKeyBytes);
            var password = "TestPassword123!";

            // Encrypt
            var encrypted = NEP2.Encrypt(originalKeyPair, password);

            // Decrypt
            var decryptedKeyPair = NEP2.Decrypt(encrypted, password);

            // Should match original
            decryptedKeyPair.PrivateKeyBytes.Should().BeEquivalentTo(originalKeyPair.PrivateKeyBytes);
            decryptedKeyPair.PublicKey.GetEncoded().Should().BeEquivalentTo(originalKeyPair.PublicKey.GetEncoded());
        }

        [Fact]
        public void TestDecryptWithWrongPassword()
        {
            // Test that wrong password throws exception
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);
            var correctPassword = "CorrectPassword";
            var wrongPassword = "WrongPassword";

            var encrypted = NEP2.Encrypt(keyPair, correctPassword);

            // Should throw with wrong password
            Action act = () => NEP2.Decrypt(encrypted, wrongPassword);
            act.Should().Throw<NEP2Exception>()
                .WithMessage("*password*");
        }

        [Fact]
        public void TestInvalidNEP2Format()
        {
            // Test that invalid NEP-2 format throws exception
            var invalidFormats = new[]
            {
                "",  // empty
                "5K",  // too short
                "6P1234567890123456789012345678901234567890123456789012345",  // wrong length
                "7P1234567890123456789012345678901234567890123456789012345678",  // wrong prefix
                "6P123456789012345678901234567890123456789012345678901234567!",  // invalid characters
            };

            foreach (var invalidFormat in invalidFormats)
            {
                Action act = () => NEP2.Decrypt(invalidFormat, "password");
                act.Should().Throw<FormatException>()
                    .WithMessage("*NEP-2*");
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("password")]
        [InlineData("VeryLongPasswordWithSpecialCharacters!@#$%^&*()_+-=")]
        [InlineData("🔐🔑💎")]  // Unicode password
        public void TestVariousPasswords(string password)
        {
            // Test encryption/decryption with various password formats
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);

            var encrypted = NEP2.Encrypt(keyPair, password);
            var decrypted = NEP2.Decrypt(encrypted, password);

            decrypted.PrivateKeyBytes.Should().BeEquivalentTo(keyPair.PrivateKeyBytes);
        }

        [Fact]
        public void TestNEP2WithDifferentScryptParams()
        {
            // Test various scrypt parameter combinations
            var testParams = new NeoSharp.Wallet.NEP6.ScryptParams[]
            {
                new() { N = 16384, R = 8, P = 8 },     // Default NEP-2
                new() { N = 256, R = 1, P = 1 },       // Fast (testing)
                new() { N = 32768, R = 8, P = 8 },     // Higher security
                new() { N = 4096, R = 4, P = 4 },      // Moderate
            };

            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);
            var password = "TestPassword";

            foreach (var scryptParam in testParams)
            {
                var encrypted = NEP2.Encrypt(keyPair, password, scryptParam);
                var decrypted = NEP2.Decrypt(encrypted, password, scryptParam);

                encrypted.Should().StartWith("6P");
                encrypted.Should().HaveLength(58);
                decrypted.PrivateKeyBytes.Should().BeEquivalentTo(keyPair.PrivateKeyBytes);
            }
        }

        [Fact]
        public void TestNEP2KeyChecksumValidation()
        {
            // Test that checksum validation works correctly
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);
            var password = "TestPassword";

            var encrypted = NEP2.Encrypt(keyPair, password);
            
            // Corrupt the checksum (last few characters)
            var corrupted = encrypted.Substring(0, 54) + "XXXX";

            Action act = () => NEP2.Decrypt(corrupted, password);
            act.Should().Throw<FormatException>()
                .WithMessage("*base58*");
        }

        [Fact]
        public void TestNEP2AddressGeneration()
        {
            // Test that decrypted key can generate correct address
            var encrypted = "6PYNuCdKFYG8m7hYDHdoEdmWnrkV9ETs6qqTHUsRUZ7yQvzZrCCtDxSf9D"; // Correct encrypted key
            var password = TestConstants.DefaultAccountPassword;

            var keyPair = NEP2.Decrypt(encrypted, password);
            var address = keyPair.GetAddress();

            address.Should().Be(TestConstants.DefaultAccountAddress);
        }

        [Fact]
        public void TestMultipleEncryptionsProduceSameResults()
        {
            // Test that multiple encryptions of same key produce same results
            // (NEP-2 is deterministic for same inputs)
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);
            var password = "TestPassword";

            var encrypted1 = NEP2.Encrypt(keyPair, password);
            var encrypted2 = NEP2.Encrypt(keyPair, password);

            // Should be same (deterministic encryption)
            encrypted1.Should().Be(encrypted2);

            // Both should decrypt to same key
            var decrypted1 = NEP2.Decrypt(encrypted1, password);
            var decrypted2 = NEP2.Decrypt(encrypted2, password);

            decrypted1.PrivateKeyBytes.Should().BeEquivalentTo(decrypted2.PrivateKeyBytes);
            decrypted1.PrivateKeyBytes.Should().BeEquivalentTo(keyPair.PrivateKeyBytes);
        }
    }
}