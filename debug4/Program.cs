using System;
using NeoSharp.Crypto;
using NeoSharp.Utils;

// Test the WIF from the failing test
var wif = "L1eV34wPoj9weqhGijdDLtVQzUpWGHszXXpdU9dPuh2nRFFzFa7E";

// Import using regular ECKeyPair
var keyPair = ECKeyPair.FromWIF(wif);
var privateKey = keyPair.PrivateKey;
var regularAddress = keyPair.GetAddress();

// Import using SecureECKeyPair
using var secureKeyPair = SecureECKeyPair.Create(privateKey.PrivateKeyBytes);
var secureAddress = secureKeyPair.GetAddress();

Console.WriteLine($"WIF: {wif}");
Console.WriteLine($"Regular ECKeyPair address: {regularAddress}");
Console.WriteLine($"SecureECKeyPair address:   {secureAddress}");
Console.WriteLine($"Expected address:          2do19BsM6TqFFo8j36UqQxZj15AkyDswwsDq");
Console.WriteLine($"Actual address:            2ddPMF3ajyp94qQ8NuaVvSkQVWuyJsd6ZvMu");
Console.WriteLine($"Addresses match: {regularAddress == secureAddress}");