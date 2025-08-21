using System;
using System.Threading.Tasks;
using NeoSharp.Protocol;
using NeoSharp.Protocol.Core.Response;
using NeoSharp.Types;
using NeoSharp.Utils;

namespace NeoSharp.Contract
{
    /// <summary>
    /// Represents a token wrapper class that contains shared methods for fungible NEP-17 and non-fungible NEP-11 token standards
    /// </summary>
    public class Token : SmartContract
    {
        private const string TotalSupplyMethod = "totalSupply";
        private const string SymbolMethod = "symbol";
        private const string DecimalsMethod = "decimals";

        private int? _totalSupply;
        private int? _decimals;
        private string? _symbol;

        /// <summary>
        /// Initializes a new Token instance
        /// </summary>
        /// <param name="scriptHash">The token contract's script hash</param>
        /// <param name="neoSharp">The NeoSharp instance for invocations</param>
        public Token(Hash160 scriptHash, INeoSharp neoSharp) : base(scriptHash, neoSharp)
        {
        }

        /// <summary>
        /// Gets the total supply of this token in fractions
        /// The return value is retrieved from the neo-node only once and then cached
        /// </summary>
        /// <returns>The total supply</returns>
        public virtual async Task<int> GetTotalSupplyAsync()
        {
            if (_totalSupply.HasValue)
                return _totalSupply.Value;

            _totalSupply = await CallFunctionReturningIntAsync(TotalSupplyMethod);
            return _totalSupply.Value;
        }

        /// <summary>
        /// Gets the number of fractions that one unit of this token can be divided into
        /// The return value is retrieved from the neo-node only once and then cached
        /// </summary>
        /// <returns>The number of decimals</returns>
        public virtual async Task<int> GetDecimalsAsync()
        {
            if (_decimals.HasValue)
                return _decimals.Value;

            _decimals = await CallFunctionReturningIntAsync(DecimalsMethod);
            return _decimals.Value;
        }

        /// <summary>
        /// Gets the symbol of this token
        /// The return value is retrieved from the neo-node only once and then cached
        /// </summary>
        /// <returns>The token symbol</returns>
        public virtual async Task<string> GetSymbolAsync()
        {
            if (_symbol != null)
                return _symbol;

            _symbol = await CallFunctionReturningStringAsync(SymbolMethod);
            return _symbol;
        }

        /// <summary>
        /// Converts the token amount from a decimal number to token fractions according to this token's decimals
        /// Use this method to convert e.g. 1.5 GAS to its fraction value 150,000,000
        /// </summary>
        /// <param name="amount">The token amount in decimals</param>
        /// <returns>The token amount in fractions</returns>
        public async Task<long> ToFractionsAsync(decimal amount)
        {
            var decimals = await GetDecimalsAsync();
            return ToFractions(amount, decimals);
        }

        /// <summary>
        /// Converts the token amount from a decimal number to token fractions according to the specified decimals
        /// Use this method to convert e.g. a token amount of 25.5 for a token with 4 decimals to 255,000
        /// </summary>
        /// <param name="amount">The token amount in decimals</param>
        /// <param name="decimals">The number of decimals</param>
        /// <returns>The token amount in fractions</returns>
        /// <exception cref="ArgumentException">Thrown when amount has more decimal places than supported</exception>
        public static long ToFractions(decimal amount, int decimals)
        {
            // Get number of decimal places in the amount
            var scale = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];
            
            if (scale > decimals)
            {
                throw new ArgumentException(
                    "The provided amount has too many decimal points. " +
                    "Make sure the decimals of the provided amount do not exceed the supported token decimals.",
                    nameof(amount));
            }

            var multiplier = (decimal)Math.Pow(10, decimals);
            return (long)(amount * multiplier);
        }

        /// <summary>
        /// Converts the token amount from token fractions to its decimal value according to this token's decimals
        /// Use this method to convert e.g. 600,000 GAS to its decimal value 0.006
        /// </summary>
        /// <param name="amount">The token amount in fractions</param>
        /// <returns>The token amount in decimals</returns>
        public async Task<decimal> ToDecimalsAsync(long amount)
        {
            var decimals = await GetDecimalsAsync();
            return ToDecimals(amount, decimals);
        }

        /// <summary>
        /// Converts the token amount from token fractions to its decimal value according to the specified decimals
        /// Use this method to convert e.g. 600,000 token fractions to its decimal value
        /// </summary>
        /// <param name="amount">The token amount in fractions</param>
        /// <param name="decimals">The number of decimals</param>
        /// <returns>The token amount in decimals</returns>
        public static decimal ToDecimals(long amount, int decimals)
        {
            var divisor = (decimal)Math.Pow(10, decimals);
            return amount / divisor;
        }

        /// <summary>
        /// Resolves an NNS domain name to a Hash160 address
        /// </summary>
        /// <param name="name">The NNS domain name</param>
        /// <returns>The resolved Hash160 address</returns>
        /// <exception cref="ArgumentNullException">Thrown when name is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when resolution fails</exception>
        protected virtual async Task<Hash160> ResolveNnsTextRecordAsync(NnsName name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            try
            {
                // NNS contract hash on Neo N3 MainNet
                var nnsContractHash = new Hash160("0x50ac1c37690cc2cfc594472833cf57505d5f46de");
                
                // Create NNS contract instance
                var nnsContract = new SmartContract(nnsContractHash, NeoSharp);
                
                // Call 'resolve' method on NNS contract
                // The resolve method takes a domain name and record type as parameters
                var result = await nnsContract.CallFunctionAsync("resolve", 
                    new object[] { name.Value, 1 }); // 1 = A record type for addresses
                
                if (result?.Stack != null && result.Stack.Count() > 0)
                {
                    var stackItem = result.Stack[0];
                    if (stackItem?.Value is string addressStr && !string.IsNullOrEmpty(addressStr))
                    {
                        // Try to parse as Hash160 address
                        if (addressStr.Length == 40) // Raw hex format
                        {
                            return new Hash160(addressStr);
                        }
                        else if (addressStr.Length == 34 && addressStr.StartsWith("N")) // Neo address format
                        {
                            // Convert Neo address to script hash
                            return addressStr.ToScriptHash();
                        }
                    }
                }
                
                throw new InvalidOperationException($"Unable to resolve NNS name '{name.Value}' to a valid address");
            }
            catch (Exception ex) when (!(ex is ArgumentNullException))
            {
                throw new InvalidOperationException($"NNS resolution failed for '{name.Value}': {ex.Message}", ex);
            }
        }
    }
}