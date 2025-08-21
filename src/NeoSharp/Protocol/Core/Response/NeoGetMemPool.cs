using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getmempool method.
    /// </summary>
    public class NeoGetMemPool
    {
        /// <summary>
        /// Represents the memory pool details.
        /// </summary>
        public class MemPoolDetails
        {
            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [JsonPropertyName("height")]
            public int Height { get; set; }

            /// <summary>
            /// Gets or sets the list of verified transactions.
            /// </summary>
            [JsonPropertyName("verified")]
            public List<Hash256> Verified { get; set; }

            /// <summary>
            /// Gets or sets the list of unverified transactions.
            /// </summary>
            [JsonPropertyName("unverified")]
            public List<Hash256> Unverified { get; set; }
        }
    }
}