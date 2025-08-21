using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a native contract state.
    /// </summary>
    public class NativeContractState
    {
        /// <summary>
        /// Gets or sets the contract ID.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the contract hash.
        /// </summary>
        [JsonPropertyName("hash")]
        public Hash160 Hash { get; set; }

        /// <summary>
        /// Gets or sets the NEF (Neo Executable Format) data.
        /// </summary>
        [JsonPropertyName("nef")]
        public ContractNef Nef { get; set; }

        /// <summary>
        /// Gets or sets the contract manifest.
        /// </summary>
        [JsonPropertyName("manifest")]
        public ContractManifest Manifest { get; set; }

        /// <summary>
        /// Gets or sets the update counter.
        /// </summary>
        [JsonPropertyName("updatecounter")]
        public int UpdateCounter { get; set; }
    }
}