using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getpeers method.
    /// </summary>
    public class NeoGetPeers
    {
        /// <summary>
        /// Represents the peers information.
        /// </summary>
        public class Peers
        {
            /// <summary>
            /// Gets or sets the list of unconnected peers.
            /// </summary>
            [JsonPropertyName("unconnected")]
            public List<Peer> Unconnected { get; set; }

            /// <summary>
            /// Gets or sets the list of bad peers.
            /// </summary>
            [JsonPropertyName("bad")]
            public List<Peer> Bad { get; set; }

            /// <summary>
            /// Gets or sets the list of connected peers.
            /// </summary>
            [JsonPropertyName("connected")]
            public List<Peer> Connected { get; set; }
        }

        /// <summary>
        /// Represents a peer.
        /// </summary>
        public class Peer
        {
            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; }

            /// <summary>
            /// Gets or sets the port.
            /// </summary>
            [JsonPropertyName("port")]
            public int Port { get; set; }
        }
    }
}