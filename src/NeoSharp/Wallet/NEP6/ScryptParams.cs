using System.Text.Json.Serialization;

namespace NeoSharp.Wallet.NEP6
{
    /// <summary>
    /// Represents Scrypt parameters for key derivation.
    /// </summary>
    public class ScryptParams
    {
        /// <summary>
        /// Gets or sets the CPU/memory cost parameter.
        /// </summary>
        [JsonPropertyName("n")]
        public int N { get; set; }

        /// <summary>
        /// Gets or sets the block size parameter.
        /// </summary>
        [JsonPropertyName("r")]
        public int R { get; set; }

        /// <summary>
        /// Gets or sets the parallelization parameter.
        /// </summary>
        [JsonPropertyName("p")]
        public int P { get; set; }

        /// <summary>
        /// Gets or sets the derived key length.
        /// </summary>
        [JsonPropertyName("dkLen")]
        public int DkLen { get; set; }

        /// <summary>
        /// Gets the default Scrypt parameters for NEP-6.
        /// </summary>
        public static ScryptParams Default => new()
        {
            N = 16384,
            R = 8,
            P = 8,
            DkLen = 64
        };

        /// <summary>
        /// Gets light Scrypt parameters for faster operations.
        /// </summary>
        public static ScryptParams Light => new()
        {
            N = 1024,
            R = 8,
            P = 1,
            DkLen = 64
        };

        /// <summary>
        /// Gets heavy Scrypt parameters for enhanced security.
        /// </summary>
        public static ScryptParams Heavy => new()
        {
            N = 262144,
            R = 8,
            P = 8,
            DkLen = 64
        };

        /// <summary>
        /// Initializes a new instance of the ScryptParams class.
        /// </summary>
        public ScryptParams()
        {
            N = 16384;
            R = 8;
            P = 8;
            DkLen = 64;
        }

        /// <summary>
        /// Initializes a new instance of the ScryptParams class.
        /// </summary>
        /// <param name="n">The CPU/memory cost parameter.</param>
        /// <param name="r">The block size parameter.</param>
        /// <param name="p">The parallelization parameter.</param>
        /// <param name="dkLen">The derived key length.</param>
        public ScryptParams(int n, int r, int p, int dkLen = 64)
        {
            N = n;
            R = r;
            P = p;
            DkLen = dkLen;
        }

        /// <summary>
        /// Validates the Scrypt parameters.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            // N must be a power of 2
            if (N <= 0 || (N & (N - 1)) != 0)
                return false;

            // R and P must be positive
            if (R <= 0 || P <= 0)
                return false;

            // DkLen must be positive
            if (DkLen <= 0)
                return false;

            // Validate memory requirements (N * R * P should be reasonable)
            long memoryRequired = (long)N * R * P;
            if (memoryRequired > int.MaxValue)
                return false;

            return true;
        }

        /// <summary>
        /// Creates a copy of these parameters.
        /// </summary>
        /// <returns>A copy of the parameters.</returns>
        public ScryptParams Copy()
        {
            return new ScryptParams(N, R, P, DkLen);
        }

        /// <summary>
        /// Estimates the memory usage in bytes.
        /// </summary>
        /// <returns>The estimated memory usage.</returns>
        public long EstimateMemoryUsage()
        {
            // Scrypt uses approximately 128 * N * R bytes
            return 128L * N * R;
        }

        /// <summary>
        /// Estimates the time complexity relative to default parameters.
        /// </summary>
        /// <returns>The relative time complexity.</returns>
        public double EstimateRelativeTime()
        {
            var defaultParams = Default;
            double thisComplexity = (double)N * R * P;
            double defaultComplexity = (double)defaultParams.N * defaultParams.R * defaultParams.P;
            return thisComplexity / defaultComplexity;
        }

        /// <summary>
        /// Gets a string representation of the parameters.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return $"ScryptParams(N={N}, R={R}, P={P}, DkLen={DkLen})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not ScryptParams other)
                return false;

            return N == other.N && R == other.R && P == other.P && DkLen == other.DkLen;
        }

        /// <summary>
        /// Gets the hash code for these parameters.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(N, R, P, DkLen);
        }
    }
}