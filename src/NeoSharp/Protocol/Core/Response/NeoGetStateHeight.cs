using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getstateheight method.
    /// </summary>
    public class NeoGetStateHeight
    {
        /// <summary>
        /// Represents the state height.
        /// </summary>
        public class StateHeight
        {
            /// <summary>
            /// Gets or sets the local root index.
            /// </summary>
            [JsonPropertyName("localrootindex")]
            public int LocalRootIndex { get; set; }

            /// <summary>
            /// Gets or sets the validated root index.
            /// </summary>
            [JsonPropertyName("validatedrootindex")]
            public int ValidatedRootIndex { get; set; }
        }
    }
}