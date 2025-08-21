using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoSharp.Crypto;
using NeoSharp.Script;
using NeoSharp.Serialization;
using NeoSharp.Types;
using NeoSharp.Wallet;

namespace NeoSharp.Transaction
{
    /// <summary>
    /// Represents a Neo transaction.
    /// </summary>
    public class Transaction : Serialization.INeoSerializable
    {
        private byte _version;
        private uint _nonce;
        private long _systemFee;
        private long _networkFee;
        private uint _validUntilBlock;
        private List<Signer> _signers;
        private List<TransactionAttribute> _attributes;
        private byte[] _script;
        private List<Witness> _witnesses;

        private Hash256? _hash;

        /// <summary>
        /// Gets or sets the transaction version.
        /// </summary>
        public byte Version
        {
            get => _version;
            set
            {
                _version = value;
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        public uint Nonce
        {
            get => _nonce;
            set
            {
                _nonce = value;
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the system fee.
        /// </summary>
        public long SystemFee
        {
            get => _systemFee;
            set
            {
                _systemFee = value;
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the network fee.
        /// </summary>
        public long NetworkFee
        {
            get => _networkFee;
            set
            {
                _networkFee = value;
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the valid until block.
        /// </summary>
        public uint ValidUntilBlock
        {
            get => _validUntilBlock;
            set
            {
                _validUntilBlock = value;
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the signers.
        /// </summary>
        public List<Signer> Signers
        {
            get => _signers;
            set
            {
                _signers = value ?? new List<Signer>();
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public List<TransactionAttribute> Attributes
        {
            get => _attributes;
            set
            {
                _attributes = value ?? new List<TransactionAttribute>();
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        public byte[] Script
        {
            get => _script;
            set
            {
                _script = value ?? Array.Empty<byte>();
                _hash = null;
            }
        }

        /// <summary>
        /// Gets or sets the witnesses.
        /// </summary>
        public List<Witness> Witnesses
        {
            get => _witnesses;
            set => _witnesses = value ?? new List<Witness>();
        }

        /// <summary>
        /// Gets the transaction hash.
        /// </summary>
        public Hash256 Hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = new Hash256(CalculateHash());
                }
                return _hash.Value;
            }
        }

        /// <summary>
        /// Gets the size of the transaction.
        /// </summary>
        public int Size
        {
            get
            {
                var size = 1 + // Version
                          4 + // Nonce
                          8 + // SystemFee
                          8 + // NetworkFee
                          4 + // ValidUntilBlock
                          GetVarSize(_signers.Count);

                size += _signers.Sum(s => s.Size);
                size += GetVarSize(_attributes.Count);
                size += _attributes.Sum(a => a.Size);
                size += GetVarSize(_script.Length) + _script.Length;
                size += GetVarSize(_witnesses.Count);
                size += _witnesses.Sum(w => w.Size);

                return size;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Transaction class.
        /// </summary>
        public Transaction()
        {
            _version = 0;
            _signers = new List<Signer>();
            _attributes = new List<TransactionAttribute>();
            _script = Array.Empty<byte>();
            _witnesses = new List<Witness>();
        }

        /// <summary>
        /// Serializes the transaction without witnesses.
        /// </summary>
        /// <param name="writer">The writer to serialize to.</param>
        public void SerializeUnsigned(Serialization.BinaryWriter writer)
        {
            writer.WriteByte(Version);
            writer.WriteUInt32(Nonce);
            writer.WriteInt64(SystemFee);
            writer.WriteInt64(NetworkFee);
            writer.WriteUInt32(ValidUntilBlock);
            writer.WriteVarInt(Signers.Count);
            foreach (var signer in Signers)
            {
                signer.Serialize(writer);
            }
            writer.WriteVarInt(Attributes.Count);
            foreach (var attribute in Attributes)
            {
                attribute.Serialize(writer);
            }
            writer.WriteVarBytes(Script);
        }

        /// <summary>
        /// Serializes the transaction.
        /// </summary>
        /// <param name="writer">The writer to serialize to.</param>
        public void Serialize(Serialization.BinaryWriter writer)
        {
            SerializeUnsigned(writer);
            writer.WriteVarInt(Witnesses.Count);
            foreach (var witness in Witnesses)
            {
                witness.Serialize(writer);
            }
        }

        /// <summary>
        /// Deserializes the transaction.
        /// </summary>
        /// <param name="reader">The reader to deserialize from.</param>
        public void Deserialize(Serialization.BinaryReader reader)
        {
            DeserializeUnsigned(reader);
            var witnessCount = (int)reader.ReadVarInt();
            Witnesses = new List<Witness>(witnessCount);
            for (int i = 0; i < witnessCount; i++)
            {
                var witness = new Witness();
                witness.Deserialize(reader);
                Witnesses.Add(witness);
            }
        }

        /// <summary>
        /// Deserializes the transaction without witnesses.
        /// </summary>
        /// <param name="reader">The reader to deserialize from.</param>
        public void DeserializeUnsigned(Serialization.BinaryReader reader)
        {
            Version = reader.ReadByte();
            Nonce = reader.ReadUInt32();
            SystemFee = reader.ReadInt64();
            NetworkFee = reader.ReadInt64();
            ValidUntilBlock = reader.ReadUInt32();

            var signerCount = (int)reader.ReadVarInt();
            Signers = new List<Signer>(signerCount);
            for (int i = 0; i < signerCount; i++)
            {
                var signer = new Signer();
                signer.Deserialize(reader);
                Signers.Add(signer);
            }

            var attributeCount = (int)reader.ReadVarInt();
            Attributes = new List<TransactionAttribute>(attributeCount);
            for (int i = 0; i < attributeCount; i++)
            {
                var attribute = new TransactionAttribute();
                attribute.Deserialize(reader);
                Attributes.Add(attribute);
            }

            Script = reader.ReadVarBytes();
        }

        /// <summary>
        /// Gets the hash data for signing.
        /// </summary>
        /// <param name="magic">The network magic number.</param>
        /// <returns>The hash data.</returns>
        public byte[] GetHashData(uint magic)
        {
            using var ms = new MemoryStream();
            using var writer = new Serialization.BinaryWriter(ms);
            writer.WriteUInt32(magic);
            writer.Write(Hash.ToArray());
            return ms.ToArray();
        }

        /// <summary>
        /// Signs the transaction with the given account.
        /// </summary>
        /// <param name="account">The account to sign with.</param>
        /// <param name="magic">The network magic number.</param>
        public void Sign(Account account, uint magic = NeoConstants.MainNetMagic)
        {
            var hashData = GetHashData(magic);
            var signature = account.SignData(hashData);
            
            // Find or create witness for this account
            var scriptHash = account.GetScriptHash();
            var signerIndex = Signers.FindIndex(s => s.Account.Equals(scriptHash));
            
            if (signerIndex >= 0)
            {
                while (Witnesses.Count <= signerIndex)
                {
                    Witnesses.Add(new Witness());
                }

                var invocationScript = new ScriptBuilder()
                    .EmitPush(signature)
                    .ToArray();
                    
                var verificationScript = account.GetVerificationScript();
                
                Witnesses[signerIndex] = new Witness
                {
                    InvocationScript = invocationScript,
                    VerificationScript = verificationScript
                };
            }
        }

        private byte[] CalculateHash()
        {
            using var ms = new MemoryStream();
            using var writer = new Serialization.BinaryWriter(ms);
            SerializeUnsigned(writer);
            return NeoSharp.Crypto.Hash.Hash256(ms.ToArray());
        }

        private static int GetVarSize(int value)
        {
            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3;
            else
                return 5;
        }
    }

    /// <summary>
    /// Represents a transaction attribute.
    /// </summary>
    public class TransactionAttribute : INeoSerializable
    {
        /// <summary>
        /// Gets or sets the attribute type.
        /// </summary>
        public TransactionAttributeType Type { get; set; }

        /// <summary>
        /// Gets or sets the attribute data.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets the size of the attribute.
        /// </summary>
        public int Size => 1 + Data.Length;

        public void Serialize(Serialization.BinaryWriter writer)
        {
            writer.WriteByte((byte)Type);
            writer.Write(Data);
        }

        public void Deserialize(Serialization.BinaryReader reader)
        {
            Type = (TransactionAttributeType)reader.ReadByte();
            // Read data based on type
            switch (Type)
            {
                case TransactionAttributeType.HighPriority:
                    Data = Array.Empty<byte>();
                    break;
                case TransactionAttributeType.OracleResponse:
                    Data = reader.ReadBytes(reader.ReadByte() + 8); // id (8) + code (1) + result
                    break;
                default:
                    throw new FormatException($"Invalid transaction attribute type: {Type}");
            }
        }
    }

    /// <summary>
    /// Transaction attribute types.
    /// </summary>
    public enum TransactionAttributeType : byte
    {
        /// <summary>
        /// High priority attribute.
        /// </summary>
        HighPriority = 0x01,

        /// <summary>
        /// Oracle response attribute.
        /// </summary>
        OracleResponse = 0x11
    }
}