using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeoSharp.Core;
using NeoSharp.Protocol;
using NeoSharp.Protocol.Core.Response;
using NeoSharp.Protocol.Responses;
using NeoSharp.Script;
using NeoSharp.Types;
using NeoSharp.Transaction;
using NeoSharp.Utils;

namespace NeoSharp.Contract
{
    /// <summary>
    /// Represents a smart contract on the Neo blockchain and provides methods to invoke and deploy it
    /// </summary>
    public class SmartContract
    {
        /// <summary>
        /// Default number of items to retrieve when iterating
        /// </summary>
        public const int DefaultIteratorCount = 100;

        /// <summary>
        /// The script hash of this smart contract
        /// </summary>
        protected readonly Hash160 ScriptHash;

        /// <summary>
        /// The NeoSharp instance used for contract invocations
        /// </summary>
        protected readonly INeoSharp NeoSharp;

        /// <summary>
        /// Initializes a new SmartContract instance
        /// </summary>
        /// <param name="scriptHash">The smart contract's script hash</param>
        /// <param name="neoSharp">The NeoSharp instance for invocations</param>
        public SmartContract(Hash160 scriptHash, INeoSharp neoSharp)
        {
            ScriptHash = scriptHash;
            NeoSharp = neoSharp ?? throw new ArgumentNullException(nameof(neoSharp));
        }

        /// <summary>
        /// Initializes a TransactionBuilder for invoking this contract with the specified function and parameters
        /// </summary>
        /// <param name="function">The function to invoke</param>
        /// <param name="parameters">The parameters to pass with the invocation</param>
        /// <returns>A transaction builder for setting further details</returns>
        /// <exception cref="ArgumentException">Thrown when function name is empty</exception>
        public virtual TransactionBuilder InvokeFunction(string function, params ContractParameter?[] parameters)
        {
            if (string.IsNullOrEmpty(function))
                throw new ArgumentException("The invocation function must not be empty", nameof(function));

            var script = BuildInvokeFunctionScript(function, parameters);
            return new TransactionBuilder(NeoSharp).AddScript(script);
        }

        /// <summary>
        /// Builds a script to invoke a function on this smart contract
        /// </summary>
        /// <param name="function">The function to invoke</param>
        /// <param name="parameters">The parameters to pass with the invocation</param>
        /// <returns>The invocation script</returns>
        /// <exception cref="ArgumentException">Thrown when function name is empty</exception>
        public virtual byte[] BuildInvokeFunctionScript(string function, params ContractParameter?[] parameters)
        {
            if (string.IsNullOrEmpty(function))
                throw new ArgumentException("The invocation function must not be empty", nameof(function));

            return new ScriptBuilder()
                .ContractCall(ScriptHash, function, parameters)
                .ToArray();
        }

        /// <summary>
        /// Sends an invokefunction RPC call expecting a string return type
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <returns>The string returned by the contract</returns>
        public virtual async Task<string> CallFunctionReturningStringAsync(string function, params ContractParameter[] parameters)
        {
            var result = await CallInvokeFunctionAsync(function, parameters);
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            var stackItem = invocationResult.GetFirstStackItem();
            if (!stackItem.IsByteString)
                throw ContractException.UnexpectedReturnType(stackItem.Type, "ByteString");

            return stackItem.GetString() ?? throw new InvalidOperationException("String value is null");
        }

        /// <summary>
        /// Sends an invokefunction RPC call expecting an integer return type
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <returns>The integer returned by the contract</returns>
        public virtual async Task<int> CallFunctionReturningIntAsync(string function, params ContractParameter[] parameters)
        {
            var result = await CallInvokeFunctionAsync(function, parameters);
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            var stackItem = invocationResult.GetFirstStackItem();
            if (!stackItem.IsInteger)
                throw ContractException.UnexpectedReturnType(stackItem.Type, "Integer");

            return stackItem.GetInteger();
        }

        /// <summary>
        /// Sends an invokefunction RPC call expecting a boolean return type
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <returns>The boolean returned by the contract</returns>
        public virtual async Task<bool> CallFunctionReturningBoolAsync(string function, params ContractParameter[] parameters)
        {
            var result = await CallInvokeFunctionAsync(function, parameters);
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            var stackItem = invocationResult.GetFirstStackItem();
            return stackItem switch
            {
                _ when stackItem.IsBoolean => stackItem.GetBoolean(),
                _ when stackItem.IsInteger => stackItem.GetInteger() != 0,
                _ when stackItem.IsByteString => stackItem.GetByteArray()?.Length > 0,
                _ when stackItem.IsBuffer() => stackItem.GetByteArray()?.Length > 0,
                _ => throw ContractException.UnexpectedReturnType(stackItem.Type, "Boolean", "Integer", "ByteString", "Buffer")
            };
        }

        /// <summary>
        /// Sends an invokefunction RPC call expecting a script hash return type
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <returns>The script hash returned by the contract</returns>
        public virtual async Task<Hash160> CallFunctionReturningScriptHashAsync(string function, params ContractParameter[] parameters)
        {
            var result = await CallInvokeFunctionAsync(function, parameters);
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            return ExtractScriptHash(invocationResult.Stack.First());
        }

        /// <summary>
        /// Extracts a script hash from a stack item
        /// </summary>
        /// <param name="item">The stack item</param>
        /// <returns>The extracted script hash</returns>
        /// <exception cref="ContractException">Thrown when the item format is invalid</exception>
        private Hash160 ExtractScriptHash(StackItem item)
        {
            if (!item.IsByteString)
                throw ContractException.UnexpectedReturnType(item.Type, "ByteString");

            try
            {
                var hex = item.GetHexString();
                if (hex == null)
                    throw new InvalidOperationException("Hex string is null");
                    
                return new Hash160(hex.ReverseHex());
            }
            catch (Exception ex)
            {
                throw new ContractException($"Return type did not contain script hash in expected format: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends an invokefunction RPC call expecting an iterator return type
        /// </summary>
        /// <typeparam name="T">The type to map iterator items to</typeparam>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <param name="mapper">The function to map stack items</param>
        /// <returns>The iterator</returns>
        public virtual async Task<Iterator<T>> CallFunctionReturningIteratorAsync<T>(
            string function, 
            ContractParameter[] parameters,
            Func<StackItem, T> mapper)
        {
            var result = await CallInvokeFunctionAsync(function, parameters);
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            var stackItem = invocationResult.GetFirstStackItem();
            if (!stackItem.IsInteropInterface())
                throw ContractException.UnexpectedReturnType(stackItem.Type, "InteropInterface");

            var sessionId = invocationResult.SessionId;
            if (sessionId == null)
                throw new NeoSharpException("No session id was found. The connected Neo node might not support sessions.");

            var iteratorId = stackItem.GetIteratorId();
            if (iteratorId == null)
                throw new NeoSharpException("No iterator id was found in the stack item.");

            return new Iterator<T>(NeoSharp, sessionId, iteratorId, mapper);
        }

        /// <summary>
        /// Sends an invokefunction RPC call and traverses the returned iterator
        /// </summary>
        /// <typeparam name="T">The type to map iterator items to</typeparam>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <param name="maxItems">The maximum number of items to return</param>
        /// <param name="mapper">The function to map stack items</param>
        /// <returns>The mapped iterator items</returns>
        public virtual async Task<List<T>> CallFunctionAndTraverseIteratorAsync<T>(
            string function,
            ContractParameter[] parameters,
            int maxItems = DefaultIteratorCount,
            Func<StackItem, T>? mapper = null)
        {
            mapper ??= item => (T)(object)item;
            
            var iterator = await CallFunctionReturningIteratorAsync(function, parameters, mapper);
            var items = await iterator.TraverseAsync(maxItems);
            await iterator.TerminateSessionAsync();
            return items;
        }

        /// <summary>
        /// Calls a function and unwraps the returned iterator on the NeoVM
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <param name="maxItems">The maximum number of items to return</param>
        /// <param name="signers">The signers for this request</param>
        /// <returns>The unwrapped stack items</returns>
        public virtual async Task<List<StackItem>> CallFunctionAndUnwrapIteratorAsync(
            string function,
            ContractParameter[] parameters,
            int maxItems,
            params Signer[] signers)
        {
            var script = ScriptBuilder.BuildContractCallAndUnwrapIterator(ScriptHash, function, parameters, maxItems);
            var result = await NeoSharp.InvokeScriptAsync(HexExtensions.ToHexString(script), signers);
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            return invocationResult.Stack.FirstOrDefault()?.GetList() ?? new List<StackItem>();
        }

        /// <summary>
        /// Sends an invokefunction RPC call to the contract
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="parameters">The contract parameters</param>
        /// <param name="signers">The signers for this request</param>
        /// <returns>The call response</returns>
        /// <exception cref="ArgumentException">Thrown when function name is empty</exception>
        public virtual async Task<InvokeFunctionResponse> CallInvokeFunctionAsync(
            string function,
            ContractParameter[] parameters,
            params Signer[] signers)
        {
            if (string.IsNullOrEmpty(function))
                throw new ArgumentException("The invocation function must not be empty", nameof(function));

            var result = await NeoSharp.InvokeFunctionAsync(ScriptHash, function, parameters, signers);
            
            // Convert InvocationResult to InvokeFunctionResponse
            return new InvokeFunctionResponse
            {
                State = result.State,
                GasConsumed = result.GasConsumed,
                Stack = result.Stack,
                Exception = result.Exception,
                Tx = result.Tx,
                SessionId = result.SessionId
            };
        }

        /// <summary>
        /// Invokes a function on this smart contract (alias for CallInvokeFunctionAsync)
        /// </summary>
        /// <param name="function">The function to invoke</param>
        /// <param name="parameters">The parameters to pass with the invocation</param>
        /// <param name="signers">The signers to use</param>
        /// <returns>The invocation result</returns>
        public virtual async Task<InvokeFunctionResponse> CallFunctionAsync(string function, object[] parameters, params Signer[] signers)
        {
            // Convert object array to ContractParameter array
            var contractParameters = parameters?.Select(p => ContractParameter.FromObject(p)).ToArray() ?? new ContractParameter[0];
            return await CallInvokeFunctionAsync(function, contractParameters, signers);
        }

        /// <summary>
        /// Throws an exception if the invocation result has a fault state
        /// </summary>
        /// <param name="invocationResult">The invocation result to check</param>
        /// <exception cref="ProtocolException">Thrown when the result has a fault state</exception>
        protected virtual void ThrowIfFaultState(InvocationResult invocationResult)
        {
            if (invocationResult.HasStateFault)
            {
                throw new ProtocolException($"Invocation fault state: {invocationResult.Exception}");
            }
        }

        /// <summary>
        /// Gets the manifest of this smart contract
        /// </summary>
        /// <returns>The contract manifest</returns>
        public virtual async Task<ContractManifest> GetManifestAsync()
        {
            var contractState = await NeoSharp.GetContractStateAsync(ScriptHash);
            return contractState.Manifest;
        }

        /// <summary>
        /// Gets the name of this smart contract
        /// </summary>
        /// <returns>The contract name</returns>
        public virtual async Task<string?> GetNameAsync()
        {
            var manifest = await GetManifestAsync();
            return manifest.Name;
        }

        /// <summary>
        /// Calculates the hash of a native contract
        /// </summary>
        /// <param name="contractName">The contract name</param>
        /// <returns>The contract hash</returns>
        public static Hash160 CalculateNativeContractHash(string contractName)
        {
            return CalculateContractHash(Hash160.Zero, 0, contractName);
        }

        /// <summary>
        /// Calculates the hash of a contract deployed by the specified sender
        /// </summary>
        /// <param name="sender">The sender of the deployment transaction</param>
        /// <param name="nefChecksum">The checksum of the contract's NEF file</param>
        /// <param name="contractName">The contract's name</param>
        /// <returns>The contract hash</returns>
        public static Hash160 CalculateContractHash(Hash160 sender, int nefChecksum, string contractName)
        {
            var script = ScriptBuilder.BuildContractHashScript(sender, nefChecksum, contractName);
            return Hash160.FromScript(script);
        }
    }
}