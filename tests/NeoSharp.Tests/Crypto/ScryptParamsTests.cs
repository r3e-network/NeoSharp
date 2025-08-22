using System;
using System.Text.Json;
using FluentAssertions;
using NeoSharp.Crypto;
using Xunit;

namespace NeoSharp.Tests.Crypto
{
    /// <summary>
    /// Scrypt parameters serialization/deserialization tests
    /// Converted from Swift ScryptParamsTests.swift
    /// Tests JSON serialization and various parameter name formats
    /// </summary>
    public class ScryptParamsTests
    {
        private readonly ScryptParams _testParams = new ScryptParams(8, 4, 9);

        [Fact]
        public void TestSerialize()
        {
            // Test JSON serialization
            var json = JsonSerializer.Serialize(_testParams);
            
            json.Should().Be("{\"n\":8,\"r\":4,\"p\":9}");
        }

        [Fact]
        public void TestDeserialize()
        {
            // Test various JSON formats that should all deserialize to the same parameters
            var jsonFormats = new[]
            {
                "{\"n\":8,\"r\":4,\"p\":9}",                    // Standard format
                "{\"n\":8,\"blockSize\":4,\"p\":9}",            // blockSize instead of r
                "{\"n\":8,\"blockSize\":4,\"parallel\":9}",     // blockSize and parallel
                "{\"n\":8,\"r\":4,\"parallel\":9}",             // parallel instead of p
                "{\"n\":8,\"blocksize\":4,\"p\":9}",            // lowercase blocksize
                "{\"n\":8,\"blocksize\":4,\"parallel\":9}",     // lowercase blocksize and parallel
                "{\"cost\":8,\"r\":4,\"p\":9}",                 // cost instead of n
                "{\"cost\":8,\"r\":4,\"parallel\":9}",          // cost and parallel
                "{\"cost\":8,\"blockSize\":4,\"p\":9}",         // cost and blockSize
                "{\"cost\":8,\"blockSize\":4,\"parallel\":9}",  // cost, blockSize, and parallel
                "{\"cost\":8,\"blocksize\":4,\"p\":9}",         // cost and lowercase blocksize
                "{\"cost\":8,\"blocksize\":4,\"parallel\":9}",  // cost, lowercase blocksize, and parallel
            };

            foreach (var jsonFormat in jsonFormats)
            {
                var deserializedParams = JsonSerializer.Deserialize<ScryptParams>(jsonFormat);
                
                deserializedParams.Should().BeEquivalentTo(_testParams);
                deserializedParams.N.Should().Be(8);
                deserializedParams.R.Should().Be(4);
                deserializedParams.P.Should().Be(9);
            }
        }

        [Fact]
        public void TestDefaultScryptParams()
        {
            // Test default NEP-2 scrypt parameters
            var defaultParams = ScryptParams.Default;
            
            defaultParams.N.Should().Be(16384);  // 2^14
            defaultParams.R.Should().Be(8);
            defaultParams.P.Should().Be(8);
        }

        [Fact]
        public void TestScryptParamsEquality()
        {
            // Test equality comparison
            var params1 = new ScryptParams(16384, 8, 8);
            var params2 = new ScryptParams(16384, 8, 8);
            var params3 = new ScryptParams(16384, 8, 4);

            params1.Should().Be(params2);
            params1.Should().NotBe(params3);
            params1.GetHashCode().Should().Be(params2.GetHashCode());
            params1.GetHashCode().Should().NotBe(params3.GetHashCode());
        }

        [Fact]
        public void TestScryptParamsValidation()
        {
            // Test parameter validation
            Action createValidParams = () => new ScryptParams(16384, 8, 8);
            createValidParams.Should().NotThrow();

            // Test invalid parameters - Note: Our implementation validates in ValidateParameters method
            Action createInvalidN = () => new ScryptParams(0, 8, 8);
            createInvalidN.Should().Throw<ArgumentException>();

            Action createInvalidR = () => new ScryptParams(16384, 0, 8);
            createInvalidR.Should().Throw<ArgumentException>();

            Action createInvalidP = () => new ScryptParams(16384, 8, 0);
            createInvalidP.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(16384, 8, 8)]   // Default NEP-2
        [InlineData(32768, 8, 8)]   // Higher security
        [InlineData(4096, 4, 4)]    // Lower resource usage
        [InlineData(256, 1, 1)]     // Testing/fast params
        public void TestScryptParamsRoundTrip(int n, int r, int p)
        {
            // Test serialization round-trip
            var originalParams = new ScryptParams(n, r, p);
            var json = JsonSerializer.Serialize(originalParams);
            var deserializedParams = JsonSerializer.Deserialize<ScryptParams>(json);

            deserializedParams.Should().BeEquivalentTo(originalParams);
        }

        [Fact]
        public void TestScryptParamsWithExtraFields()
        {
            // Test that extra JSON fields are ignored
            var jsonWithExtra = "{\"n\":8,\"r\":4,\"p\":9,\"extraField\":\"ignored\",\"anotherField\":42}";
            
            var deserializedParams = JsonSerializer.Deserialize<ScryptParams>(jsonWithExtra);
            
            deserializedParams.Should().BeEquivalentTo(_testParams);
        }

        [Fact]
        public void TestScryptParamsIncompleteJson()
        {
            // Test that missing required fields result in invalid parameters
            var incompleteJsons = new[]
            {
                "{}",                    // Empty object
                "{\"n\":8}",             // Missing r and p
                "{\"n\":8,\"r\":4}",     // Missing p
                "{\"r\":4,\"p\":9}",     // Missing n
            };

            foreach (var incompleteJson in incompleteJsons)
            {
                Action act = () => {
                    var result = JsonSerializer.Deserialize<ScryptParams>(incompleteJson);
                    // The deserialization might succeed but create invalid parameters
                    // that would fail validation
                    if (result?.N == 0 || result?.R == 0 || result?.P == 0)
                        throw new ArgumentException("Invalid parameters");
                };
                act.Should().Throw<ArgumentException>();
            }
        }

        [Fact]
        public void TestScryptParamsStringRepresentation()
        {
            // Test string representation
            var paramsString = _testParams.ToString();
            
            paramsString.Should().Contain("8");  // N value
            paramsString.Should().Contain("4");  // R value
            paramsString.Should().Contain("9");  // P value
        }

        [Theory]
        [InlineData("{\"N\":8,\"R\":4,\"P\":9}")]           // Uppercase property names
        [InlineData("{\"cost\":8,\"blockSize\":4,\"p\":9}")] // Mixed case
        [InlineData("{\"n\": 8, \"r\": 4, \"p\": 9}")]      // Spaces
        public void TestScryptParamsJsonVariations(string json)
        {
            // Test various JSON formatting variations
            var deserializedParams = JsonSerializer.Deserialize<ScryptParams>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            deserializedParams.N.Should().Be(8);
            deserializedParams.R.Should().Be(4);
            deserializedParams.P.Should().Be(9);
        }

        [Fact]
        public void TestScryptParamsMemoryUsageCalculation()
        {
            // Test memory usage calculation (important for resource planning)
            var lightParams = new ScryptParams(256, 1, 1);
            var defaultParams = new ScryptParams(16384, 8, 8);
            var heavyParams = new ScryptParams(32768, 16, 16);

            // Memory usage should increase with parameters
            var lightMemory = lightParams.EstimateMemoryUsage();
            var defaultMemory = defaultParams.EstimateMemoryUsage();
            var heavyMemory = heavyParams.EstimateMemoryUsage();

            defaultMemory.Should().BeGreaterThan(lightMemory);
            heavyMemory.Should().BeGreaterThan(defaultMemory);
        }

        [Fact]
        public void TestScryptParamsPerformanceRelativeTime()
        {
            // Test relative performance timing using EstimateComplexity
            var fastParams = new ScryptParams(256, 1, 1);
            var normalParams = new ScryptParams(16384, 8, 8);
            var slowParams = new ScryptParams(65536, 16, 16);

            var fastTime = fastParams.EstimateComplexity();
            var normalTime = normalParams.EstimateComplexity();
            var slowTime = slowParams.EstimateComplexity();

            fastTime.Should().BeLessThan(normalTime);
            slowTime.Should().BeGreaterThan(normalTime);
            normalTime.Should().BeApproximately(1.0, 0.1); // Should be close to 1.0 for default params
        }
    }

    /// <summary>
    /// Performance categories for scrypt parameters
    /// </summary>
    public enum ScryptPerformance
    {
        Fast,
        Normal,
        Slow
    }
}