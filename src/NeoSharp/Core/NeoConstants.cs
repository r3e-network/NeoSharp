using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;

namespace NeoSharp.Core
{
    /// <summary>
    /// Constants used throughout the Neo system.
    /// </summary>
    public static class NeoConstants
    {
        /// <summary>
        /// The secp256r1 curve parameters.
        /// </summary>
        public static readonly X9ECParameters Secp256r1 = ECNamedCurveTable.GetByName("secp256r1");

        /// <summary>
        /// The secp256r1 domain parameters.
        /// </summary>
        public static readonly ECDomainParameters Secp256r1Domain = new ECDomainParameters(
            Secp256r1.Curve, Secp256r1.G, Secp256r1.N, Secp256r1.H);

        /// <summary>
        /// Half of the secp256r1 curve order (used for signature verification)
        /// </summary>
        public static readonly Org.BouncyCastle.Math.BigInteger Secp256r1HalfCurveOrder = 
            Secp256r1.N.ShiftRight(1);

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
        /// Size of a signature in bytes (r + s).
        /// </summary>
        public const int SignatureSize = 64;

        /// <summary>
        /// Maximum size of a transaction in bytes.
        /// </summary>
        public const int MaxTransactionSize = 102400; // 100KB

        /// <summary>
        /// Maximum number of attributes in a transaction.
        /// </summary>
        public const int MaxTransactionAttributes = 16;

        /// <summary>
        /// Maximum number of cosigners in a transaction.
        /// </summary>
        public const int MaxCosigners = 16;

        /// <summary>
        /// Maximum size of an invocation script.
        /// </summary>
        public const int MaxInvocationScriptSize = 1024;

        /// <summary>
        /// Maximum size of a verification script.
        /// </summary>
        public const int MaxVerificationScriptSize = 1024;

        /// <summary>
        /// NEO native contract hash.
        /// </summary>
        public const string NeoToken = "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5";

        /// <summary>
        /// GAS native contract hash.
        /// </summary>
        public const string GasToken = "0xd2a4cff31913016155e38e474a2c06d08be276cf";

        /// <summary>
        /// Policy native contract hash.
        /// </summary>
        public const string PolicyContract = "0xcc5e4edd9f5f8dba8bb65734541df7a1c081c67b";

        /// <summary>
        /// RoleManagement native contract hash.
        /// </summary>
        public const string RoleManagement = "0x49cf4e5378ffcd4dec034fd98a174c5491e395e2";

        /// <summary>
        /// Oracle native contract hash.
        /// </summary>
        public const string OracleContract = "0x627c76e0fc7fde0406f8cf0e9c9f6e9b5bc8f3f4";

        /// <summary>
        /// NameService native contract hash.
        /// </summary>
        public const string NameService = "0xe8e2240d97c6b5451a8f95de1bd648b970e2a5f3";

        /// <summary>
        /// Management native contract hash.
        /// </summary>
        public const string ManagementContract = "0xfffdc93764dbaddd97c48f252a53ea4643faa3fd";

        /// <summary>
        /// Ledger native contract hash.
        /// </summary>
        public const string LedgerContract = "0xda65b600f7124ce6c79950c1772a36403104f2be";

        /// <summary>
        /// CryptoLib native contract hash.
        /// </summary>
        public const string CryptoLib = "0x1bf5c17d6a0a788de30d8cbf3a86a3ea8af00e2a";

        /// <summary>
        /// StdLib native contract hash.
        /// </summary>
        public const string StdLib = "0xacce6fd80d44e1796aa0c2c625e9e4e0ce39efc0";

        /// <summary>
        /// Address version byte for Neo N3.
        /// </summary>
        public const byte AddressVersion = 0x35; // 53 in decimal, produces addresses starting with 'N'

        /// <summary>
        /// Maximum number of public keys in a multi-signature contract.
        /// </summary>
        public const int MaxMultisigPublicKeys = 1024;

        /// <summary>
        /// One GAS in fractions (10^8).
        /// </summary>
        public const long OneGas = 100_000_000;

        /// <summary>
        /// One NEO (always 1 since NEO is indivisible).
        /// </summary>
        public const long OneNeo = 1;
    }
}