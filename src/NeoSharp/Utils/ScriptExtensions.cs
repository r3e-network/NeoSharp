using System;
using NeoSharp.Crypto;
using NeoSharp.Types;

namespace NeoSharp.Utils
{
    /// <summary>
    /// Extension methods for script operations.
    /// </summary>
    public static class ScriptExtensions
    {
        /// <summary>
        /// Converts a script to its script hash.
        /// </summary>
        /// <param name="script">The script bytes.</param>
        /// <returns>The script hash.</returns>
        public static Hash160 ToScriptHash(this byte[] script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));
            
            // NEO script hash = RIPEMD160(SHA256(script))
            return new Hash160(Hash.Hash160(script));
        }

        /// <summary>
        /// Checks if a script is a standard verification script.
        /// </summary>
        /// <param name="script">The script bytes.</param>
        /// <returns>True if standard, false otherwise.</returns>
        public static bool IsStandardContract(this byte[] script)
        {
            if (script == null || script.Length < 2)
                return false;
            
            // Check for single-signature pattern
            if (script.Length == 40)
            {
                // PUSH21 (0x21) + 33 bytes pubkey + PUSH1 (0x51) + PUSH1 (0x51) + SYSCALL (0x68) + 4 bytes
                if (script[0] == 0x21 && script[34] == 0x51 && script[35] == 0x51 && script[36] == 0x68)
                    return true;
            }
            
            // Check multi-signature pattern: PUSHINT threshold, PUSHDATA pubkeys..., PUSHINT pubkey_count, SYSCALL CheckMultisig
            if (script.Length >= 7)
            {
                int pos = 0;
                
                // Check if starts with threshold (PUSHINT1-PUSHINT16)
                if (script[pos] >= 0x51 && script[pos] <= 0x60)
                {
                    pos++;
                    var pubKeyCount = 0;
                    
                    // Count public keys
                    while (pos < script.Length - 2)
                    {
                        if (script[pos] == 0x21 && pos + 33 < script.Length) // PUSHDATA1 33 bytes
                        {
                            pos += 34; // Skip opcode + length + 33 bytes
                            pubKeyCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    // Check if ends with PUSHINT pubkey_count + SYSCALL CheckMultisig
                    if (pos + 2 <= script.Length &&
                        script[pos] >= 0x51 && script[pos] <= 0x60 && // PUSHINT for count
                        script[pos + 1] == 0x68) // SYSCALL CheckMultisig
                    {
                        return pubKeyCount > 0;
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Gets the public keys from a multi-signature redeem script.
        /// </summary>
        /// <param name="script">The script bytes.</param>
        /// <returns>The public keys, or null if not a valid multi-sig script.</returns>
        public static ECPoint[]? GetPublicKeysFromScript(this byte[] script)
        {
            if (script == null || script.Length < 42)
                return null;
            
            try
            {
                var publicKeys = new List<ECPoint>();
                int pos = 0;
                
                // Skip threshold (PUSHINT1-PUSHINT16)
                if (pos < script.Length && script[pos] >= 0x51 && script[pos] <= 0x60)
                {
                    pos++;
                    
                    // Parse public keys
                    while (pos < script.Length - 2)
                    {
                        if (script[pos] == 0x21 && pos + 33 < script.Length) // PUSHDATA1 33 bytes
                        {
                            pos++; // Skip PUSHDATA opcode
                            var pubKeyBytes = new byte[33];
                            Array.Copy(script, pos, pubKeyBytes, 0, 33);
                            pos += 33;
                            
                            publicKeys.Add(new ECPoint(pubKeyBytes));
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    return publicKeys.Count > 0 ? publicKeys.ToArray() : null;
                }
            }
            catch
            {
                // Return null on any parsing error
            }
            
            return null;
        }

        /// <summary>
        /// Gets the signature threshold from a multi-signature redeem script.
        /// </summary>
        /// <param name="script">The script bytes.</param>
        /// <returns>The threshold, or 0 if not a valid multi-sig script.</returns>
        public static int GetSignatureThresholdFromScript(this byte[] script)
        {
            if (script == null || script.Length < 42)
                return 0;
            
            if (script == null || script.Length < 3)
                return 0;
                
            try
            {
                // First byte should be threshold (PUSHINT1-PUSHINT16)
                if (script[0] >= 0x51 && script[0] <= 0x60)
                {
                    return script[0] - 0x50; // Convert PUSHINT opcode to number
                }
            }
            catch
            {
                // Return 0 on any parsing error
            }
            
            return 0;
        }
    }
}