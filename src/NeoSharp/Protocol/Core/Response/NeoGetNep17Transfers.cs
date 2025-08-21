using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getnep17transfers method.
    /// </summary>
    public class NeoGetNep17Transfers
    {
        /// <summary>
        /// Represents the NEP-17 transfers.
        /// </summary>
        public class Nep17Transfers
        {
            /// <summary>
            /// Gets or sets the sent transfers.
            /// </summary>
            [JsonPropertyName("sent")]
            public List<Nep17Transfer> Sent { get; set; }

            /// <summary>
            /// Gets or sets the received transfers.
            /// </summary>
            [JsonPropertyName("received")]
            public List<Nep17Transfer> Received { get; set; }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; }
        }

        /// <summary>
        /// Represents a NEP-17 transfer.
        /// </summary>
        public class Nep17Transfer
        {
            /// <summary>
            /// Gets or sets the timestamp.
            /// </summary>
            [JsonPropertyName("timestamp")]
            public long Timestamp { get; set; }

            /// <summary>
            /// Gets or sets the asset hash.
            /// </summary>
            [JsonPropertyName("assethash")]
            public Hash160 AssetHash { get; set; }

            /// <summary>
            /// Gets or sets the transfer address.
            /// </summary>
            [JsonPropertyName("transferaddress")]
            public string TransferAddress { get; set; }

            /// <summary>
            /// Gets or sets the amount.
            /// </summary>
            [JsonPropertyName("amount")]
            public string Amount { get; set; }

            /// <summary>
            /// Gets or sets the block index.
            /// </summary>
            [JsonPropertyName("blockindex")]
            public int BlockIndex { get; set; }

            /// <summary>
            /// Gets or sets the transfer notify index.
            /// </summary>
            [JsonPropertyName("transfernotifyindex")]
            public int TransferNotifyIndex { get; set; }

            /// <summary>
            /// Gets or sets the transaction hash.
            /// </summary>
            [JsonPropertyName("txhash")]
            public Hash256 TxHash { get; set; }
        }
    }
}