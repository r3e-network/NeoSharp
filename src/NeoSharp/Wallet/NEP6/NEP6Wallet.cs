using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NeoSharp.Crypto;
using NeoSharp.Types;

namespace NeoSharp.Wallet.NEP6
{
    /// <summary>
    /// Represents a NEP-6 standard wallet.
    /// </summary>
    public class NEP6Wallet
    {
        /// <summary>
        /// Gets or sets the wallet name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the wallet version.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Gets or sets the scrypt parameters.
        /// </summary>
        [JsonPropertyName("scrypt")]
        public ScryptParams Scrypt { get; set; }

        /// <summary>
        /// Gets or sets the accounts.
        /// </summary>
        [JsonPropertyName("accounts")]
        public List<NEP6Account> Accounts { get; set; }

        /// <summary>
        /// Gets or sets extra data.
        /// </summary>
        [JsonPropertyName("extra")]
        public object Extra { get; set; }

        /// <summary>
        /// Initializes a new instance of the NEP6Wallet class.
        /// </summary>
        public NEP6Wallet()
        {
            Accounts = new List<NEP6Account>();
            Scrypt = ScryptParams.Default;
        }

        /// <summary>
        /// Initializes a new instance of the NEP6Wallet class.
        /// </summary>
        /// <param name="name">The wallet name.</param>
        /// <param name="scrypt">The scrypt parameters.</param>
        public NEP6Wallet(string name, ScryptParams scrypt = null)
        {
            Name = name;
            Scrypt = scrypt ?? ScryptParams.Default;
            Accounts = new List<NEP6Account>();
        }

        /// <summary>
        /// Creates a new account in the wallet.
        /// </summary>
        /// <param name="password">The password to encrypt the private key.</param>
        /// <returns>The created account.</returns>
        public NEP6Account CreateAccount(string password)
        {
            var privateKey = new ECPrivateKey();
            var keyPair = new ECKeyPair(privateKey);
            var account = new Account(keyPair);
            var nep6Account = new NEP6Account
            {
                Address = account.Address,
                Label = null,
                IsDefault = Accounts.Count == 0,
                Lock = false,
                Key = NEP2.Encrypt(keyPair.PrivateKeyBytes, password, Scrypt),
                Contract = NEP6Contract.CreateSingleSigContract(keyPair.PublicKey.ToECPoint()),
                Extra = null
            };

            Accounts.Add(nep6Account);
            return nep6Account;
        }

        /// <summary>
        /// Imports an account from a WIF private key.
        /// </summary>
        /// <param name="wif">The WIF private key.</param>
        /// <param name="password">The password to encrypt the private key.</param>
        /// <returns>The imported account.</returns>
        public NEP6Account ImportAccount(string wif, string password)
        {
            var privateKey = ECPrivateKey.FromWIF(wif);
            var keyPair = new ECKeyPair(privateKey);
            var account = new Account(keyPair);
            var nep6Account = new NEP6Account
            {
                Address = account.Address,
                Label = null,
                IsDefault = Accounts.Count == 0,
                Lock = false,
                Key = NEP2.Encrypt(keyPair.PrivateKeyBytes, password, Scrypt),
                Contract = NEP6Contract.CreateSingleSigContract(keyPair.PublicKey.ToECPoint()),
                Extra = null
            };

            Accounts.Add(nep6Account);
            return nep6Account;
        }

        /// <summary>
        /// Gets an account by address.
        /// </summary>
        /// <param name="address">The account address.</param>
        /// <returns>The account or null if not found.</returns>
        public NEP6Account GetAccount(string address)
        {
            return Accounts.FirstOrDefault(a => a.Address == address);
        }

        /// <summary>
        /// Removes an account by address.
        /// </summary>
        /// <param name="address">The account address.</param>
        /// <returns>True if the account was removed, false otherwise.</returns>
        public bool RemoveAccount(string address)
        {
            var account = GetAccount(address);
            if (account != null)
            {
                Accounts.Remove(account);
                
                // If this was the default account and there are other accounts,
                // make the first one default
                if (account.IsDefault && Accounts.Count > 0)
                {
                    Accounts[0].IsDefault = true;
                }
                
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the default account.
        /// </summary>
        /// <returns>The default account or null if no default account.</returns>
        public NEP6Account GetDefaultAccount()
        {
            return Accounts.FirstOrDefault(a => a.IsDefault);
        }

        /// <summary>
        /// Sets the default account.
        /// </summary>
        /// <param name="address">The address of the account to make default.</param>
        /// <returns>True if successful, false if account not found.</returns>
        public bool SetDefaultAccount(string address)
        {
            var account = GetAccount(address);
            if (account == null) return false;

            // Clear current default
            foreach (var acc in Accounts)
            {
                acc.IsDefault = false;
            }

            // Set new default
            account.IsDefault = true;
            return true;
        }

        /// <summary>
        /// Decrypts an account's private key.
        /// </summary>
        /// <param name="address">The account address.</param>
        /// <param name="password">The password to decrypt with.</param>
        /// <returns>The account or null if decryption fails.</returns>
        public Account DecryptAccount(string address, string password)
        {
            var nep6Account = GetAccount(address);
            if (nep6Account == null) return null;

            try
            {
                var keyPair = NEP2.Decrypt(nep6Account.Key, password, Scrypt);
                return new Account(keyPair);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the wallet to a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads a wallet from a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The loaded wallet.</returns>
        public static NEP6Wallet Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<NEP6Wallet>(json);
        }

        /// <summary>
        /// Creates a new wallet and saves it to a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="name">The wallet name.</param>
        /// <param name="password">The password for the default account.</param>
        /// <param name="scrypt">The scrypt parameters.</param>
        /// <returns>The created wallet.</returns>
        public static NEP6Wallet Create(string path, string name, string password, ScryptParams scrypt = null)
        {
            var wallet = new NEP6Wallet(name, scrypt);
            wallet.CreateAccount(password);
            wallet.Save(path);
            return wallet;
        }
    }
}