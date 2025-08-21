using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for findstates method.
    /// </summary>
    public class NeoFindStates
    {
        /// <summary>
        /// Represents the states.
        /// </summary>
        public class States
        {
            /// <summary>
            /// Gets or sets whether the proof was truncated.
            /// </summary>
            [JsonPropertyName("truncated")]
            public bool Truncated { get; set; }

            /// <summary>
            /// Gets or sets the first proof.
            /// </summary>
            [JsonPropertyName("firstproof")]
            public string FirstProof { get; set; }

            /// <summary>
            /// Gets or sets the last proof.
            /// </summary>
            [JsonPropertyName("lastproof")]
            public string LastProof { get; set; }

            /// <summary>
            /// Gets or sets the results.
            /// </summary>
            [JsonPropertyName("results")]
            public List<StateResult> Results { get; set; }
        }

        /// <summary>
        /// Represents a state result.
        /// </summary>
        public class StateResult
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            [JsonPropertyName("key")]
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
    }
}