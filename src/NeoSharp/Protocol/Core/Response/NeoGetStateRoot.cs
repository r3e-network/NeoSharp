using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getstateroot method.
    /// </summary>
    public class NeoGetStateRoot
    {
        /// <summary>
        /// Represents a state root.
        /// </summary>
        public class StateRoot
        {
            /// <summary>
            /// Gets or sets the version.
            /// </summary>
            [JsonPropertyName("version")]
            public byte Version { get; set; }

            /// <summary>
            /// Gets or sets the index.
            /// </summary>
            [JsonPropertyName("index")]
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the root hash.
            /// </summary>
            [JsonPropertyName("roothash")]
            public Hash256 RootHash { get; set; }

            /// <summary>
            /// Gets or sets the witnesses.
            /// </summary>
            [JsonPropertyName("witnesses")]
            public List<NeoWitness> Witnesses { get; set; }
        }
    }
}