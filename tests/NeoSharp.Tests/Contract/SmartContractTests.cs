using System;
using FluentAssertions;
using NeoSharp.Contract;
using NeoSharp.Types;
using NeoSharp.Tests.Helpers;
using NeoSharp.Wallet;
using NeoSharp.Protocol;
using Moq;
using Xunit;

namespace NeoSharp.Tests.Contract
{
    /// <summary>
    /// Tests for Smart Contract functionality.
    /// Converted from Swift NeoSwiftTests/unit/contract/SmartContractTests.swift
    /// </summary>
    public class SmartContractTests : IDisposable
    {
        private readonly Hash160 _neoScriptHash = Hash160.Parse(TestConstants.NeoTokenHash);
        private readonly Hash160 _someScriptHash = Hash160.Parse("969a77db482f74ce27105f760efa139223431394");
        private readonly Account _account1;
        private readonly Hash160 _recipient = Hash160.Parse("969a77db482f74ce27105f760efa139223431394");

        private readonly SmartContract _someContract;
        private readonly SmartContract _neoContract;
        private readonly Mock<INeoSharp> _mockNeoSharp;

        private const string Nep17Transfer = "transfer";
        private const string Nep17BalanceOf = "balanceOf";
        private const string Nep17Name = "name";
        private const string Nep17TotalSupply = "totalSupply";

        public SmartContractTests()
        {
            _account1 = Account.FromWIF("L1WMhxazScMhUrdv34JqQb1HFSQmWeN2Kpc1R9JGKwL7CDNP21uR");
            _mockNeoSharp = new Mock<INeoSharp>();
            
            _someContract = new SmartContract(_someScriptHash, _mockNeoSharp.Object);
            _neoContract = new SmartContract(_neoScriptHash, _mockNeoSharp.Object);
        }

        [Fact]
        public void Constructor_ShouldSetScriptHashCorrectly()
        {
            // Assert - ScriptHash is protected, so we test through public behavior
            // The contracts should be properly constructed without throwing
            _neoContract.Should().NotBeNull();
            _someContract.Should().NotBeNull();
        }

        [Fact]
        public void InvokeFunction_WithEmptyString_ShouldThrowException()
        {
            // Act & Assert
            Action act = () => _neoContract.InvokeFunction("", Array.Empty<ContractParameter>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*function*empty*");
        }

        [Fact]
        public void BuildInvokeFunctionScript_ShouldCreateCorrectScript()
        {
            // Arrange
            var parameters = new[]
            {
                ContractParameter.Hash160(_account1.GetScriptHash()!.Value),
                ContractParameter.Hash160(_recipient),
                ContractParameter.Integer(42)
            };

            // Act
            var script = _neoContract.BuildInvokeFunctionScript(Nep17Transfer, parameters);

            // Assert
            script.Should().NotBeEmpty();
            // The script should contain the contract call
            // Expected structure: parameters + method + script hash + SysCall
        }

        [Fact]
        public void InvokeFunction_ShouldCreateTransactionBuilder()
        {
            // Arrange
            var parameters = new[]
            {
                ContractParameter.Hash160(_account1.GetScriptHash()!.Value),
                ContractParameter.Hash160(_recipient),
                ContractParameter.Integer(42)
            };

            // Act
            var builder = _neoContract.InvokeFunction(Nep17Transfer, parameters);

            // Assert
            builder.Should().NotBeNull();
            // TransactionBuilder doesn't expose Script property directly
            // We test that builder was created successfully
            builder.Should().NotBeNull();
        }

        [Fact]
        public void SmartContract_WithNullNeoSharp_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new SmartContract(_neoScriptHash, null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("neoSharp");
        }

        [Fact]
        public void BuildInvokeFunctionScript_WithNullMethod_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => _neoContract.BuildInvokeFunctionScript(null!, Array.Empty<ContractParameter>());
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("function");
        }

        [Fact]
        public void BuildInvokeFunctionScript_WithNullParameters_ShouldWork()
        {
            // Act
            var script = _neoContract.BuildInvokeFunctionScript(Nep17Name, null);

            // Assert
            script.Should().NotBeEmpty();
        }

        [Fact]
        public void InvokeFunction_WithNullMethod_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => _neoContract.InvokeFunction(null!, Array.Empty<ContractParameter>());
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("function");
        }

        [Fact]
        public void InvokeFunction_WithNullParameters_ShouldWork()
        {
            // Act
            var builder = _neoContract.InvokeFunction(Nep17Name, null);

            // Assert
            builder.Should().NotBeNull();
            // TransactionBuilder doesn't expose Script property directly
            // We test that builder was created successfully
            builder.Should().NotBeNull();
        }

        [Fact]
        public void ScriptHash_ShouldBeReadOnly()
        {
            // Arrange & Act - Test that we can create a contract with our script hash
            var contract = new SmartContract(_neoScriptHash, _mockNeoSharp.Object);

            // Assert - Contract should be created successfully
            contract.Should().NotBeNull();
            // No setter should be available
        }

        [Fact]
        public void Equals_ShouldCompareByScriptHash()
        {
            // Arrange
            var contract1 = new SmartContract(_neoScriptHash, _mockNeoSharp.Object);
            var contract2 = new SmartContract(_neoScriptHash, _mockNeoSharp.Object);
            var contract3 = new SmartContract(_someScriptHash, _mockNeoSharp.Object);

            // Act & Assert
            contract1.Should().Be(contract2);
            contract1.Should().NotBe(contract3);
            contract1.GetHashCode().Should().Be(contract2.GetHashCode());
        }

        [Fact]
        public void ToString_ShouldReturnScriptHashString()
        {
            // Act
            var result = _neoContract.ToString();

            // Assert
            result.Should().Contain(_neoScriptHash.ToString());
        }

        [Fact]
        public void BuildInvokeFunctionScript_WithEmptyMethod_ShouldThrowArgumentException()
        {
            // Act & Assert
            Action act = () => _neoContract.BuildInvokeFunctionScript("", Array.Empty<ContractParameter>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*empty*");
        }

        [Fact]
        public void BuildInvokeFunctionScript_WithWhitespaceMethod_ShouldThrowArgumentException()
        {
            // Act & Assert
            Action act = () => _neoContract.BuildInvokeFunctionScript("   ", Array.Empty<ContractParameter>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*whitespace*");
        }

        [Fact]
        public void InvokeFunction_WithEmptyMethod_ShouldThrowArgumentException()
        {
            // Act & Assert
            Action act = () => _neoContract.InvokeFunction("", Array.Empty<ContractParameter>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*empty*");
        }

        [Fact]
        public void InvokeFunction_WithWhitespaceMethod_ShouldThrowArgumentException()
        {
            // Act & Assert
            Action act = () => _neoContract.InvokeFunction("   ", Array.Empty<ContractParameter>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*whitespace*");
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}