using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getnep11balances method.
    /// </summary>
    public class NeoGetNep11Balances
    {
        /// <summary>
        /// Represents the NEP-11 balances.
        /// </summary>
        public class Nep11Balances
        {
            /// <summary>
            /// Gets or sets the balance list.
            /// </summary>
            [JsonPropertyName("balance")]
            public List<Nep11Balance> Balance { get; set; }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; }
        }

        /// <summary>
        /// Represents a NEP-11 balance.
        /// </summary>
        public class Nep11Balance
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
            /// Gets or sets the tokens.
            /// </summary>
            [JsonPropertyName("tokens")]
            public List<Nep11Token> Tokens { get; set; }
        }

        /// <summary>
        /// Represents a NEP-11 token.
        /// </summary>
        public class Nep11Token
        {
            /// <summary>
            /// Gets or sets the token ID.
            /// </summary>
            [JsonPropertyName("tokenid")]
            public string TokenId { get; set; }

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