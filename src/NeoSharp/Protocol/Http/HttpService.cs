using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NeoSharp.Protocol.Http
{
    /// <summary>
    /// HTTP service for JSON-RPC communication.
    /// </summary>
    public class HttpService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpService> _logger;
        private readonly Uri _url;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the HttpService class.
        /// </summary>
        /// <param name="url">The RPC endpoint URL.</param>
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="logger">The logger instance.</param>
        public HttpService(string url, HttpClient httpClient = null, ILogger<HttpService> logger = null)
        {
            _url = new Uri(url);
            _httpClient = httpClient ?? new HttpClient();
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Sends a JSON-RPC request.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="method">The RPC method name.</param>
        /// <param name="parameters">The method parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        public async Task<T> SendAsync<T>(string method, object[] parameters = null, CancellationToken cancellationToken = default)
        {
            var request = new JsonRpcRequest
            {
                JsonRpc = "2.0",
                Method = method,
                Params = parameters ?? Array.Empty<object>(),
                Id = Guid.NewGuid().ToString()
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger?.LogDebug("Sending JSON-RPC request: {Method}", method);

            try
            {
                var response = await _httpClient.PostAsync(_url, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var rpcResponse = JsonSerializer.Deserialize<JsonRpcResponse<T>>(responseJson, _jsonOptions);

                if (rpcResponse.Error != null)
                {
                    throw new JsonRpcException($"JSON-RPC error {rpcResponse.Error.Code}: {rpcResponse.Error.Message}");
                }

                return rpcResponse.Result;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "HTTP request failed for method {Method}", method);
                throw new JsonRpcException($"HTTP request failed: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger?.LogError(ex, "Request timeout for method {Method}", method);
                throw new JsonRpcException($"Request timeout: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "JSON serialization error for method {Method}", method);
                throw new JsonRpcException($"JSON error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Disposes the HTTP service.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the HTTP service.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _httpClient?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// JSON-RPC request.
    /// </summary>
    internal class JsonRpcRequest
    {
        /// <summary>
        /// Gets or sets the JSON-RPC version.
        /// </summary>
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; }

        /// <summary>
        /// Gets or sets the method name.
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        [JsonPropertyName("params")]
        public object[] Params { get; set; }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    /// <summary>
    /// JSON-RPC response.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    internal class JsonRpcResponse<T>
    {
        /// <summary>
        /// Gets or sets the JSON-RPC version.
        /// </summary>
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        [JsonPropertyName("result")]
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        [JsonPropertyName("error")]
        public JsonRpcError Error { get; set; }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    /// <summary>
    /// JSON-RPC error.
    /// </summary>
    internal class JsonRpcError
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets additional error data.
        /// </summary>
        [JsonPropertyName("data")]
        public object Data { get; set; }
    }

    /// <summary>
    /// JSON-RPC exception.
    /// </summary>
    public class JsonRpcException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the JsonRpcException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public JsonRpcException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonRpcException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public JsonRpcException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}