using System.Text.Json.Serialization;

namespace NeoSharp.Wallet.NEP6
{
    /// <summary>
    /// Represents a NEP-6 standard account.
    /// </summary>
    public class NEP6Account
    {
        /// <summary>
        /// Gets or sets the account address.
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the account label.
        /// </summary>
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default account.
        /// </summary>
        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets whether the account is locked.
        /// </summary>
        [JsonPropertyName("lock")]
        public bool Lock { get; set; }

        /// <summary>
        /// Gets or sets the encrypted private key (NEP-2 format).
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the contract.
        /// </summary>
        [JsonPropertyName("contract")]
        public NEP6Contract? Contract { get; set; }

        /// <summary>
        /// Gets or sets extra data.
        /// </summary>
        [JsonPropertyName("extra")]
        public object? Extra { get; set; }

        /// <summary>
        /// Initializes a new instance of the NEP6Account class.
        /// </summary>
        public NEP6Account()
        {
            Address = string.Empty;
            Key = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the NEP6Account class.
        /// </summary>
        /// <param name="address">The account address.</param>
        /// <param name="key">The encrypted private key.</param>
        /// <param name="contract">The contract.</param>
        public NEP6Account(string address, string key, NEP6Contract? contract = null)
        {
            Address = address;
            Key = key;
            Contract = contract;
        }

        /// <summary>
        /// Creates a copy of this account.
        /// </summary>
        /// <returns>A copy of the account.</returns>
        public NEP6Account Copy()
        {
            return new NEP6Account
            {
                Address = Address,
                Label = Label,
                IsDefault = IsDefault,
                Lock = Lock,
                Key = Key,
                Contract = Contract?.Copy(),
                Extra = Extra
            };
        }

        /// <summary>
        /// Validates the account data.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Address))
                return false;

            if (string.IsNullOrEmpty(Key))
                return false;

            // Validate address format (should start with 'N' for Neo N3)
            if (!Address.StartsWith("N") || Address.Length != 34)
                return false;

            // Validate NEP-2 key format (should be 58 characters base58)
            if (Key.Length != 58)
                return false;

            return true;
        }

        /// <summary>
        /// Updates the label of the account.
        /// </summary>
        /// <param name="newLabel">The new label.</param>
        public void UpdateLabel(string? newLabel)
        {
            Label = newLabel;
        }

        /// <summary>
        /// Locks or unlocks the account.
        /// </summary>
        /// <param name="locked">True to lock, false to unlock.</param>
        public void SetLock(bool locked)
        {
            Lock = locked;
        }

        /// <summary>
        /// Checks if this account can sign transactions.
        /// </summary>
        /// <returns>True if the account can sign, false otherwise.</returns>
        public bool CanSign()
        {
            return !Lock && !string.IsNullOrEmpty(Key) && Contract != null;
        }

        /// <summary>
        /// Gets a string representation of the account.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var label = string.IsNullOrEmpty(Label) ? "Unlabeled" : Label;
            var defaultStr = IsDefault ? " (Default)" : "";
            var lockStr = Lock ? " [Locked]" : "";
            return $"{label}: {Address}{defaultStr}{lockStr}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not NEP6Account other)
                return false;

            return Address == other.Address;
        }

        /// <summary>
        /// Gets the hash code for this account.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Address?.GetHashCode() ?? 0;
        }
    }
}