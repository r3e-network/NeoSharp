namespace NeoSharp.Types
{
    /// <summary>
    /// Neo interop service identifiers
    /// </summary>
    public static class InteropService
    {
        /// <summary>
        /// System calls
        /// </summary>
        public static class System
        {
            public const int Runtime_Platform = 0x49447520;
            public const int Runtime_GetTrigger = 0x2DA99683;
            public const int Runtime_GetTime = unchecked((int)0xE5A72D57);
            public const int Runtime_GetScriptContainer = 0x2C25ADE4;
            public const int Runtime_GetExecutingScriptHash = 0x0F8B8CD4;
            public const int Runtime_GetCallingScriptHash = 0x2B83BAE9;
            public const int Runtime_GetEntryScriptHash = 0x4ACFB1B2;
            public const int Runtime_CheckWitness = 0x41627D9B;
            public const int Runtime_GetInvocationCounter = unchecked((int)0xC8D2FA63);
            public const int Runtime_Log = 0x40010EFA;
            public const int Runtime_Notify = unchecked((int)0xBAF8F9F4);
            public const int Runtime_GetNotifications = unchecked((int)0x8DCA3018);
            public const int Runtime_GasLeft = 0x28C9D7ED;
        }

        /// <summary>
        /// Crypto calls
        /// </summary>
        public static class Crypto
        {
            public const int CheckSig = 0x5C1F5FC1;
            public const int CheckMultisig = 0x7FC75C4F;
        }

        /// <summary>
        /// Contract calls
        /// </summary>
        public static class Contract
        {
            public const int Call = unchecked((int)0x98C90D39);
            public const int CallEx = 0x78A7BBEF;
            public const int IsStandard = 0x7F73B1E1;
            public const int GetCallFlags = 0x4BA2F1C7;
        }
    }
}