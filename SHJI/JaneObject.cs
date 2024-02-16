using Jane.AST;

namespace SHJI
{
    public interface IJaneObject
    {
        public readonly static IJaneObject JANE_ABYSS = new JaneAbyss();
        public readonly static IJaneObject JANE_TRUE = new JaneBool() { Value = true };
        public readonly static IJaneObject JANE_FALSE = new JaneBool() { Value = false };
        public readonly static IJaneObject JANE_UNINITIALIZED = new JaneAbyss(false);

        public ObjectType Type();
        public string Inspect();

        public string? ToString() => Inspect();
    }

    public delegate IJaneObject JaneBuiltInFunction(params IJaneObject[] args);

    public struct JaneBuiltIn : IJaneObject
    {
        public JaneBuiltInFunction Fn;
        public readonly ObjectType Type() => ObjectType.BuiltIn;
        public readonly string Inspect() => $"{Fn}";

        public static implicit operator JaneBuiltIn(JaneBuiltInFunction f) => new() { Fn = f };
    }

    public struct JaneArray : IJaneObject
    {
        public IJaneObject[] Value;
        public readonly ObjectType Type() => ObjectType.Array;
        public readonly string Inspect() => $"[{Value.Select(x => x.Inspect()).Aggregate((a, b) => $"{a} {b}")}]";
    }

    public struct JaneFunction : IJaneObject
    {
        public string Name;
        public Identifier[] Parameters;
        public string[] Flags;
        public BlockStatement Body;
        public string ReturnType;
        public JaneEnvironment Environment;

        public readonly ObjectType Type() => ObjectType.Function;
        public readonly string Inspect() => $"fn {(Flags.Length == 0 ? "" : Flags.Select(x => x.Length == 1 ? $"-{x} " : $"--{x} ").Aggregate((a, b) => a + b))}{Name} ({Parameters.Select(x => x.ToString()).Aggregate((a, b) => $"{a}, {b}")}){(ReturnType is null ? "" : $" -> {ReturnType}")} {Body}";
    }

    #region Integer Types
    public struct JaneInt : IJaneObject
    {
        public int Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int32;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneShort : IJaneObject
    {
        public short Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int16;
        public readonly string Inspect() => Value.ToString();
    }
    public struct JaneLong : IJaneObject
    {
        public long Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int64;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneInt128 : IJaneObject
    {
        public Int128 Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int128;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneByte : IJaneObject
    {
        public byte Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt8;
        public readonly string Inspect() => Value.ToString();
    }
    public struct JaneSByte : IJaneObject
    {
        public sbyte Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.Int8;
        public readonly string Inspect() => Value.ToString();
    }
    public struct JaneUInt : IJaneObject
    {
        public uint Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt32;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneUShort : IJaneObject
    {
        public ushort Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt16;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneULong : IJaneObject
    {
        public ulong Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt64;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneUInt128 : IJaneObject
    {
        public UInt128 Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt128;
        public readonly string Inspect() => Value.ToString();
    }
    #endregion
    #region Floating Point Numbers
    public struct JaneFloat : IJaneObject
    {
        public float Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Float32;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneDouble : IJaneObject
    {
        public double Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Float64;
        public readonly string Inspect() => Value.ToString();
    }
    #endregion
    #region Other Primitives
    public struct JaneString : IJaneObject
    {
        public string Value { get; set; }
        public readonly ObjectType Type() => ObjectType.String;
        public readonly string Inspect() => Value;
    }

    public struct JaneBool : IJaneObject
    {
        public bool Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.Bool;
        public readonly string Inspect() => Value.ToString().ToLower();
    }

    public struct JaneChar : IJaneObject
    {
        public char Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.Char;
        public readonly string Inspect() => Value.ToString();
    }

    public struct JaneAbyss(bool init = true) : IJaneObject
    {
        public bool init = init;

        public readonly ObjectType Type() => ObjectType.Abyss;
        public readonly string Inspect() => "abyss";
    }
    #endregion

    public enum ObjectType
    {
        Abyss,
        Bool,
        Int8,
        Int16,
        Int32,
        Int64,
        Int128,
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        UInt128,
        Float32,
        Float64,
        Char,
        String,
        Function,
        Tuple,
        Array,
        Struct,
        Interface,
        Class,
        BuiltIn
    }
}
