using System.Text.Json.Serialization;

#nullable enable

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getunclaimedgas method.
    /// </summary>
    public class NeoGetUnclaimedGas
    {
        /// <summary>
        /// Represents the unclaimed gas information.
        /// </summary>
        public class GetUnclaimedGas
        {
            /// <summary>
            /// Gets or sets the unclaimed amount.
            /// </summary>
            [JsonPropertyName("unclaimed")]
            public string Unclaimed { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; } = string.Empty;
        }
    }
}
