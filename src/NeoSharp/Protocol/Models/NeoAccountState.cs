using System.Numerics;
using NeoSharp.Types;

namespace NeoSharp.Protocol.Models
{
    /// <summary>
    /// Represents a Neo account state
    /// </summary>
    public class NeoAccountState
    {
        /// <summary>
        /// Gets or sets the account script hash
        /// </summary>
        public Hash160 ScriptHash { get; set; } = Hash160.Zero;

        /// <summary>
        /// Gets or sets the account balance
        /// </summary>
        public BigInteger Balance { get; set; }

        /// <summary>
        /// Gets or sets the last updated block index
        /// </summary>
        public long LastUpdatedBlock { get; set; }

        /// <summary>
        /// Gets or sets whether the account is frozen
        /// </summary>
        public bool IsFrozen { get; set; }

        /// <summary>
        /// Gets or sets the account vote target
        /// </summary>
        public byte[]? VoteTo { get; set; }

        /// <summary>
        /// Initializes a new NeoAccountState with specified values
        /// </summary>
        /// <param name="scriptHash">The account script hash</param>
        /// <param name="balance">The account balance</param>
        /// <param name="lastUpdatedBlock">The last updated block index</param>
        public NeoAccountState(Hash160 scriptHash, BigInteger balance, long lastUpdatedBlock)
        {
            ScriptHash = scriptHash;
            Balance = balance;
            LastUpdatedBlock = lastUpdatedBlock;
            IsFrozen = false;
            VoteTo = null;
        }

        /// <summary>
        /// Initializes a new empty NeoAccountState
        /// </summary>
        public NeoAccountState()
        {
            ScriptHash = Hash160.Zero;
            Balance = BigInteger.Zero;
            LastUpdatedBlock = 0;
            IsFrozen = false;
            VoteTo = null;
        }

        /// <summary>
        /// Creates a NeoAccountState with no balance
        /// </summary>
        /// <param name="scriptHash">The account script hash</param>
        /// <returns>A NeoAccountState with zero balance</returns>
        public static NeoAccountState WithNoBalance(Hash160 scriptHash)
        {
            return new NeoAccountState(scriptHash, BigInteger.Zero, 0);
        }

        /// <summary>
        /// Creates a NeoAccountState with no vote
        /// </summary>
        /// <param name="scriptHash">The account script hash</param>
        /// <param name="balance">The account balance</param>
        /// <param name="lastUpdatedBlock">The last updated block index</param>
        /// <returns>A NeoAccountState with no vote target</returns>
        public static NeoAccountState WithNoVote(Hash160 scriptHash, BigInteger balance, long lastUpdatedBlock)
        {
            return new NeoAccountState(scriptHash, balance, lastUpdatedBlock);
        }
    }
}