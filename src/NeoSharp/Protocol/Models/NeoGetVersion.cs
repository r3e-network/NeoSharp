using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Models
{
    /// <summary>
    /// Response from getversion RPC call
    /// </summary>
    public class NeoGetVersion
    {
        /// <summary>
        /// Version information
        /// </summary>
        public class NeoVersion
        {
            /// <summary>
            /// TCP port
            /// </summary>
            [JsonPropertyName("tcpport")]
            public int TcpPort { get; set; }

            /// <summary>
            /// WebSocket port
            /// </summary>
            [JsonPropertyName("wsport")]
            public int WsPort { get; set; }

            /// <summary>
            /// Nonce
            /// </summary>
            [JsonPropertyName("nonce")]
            public long Nonce { get; set; }

            /// <summary>
            /// User agent
            /// </summary>
            [JsonPropertyName("useragent")]
            public string UserAgent { get; set; } = string.Empty;

            /// <summary>
            /// Protocol information
            /// </summary>
            [JsonPropertyName("protocol")]
            public ProtocolInfo Protocol { get; set; } = new();
        }

        /// <summary>
        /// Protocol information
        /// </summary>
        public class ProtocolInfo
        {
            /// <summary>
            /// Network ID
            /// </summary>
            [JsonPropertyName("network")]
            public long Network { get; set; }

            /// <summary>
            /// Validators count
            /// </summary>
            [JsonPropertyName("validatorscount")]
            public int ValidatorsCount { get; set; }

            /// <summary>
            /// Milliseconds per block
            /// </summary>
            [JsonPropertyName("msperblock")]
            public long MillisecondsPerBlock { get; set; }

            /// <summary>
            /// Maximum transactions per block
            /// </summary>
            [JsonPropertyName("maxtraceableblocks")]
            public int MaxTraceableBlocks { get; set; }

            /// <summary>
            /// Address version
            /// </summary>
            [JsonPropertyName("addressversion")]
            public byte AddressVersion { get; set; }

            /// <summary>
            /// Maximum transactions per block
            /// </summary>
            [JsonPropertyName("maxtransactionsperblock")]
            public int MaxTransactionsPerBlock { get; set; }

            /// <summary>
            /// Memory pool maximum transactions
            /// </summary>
            [JsonPropertyName("memorypoolmaxtransactions")]
            public int MemoryPoolMaxTransactions { get; set; }

            /// <summary>
            /// Initial gas distribution
            /// </summary>
            [JsonPropertyName("initialgasdistribution")]
            public string InitialGasDistribution { get; set; } = string.Empty;
        }
    }
}