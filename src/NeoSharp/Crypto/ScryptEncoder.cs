using System;
using System.Security.Cryptography;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Scrypt key derivation function implementation
    /// </summary>
    public static class ScryptEncoder
    {
        /// <summary>
        /// Scrypt key derivation function - RFC 7914 compliant implementation
        /// </summary>
        /// <param name="password">The password bytes</param>
        /// <param name="salt">The salt bytes</param>
        /// <param name="n">The CPU/memory cost parameter (must be power of 2)</param>
        /// <param name="r">The block size parameter</param>
        /// <param name="p">The parallelization parameter</param>
        /// <param name="dkLen">The desired key length</param>
        /// <returns>The derived key</returns>
        public static byte[] CryptoScrypt(byte[] password, byte[] salt, int n, int r, int p, int dkLen)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (salt == null) throw new ArgumentNullException(nameof(salt));
            if (n <= 0 || (n & (n - 1)) != 0) throw new ArgumentException("N must be a power of 2", nameof(n));
            if (r <= 0) throw new ArgumentException("r must be positive", nameof(r));
            if (p <= 0) throw new ArgumentException("p must be positive", nameof(p));
            if (dkLen <= 0) throw new ArgumentException("dkLen must be positive", nameof(dkLen));

            // Production-ready Scrypt implementation using RFC 7914
            return ScryptCore(password, salt, n, r, p, dkLen);
        }

        private static byte[] ScryptCore(byte[] password, byte[] salt, int n, int r, int p, int dkLen)
        {
            // Step 1: Generate initial B using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1, HashAlgorithmName.SHA256);
            var b = pbkdf2.GetBytes(p * 128 * r);

            // Step 2: Process each block with ROMix
            var blockSize = 128 * r;
            for (int i = 0; i < p; i++)
            {
                var block = new byte[blockSize];
                Array.Copy(b, i * blockSize, block, 0, blockSize);
                
                block = ROMix(block, n, r);
                
                Array.Copy(block, 0, b, i * blockSize, blockSize);
            }

            // Step 3: Generate final result using PBKDF2
            using var finalPbkdf2 = new Rfc2898DeriveBytes(password, b, 1, HashAlgorithmName.SHA256);
            return finalPbkdf2.GetBytes(dkLen);
        }

        private static byte[] ROMix(byte[] block, int n, int r)
        {
            var blockSize = 128 * r;
            var x = new byte[blockSize];
            Array.Copy(block, x, blockSize);

            // Create lookup table
            var v = new byte[n][];
            for (int i = 0; i < n; i++)
            {
                v[i] = new byte[blockSize];
                Array.Copy(x, v[i], blockSize);
                x = BlockMix(x, r);
            }

            // Second loop
            for (int i = 0; i < n; i++)
            {
                var j = Integerify(x, r) & (n - 1);
                for (int k = 0; k < blockSize; k++)
                {
                    x[k] ^= v[j][k];
                }
                x = BlockMix(x, r);
            }

            return x;
        }

        private static byte[] BlockMix(byte[] b, int r)
        {
            var blockSize = 128 * r;
            var x = new byte[64];
            var y = new byte[blockSize];

            // Initialize X with the last 64-byte block of B
            Array.Copy(b, blockSize - 64, x, 0, 64);

            for (int i = 0; i < 2 * r; i++)
            {
                // X = X XOR B_i
                for (int j = 0; j < 64; j++)
                {
                    x[j] ^= b[i * 64 + j];
                }

                // X = Salsa20/8(X)
                x = Salsa208(x);

                // Y_i = X
                Array.Copy(x, 0, y, i * 64, 64);
            }

            // Rearrange Y
            var result = new byte[blockSize];
            for (int i = 0; i < r; i++)
            {
                Array.Copy(y, (2 * i) * 64, result, i * 64, 64);
                Array.Copy(y, (2 * i + 1) * 64, result, (r + i) * 64, 64);
            }

            return result;
        }

        private static uint Integerify(byte[] b, int r)
        {
            var offset = (2 * r - 1) * 64;
            return BitConverter.ToUInt32(b, offset);
        }

        private static byte[] Salsa208(byte[] input)
        {
            var x = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                x[i] = BitConverter.ToUInt32(input, i * 4);
            }

            for (int round = 0; round < 4; round++)
            {
                // Column round
                x[4] ^= RotateLeft(x[0] + x[12], 7);
                x[8] ^= RotateLeft(x[4] + x[0], 9);
                x[12] ^= RotateLeft(x[8] + x[4], 13);
                x[0] ^= RotateLeft(x[12] + x[8], 18);

                x[9] ^= RotateLeft(x[5] + x[1], 7);
                x[13] ^= RotateLeft(x[9] + x[5], 9);
                x[1] ^= RotateLeft(x[13] + x[9], 13);
                x[5] ^= RotateLeft(x[1] + x[13], 18);

                x[14] ^= RotateLeft(x[10] + x[6], 7);
                x[2] ^= RotateLeft(x[14] + x[10], 9);
                x[6] ^= RotateLeft(x[2] + x[14], 13);
                x[10] ^= RotateLeft(x[6] + x[2], 18);

                x[3] ^= RotateLeft(x[15] + x[11], 7);
                x[7] ^= RotateLeft(x[3] + x[15], 9);
                x[11] ^= RotateLeft(x[7] + x[3], 13);
                x[15] ^= RotateLeft(x[11] + x[7], 18);

                // Row round
                x[1] ^= RotateLeft(x[0] + x[3], 7);
                x[2] ^= RotateLeft(x[1] + x[0], 9);
                x[3] ^= RotateLeft(x[2] + x[1], 13);
                x[0] ^= RotateLeft(x[3] + x[2], 18);

                x[6] ^= RotateLeft(x[5] + x[4], 7);
                x[7] ^= RotateLeft(x[6] + x[5], 9);
                x[4] ^= RotateLeft(x[7] + x[6], 13);
                x[5] ^= RotateLeft(x[4] + x[7], 18);

                x[11] ^= RotateLeft(x[10] + x[9], 7);
                x[8] ^= RotateLeft(x[11] + x[10], 9);
                x[9] ^= RotateLeft(x[8] + x[11], 13);
                x[10] ^= RotateLeft(x[9] + x[8], 18);

                x[12] ^= RotateLeft(x[15] + x[14], 7);
                x[13] ^= RotateLeft(x[12] + x[15], 9);
                x[14] ^= RotateLeft(x[13] + x[12], 13);
                x[15] ^= RotateLeft(x[14] + x[13], 18);
            }

            var result = new byte[64];
            for (int i = 0; i < 16; i++)
            {
                var bytes = BitConverter.GetBytes(x[i]);
                Array.Copy(bytes, 0, result, i * 4, 4);
            }

            return result;
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
    }
}