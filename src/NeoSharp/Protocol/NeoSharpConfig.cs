using System;
using NeoSharp.Types;

namespace NeoSharp.Protocol
{
    /// <summary>
    /// Configuration for NeoSharp client.
    /// </summary>
    public class NeoSharpConfig
    {
        /// <summary>
        /// Default block time in milliseconds.
        /// </summary>
        public const int DEFAULT_BLOCK_TIME = 15000;

        /// <summary>
        /// Default polling interval in milliseconds.
        /// </summary>
        public const int DEFAULT_POLLING_INTERVAL = 1000;

        /// <summary>
        /// Maximum valid until block increment base.
        /// </summary>
        public const int MAX_VALID_UNTIL_BLOCK_INCREMENT_BASE = 5760;

        /// <summary>
        /// Default address version for Neo3.
        /// </summary>
        public const byte DEFAULT_ADDRESS_VERSION = 0x35;

        /// <summary>
        /// Gets or sets the NNS resolver hash.
        /// </summary>
        public Hash160 NnsResolver { get; set; }

        /// <summary>
        /// Gets or sets the block interval in milliseconds.
        /// </summary>
        public int BlockInterval { get; set; } = DEFAULT_BLOCK_TIME;

        /// <summary>
        /// Gets or sets the polling interval in milliseconds.
        /// </summary>
        public int PollingInterval { get; set; } = DEFAULT_POLLING_INTERVAL;

        /// <summary>
        /// Gets or sets the maximum valid until block increment.
        /// </summary>
        public int MaxValidUntilBlockIncrement { get; set; } = MAX_VALID_UNTIL_BLOCK_INCREMENT_BASE / DEFAULT_BLOCK_TIME * 1000;

        /// <summary>
        /// Gets or sets whether to allow transmission on fault.
        /// </summary>
        public bool AllowTransmissionOnFault { get; set; } = false;

        /// <summary>
        /// Gets or sets the network magic number.
        /// </summary>
        public uint NetworkMagic { get; set; } = 860833102; // Neo3 MainNet

        /// <summary>
        /// Gets or sets the address version.
        /// </summary>
        public byte AddressVersion { get; set; } = DEFAULT_ADDRESS_VERSION;

        /// <summary>
        /// Allows transmission of scripts that lead to a FAULT VM state.
        /// </summary>
        /// <returns>The updated configuration.</returns>
        public NeoSharpConfig EnableTransmissionOnFault()
        {
            AllowTransmissionOnFault = true;
            return this;
        }

        /// <summary>
        /// Sets the network to MainNet.
        /// </summary>
        /// <returns>The updated configuration.</returns>
        public NeoSharpConfig UseMainNet()
        {
            NetworkMagic = 860833102;
            return this;
        }

        /// <summary>
        /// Sets the network to TestNet.
        /// </summary>
        /// <returns>The updated configuration.</returns>
        public NeoSharpConfig UseTestNet()
        {
            NetworkMagic = 894710606;
            return this;
        }

        /// <summary>
        /// Sets a custom network magic.
        /// </summary>
        /// <param name="magic">The network magic number.</param>
        /// <returns>The updated configuration.</returns>
        public NeoSharpConfig UseCustomNet(uint magic)
        {
            NetworkMagic = magic;
            return this;
        }
    }
}