using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NeoSharp.Types;
using NeoSharp.Wallet.NEP6;
using NeoSharp.Crypto;
using NeoSharp.Protocol;

namespace NeoSharp.Wallet
{
    /// <summary>
    /// The wallet manages a collection of accounts.
    /// </summary>
    public class Wallet : IDisposable
    {
        private const string DefaultWalletName = "NeoSharpWallet";
        public const string CurrentVersion = "3.0";
        
        private readonly Dictionary<Hash160, Account> _accountsMap = new();
        private Hash160? _defaultAccountHash;
        private bool _disposed;

        /// <summary>
        /// Gets or sets the wallet name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the wallet version
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets or sets the scrypt parameters
        /// </summary>
        public NEP6.ScryptParams ScryptParams { get; private set; }

        /// <summary>
        /// Gets the accounts in this wallet (sorted by script hash)
        /// </summary>
        public IReadOnlyList<Account> Accounts => _accountsMap.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList().AsReadOnly();

        /// <summary>
        /// Gets the default account of this wallet
        /// </summary>
        public Account? DefaultAccount => _defaultAccountHash.HasValue ? _accountsMap.GetValueOrDefault(_defaultAccountHash.Value) : null;

        /// <summary>
        /// Initializes a new wallet with default settings
        /// </summary>
        public Wallet()
        {
            Name = DefaultWalletName;
            Version = CurrentVersion;
            ScryptParams = NEP6.ScryptParams.Default;
        }

        /// <summary>
        /// Initializes a new wallet with specified name
        /// </summary>
        /// <param name="name">The wallet name</param>
        public Wallet(string name) : this()
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Sets the given account to the default account of this wallet
        /// </summary>
        /// <param name="account">The new default account</param>
        /// <returns>The wallet (self)</returns>
        public Wallet SetDefaultAccount(Account account)
        {
            var scriptHash = account.GetScriptHash();
            if (scriptHash == null)
                throw new ArgumentException("Account must have a valid script hash", nameof(account));
            
            return SetDefaultAccount(scriptHash.Value);
        }

        /// <summary>
        /// Sets the account with the given script hash to the default account of this wallet
        /// </summary>
        /// <param name="accountHash">The new default account's script hash</param>
        /// <returns>The wallet (self)</returns>
        public Wallet SetDefaultAccount(Hash160 accountHash)
        {
            if (!_accountsMap.ContainsKey(accountHash))
                throw new ArgumentException($"Cannot set default account on wallet. Wallet does not contain the account with script hash {accountHash}.");
            
            _defaultAccountHash = accountHash;
            return this;
        }

        /// <summary>
        /// Checks whether an account is the default account in the wallet
        /// </summary>
        /// <param name="account">The account to be checked</param>
        /// <returns>Whether the given account is the default account in this wallet</returns>
        public bool IsDefault(Account account)
        {
            return IsDefault(account.GetScriptHash());
        }

        /// <summary>
        /// Checks whether an account is the default account in the wallet
        /// </summary>
        /// <param name="accountHash">The account hash to be checked</param>
        /// <returns>Whether the given account is the default account in this wallet</returns>
        public bool IsDefault(Hash160? accountHash)
        {
            return DefaultAccount?.GetScriptHash() == accountHash && accountHash != null;
        }

        /// <summary>
        /// Sets the wallet name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>This wallet (for method chaining)</returns>
        public Wallet SetName(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        /// <summary>
        /// Sets the wallet version
        /// </summary>
        /// <param name="version">The version</param>
        /// <returns>This wallet (for method chaining)</returns>
        public Wallet SetVersion(string version)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            return this;
        }

        /// <summary>
        /// Sets the scrypt parameters
        /// </summary>
        /// <param name="scryptParams">The scrypt parameters</param>
        /// <returns>This wallet (for method chaining)</returns>
        public Wallet SetScryptParams(NEP6.ScryptParams scryptParams)
        {
            ScryptParams = scryptParams ?? throw new ArgumentNullException(nameof(scryptParams));
            return this;
        }

        /// <summary>
        /// Adds the given accounts to this wallet, if it doesn't contain an account with the same script hash
        /// </summary>
        /// <param name="accounts">The accounts to add</param>
        /// <returns>The wallet (self)</returns>
        public Wallet AddAccounts(params Account[] accounts)
        {
            return AddAccounts(accounts.AsEnumerable());
        }

        /// <summary>
        /// Adds the given accounts to this wallet, if it doesn't contain an account with the same script hash
        /// </summary>
        /// <param name="accounts">The accounts to add</param>
        /// <returns>The wallet (self)</returns>
        public Wallet AddAccounts(IEnumerable<Account> accounts)
        {
            var accountsList = accounts.ToList();
            var filteredAccounts = accountsList.Where(account =>
            {
                var scriptHash = account.GetScriptHash();
                return scriptHash != null && !_accountsMap.ContainsKey(scriptHash.Value);
            }).ToList();

            var accountWithExistingWallet = filteredAccounts.FirstOrDefault(a => a.Wallet != null);
            if (accountWithExistingWallet != null)
            {
                throw new ArgumentException($"The account {accountWithExistingWallet.Address} is already contained in a wallet. Please remove this account from its containing wallet before adding it to another wallet.");
            }

            foreach (var account in filteredAccounts)
            {
                var scriptHash = account.GetScriptHash()!.Value;
                _accountsMap[scriptHash] = account;
                account.SetWallet(this);
            }

            return this;
        }

        /// <summary>
        /// Removes the account from this wallet
        /// </summary>
        /// <param name="account">The account to be removed</param>
        /// <returns>True if an account was removed, false if no account with the given address was found</returns>
        public bool RemoveAccount(Account account)
        {
            var scriptHash = account.GetScriptHash();
            if (scriptHash == null) return false;
            
            return RemoveAccount(scriptHash.Value);
        }

        /// <summary>
        /// Removes the account from this wallet
        /// </summary>
        /// <param name="accountHash">The Hash160 of the account to be removed</param>
        /// <returns>True if an account was removed, false if no account with the given address was found</returns>
        public bool RemoveAccount(Hash160 accountHash)
        {
            if (!_accountsMap.ContainsKey(accountHash)) return false;
            
            if (_accountsMap.Count <= 1)
                throw new InvalidOperationException($"The account {accountHash.ToAddress()} is the only account in the wallet. It cannot be removed.");
            
            var account = _accountsMap[accountHash];
            account?.SetWallet(null);
            
            if (accountHash.Equals(_defaultAccountHash))
            {
                var newDefault = _accountsMap.Keys.First(k => !k.Equals(accountHash));
                SetDefaultAccount(newDefault);
            }
            
            var removed = _accountsMap.Remove(accountHash);
            if (removed)
            {
                account?.Dispose();
            }
            
            return removed;
        }

        /// <summary>
        /// Decrypts all accounts in the wallet
        /// </summary>
        /// <param name="password">The password to use for decryption</param>
        public void DecryptAllAccounts(string password)
        {
            foreach (var account in _accountsMap.Values)
            {
                try
                {
                    account.DecryptPrivateKey(password, ScryptParams);
                }
                catch
                {
                    // Skip accounts that can't be decrypted
                }
            }
        }

        /// <summary>
        /// Encrypts all accounts in the wallet
        /// </summary>
        /// <param name="password">The password to use for encryption</param>
        public void EncryptAllAccounts(string password)
        {
            foreach (var account in _accountsMap.Values)
            {
                try
                {
                    account.EncryptPrivateKey(password, ScryptParams);
                }
                catch
                {
                    // Skip accounts that can't be encrypted
                }
            }
        }

        /// <summary>
        /// Converts this wallet to NEP-6 format
        /// </summary>
        /// <returns>The NEP-6 wallet</returns>
        public NEP6Wallet ToNEP6Wallet()
        {
            var accounts = _accountsMap.Values.Select(a => a.ToNEP6Account()).ToList();
            return new NEP6Wallet
            {
                Name = Name,
                Version = Version,
                Scrypt = ScryptParams,
                Accounts = accounts,
                Extra = null
            };
        }

        /// <summary>
        /// Gets an account by script hash
        /// </summary>
        /// <param name="accountHash">The script hash</param>
        /// <returns>The account or null if not found</returns>
        public Account? GetAccount(Hash160 accountHash)
        {
            return _accountsMap.GetValueOrDefault(accountHash);
        }

        /// <summary>
        /// Gets an account by address
        /// </summary>
        /// <param name="address">The address</param>
        /// <returns>The account or null if not found</returns>
        public Account? GetAccount(string address)
        {
            try
            {
                var scriptHash = Hash160.FromAddress(address);
                return GetAccount(scriptHash);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if the wallet holds an account with the given script hash
        /// </summary>
        /// <param name="accountHash">The script hash</param>
        /// <returns>True if the wallet contains the account</returns>
        public bool HoldsAccount(Hash160 accountHash)
        {
            return _accountsMap.ContainsKey(accountHash);
        }

        /// <summary>
        /// Gets the balances of all NEP-17 tokens that this wallet owns
        /// </summary>
        /// <param name="neoSharp">The NeoSharp object used to call the neo node</param>
        /// <returns>The map of token script hashes to token amounts</returns>
        public async Task<Dictionary<Hash160, int>> GetNep17TokenBalancesAsync(INeoSharp neoSharp)
        {
            var balances = new Dictionary<Hash160, int>();
            
            foreach (var account in _accountsMap.Values)
            {
                try
                {
                    var accountBalances = await account.GetNep17BalancesAsync(neoSharp);
                    foreach (var kvp in accountBalances)
                    {
                        if (balances.ContainsKey(kvp.Key))
                        {
                            balances[kvp.Key] += kvp.Value;
                        }
                        else
                        {
                            balances[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch
                {
                    // Skip accounts that can't get balances
                }
            }
            
            return balances;
        }

        #region Static Factory Methods

        /// <summary>
        /// Creates a new wallet with one account
        /// </summary>
        /// <returns>The new wallet</returns>
        public static Wallet Create()
        {
            var account = Account.Create();
            var wallet = new Wallet();
            wallet.AddAccounts(account);
            var scriptHash = account.GetScriptHash()!.Value;
            wallet.SetDefaultAccount(scriptHash);
            return wallet;
        }

        /// <summary>
        /// Creates a new wallet with one account that is set as the default account and encrypts it
        /// </summary>
        /// <param name="password">The passphrase used to encrypt the account</param>
        /// <returns>The new wallet</returns>
        public static Wallet Create(string password)
        {
            var wallet = Create();
            wallet.EncryptAllAccounts(password);
            return wallet;
        }

        /// <summary>
        /// Creates a new wallet with one account and persists it to a NEP6 wallet file
        /// </summary>
        /// <param name="password">Password used to encrypt the account</param>
        /// <param name="destinationPath">Destination to the new NEP6 wallet file</param>
        /// <returns>The new wallet</returns>
        public static Wallet Create(string password, string destinationPath)
        {
            var wallet = Create(password);
            wallet.SaveNEP6Wallet(destinationPath);
            return wallet;
        }

        /// <summary>
        /// Creates a new wallet with the given accounts
        /// </summary>
        /// <param name="accounts">The accounts to add to the new wallet</param>
        /// <returns>The new wallet</returns>
        public static Wallet WithAccounts(params Account[] accounts)
        {
            if (accounts.Length == 0)
                throw new ArgumentException("No accounts provided to initialize a wallet.", nameof(accounts));
            
            var wallet = new Wallet();
            wallet.AddAccounts(accounts);
            var firstAccountHash = accounts[0].GetScriptHash()!.Value;
            wallet.SetDefaultAccount(firstAccountHash);
            return wallet;
        }

        /// <summary>
        /// Loads a wallet from a NEP6 wallet file
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The loaded wallet</returns>
        public static Wallet FromNEP6Wallet(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var nep6Wallet = JsonSerializer.Deserialize<NEP6Wallet>(json) ?? 
                           throw new InvalidOperationException("Failed to deserialize NEP6 wallet");
            return FromNEP6Wallet(nep6Wallet);
        }

        /// <summary>
        /// Creates a wallet from a NEP6Wallet object
        /// </summary>
        /// <param name="nep6Wallet">The NEP6 wallet</param>
        /// <returns>The wallet</returns>
        public static Wallet FromNEP6Wallet(NEP6Wallet nep6Wallet)
        {
            var accounts = nep6Wallet.Accounts.Select(Account.FromNEP6Account).ToArray();
            var defaultNep6Account = nep6Wallet.Accounts.FirstOrDefault(a => a.IsDefault);
            
            if (defaultNep6Account == null)
                throw new ArgumentException("The NEP-6 wallet does not contain any default account.");
            
            var defaultAccount = Account.FromNEP6Account(defaultNep6Account);
            var defaultHash = defaultAccount.GetScriptHash()!.Value;
            
            var wallet = new Wallet()
                .SetName(nep6Wallet.Name)
                .SetVersion(nep6Wallet.Version)
                .SetScryptParams(nep6Wallet.Scrypt)
                .AddAccounts(accounts)
                .SetDefaultAccount(defaultHash);
            
            return wallet;
        }

        #endregion

        /// <summary>
        /// Creates a NEP-6 compatible wallet file
        /// </summary>
        /// <param name="destinationPath">The file path where the wallet file should be saved</param>
        /// <returns>The wallet (self)</returns>
        public Wallet SaveNEP6Wallet(string destinationPath)
        {
            var nep6Wallet = ToNEP6Wallet();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(nep6Wallet, options);
            
            // If destination is a directory, append wallet name
            if (Directory.Exists(destinationPath))
            {
                destinationPath = Path.Combine(destinationPath, $"{Name}.json");
            }
            
            File.WriteAllText(destinationPath, json);
            return this;
        }

        /// <summary>
        /// Disposes the wallet and all accounts
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var account in _accountsMap.Values)
                {
                    account.Dispose();
                }
                _accountsMap.Clear();
                _disposed = true;
            }
        }
    }
}