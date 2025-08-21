using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a witness in the Neo blockchain.
    /// </summary>
    public class NeoWitness
    {
        /// <summary>
        /// Gets or sets the invocation script.
        /// </summary>
        [JsonPropertyName("invocation")]
        public string Invocation { get; set; }

        /// <summary>
        /// Gets or sets the verification script.
        /// </summary>
        [JsonPropertyName("verification")]
        public string Verification { get; set; }
    }
}