using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NeoSharp.Protocol.Http;
using Xunit;

namespace NeoSharp.Tests.Protocol
{
    public class HttpServiceTests
    {
        [Fact]
        public void Dispose_WithExternalHttpClient_DoesNotDisposeHandler()
        {
            var handler = new TrackingHttpMessageHandler();
            var client = new HttpClient(handler);

            using (var service = new HttpService("http://localhost", client, NullLogger.Instance))
            {
                // Intentionally empty. We just want to exercise disposal.
            }

            Assert.False(handler.IsDisposed);
        }

        [Fact]
        public async Task SendAsync_WithRpcError_ThrowsJsonRpcException()
        {
            var handler = new TrackingHttpMessageHandler
            {
                Response = CreateJsonResponse("{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"error\":{\"code\":-1,\"message\":\"boom\"}}")
            };

            using var client = new HttpClient(handler);
            using var service = new HttpService("http://localhost", client, NullLogger.Instance);

            await Assert.ThrowsAsync<JsonRpcException>(() => service.SendAsync<int>("testmethod"));
        }

        [Fact]
        public async Task SendAsync_WithEmptyPayload_ThrowsJsonRpcException()
        {
            var handler = new TrackingHttpMessageHandler
            {
                Response = CreateJsonResponse(""),
            };

            using var client = new HttpClient(handler);
            using var service = new HttpService("http://localhost", client, NullLogger.Instance);

            await Assert.ThrowsAsync<JsonRpcException>(() => service.SendAsync<int>("testmethod"));
        }

        private static HttpResponseMessage CreateJsonResponse(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }

        private sealed class TrackingHttpMessageHandler : HttpMessageHandler
        {
            public HttpResponseMessage Response { get; init; } = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"result\":null}", Encoding.UTF8, "application/json")
            };

            public bool IsDisposed { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Response);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                IsDisposed = true;
            }
        }
    }
}
