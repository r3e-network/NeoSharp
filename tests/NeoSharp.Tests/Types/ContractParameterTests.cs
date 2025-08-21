using System;
using System.Collections.Generic;
using System.Numerics;
using FluentAssertions;
using NeoSharp.Types;
using NeoSharp.Crypto;
using NeoSharp.Wallet;
using Xunit;

namespace NeoSharp.Tests.Types
{
    public class ContractParameterTests
    {
        [Fact]
        public void Boolean_ShouldCreateBooleanParameter()
        {
            // Act
            var param = ContractParameter.Boolean(true);

            // Assert
            param.Type.Should().Be(ContractParameterType.Boolean);
            param.Value.Should().Be(true);
        }

        [Fact]
        public void Integer_WithInt_ShouldCreateIntegerParameter()
        {
            // Act
            var param = ContractParameter.Integer(42);

            // Assert
            param.Type.Should().Be(ContractParameterType.Integer);
            param.Value.Should().Be(42);
        }

        [Fact]
        public void Integer_WithBigInteger_ShouldCreateIntegerParameter()
        {
            // Arrange
            var bigInt = BigInteger.Parse("123456789012345678901234567890");

            // Act
            var param = ContractParameter.Integer(bigInt);

            // Assert
            param.Type.Should().Be(ContractParameterType.Integer);
            param.Value.Should().Be(bigInt);
        }

        [Fact]
        public void String_ShouldCreateStringParameter()
        {
            // Act
            var param = ContractParameter.String("Hello, Neo!");

            // Assert
            param.Type.Should().Be(ContractParameterType.String);
            param.Value.Should().Be("Hello, Neo!");
        }

        [Fact]
        public void ByteArray_WithBytes_ShouldCreateByteArrayParameter()
        {
            // Arrange
            var bytes = new byte[] { 0x01, 0x02, 0x03 };

            // Act
            var param = ContractParameter.ByteArray(bytes);

            // Assert
            param.Type.Should().Be(ContractParameterType.ByteArray);
            param.Value.Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void Hash160_ShouldCreateHash160Parameter()
        {
            // Arrange
            var hash = Hash160.Parse("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");

            // Act
            var param = ContractParameter.Hash160(hash);

            // Assert
            param.Type.Should().Be(ContractParameterType.Hash160);
            param.Value.Should().Be(hash);
        }

        [Fact]
        public void Hash256_ShouldCreateHash256Parameter()
        {
            // Arrange
            var hash = Hash256.Parse("0x0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");

            // Act
            var param = ContractParameter.Hash256(hash);

            // Assert
            param.Type.Should().Be(ContractParameterType.Hash256);
            param.Value.Should().Be(hash);
        }

        [Fact]
        public void Array_ShouldCreateArrayParameter()
        {
            // Act
            var param = ContractParameter.Array(42, "test", true);

            // Assert
            param.Type.Should().Be(ContractParameterType.Array);
            param.Value.Should().BeOfType<ContractParameter[]>();
            
            var array = param.Value as ContractParameter[];
            array.Should().HaveCount(3);
            array[0].Type.Should().Be(ContractParameterType.Integer);
            array[0].Value.Should().Be(42);
            array[1].Type.Should().Be(ContractParameterType.String);
            array[1].Value.Should().Be("test");
            array[2].Type.Should().Be(ContractParameterType.Boolean);
            array[2].Value.Should().Be(true);
        }

        [Fact]
        public void MapToContractParameter_ShouldHandleVariousTypes()
        {
            // Test null
            var nullParam = ContractParameter.MapToContractParameter(null);
            nullParam.Type.Should().Be(ContractParameterType.Any);
            nullParam.Value.Should().BeNull();

            // Test boolean
            var boolParam = ContractParameter.MapToContractParameter(true);
            boolParam.Type.Should().Be(ContractParameterType.Boolean);
            boolParam.Value.Should().Be(true);

            // Test integer
            var intParam = ContractParameter.MapToContractParameter(123);
            intParam.Type.Should().Be(ContractParameterType.Integer);
            intParam.Value.Should().Be(123);

            // Test string
            var stringParam = ContractParameter.MapToContractParameter("test");
            stringParam.Type.Should().Be(ContractParameterType.String);
            stringParam.Value.Should().Be("test");

            // Test byte array
            var bytes = new byte[] { 1, 2, 3 };
            var byteParam = ContractParameter.MapToContractParameter(bytes);
            byteParam.Type.Should().Be(ContractParameterType.ByteArray);
            byteParam.Value.Should().BeEquivalentTo(bytes);
        }

        [Fact]
        public void ToJson_Boolean_ShouldSerializeCorrectly()
        {
            // Arrange
            var param = ContractParameter.Boolean(true);

            // Act
            var json = param.ToJson();

            // Assert
            json["type"].Should().Be("Boolean");
            json["value"].Should().Be(true);
        }

        [Fact]
        public void ToJson_Integer_ShouldSerializeCorrectly()
        {
            // Arrange
            var param = ContractParameter.Integer(42);

            // Act
            var json = param.ToJson();

            // Assert
            json["type"].Should().Be("Integer");
            json["value"].Should().Be("42");
        }

        [Fact]
        public void ToJson_String_ShouldSerializeCorrectly()
        {
            // Arrange
            var param = ContractParameter.String("Hello");

            // Act
            var json = param.ToJson();

            // Assert
            json["type"].Should().Be("String");
            json["value"].Should().Be("Hello");
        }

        [Fact]
        public void ToJson_ByteArray_ShouldSerializeAsBase64()
        {
            // Arrange
            var bytes = new byte[] { 1, 2, 3, 4 };
            var param = ContractParameter.ByteArray(bytes);

            // Act
            var json = param.ToJson();

            // Assert
            json["type"].Should().Be("ByteArray");
            json["value"].Should().Be(Convert.ToBase64String(bytes));
        }

        [Fact]
        public void ToJson_Hash160_ShouldSerializeCorrectly()
        {
            // Arrange
            var hash = Hash160.Parse("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
            var param = ContractParameter.Hash160(hash);

            // Act
            var json = param.ToJson();

            // Assert
            json["type"].Should().Be("Hash160");
            json["value"].Should().Be(hash.ToString());
        }

        [Fact]
        public void ToJson_Array_ShouldSerializeCorrectly()
        {
            // Arrange
            var param = ContractParameter.Array(
                ContractParameter.Integer(1),
                ContractParameter.String("test"),
                ContractParameter.Boolean(true)
            );

            // Act
            var json = param.ToJson();

            // Assert
            json["type"].Should().Be("Array");
            json["value"].Should().BeOfType<List<Dictionary<string, object>>>();
            
            var array = json["value"] as List<Dictionary<string, object>>;
            array.Should().HaveCount(3);
            array[0]["type"].Should().Be("Integer");
            array[0]["value"].Should().Be("1");
            array[1]["type"].Should().Be("String");
            array[1]["value"].Should().Be("test");
            array[2]["type"].Should().Be("Boolean");
            array[2]["value"].Should().Be(true);
        }

        [Fact]
        public void Equals_ShouldWorkCorrectly()
        {
            // Arrange
            var param1 = new ContractParameter("test", ContractParameterType.String, "value");
            var param2 = new ContractParameter("test", ContractParameterType.String, "value");
            var param3 = new ContractParameter("test", ContractParameterType.String, "different");
            var param4 = new ContractParameter("other", ContractParameterType.String, "value");

            // Act & Assert
            param1.Equals(param2).Should().BeTrue();
            param1.Equals(param3).Should().BeFalse();
            param1.Equals(param4).Should().BeFalse();
            (param1 == param2).Should().BeTrue();
            (param1 != param3).Should().BeTrue();
        }
    }
}