using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for calculatenetworkfee method.
    /// </summary>
    public class NeoNetworkFee
    {
        /// <summary>
        /// Gets or sets the network fee.
        /// </summary>
        [JsonPropertyName("networkfee")]
        public string NetworkFee { get; set; }
    }
}