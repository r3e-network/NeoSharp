using System;
using NeoSharp.Crypto;
using NeoSharp.Utils;

// Test WIF to address conversion
var wif = "L1eV34wPoj9weqhGijdDLtVQzUpWGHszXXpdU9dPuh2nRFFzFa7E";
var keyPair = ECKeyPair.FromWIF(wif);
var address = keyPair.GetAddress();

Console.WriteLine($"WIF:     {wif}");
Console.WriteLine($"Address: {address}");
Console.WriteLine($"Expected: NM7Aky765FG8NhhwtxjXRx7jEL1cnw7PBP");