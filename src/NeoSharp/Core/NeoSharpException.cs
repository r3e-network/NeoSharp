using System;

namespace NeoSharp.Core
{
    /// <summary>
    /// Base exception class for NeoSharp-related errors
    /// </summary>
    public class NeoSharpException : Exception
    {
        public NeoSharpException() : base() { }
        
        public NeoSharpException(string message) : base(message) { }
        
        public NeoSharpException(string message, Exception innerException) : base(message, innerException) { }
    }
}