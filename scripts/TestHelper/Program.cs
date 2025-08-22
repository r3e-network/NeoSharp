using System;
using System.Numerics;

Console.WriteLine("BigInteger serialization test:");
Console.WriteLine($"BigInteger.Zero.ToByteArray(): [{string.Join(", ", BigInteger.Zero.ToByteArray().Select(b => "0x" + b.ToString("X2")))}]");
Console.WriteLine($"BigInteger(255).ToByteArray(): [{string.Join(", ", new BigInteger(255).ToByteArray().Select(b => "0x" + b.ToString("X2")))}]");
Console.WriteLine($"BigInteger(-1).ToByteArray(): [{string.Join(", ", new BigInteger(-1).ToByteArray().Select(b => "0x" + b.ToString("X2")))}]");

// Check length
Console.WriteLine($"BigInteger.Zero length: {BigInteger.Zero.ToByteArray().Length}");
Console.WriteLine($"BigInteger(255) length: {new BigInteger(255).ToByteArray().Length}");
Console.WriteLine($"BigInteger(-1) length: {new BigInteger(-1).ToByteArray().Length}");
