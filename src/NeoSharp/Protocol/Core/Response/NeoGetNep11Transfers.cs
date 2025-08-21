using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getnep11transfers method.
    /// </summary>
    public class NeoGetNep11Transfers
    {
        /// <summary>
        /// Represents the NEP-11 transfers.
        /// </summary>
        public class Nep11Transfers
        {
            /// <summary>
            /// Gets or sets the sent transfers.
            /// </summary>
            [JsonPropertyName("sent")]
            public List<Nep11Transfer> Sent { get; set; }

            /// <summary>
            /// Gets or sets the received transfers.
            /// </summary>
            [JsonPropertyName("received")]
            public List<Nep11Transfer> Received { get; set; }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; }
        }

        /// <summary>
        /// Represents a NEP-11 transfer.
        /// </summary>
        public class Nep11Transfer
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
            /// Gets or sets the token ID.
            /// </summary>
            [JsonPropertyName("tokenid")]
            public string TokenId { get; set; }

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