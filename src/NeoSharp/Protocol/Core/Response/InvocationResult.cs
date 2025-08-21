using System.Linq;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Result of a contract invocation
    /// </summary>
    public class InvocationResult
    {
        /// <summary>
        /// Gets or sets the execution state
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the gas consumed
        /// </summary>
        public string? GasConsumed { get; set; }

        /// <summary>
        /// Gets or sets the stack of return values
        /// </summary>
        public StackItem[]? Stack { get; set; }

        /// <summary>
        /// Gets or sets the exception message if execution failed
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// Gets or sets the transaction hash
        /// </summary>
        public string? Tx { get; set; }
        
        /// <summary>
        /// Gets or sets the session ID for iterator operations
        /// </summary>
        public string? SessionId { get; set; }
        
        /// <summary>
        /// Gets whether the invocation resulted in a fault state
        /// </summary>
        public bool HasStateFault => State?.ToUpper() == "FAULT";

        /// <summary>
        /// Gets the result as InvocationResult (for compatibility)
        /// </summary>
        /// <returns>This instance</returns>
        public InvocationResult GetResult()
        {
            return this;
        }

        /// <summary>
        /// Gets the first stack item from the results
        /// </summary>
        /// <returns>The first stack item or null</returns>
        public StackItem? GetFirstStackItem()
        {
            return Stack?.FirstOrDefault();
        }
    }

}