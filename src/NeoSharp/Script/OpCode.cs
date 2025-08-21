using System;
using System.ComponentModel;
using NeoSharp.Utils;

namespace NeoSharp.Script
{
    /// <summary>
    /// Represents Neo VM operation codes
    /// </summary>
    public enum OpCode : byte
    {
        #region Constants
        
        [Description("Push a 1-byte signed integer")]
        PushInt8 = 0x00,
        
        [Description("Push a 2-byte signed integer")]
        PushInt16 = 0x01,
        
        [Description("Push a 4-byte signed integer")]
        PushInt32 = 0x02,
        
        [Description("Push an 8-byte signed integer")]
        PushInt64 = 0x03,
        
        [Description("Push a 16-byte signed integer")]
        PushInt128 = 0x04,
        
        [Description("Push a 32-byte signed integer")]
        PushInt256 = 0x05,
        
        [Description("Push absolute address")]
        PushA = 0x0A,
        
        [Description("Push null")]
        PushNull = 0x0B,
        
        [Description("Push data with 1-byte length prefix")]
        PushData1 = 0x0C,
        
        [Description("Push data with 2-byte length prefix")]
        PushData2 = 0x0D,
        
        [Description("Push data with 4-byte length prefix")]
        PushData4 = 0x0E,
        
        [Description("Push -1")]
        PushM1 = 0x0F,
        
        [Description("Push 0")]
        Push0 = 0x10,
        
        [Description("Push 1")]
        Push1 = 0x11,
        
        [Description("Push 2")]
        Push2 = 0x12,
        
        [Description("Push 3")]
        Push3 = 0x13,
        
        [Description("Push 4")]
        Push4 = 0x14,
        
        [Description("Push 5")]
        Push5 = 0x15,
        
        [Description("Push 6")]
        Push6 = 0x16,
        
        [Description("Push 7")]
        Push7 = 0x17,
        
        [Description("Push 8")]
        Push8 = 0x18,
        
        [Description("Push 9")]
        Push9 = 0x19,
        
        [Description("Push 10")]
        Push10 = 0x1A,
        
        [Description("Push 11")]
        Push11 = 0x1B,
        
        [Description("Push 12")]
        Push12 = 0x1C,
        
        [Description("Push 13")]
        Push13 = 0x1D,
        
        [Description("Push 14")]
        Push14 = 0x1E,
        
        [Description("Push 15")]
        Push15 = 0x1F,
        
        [Description("Push 16")]
        Push16 = 0x20,

        #endregion

        #region Flow Control
        
        [Description("No operation")]
        Nop = 0x21,
        
        [Description("Unconditional jump")]
        Jmp = 0x22,
        
        [Description("Unconditional jump (long form)")]
        JmpL = 0x23,
        
        [Description("Jump if true")]
        JmpIf = 0x24,
        
        [Description("Jump if true (long form)")]
        JmpIfL = 0x25,
        
        [Description("Jump if false")]
        JmpIfNot = 0x26,
        
        [Description("Jump if false (long form)")]
        JmpIfNotL = 0x27,
        
        [Description("Jump if equal")]
        JmpEq = 0x28,
        
        [Description("Jump if equal (long form)")]
        JmpEqL = 0x29,
        
        [Description("Jump if not equal")]
        JmpNe = 0x2A,
        
        [Description("Jump if not equal (long form)")]
        JmpNeL = 0x2B,
        
        [Description("Jump if greater than")]
        JmpGt = 0x2C,
        
        [Description("Jump if greater than (long form)")]
        JmpGtL = 0x2D,
        
        [Description("Jump if greater than or equal")]
        JmpGe = 0x2E,
        
        [Description("Jump if greater than or equal (long form)")]
        JmpGeL = 0x2F,
        
        [Description("Jump if less than")]
        JmpLt = 0x30,
        
        [Description("Jump if less than (long form)")]
        JmpLtL = 0x31,
        
        [Description("Jump if less than or equal")]
        JmpLe = 0x32,
        
        [Description("Jump if less than or equal (long form)")]
        JmpLeL = 0x33,
        
        [Description("Call function")]
        Call = 0x34,
        
        [Description("Call function (long form)")]
        CallL = 0x35,
        
        [Description("Call function with absolute address")]
        CallA = 0x36,
        
        [Description("Call function with token")]
        CallT = 0x37,
        
        [Description("Abort execution")]
        Abort = 0x38,
        
        [Description("Assert condition")]
        Assert = 0x39,
        
        [Description("Throw exception")]
        Throw = 0x3A,
        
        [Description("Try block")]
        Try = 0x3B,
        
        [Description("Try block (long form)")]
        TryL = 0x3C,
        
        [Description("End try block")]
        EndTry = 0x3D,
        
        [Description("End try block (long form)")]
        EndTryL = 0x3E,
        
        [Description("End finally block")]
        EndFinally = 0x3F,
        
        [Description("Return from function")]
        Ret = 0x40,
        
        [Description("System call")]
        SysCall = 0x41,

        #endregion

        #region Stack
        
        [Description("Get stack depth")]
        Depth = 0x43,
        
        [Description("Drop top stack item")]
        Drop = 0x45,
        
        [Description("Remove second stack item")]
        Nip = 0x46,
        
        [Description("Drop n-th stack item")]
        XDrop = 0x48,
        
        [Description("Clear the stack")]
        Clear = 0x49,
        
        [Description("Duplicate top stack item")]
        Dup = 0x4A,
        
        [Description("Copy second stack item to top")]
        Over = 0x4B,
        
        [Description("Copy n-th stack item to top")]
        Pick = 0x4D,
        
        [Description("Copy top item and insert at third position")]
        Tuck = 0x4E,
        
        [Description("Swap top two stack items")]
        Swap = 0x50,
        
        [Description("Rotate top three stack items")]
        Rot = 0x51,
        
        [Description("Move n-th item to top of stack")]
        Roll = 0x52,
        
        [Description("Reverse top three stack items")]
        Reverse3 = 0x53,
        
        [Description("Reverse top four stack items")]
        Reverse4 = 0x54,
        
        [Description("Reverse top n stack items")]
        ReverseN = 0x55,

        #endregion

        #region Slot
        
        [Description("Initialize static field slots")]
        InitSSlot = 0x56,
        
        [Description("Initialize local and argument slots")]
        InitSlot = 0x57,
        
        [Description("Load static field 0")]
        LdSFld0 = 0x58,
        
        [Description("Load static field 1")]
        LdSFld1 = 0x59,
        
        [Description("Load static field 2")]
        LdSFld2 = 0x5A,
        
        [Description("Load static field 3")]
        LdSFld3 = 0x5B,
        
        [Description("Load static field 4")]
        LdSFld4 = 0x5C,
        
        [Description("Load static field 5")]
        LdSFld5 = 0x5D,
        
        [Description("Load static field 6")]
        LdSFld6 = 0x5E,
        
        [Description("Load static field")]
        LdSFld = 0x5F,
        
        [Description("Store static field 0")]
        StSFld0 = 0x60,
        
        [Description("Store static field 1")]
        StSFld1 = 0x61,
        
        [Description("Store static field 2")]
        StSFld2 = 0x62,
        
        [Description("Store static field 3")]
        StSFld3 = 0x63,
        
        [Description("Store static field 4")]
        StSFld4 = 0x64,
        
        [Description("Store static field 5")]
        StSFld5 = 0x65,
        
        [Description("Store static field 6")]
        StSFld6 = 0x66,
        
        [Description("Store static field")]
        StSFld = 0x67,
        
        [Description("Load local variable 0")]
        LdLoc0 = 0x68,
        
        [Description("Load local variable 1")]
        LdLoc1 = 0x69,
        
        [Description("Load local variable 2")]
        LdLoc2 = 0x6A,
        
        [Description("Load local variable 3")]
        LdLoc3 = 0x6B,
        
        [Description("Load local variable 4")]
        LdLoc4 = 0x6C,
        
        [Description("Load local variable 5")]
        LdLoc5 = 0x6D,
        
        [Description("Load local variable 6")]
        LdLoc6 = 0x6E,
        
        [Description("Load local variable")]
        LdLoc = 0x6F,
        
        [Description("Store local variable 0")]
        StLoc0 = 0x70,
        
        [Description("Store local variable 1")]
        StLoc1 = 0x71,
        
        [Description("Store local variable 2")]
        StLoc2 = 0x72,
        
        [Description("Store local variable 3")]
        StLoc3 = 0x73,
        
        [Description("Store local variable 4")]
        StLoc4 = 0x74,
        
        [Description("Store local variable 5")]
        StLoc5 = 0x75,
        
        [Description("Store local variable 6")]
        StLoc6 = 0x76,
        
        [Description("Store local variable")]
        StLoc = 0x77,
        
        [Description("Load argument 0")]
        LdArg0 = 0x78,
        
        [Description("Load argument 1")]
        LdArg1 = 0x79,
        
        [Description("Load argument 2")]
        LdArg2 = 0x7A,
        
        [Description("Load argument 3")]
        LdArg3 = 0x7B,
        
        [Description("Load argument 4")]
        LdArg4 = 0x7C,
        
        [Description("Load argument 5")]
        LdArg5 = 0x7D,
        
        [Description("Load argument 6")]
        LdArg6 = 0x7E,
        
        [Description("Load argument")]
        LdArg = 0x7F,
        
        [Description("Store argument 0")]
        StArg0 = 0x80,
        
        [Description("Store argument 1")]
        StArg1 = 0x81,
        
        [Description("Store argument 2")]
        StArg2 = 0x82,
        
        [Description("Store argument 3")]
        StArg3 = 0x83,
        
        [Description("Store argument 4")]
        StArg4 = 0x84,
        
        [Description("Store argument 5")]
        StArg5 = 0x85,
        
        [Description("Store argument 6")]
        StArg6 = 0x86,
        
        [Description("Store argument")]
        StArg = 0x87,

        #endregion

        #region Splice
        
        [Description("Create new buffer")]
        NewBuffer = 0x88,
        
        [Description("Memory copy")]
        MemCpy = 0x89,
        
        [Description("Concatenate")]
        Cat = 0x8B,
        
        [Description("Substring")]
        SubStr = 0x8C,
        
        [Description("Left substring")]
        Left = 0x8D,
        
        [Description("Right substring")]
        Right = 0x8E,

        #endregion

        #region Bitwise Logic
        
        [Description("Bitwise invert")]
        Invert = 0x90,
        
        [Description("Bitwise AND")]
        And = 0x91,
        
        [Description("Bitwise OR")]
        Or = 0x92,
        
        [Description("Bitwise XOR")]
        Xor = 0x93,
        
        [Description("Equal")]
        Equal = 0x97,
        
        [Description("Not equal")]
        NotEqual = 0x98,

        #endregion

        #region Arithmetic
        
        [Description("Sign of number")]
        Sign = 0x99,
        
        [Description("Absolute value")]
        Abs = 0x9A,
        
        [Description("Negate number")]
        Negate = 0x9B,
        
        [Description("Increment")]
        Inc = 0x9C,
        
        [Description("Decrement")]
        Dec = 0x9D,
        
        [Description("Addition")]
        Add = 0x9E,
        
        [Description("Subtraction")]
        Sub = 0x9F,
        
        [Description("Multiplication")]
        Mul = 0xA0,
        
        [Description("Division")]
        Div = 0xA1,
        
        [Description("Modulo")]
        Mod = 0xA2,
        
        [Description("Power")]
        Pow = 0xA3,
        
        [Description("Square root")]
        Sqrt = 0xA4,
        
        [Description("Modular multiplication")]
        ModMul = 0xA5,
        
        [Description("Modular power")]
        ModPow = 0xA6,
        
        [Description("Shift left")]
        ShL = 0xA8,
        
        [Description("Shift right")]
        ShR = 0xA9,
        
        [Description("Boolean NOT")]
        Not = 0xAA,
        
        [Description("Boolean AND")]
        BoolAnd = 0xAB,
        
        [Description("Boolean OR")]
        BoolOr = 0xAC,
        
        [Description("Not zero")]
        Nz = 0xB1,
        
        [Description("Numeric equal")]
        NumEqual = 0xB3,
        
        [Description("Numeric not equal")]
        NumNotEqual = 0xB4,
        
        [Description("Less than")]
        Lt = 0xB5,
        
        [Description("Less than or equal")]
        Le = 0xB6,
        
        [Description("Greater than")]
        Gt = 0xB7,
        
        [Description("Greater than or equal")]
        Ge = 0xB8,
        
        [Description("Minimum")]
        Min = 0xB9,
        
        [Description("Maximum")]
        Max = 0xBA,
        
        [Description("Within range")]
        Within = 0xBB,

        #endregion

        #region Compound-Type
        
        [Description("Pack map")]
        PackMap = 0xBE,
        
        [Description("Pack struct")]
        PackStruct = 0xBF,
        
        [Description("Pack array")]
        Pack = 0xC0,
        
        [Description("Unpack array")]
        Unpack = 0xC1,
        
        [Description("Create empty array")]
        NewArray0 = 0xC2,
        
        [Description("Create new array")]
        NewArray = 0xC3,
        
        [Description("Create new typed array")]
        NewArrayT = 0xC4,
        
        [Description("Create empty struct")]
        NewStruct0 = 0xC5,
        
        [Description("Create new struct")]
        NewStruct = 0xC6,
        
        [Description("Create new map")]
        NewMap = 0xC8,
        
        [Description("Get size")]
        Size = 0xCA,
        
        [Description("Has key")]
        HasKey = 0xCB,
        
        [Description("Get keys")]
        Keys = 0xCC,
        
        [Description("Get values")]
        Values = 0xCD,
        
        [Description("Pick item")]
        PickItem = 0xCE,
        
        [Description("Append")]
        Append = 0xCF,
        
        [Description("Set item")]
        SetItem = 0xD0,
        
        [Description("Reverse items")]
        ReverseItems = 0xD1,
        
        [Description("Remove")]
        Remove = 0xD2,
        
        [Description("Clear items")]
        ClearItems = 0xD3,

        #endregion

        #region Types
        
        [Description("Is null")]
        IsNull = 0xD8,
        
        [Description("Is type")]
        IsType = 0xD9,
        
        [Description("Convert")]
        Convert = 0xDB

        #endregion
    }

    /// <summary>
    /// Extension methods for OpCode
    /// </summary>
    public static class OpCodeExtensions
    {
        /// <summary>
        /// Gets the byte value of the opcode
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The byte value</returns>
        public static byte ToByte(this OpCode opCode) => (byte)opCode;

        /// <summary>
        /// Gets the hex string representation of the opcode
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The hex string</returns>
        public static string ToHexString(this OpCode opCode) => 
            new[] { (byte)opCode }.ToHexString();

        /// <summary>
        /// Gets the execution price of the opcode in GAS
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The execution price</returns>
        public static long GetPrice(this OpCode opCode)
        {
            return opCode switch
            {
                OpCode.PushInt8 or OpCode.PushInt16 or OpCode.PushInt32 or OpCode.PushInt64 or
                OpCode.PushNull or OpCode.PushM1 or OpCode.Push0 or OpCode.Push1 or OpCode.Push2 or
                OpCode.Push3 or OpCode.Push4 or OpCode.Push5 or OpCode.Push6 or OpCode.Push7 or
                OpCode.Push8 or OpCode.Push9 or OpCode.Push10 or OpCode.Push11 or OpCode.Push12 or
                OpCode.Push13 or OpCode.Push14 or OpCode.Push15 or OpCode.Push16 or OpCode.Nop or
                OpCode.Assert => 1,

                OpCode.PushInt128 or OpCode.PushInt256 or OpCode.PushA or OpCode.Try or
                OpCode.EndTry or OpCode.EndTryL or OpCode.EndFinally or OpCode.Invert or
                OpCode.Sign or OpCode.Abs or OpCode.Negate or OpCode.Inc or OpCode.Dec or
                OpCode.Not or OpCode.Nz or OpCode.Size => 1 << 2,

                OpCode.PushData1 or OpCode.And or OpCode.Or or OpCode.Xor or OpCode.Add or
                OpCode.Sub or OpCode.Mul or OpCode.Div or OpCode.Mod or OpCode.ShL or OpCode.ShR or
                OpCode.BoolAnd or OpCode.BoolOr or OpCode.NumEqual or OpCode.NumNotEqual or
                OpCode.Lt or OpCode.Le or OpCode.Gt or OpCode.Ge or OpCode.Min or OpCode.Max or
                OpCode.Within or OpCode.NewMap => 1 << 3,

                OpCode.XDrop or OpCode.Clear or OpCode.Roll or OpCode.ReverseN or OpCode.InitSSlot or
                OpCode.NewArray0 or OpCode.NewStruct0 or OpCode.Keys or OpCode.Remove or
                OpCode.ClearItems => 1 << 4,

                OpCode.Equal or OpCode.NotEqual or OpCode.ModMul => 1 << 5,

                OpCode.InitSlot or OpCode.Pow or OpCode.HasKey or OpCode.PickItem => 1 << 6,

                OpCode.NewBuffer => 1 << 8,

                OpCode.PushData2 or OpCode.Call or OpCode.CallL or OpCode.CallA or OpCode.Throw or
                OpCode.NewArray or OpCode.NewArrayT or OpCode.NewStruct => 1 << 9,

                OpCode.MemCpy or OpCode.Cat or OpCode.SubStr or OpCode.Left or OpCode.Right or
                OpCode.Sqrt or OpCode.ModPow or OpCode.PackMap or OpCode.PackStruct or OpCode.Pack or
                OpCode.Unpack => 1 << 11,

                OpCode.PushData4 => 1 << 12,

                OpCode.Values or OpCode.Append or OpCode.SetItem or OpCode.ReverseItems or
                OpCode.Convert => 1 << 13,

                OpCode.CallT => 1 << 15,

                OpCode.Abort or OpCode.Ret or OpCode.SysCall => 0,

                _ => 1 << 1
            };
        }

        /// <summary>
        /// Gets the operand size information for the opcode
        /// </summary>
        /// <param name="opCode">The opcode</param>
        /// <returns>The operand size, or null if no operand</returns>
        public static OperandSize? GetOperandSize(this OpCode opCode)
        {
            return opCode switch
            {
                OpCode.PushInt8 or OpCode.Jmp or OpCode.JmpIf or OpCode.JmpIfNot or OpCode.JmpEq or
                OpCode.JmpNe or OpCode.JmpGt or OpCode.JmpGe or OpCode.JmpLt or OpCode.JmpLe or
                OpCode.Call or OpCode.EndTry or OpCode.InitSSlot or OpCode.LdSFld or OpCode.StSFld or
                OpCode.LdLoc or OpCode.StLoc or OpCode.LdArg or OpCode.StArg or OpCode.NewArrayT or
                OpCode.IsType or OpCode.Convert => new OperandSize(1),

                OpCode.PushInt16 or OpCode.CallT or OpCode.Try or OpCode.InitSlot => new OperandSize(2),

                OpCode.PushInt32 or OpCode.PushA or OpCode.JmpL or OpCode.JmpIfL or OpCode.JmpIfNotL or
                OpCode.JmpEqL or OpCode.JmpNeL or OpCode.JmpGtL or OpCode.JmpGeL or OpCode.JmpLtL or
                OpCode.JmpLeL or OpCode.CallL or OpCode.EndTryL or OpCode.SysCall => new OperandSize(4),

                OpCode.PushInt64 or OpCode.TryL => new OperandSize(8),

                OpCode.PushInt128 => new OperandSize(16),

                OpCode.PushInt256 => new OperandSize(32),

                OpCode.PushData1 => new OperandSize(0, 1),

                OpCode.PushData2 => new OperandSize(0, 2),

                OpCode.PushData4 => new OperandSize(0, 4),

                _ => null
            };
        }
    }

    /// <summary>
    /// Represents operand size information for opcodes
    /// </summary>
    public sealed class OperandSize
    {
        /// <summary>
        /// The prefix size (for variable-length operands)
        /// </summary>
        public int PrefixSize { get; }

        /// <summary>
        /// The fixed size (for fixed-length operands)
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Initializes operand size with fixed size
        /// </summary>
        /// <param name="size">The fixed size</param>
        public OperandSize(int size)
        {
            Size = size;
            PrefixSize = 0;
        }

        /// <summary>
        /// Initializes operand size with prefix size
        /// </summary>
        /// <param name="size">The size</param>
        /// <param name="prefixSize">The prefix size</param>
        public OperandSize(int size, int prefixSize)
        {
            Size = size;
            PrefixSize = prefixSize;
        }
    }
}