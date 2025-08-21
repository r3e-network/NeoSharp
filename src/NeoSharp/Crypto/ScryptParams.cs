using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Parameters for the Scrypt password-based key derivation function.
    /// Scrypt is designed to be memory-hard and computationally expensive to resist brute-force attacks.
    /// </summary>
    public class ScryptParams : IEquatable<ScryptParams>
    {
        /// <summary>
        /// Standard N parameter (CPU/memory cost parameter) - 2^14 = 16384
        /// </summary>
        public const int N_STANDARD = 1 << 14;
        
        /// <summary>
        /// Standard r parameter (block size parameter)
        /// </summary>
        public const int R_STANDARD = 8;
        
        /// <summary>
        /// Standard p parameter (parallelization parameter)
        /// </summary>
        public const int P_STANDARD = 8;

        /// <summary>
        /// Default Scrypt parameters using standard values
        /// </summary>
        public static readonly ScryptParams Default = new(N_STANDARD, R_STANDARD, P_STANDARD);

        /// <summary>
        /// Fast Scrypt parameters for testing (reduced security)
        /// </summary>
        public static readonly ScryptParams Fast = new(1024, 1, 1);

        /// <summary>
        /// High security Scrypt parameters (increased computational cost)
        /// </summary>
        public static readonly ScryptParams HighSecurity = new(1 << 20, 8, 1); // 2^20 = 1048576

        /// <summary>
        /// CPU/memory cost parameter. Must be a power of 2 greater than 1.
        /// Higher values increase memory usage and computation time exponentially.
        /// </summary>
        [JsonPropertyName("n")]
        public int N { get; }

        /// <summary>
        /// Block size parameter. Affects memory usage linearly.
        /// Typically set to 8 for optimal performance.
        /// </summary>
        [JsonPropertyName("r")]
        public int R { get; }

        /// <summary>
        /// Parallelization parameter. Can be used to tune for available CPU cores.
        /// Higher values increase memory usage linearly but allow parallel processing.
        /// </summary>
        [JsonPropertyName("p")]
        public int P { get; }

        /// <summary>
        /// Initializes new Scrypt parameters with the specified values.
        /// </summary>
        /// <param name="n">CPU/memory cost parameter (must be power of 2 > 1)</param>
        /// <param name="r">Block size parameter (must be > 0)</param>
        /// <param name="p">Parallelization parameter (must be > 0)</param>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
        [JsonConstructor]
        public ScryptParams(int n, int r, int p)
        {
            ValidateParameters(n, r, p);
            N = n;
            R = r;
            P = p;
        }

        /// <summary>
        /// Validates Scrypt parameters for correctness and security.
        /// </summary>
        /// <param name="n">CPU/memory cost parameter</param>
        /// <param name="r">Block size parameter</param>
        /// <param name="p">Parallelization parameter</param>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
        private static void ValidateParameters(int n, int r, int p)
        {
            if (n <= 1)
                throw new ArgumentException("N parameter must be greater than 1", nameof(n));
            
            if ((n & (n - 1)) != 0)
                throw new ArgumentException("N parameter must be a power of 2", nameof(n));
            
            if (r <= 0)
                throw new ArgumentException("R parameter must be positive", nameof(r));
            
            if (p <= 0)
                throw new ArgumentException("P parameter must be positive", nameof(p));

            // Check for potential integer overflow in memory calculation
            // Memory usage ≈ 128 * N * r * p bytes
            const long maxMemory = (long)int.MaxValue / 128;
            if ((long)n * r * p > maxMemory)
                throw new ArgumentException("Parameters would result in excessive memory usage");
        }

        /// <summary>
        /// Estimates the memory usage in bytes for these Scrypt parameters.
        /// </summary>
        /// <returns>Estimated memory usage in bytes</returns>
        public long EstimateMemoryUsage()
        {
            return 128L * N * R * P;
        }

        /// <summary>
        /// Estimates the computational complexity relative to default parameters.
        /// </summary>
        /// <returns>Relative computational complexity factor</returns>
        public double EstimateComplexity()
        {
            var defaultComplexity = (double)Default.N * Default.R * Default.P;
            var currentComplexity = (double)N * R * P;
            return currentComplexity / defaultComplexity;
        }

        /// <summary>
        /// Creates Scrypt parameters optimized for the current system.
        /// </summary>
        /// <param name="targetTimeMs">Target computation time in milliseconds</param>
        /// <returns>Optimized Scrypt parameters</returns>
        public static ScryptParams CreateOptimized(int targetTimeMs = 1000)
        {
            // This is a simplified heuristic. In practice, you'd want to benchmark
            // the actual system and adjust parameters accordingly.
            var complexity = Math.Max(1.0, targetTimeMs / 100.0);
            
            if (complexity < 0.1)
                return Fast;
            else if (complexity > 10.0)
                return HighSecurity;
            else
                return Default;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current ScryptParams.
        /// </summary>
        /// <param name="obj">The object to compare with the current ScryptParams</param>
        /// <returns>True if the specified object is equal to the current ScryptParams; otherwise, false</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as ScryptParams);
        }

        /// <summary>
        /// Determines whether the specified ScryptParams is equal to the current ScryptParams.
        /// </summary>
        /// <param name="other">The ScryptParams to compare with the current ScryptParams</param>
        /// <returns>True if the specified ScryptParams is equal to the current ScryptParams; otherwise, false</returns>
        public bool Equals(ScryptParams? other)
        {
            return other is not null && N == other.N && R == other.R && P == other.P;
        }

        /// <summary>
        /// Returns the hash code for this ScryptParams.
        /// </summary>
        /// <returns>A hash code for the current ScryptParams</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(N, R, P);
        }

        /// <summary>
        /// Returns a string representation of the ScryptParams.
        /// </summary>
        /// <returns>A string that represents the current ScryptParams</returns>
        public override string ToString()
        {
            var memoryMB = EstimateMemoryUsage() / (1024 * 1024);
            var complexity = EstimateComplexity();
            return $"ScryptParams(N={N}, R={R}, P={P}, ~{memoryMB}MB, {complexity:F2}x complexity)";
        }

        /// <summary>
        /// Serializes the ScryptParams to JSON.
        /// </summary>
        /// <returns>JSON representation of the ScryptParams</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Deserializes ScryptParams from JSON.
        /// </summary>
        /// <param name="json">JSON string containing ScryptParams</param>
        /// <returns>Deserialized ScryptParams</returns>
        /// <exception cref="ArgumentException">Thrown when JSON is invalid or contains invalid parameters</exception>
        public static ScryptParams FromJson(string json)
        {
            try
            {
                var result = JsonSerializer.Deserialize<ScryptParams>(json);
                return result ?? throw new ArgumentException("Failed to deserialize ScryptParams from JSON");
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format for ScryptParams: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Determines whether two ScryptParams instances are equal.
        /// </summary>
        /// <param name="left">The first ScryptParams to compare</param>
        /// <param name="right">The second ScryptParams to compare</param>
        /// <returns>True if the ScryptParams instances are equal; otherwise, false</returns>
        public static bool operator ==(ScryptParams? left, ScryptParams? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            
            return left?.Equals(right) ?? false;
        }

        /// <summary>
        /// Determines whether two ScryptParams instances are not equal.
        /// </summary>
        /// <param name="left">The first ScryptParams to compare</param>
        /// <param name="right">The second ScryptParams to compare</param>
        /// <returns>True if the ScryptParams instances are not equal; otherwise, false</returns>
        public static bool operator !=(ScryptParams? left, ScryptParams? right)
        {
            return !(left == right);
        }
    }
}