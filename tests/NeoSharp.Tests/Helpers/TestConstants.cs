using System;
using System.Collections.Generic;

namespace NeoSharp.Tests.Helpers
{
    /// <summary>
    /// Test constants and utilities for NeoSharp tests.
    /// Converted from Swift TestProperties.swift
    /// </summary>
    public static class TestConstants
    {
        // MARK: Native Contracts - Public blockchain constants
        public const string ContractManagementHash = "fffdc93764dbaddd97c48f252a53ea4643faa3fd";
        public const string StdLibHash = "acce6fd80d44e1796aa0c2c625e9e4e0ce39efc0";
        public const string CryptoLibHash = "726cb6e0cd8628a1350a611384688911ab75f51b";
        public const string LedgerContractHash = "da65b600f7124ce6c79950c1772a36403104f2be";
        public const string NeoTokenHash = "ef4073a0f2b305a38ec4050e4d3d28bc40ea63f5";
        public const string GasTokenHash = "d2a4cff31913016155e38e474a2c06d08be276cf";
        public const string GasTokenName = "GasToken";
        public const string PolicyContractHash = "cc5e4edd9f5f8dba8bb65734541df7a1c081c67b";
        public const string RoleManagementHash = "49cf4e5378ffcd4dec034fd98a174c5491e395e2";
        public const string OracleContractHash = "fe924b7cfe89ddd271abaf7210a80a7e11178758";
        public const string NameServiceHash = "7a8fcf0392cd625647907afa8e45cc66872b596b";

        // Test account constants (safe test values) - Updated to be consistent with private key
        public const string DefaultAccountAddress = "2e1hLQVqdSvizuTzKxXTraC9FzijLQuhavAT";
        public const string DefaultAccountScriptHash = "7f4896875d05a596a413300712c964998ebec290";
        public const string DefaultAccountVerificationScript = "0c2102c0b60c995bc092e866f15a37c176bb59b7ebacf069ba94c0ebf561cb8f95623841138defaf";
        public const string DefaultAccountPublicKey = "02c0b60c995bc092e866f15a37c176bb59b7ebacf069ba94c0ebf561cb8f956238";
        public const string DefaultAccountPrivateKey = "e6e919577dd7b8e97805151c05ae07ff4f752654d6d8797597aca989c02c4cb3";
        public const string DefaultAccountWIF = "L4xa4S78qj87q9FRkMQDeZsrymQG6ThR5oczagNNNnBrWRjicF36";
        public const string DefaultAccountPassword = "123";

        public const string CommitteeAccountAddress = "NUnP2fsptTkqgXVL4sJ6gQrLF1LdNTnYfj";
        public const string CommitteeAccountScriptHash = "6c29bb81b9ac6e2b2d3a65ac4cc62b90b4fdc298";
        public const string CommitteeAccountVerificationScript = "110c21026aa8fe6b4360a67a530e23c08c6a72525afde34719c5436f9d3ced759f939a3d110b41138defaf";

        public const string Client1AccountWIF = "L3j3AKCiDEoaaDFLfFGb9pDhQrNrJaHxrB1xJgQqaE5yxTPtgQAC";

        // Network constants
        public const byte NeoAddressVersion = 0x35;
        public const int DefaultMagic = 0x00746E41; // "Ant" in little endian

        // Token constants
        public const int NeoTokenDecimals = 0;
        public const int GasTokenDecimals = 8;
        public const long NeoTokenTotalSupply = 100_000_000;
        public const long GasTokenTotalSupply = 52_000_000;

        // Common test data
        public static readonly byte[] TestBytes20 = new byte[]
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,
            0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14
        };

        public static readonly byte[] TestBytes32 = new byte[]
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a,
            0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14,
            0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e,
            0x1f, 0x20
        };

        // Script OpCodes
        public static class OpCode
        {
            public const byte PushData1 = 0x0c;
            public const byte SysCall = 0x41;
            public const byte Push1 = 0x51;
            public const byte Push0 = 0x50;
            public const byte Ret = 0x40;
        }

        // InteropService hashes
        public static class InteropService
        {
            public const string SystemCryptoCheckSig = "System.Crypto.CheckSig";
            public const uint SystemCryptoCheckSigHash = 0x41627d5b;
        }

        /// <summary>
        /// Helper method to convert hex string to byte array
        /// </summary>
        public static byte[] HexToBytes(string hex)
        {
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2);
            
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Helper method to convert byte array to hex string
        /// </summary>
        public static string BytesToHex(byte[] bytes, bool withPrefix = false)
        {
            var hex = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            return withPrefix ? "0x" + hex : hex;
        }

        /// <summary>
        /// Helper method for asserting error messages in tests
        /// </summary>
        public static void AssertErrorMessage<T>(string expectedMessage, Func<T> expression) where T : class
        {
            try
            {
                expression();
                throw new Xunit.Sdk.XunitException("Expected exception was not thrown");
            }
            catch (Exception ex) when (!(ex is Xunit.Sdk.XunitException))
            {
                if (!ex.Message.Contains(expectedMessage))
                {
                    throw new Xunit.Sdk.XunitException($"Expected message '{expectedMessage}' but got '{ex.Message}'");
                }
            }
        }
    }
}