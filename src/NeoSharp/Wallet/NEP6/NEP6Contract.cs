using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Crypto;
using NeoSharp.Types;
using NeoSharp.Utils;

namespace NeoSharp.Wallet.NEP6
{
    /// <summary>
    /// Represents a NEP-6 standard contract.
    /// </summary>
    public class NEP6Contract
    {
        /// <summary>
        /// Gets or sets the contract script.
        /// </summary>
        [JsonPropertyName("script")]
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        [JsonPropertyName("parameters")]
        public List<NEP6Parameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets whether the contract is deployed.
        /// </summary>
        [JsonPropertyName("deployed")]
        public bool Deployed { get; set; }

        /// <summary>
        /// Initializes a new instance of the NEP6Contract class.
        /// </summary>
        public NEP6Contract()
        {
            Script = string.Empty;
            Parameters = new List<NEP6Parameter>();
        }

        /// <summary>
        /// Initializes a new instance of the NEP6Contract class.
        /// </summary>
        /// <param name="script">The contract script.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="deployed">Whether the contract is deployed.</param>
        public NEP6Contract(string script, List<NEP6Parameter> parameters, bool deployed = false)
        {
            Script = script;
            Parameters = parameters ?? new List<NEP6Parameter>();
            Deployed = deployed;
        }

        /// <summary>
        /// Creates a single-signature contract.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns>The NEP6 contract.</returns>
        public static NEP6Contract CreateSingleSigContract(ECPoint publicKey)
        {
            // Create verification script for single signature
            // Script: PUSH21 <publicKey> PUSH1 PUSH1 SYSCALL System.Crypto.CheckSig
            var scriptBuilder = new Script.ScriptBuilder();
            scriptBuilder.EmitPush(publicKey.GetEncoded(true));
            scriptBuilder.EmitPush(1);
            scriptBuilder.EmitPush(1);
            scriptBuilder.EmitSysCall("System.Crypto.CheckSig");
            
            var script = scriptBuilder.ToArray();
            var scriptHex = script.ToHexString();
            
            var parameters = new List<NEP6Parameter>
            {
                new NEP6Parameter("signature")
            };
            
            return new NEP6Contract(scriptHex, parameters);
        }

        /// <summary>
        /// Creates a multi-signature contract.
        /// </summary>
        /// <param name="m">The minimum number of signatures required.</param>
        /// <param name="publicKeys">The public keys.</param>
        /// <returns>The NEP6 contract.</returns>
        public static NEP6Contract CreateMultiSigContract(int m, List<ECPoint> publicKeys)
        {
            if (m <= 0 || m > publicKeys.Count)
                throw new ArgumentException("Invalid signature threshold");
            
            if (publicKeys.Count > 1024)
                throw new ArgumentException("Too many public keys");
            
            // Sort public keys
            var sortedKeys = new List<ECPoint>(publicKeys);
            sortedKeys.Sort((a, b) => a.GetEncoded(true).AsSpan().SequenceCompareTo(b.GetEncoded(true)));
            
            // Create verification script for multi-signature
            var scriptBuilder = new Script.ScriptBuilder();
            scriptBuilder.EmitPush(m);
            
            foreach (var publicKey in sortedKeys)
            {
                scriptBuilder.EmitPush(publicKey.GetEncoded(true));
            }
            
            scriptBuilder.EmitPush(publicKeys.Count);
            scriptBuilder.EmitPush(publicKeys.Count);
            scriptBuilder.EmitSysCall("System.Crypto.CheckMultisig");
            
            var script = scriptBuilder.ToArray();
            var scriptHex = script.ToHexString();
            
            var parameters = new List<NEP6Parameter>();
            for (int i = 0; i < m; i++)
            {
                parameters.Add(new NEP6Parameter("signature"));
            }
            
            return new NEP6Contract(scriptHex, parameters);
        }

        /// <summary>
        /// Creates a copy of this contract.
        /// </summary>
        /// <returns>A copy of the contract.</returns>
        public NEP6Contract Copy()
        {
            var parametersCopy = new List<NEP6Parameter>();
            foreach (var param in Parameters)
            {
                parametersCopy.Add(param.Copy());
            }
            
            return new NEP6Contract
            {
                Script = Script,
                Parameters = parametersCopy,
                Deployed = Deployed
            };
        }

        /// <summary>
        /// Gets the script hash of this contract.
        /// </summary>
        /// <returns>The script hash.</returns>
        public Hash160 GetScriptHash()
        {
            if (string.IsNullOrEmpty(Script))
                throw new InvalidOperationException("Script is empty");
            
            var scriptBytes = Script.HexToBytes();
            return new Hash160(Hash.Hash160(scriptBytes));
        }

        /// <summary>
        /// Gets the address of this contract.
        /// </summary>
        /// <returns>The contract address.</returns>
        public string GetAddress()
        {
            var scriptHash = GetScriptHash();
            return scriptHash.ToAddress();
        }

        /// <summary>
        /// Validates the contract data.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Script))
                return false;
            
            try
            {
                // Try to parse the script as hex
                var scriptBytes = Script.HexToBytes();
                if (scriptBytes.Length == 0)
                    return false;
                
                // Validate parameters
                foreach (var param in Parameters)
                {
                    if (!param.IsValid())
                        return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if this is a standard contract.
        /// </summary>
        /// <returns>True if standard, false otherwise.</returns>
        public bool IsStandard()
        {
            if (Parameters.Count == 0)
                return false;
            
            // Check for single-signature pattern
            if (Parameters.Count == 1 && Parameters[0].Name == "signature")
                return true;
            
            // Check for multi-signature pattern
            if (Parameters.All(p => p.Name == "signature"))
                return true;
            
            return false;
        }

        /// <summary>
        /// Gets the required signature count for this contract.
        /// </summary>
        /// <returns>The number of signatures required.</returns>
        public int GetRequiredSignatureCount()
        {
            return Parameters.Count(p => p.Name == "signature");
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not NEP6Contract other)
                return false;
            
            return Script == other.Script && Deployed == other.Deployed;
        }

        /// <summary>
        /// Gets the hash code for this contract.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Script, Deployed);
        }
    }

    /// <summary>
    /// Represents a parameter in a NEP-6 contract.
    /// </summary>
    public class NEP6Parameter
    {
        /// <summary>
        /// Gets or sets the parameter name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameter type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the NEP6Parameter class.
        /// </summary>
        public NEP6Parameter()
        {
            Name = string.Empty;
            Type = "signature";
        }

        /// <summary>
        /// Initializes a new instance of the NEP6Parameter class.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="type">The parameter type.</param>
        public NEP6Parameter(string name, string type = "signature")
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Creates a copy of this parameter.
        /// </summary>
        /// <returns>A copy of the parameter.</returns>
        public NEP6Parameter Copy()
        {
            return new NEP6Parameter(Name, Type);
        }

        /// <summary>
        /// Validates the parameter data.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Type);
        }
    }
}