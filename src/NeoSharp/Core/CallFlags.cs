using System;

namespace NeoSharp.Core
{
    /// <summary>
    /// Flags for contract calls
    /// </summary>
    [Flags]
    public enum CallFlags : byte
    {
        None = 0,
        ReadStates = 0x01,
        WriteStates = 0x02,
        AllowCall = 0x04,
        AllowNotify = 0x08,
        States = ReadStates | WriteStates,
        ReadOnly = ReadStates | AllowCall,
        All = States | AllowCall | AllowNotify
    }
}