using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a block in the Neo blockchain.
    /// </summary>
    public class NeoBlock
    {
        /// <summary>
        /// Gets or sets the block hash.
        /// </summary>
        [JsonPropertyName("hash")]
        public Hash256 Hash { get; set; }

        /// <summary>
        /// Gets or sets the block size in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the block version.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the previous block hash.
        /// </summary>
        [JsonPropertyName("previousblockhash")]
        public Hash256 PreviousBlockHash { get; set; }

        /// <summary>
        /// Gets or sets the merkle root.
        /// </summary>
        [JsonPropertyName("merkleroot")]
        public Hash256 MerkleRoot { get; set; }

        /// <summary>
        /// Gets or sets the block time (Unix timestamp in seconds).
        /// </summary>
        [JsonPropertyName("time")]
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the block index.
        /// </summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the primary index.
        /// </summary>
        [JsonPropertyName("primary")]
        public byte Primary { get; set; }

        /// <summary>
        /// Gets or sets the next consensus address.
        /// </summary>
        [JsonPropertyName("nextconsensus")]
        public string NextConsensus { get; set; }

        /// <summary>
        /// Gets or sets the witnesses.
        /// </summary>
        [JsonPropertyName("witnesses")]
        public List<NeoWitness> Witnesses { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        [JsonPropertyName("tx")]
        public List<Transaction> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the confirmations.
        /// </summary>
        [JsonPropertyName("confirmations")]
        public int Confirmations { get; set; }

        /// <summary>
        /// Gets or sets the next block hash.
        /// </summary>
        [JsonPropertyName("nextblockhash")]
        public Hash256 NextBlockHash { get; set; }

        /// <summary>
        /// Gets the block time as DateTime.
        /// </summary>
        [JsonIgnore]
        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Time).DateTime;
    }
}