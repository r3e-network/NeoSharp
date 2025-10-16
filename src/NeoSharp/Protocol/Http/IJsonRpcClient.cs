using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace NeoSharp.Protocol.Http
{
    /// <summary>
    /// Abstraction over a JSON-RPC transport layer.
    /// </summary>
    public interface IJsonRpcClient : IDisposable
    {
        /// <summary>
        /// Sends a JSON-RPC request and returns the typed result.
        /// </summary>
        /// <typeparam name="T">The expected result type.</typeparam>
        /// <param name="method">The RPC method name.</param>
        /// <param name="parameters">The RPC method parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The RPC result.</returns>
        Task<T> SendAsync<T>(string method, object[]? parameters = null, CancellationToken cancellationToken = default);
    }
}
