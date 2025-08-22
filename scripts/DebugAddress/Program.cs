using System;
using System.Linq;
using NeoSharp.Types;
using NeoSharp.Utils;

Console.WriteLine("Debug Address Generation");

// Test constants from TestConstants
var scriptHashString = "90c2be8e9964c912073013a496a5055d8796487f";
var expectedAddress = "2e1hLQVqdSvizuTzKxXTraC9FzijLQuhavAT";

Console.WriteLine($"Script Hash: {scriptHashString}");
Console.WriteLine($"Expected Address: {expectedAddress}");

// Parse the hash
var hash = Hash160.Parse(scriptHashString);
Console.WriteLine($"Parsed Hash bytes: {string.Join(" ", hash.ToArray().Select(b => b.ToString("x2")))}");

// Convert to address
var actualAddress = hash.ToAddress();
Console.WriteLine($"Actual Address: {actualAddress}");

// Check if they match
Console.WriteLine($"Match: {actualAddress == expectedAddress}");

// Let's also try converting the expected address back to script hash
try 
{
    Console.WriteLine($"Expected Address length: {expectedAddress.Length}");
    var decodedBytes = NeoSharp.Crypto.Base58.Decode(expectedAddress);
    Console.WriteLine($"Expected Address decoded bytes length: {decodedBytes.Length}");
    Console.WriteLine($"Expected Address decoded bytes: {string.Join(" ", decodedBytes.Select(b => b.ToString("x2")))}");
    
    var backToScriptHash = expectedAddress.AddressToScriptHash();
    Console.WriteLine($"Expected Address -> Script Hash: {string.Join("", backToScriptHash.Select(b => b.ToString("x2")))}");
    Console.WriteLine($"Script Hash matches: {string.Join("", backToScriptHash.Select(b => b.ToString("x2"))) == scriptHashString}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error converting address to script hash: {ex.Message}");
}

// Let's also check what the actual address decodes to
try 
{
    Console.WriteLine($"Actual Address length: {actualAddress.Length}");
    var actualDecodedBytes = NeoSharp.Crypto.Base58.Decode(actualAddress);
    Console.WriteLine($"Actual Address decoded bytes length: {actualDecodedBytes.Length}");
    Console.WriteLine($"Actual Address decoded bytes: {string.Join(" ", actualDecodedBytes.Select(b => b.ToString("x2")))}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error decoding actual address: {ex.Message}");
}