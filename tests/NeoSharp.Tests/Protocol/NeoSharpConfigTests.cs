using NeoSharp.Protocol;
using NeoSharp.Types;
using Xunit;

namespace NeoSharp.Tests.Protocol
{
    public class NeoSharpConfigTests
    {
        [Fact]
        public void DefaultConfiguration_UsesExpectedValues()
        {
            var config = new NeoSharpConfig();

            Assert.Equal(NeoSharpConfig.DEFAULT_BLOCK_TIME, config.BlockInterval);
            Assert.Equal(NeoSharpConfig.DEFAULT_POLLING_INTERVAL, config.PollingInterval);
            Assert.Equal(NeoSharpConfig.MAX_VALID_UNTIL_BLOCK_INCREMENT_BASE / NeoSharpConfig.DEFAULT_BLOCK_TIME, config.MaxValidUntilBlockIncrement);
            Assert.False(config.AllowTransmissionOnFault);
            Assert.Equal(NeoSharpConfig.DEFAULT_ADDRESS_VERSION, config.AddressVersion);
        }

        [Fact]
        public void EnableTransmissionOnFault_FlipsFlagAndReturnsConfig()
        {
            var config = new NeoSharpConfig();

            var result = config.EnableTransmissionOnFault();

            Assert.True(config.AllowTransmissionOnFault);
            Assert.Same(config, result);
        }

        [Fact]
        public void NetworkHelpers_SetExpectedMagic()
        {
            var config = new NeoSharpConfig();

            config.UseMainNet();
            Assert.Equal(860833102u, config.NetworkMagic);

            config.UseTestNet();
            Assert.Equal(894710606u, config.NetworkMagic);

            config.UseCustomNet(123456u);
            Assert.Equal(123456u, config.NetworkMagic);
        }

        [Fact]
        public void NnsResolver_CanBeAssigned()
        {
            var resolver = Hash160.Parse("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
            var config = new NeoSharpConfig { NnsResolver = resolver };

            Assert.Equal(resolver, config.NnsResolver);
        }
    }
}
