using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Thread-safe hash cache for repeated cryptographic operations.
    /// Provides performance optimization by caching frequently computed hashes
    /// while maintaining security and memory efficiency.
    /// </summary>
    public sealed class HashCache : IDisposable
    {
        /// <summary>
        /// Shared instance for global hash caching.
        /// </summary>
        public static readonly HashCache Shared = new();

        private readonly ConcurrentDictionary<string, CachedHashEntry> _cache;
        private readonly Timer _cleanupTimer;
        private readonly object _statsLock = new();
        private bool _disposed = false;

        // Statistics
        private long _hits = 0;
        private long _misses = 0;

        /// <summary>
        /// Maximum number of cached hashes (default: 1000).
        /// </summary>
        public int MaxCacheSize { get; set; } = 1000;

        /// <summary>
        /// Cache entry time-to-live in milliseconds (default: 1 hour).
        /// </summary>
        public int CacheTtlMs { get; set; } = 60 * 60 * 1000; // 1 hour

        /// <summary>
        /// Maximum memory usage in bytes (default: 50MB).
        /// </summary>
        public long MaxMemoryUsage { get; set; } = 50 * 1024 * 1024; // 50MB

        /// <summary>
        /// Gets the current number of cached entries.
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// Gets cache hit statistics.
        /// </summary>
        public (long hits, long misses, double hitRate) Statistics
        {
            get
            {
                lock (_statsLock)
                {
                    var total = _hits + _misses;
                    var hitRate = total > 0 ? (double)_hits / total : 0.0;
                    return (_hits, _misses, hitRate);
                }
            }
        }

        /// <summary>
        /// Initializes a new HashCache instance.
        /// </summary>
        public HashCache()
        {
            _cache = new ConcurrentDictionary<string, CachedHashEntry>();
            
            // Setup cleanup timer to run every 5 minutes
            _cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Gets or computes SHA256 hash with caching.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The SHA256 hash</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public byte[] Sha256(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ThrowIfDisposed();

            return GetOrCompute(data, "sha256", d => d.Hash256());
        }

        /// <summary>
        /// Gets or computes SHA256 hash asynchronously with caching.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The SHA256 hash</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public async Task<byte[]> Sha256Async(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ThrowIfDisposed();

            return await Task.Run(() => Sha256(data));
        }

        /// <summary>
        /// Gets or computes double SHA256 (Hash256) with caching.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The double SHA256 hash</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public byte[] Hash256(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ThrowIfDisposed();

            return GetOrCompute(data, "hash256", d => d.Hash256());
        }

        /// <summary>
        /// Gets or computes double SHA256 (Hash256) asynchronously with caching.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The double SHA256 hash</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public async Task<byte[]> Hash256Async(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ThrowIfDisposed();

            return await Task.Run(() => Hash256(data));
        }

        /// <summary>
        /// Gets or computes RIPEMD160(SHA256) (Hash160) with caching.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The Hash160 result</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public byte[] Hash160(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ThrowIfDisposed();

            return GetOrCompute(data, "hash160", d => d.Sha256ThenRipemd160());
        }

        /// <summary>
        /// Gets or computes RIPEMD160(SHA256) (Hash160) asynchronously with caching.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The Hash160 result</returns>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public async Task<byte[]> Hash160Async(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ThrowIfDisposed();

            return await Task.Run(() => Hash160(data));
        }

        /// <summary>
        /// Clears all cached hashes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public void ClearCache()
        {
            ThrowIfDisposed();
            
            _cache.Clear();
            
            lock (_statsLock)
            {
                _hits = 0;
                _misses = 0;
            }
        }

        /// <summary>
        /// Removes cached hash for specific data and algorithm.
        /// </summary>
        /// <param name="data">The data to remove from cache</param>
        /// <param name="algorithm">The algorithm identifier</param>
        /// <exception cref="ArgumentNullException">Thrown when data or algorithm is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public void RemoveCached(byte[] data, string algorithm)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(algorithm))
                throw new ArgumentNullException(nameof(algorithm));

            ThrowIfDisposed();

            var key = GenerateCacheKey(data, algorithm);
            _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// Gets or computes a hash with caching.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <param name="algorithm">The algorithm identifier</param>
        /// <param name="computeFunc">The function to compute the hash if not cached</param>
        /// <returns>The hash result</returns>
        private byte[] GetOrCompute(byte[] data, string algorithm, Func<byte[], byte[]> computeFunc)
        {
            var key = GenerateCacheKey(data, algorithm);
            var now = DateTime.UtcNow;

            // Try to get from cache
            if (_cache.TryGetValue(key, out var cachedEntry))
            {
                // Check if entry is still valid
                if (now.Subtract(cachedEntry.Timestamp).TotalMilliseconds <= CacheTtlMs)
                {
                    lock (_statsLock)
                    {
                        _hits++;
                    }
                    cachedEntry.LastAccessed = now;
                    return cachedEntry.Hash;
                }
                else
                {
                    // Entry expired, remove it
                    _cache.TryRemove(key, out _);
                }
            }

            // Compute hash
            var hash = computeFunc(data);
            
            // Store in cache if there's room
            if (_cache.Count < MaxCacheSize)
            {
                var entry = new CachedHashEntry
                {
                    Hash = hash,
                    Timestamp = now,
                    LastAccessed = now,
                    DataSize = data.Length
                };
                
                _cache.TryAdd(key, entry);
            }

            lock (_statsLock)
            {
                _misses++;
            }

            return hash;
        }

        /// <summary>
        /// Generates a cache key for the given data and algorithm.
        /// Uses a combination of data length, algorithm, and hash of the data for uniqueness.
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="algorithm">The algorithm identifier</param>
        /// <returns>A unique cache key</returns>
        private static string GenerateCacheKey(byte[] data, string algorithm)
        {
            // Create a unique key based on algorithm, length, and a hash of the data
            // This avoids storing the full data as the key while maintaining uniqueness
            var dataHash = data.Hash256();
            var hashHex = Convert.ToHexString(dataHash.AsSpan(0, Math.Min(8, dataHash.Length))); // Use first 8 bytes
            
            return $"{algorithm}:{data.Length}:{hashHex}";
        }

        /// <summary>
        /// Performs periodic cleanup of expired cache entries.
        /// </summary>
        /// <param name="state">Timer state (unused)</param>
        private void PerformCleanup(object? state)
        {
            if (_disposed) return;

            try
            {
                var now = DateTime.UtcNow;
                var expiredKeys = new List<string>();
                var totalMemoryUsage = 0L;

                // Find expired entries and calculate memory usage
                foreach (var kvp in _cache)
                {
                    var entry = kvp.Value;
                    var age = now.Subtract(entry.Timestamp).TotalMilliseconds;
                    var timeSinceAccess = now.Subtract(entry.LastAccessed).TotalMilliseconds;
                    
                    // Mark for removal if expired or not accessed recently
                    if (age > CacheTtlMs || timeSinceAccess > CacheTtlMs / 2)
                    {
                        expiredKeys.Add(kvp.Key);
                    }
                    else
                    {
                        totalMemoryUsage += entry.EstimatedMemoryUsage;
                    }
                }

                // Remove expired entries
                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                }

                // If still over memory limit, remove least recently accessed entries
                if (totalMemoryUsage > MaxMemoryUsage)
                {
                    var entries = new List<KeyValuePair<string, CachedHashEntry>>(_cache);
                    entries.Sort((a, b) => a.Value.LastAccessed.CompareTo(b.Value.LastAccessed));

                    foreach (var entry in entries)
                    {
                        if (totalMemoryUsage <= MaxMemoryUsage) break;
                        
                        if (_cache.TryRemove(entry.Key, out var removed))
                        {
                            totalMemoryUsage -= removed.EstimatedMemoryUsage;
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors to prevent timer from stopping
            }
        }

        /// <summary>
        /// Throws ObjectDisposedException if this instance has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HashCache));
        }

        /// <summary>
        /// Releases all resources used by the HashCache.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _cleanupTimer?.Dispose();
            _cache.Clear();
            _disposed = true;
        }

        /// <summary>
        /// Cached hash entry with metadata.
        /// </summary>
        private class CachedHashEntry
        {
            public byte[] Hash { get; set; } = Array.Empty<byte>();
            public DateTime Timestamp { get; set; }
            public DateTime LastAccessed { get; set; }
            public int DataSize { get; set; }

            public long EstimatedMemoryUsage => Hash.Length + DataSize + 64; // Approximate overhead
        }
    }

    /// <summary>
    /// Extension methods for cached hash operations.
    /// </summary>
    public static class HashCacheExtensions
    {
        /// <summary>
        /// Compute SHA256 with caching using the shared cache instance.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The SHA256 hash</returns>
        public static byte[] CachedSha256(this byte[] data)
        {
            return HashCache.Shared.Sha256(data);
        }

        /// <summary>
        /// Compute Hash256 (double SHA256) with caching using the shared cache instance.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The Hash256 result</returns>
        public static byte[] CachedHash256(this byte[] data)
        {
            return HashCache.Shared.Hash256(data);
        }

        /// <summary>
        /// Compute Hash160 (RIPEMD160(SHA256)) with caching using the shared cache instance.
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns>The Hash160 result</returns>
        public static byte[] CachedHash160(this byte[] data)
        {
            return HashCache.Shared.Hash160(data);
        }

        /// <summary>
        /// Compute SHA256 of UTF8 bytes with caching using the shared cache instance.
        /// </summary>
        /// <param name="str">The string to hash</param>
        /// <returns>The SHA256 hash</returns>
        public static byte[] CachedSha256(this string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str).CachedSha256();
        }

        /// <summary>
        /// Compute Hash256 of UTF8 bytes with caching using the shared cache instance.
        /// </summary>
        /// <param name="str">The string to hash</param>
        /// <returns>The Hash256 result</returns>
        public static byte[] CachedHash256(this string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str).CachedHash256();
        }

        /// <summary>
        /// Compute Hash160 of UTF8 bytes with caching using the shared cache instance.
        /// </summary>
        /// <param name="str">The string to hash</param>
        /// <returns>The Hash160 result</returns>
        public static byte[] CachedHash160(this string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str).CachedHash160();
        }
    }
}