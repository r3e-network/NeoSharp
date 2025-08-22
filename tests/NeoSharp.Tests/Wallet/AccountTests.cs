using System;
using FluentAssertions;
using NeoSharp.Crypto;
using NeoSharp.Script;
using NeoSharp.Tests.Helpers;
using NeoSharp.Types;
using NeoSharp.Utils;
using NeoSharp.Wallet;
using Xunit;

namespace NeoSharp.Tests.Wallet
{
    /// <summary>
    /// Tests for Account functionality.
    /// Converted from Swift NeoSwiftTests/unit/wallet/AccountTests.swift
    /// </summary>
    public class AccountTests
    {
        [Fact]
        public void Create_ShouldCreateValidGenericAccount()
        {
            // Act
            var account = Account.Create();

            // Assert
            account.Should().NotBeNull();
            account.Address.Should().NotBeNullOrEmpty();
            account.VerificationScript.Should().NotBeNull();
            account.KeyPair.Should().NotBeNull();
            account.Label.Should().NotBeNullOrEmpty();
            account.EncryptedPrivateKey.Should().BeNull();
            account.IsLocked.Should().BeFalse();
            account.IsDefault.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithExistingKeyPair_ShouldCreateCorrectAccount()
        {
            // Arrange
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);

            // Act
            var account = new Account(keyPair);

            // Assert
            account.IsMultiSig.Should().BeFalse();
            account.Address.Should().NotBeNullOrEmpty();
            account.Label.Should().NotBeNullOrEmpty();
            account.VerificationScript.Should().NotBeNull();
        }

        [Fact]
        public void FromVerificationScript_ShouldCreateCorrectAccount()
        {
            // Arrange
            var scriptBytes = TestConstants.HexToBytes("0c2102163946a133e3d2e0d987fb90cb01b060ed1780f1718e2da28edf13b965fd2b600b4195440d78");
            var verificationScript = new VerificationScript(scriptBytes);

            // Act
            var account = Account.FromVerificationScript(verificationScript);

            // Assert
            account.Address.Should().NotBeNullOrEmpty();
            account.VerificationScript.Script.Should().BeEquivalentTo(scriptBytes);
        }

        [Fact]
        public void FromPublicKey_ShouldCreateCorrectAccount()
        {
            // Arrange
            var publicKey = new ECPublicKey(TestConstants.HexToBytes(TestConstants.DefaultAccountPublicKey));

            // Act
            var account = Account.FromPublicKey(publicKey);

            // Assert
            account.Address.Should().NotBeNullOrEmpty();
            account.VerificationScript.Should().NotBeNull();
        }

        [Fact]
        public void CreateMultiSigAccount_ShouldCreateCorrectMultiSigAccount()
        {
            // Arrange
            var publicKey = new ECPublicKey(TestConstants.HexToBytes(TestConstants.DefaultAccountPublicKey));
            var publicKeys = new[] { publicKey };

            // Act
            var account = Account.CreateMultiSigAccount(publicKeys, 1);

            // Assert
            account.IsMultiSig.Should().BeTrue();
            account.SigningThreshold.Should().Be(1);
            account.NumberOfParticipants.Should().Be(1);
            account.VerificationScript.Should().NotBeNull();
        }

        [Fact]
        public void FromWIF_ShouldCreateCorrectAccount()
        {
            // Arrange
            var wif = TestConstants.DefaultAccountWIF;

            // Act
            var account = Account.FromWIF(wif);

            // Assert
            account.Should().NotBeNull();
            account.Address.Should().Be(TestConstants.DefaultAccountAddress);
            account.KeyPair.Should().NotBeNull();
            account.IsLocked.Should().BeFalse();
        }

        [Fact]
        public void FromPrivateKey_ShouldCreateCorrectAccount()
        {
            // Arrange
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);

            // Act
            var account = new Account(new ECKeyPair(privateKeyBytes));

            // Assert
            account.Should().NotBeNull();
            account.Address.Should().NotBeNullOrEmpty();
            account.KeyPair.PrivateKeyBytes.Should().BeEquivalentTo(privateKeyBytes);
        }

        [Fact]
        public void GetScriptHash_ShouldReturnCorrectHash()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);

            // Act
            var scriptHash = account.GetScriptHash();

            // Assert
            scriptHash.Should().NotBeNull();
            scriptHash.ToString().Should().Be("0x" + TestConstants.DefaultAccountScriptHash);
        }

        [Fact]
        public void GetAddress_ShouldReturnCorrectAddress()
        {
            // Arrange
            var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
            var keyPair = new ECKeyPair(privateKeyBytes);
            var account = new Account(keyPair);

            // Act
            var address = account.Address;

            // Assert
            address.Should().Be(TestConstants.DefaultAccountAddress);
        }

        [Fact]
        public void IsWatchOnly_ShouldReturnTrueForAccountsWithoutPrivateKey()
        {
            // Arrange
            var publicKey = new ECPublicKey(TestConstants.HexToBytes(TestConstants.DefaultAccountPublicKey));
            var account = Account.FromPublicKey(publicKey);

            // Act & Assert
            account.HasPrivateKey.Should().BeFalse(); // IsWatchOnly equivalent
        }

        [Fact]
        public void IsWatchOnly_ShouldReturnFalseForAccountsWithPrivateKey()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);

            // Act & Assert
            account.HasPrivateKey.Should().BeTrue(); // Not watch-only
        }

        [Fact]
        public void EncryptPrivateKey_ShouldEncryptPrivateKey()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);
            var password = TestConstants.DefaultAccountPassword;

            // Act
            account.EncryptPrivateKey(password);

            // Assert
            account.KeyPair.Should().BeNull();
            account.EncryptedPrivateKey.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void DecryptPrivateKey_ShouldDecryptPrivateKey()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);
            var password = TestConstants.DefaultAccountPassword;
            account.EncryptPrivateKey(password);

            // Act
            account.DecryptPrivateKey(password);

            // Assert
            account.KeyPair.Should().NotBeNull();
            account.EncryptedPrivateKey.Should().NotBeNull(); // Encrypted key is kept
        }

        [Fact]
        public void DecryptPrivateKey_WithWrongPassword_ShouldThrowException()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);
            account.EncryptPrivateKey(TestConstants.DefaultAccountPassword);

            // Act & Assert
            Action act = () => account.DecryptPrivateKey("wrongpassword");
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Sign_ShouldCreateValidSignature()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);
            var message = "Hello, Neo!"u8.ToArray();
            var messageHash = NeoSharp.Crypto.Hash.SHA256(message);

            // Act
            var signature = account.Sign(messageHash);

            // Assert
            signature.Should().NotBeNull();
            signature.Length.Should().Be(64); // r + s values, 32 bytes each
        }

        [Fact]
        public void Sign_WithLockedAccount_ShouldThrowException()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);
            account.EncryptPrivateKey(TestConstants.DefaultAccountPassword);
            var message = "Hello, Neo!"u8.ToArray();
            var messageHash = NeoSharp.Crypto.Hash.SHA256(message);

            // Act & Assert
            Action act = () => account.Sign(messageHash);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*watch-only*");
        }

        [Fact]
        public void SignData_ShouldCreateValidSignature()
        {
            // Arrange
            var account = Account.FromWIF(TestConstants.DefaultAccountWIF);
            var message = "Hello, Neo!"u8.ToArray();
            var messageHash = NeoSharp.Crypto.Hash.SHA256(message);

            // Act
            var signature = account.SignData(messageHash);

            // Assert
            signature.Should().NotBeNull();
            signature.Length.Should().Be(64); // r + s values, 32 bytes each
        }

        [Fact]
        public void CreateMultiSigAccount_WithInvalidThreshold_ShouldThrowException()
        {
            // Arrange
            var publicKey = new ECPublicKey(TestConstants.HexToBytes(TestConstants.DefaultAccountPublicKey));
            var publicKeys = new[] { publicKey };

            // Act & Assert
            Action act = () => Account.CreateMultiSigAccount(publicKeys, 0);
            act.Should().Throw<ArgumentException>();

            Action act2 = () => Account.CreateMultiSigAccount(publicKeys, 2);
            act2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CreateMultiSigAccount_WithNullPublicKeys_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => Account.CreateMultiSigAccount(null, 1);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("publicKeys");
        }

        [Fact]
        public void CreateMultiSigAccount_WithEmptyPublicKeys_ShouldThrowArgumentException()
        {
            // Act & Assert
            Action act = () => Account.CreateMultiSigAccount(Array.Empty<ECPublicKey>(), 1);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*required*");
        }

        [Fact]
        public void Constructor_WithNullKeyPair_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new Account((ECKeyPair)null);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("keyPair");
        }

        [Fact]
        public void Label_ShouldBeSettable()
        {
            // Arrange
            var account = Account.Create();
            var newLabel = "MyCustomLabel";

            // Act
            account.Label = newLabel;

            // Assert
            account.Label.Should().Be(newLabel);
        }

        [Fact]
        public void IsDefault_ShouldReflectWalletState()
        {
            // Arrange
            var account = Account.Create();

            // Act & Assert
            // IsDefault is computed based on wallet state, not settable directly
            account.IsDefault.Should().BeFalse(); // No wallet assigned
        }

        [Fact]
        public void Equals_ShouldCompareByScriptHash()
        {
            // Arrange
            var account1 = Account.FromWIF(TestConstants.DefaultAccountWIF);
            var account2 = Account.FromWIF(TestConstants.DefaultAccountWIF);
            var account3 = Account.Create();

            // Act & Assert
            account1.Should().Be(account2);
            account1.Should().NotBe(account3);
            account1.GetHashCode().Should().Be(account2.GetHashCode());
        }
    }
}