using FluentAssertions;
using NeoSharp.Types;
using NeoSharp.Script;
using NeoSharp.Transaction;
using Xunit;

namespace NeoSharp.Tests.Types
{
    /// <summary>
    /// Enum type tests converted from Swift NeoSwift EnumTypeTests.swift
    /// </summary>
    public class EnumTypeTests
    {
        [Fact]
        public void ContractParameterType_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var stringType = ContractParameterType.String;

            // Assert
            stringType.GetJsonValue().Should().Be("String");
            ((byte)stringType).Should().Be(0x13);
            
            ContractParameterTypeExtensions.FromJsonValue("String").Should().Be(stringType);
            
            // Test invalid values throw exceptions
            Assert.Throws<ArgumentException>(() => ContractParameterTypeExtensions.FromJsonValue("Invalid"));
        }

        [Theory]
        [InlineData(ContractParameterType.Any, "Any", 0x00)]
        [InlineData(ContractParameterType.Boolean, "Boolean", 0x10)]
        [InlineData(ContractParameterType.Integer, "Integer", 0x11)]
        [InlineData(ContractParameterType.ByteArray, "ByteArray", 0x12)]
        [InlineData(ContractParameterType.String, "String", 0x13)]
        [InlineData(ContractParameterType.Hash160, "Hash160", 0x14)]
        [InlineData(ContractParameterType.Hash256, "Hash256", 0x15)]
        [InlineData(ContractParameterType.PublicKey, "PublicKey", 0x16)]
        [InlineData(ContractParameterType.Signature, "Signature", 0x17)]
        [InlineData(ContractParameterType.Array, "Array", 0x20)]
        [InlineData(ContractParameterType.Map, "Map", 0x22)]
        public void ContractParameterType_AllValues_ShouldHaveCorrectProperties(ContractParameterType paramType, string expectedJson, byte expectedByte)
        {
            // Assert
            paramType.GetJsonValue().Should().Be(expectedJson);
            ((byte)paramType).Should().Be(expectedByte);
        }

        [Fact]
        public void WitnessScope_ShouldWorkCorrectly()
        {
            // Test enum values and their byte representations
            ((byte)WitnessScope.None).Should().Be(0x00);
            ((byte)WitnessScope.CalledByEntry).Should().Be(0x01);
            ((byte)WitnessScope.CustomContracts).Should().Be(0x10);
            ((byte)WitnessScope.CustomGroups).Should().Be(0x20);
            ((byte)WitnessScope.WitnessRules).Should().Be(0x40);
            ((byte)WitnessScope.Global).Should().Be(0x80);
        }

        [Fact]
        public void OpCode_ShouldHaveCorrectByteValues()
        {
            // Test a few key opcodes
            ((byte)OpCode.Push0).Should().Be(0x10);
            ((byte)OpCode.Push1).Should().Be(0x11);
            ((byte)OpCode.PushData1).Should().Be(0x0C);
            ((byte)OpCode.PushData2).Should().Be(0x0D);
            ((byte)OpCode.PushData4).Should().Be(0x0E);
            ((byte)OpCode.Ret).Should().Be(0x40);
            ((byte)OpCode.SysCall).Should().Be(0x41);
        }

        [Fact]
        public void TransactionAttributeType_ShouldWorkCorrectly()
        {
            // Test transaction attribute types
            ((byte)TransactionAttributeType.HighPriority).Should().Be(0x01);
            ((byte)TransactionAttributeType.OracleResponse).Should().Be(0x11);
        }

        [Fact]
        public void ContractParameterType_Collections_ShouldBeDetected()
        {
            // Test collection detection
            ContractParameterType.Array.IsCollection().Should().BeTrue();
            ContractParameterType.Map.IsCollection().Should().BeTrue();
            ContractParameterType.String.IsCollection().Should().BeFalse();
            ContractParameterType.Integer.IsCollection().Should().BeFalse();
        }

        [Fact]
        public void ContractParameterType_Hashes_ShouldBeDetected()
        {
            // Test hash detection
            ContractParameterType.Hash160.IsHash().Should().BeTrue();
            ContractParameterType.Hash256.IsHash().Should().BeTrue();
            ContractParameterType.String.IsHash().Should().BeFalse();
            ContractParameterType.ByteArray.IsHash().Should().BeFalse();
        }

        [Fact]
        public void ContractParameterType_BinaryData_ShouldBeDetected()
        {
            // Test binary data detection
            ContractParameterType.ByteArray.IsBinaryData().Should().BeTrue();
            ContractParameterType.Signature.IsBinaryData().Should().BeTrue();
            ContractParameterType.PublicKey.IsBinaryData().Should().BeTrue();
            ContractParameterType.Hash160.IsBinaryData().Should().BeTrue();
            ContractParameterType.Hash256.IsBinaryData().Should().BeTrue();
            ContractParameterType.String.IsBinaryData().Should().BeFalse();
            ContractParameterType.Integer.IsBinaryData().Should().BeFalse();
        }

        [Theory]
        [InlineData("String")]
        [InlineData("Integer")]
        [InlineData("Boolean")]
        [InlineData("Array")]
        [InlineData("Map")]
        public void ContractParameterType_FromJsonValue_WithValidValues_ShouldSucceed(string jsonValue)
        {
            // Act & Assert - Should not throw
            var result = ContractParameterTypeExtensions.FromJsonValue(jsonValue);
            result.GetJsonValue().Should().Be(jsonValue);
        }
    }
}