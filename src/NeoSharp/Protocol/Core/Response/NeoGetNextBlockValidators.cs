using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getnextblockvalidators method.
    /// </summary>
    public class NeoGetNextBlockValidators
    {
        /// <summary>
        /// Represents a validator.
        /// </summary>
        public class Validator
        {
            /// <summary>
            /// Gets or sets the public key.
            /// </summary>
            [JsonPropertyName("publickey")]
            public string PublicKey { get; set; }

            /// <summary>
            /// Gets or sets the votes.
            /// </summary>
            [JsonPropertyName("votes")]
            public string Votes { get; set; }
        }
    }
}