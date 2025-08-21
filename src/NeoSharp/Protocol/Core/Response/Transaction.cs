using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a transaction in the Neo blockchain.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Gets or sets the transaction hash.
        /// </summary>
        [JsonPropertyName("hash")]
        public Hash256 Hash { get; set; }

        /// <summary>
        /// Gets or sets the transaction size in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the transaction version.
        /// </summary>
        [JsonPropertyName("version")]
        public byte Version { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        [JsonPropertyName("nonce")]
        public uint Nonce { get; set; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        [JsonPropertyName("sender")]
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the system fee.
        /// </summary>
        [JsonPropertyName("sysfee")]
        public string SystemFee { get; set; }

        /// <summary>
        /// Gets or sets the network fee.
        /// </summary>
        [JsonPropertyName("netfee")]
        public string NetworkFee { get; set; }

        /// <summary>
        /// Gets or sets the valid until block.
        /// </summary>
        [JsonPropertyName("validuntilblock")]
        public long ValidUntilBlock { get; set; }

        /// <summary>
        /// Gets or sets the signers.
        /// </summary>
        [JsonPropertyName("signers")]
        public List<TransactionSigner> Signers { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        [JsonPropertyName("attributes")]
        public List<TransactionAttribute> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        [JsonPropertyName("script")]
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets the witnesses.
        /// </summary>
        [JsonPropertyName("witnesses")]
        public List<NeoWitness> Witnesses { get; set; }

        /// <summary>
        /// Gets or sets the block hash where this transaction was included.
        /// </summary>
        [JsonPropertyName("blockhash")]
        public Hash256 BlockHash { get; set; }

        /// <summary>
        /// Gets or sets the confirmations.
        /// </summary>
        [JsonPropertyName("confirmations")]
        public int Confirmations { get; set; }

        /// <summary>
        /// Gets or sets the block time.
        /// </summary>
        [JsonPropertyName("blocktime")]
        public long BlockTime { get; set; }

        /// <summary>
        /// Gets or sets the VM state.
        /// </summary>
        [JsonPropertyName("vmstate")]
        public string VmState { get; set; }
    }
}