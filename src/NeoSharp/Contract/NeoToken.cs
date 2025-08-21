using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeoSharp.Core;
using NeoSharp.Crypto;
using NeoSharp.Protocol.Responses;
using NeoSharp.Protocol.Core.Response;
using NeoSharp.Protocol;
using NeoSharp.Protocol.Models;
using NeoSharp.Transaction;
using NeoSharp.Utils;
using NeoSharp.Types;
using NeoSharp.Wallet;

namespace NeoSharp.Contract
{
    /// <summary>
    /// Represents the NeoToken native contract and provides methods to invoke its functions
    /// </summary>
    public class NeoToken : FungibleToken
    {
        /// <summary>
        /// The contract name
        /// </summary>
        public const string Name = "NeoToken";

        /// <summary>
        /// The contract script hash
        /// </summary>
        public static new readonly Hash160 ScriptHash = CalculateNativeContractHash(Name);

        /// <summary>
        /// The number of decimal places
        /// </summary>
        public const int Decimals = 0;

        /// <summary>
        /// The token symbol
        /// </summary>
        public const string Symbol = "NEO";

        /// <summary>
        /// The total supply
        /// </summary>
        public const long TotalSupply = 100_000_000L;

        // Method names
        private const string UnclaimedGasMethod = "unclaimedGas";
        private const string RegisterCandidateMethod = "registerCandidate";
        private const string UnregisterCandidateMethod = "unregisterCandidate";
        private const string VoteMethod = "vote";
        private const string GetCandidatesMethod = "getCandidates";
        private const string GetAllCandidatesMethod = "getAllCandidates";
        private const string GetCandidateVoteMethod = "getCandidateVote";
        private const string GetCommitteeMethod = "getCommittee";
        private const string GetNextBlockValidatorsMethod = "getNextBlockValidators";
        private const string SetGasPerBlockMethod = "setGasPerBlock";
        private const string GetGasPerBlockMethod = "getGasPerBlock";
        private const string SetRegisterPriceMethod = "setRegisterPrice";
        private const string GetRegisterPriceMethod = "getRegisterPrice";
        private const string GetAccountStateMethod = "getAccountState";

        /// <summary>
        /// Initializes a new NeoToken instance
        /// </summary>
        /// <param name="neoSharp">The NeoSharp instance for invocations</param>
        public NeoToken(INeoSharp neoSharp) : base(ScriptHash, neoSharp)
        {
        }

        /// <summary>
        /// Returns the name of the NEO token
        /// Doesn't require a call to the Neo node
        /// </summary>
        /// <returns>The token name</returns>
        public override async Task<string?> GetNameAsync()
        {
            await Task.CompletedTask; // For async consistency
            return Name;
        }

        /// <summary>
        /// Returns the symbol of the NEO token
        /// Doesn't require a call to the Neo node
        /// </summary>
        /// <returns>The token symbol</returns>
        public override async Task<string> GetSymbolAsync()
        {
            await Task.CompletedTask; // For async consistency
            return Symbol;
        }

        /// <summary>
        /// Returns the total supply of the NEO token
        /// Doesn't require a call to the Neo node
        /// </summary>
        /// <returns>The total supply</returns>
        public override async Task<int> GetTotalSupplyAsync()
        {
            await Task.CompletedTask; // For async consistency
            return (int)TotalSupply;
        }

        /// <summary>
        /// Returns the number of decimals of the NEO token
        /// Doesn't require a call to the Neo node
        /// </summary>
        /// <returns>The number of decimals</returns>
        public override async Task<int> GetDecimalsAsync()
        {
            await Task.CompletedTask; // For async consistency
            return Decimals;
        }

        #region Unclaimed Gas

        /// <summary>
        /// Gets the amount of unclaimed GAS at the given height for the given account
        /// </summary>
        /// <param name="account">The account</param>
        /// <param name="blockHeight">The block height</param>
        /// <returns>The amount of unclaimed GAS</returns>
        public async Task<long> GetUnclaimedGasAsync(Account account, int blockHeight)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
                
            var scriptHash = account.GetScriptHash();
            if (scriptHash == null)
                throw new ArgumentException("Account must have a valid script hash", nameof(account));
            return await GetUnclaimedGasAsync(scriptHash.Value, blockHeight);
        }

        /// <summary>
        /// Gets the amount of unclaimed GAS at the given height for the given script hash
        /// </summary>
        /// <param name="scriptHash">The account's script hash</param>
        /// <param name="blockHeight">The block height</param>
        /// <returns>The amount of unclaimed GAS</returns>
        public async Task<long> GetUnclaimedGasAsync(Hash160 scriptHash, int blockHeight)
        {
            if (scriptHash == null)
                throw new ArgumentNullException(nameof(scriptHash));

            var result = await CallFunctionReturningIntAsync(UnclaimedGasMethod,
                ContractParameter.Hash160(scriptHash),
                ContractParameter.Integer(blockHeight));
            
            return result;
        }

        #endregion

        #region Candidate Registration

        /// <summary>
        /// Creates a transaction script for registering a candidate with the given public key
        /// Note that the transaction has to be signed with the account corresponding to the public key
        /// </summary>
        /// <param name="candidateKey">The public key to register as a candidate</param>
        /// <returns>A transaction builder</returns>
        public TransactionBuilder RegisterCandidate(ECPublicKey candidateKey)
        {
            if (candidateKey == null)
                throw new ArgumentNullException(nameof(candidateKey));

            return InvokeFunction(RegisterCandidateMethod,
                ContractParameter.PublicKey(candidateKey.GetEncoded(true)));
        }

        /// <summary>
        /// Creates a transaction script for unregistering a candidate with the given public key
        /// </summary>
        /// <param name="candidateKey">The public key to unregister as a candidate</param>
        /// <returns>A transaction builder</returns>
        public TransactionBuilder UnregisterCandidate(ECPublicKey candidateKey)
        {
            if (candidateKey == null)
                throw new ArgumentNullException(nameof(candidateKey));

            return InvokeFunction(UnregisterCandidateMethod,
                ContractParameter.PublicKey(candidateKey.GetEncoded(true)));
        }

        #endregion

        #region Committee and Candidates Information

        /// <summary>
        /// Gets the public keys of the current committee members
        /// </summary>
        /// <returns>The committee members' public keys</returns>
        public async Task<List<ECPublicKey>> GetCommitteeAsync()
        {
            return await CallFunctionReturningListOfPublicKeysAsync(GetCommitteeMethod);
        }

        /// <summary>
        /// Gets the public keys of the registered candidates and their corresponding vote count
        /// Note that this method returns at max 256 candidates. Use GetAllCandidatesIteratorAsync to traverse all candidates if there are more than 256.
        /// </summary>
        /// <returns>The candidates</returns>
        public async Task<List<Candidate>> GetCandidatesAsync()
        {
            var result = await CallInvokeFunctionAsync(GetCandidatesMethod, Array.Empty<ContractParameter>());
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            var arrayItem = invocationResult.GetFirstStackItem();
            if (!arrayItem.IsArray)
                throw ContractException.UnexpectedReturnType(arrayItem.Type, "Array");

            var list = arrayItem.GetList();
            if (list == null)
                return new List<Candidate>();

            return list.Select(CandidateMapper).ToList();
        }

        /// <summary>
        /// Checks if there is a candidate with the provided public key
        /// Note that this only checks the first 256 candidates. Use GetAllCandidatesIteratorAsync to traverse all candidates if there are more than 256.
        /// </summary>
        /// <param name="publicKey">The candidate's public key</param>
        /// <returns>True if the public key belongs to a candidate, otherwise false</returns>
        public async Task<bool> IsCandidateAsync(ECPublicKey publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));

            var candidates = await GetCandidatesAsync();
            return candidates.Any(c => c.PublicKey.Equals(publicKey));
        }

        /// <summary>
        /// Gets an iterator of all registered candidates
        /// Use the method Iterator.TraverseAsync to traverse the iterator and retrieve all candidates
        /// </summary>
        /// <returns>An iterator of all registered candidates</returns>
        public async Task<Iterator<Candidate>> GetAllCandidatesIteratorAsync()
        {
            return await CallFunctionReturningIteratorAsync<Candidate>(
                GetAllCandidatesMethod,
                Array.Empty<ContractParameter>(),
                CandidateMapper);
        }

        /// <summary>
        /// Maps a stack item to a Candidate object
        /// </summary>
        /// <param name="stackItem">The stack item to map</param>
        /// <returns>The mapped candidate</returns>
        private static Candidate CandidateMapper(StackItem stackItem)
        {
            var list = stackItem.GetList();
            if (list == null || list.Count < 2)
                throw new InvalidOperationException("Invalid candidate stack item format");

            var publicKeyBytes = list[0].GetByteArray();
            var votes = list[1].GetInteger();

            if (publicKeyBytes == null)
                throw new InvalidOperationException("Public key bytes are null");

            var publicKey = new ECPublicKey(publicKeyBytes);
            return new Candidate(publicKey, votes);
        }

        /// <summary>
        /// Gets the votes for a specific candidate
        /// </summary>
        /// <param name="publicKey">The candidate's public key</param>
        /// <returns>The candidate's votes, or -1 if not found</returns>
        public async Task<int> GetCandidateVotesAsync(ECPublicKey publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));

            return await CallFunctionReturningIntAsync(GetCandidateVoteMethod,
                ContractParameter.PublicKey(publicKey));
        }

        /// <summary>
        /// Gets the public keys of the next block's validators
        /// </summary>
        /// <returns>The validators' public keys</returns>
        public async Task<List<ECPublicKey>> GetNextBlockValidatorsAsync()
        {
            return await CallFunctionReturningListOfPublicKeysAsync(GetNextBlockValidatorsMethod);
        }

        /// <summary>
        /// Calls a function that returns a list of public keys
        /// </summary>
        /// <param name="function">The function name</param>
        /// <returns>The list of public keys</returns>
        private async Task<List<ECPublicKey>> CallFunctionReturningListOfPublicKeysAsync(string function)
        {
            var result = await CallInvokeFunctionAsync(function, Array.Empty<ContractParameter>());
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);
            
            var arrayItem = invocationResult.GetFirstStackItem();
            if (!arrayItem.IsArray)
                throw ContractException.UnexpectedReturnType(arrayItem.Type, "Array");

            var list = arrayItem.GetList();
            if (list == null)
                return new List<ECPublicKey>();

            return list.Select(ExtractPublicKey).ToList();
        }

        /// <summary>
        /// Extracts a public key from a stack item
        /// </summary>
        /// <param name="keyItem">The stack item containing the public key</param>
        /// <returns>The extracted public key</returns>
        private static ECPublicKey ExtractPublicKey(StackItem keyItem)
        {
            if (!keyItem.IsByteString)
                throw ContractException.UnexpectedReturnType(keyItem.Type, "ByteString");

            try
            {
                var keyBytes = keyItem.GetByteArray();
                if (keyBytes == null)
                    throw new InvalidOperationException("Public key bytes are null");
                    
                return new ECPublicKey(keyBytes);
            }
            catch (Exception ex)
            {
                throw new ContractException("Byte array return type did not contain public key in expected format", ex);
            }
        }

        #endregion

        #region Voting

        /// <summary>
        /// Creates a transaction script to vote for the given candidate
        /// </summary>
        /// <param name="voter">The account that casts the vote</param>
        /// <param name="candidate">The candidate to vote for. If null, then the current vote of the voter is withdrawn</param>
        /// <returns>A transaction builder</returns>
        public async Task<TransactionBuilder> VoteAsync(Account voter, ECPublicKey? candidate)
        {
            if (voter == null)
                throw new ArgumentNullException(nameof(voter));

            var voterScriptHash = voter.GetScriptHash();
            if (voterScriptHash == null)
                throw new ArgumentException("Voter account must have a valid script hash", nameof(voter));
            return await VoteAsync(voterScriptHash.Value, candidate);
        }

        /// <summary>
        /// Creates a transaction script to vote for the given candidate
        /// </summary>
        /// <param name="voter">The account script hash that casts the vote</param>
        /// <param name="candidate">The candidate to vote for. If null, then the current vote of the voter is withdrawn</param>
        /// <returns>A transaction builder</returns>
        public async Task<TransactionBuilder> VoteAsync(Hash160 voter, ECPublicKey? candidate)
        {
            if (voter == null)
                throw new ArgumentNullException(nameof(voter));

            await Task.CompletedTask; // For async consistency

            if (candidate == null)
            {
                return InvokeFunction(VoteMethod,
                    ContractParameter.Hash160(voter),
                    ContractParameter.Any(null));
            }

            return InvokeFunction(VoteMethod,
                ContractParameter.Hash160(voter),
                ContractParameter.PublicKey(candidate.GetEncoded(true)));
        }

        /// <summary>
        /// Creates a transaction script to cancel the vote of the voter
        /// </summary>
        /// <param name="voter">The account for which to cancel the vote</param>
        /// <returns>A transaction builder</returns>
        public async Task<TransactionBuilder> CancelVoteAsync(Account voter)
        {
            if (voter == null)
                throw new ArgumentNullException(nameof(voter));

            var voterScriptHash = voter.GetScriptHash();
            if (voterScriptHash == null)
                throw new ArgumentException("Voter account must have a valid script hash", nameof(voter));
            return await CancelVoteAsync(voterScriptHash.Value);
        }

        /// <summary>
        /// Creates a transaction script to cancel the vote of the voter
        /// </summary>
        /// <param name="voter">The account script hash for which to cancel the vote</param>
        /// <returns>A transaction builder</returns>
        public async Task<TransactionBuilder> CancelVoteAsync(Hash160 voter)
        {
            return await VoteAsync(voter, null);
        }

        /// <summary>
        /// Builds a script to vote for a candidate
        /// </summary>
        /// <param name="voter">The account that casts the vote</param>
        /// <param name="candidate">The candidate to vote for. If null, then the current vote is withdrawn</param>
        /// <returns>The voting script</returns>
        public byte[] BuildVoteScript(Hash160 voter, ECPublicKey? candidate)
        {
            if (voter == null)
                throw new ArgumentNullException(nameof(voter));

            if (candidate == null)
            {
                return BuildInvokeFunctionScript(VoteMethod,
                    ContractParameter.Hash160(voter),
                    ContractParameter.Any(null));
            }

            return BuildInvokeFunctionScript(VoteMethod,
                ContractParameter.Hash160(voter),
                ContractParameter.PublicKey(candidate.GetEncoded(true)));
        }

        #endregion

        #region Network Settings

        /// <summary>
        /// Gets the number of GAS generated in each block
        /// </summary>
        /// <returns>The max GAS amount per block</returns>
        public async Task<int> GetGasPerBlockAsync()
        {
            return await CallFunctionReturningIntAsync(GetGasPerBlockMethod);
        }

        /// <summary>
        /// Creates a transaction script to set the number of GAS generated in each block
        /// This contract invocation can only be successful if it is signed by the network committee
        /// </summary>
        /// <param name="gasPerBlock">The maximum amount of GAS in one block</param>
        /// <returns>The transaction builder</returns>
        public TransactionBuilder SetGasPerBlock(int gasPerBlock)
        {
            return InvokeFunction(SetGasPerBlockMethod,
                ContractParameter.Integer(gasPerBlock));
        }

        /// <summary>
        /// Gets the price to register as a candidate
        /// </summary>
        /// <returns>The price to register as a candidate</returns>
        public async Task<int> GetRegisterPriceAsync()
        {
            return await CallFunctionReturningIntAsync(GetRegisterPriceMethod);
        }

        /// <summary>
        /// Creates a transaction script to set the price for candidate registration
        /// This contract invocation can only be successful if it is signed by the network committee
        /// </summary>
        /// <param name="registerPrice">The price to register as a candidate</param>
        /// <returns>The transaction builder</returns>
        public TransactionBuilder SetRegisterPrice(int registerPrice)
        {
            return InvokeFunction(SetRegisterPriceMethod,
                ContractParameter.Integer(registerPrice));
        }

        /// <summary>
        /// Gets the state of an account
        /// </summary>
        /// <param name="accountHash">The account script hash to get the state from</param>
        /// <returns>The account state</returns>
        public async Task<Protocol.Models.NeoAccountState> GetAccountStateAsync(Hash160 accountHash)
        {
            if (accountHash == null)
                throw new ArgumentNullException(nameof(accountHash));

            var result = await CallInvokeFunctionAsync(GetAccountStateMethod,
                new[] { ContractParameter.Hash160(accountHash) });
            
            var invocationResult = result.GetResult();
            ThrowIfFaultState(invocationResult);

            var stackItem = invocationResult.Stack.FirstOrDefault();
            if (stackItem == null)
                throw new NeoSharpException("Account State stack was empty");

            // Handle null state (no balance)
            if (stackItem.IsAny)
                return Protocol.Models.NeoAccountState.WithNoBalance(Hash160.Zero);

            var stateList = stackItem.GetList();
            if (stateList == null || stateList.Count() < 3)
                throw new NeoSharpException("Account State stack was malformed");

            var balance = stateList[0].GetInteger();
            var updateHeight = stateList[1].GetInteger();
            var publicKeyItem = stateList[2];

            // Handle no vote case
            if (publicKeyItem.IsAny)
                return Protocol.Models.NeoAccountState.WithNoVote(Hash160.Zero, balance, updateHeight);

            // Handle voted case
            var publicKeyHex = publicKeyItem.GetHexString();
            if (publicKeyHex == null)
                throw new NeoSharpException("Public key hex string is null");
                
            var publicKeyBytes = publicKeyHex.HexToBytes();
            return new Protocol.Models.NeoAccountState(Hash160.Zero, balance, updateHeight) { VoteTo = publicKeyBytes };
        }

        #endregion

        /// <summary>
        /// Represents the state of a candidate
        /// </summary>
        public class Candidate : IEquatable<Candidate>
        {
            /// <summary>
            /// The candidate's public key
            /// </summary>
            public ECPublicKey PublicKey { get; }

            /// <summary>
            /// The candidate's votes based on the summed up NEO balances of voters
            /// </summary>
            public int Votes { get; }

            /// <summary>
            /// Initializes a new Candidate instance
            /// </summary>
            /// <param name="publicKey">The candidate's public key</param>
            /// <param name="votes">The candidate's vote count</param>
            public Candidate(ECPublicKey publicKey, int votes)
            {
                PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
                Votes = votes;
            }

            /// <summary>
            /// Determines whether this candidate equals another
            /// </summary>
            /// <param name="other">The other candidate to compare</param>
            /// <returns>True if the candidates are equal</returns>
            public bool Equals(Candidate? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return PublicKey.Equals(other.PublicKey) && Votes == other.Votes;
            }

            /// <summary>
            /// Determines whether this candidate equals another object
            /// </summary>
            /// <param name="obj">The object to compare</param>
            /// <returns>True if the objects are equal</returns>
            public override bool Equals(object? obj) => Equals(obj as Candidate);

            /// <summary>
            /// Gets the hash code for this candidate
            /// </summary>
            /// <returns>The hash code</returns>
            public override int GetHashCode() => HashCode.Combine(PublicKey, Votes);

            /// <summary>
            /// Returns a string representation of this candidate
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString() => $"Candidate(PublicKey={PublicKey}, Votes={Votes})";

            public static bool operator ==(Candidate? left, Candidate? right) => 
                EqualityComparer<Candidate>.Default.Equals(left, right);

            public static bool operator !=(Candidate? left, Candidate? right) => 
                !(left == right);
        }
    }
}