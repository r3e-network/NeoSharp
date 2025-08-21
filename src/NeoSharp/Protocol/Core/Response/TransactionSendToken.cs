using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a transaction send token.
    /// </summary>
    public class TransactionSendToken
    {
        /// <summary>
        /// Gets or sets the token hash.
        /// </summary>
        [JsonPropertyName("asset")]
        public Hash160 Asset { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }
    }
}