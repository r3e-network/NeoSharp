using System;
using NeoSharp.Crypto;
using NeoSharp.Types;
using NeoSharp.Wallet;

namespace NeoSharp.Transaction
{
    /// <summary>
    /// Represents an account-based signer for transactions
    /// </summary>
    public class AccountSigner : Signer
    {
        /// <summary>
        /// The account to use for signing
        /// </summary>
        public Account Account { get; }

        /// <summary>
        /// Initializes a new AccountSigner
        /// </summary>
        /// <param name="account">The account to use for signing</param>
        /// <param name="scopes">The witness scopes (default: CalledByEntry)</param>
        public AccountSigner(Account account, WitnessScope scopes = WitnessScope.CalledByEntry) 
            : base(account.GetScriptHash() ?? throw new ArgumentException("Account must have a valid script hash", nameof(account)), scopes)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }

        /// <summary>
        /// Initializes a new AccountSigner with allowed contracts
        /// </summary>
        /// <param name="account">The account to use for signing</param>
        /// <param name="scopes">The witness scopes</param>
        /// <param name="allowedContracts">The allowed contracts</param>
        public AccountSigner(Account account, WitnessScope scopes, Hash160[] allowedContracts) 
            : base(account.GetScriptHash() ?? throw new ArgumentException("Account must have a valid script hash", nameof(account)), scopes, allowedContracts)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }

        /// <summary>
        /// Initializes a new AccountSigner with allowed groups
        /// </summary>
        /// <param name="account">The account to use for signing</param>
        /// <param name="scopes">The witness scopes</param>
        /// <param name="allowedContracts">The allowed contracts</param>
        /// <param name="allowedGroups">The allowed groups</param>
        public AccountSigner(Account account, WitnessScope scopes, Hash160[] allowedContracts, ECPoint[] allowedGroups) 
            : base(account.GetScriptHash() ?? throw new ArgumentException("Account must have a valid script hash", nameof(account)), scopes, allowedContracts, allowedGroups)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }
    }
}