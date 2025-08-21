using System;
using System.Collections.Generic;
using System.Linq;
using NeoSharp.Protocol;
using NeoSharp.Types;

namespace NeoSharp.Transaction
{
    /// <summary>
    /// Builder for Neo transactions
    /// </summary>
    public class TransactionBuilder
    {
        private readonly List<byte[]> _scripts = new();
        private readonly List<Signer> _signers = new();
        private readonly INeoSharp _neoSharp;

        /// <summary>
        /// Initializes a new instance of the TransactionBuilder class
        /// </summary>
        /// <param name="neoSharp">The Neo client instance</param>
        public TransactionBuilder(INeoSharp neoSharp)
        {
            _neoSharp = neoSharp ?? throw new ArgumentNullException(nameof(neoSharp));
        }

        /// <summary>
        /// Gets the signers for this transaction
        /// </summary>
        public List<Signer> Signers => _signers;

        /// <summary>
        /// Adds a script to the transaction
        /// </summary>
        /// <param name="script">The script bytes</param>
        /// <returns>This builder for chaining</returns>
        public TransactionBuilder AddScript(byte[] script)
        {
            if (script == null || script.Length == 0)
                throw new ArgumentException("Script cannot be null or empty", nameof(script));
            
            _scripts.Add(script);
            return this;
        }

        /// <summary>
        /// Adds a signer to the transaction
        /// </summary>
        /// <param name="signer">The signer to add</param>
        /// <returns>This builder for chaining</returns>
        public TransactionBuilder AddSigner(Signer signer)
        {
            if (signer == null)
                throw new ArgumentNullException(nameof(signer));
                
            _signers.Add(signer);
            return this;
        }

        /// <summary>
        /// Builds the transaction
        /// </summary>
        /// <returns>The transaction bytes</returns>
        public byte[] Build()
        {
            if (_scripts.Count == 0)
                throw new InvalidOperationException("At least one script must be added");

            // This is a simplified implementation
            // In a real Neo transaction, you would need to construct the proper transaction format
            var result = new List<byte>();
            
            foreach (var script in _scripts)
            {
                result.AddRange(script);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Creates a new transaction builder
        /// </summary>
        /// <param name="neoSharp">The Neo client instance</param>
        /// <returns>A new TransactionBuilder instance</returns>
        public static TransactionBuilder Create(INeoSharp neoSharp)
        {
            return new TransactionBuilder(neoSharp);
        }
    }
}