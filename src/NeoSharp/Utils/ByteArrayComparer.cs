using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoSharp.Utils
{
    /// <summary>
    /// Comparer for byte arrays
    /// </summary>
    public class ByteArrayComparer : IComparer<byte[]>
    {
        /// <summary>
        /// Default instance of ByteArrayComparer
        /// </summary>
        public static readonly ByteArrayComparer Default = new ByteArrayComparer();

        /// <summary>
        /// Compares two byte arrays lexicographically
        /// </summary>
        /// <param name="x">First byte array</param>
        /// <param name="y">Second byte array</param>
        /// <returns>Comparison result</returns>
        public int Compare(byte[]? x, byte[]? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int minLength = Math.Min(x.Length, y.Length);
            
            for (int i = 0; i < minLength; i++)
            {
                int comparison = x[i].CompareTo(y[i]);
                if (comparison != 0)
                    return comparison;
            }

            return x.Length.CompareTo(y.Length);
        }
    }
}