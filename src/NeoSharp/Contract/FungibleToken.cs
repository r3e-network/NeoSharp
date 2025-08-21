using System;
using System.Linq;
using System.Threading.Tasks;
using NeoSharp.Protocol;
using NeoSharp.Protocol.Core.Response;
using NeoSharp.Transaction;
using NeoSharp.Types;
using NeoSharp.Wallet;

namespace NeoSharp.Contract
{
    /// <summary>
    /// Represents a fungible token contract that is compliant with the NEP-17 standard
    /// </summary>
    public class FungibleToken : Token
    {
        private const string BalanceOfMethod = "balanceOf";
        private const string TransferMethod = "transfer";

        /// <summary>
        /// Initializes a new FungibleToken instance
        /// </summary>
        /// <param name="scriptHash">The token contract's script hash</param>
        /// <param name="neoSharp">The NeoSharp instance for invocations</param>
        public FungibleToken(Hash160 scriptHash, INeoSharp neoSharp) : base(scriptHash, neoSharp)
        {
        }

        /// <summary>
        /// Gets the token balance for the given account
        /// The token amount is returned in token fractions. E.g., an amount of 1 GAS is returned as 1*10^8 GAS fractions.
        /// The balance is not cached locally. Every time this method is called requests are sent to the Neo node.
        /// </summary>
        /// <param name="account">The account to fetch the balance for</param>
        /// <returns>The token balance in fractions</returns>
        public virtual async Task<long> GetBalanceOfAsync(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
                
            var scriptHash = account.GetScriptHash();
            if (scriptHash == null)
                throw new ArgumentException("Account must have a valid script hash", nameof(account));
            return await GetBalanceOfAsync(scriptHash.Value);
        }

        /// <summary>
        /// Gets the token balance for the given account script hash
        /// The token amount is returned in token fractions. E.g., an amount of 1 GAS is returned as 1*10^8 GAS fractions.
        /// The balance is not cached locally. Every time this method is called requests are sent to the Neo node.
        /// </summary>
        /// <param name="scriptHash">The script hash to fetch the balance for</param>
        /// <returns>The token balance in fractions</returns>
        public virtual async Task<long> GetBalanceOfAsync(Hash160 scriptHash)
        {
            if (scriptHash == null)
                throw new ArgumentNullException(nameof(scriptHash));

            var balance = await CallFunctionReturningIntAsync(BalanceOfMethod, ContractParameter.Hash160(scriptHash));
            return balance;
        }

        /// <summary>
        /// Gets the token balance for all accounts in the given wallet
        /// The token amount is returned in token fractions. E.g., an amount of 1 GAS is returned as 1*10^8 GAS fractions.
        /// The balance is not cached locally. Every time this method is called requests are sent to the Neo node.
        /// </summary>
        /// <param name="wallet">The wallet to fetch the balance for</param>
        /// <returns>The total token balance across all accounts</returns>
        public virtual async Task<long> GetBalanceOfAsync(Wallet.Wallet wallet)
        {
            if (wallet == null)
                throw new ArgumentNullException(nameof(wallet));

            long totalBalance = 0;
            foreach (var account in wallet.Accounts)
            {
                totalBalance += await GetBalanceOfAsync(account);
            }
            return totalBalance;
        }

        /// <summary>
        /// Creates a transfer transaction with the sender account set as a signer
        /// Only use this method when the recipient is a deployed smart contract to avoid unnecessary additional fees.
        /// Otherwise, use the method without a contract parameter for data.
        /// </summary>
        /// <param name="from">The sender account</param>
        /// <param name="to">The script hash of the recipient</param>
        /// <param name="amount">The amount to transfer in token fractions</param>
        /// <param name="data">Optional data passed to the onPayment method if recipient is a contract</param>
        /// <returns>A transaction builder ready for signing</returns>
        public virtual TransactionBuilder Transfer(Account from, Hash160 to, long amount, ContractParameter? data = null)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            var fromScriptHash = from.GetScriptHash();
            if (fromScriptHash == null)
                throw new ArgumentException("From account must have a valid script hash", nameof(from));
            return Transfer(fromScriptHash.Value, to, amount, data)
                .AddSigner(new AccountSigner(from));
        }

        /// <summary>
        /// Creates a transfer transaction without setting signers
        /// No signers are set on the returned transaction builder. It is up to you to set the correct ones,
        /// e.g., a ContractSigner in case the 'from' address is a contract.
        /// </summary>
        /// <param name="from">The script hash of the sender</param>
        /// <param name="to">The script hash of the recipient</param>
        /// <param name="amount">The amount to transfer in token fractions</param>
        /// <param name="data">Optional data passed to the onPayment method if recipient is a contract</param>
        /// <returns>A transaction builder ready for signing</returns>
        public virtual TransactionBuilder Transfer(Hash160 from, Hash160 to, long amount, ContractParameter? data = null)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            if (amount < 0)
                throw new ArgumentException("The amount must be greater than or equal to 0", nameof(amount));

            var transferScript = BuildTransferScript(from, to, amount, data);
            return new TransactionBuilder(NeoSharp).AddScript(transferScript);
        }

        /// <summary>
        /// Builds a script that invokes the transfer method on the fungible token
        /// </summary>
        /// <param name="from">The sender script hash</param>
        /// <param name="to">The recipient script hash</param>
        /// <param name="amount">The transfer amount in token fractions</param>
        /// <param name="data">Optional data passed to the onPayment method if recipient is a contract</param>
        /// <returns>The transfer script</returns>
        public virtual byte[] BuildTransferScript(Hash160 from, Hash160 to, long amount, ContractParameter? data = null)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            var parameters = new ContractParameter[]
            {
                ContractParameter.Hash160(from),
                ContractParameter.Hash160(to),
                ContractParameter.Integer(amount),
                data ?? ContractParameter.Any(null)
            };

            return BuildInvokeFunctionScript(TransferMethod, parameters);
        }

        /// <summary>
        /// Creates a transfer transaction using NNS domain name resolution
        /// Resolves the text record of the recipient's NNS domain name. The resolved value is expected to be a valid Neo address.
        /// The sender account is set as a signer of the transaction.
        /// </summary>
        /// <param name="from">The sender account</param>
        /// <param name="to">The NNS domain name to resolve</param>
        /// <param name="amount">The amount to transfer in token fractions</param>
        /// <param name="data">Optional data passed to the onPayment method if recipient is a contract</param>
        /// <returns>A transaction builder ready for signing</returns>
        public virtual async Task<TransactionBuilder> TransferAsync(Account from, NnsName to, long amount, ContractParameter? data = null)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            var fromScriptHash = from.GetScriptHash();
            if (fromScriptHash == null)
                throw new ArgumentException("From account must have a valid script hash", nameof(from));
            var result = await TransferAsync(fromScriptHash.Value, to, amount, data);
            return result.AddSigner(new AccountSigner(from));
        }

        /// <summary>
        /// Creates a transfer transaction using NNS domain name resolution without setting signers
        /// No signers are set on the returned transaction builder. It is up to you to set the correct ones,
        /// e.g., a ContractSigner in case the 'from' address is a contract.
        /// </summary>
        /// <param name="from">The sender script hash</param>
        /// <param name="to">The NNS domain name to resolve</param>
        /// <param name="amount">The amount to transfer in token fractions</param>
        /// <param name="data">Optional data passed to the onPayment method if recipient is a contract</param>
        /// <returns>A transaction builder ready for signing</returns>
        public virtual async Task<TransactionBuilder> TransferAsync(Hash160 from, NnsName to, long amount, ContractParameter? data = null)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            var resolvedHash = await ResolveNnsTextRecordAsync(to);
            return Transfer(from, resolvedHash, amount, data);
        }
    }
}