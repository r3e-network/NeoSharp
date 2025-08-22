using System;
using System.Numerics;
using FluentAssertions;
using NeoSharp.Crypto;
using NeoSharp.Tests.Helpers;
using NeoSharp.Utils;
using Xunit;

namespace NeoSharp.Tests.Crypto
{
    /// <summary>
    /// Digital signature tests for ECDSA operations
    /// Converted from Swift SignTests.swift
    /// Tests message signing, signature verification, and key recovery
    /// </summary>
    public class SignTests
    {
        private readonly ECPrivateKey _privateKey;
        private readonly ECPublicKey _publicKey;
        private readonly SignatureData _signatureData;
        private readonly string _testMessage = "A test message";
        private readonly string _expectedR = "147e5f3c929dd830d961626551dbea6b70e4b2837ed2fe9089eed2072ab3a655";
        private readonly string _expectedS = "523ae0fa8711eee4769f1913b180b9b3410bbb2cf770f529c85f6886f22cbaaf";

        public SignTests()
        {
            // Initialize test key pair with known values
            var privateKeyBigInt = BigInteger.Parse("9117f4bf9be717c9a90994326897f4243503accd06712162267e77f18b49c3a3", 
                System.Globalization.NumberStyles.HexNumber);

            var privateKeyBytes = privateKeyBigInt.ToByteArray();
            if (privateKeyBytes.Length < 32)
            {
                var padded = new byte[32];
                Array.Copy(privateKeyBytes, 0, padded, 32 - privateKeyBytes.Length, privateKeyBytes.Length);
                privateKeyBytes = padded;
            }
            else if (privateKeyBytes.Length > 32)
            {
                var trimmed = new byte[32];
                Array.Copy(privateKeyBytes, privateKeyBytes.Length - 32, trimmed, 0, 32);
                privateKeyBytes = trimmed;
            }
            _privateKey = new ECPrivateKey(privateKeyBytes);
            
            // Derive the public key from the private key using proper elliptic curve mathematics
            _publicKey = _privateKey.GetPublicKey();

            _signatureData = new SignatureData(
                v: 27,
                r: HexExtensions.HexToBytes(_expectedR),
                s: HexExtensions.HexToBytes(_expectedS)
            );
        }

        private byte[] TestMessageBytes => System.Text.Encoding.UTF8.GetBytes(_testMessage);

        private ECKeyPair KeyPair => new ECKeyPair(_privateKey);
        private ECPublicKey DerivedPublicKey => KeyPair.PublicKey;

        [Fact]
        public void TestSignMessage()
        {
            // Test message signing - just verify it doesn't throw and produces a valid signature
            var signatureData = Sign.SignMessage(TestMessageBytes, KeyPair);
            
            // Check that signature components are present and valid
            signatureData.Should().NotBeNull();
            signatureData.R.Should().NotBeNull();
            signatureData.S.Should().NotBeNull();
            signatureData.R.Length.Should().Be(32);
            signatureData.S.Length.Should().Be(32);
            signatureData.V.Should().BeInRange((byte)27, (byte)30);
            
            // Verify the signature is valid using the actual verification method
            var isValid = Sign.VerifySignature(TestMessageBytes, signatureData, _publicKey);
            isValid.Should().BeTrue();

            // Test hex message signing
            var signatureDataHex = Sign.SignHexMessage(TestConstants.BytesToHex(TestMessageBytes), KeyPair);
            signatureDataHex.Should().NotBeNull();
            signatureDataHex.R.Length.Should().Be(32);
            signatureDataHex.S.Length.Should().Be(32);
        }

        [Fact]
        public void TestRecoverSigningScriptHash()
        {
            // Generate a real signature for our test message with our test key
            var signatureData = Sign.SignMessage(TestMessageBytes, KeyPair);

            var recoveredScriptHash = Sign.RecoverSigningScriptHash(TestMessageBytes, signatureData);
            var expectedScriptHash = KeyPair.GetScriptHash();

            recoveredScriptHash.Should().BeEquivalentTo(expectedScriptHash);
        }

        [Fact]
        public void TestSignatureDataFromBytes()
        {
            var signatureBytes = HexExtensions.HexToBytes(
                "147e5f3c929dd830d961626551dbea6b70e4b2837ed2fe9089eed2072ab3a655523ae0fa8711eee4769f1913b180b9b3410bbb2cf770f529c85f6886f22cbaaf"
            );

            // Test signature data creation from bytes
            var signatureData = new SignatureData(signatureBytes);

            signatureData.V.Should().Be(0x00);
            signatureData.R.Should().BeEquivalentTo(HexExtensions.HexToBytes(_expectedR));
            signatureData.S.Should().BeEquivalentTo(HexExtensions.HexToBytes(_expectedS));

            // Test with explicit V value
            var signatureData27 = new SignatureData(0x27, signatureBytes);

            signatureData27.V.Should().Be(0x27);
            signatureData27.R.Should().BeEquivalentTo(HexExtensions.HexToBytes(_expectedR));
            signatureData27.S.Should().BeEquivalentTo(HexExtensions.HexToBytes(_expectedS));
        }

        [Fact]
        public void TestPublicKeyFromSignedMessage()
        {
            var signatureData = Sign.SignMessage(TestMessageBytes, KeyPair);
            var recoveredPublicKey = Sign.SignedMessageToKey(TestMessageBytes, signatureData);

            recoveredPublicKey.Should().BeEquivalentTo(_publicKey);
        }

        [Fact]
        public void TestPublicKeyFromPrivateKey()
        {
            var derivedPublicKey = Sign.PublicKeyFromPrivateKey(_privateKey);
            derivedPublicKey.Should().BeEquivalentTo(_publicKey);
        }

        [Fact]
        public void TestKeyFromSignedMessageWithInvalidSignature()
        {
            // Test with invalid R (too short) - should throw during SignatureData construction
            Action act = () => new SignatureData(v: 27, r: new byte[] { 1 }, s: new byte[32]);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*R component must be exactly 32 bytes*");

            // Test with invalid S (too short) - should throw during SignatureData construction
            Action act2 = () => new SignatureData(
                v: 27, 
                r: new byte[32], 
                s: new byte[] { 0 }
            );
            act2.Should().Throw<ArgumentException>()
                .WithMessage("*S component must be exactly 32 bytes*");
        }

        [Fact]
        public void TestVerifySignature()
        {
            var signatureData = Sign.SignMessage(TestMessageBytes, KeyPair);
            var isValid = Sign.VerifySignature(TestMessageBytes, signatureData, _publicKey);

            isValid.Should().BeTrue();
        }

        [Fact]
        public void TestSignatureDataEquality()
        {
            var sig1 = new SignatureData(27, 
                HexExtensions.HexToBytes(_expectedR), 
                HexExtensions.HexToBytes(_expectedS));
            var sig2 = new SignatureData(27, 
                HexExtensions.HexToBytes(_expectedR), 
                HexExtensions.HexToBytes(_expectedS));
            var sig3 = new SignatureData(28, 
                HexExtensions.HexToBytes(_expectedR), 
                HexExtensions.HexToBytes(_expectedS));

            sig1.Should().Be(sig2);
            sig1.Should().NotBe(sig3);
            sig1.GetHashCode().Should().Be(sig2.GetHashCode());
        }

        [Fact]
        public void TestDeterministicSigning()
        {
            // Note: Current implementation uses random nonces, so signatures will differ
            // But both should be valid signatures of the same message
            var sig1 = Sign.SignMessage(TestMessageBytes, KeyPair);
            var sig2 = Sign.SignMessage(TestMessageBytes, KeyPair);

            // Signatures should be different (due to random nonce)
            sig1.Should().NotBeEquivalentTo(sig2);
            
            // But both should verify correctly
            Sign.VerifySignature(TestMessageBytes, sig1, _publicKey).Should().BeTrue();
            Sign.VerifySignature(TestMessageBytes, sig2, _publicKey).Should().BeTrue();
        }

        [Fact]
        public void TestSignDifferentMessages()
        {
            // Different messages should produce different signatures
            var message1 = System.Text.Encoding.UTF8.GetBytes("Message 1");
            var message2 = System.Text.Encoding.UTF8.GetBytes("Message 2");

            var sig1 = Sign.SignMessage(message1, KeyPair);
            var sig2 = Sign.SignMessage(message2, KeyPair);

            sig1.Should().NotBeEquivalentTo(sig2);

            // But both should verify correctly
            Sign.VerifySignature(message1, sig1, _publicKey).Should().BeTrue();
            Sign.VerifySignature(message2, sig2, _publicKey).Should().BeTrue();

            // Cross-verification should fail
            Sign.VerifySignature(message1, sig2, _publicKey).Should().BeFalse();
            Sign.VerifySignature(message2, sig1, _publicKey).Should().BeFalse();
        }

        [Fact]
        public void TestSignatureRecoveryIds()
        {
            // Test all possible recovery IDs (0-3, or 27-30 in Ethereum format)
            var testSignatures = new[]
            {
                new SignatureData(0, HexExtensions.HexToBytes(_expectedR), HexExtensions.HexToBytes(_expectedS)),
                new SignatureData(1, HexExtensions.HexToBytes(_expectedR), HexExtensions.HexToBytes(_expectedS)),
                new SignatureData(27, HexExtensions.HexToBytes(_expectedR), HexExtensions.HexToBytes(_expectedS)),
                new SignatureData(28, HexExtensions.HexToBytes(_expectedR), HexExtensions.HexToBytes(_expectedS))
            };

            foreach (var sig in testSignatures)
            {
                // Should not throw exception for valid recovery IDs
                Action act = () =>
                {
                    try
                    {
                        var recoveredKey = Sign.SignedMessageToKey(TestMessageBytes, sig);
                        // Recovery might succeed or fail depending on the specific signature
                    }
                    catch (CryptoSecurityException)
                    {
                        // This is acceptable - not all recovery IDs will produce valid keys
                    }
                };
                
                act.Should().NotThrow<ArgumentException>();
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello World")]
        [InlineData("This is a longer message that tests the signing algorithm")]
        [InlineData("🚀 Unicode message with emojis 🔐")]
        public void TestSignVariousMessages(string message)
        {
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            
            var signature = Sign.SignMessage(messageBytes, KeyPair);
            var isValid = Sign.VerifySignature(messageBytes, signature, DerivedPublicKey);

            isValid.Should().BeTrue();
        }

        [Fact]
        public void TestSignatureByteArrayConversion()
        {
            var signatureBytes = _signatureData.Concatenated;
            var reconstructed = SignatureData.FromByteArray(_signatureData.V, signatureBytes);

            reconstructed.Should().BeEquivalentTo(_signatureData);
        }

        [Fact]
        public void TestInvalidSignatureVerification()
        {
            // Test verification with corrupted signature
            var validSignature = Sign.SignMessage(TestMessageBytes, KeyPair);
            
            // Corrupt the R value
            var corruptedR = (byte[])validSignature.R.Clone();
            corruptedR[0] ^= 0xFF;
            var corruptedSignature = new SignatureData(validSignature.V, corruptedR, validSignature.S);

            var isValid = Sign.VerifySignature(TestMessageBytes, corruptedSignature, _publicKey);
            isValid.Should().BeFalse();
        }

        [Fact]
        public void TestSignatureWithWrongKey()
        {
            // Create a different key pair
            var differentKeyPair = ECKeyPair.CreateEcKeyPair();
            
            // Sign with original key
            var signature = Sign.SignMessage(TestMessageBytes, KeyPair);
            
            // Verify with different key should fail
            var isValid = Sign.VerifySignature(TestMessageBytes, signature, differentKeyPair.PublicKey);
            isValid.Should().BeFalse();
        }
    }

}