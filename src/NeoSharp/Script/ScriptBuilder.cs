using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NeoSharp.Core;
using NeoSharp.Crypto;
using NeoSharp.Serialization;
using NeoSharp.Types;
using NeoSharp.Utils;

namespace NeoSharp.Script
{
    /// <summary>
    /// Builder for Neo VM scripts
    /// </summary>
    public sealed class ScriptBuilder : IDisposable
    {
        private readonly Serialization.BinaryWriter _writer;
        private bool _disposed;

        /// <summary>
        /// Gets the current length of the script
        /// </summary>
        public int Length => (int)_writer.Length;

        /// <summary>
        /// Initializes a new ScriptBuilder
        /// </summary>
        public ScriptBuilder()
        {
            _writer = new Serialization.BinaryWriter();
        }

        /// <summary>
        /// Appends OpCodes to the script
        /// </summary>
        /// <param name="opCodes">The OpCodes to append</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder OpCode(params OpCode[] opCodes)
        {
            foreach (var opCode in opCodes)
            {
                _writer.WriteByte((byte)opCode);
            }
            return this;
        }

        /// <summary>
        /// Appends an OpCode with argument to the script
        /// </summary>
        /// <param name="opCode">The OpCode to append</param>
        /// <param name="argument">The argument of the OpCode</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder OpCode(OpCode opCode, byte[] argument)
        {
            _writer.WriteByte((byte)opCode);
            if (argument?.Length > 0)
                _writer.WriteBytes(argument);
            return this;
        }

        /// <summary>
        /// Appends a contract call to the script
        /// </summary>
        /// <param name="scriptHash">The script hash of the contract to call</param>
        /// <param name="method">The method to call</param>
        /// <param name="parameters">The parameters for the call</param>
        /// <param name="callFlags">The call flags to use</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder ContractCall(Hash160 scriptHash, string method, ContractParameter?[] parameters, CallFlags callFlags = CallFlags.All)
        {
            if (scriptHash == null)
                throw new ArgumentNullException(nameof(scriptHash));
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Method name cannot be null or empty", nameof(method));

            // Push parameters
            if (parameters?.Length > 0)
                PushParameters(parameters);
            else
                OpCode(Script.OpCode.NewArray0);

            // Push call flags, method, and script hash
            PushInteger((int)callFlags);
            PushData(method);
            PushData(scriptHash.ToLittleEndianArray());
            
            // System call
            return SysCall("System.Contract.Call");
        }

        /// <summary>
        /// Appends a system call to the script
        /// </summary>
        /// <param name="operation">The interop service to call</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder SysCall(string operation)
        {
            OpCode(Script.OpCode.SysCall);
            var methodBytes = System.Text.Encoding.UTF8.GetBytes(operation);
            var hash = Hash.SHA256(methodBytes).Take(4).ToArray();
            _writer.WriteBytes(hash);
            return this;
        }

        /// <summary>
        /// Adds contract parameters to the script
        /// </summary>
        /// <param name="parameters">The parameters to add</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushParameters(ContractParameter?[] parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            foreach (var param in parameters)
                PushParameter(param);
            
            PushInteger(parameters.Length);
            return OpCode(Script.OpCode.Pack);
        }

        /// <summary>
        /// Adds a single contract parameter to the script
        /// </summary>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushParameter(ContractParameter? parameter)
        {
            if (parameter?.Value == null)
                return OpCode(Script.OpCode.PushNull);

            return parameter.Type switch
            {
                ContractParameterType.ByteArray => PushData((byte[])parameter.Value),
                ContractParameterType.Signature => PushData((byte[])parameter.Value),
                ContractParameterType.PublicKey => PushData((byte[])parameter.Value),
                ContractParameterType.Boolean => PushBoolean((bool)parameter.Value),
                ContractParameterType.Integer when parameter.Value is BigInteger bi => PushInteger(bi),
                ContractParameterType.Integer => PushInteger(Convert.ToInt32(parameter.Value)),
                ContractParameterType.Hash160 => PushData(((Hash160)parameter.Value).ToLittleEndianArray()),
                ContractParameterType.Hash256 => PushData(((Hash256)parameter.Value).ToLittleEndianArray()),
                ContractParameterType.String => PushData((string)parameter.Value),
                ContractParameterType.Array => PushArray((ContractParameter[])parameter.Value),
                ContractParameterType.Map => PushMap((Dictionary<ContractParameter, ContractParameter>)parameter.Value),
                ContractParameterType.Any => this,
                _ => throw new ArgumentException($"Parameter type '{parameter.Type}' not supported")
            };
        }

        /// <summary>
        /// Adds a push operation with the given BigInteger to the script
        /// </summary>
        /// <param name="value">The BigInteger to push</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushInteger(BigInteger value)
        {
            if (value >= -1 && value <= 16)
            {
                var opCode = (OpCode)((int)Script.OpCode.Push0 + (int)value);
                return OpCode(opCode);
            }

            var bytes = value.ToByteArray(isUnsigned: false, isBigEndian: false);
            
            return bytes.Length switch
            {
                1 => OpCode(Script.OpCode.PushInt8, bytes),
                2 => OpCode(Script.OpCode.PushInt16, bytes),
                <= 4 => OpCode(Script.OpCode.PushInt32, PadNumber(value, 4)),
                <= 8 => OpCode(Script.OpCode.PushInt64, PadNumber(value, 8)),
                <= 16 => OpCode(Script.OpCode.PushInt128, PadNumber(value, 16)),
                <= 32 => OpCode(Script.OpCode.PushInt256, PadNumber(value, 32)),
                _ => throw new ArgumentException($"The number {value} is out of range")
            };
        }

        /// <summary>
        /// Emits a push operation with the given value
        /// </summary>
        /// <param name="value">The value to push</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder EmitPush(object value)
        {
            if (value is byte[] bytes)
                return PushData(bytes);
            else if (value is int intValue)
                return PushInteger(intValue);
            else if (value is BigInteger bigInt)
                return PushInteger(bigInt);
            else if (value is string str)
                return PushData(str);
            else if (value is bool boolValue)
                return PushBoolean(boolValue);
            else
                throw new ArgumentException($"Unsupported push value type: {value?.GetType()}");
        }

        /// <summary>
        /// Emits a system call operation
        /// </summary>
        /// <param name="method">The method name to call</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder EmitSysCall(string method)
        {
            OpCode(Script.OpCode.SysCall);
            var methodBytes = System.Text.Encoding.UTF8.GetBytes(method);
            var hash = Hash.SHA256(methodBytes).Take(4).ToArray();
            _writer.WriteBytes(hash);
            return this;
        }

        /// <summary>
        /// Adds a push operation with the given integer to the script
        /// </summary>
        /// <param name="value">The integer to push</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushInteger(int value) => PushInteger(new BigInteger(value));

        /// <summary>
        /// Pads a number to the specified byte length
        /// </summary>
        /// <param name="value">The value to pad</param>
        /// <param name="length">The target length</param>
        /// <returns>The padded bytes</returns>
        private static byte[] PadNumber(BigInteger value, int length)
        {
            var bytes = value.ToByteArray(isUnsigned: false, isBigEndian: false);
            
            if (bytes.Length == length)
                return bytes;
            
            var result = new byte[length];
            Array.Copy(bytes, result, Math.Min(bytes.Length, length));
            
            // Pad with 0xFF for negative numbers, 0x00 for positive
            var padByte = value.Sign < 0 ? (byte)0xFF : (byte)0x00;
            for (int i = bytes.Length; i < length; i++)
            {
                result[i] = padByte;
            }
            
            return result;
        }

        /// <summary>
        /// Adds a push operation with the given boolean to the script
        /// </summary>
        /// <param name="value">The boolean to push</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushBoolean(bool value) => 
            OpCode(value ? Script.OpCode.Push1 : Script.OpCode.Push0);

        /// <summary>
        /// Adds data to the script with the correct length prefix
        /// </summary>
        /// <param name="data">The string data to add</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushData(string data) => 
            PushData(data.ToByteArray());

        /// <summary>
        /// Adds data to the script with the correct length prefix
        /// </summary>
        /// <param name="data">The byte data to add</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushData(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return OpCode(Script.OpCode.Push0);
            }

            if (data.Length < 256)
            {
                OpCode(Script.OpCode.PushData1);
                _writer.WriteByte((byte)data.Length);
            }
            else if (data.Length < 65536)
            {
                OpCode(Script.OpCode.PushData2);
                _writer.WriteUInt16((ushort)data.Length);
            }
            else
            {
                OpCode(Script.OpCode.PushData4);
                _writer.WriteUInt32((uint)data.Length);
            }
            
            _writer.WriteBytes(data);
            return this;
        }

        /// <summary>
        /// Pushes an array parameter to the script
        /// </summary>
        /// <param name="parameters">The array parameters</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushArray(ContractParameter[] parameters)
        {
            if (parameters?.Length == 0)
                return OpCode(Script.OpCode.NewArray0);
                
            return PushParameters(parameters);
        }

        /// <summary>
        /// Pushes a map parameter to the script
        /// </summary>
        /// <param name="map">The map to push</param>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder PushMap(Dictionary<ContractParameter, ContractParameter> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            foreach (var kvp in map)
            {
                PushParameter(kvp.Value);
                PushParameter(kvp.Key);
            }
            
            PushInteger(map.Count);
            return OpCode(Script.OpCode.PackMap);
        }

        /// <summary>
        /// Adds a pack operation to the script
        /// </summary>
        /// <returns>This ScriptBuilder instance</returns>
        public ScriptBuilder Pack() => OpCode(Script.OpCode.Pack);

        /// <summary>
        /// Gets the script as a byte array
        /// </summary>
        /// <returns>The script bytes</returns>
        public byte[] ToArray()
        {
            _writer.Flush();
            return _writer.ToArray();
        }

        /// <summary>
        /// Builds a verification script for a single public key
        /// </summary>
        /// <param name="publicKey">The public key in compressed format</param>
        /// <returns>The verification script</returns>
        public static byte[] BuildVerificationScript(byte[] publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            if (publicKey.Length != NeoConstants.PublicKeySizeCompressed)
                throw new ArgumentException($"Public key must be {NeoConstants.PublicKeySizeCompressed} bytes", nameof(publicKey));

            return new ScriptBuilder()
                .PushData(publicKey)
                .SysCall("System.Crypto.CheckSig")
                .ToArray();
        }

        /// <summary>
        /// Builds a verification script for multi-signature
        /// </summary>
        /// <param name="publicKeys">The public keys</param>
        /// <param name="threshold">The signing threshold</param>
        /// <returns>The verification script</returns>
        public static byte[] BuildVerificationScript(IEnumerable<ECPublicKey> publicKeys, int threshold)
        {
            var keys = publicKeys?.ToArray() ?? throw new ArgumentNullException(nameof(publicKeys));
            if (keys.Length == 0)
                throw new ArgumentException("At least one public key is required", nameof(publicKeys));
            if (threshold < 1 || threshold > keys.Length)
                throw new ArgumentException("Invalid threshold", nameof(threshold));

            var builder = new ScriptBuilder().PushInteger(threshold);
            
            foreach (var key in keys.OrderBy(k => k.GetEncoded(true), ByteArrayComparer.Default))
            {
                builder.PushData(key.GetEncoded(true));
            }
            
            return builder.PushInteger(keys.Length)
                .SysCall("System.Crypto.CheckMultisig")
                .ToArray();
        }

        /// <summary>
        /// Builds a script for calculating contract hash
        /// </summary>
        /// <param name="sender">The deploying account</param>
        /// <param name="nefChecksum">The NEF checksum</param>
        /// <param name="contractName">The contract name</param>
        /// <returns>The contract hash script</returns>
        public static byte[] BuildContractHashScript(Hash160 sender, int nefChecksum, string contractName)
        {
            return new ScriptBuilder()
                .OpCode(Script.OpCode.Abort)
                .PushData(sender.ToLittleEndianArray())
                .PushInteger(nefChecksum)
                .PushData(contractName)
                .ToArray();
        }

        /// <summary>
        /// Builds a script that calls a contract and unwraps an iterator
        /// </summary>
        /// <param name="contractHash">The contract script hash</param>
        /// <param name="method">The method to call</param>
        /// <param name="parameters">The method parameters</param>
        /// <param name="maxItems">The maximum number of items to retrieve</param>
        /// <param name="callFlags">The call flags</param>
        /// <returns>The unwrap iterator script</returns>
        public static byte[] BuildContractCallAndUnwrapIterator(
            Hash160 contractHash, 
            string method, 
            ContractParameter[] parameters,
            int maxItems, 
            CallFlags callFlags = CallFlags.All)
        {
            var builder = new ScriptBuilder()
                .PushInteger(maxItems);
                
            builder.ContractCall(contractHash, method, parameters, callFlags)
                .OpCode(Script.OpCode.NewArray0);

            var iteratorStartOffset = builder.Length;
            
            builder.OpCode(Script.OpCode.Over)
                .SysCall("System.Iterator.Next");

            var jmpIfNotOffset = builder.Length;
            builder.OpCode(Script.OpCode.JmpIfNot, new byte[] { 0x00 });

            builder.OpCode(Script.OpCode.Dup, Script.OpCode.Push2, Script.OpCode.Pick)
                .SysCall("System.Iterator.Value")
                .OpCode(Script.OpCode.Append, Script.OpCode.Dup, Script.OpCode.Size, 
                       Script.OpCode.Push3, Script.OpCode.Pick, Script.OpCode.Ge);

            var jmpIfMaxOffset = builder.Length;
            builder.OpCode(Script.OpCode.JmpIf, new byte[] { 0x00 });

            var jmpOffset = builder.Length;
            var jumpBytes = (sbyte)(iteratorStartOffset - jmpOffset);
            builder.OpCode(Script.OpCode.Jmp, new[] { (byte)jumpBytes });

            var loadResultOffset = builder.Length;
            builder.OpCode(Script.OpCode.Nip, Script.OpCode.Nip);

            var script = builder.ToArray();
            
            // Fix jump offsets
            script[jmpIfNotOffset + 1] = (byte)(loadResultOffset - jmpIfNotOffset);
            script[jmpIfMaxOffset + 1] = (byte)(loadResultOffset - jmpIfMaxOffset);

            return script;
        }

        /// <summary>
        /// Releases all resources used by the ScriptBuilder
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _writer?.Dispose();
                _disposed = true;
            }
        }
    }
}