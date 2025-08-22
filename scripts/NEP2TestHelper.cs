using System;
using NeoSharp.Crypto;
using NeoSharp.Wallet;
using NeoSharp.Wallet.NEP6;
using NeoSharp.Utils;

namespace TestHelper
{
    public class NEP2TestHelper
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Generating correct NEP-2 test values...");
            
            // Use our consistent test constants
            var privateKeyHex = "e6e919577dd7b8e97805151c05ae07ff4f752654d6d8797597aca989c02c4cb3";
            var privateKeyBytes = HexExtensions.HexToBytes(privateKeyHex);
            var keyPair = new ECKeyPair(privateKeyBytes);
            var password = "123";
            
            Console.WriteLine($"Private Key: {privateKeyHex}");
            Console.WriteLine($"Address: {keyPair.GetAddress()}");
            Console.WriteLine($"Password: {password}");
            
            // Generate NEP-2 with default parameters
            var defaultParams = ScryptParams.Default;
            var nep2Default = NEP2.Encrypt(keyPair, password, defaultParams);
            Console.WriteLine($"NEP-2 (default params): {nep2Default}");
            
            // Generate NEP-2 with custom parameters  
            var customParams = new ScryptParams { N = 256, R = 1, P = 1 };
            var nep2Custom = NEP2.Encrypt(keyPair, password, customParams);
            Console.WriteLine($"NEP-2 (custom params): {nep2Custom}");
            
            // Test decryption
            try
            {
                var decryptedDefault = NEP2.Decrypt(nep2Default, password, defaultParams);
                Console.WriteLine("Default decryption: SUCCESS");
                
                var decryptedCustom = NEP2.Decrypt(nep2Custom, password, customParams);
                Console.WriteLine("Custom decryption: SUCCESS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption error: {ex.Message}");
            }
        }
    }
}