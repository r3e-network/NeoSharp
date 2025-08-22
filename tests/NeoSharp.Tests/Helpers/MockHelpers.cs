using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace NeoSharp.Tests.Helpers
{
    /// <summary>
    /// Mock helpers for HTTP services and JSON handling.
    /// Converted from Swift MockURLSession and JSON helpers.
    /// </summary>
    public static class MockHelpers
    {
        /// <summary>
        /// Creates a mock HttpClient with predefined responses
        /// </summary>
        public static HttpClient CreateMockHttpClient(Dictionary<string, string> responses)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            
            foreach (var response in responses)
            {
                var httpResponse = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(response.Value, Encoding.UTF8, "application/json")
                };

                handlerMock
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(req => req.Content != null && req.Content.ReadAsStringAsync().Result.Contains(response.Key)),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(httpResponse);
            }

            return new HttpClient(handlerMock.Object);
        }

        /// <summary>
        /// Loads JSON from test resources
        /// </summary>
        public static string LoadJsonResource(string fileName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = $"NeoSharp.Tests.Resources.{fileName}";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Resource {resourceName} not found");
                
            using var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Creates a JSON response for mock HTTP calls
        /// </summary>
        public static string CreateJsonResponse(object data)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return JsonSerializer.Serialize(data, options);
        }

        /// <summary>
        /// Mock NEO RPC response wrapper
        /// </summary>
        public class MockRpcResponse<T>
        {
            public string JsonRpc { get; set; } = "2.0";
            public int Id { get; set; } = 1;
            public T? Result { get; set; }
            public RpcError? Error { get; set; }
        }

        public class RpcError
        {
            public int Code { get; set; }
            public string Message { get; set; } = string.Empty;
            public object? Data { get; set; }
        }

        /// <summary>
        /// Creates a successful RPC response
        /// </summary>
        public static string CreateRpcResponse<T>(T result)
        {
            var response = new MockRpcResponse<T> { Result = result };
            return CreateJsonResponse(response);
        }

        /// <summary>
        /// Creates an error RPC response
        /// </summary>
        public static string CreateRpcErrorResponse(int code, string message)
        {
            var response = new MockRpcResponse<object>
            {
                Error = new RpcError { Code = code, Message = message }
            };
            return CreateJsonResponse(response);
        }

        /// <summary>
        /// Helper to verify method was called with specific parameters
        /// </summary>
        public static void VerifyHttpCall(Mock<HttpMessageHandler> handlerMock, 
            string expectedMethod, 
            string expectedContent, 
            Times times)
        {
            handlerMock.Protected().Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method.ToString() == expectedMethod &&
                    req.Content != null && req.Content.ReadAsStringAsync().Result.Contains(expectedContent)),
                ItExpr.IsAny<CancellationToken>());
        }

        /// <summary>
        /// Creates mock blocks for testing
        /// </summary>
        public static class MockBlocks
        {
            public static object GetMockBlock(uint index, string hash)
            {
                return new
                {
                    hash,
                    size = 1234,
                    version = 0,
                    previousblockhash = "0x" + new string('0', 64),
                    merkleroot = "0x" + new string('1', 64),
                    time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    nonce = "0123456789ABCDEF",
                    index,
                    primary = 0,
                    nextconsensus = TestConstants.CommitteeAccountScriptHash,
                    witnesses = new[]
                    {
                        new
                        {
                            invocation = "DEA1",
                            verification = "BEEF"
                        }
                    },
                    tx = Array.Empty<object>()
                };
            }

            public static object GetMockTransaction(string hash)
            {
                return new
                {
                    hash,
                    size = 234,
                    version = 0,
                    nonce = 123456,
                    sender = TestConstants.DefaultAccountScriptHash,
                    sysfee = "1000000",
                    netfee = "1000000",
                    validuntilblock = 1000,
                    signers = new[]
                    {
                        new
                        {
                            account = TestConstants.DefaultAccountScriptHash,
                            scopes = "CalledByEntry"
                        }
                    },
                    attributes = Array.Empty<object>(),
                    script = "VwEAwkqgAAAAAAAAQFcBAMJKoAAAAAAAAEA=",
                    witnesses = new[]
                    {
                        new
                        {
                            invocation = "DEA1",
                            verification = TestConstants.DefaultAccountVerificationScript
                        }
                    }
                };
            }
        }
    }
}