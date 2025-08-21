using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for validateaddress method.
    /// </summary>
    public class NeoValidateAddress
    {
        /// <summary>
        /// Represents the validation result.
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            [JsonPropertyName("address")]
            public string Address { get; set; }

            /// <summary>
            /// Gets or sets whether the address is valid.
            /// </summary>
            [JsonPropertyName("isvalid")]
            public bool IsValid { get; set; }
        }
    }
}