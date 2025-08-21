using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for sendrawtransaction method.
    /// </summary>
    public class NeoSendRawTransaction
    {
        /// <summary>
        /// Represents the raw transaction result.
        /// </summary>
        public class RawTransaction
        {
            /// <summary>
            /// Gets or sets the transaction hash.
            /// </summary>
            [JsonPropertyName("hash")]
            public Hash256 Hash { get; set; }
        }
    }
}