using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getwalletbalance method.
    /// </summary>
    public class NeoGetWalletBalance
    {
        /// <summary>
        /// Represents the balance information.
        /// </summary>
        public class Balance
        {
            /// <summary>
            /// Gets or sets the balance amount.
            /// </summary>
            [JsonPropertyName("balance")]
            public string BalanceAmount { get; set; }
        }
    }
}