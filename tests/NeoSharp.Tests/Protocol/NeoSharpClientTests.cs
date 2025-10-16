using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NeoSharp.Protocol;
using NeoSharp.Protocol.Http;
using Xunit;

namespace NeoSharp.Tests.Protocol
{
    public class NeoSharpClientTests
    {
        [Fact]
        public void Dispose_WithExternalTransport_DoesNotDisposeTransport()
        {
            var transport = new FakeRpcClient();
            var neoSharp = new NeoSharp.Protocol.NeoSharp(transport);

            neoSharp.Dispose();

            transport.DisposeCount.Should().Be(0);
        }

        private sealed class FakeRpcClient : IJsonRpcClient
        {
            public int DisposeCount { get; private set; }

            public Task<T> SendAsync<T>(string method, object[]? parameters = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(default(T)!);
            }

            public void Dispose()
            {
                DisposeCount++;
            }
        }
    }
}
