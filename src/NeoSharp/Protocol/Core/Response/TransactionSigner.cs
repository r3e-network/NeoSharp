using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a transaction signer.
    /// </summary>
    public class TransactionSigner
    {
        /// <summary>
        /// Gets or sets the account script hash.
        /// </summary>
        [JsonPropertyName("account")]
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the witness scopes.
        /// </summary>
        [JsonPropertyName("scopes")]
        public string Scopes { get; set; }

        /// <summary>
        /// Gets or sets the allowed contracts.
        /// </summary>
        [JsonPropertyName("allowedcontracts")]
        public List<string> AllowedContracts { get; set; }

        /// <summary>
        /// Gets or sets the allowed groups.
        /// </summary>
        [JsonPropertyName("allowedgroups")]
        public List<string> AllowedGroups { get; set; }

        /// <summary>
        /// Gets or sets the witness rules.
        /// </summary>
        [JsonPropertyName("rules")]
        public List<WitnessRule> Rules { get; set; }
    }

    /// <summary>
    /// Represents a witness rule.
    /// </summary>
    public class WitnessRule
    {
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the condition.
        /// </summary>
        [JsonPropertyName("condition")]
        public WitnessCondition Condition { get; set; }
    }

    /// <summary>
    /// Represents a witness condition.
    /// </summary>
    public class WitnessCondition
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the expression list.
        /// </summary>
        [JsonPropertyName("expressions")]
        public List<WitnessCondition> Expressions { get; set; }

        /// <summary>
        /// Gets or sets the hash value.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the group value.
        /// </summary>
        [JsonPropertyName("group")]
        public string Group { get; set; }
    }
}