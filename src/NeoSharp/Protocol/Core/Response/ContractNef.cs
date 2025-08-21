using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a contract NEF (Neo Executable Format).
    /// </summary>
    public class ContractNef
    {
        /// <summary>
        /// Gets or sets the magic number.
        /// </summary>
        [JsonPropertyName("magic")]
        public uint Magic { get; set; }

        /// <summary>
        /// Gets or sets the compiler information.
        /// </summary>
        [JsonPropertyName("compiler")]
        public string Compiler { get; set; }

        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the tokens.
        /// </summary>
        [JsonPropertyName("tokens")]
        public List<ContractMethodToken> Tokens { get; set; }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        [JsonPropertyName("script")]
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets the checksum.
        /// </summary>
        [JsonPropertyName("checksum")]
        public uint Checksum { get; set; }
    }
}