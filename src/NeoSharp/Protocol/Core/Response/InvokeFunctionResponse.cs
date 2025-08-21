namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response for invokefunction RPC call
    /// </summary>
    public class InvokeFunctionResponse : InvocationResult
    {
        /// <summary>
        /// Gets or sets the session ID
        /// </summary>
        public string? Session { get; set; }

        /// <summary>
        /// Gets or sets the iterator results
        /// </summary>
        public object[]? Iterator { get; set; }
    }
}