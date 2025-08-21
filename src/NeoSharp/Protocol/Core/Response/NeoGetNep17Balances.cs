using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getnep17balances method.
    /// </summary>
    public class NeoGetNep17Balances
    {
        /// <summary>
        /// Represents the NEP-17 balances.
        /// </summary>
        public class Nep17Balances
        {
            /// <summary>
            /// Gets or sets the balance list.
            /// </summary>
            [JsonPropertyName("balance")]
            public List<Nep17Balance> Balance { get; set; }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; }
        }

        /// <summary>
        /// Represents a NEP-17 balance.
        /// </summary>
        public class Nep17Balance
        {
            /// <summary>
            /// Gets or sets the asset hash.
            /// </summary>
            [JsonPropertyName("assethash")]
            public string AssetHash { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the symbol.
            /// </summary>
            [JsonPropertyName("symbol")]
            public string Symbol { get; set; }

            /// <summary>
            /// Gets or sets the decimals.
            /// </summary>
            [JsonPropertyName("decimals")]
            public string Decimals { get; set; }

            /// <summary>
            /// Gets or sets the amount.
            /// </summary>
            [JsonPropertyName("amount")]
            public string Amount { get; set; }

            /// <summary>
            /// Gets or sets the last updated block.
            /// </summary>
            [JsonPropertyName("lastupdatedblock")]
            public int LastUpdatedBlock { get; set; }
        }
    }
}