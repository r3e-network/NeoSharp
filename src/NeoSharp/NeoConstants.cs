namespace NeoSharp
{
    /// <summary>
    /// Neo protocol constants.
    /// </summary>
    public static class NeoConstants
    {
        /// <summary>
        /// Maximum array size.
        /// </summary>
        public const int MaxArraySize = 1024;

        /// <summary>
        /// Maximum item size.
        /// </summary>
        public const int MaxItemSize = 1024 * 1024;

        /// <summary>
        /// Magic number for Neo MainNet.
        /// </summary>
        public const uint MainNetMagic = 0x334F454E; // 860833102

        /// <summary>
        /// Magic number for Neo TestNet.
        /// </summary>
        public const uint TestNetMagic = 0x3554334E; // 894710606

        /// <summary>
        /// Address version for Neo3.
        /// </summary>
        public const byte AddressVersion = 0x35;

        /// <summary>
        /// Size of a signature in bytes.
        /// </summary>
        public const int SignatureSize = 64;

        /// <summary>
        /// Size of a compressed public key in bytes.
        /// </summary>
        public const int PublicKeySizeCompressed = 33;

        /// <summary>
        /// Size of an uncompressed public key in bytes.
        /// </summary>
        public const int PublicKeySizeUncompressed = 65;

        /// <summary>
        /// Size of a private key in bytes.
        /// </summary>
        public const int PrivateKeySize = 32;

        /// <summary>
        /// Size of a Hash160 in bytes.
        /// </summary>
        public const int Hash160Size = 20;

        /// <summary>
        /// Size of a Hash256 in bytes.
        /// </summary>
        public const int Hash256Size = 32;

        /// <summary>
        /// Maximum stack size.
        /// </summary>
        public const int MaxStackSize = 2048;

        /// <summary>
        /// Maximum invocation stack size.
        /// </summary>
        public const int MaxInvocationStackSize = 1024;

        /// <summary>
        /// Maximum contract description length.
        /// </summary>
        public const int MaxContractDescriptionLength = 65536;

        /// <summary>
        /// Maximum manifest size.
        /// </summary>
        public const int MaxManifestSize = 65536;

        /// <summary>
        /// Native contract names.
        /// </summary>
        public static class NativeContracts
        {
            public const string ContractManagement = "ContractManagement";
            public const string CryptoLib = "CryptoLib";
            public const string GasToken = "GasToken";
            public const string LedgerContract = "Ledger";
            public const string NeoToken = "NeoToken";
            public const string OracleContract = "Oracle";
            public const string PolicyContract = "Policy";
            public const string RoleManagement = "RoleManagement";
            public const string StdLib = "StdLib";
        }

        /// <summary>
        /// Native contract hashes.
        /// </summary>
        public static class NativeContractHashes
        {
            public const string ContractManagement = "0xfffdc93764dbaddd97c48f252a53ea4643faa3fd";
            public const string CryptoLib = "0x726cb6e0cd8628a1350a611384688911ab75f51b";
            public const string GasToken = "0xd2a4cff31913016155e38e474a2c06d08be276cf";
            public const string LedgerContract = "0xda65b600f7124ce6c79950c1772a36403104f2be";
            public const string NeoToken = "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5";
            public const string OracleContract = "0x49cf4e5378ffcd4dec034fd98a174c5491e395e2";
            public const string PolicyContract = "0xcc5e4edd9f5f8dba8bb65734541df7a1c081c67b";
            public const string RoleManagement = "0x49cf4e5378ffcd4dec034fd98a174c5491e395e2";
            public const string StdLib = "0xacce6fd80d44e1796aa0c2c625e9e4e0ce39efc0";
        }
    }
}