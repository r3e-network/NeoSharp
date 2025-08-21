using System.Numerics;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Responses
{
    /// <summary>
    /// Represents a NEO account state.
    /// </summary>
    public class NeoAccountState
    {
        /// <summary>
        /// Gets or sets the NEO balance.
        /// </summary>
        [JsonPropertyName("balance")]
        public string Balance { get; set; } = "0";

        /// <summary>
        /// Gets or sets the balance height.
        /// </summary>
        [JsonPropertyName("balanceHeight")]
        public long BalanceHeight { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        [JsonPropertyName("publicKey")]
        public string? PublicKey { get; set; }

        /// <summary>
        /// Gets or sets whether the account has voted.
        /// </summary>
        [JsonPropertyName("voteTo")]
        public string? VoteTo { get; set; }

        /// <summary>
        /// Gets the balance as a BigInteger.
        /// </summary>
        /// <returns>The balance.</returns>
        public BigInteger GetBalance()
        {
            return BigInteger.TryParse(Balance, out var result) ? result : BigInteger.Zero;
        }
    }
}