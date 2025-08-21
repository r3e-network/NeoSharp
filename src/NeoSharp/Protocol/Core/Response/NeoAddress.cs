using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a Neo address.
    /// </summary>
    public class NeoAddress
    {
        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets whether it has a key.
        /// </summary>
        [JsonPropertyName("haskey")]
        public bool HasKey { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets whether it is a watch-only address.
        /// </summary>
        [JsonPropertyName("watchonly")]
        public bool WatchOnly { get; set; }
    }
}