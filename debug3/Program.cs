using System;
using System.Linq;
using NeoSharp.Crypto;
using NeoSharp.Utils;
using NeoSharp.Tests.Helpers;

// Debug signature verification
var keyPair = ECKeyPair.CreateEcKeyPair();
var message = "Hello, Neo!"u8.ToArray();
var messageHash = NeoSharp.Crypto.HashExtensions.Hash256(message);
var signature = keyPair.Sign(messageHash);

Console.WriteLine($"Message: {TestConstants.BytesToHex(message)}");
Console.WriteLine($"Message Hash: {TestConstants.BytesToHex(messageHash)}");
Console.WriteLine($"Signature: {TestConstants.BytesToHex(signature)}");
Console.WriteLine($"Signature Length: {signature.Length}");

// Try to create SignatureData and verify
try 
{
    var signatureData = new SignatureData(signature);
    Console.WriteLine($"SignatureData V: {signatureData.V}");
    Console.WriteLine($"SignatureData R: {TestConstants.BytesToHex(signatureData.R)}");
    Console.WriteLine($"SignatureData S: {TestConstants.BytesToHex(signatureData.S)}");
    
    // Debug signature reconstruction
    var r = new System.Numerics.BigInteger(signatureData.R, isUnsigned: true, isBigEndian: true);
    var s = new System.Numerics.BigInteger(signatureData.S, isUnsigned: true, isBigEndian: true);
    var ecdsaSignature = new ECDSASignature(r, s);
    var reconstructedSignature = ecdsaSignature.ToBytes();
    
    Console.WriteLine($"Original signature:      {TestConstants.BytesToHex(signature)}");
    Console.WriteLine($"Reconstructed signature: {TestConstants.BytesToHex(reconstructedSignature)}");
    Console.WriteLine($"Signatures equal: {signature.SequenceEqual(reconstructedSignature)}");
    
    // Test verification with original message
    var isValidMessage = Sign.VerifySignature(message, signatureData, keyPair.PublicKey);
    Console.WriteLine($"Verify with original message: {isValidMessage}");
    
    // Test verification with message hash (this should fail)
    var isValidHash = Sign.VerifySignature(messageHash, signatureData, keyPair.PublicKey);
    Console.WriteLine($"Verify with message hash: {isValidHash}");
    
    // Test direct ECPublicKey verification
    var isValidDirect = keyPair.PublicKey.Verify(messageHash, signature);
    Console.WriteLine($"Direct ECPublicKey.Verify: {isValidDirect}");
    
    // Test ECPublicKey verification with reconstructed signature
    var isValidReconstructed = keyPair.PublicKey.Verify(messageHash, reconstructedSignature);
    Console.WriteLine($"ECPublicKey.Verify with reconstructed: {isValidReconstructed}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}