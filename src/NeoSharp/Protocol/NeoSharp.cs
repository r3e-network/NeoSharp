using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NeoSharp.Protocol.Core.Response;
using NeoSharp.Protocol.Http;
using NeoSharp.Protocol.Models;
using NeoSharp.Types;

namespace NeoSharp.Protocol
{
    /// <summary>
    /// Main NeoSharp client implementation.
    /// </summary>
    public class NeoSharp : INeoSharp, IDisposable
    {
        private readonly IJsonRpcClient _rpcClient;
        private readonly NeoSharpConfig _config;
        private readonly ILogger<NeoSharp> _logger;
        private readonly bool _ownsRpcClient;
        private bool _disposed;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public NeoSharpConfig Config => _config;

        /// <summary>
        /// Gets the NNS resolver hash.
        /// </summary>
        public Hash160 NnsResolver => _config.NnsResolver;

        /// <summary>
        /// Gets the block interval in milliseconds.
        /// </summary>
        public int BlockInterval => _config.BlockInterval;

        /// <summary>
        /// Gets the polling interval in milliseconds.
        /// </summary>
        public int PollingInterval => _config.PollingInterval;

        /// <summary>
        /// Gets the maximum valid until block increment.
        /// </summary>
        public int MaxValidUntilBlockIncrement => _config.MaxValidUntilBlockIncrement;

        /// <summary>
        /// Initializes a new instance of the NeoSharp class.
        /// </summary>
        /// <param name="url">The RPC endpoint URL.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClient">The HTTP client to reuse.</param>
        public NeoSharp(string url, NeoSharpConfig? config = null, ILogger<NeoSharp>? logger = null, HttpClient? httpClient = null)
            : this(new HttpService(url, httpClient, logger ?? NullLogger<NeoSharp>.Instance), config, logger, ownsRpcClient: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NeoSharp class backed by the provided transport.
        /// </summary>
        /// <param name="transport">The JSON-RPC transport to use.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public NeoSharp(IJsonRpcClient transport, NeoSharpConfig? config = null, ILogger<NeoSharp>? logger = null)
            : this(transport, config, logger, ownsRpcClient: false)
        {
        }

        private NeoSharp(IJsonRpcClient transport, NeoSharpConfig? config, ILogger<NeoSharp>? logger, bool ownsRpcClient)
        {
            _rpcClient = transport ?? throw new ArgumentNullException(nameof(transport));
            _config = config ?? new NeoSharpConfig();
            _logger = logger ?? NullLogger<NeoSharp>.Instance;
            _ownsRpcClient = ownsRpcClient;
        }

        /// <summary>
        /// Allows transmission of scripts that lead to a FAULT VM state.
        /// </summary>
        public void AllowTransmissionOnFault()
        {
            _config.EnableTransmissionOnFault();
        }

        #region Blockchain Methods

        public async Task<Hash256> GetBestBlockHashAsync(CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<string>("getbestblockhash", null, cancellationToken);
            return Hash256.Parse(result);
        }

        public async Task<Hash256> GetBlockHashAsync(int blockIndex, CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<string>("getblockhash", new object[] { blockIndex }, cancellationToken);
            return Hash256.Parse(result);
        }

        public async Task<NeoBlock> GetBlockAsync(Hash256 blockHash, bool returnFullTransactionObjects = false, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoBlock>("getblock", new object[] { blockHash.ToString(), returnFullTransactionObjects }, cancellationToken);
        }

        public async Task<NeoBlock> GetBlockAsync(int blockIndex, bool returnFullTransactionObjects = false, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoBlock>("getblock", new object[] { blockIndex, returnFullTransactionObjects }, cancellationToken);
        }

        public async Task<string> GetRawBlockAsync(Hash256 blockHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getblock", new object[] { blockHash.ToString(), false }, cancellationToken);
        }

        public async Task<string> GetRawBlockAsync(int blockIndex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getblock", new object[] { blockIndex, false }, cancellationToken);
        }

        public async Task<int> GetBlockHeaderCountAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<int>("getblockheadercount", null, cancellationToken);
        }

        public async Task<int> GetBlockCountAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<int>("getblockcount", null, cancellationToken);
        }

        public async Task<NeoBlock> GetBlockHeaderAsync(Hash256 blockHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoBlock>("getblockheader", new object[] { blockHash.ToString(), true }, cancellationToken);
        }

        public async Task<NeoBlock> GetBlockHeaderAsync(int blockIndex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoBlock>("getblockheader", new object[] { blockIndex, true }, cancellationToken);
        }

        public async Task<string> GetRawBlockHeaderAsync(Hash256 blockHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getblockheader", new object[] { blockHash.ToString(), false }, cancellationToken);
        }

        public async Task<string> GetRawBlockHeaderAsync(int blockIndex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getblockheader", new object[] { blockIndex, false }, cancellationToken);
        }

        public async Task<IList<NativeContractState>> GetNativeContractsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<List<NativeContractState>>("getnativecontracts", null, cancellationToken);
            return result;
        }

        public async Task<ContractState> GetContractStateAsync(Hash160 contractHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<ContractState>("getcontractstate", new object[] { contractHash.ToString() }, cancellationToken);
        }

        public async Task<ContractState> GetNativeContractStateAsync(string contractName, CancellationToken cancellationToken = default)
        {
            var nativeContracts = await GetNativeContractsAsync(cancellationToken);
            var nativeContract = nativeContracts.FirstOrDefault(c => c.Manifest?.Name == contractName);
            if (nativeContract == null)
            {
                throw new InvalidOperationException($"Native contract '{contractName}' not found");
            }
            return await GetContractStateAsync(nativeContract.Hash, cancellationToken);
        }

        public async Task<NeoGetMemPool.MemPoolDetails> GetMemPoolAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetMemPool.MemPoolDetails>("getrawmempool", new object[] { true }, cancellationToken);
        }

        public async Task<IList<Hash256>> GetRawMemPoolAsync(CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<List<string>>("getrawmempool", null, cancellationToken);
            return result.Select(Hash256.Parse).ToList();
        }

        public async Task<Transaction.Transaction> GetTransactionAsync(Hash256 txHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<Transaction.Transaction>("getrawtransaction", new object[] { txHash.ToString(), true }, cancellationToken);
        }

        public async Task<string> GetRawTransactionAsync(Hash256 txHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getrawtransaction", new object[] { txHash.ToString(), false }, cancellationToken);
        }

        public async Task<string> GetStorageAsync(Hash160 contractHash, string keyHexString, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getstorage", new object[] { contractHash.ToString(), keyHexString }, cancellationToken);
        }

        public async Task<int> GetTransactionHeightAsync(Hash256 txHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<int>("gettransactionheight", new object[] { txHash.ToString() }, cancellationToken);
        }

        public async Task<IList<NeoGetNextBlockValidators.Validator>> GetNextBlockValidatorsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<List<NeoGetNextBlockValidators.Validator>>("getnextblockvalidators", null, cancellationToken);
            return result;
        }

        public async Task<IList<string>> GetCommitteeAsync(CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<List<string>>("getcommittee", null, cancellationToken);
            return result;
        }

        #endregion

        #region Node Methods

        public async Task<int> GetConnectionCountAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<int>("getconnectioncount", null, cancellationToken);
        }

        public async Task<NeoGetPeers.Peers> GetPeersAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetPeers.Peers>("getpeers", null, cancellationToken);
        }

        public async Task<NeoGetVersion.NeoVersion> GetVersionAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetVersion.NeoVersion>("getversion", null, cancellationToken);
        }

        public async Task<NeoSendRawTransaction.RawTransaction> SendRawTransactionAsync(string rawTransactionHex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoSendRawTransaction.RawTransaction>("sendrawtransaction", new object[] { rawTransactionHex }, cancellationToken);
        }

        public async Task<bool> SubmitBlockAsync(string serializedBlockAsHex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<bool>("submitblock", new object[] { serializedBlockAsHex }, cancellationToken);
        }

        #endregion

        #region SmartContract Methods

        public async Task<InvocationResult> InvokeFunctionAsync(Hash160 contractHash, string functionName, IList<Signer>? signers = null, CancellationToken cancellationToken = default)
        {
            return await InvokeFunctionAsync(contractHash, functionName, new List<ContractParameter>(), signers, cancellationToken);
        }

        public async Task<InvocationResult> InvokeFunctionAsync(Hash160 contractHash, string functionName, IList<ContractParameter>? parameters, IList<Signer>? signers = null, CancellationToken cancellationToken = default)
        {
            var rpcParams = new List<object> { contractHash.ToString(), functionName };
            
            rpcParams.Add(parameters?.Select(p => p.ToJson()).ToList() ?? new List<Dictionary<string, object?>>());
            
            if (signers != null && signers.Any())
            {
                rpcParams.Add(signers.Select(s => s.ToJson()).ToList());
            }

            return await _rpcClient.SendAsync<InvocationResult>("invokefunction", rpcParams.ToArray(), cancellationToken);
        }

        public async Task<InvocationResult> InvokeFunctionDiagnosticsAsync(Hash160 contractHash, string functionName, IList<Signer>? signers = null, CancellationToken cancellationToken = default)
        {
            return await InvokeFunctionDiagnosticsAsync(contractHash, functionName, new List<ContractParameter>(), signers, cancellationToken);
        }

        public async Task<InvocationResult> InvokeFunctionDiagnosticsAsync(Hash160 contractHash, string functionName, IList<ContractParameter>? parameters, IList<Signer>? signers = null, CancellationToken cancellationToken = default)
        {
            var rpcParams = new List<object> { contractHash.ToString(), functionName };
            
            rpcParams.Add(parameters?.Select(p => p.ToJson()).ToList() ?? new List<Dictionary<string, object?>>());
            
            if (signers != null && signers.Any())
            {
                rpcParams.Add(signers.Select(s => s.ToJson()).ToList());
            }
            
            rpcParams.Add(true); // Enable diagnostics

            return await _rpcClient.SendAsync<InvocationResult>("invokefunction", rpcParams.ToArray(), cancellationToken);
        }

        public async Task<InvocationResult> InvokeScriptAsync(string scriptHex, IList<Signer>? signers = null, CancellationToken cancellationToken = default)
        {
            var rpcParams = new List<object> { scriptHex };
            
            if (signers != null && signers.Any())
            {
                rpcParams.Add(signers.Select(s => s.ToJson()).ToList());
            }

            return await _rpcClient.SendAsync<InvocationResult>("invokescript", rpcParams.ToArray(), cancellationToken);
        }

        public async Task<InvocationResult> InvokeScriptDiagnosticsAsync(string scriptHex, IList<Signer>? signers = null, CancellationToken cancellationToken = default)
        {
            var rpcParams = new List<object> { scriptHex };
            
            if (signers != null && signers.Any())
            {
                rpcParams.Add(signers.Select(s => s.ToJson()).ToList());
            }
            
            rpcParams.Add(true); // Enable diagnostics

            return await _rpcClient.SendAsync<InvocationResult>("invokescript", rpcParams.ToArray(), cancellationToken);
        }

        public async Task<IList<StackItem>> TraverseIteratorAsync(string sessionId, string iteratorId, int count, CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<List<StackItem>>("traverseiterator", new object[] { sessionId, iteratorId, count }, cancellationToken);
            return result;
        }

        public async Task<bool> TerminateSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<bool>("terminatesession", new object[] { sessionId }, cancellationToken);
        }

        public async Task<InvocationResult> InvokeContractVerifyAsync(Hash160 contractHash, IList<ContractParameter>? methodParameters = null, IList<Signer>? signers = null, CancellationToken cancellationToken = default)
        {
            var rpcParams = new List<object> { contractHash.ToString() };
            
            rpcParams.Add(methodParameters?.Select(p => p.ToJson()).ToList() ?? new List<Dictionary<string, object?>>());
            
            if (signers != null && signers.Any())
            {
                rpcParams.Add(signers.Select(s => s.ToJson()).ToList());
            }

            return await _rpcClient.SendAsync<InvocationResult>("invokecontractverify", rpcParams.ToArray(), cancellationToken);
        }

        public async Task<NeoGetUnclaimedGas.GetUnclaimedGas> GetUnclaimedGasAsync(Hash160 scriptHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetUnclaimedGas.GetUnclaimedGas>("getunclaimedgas", new object[] { scriptHash.ToString() }, cancellationToken);
        }

        #endregion

        #region Utilities Methods

        public async Task<IList<NeoListPlugins.Plugin>> ListPluginsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<List<NeoListPlugins.Plugin>>("listplugins", null, cancellationToken);
            return result;
        }

        public async Task<NeoValidateAddress.Result> ValidateAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoValidateAddress.Result>("validateaddress", new object[] { address }, cancellationToken);
        }

        #endregion

        #region Wallet Methods

        public async Task<bool> CloseWalletAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<bool>("closewallet", null, cancellationToken);
        }

        public async Task<bool> OpenWalletAsync(string walletPath, string password, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<bool>("openwallet", new object[] { walletPath, password }, cancellationToken);
        }

        public async Task<string> DumpPrivKeyAsync(Hash160 scriptHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("dumpprivkey", new object[] { scriptHash.ToString() }, cancellationToken);
        }

        public async Task<NeoGetWalletBalance.Balance> GetWalletBalanceAsync(Hash160 tokenHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetWalletBalance.Balance>("getwalletbalance", new object[] { tokenHash.ToString() }, cancellationToken);
        }

        public async Task<string> GetNewAddressAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getnewaddress", null, cancellationToken);
        }

        public async Task<string> GetWalletUnclaimedGasAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getwalletunclaimedgas", null, cancellationToken);
        }

        public async Task<NeoAddress> ImportPrivKeyAsync(string privateKeyInWIF, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoAddress>("importprivkey", new object[] { privateKeyInWIF }, cancellationToken);
        }

        public async Task<NeoNetworkFee> CalculateNetworkFeeAsync(string transactionHex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoNetworkFee>("calculatenetworkfee", new object[] { transactionHex }, cancellationToken);
        }

        public async Task<IList<NeoAddress>> ListAddressAsync(CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<List<NeoAddress>>("listaddress", null, cancellationToken);
            return result;
        }

        public async Task<Transaction.Transaction> SendFromAsync(Hash160 tokenHash, Hash160 from, Hash160 to, long amount, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<Transaction.Transaction>("sendfrom", new object[] { tokenHash.ToString(), from.ToString(), to.ToString(), amount }, cancellationToken);
        }

        public async Task<Transaction.Transaction> SendFromAsync(Hash160 from, TransactionSendToken txSendToken, CancellationToken cancellationToken = default)
        {
            return await SendFromAsync(txSendToken.Asset, from, Hash160.Parse(txSendToken.Address), long.Parse(txSendToken.Value), cancellationToken);
        }

        public async Task<Transaction.Transaction> SendManyAsync(IList<TransactionSendToken> txSendTokens, CancellationToken cancellationToken = default)
        {
            var transfers = txSendTokens.Select(t => new
            {
                asset = t.Asset.ToString(),
                value = t.Value,
                address = t.Address
            }).ToList();

            return await _rpcClient.SendAsync<Transaction.Transaction>("sendmany", new object[] { transfers }, cancellationToken);
        }

        public async Task<Transaction.Transaction> SendManyAsync(Hash160 from, IList<TransactionSendToken> txSendTokens, CancellationToken cancellationToken = default)
        {
            var transfers = txSendTokens.Select(t => new
            {
                asset = t.Asset.ToString(),
                value = t.Value,
                address = t.Address
            }).ToList();

            return await _rpcClient.SendAsync<Transaction.Transaction>("sendmany", new object[] { from.ToString(), transfers }, cancellationToken);
        }

        public async Task<Transaction.Transaction> SendToAddressAsync(Hash160 tokenHash, Hash160 to, long amount, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<Transaction.Transaction>("sendtoaddress", new object[] { tokenHash.ToString(), to.ToString(), amount }, cancellationToken);
        }

        public async Task<Transaction.Transaction> SendToAddressAsync(TransactionSendToken txSendToken, CancellationToken cancellationToken = default)
        {
            return await SendToAddressAsync(txSendToken.Asset, Hash160.Parse(txSendToken.Address), long.Parse(txSendToken.Value), cancellationToken);
        }

        #endregion

        #region TokenTracker

        public async Task<NeoGetNep17Balances.Nep17Balances> GetNep17BalancesAsync(Hash160 scriptHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetNep17Balances.Nep17Balances>("getnep17balances", new object[] { scriptHash.ToString() }, cancellationToken);
        }

        public async Task<NeoGetNep17Transfers.Nep17Transfers> GetNep17TransfersAsync(Hash160 scriptHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetNep17Transfers.Nep17Transfers>("getnep17transfers", new object[] { scriptHash.ToString() }, cancellationToken);
        }

        public async Task<NeoGetNep17Transfers.Nep17Transfers> GetNep17TransfersAsync(Hash160 scriptHash, DateTime from, CancellationToken cancellationToken = default)
        {
            var fromTimestamp = new DateTimeOffset(from).ToUnixTimeMilliseconds();
            return await _rpcClient.SendAsync<NeoGetNep17Transfers.Nep17Transfers>("getnep17transfers", new object[] { scriptHash.ToString(), fromTimestamp }, cancellationToken);
        }

        public async Task<NeoGetNep17Transfers.Nep17Transfers> GetNep17TransfersAsync(Hash160 scriptHash, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            var fromTimestamp = new DateTimeOffset(from).ToUnixTimeMilliseconds();
            var toTimestamp = new DateTimeOffset(to).ToUnixTimeMilliseconds();
            return await _rpcClient.SendAsync<NeoGetNep17Transfers.Nep17Transfers>("getnep17transfers", new object[] { scriptHash.ToString(), fromTimestamp, toTimestamp }, cancellationToken);
        }

        public async Task<NeoGetNep11Balances.Nep11Balances> GetNep11BalancesAsync(Hash160 scriptHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetNep11Balances.Nep11Balances>("getnep11balances", new object[] { scriptHash.ToString() }, cancellationToken);
        }

        public async Task<NeoGetNep11Transfers.Nep11Transfers> GetNep11TransfersAsync(Hash160 scriptHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetNep11Transfers.Nep11Transfers>("getnep11transfers", new object[] { scriptHash.ToString() }, cancellationToken);
        }

        public async Task<NeoGetNep11Transfers.Nep11Transfers> GetNep11TransfersAsync(Hash160 scriptHash, DateTime from, CancellationToken cancellationToken = default)
        {
            var fromTimestamp = new DateTimeOffset(from).ToUnixTimeMilliseconds();
            return await _rpcClient.SendAsync<NeoGetNep11Transfers.Nep11Transfers>("getnep11transfers", new object[] { scriptHash.ToString(), fromTimestamp }, cancellationToken);
        }

        public async Task<NeoGetNep11Transfers.Nep11Transfers> GetNep11TransfersAsync(Hash160 scriptHash, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            var fromTimestamp = new DateTimeOffset(from).ToUnixTimeMilliseconds();
            var toTimestamp = new DateTimeOffset(to).ToUnixTimeMilliseconds();
            return await _rpcClient.SendAsync<NeoGetNep11Transfers.Nep11Transfers>("getnep11transfers", new object[] { scriptHash.ToString(), fromTimestamp, toTimestamp }, cancellationToken);
        }

        public async Task<IDictionary<string, string>> GetNep11PropertiesAsync(Hash160 scriptHash, string tokenId, CancellationToken cancellationToken = default)
        {
            var result = await _rpcClient.SendAsync<Dictionary<string, string>>("getnep11properties", new object[] { scriptHash.ToString(), tokenId }, cancellationToken);
            return result;
        }

        #endregion

        #region ApplicationLogs

        public async Task<NeoApplicationLog> GetApplicationLogAsync(Hash256 txHash, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoApplicationLog>("getapplicationlog", new object[] { txHash.ToString() }, cancellationToken);
        }

        #endregion

        #region StateService

        public async Task<NeoGetStateRoot.StateRoot> GetStateRootAsync(int blockIndex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetStateRoot.StateRoot>("getstateroot", new object[] { blockIndex }, cancellationToken);
        }

        public async Task<string> GetProofAsync(Hash256 rootHash, Hash160 contractHash, string storageKeyHex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getproof", new object[] { rootHash.ToString(), contractHash.ToString(), storageKeyHex }, cancellationToken);
        }

        public async Task<string> VerifyProofAsync(Hash256 rootHash, string proofDataHex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("verifyproof", new object[] { rootHash.ToString(), proofDataHex }, cancellationToken);
        }

        public async Task<NeoGetStateHeight.StateHeight> GetStateHeightAsync(CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<NeoGetStateHeight.StateHeight>("getstateheight", null, cancellationToken);
        }

        public async Task<string> GetStateAsync(Hash256 rootHash, Hash160 contractHash, string keyHex, CancellationToken cancellationToken = default)
        {
            return await _rpcClient.SendAsync<string>("getstate", new object[] { rootHash.ToString(), contractHash.ToString(), keyHex }, cancellationToken);
        }

        public async Task<NeoFindStates.States> FindStatesAsync(Hash256 rootHash, Hash160 contractHash, string keyPrefixHex, string? startKeyHex = null, int? countFindResultItems = null, CancellationToken cancellationToken = default)
        {
            var rpcParams = new List<object> { rootHash.ToString(), contractHash.ToString(), keyPrefixHex };
            
            if (!string.IsNullOrEmpty(startKeyHex))
            {
                rpcParams.Add(startKeyHex);
                
                if (countFindResultItems.HasValue)
                {
                    rpcParams.Add(countFindResultItems.Value);
                }
            }

            return await _rpcClient.SendAsync<NeoFindStates.States>("findstates", rpcParams.ToArray(), cancellationToken);
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing && _ownsRpcClient)
            {
                _rpcClient.Dispose();
            }

            _disposed = true;
        }
    }
}
