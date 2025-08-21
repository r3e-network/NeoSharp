using System;
using System.Linq;
using System.Threading.Tasks;
using NeoSharp.Crypto;
using NeoSharp.Types;
using NeoSharp.Script;
using NeoSharp.Wallet.NEP6;
using NeoSharp.Protocol;

namespace NeoSharp.Wallet
{
    /// <summary>
    /// Represents a Neo account.
    /// An account can be a single-signature or multi-signature account.
    /// The latter does not contain EC key material because it is based on multiple EC key pairs.
    /// </summary>
    public class Account : IDisposable
    {
        private ECKeyPair? _keyPair;
        private bool _disposed;
        private bool _isLocked;
        private string? _encryptedPrivateKey;
        private Wallet? _wallet;

        /// <summary>
        /// This account's EC key pair if available. Null if the key pair is not available, e.g., the account was encrypted.
        /// </summary>
        public ECKeyPair? KeyPair => _keyPair;

        /// <summary>
        /// Gets the account address
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Gets or sets the account label
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets the verification script
        /// </summary>
        public VerificationScript? VerificationScript { get; }

        /// <summary>
        /// Gets whether the account is locked
        /// </summary>
        public bool IsLocked => _isLocked;

        /// <summary>
        /// Gets the encrypted private key
        /// </summary>
        public string? EncryptedPrivateKey => _encryptedPrivateKey;

        /// <summary>
        /// Gets or sets the wallet this account belongs to
        /// </summary>
        public Wallet? Wallet
        {
            get => _wallet;
            internal set => _wallet = value;
        }

        /// <summary>
        /// The signing threshold (null if the account is single-sig)
        /// </summary>
        public int? SigningThreshold { get; }

        /// <summary>
        /// The number of involved keys (null if the account is single-sig)
        /// </summary>
        public int? NumberOfParticipants { get; }

        /// <summary>
        /// Gets the script hash for this account
        /// </summary>
        public Hash160? ScriptHash => GetScriptHash();

        /// <summary>
        /// Whether the account is default in the wallet
        /// </summary>
        public bool IsDefault => _wallet?.IsDefault(this) ?? false;

        /// <summary>
        /// True if this account is a multi-sig account. Otherwise false.
        /// </summary>
        public bool IsMultiSig => SigningThreshold.HasValue && NumberOfParticipants.HasValue;

        /// <summary>
        /// Gets whether this account has a private key
        /// </summary>
        public bool HasPrivateKey => _keyPair != null;

        /// <summary>
        /// Constructs a new account with the given EC key pair
        /// </summary>
        /// <param name="keyPair">The key pair of the account</param>
        /// <param name="signingThreshold">The signing threshold (null if the account is single-sig)</param>
        /// <param name="numberOfParticipants">The number of involved keys (null if the account is single-sig)</param>
        public Account(ECKeyPair keyPair, int? signingThreshold = null, int? numberOfParticipants = null)
        {
            _keyPair = keyPair ?? throw new ArgumentNullException(nameof(keyPair));
            Address = keyPair.GetAddress();
            Label = Address;
            VerificationScript = new VerificationScript(keyPair.PublicKey);
            SigningThreshold = signingThreshold;
            NumberOfParticipants = numberOfParticipants;
        }

        /// <summary>
        /// Constructs a new account with address and optional verification script
        /// </summary>
        /// <param name="address">The account address</param>
        /// <param name="label">The account label</param>
        /// <param name="verificationScript">The verification script (optional)</param>
        /// <param name="signingThreshold">The signing threshold (null if single-sig)</param>
        /// <param name="numberOfParticipants">The number of participants (null if single-sig)</param>
        public Account(string address, string? label = null, VerificationScript? verificationScript = null, 
                      int? signingThreshold = null, int? numberOfParticipants = null)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Label = label;
            VerificationScript = verificationScript;
            SigningThreshold = signingThreshold;
            NumberOfParticipants = numberOfParticipants;
        }

        /// <summary>
        /// Internal constructor for full account state
        /// </summary>
        internal Account(ECKeyPair? keyPair, string address, string? label, VerificationScript? verificationScript,
                        bool isLocked, string? encryptedPrivateKey, Wallet? wallet, int? signingThreshold, int? numberOfParticipants)
        {
            _keyPair = keyPair;
            Address = address;
            Label = label;
            VerificationScript = verificationScript;
            _isLocked = isLocked;
            _encryptedPrivateKey = encryptedPrivateKey;
            _wallet = wallet;
            SigningThreshold = signingThreshold;
            NumberOfParticipants = numberOfParticipants;
        }

        /// <summary>
        /// Sets the label for this account
        /// </summary>
        /// <param name="label">The new label</param>
        /// <returns>This account (for method chaining)</returns>
        public Account SetLabel(string label)
        {
            Label = label;
            return this;
        }

        /// <summary>
        /// Sets the wallet for this account
        /// </summary>
        /// <param name="wallet">The wallet</param>
        /// <returns>This account (for method chaining)</returns>
        internal Account SetWallet(Wallet? wallet)
        {
            _wallet = wallet;
            return this;
        }

        /// <summary>
        /// Locks this account
        /// </summary>
        /// <returns>This account (for method chaining)</returns>
        public Account Lock()
        {
            _isLocked = true;
            return this;
        }

        /// <summary>
        /// Unlocks this account
        /// </summary>
        public void Unlock()
        {
            _isLocked = false;
        }

        /// <summary>
        /// Decrypts this account's private key according to the NEP-2 standard if not already decrypted
        /// </summary>
        /// <param name="password">The passphrase used to decrypt this account's private key</param>
        /// <param name="scryptParams">The Scrypt parameters used for decryption (defaults to standard params)</param>
        public void DecryptPrivateKey(string password, NEP6.ScryptParams? scryptParams = null)
        {
            if (_keyPair != null) return;
            
            if (_encryptedPrivateKey == null)
                throw new InvalidOperationException("The account does not hold an encrypted private key.");

            scryptParams ??= NEP6.ScryptParams.Default;
            _keyPair = NEP2.Decrypt(_encryptedPrivateKey, password, scryptParams);
        }

        /// <summary>
        /// Encrypts this account's private key according to the NEP-2 standard
        /// </summary>
        /// <param name="password">The passphrase used to encrypt this account's private key</param>
        /// <param name="scryptParams">The Scrypt parameters used for encryption (defaults to standard params)</param>
        public void EncryptPrivateKey(string password, NEP6.ScryptParams? scryptParams = null)
        {
            if (_keyPair == null)
                throw new InvalidOperationException("The account does not hold a decrypted private key.");

            scryptParams ??= NEP6.ScryptParams.Default;
            _encryptedPrivateKey = NEP2.Encrypt(_keyPair, password, scryptParams);
            _keyPair = null;
        }

        /// <summary>
        /// Gets the script hash for this account
        /// </summary>
        /// <returns>The script hash</returns>
        public Hash160? GetScriptHash()
        {
            try
            {
                return Hash160.FromAddress(Address);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the signing threshold for multi-sig accounts
        /// </summary>
        /// <returns>The signing threshold</returns>
        /// <exception cref="InvalidOperationException">Thrown if account is not multi-sig</exception>
        public int GetSigningThreshold()
        {
            if (!IsMultiSig || !SigningThreshold.HasValue)
                throw new InvalidOperationException($"Cannot get signing threshold from account {Address}, because it is not multi-sig.");
            
            return SigningThreshold.Value;
        }

        /// <summary>
        /// Gets the number of participants for multi-sig accounts
        /// </summary>
        /// <returns>The number of participants</returns>
        /// <exception cref="InvalidOperationException">Thrown if account is not multi-sig</exception>
        public int GetNumberOfParticipants()
        {
            if (!IsMultiSig || !NumberOfParticipants.HasValue)
                throw new InvalidOperationException($"Cannot get number of participants from account {Address}, because it is not multi-sig.");
            
            return NumberOfParticipants.Value;
        }

        /// <summary>
        /// Gets the balances of all NEP-17 tokens that this account owns
        /// </summary>
        /// <param name="neoSharp">The NeoSharp object used to call the neo node</param>
        /// <returns>The map of token script hashes to token amounts</returns>
        public async Task<Dictionary<Hash160, int>> GetNep17BalancesAsync(INeoSharp neoSharp)
        {
            var scriptHash = GetScriptHash();
            if (scriptHash == null)
                throw new InvalidOperationException("Cannot get balances for account without valid script hash");

            var response = await neoSharp.GetNep17BalancesAsync(scriptHash.Value);
            var balances = new Dictionary<Hash160, int>();
            
            if (response?.Balance != null)
            {
                foreach (var balance in response.Balance)
                {
                    if (int.TryParse(balance.Amount, out var amount))
                    {
                        try
                        {
                            var assetHash = new Hash160(balance.AssetHash);
                            balances[assetHash] = amount;
                        }
                        catch
                        {
                            // Skip invalid asset hashes
                        }
                    }
                }
            }
            
            return balances;
        }

        /// <summary>
        /// Converts this account to a NEP-6 account format
        /// </summary>
        /// <returns>The NEP-6 account</returns>
        /// <exception cref="InvalidOperationException">Thrown if private key is available but not encrypted</exception>
        public NEP6Account ToNEP6Account()
        {
            if (_keyPair != null && _encryptedPrivateKey == null)
                throw new InvalidOperationException("Account private key is available but not encrypted.");

            NEP6Contract? contract = null;
            if (VerificationScript != null)
            {
                var parameters = new List<NEP6Parameter>();
                if (VerificationScript.IsMultiSigScript())
                {
                    var nrOfAccounts = VerificationScript.GetNumberOfAccounts();
                    for (int i = 0; i < nrOfAccounts; i++)
                    {
                        parameters.Add(new NEP6Parameter($"signature{i}", "Signature"));
                    }
                }
                else if (VerificationScript.IsSingleSigScript())
                {
                    parameters.Add(new NEP6Parameter("signature", "Signature"));
                }
                
                var script = Convert.ToBase64String(VerificationScript.Script);
                contract = new NEP6Contract
                {
                    Script = script,
                    Parameters = parameters,
                    Deployed = false
                };
            }

            return new NEP6Account
            {
                Address = Address,
                Label = Label,
                IsDefault = IsDefault,
                Lock = IsLocked,
                Key = _encryptedPrivateKey,
                Contract = contract,
                Extra = null
            };
        }

        /// <summary>
        /// Signs data with this account's private key
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <returns>The signature</returns>
        /// <exception cref="InvalidOperationException">Thrown if no private key is available</exception>
        public byte[] Sign(byte[] data)
        {
            if (_keyPair == null)
                throw new InvalidOperationException("Cannot sign with a watch-only account");
            
            return _keyPair.Sign(data);
        }

        /// <summary>
        /// Gets the public key of this account
        /// </summary>
        /// <returns>The public key</returns>
        public ECPublicKey GetPublicKey()
        {
            if (_keyPair == null)
                throw new InvalidOperationException("No public key available");
            return _keyPair.PublicKey;
        }

        /// <summary>
        /// Signs data with the account's private key (alias for Sign)
        /// </summary>
        /// <param name="data">The data to sign</param>
        /// <returns>The signature</returns>
        public byte[] SignData(byte[] data)
        {
            return Sign(data);
        }

        /// <summary>
        /// Gets the verification script for this account
        /// </summary>
        /// <returns>The verification script bytes</returns>
        public byte[] GetVerificationScript()
        {
            if (VerificationScript == null)
                throw new InvalidOperationException("No verification script available");
            
            return VerificationScript.Script;
        }

        #region Static Factory Methods

        /// <summary>
        /// Creates an account from the given verification script
        /// </summary>
        /// <param name="script">The verification script</param>
        /// <returns>The account with a verification script</returns>
        public static Account FromVerificationScript(VerificationScript script)
        {
            var address = Hash160.FromScript(script.Script).ToAddress();
            int? signingThreshold = null, numberOfParticipants = null;
            
            if (script.IsMultiSigScript())
            {
                signingThreshold = script.GetSigningThreshold();
                numberOfParticipants = script.GetNumberOfAccounts();
            }
            
            return new Account(address, address, script, signingThreshold, numberOfParticipants);
        }

        /// <summary>
        /// Creates an account from the given public key
        /// </summary>
        /// <param name="publicKey">The public key</param>
        /// <returns>The account with a verification script</returns>
        public static Account FromPublicKey(ECPublicKey publicKey)
        {
            var script = new VerificationScript(publicKey);
            var address = Hash160.FromScript(script.Script).ToAddress();
            return new Account(address, address, script);
        }

        /// <summary>
        /// Creates a multi-sig account from the given public keys
        /// </summary>
        /// <param name="publicKeys">The public keys</param>
        /// <param name="signingThreshold">The number of signatures needed</param>
        /// <returns>The multi-sig account</returns>
        public static Account CreateMultiSigAccount(ECPublicKey[] publicKeys, int signingThreshold)
        {
            var script = VerificationScript.CreateMultiSigScript(publicKeys, signingThreshold);
            var address = Hash160.FromScript(script.Script).ToAddress();
            return new Account(address, address, script, signingThreshold, publicKeys.Length);
        }

        /// <summary>
        /// Creates a multi-sig account with the given address
        /// </summary>
        /// <param name="address">The address of the multi-sig account</param>
        /// <param name="signingThreshold">The number of signatures needed</param>
        /// <param name="numberOfParticipants">The number of participating accounts</param>
        /// <returns>The multi-sig account</returns>
        public static Account CreateMultiSigAccount(string address, int signingThreshold, int numberOfParticipants)
        {
            return new Account(address, address, null, signingThreshold, numberOfParticipants);
        }

        /// <summary>
        /// Creates an account from the given WIF
        /// </summary>
        /// <param name="wif">The WIF of the account</param>
        /// <returns>The account</returns>
        public static Account FromWIF(string wif)
        {
            var keyPair = ECKeyPair.FromWIF(wif);
            return new Account(keyPair);
        }

        /// <summary>
        /// Creates an account from the provided NEP-6 account
        /// </summary>
        /// <param name="nep6Account">The account in NEP-6 format</param>
        /// <returns>The account</returns>
        public static Account FromNEP6Account(NEP6Account nep6Account)
        {
            VerificationScript? verificationScript = null;
            int? signingThreshold = null, numberOfParticipants = null;
            
            if (nep6Account.Contract?.Script != null && !string.IsNullOrEmpty(nep6Account.Contract.Script))
            {
                var scriptBytes = Convert.FromBase64String(nep6Account.Contract.Script);
                verificationScript = new VerificationScript(scriptBytes);
                
                if (verificationScript.IsMultiSigScript())
                {
                    signingThreshold = verificationScript.GetSigningThreshold();
                    numberOfParticipants = verificationScript.GetNumberOfAccounts();
                }
            }
            
            return new Account(null, nep6Account.Address, nep6Account.Label, verificationScript,
                             nep6Account.Lock, nep6Account.Key, null, signingThreshold, numberOfParticipants);
        }

        /// <summary>
        /// Creates an account from the given address
        /// </summary>
        /// <param name="address">The address of the account</param>
        /// <returns>The account</returns>
        public static Account FromAddress(string address)
        {
            if (!IsValidAddress(address))
                throw new ArgumentException("Invalid address.", nameof(address));
            
            return new Account(address, address);
        }

        /// <summary>
        /// Creates an account from the given script hash
        /// </summary>
        /// <param name="scriptHash">The script hash of the account</param>
        /// <returns>The account</returns>
        public static Account FromScriptHash(Hash160 scriptHash)
        {
            return FromAddress(scriptHash.ToAddress());
        }

        /// <summary>
        /// Creates a new account with a fresh key pair
        /// </summary>
        /// <returns>The new account</returns>
        public static Account Create()
        {
            var keyPair = ECKeyPair.CreateEcKeyPair();
            return new Account(keyPair);
        }

        /// <summary>
        /// Validates if an address is valid
        /// </summary>
        /// <param name="address">The address to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool IsValidAddress(string address)
        {
            try
            {
                Hash160.FromAddress(address);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Disposes the account and clears sensitive data
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _keyPair?.Dispose();
                _keyPair = null;
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Represents a verification script for an account
    /// </summary>
    public class VerificationScript
    {
        /// <summary>
        /// Gets the script bytes
        /// </summary>
        public byte[] Script { get; }

        /// <summary>
        /// Initializes a new verification script from bytes
        /// </summary>
        /// <param name="script">The script bytes</param>
        public VerificationScript(byte[] script)
        {
            Script = script ?? throw new ArgumentNullException(nameof(script));
        }

        /// <summary>
        /// Initializes a new verification script from a public key
        /// </summary>
        /// <param name="publicKey">The public key</param>
        public VerificationScript(ECPublicKey publicKey)
        {
            Script = ScriptBuilder.BuildVerificationScript(publicKey.GetEncoded(true));
        }

        /// <summary>
        /// Creates a multi-sig verification script
        /// </summary>
        /// <param name="publicKeys">The public keys</param>
        /// <param name="signingThreshold">The signing threshold</param>
        /// <returns>The verification script</returns>
        public static VerificationScript CreateMultiSigScript(ECPublicKey[] publicKeys, int signingThreshold)
        {
            var script = ScriptBuilder.BuildVerificationScript(publicKeys, signingThreshold);
            return new VerificationScript(script);
        }

        /// <summary>
        /// Checks if this is a multi-sig script
        /// </summary>
        /// <returns>True if multi-sig, false otherwise</returns>
        public bool IsMultiSigScript()
        {
            // Implementation depends on script analysis
            // For now, return false as a placeholder
            return false;
        }

        /// <summary>
        /// Checks if this is a single-sig script
        /// </summary>
        /// <returns>True if single-sig, false otherwise</returns>
        public bool IsSingleSigScript()
        {
            // Implementation depends on script analysis
            // For now, return true as a placeholder
            return true;
        }

        /// <summary>
        /// Gets the signing threshold for multi-sig scripts
        /// </summary>
        /// <returns>The signing threshold</returns>
        public int GetSigningThreshold()
        {
            // Implementation depends on script analysis
            return 1;
        }

        /// <summary>
        /// Gets the number of accounts in the script
        /// </summary>
        /// <returns>The number of accounts</returns>
        public int GetNumberOfAccounts()
        {
            // Implementation depends on script analysis
            return 1;
        }
    }
}