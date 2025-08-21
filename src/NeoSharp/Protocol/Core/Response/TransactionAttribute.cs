using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a transaction attribute.
    /// </summary>
    public class TransactionAttribute
    {
        /// <summary>
        /// Gets or sets the attribute type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the allow multiple flag.
        /// </summary>
        [JsonPropertyName("allowmultiple")]
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
}