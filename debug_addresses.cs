using System;
using NeoSharp.Crypto;
using NeoSharp.Utils;
using NeoSharp.Tests.Helpers;

// Test address generation to see what we get
var privateKeyHex = "e6e919577dd7b8e97805151c05ae07ff4f752654d6d8797597aca989c02c4cb3";
var privateKey = TestConstants.HexToBytes(privateKeyHex);

var keyPair = new ECKeyPair(privateKey);
var address = keyPair.GetAddress();
var scriptHash = keyPair.GetScriptHash();

Console.WriteLine($"Private Key: {privateKeyHex}");
Console.WriteLine($"Public Key:  {TestConstants.BytesToHex(keyPair.PublicKey.EncodedBytes)}");
Console.WriteLine($"WIF:         {keyPair.ToWIF()}");
Console.WriteLine($"Address:     {address}");
Console.WriteLine($"Script Hash: {scriptHash}");
Console.WriteLine($"Expected Address: {TestConstants.DefaultAccountAddress}");
Console.WriteLine($"Expected Script:  {TestConstants.DefaultAccountScriptHash}");

// Check WIF parsing
try {
    var fromWIF = ECKeyPair.FromWIF("L4xa4S78qj87q9FRkMQDeZsrymQG6ThR5oczagNNNnBrWRjicF36");
    var wifAddress = fromWIF.GetAddress();
    Console.WriteLine($"Address from WIF: {wifAddress}");
} catch (Exception e) {
    Console.WriteLine($"WIF parsing error: {e.Message}");
}
