using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a contract method token.
    /// </summary>
    public class ContractMethodToken
    {
        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the parameter count.
        /// </summary>
        [JsonPropertyName("paramcount")]
        public int ParamCount { get; set; }

        /// <summary>
        /// Gets or sets whether it has return value.
        /// </summary>
        [JsonPropertyName("hasreturnvalue")]
        public bool HasReturnValue { get; set; }

        /// <summary>
        /// Gets or sets the call flags.
        /// </summary>
        [JsonPropertyName("callflags")]
        public string CallFlags { get; set; }
    }
}