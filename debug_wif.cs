using System;
using System.Linq;
using NeoSharp.Crypto;
using NeoSharp.Utils;

class Program
{
    static void Main()
    {
        var wif = "L3RXk8pNNApAKQrJ5g8VovbfdftRMCi75oLRGDGAqM8C93B9xjaN";
        
        Console.WriteLine($"Testing WIF: {wif}");
        Console.WriteLine($"WIF Length: {wif.Length}");
        
        try
        {
            // Decode Base58
            var data = Base58.Decode(wif);
            Console.WriteLine($"Decoded data length: {data.Length}");
            Console.WriteLine($"Decoded data (hex): {Convert.ToHexString(data)}");
            
            if (data.Length == 37 || data.Length == 38)
            {
                Console.WriteLine($"Version byte: 0x{data[0]:X2}");
                
                if (data.Length == 38)
                {
                    Console.WriteLine($"Compression flag: 0x{data[33]:X2}");
                }
                
                // Extract components
                var toHash = data.Take(data.Length - 4).ToArray();
                var providedChecksum = data.Skip(data.Length - 4).ToArray();
                
                Console.WriteLine($"Data to hash (hex): {Convert.ToHexString(toHash)}");
                Console.WriteLine($"Provided checksum: {Convert.ToHexString(providedChecksum)}");
                
                // Calculate checksum using our Hash.DoubleSHA256
                var calculatedHash = Hash.DoubleSHA256(toHash);
                var calculatedChecksum = calculatedHash.Take(4).ToArray();
                
                Console.WriteLine($"Calculated hash: {Convert.ToHexString(calculatedHash)}");
                Console.WriteLine($"Calculated checksum: {Convert.ToHexString(calculatedChecksum)}");
                
                Console.WriteLine($"Checksums match: {calculatedChecksum.SequenceEqual(providedChecksum)}");
                
                // Also try manual double SHA256
                var hash1 = Hash.SHA256(toHash);
                var hash2 = Hash.SHA256(hash1);
                var manualChecksum = hash2.Take(4).ToArray();
                
                Console.WriteLine($"Manual double SHA256: {Convert.ToHexString(hash2)}");
                Console.WriteLine($"Manual checksum: {Convert.ToHexString(manualChecksum)}");
                Console.WriteLine($"Manual checksums match: {manualChecksum.SequenceEqual(providedChecksum)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}