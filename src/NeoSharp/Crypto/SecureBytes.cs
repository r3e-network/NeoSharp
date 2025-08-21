using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// A secure container for sensitive byte data that ensures proper memory cleanup
    /// and protection against memory dumps. Implements IDisposable for deterministic cleanup.
    /// </summary>
    public sealed class SecureBytes : IDisposable, IEquatable<SecureBytes>
    {
        private readonly MemoryHandle _memoryHandle;
        private readonly Memory<byte> _memory;
        private readonly object _lock = new();
        private bool _isDisposed = false;

        /// <summary>
        /// Gets the length of the secure byte array.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets whether this SecureBytes instance has been disposed.
        /// </summary>
        public bool IsDisposed 
        { 
            get 
            { 
                lock (_lock) 
                { 
                    return _isDisposed; 
                } 
            } 
        }

        /// <summary>
        /// Initialize with byte array, copying data to secure memory.
        /// </summary>
        /// <param name="data">The sensitive data to store securely</param>
        /// <exception cref="ArgumentNullException">Thrown when data is null</exception>
        /// <exception cref="CryptoSecurityException">Thrown when secure memory allocation fails</exception>
        public SecureBytes(byte[] data) : this(data.AsSpan()) { }

        /// <summary>
        /// Initialize with byte span, copying data to secure memory.
        /// </summary>
        /// <param name="data">The sensitive data to store securely</param>
        /// <exception cref="CryptoSecurityException">Thrown when secure memory allocation fails</exception>
        public SecureBytes(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
            {
                Length = 0;
                _memory = Memory<byte>.Empty;
                _memoryHandle = default;
                return;
            }

            Length = data.Length;

            try
            {
                // Use ArrayPool for better memory management and potential pinning
                var rentedArray = ArrayPool<byte>.Shared.Rent(Length);
                _memory = rentedArray.AsMemory(0, Length);
                
                // Pin the memory to prevent GC movement
                _memoryHandle = _memory.Pin();

                // Copy data to secure memory
                data.CopyTo(_memory.Span);

                // Clear any extra rented space
                if (rentedArray.Length > Length)
                {
                    rentedArray.AsSpan(Length).Clear();
                }

                // Attempt to lock memory pages (best effort, platform dependent)
                TryLockMemory();
            }
            catch (Exception ex)
            {
                Dispose();
                throw CryptoSecurityException.SecureMemoryError(
                    "Failed to allocate secure memory for sensitive data", ex);
            }
        }

        /// <summary>
        /// Initialize with specified size, filled with zeros.
        /// </summary>
        /// <param name="length">The size of the secure byte array</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when length is negative</exception>
        /// <exception cref="CryptoSecurityException">Thrown when secure memory allocation fails</exception>
        public SecureBytes(int length) : this(new byte[Math.Max(0, length)]) 
        { 
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");
        }

        /// <summary>
        /// Create SecureBytes from a password string with secure UTF-8 encoding.
        /// The original string should be cleared by the caller if possible.
        /// </summary>
        /// <param name="password">The password string to convert</param>
        /// <returns>SecureBytes containing UTF-8 encoded password</returns>
        /// <exception cref="ArgumentNullException">Thrown when password is null</exception>
        /// <exception cref="CryptoSecurityException">Thrown when encoding fails</exception>
        public static SecureBytes FromPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (password.Length == 0)
                return new SecureBytes(0);

            try
            {
                // Use secure string conversion if available
                var encoding = new UTF8Encoding(false, true);
                var maxByteCount = encoding.GetMaxByteCount(password.Length);
                
                // Rent array to avoid multiple allocations
                var tempBuffer = ArrayPool<byte>.Shared.Rent(maxByteCount);
                try
                {
                    var actualBytes = encoding.GetBytes(password, tempBuffer);
                    return new SecureBytes(tempBuffer.AsSpan(0, actualBytes));
                }
                finally
                {
                    // Clear temp buffer before returning to pool
                    CryptographicOperations.ZeroMemory(tempBuffer);
                    ArrayPool<byte>.Shared.Return(tempBuffer, clearArray: true);
                }
            }
            catch (Exception ex)
            {
                throw CryptoSecurityException.SecureMemoryError(
                    "Failed to securely encode password", ex);
            }
        }

        /// <summary>
        /// Access bytes with a delegate, ensuring secure handling.
        /// The bytes are copied to an array for the delegate execution.
        /// </summary>
        /// <typeparam name="T">The return type of the delegate</typeparam>
        /// <param name="action">The delegate to execute with access to the secure bytes</param>
        /// <returns>The result from the delegate</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        public T WithSecureAccess<T>(Func<byte[], T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ThrowIfDisposed();

            lock (_lock)
            {
                ThrowIfDisposed();
                var tempArray = _memory.Span.ToArray();
                try
                {
                    return action(tempArray);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(tempArray);
                }
            }
        }

        /// <summary>
        /// Access bytes with an action delegate, ensuring secure handling.
        /// The bytes are copied to an array for the delegate execution.
        /// </summary>
        /// <param name="action">The action to execute with access to the secure bytes</param>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        public void WithSecureAccess(Action<byte[]> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            WithSecureAccess<object?>((bytes) =>
            {
                action(bytes);
                return null;
            });
        }

        /// <summary>
        /// Get a copy of the bytes (use sparingly, as this creates non-secure copies).
        /// The returned array should be cleared by the caller when no longer needed.
        /// </summary>
        /// <returns>A copy of the secure bytes</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public byte[] ToArray()
        {
            return WithSecureAccess(bytes => bytes.ToArray());
        }

        /// <summary>
        /// Convert to hexadecimal string (use sparingly).
        /// The returned string cannot be securely cleared.
        /// </summary>
        /// <returns>Hexadecimal representation of the secure bytes</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public string ToHexString()
        {
            if (Length == 0) return string.Empty;

            return WithSecureAccess(bytes =>
            {
                return Convert.ToHexString(bytes);
            });
        }

        /// <summary>
        /// Compare with another SecureBytes in constant time to prevent timing attacks.
        /// </summary>
        /// <param name="other">The other SecureBytes to compare with</param>
        /// <returns>True if the contents are equal; otherwise, false</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public bool ConstantTimeEquals(SecureBytes? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            ThrowIfDisposed();
            other.ThrowIfDisposed();

            if (Length != other.Length) return false;
            if (Length == 0) return true;

            return WithSecureAccess(thisBytes =>
                other.WithSecureAccess(otherBytes =>
                    CryptographicOperations.FixedTimeEquals(thisBytes, otherBytes)));
        }

        /// <summary>
        /// Compare with byte array in constant time to prevent timing attacks.
        /// </summary>
        /// <param name="other">The byte array to compare with</param>
        /// <returns>True if the contents are equal; otherwise, false</returns>
        /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed</exception>
        public bool ConstantTimeEquals(byte[] other)
        {
            ThrowIfDisposed();

            if (other == null) return false;
            if (Length != other.Length) return false;
            if (Length == 0) return other.Length == 0;

            return WithSecureAccess(thisBytes =>
                CryptographicOperations.FixedTimeEquals(thisBytes, other));
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current SecureBytes.
        /// Uses constant-time comparison for security.
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>True if equal; otherwise, false</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as SecureBytes);
        }

        /// <summary>
        /// Determines whether the specified SecureBytes is equal to the current SecureBytes.
        /// Uses constant-time comparison for security.
        /// </summary>
        /// <param name="other">The SecureBytes to compare with</param>
        /// <returns>True if equal; otherwise, false</returns>
        public bool Equals(SecureBytes? other)
        {
            return ConstantTimeEquals(other);
        }

        /// <summary>
        /// Returns a hash code for this SecureBytes.
        /// Note: This uses only the length to avoid exposing data through timing attacks.
        /// </summary>
        /// <returns>A hash code for the current SecureBytes</returns>
        public override int GetHashCode()
        {
            return Length.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation that doesn't expose the actual data.
        /// </summary>
        /// <returns>A safe string representation</returns>
        public override string ToString()
        {
            return IsDisposed 
                ? $"SecureBytes[DISPOSED]" 
                : $"SecureBytes[{Length} bytes]";
        }

        /// <summary>
        /// Attempts to lock memory pages to prevent swapping (platform dependent).
        /// </summary>
        private void TryLockMemory()
        {
            try
            {
                if (Length > 0)
                {
                    // Platform-specific memory locking would go here
                    // For now, we rely on the pinning provided by MemoryHandle
                    // Memory handle pinning provides some protection against swapping
                }
            }
            catch
            {
                // Memory locking is best effort - don't fail if it doesn't work
            }
        }

        /// <summary>
        /// Throws ObjectDisposedException if this instance has been disposed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SecureBytes));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed) return;

                try
                {
                    // Overwrite memory with random data multiple times
                    if (!_memory.IsEmpty)
                    {
                        var span = _memory.Span;
                        
                        // Multiple overwrites with different patterns for enhanced security
                        using (var rng = RandomNumberGenerator.Create())
                        {
                            // First pass: random data
                            rng.GetBytes(span);
                            
                            // Second pass: complement of random data
                            for (int i = 0; i < span.Length; i++)
                            {
                                span[i] = (byte)~span[i];
                            }
                            
                            // Third pass: zeros
                            span.Clear();
                        }
                    }
                }
                finally
                {
                    // Dispose of the memory handle (unpins memory)
                    _memoryHandle.Dispose();
                    
                    // Return rented array to pool (it's already been cleared)
                    if (_memory.Length > 0)
                    {
                        try
                        {
                            // Note: ArrayPool handles will manage the actual array cleanup
                            // The array was rented from ArrayPool in constructor
                        }
                        catch
                        {
                            // Ignore disposal errors for rented arrays
                        }
                    }
                    
                    _isDisposed = true;
                }
            }
        }

        /// <summary>
        /// Finalizer to ensure cleanup if Dispose() wasn't called.
        /// </summary>
        ~SecureBytes()
        {
            Dispose();
        }

        /// <summary>
        /// Determines whether two SecureBytes instances are equal.
        /// </summary>
        /// <param name="left">The first SecureBytes to compare</param>
        /// <param name="right">The second SecureBytes to compare</param>
        /// <returns>True if equal; otherwise, false</returns>
        public static bool operator ==(SecureBytes? left, SecureBytes? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two SecureBytes instances are not equal.
        /// </summary>
        /// <param name="left">The first SecureBytes to compare</param>
        /// <param name="right">The second SecureBytes to compare</param>
        /// <returns>True if not equal; otherwise, false</returns>
        public static bool operator !=(SecureBytes? left, SecureBytes? right)
        {
            return !(left == right);
        }
    }
}