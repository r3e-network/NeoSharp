using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NeoSharp.Protocol.Core.Response;
using NeoSharp.Protocol.Models;
using NeoSharp.Types;

namespace NeoSharp.Protocol
{
    /// <summary>
    /// Interface for NeoSharp client
    /// </summary>
    public interface INeoSharp
    {
        #region Blockchain Methods
        
        /// <summary>
        /// Gets the hash of the tallest block in the main chain.
        /// </summary>
        Task<Hash256> GetBestBlockHashAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the hash of a specific block by its index.
        /// </summary>
        Task<Hash256> GetBlockHashAsync(int blockIndex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets block information by block hash.
        /// </summary>
        Task<NeoBlock> GetBlockAsync(Hash256 blockHash, bool returnFullTransactionObjects = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets block information by block index.
        /// </summary>
        Task<NeoBlock> GetBlockAsync(int blockIndex, bool returnFullTransactionObjects = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the raw block data by block hash.
        /// </summary>
        Task<string> GetRawBlockAsync(Hash256 blockHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the raw block data by block index.
        /// </summary>
        Task<string> GetRawBlockAsync(int blockIndex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the number of block headers in the blockchain.
        /// </summary>
        Task<int> GetBlockHeaderCountAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the number of blocks in the blockchain.
        /// </summary>
        Task<int> GetBlockCountAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets block header information by block hash.
        /// </summary>
        Task<NeoBlock> GetBlockHeaderAsync(Hash256 blockHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets block header information by block index.
        /// </summary>
        Task<NeoBlock> GetBlockHeaderAsync(int blockIndex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the raw block header data by block hash.
        /// </summary>
        Task<string> GetRawBlockHeaderAsync(Hash256 blockHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the raw block header data by block index.
        /// </summary>
        Task<string> GetRawBlockHeaderAsync(int blockIndex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets information about native contracts.
        /// </summary>
        Task<IList<NativeContractState>> GetNativeContractsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the contract state by contract hash.
        /// </summary>
        Task<ContractState> GetContractStateAsync(Hash160 contractHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the native contract state by name.
        /// </summary>
        Task<ContractState> GetNativeContractStateAsync(string contractName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current memory pool details.
        /// </summary>
        Task<NeoGetMemPool.MemPoolDetails> GetMemPoolAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the list of transaction hashes in the memory pool.
        /// </summary>
        Task<IList<Hash256>> GetRawMemPoolAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a transaction by its hash.
        /// </summary>
        Task<Transaction.Transaction> GetTransactionAsync(Hash256 txHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the raw transaction data by transaction hash.
        /// </summary>
        Task<string> GetRawTransactionAsync(Hash256 txHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the value stored in a contract's storage.
        /// </summary>
        Task<string> GetStorageAsync(Hash160 contractHash, string keyHexString, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the block height in which a transaction was included.
        /// </summary>
        Task<int> GetTransactionHeightAsync(Hash256 txHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the validators for the next block.
        /// </summary>
        Task<IList<NeoGetNextBlockValidators.Validator>> GetNextBlockValidatorsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the list of current committee members.
        /// </summary>
        Task<IList<string>> GetCommitteeAsync(CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Node Methods
        
        /// <summary>
        /// Gets the current number of connections for the node.
        /// </summary>
        Task<int> GetConnectionCountAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the list of nodes that this node is connected to.
        /// </summary>
        Task<NeoGetPeers.Peers> GetPeersAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the version information about the node.
        /// </summary>
        Task<NeoGetVersion.NeoVersion> GetVersionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sends a raw transaction to the network.
        /// </summary>
        Task<NeoSendRawTransaction.RawTransaction> SendRawTransactionAsync(string rawTransactionHex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Submits a new block to the network.
        /// </summary>
        Task<bool> SubmitBlockAsync(string serializedBlockAsHex, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region SmartContract Methods
        
        /// <summary>
        /// Invokes a smart contract function without parameters.
        /// </summary>
        Task<InvocationResult> InvokeFunctionAsync(Hash160 contractHash, string functionName, IList<Signer> signers = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invokes a smart contract function with parameters.
        /// </summary>
        Task<InvocationResult> InvokeFunctionAsync(Hash160 contractHash, string functionName, IList<ContractParameter> parameters, IList<Signer> signers = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invokes a smart contract function with diagnostics.
        /// </summary>
        Task<InvocationResult> InvokeFunctionDiagnosticsAsync(Hash160 contractHash, string functionName, IList<Signer> signers = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invokes a smart contract function with parameters and diagnostics.
        /// </summary>
        Task<InvocationResult> InvokeFunctionDiagnosticsAsync(Hash160 contractHash, string functionName, IList<ContractParameter> parameters, IList<Signer> signers = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invokes a script given its hex representation.
        /// </summary>
        Task<InvocationResult> InvokeScriptAsync(string scriptHex, IList<Signer> signers = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invokes a script with diagnostics.
        /// </summary>
        Task<InvocationResult> InvokeScriptDiagnosticsAsync(string scriptHex, IList<Signer> signers = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Traverses an iterator.
        /// </summary>
        Task<IList<StackItem>> TraverseIteratorAsync(string sessionId, string iteratorId, int count, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Terminates a session.
        /// </summary>
        Task<bool> TerminateSessionAsync(string sessionId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invokes a contract verification method.
        /// </summary>
        Task<InvocationResult> InvokeContractVerifyAsync(Hash160 contractHash, IList<ContractParameter> methodParameters = null, IList<Signer> signers = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the amount of unclaimed GAS for an address.
        /// </summary>
        Task<NeoGetUnclaimedGas.GetUnclaimedGas> GetUnclaimedGasAsync(Hash160 scriptHash, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Utilities Methods
        
        /// <summary>
        /// Gets the list of loaded plugins.
        /// </summary>
        Task<IList<NeoListPlugins.Plugin>> ListPluginsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validates an address.
        /// </summary>
        Task<NeoValidateAddress.Result> ValidateAddressAsync(string address, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Wallet Methods
        
        /// <summary>
        /// Closes the currently opened wallet.
        /// </summary>
        Task<bool> CloseWalletAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Opens a wallet file.
        /// </summary>
        Task<bool> OpenWalletAsync(string walletPath, string password, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Exports the private key of an address.
        /// </summary>
        Task<string> DumpPrivKeyAsync(Hash160 scriptHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the balance of a specific asset.
        /// </summary>
        Task<NeoGetWalletBalance.Balance> GetWalletBalanceAsync(Hash160 tokenHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new address in the currently opened wallet.
        /// </summary>
        Task<string> GetNewAddressAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the amount of unclaimed GAS in the wallet.
        /// </summary>
        Task<string> GetWalletUnclaimedGasAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Imports a private key to the currently opened wallet.
        /// </summary>
        Task<NeoAddress> ImportPrivKeyAsync(string privateKeyInWIF, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calculates the network fee for a transaction.
        /// </summary>
        Task<NeoNetworkFee> CalculateNetworkFeeAsync(string transactionHex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Lists all addresses in the currently opened wallet.
        /// </summary>
        Task<IList<NeoAddress>> ListAddressAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Transfers an asset from one address to another.
        /// </summary>
        Task<Transaction.Transaction> SendFromAsync(Hash160 tokenHash, Hash160 from, Hash160 to, long amount, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Transfers an asset from one address using TransactionSendToken.
        /// </summary>
        Task<Transaction.Transaction> SendFromAsync(Hash160 from, TransactionSendToken txSendToken, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sends assets to multiple addresses.
        /// </summary>
        Task<Transaction.Transaction> SendManyAsync(IList<TransactionSendToken> txSendTokens, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sends assets to multiple addresses from a specific address.
        /// </summary>
        Task<Transaction.Transaction> SendManyAsync(Hash160 from, IList<TransactionSendToken> txSendTokens, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Transfers an asset to an address.
        /// </summary>
        Task<Transaction.Transaction> SendToAddressAsync(Hash160 tokenHash, Hash160 to, long amount, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Transfers an asset to an address using TransactionSendToken.
        /// </summary>
        Task<Transaction.Transaction> SendToAddressAsync(TransactionSendToken txSendToken, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region TokenTracker
        
        /// <summary>
        /// Gets the NEP-17 token balances of an address.
        /// </summary>
        Task<NeoGetNep17Balances.Nep17Balances> GetNep17BalancesAsync(Hash160 scriptHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the NEP-17 transfer history of an address.
        /// </summary>
        Task<NeoGetNep17Transfers.Nep17Transfers> GetNep17TransfersAsync(Hash160 scriptHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the NEP-17 transfer history of an address from a specific date.
        /// </summary>
        Task<NeoGetNep17Transfers.Nep17Transfers> GetNep17TransfersAsync(Hash160 scriptHash, DateTime from, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the NEP-17 transfer history of an address between two dates.
        /// </summary>
        Task<NeoGetNep17Transfers.Nep17Transfers> GetNep17TransfersAsync(Hash160 scriptHash, DateTime from, DateTime to, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the NEP-11 token balances of an address.
        /// </summary>
        Task<NeoGetNep11Balances.Nep11Balances> GetNep11BalancesAsync(Hash160 scriptHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the NEP-11 transfer history of an address.
        /// </summary>
        Task<NeoGetNep11Transfers.Nep11Transfers> GetNep11TransfersAsync(Hash160 scriptHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the NEP-11 transfer history of an address from a specific date.
        /// </summary>
        Task<NeoGetNep11Transfers.Nep11Transfers> GetNep11TransfersAsync(Hash160 scriptHash, DateTime from, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the NEP-11 transfer history of an address between two dates.
        /// </summary>
        Task<NeoGetNep11Transfers.Nep11Transfers> GetNep11TransfersAsync(Hash160 scriptHash, DateTime from, DateTime to, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the properties of a NEP-11 token.
        /// </summary>
        Task<IDictionary<string, string>> GetNep11PropertiesAsync(Hash160 scriptHash, string tokenId, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region ApplicationLogs
        
        /// <summary>
        /// Gets the application log for a transaction.
        /// </summary>
        Task<NeoApplicationLog> GetApplicationLogAsync(Hash256 txHash, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region StateService
        
        /// <summary>
        /// Gets the state root by block index.
        /// </summary>
        Task<NeoGetStateRoot.StateRoot> GetStateRootAsync(int blockIndex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a proof for storage key.
        /// </summary>
        Task<string> GetProofAsync(Hash256 rootHash, Hash160 contractHash, string storageKeyHex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifies a proof.
        /// </summary>
        Task<string> VerifyProofAsync(Hash256 rootHash, string proofDataHex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the state height.
        /// </summary>
        Task<NeoGetStateHeight.StateHeight> GetStateHeightAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the state value.
        /// </summary>
        Task<string> GetStateAsync(Hash256 rootHash, Hash160 contractHash, string keyHex, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Finds states by prefix.
        /// </summary>
        Task<NeoFindStates.States> FindStatesAsync(Hash256 rootHash, Hash160 contractHash, string keyPrefixHex, string startKeyHex = null, int? countFindResultItems = null, CancellationToken cancellationToken = default);
        
        #endregion
    }
}