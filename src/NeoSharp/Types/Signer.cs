using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoSharp.Crypto;
using NeoSharp.Serialization;

namespace NeoSharp.Types
{
    /// <summary>
    /// Represents a transaction signer
    /// </summary>
    public class Signer : INeoSerializable
    {
        /// <summary>
        /// Initializes a new instance of the Signer class
        /// </summary>
        public Signer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Signer class
        /// </summary>
        /// <param name="account">The signer account hash</param>
        /// <param name="scopes">The witness scopes</param>
        public Signer(Hash160 account, WitnessScope scopes = WitnessScope.CalledByEntry)
        {
            Account = account;
            Scopes = scopes;
        }

        /// <summary>
        /// Initializes a new instance of the Signer class with allowed contracts
        /// </summary>
        /// <param name="account">The signer account hash</param>
        /// <param name="scopes">The witness scopes</param>
        /// <param name="allowedContracts">The allowed contracts</param>
        public Signer(Hash160 account, WitnessScope scopes, Hash160[] allowedContracts)
        {
            Account = account;
            Scopes = scopes;
            AllowedContracts = allowedContracts;
        }

        /// <summary>
        /// Initializes a new instance of the Signer class with allowed contracts and groups
        /// </summary>
        /// <param name="account">The signer account hash</param>
        /// <param name="scopes">The witness scopes</param>
        /// <param name="allowedContracts">The allowed contracts</param>
        /// <param name="allowedGroups">The allowed groups</param>
        public Signer(Hash160 account, WitnessScope scopes, Hash160[] allowedContracts, ECPoint[] allowedGroups)
        {
            Account = account;
            Scopes = scopes;
            AllowedContracts = allowedContracts;
            AllowedGroups = allowedGroups?.Select(g => g.EncodedBytes).ToArray();
        }

        /// <summary>
        /// Gets or sets the signer account
        /// </summary>
        public Hash160 Account { get; set; } = Hash160.Zero;

        /// <summary>
        /// Gets or sets the witness scopes
        /// </summary>
        public WitnessScope Scopes { get; set; }

        /// <summary>
        /// Gets or sets the allowed contracts
        /// </summary>
        public Hash160[]? AllowedContracts { get; set; }

        /// <summary>
        /// Gets or sets the allowed groups
        /// </summary>
        public byte[][]? AllowedGroups { get; set; }

        /// <summary>
        /// Gets the size of the signer.
        /// </summary>
        public int Size
        {
            get
            {
                var size = 20 + 1; // Account hash + scopes
                if ((Scopes & WitnessScope.CustomContracts) != 0)
                {
                    size += Serialization.BinaryWriterExtensions.GetVarSize(AllowedContracts?.Length ?? 0);
                    size += (AllowedContracts?.Length ?? 0) * 20;
                }
                if ((Scopes & WitnessScope.CustomGroups) != 0)
                {
                    size += Serialization.BinaryWriterExtensions.GetVarSize(AllowedGroups?.Length ?? 0);
                    size += AllowedGroups?.Sum(g => g.Length) ?? 0;
                }
                return size;
            }
        }

        public void Serialize(Serialization.BinaryWriter writer)
        {
            writer.Write(Account.ToArray());
            writer.WriteByte((byte)Scopes);
            
            if ((Scopes & WitnessScope.CustomContracts) != 0)
            {
                writer.WriteVarInt(AllowedContracts?.Length ?? 0);
                if (AllowedContracts != null)
                {
                    foreach (var contract in AllowedContracts)
                    {
                        writer.Write(contract.ToArray());
                    }
                }
            }
            
            if ((Scopes & WitnessScope.CustomGroups) != 0)
            {
                writer.WriteVarInt(AllowedGroups?.Length ?? 0);
                if (AllowedGroups != null)
                {
                    foreach (var group in AllowedGroups)
                    {
                        writer.Write(group);
                    }
                }
            }
        }

        public void Deserialize(Serialization.BinaryReader reader)
        {
            Account = new Hash160(reader.ReadBytes(20));
            Scopes = (WitnessScope)reader.ReadByte();
            
            if ((Scopes & WitnessScope.CustomContracts) != 0)
            {
                var count = (int)reader.ReadVarInt();
                AllowedContracts = new Hash160[count];
                for (int i = 0; i < count; i++)
                {
                    AllowedContracts[i] = new Hash160(reader.ReadBytes(20));
                }
            }
            
            if ((Scopes & WitnessScope.CustomGroups) != 0)
            {
                var count = (int)reader.ReadVarInt();
                AllowedGroups = new byte[count][];
                for (int i = 0; i < count; i++)
                {
                    AllowedGroups[i] = reader.ReadBytes(33); // ECPoint size
                }
            }
        }

        /// <summary>
        /// Converts the signer to JSON format for RPC calls.
        /// </summary>
        /// <returns>A dictionary representing the JSON format.</returns>
        public Dictionary<string, object> ToJson()
        {
            var json = new Dictionary<string, object>
            {
                ["account"] = Account.ToString(),
                ["scopes"] = Scopes.ToString()
            };

            if (AllowedContracts != null && AllowedContracts.Length > 0)
            {
                json["allowedcontracts"] = AllowedContracts.Select(c => c.ToString()).ToList();
            }

            if (AllowedGroups != null && AllowedGroups.Length > 0)
            {
                json["allowedgroups"] = AllowedGroups.Select(g => Convert.ToBase64String(g)).ToList();
            }

            return json;
        }
    }

    /// <summary>
    /// Witness scope enumeration
    /// </summary>
    [Flags]
    public enum WitnessScope : byte
    {
        None = 0,
        CalledByEntry = 0x01,
        CustomContracts = 0x10,
        CustomGroups = 0x20,
        Global = 0x80
    }
}