using System;
using NeoSharp.Crypto;
using NeoSharp.Tests.Helpers;
using NeoSharp.Utils;

class Program 
{
    static void Main() 
    {
        var privateKeyBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountPrivateKey);
        var keyPair = new ECKeyPair(privateKeyBytes);
        
        Console.WriteLine($"Private key: {TestConstants.DefaultAccountPrivateKey}");
        Console.WriteLine($"Public key: {keyPair.PublicKey.GetEncoded().ToHexString()}");
        
        var scriptHash = keyPair.GetScriptHash();
        Console.WriteLine($"Script hash (big-endian): {scriptHash.ToHex()}");
        Console.WriteLine($"Script hash (little-endian): {scriptHash.ToLittleEndianArray().ToHexString()}");
        Console.WriteLine($"Expected script hash: {TestConstants.DefaultAccountScriptHash}");
        
        var address = keyPair.GetAddress();
        Console.WriteLine($"Generated address: {address}");
        Console.WriteLine($"Expected address: {TestConstants.DefaultAccountAddress}");
        
        // Also test the expected script hash directly
        var expectedBytes = TestConstants.HexToBytes(TestConstants.DefaultAccountScriptHash);
        var expectedAddress = expectedBytes.ScriptHashToAddress();
        Console.WriteLine($"Expected script hash -> address: {expectedAddress}");
    }
}
