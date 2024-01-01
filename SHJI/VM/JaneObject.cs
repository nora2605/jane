using Jane.AST;

namespace SHJI.VM
{
    internal interface IJaneObject
    {
        public readonly static IJaneObject JANE_ABYSS = new JaneAbyss();
        public readonly static IJaneObject JANE_TRUE = new JaneBool() { Value = true };
        public readonly static IJaneObject JANE_FALSE = new JaneBool() { Value = false };
        public readonly static IJaneObject JANE_UNINITIALIZED = new JaneAbyss(false);

        public ObjectType Type();
        public string Inspect();

        public string? ToString() => Inspect();
    }

    internal delegate IJaneObject JaneBuiltinFunction(params IJaneObject[] args);

    internal struct JaneBuiltin : IJaneObject
    {
        public JaneBuiltinFunction Fn;
        public readonly ObjectType Type() => ObjectType.Builtin;
        public readonly string Inspect() => $"{Fn}";

        public static implicit operator JaneBuiltin(JaneBuiltinFunction f) => new() { Fn = f };
    }

    internal struct JaneArray : IJaneObject
    {
        public IJaneObject[] Value;
        public readonly ObjectType Type() => ObjectType.Array;
        public readonly string Inspect() => $"[{Value.Select(x => x.Inspect()).Aggregate((a, b) => $"{a} {b}")}]";
    }

    internal struct JaneFunction : IJaneObject
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
    internal struct JaneInt : IJaneObject
    {
        public int Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int32;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneShort : IJaneObject
    {
        public short Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int16;
        public readonly string Inspect() => Value.ToString();
    }
    internal struct JaneLong : IJaneObject
    {
        public long Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int64;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneInt128 : IJaneObject
    {
        public Int128 Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Int128;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneByte : IJaneObject
    {
        public byte Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt8;
        public readonly string Inspect() => Value.ToString();
    }
    internal struct JaneSByte : IJaneObject
    {
        public sbyte Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.Int8;
        public readonly string Inspect() => Value.ToString();
    }
    internal struct JaneUInt : IJaneObject
    {
        public uint Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt32;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneUShort : IJaneObject
    {
        public ushort Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt16;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneULong : IJaneObject
    {
        public ulong Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.UInt64;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneUInt128 : IJaneObject
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
    internal struct JaneFloat : IJaneObject
    {
        public float Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Float32;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneDouble : IJaneObject
    {
        public double Value { get; set; }
        public readonly ObjectType Type() => ObjectType.Float64;
        public readonly string Inspect() => Value.ToString();
    }
    #endregion
    #region Other Primitives
    internal struct JaneString : IJaneObject
    {
        public string Value { get; set; }
        public readonly ObjectType Type() => ObjectType.String;
        public readonly string Inspect() => Value;
    }

    internal struct JaneBool : IJaneObject
    {
        public bool Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.Bool;
        public readonly string Inspect() => Value.ToString().ToLower();
    }

    internal struct JaneChar : IJaneObject
    {
        public char Value
        {
            get; set;
        }
        public readonly ObjectType Type() => ObjectType.Char;
        public readonly string Inspect() => Value.ToString();
    }

    internal struct JaneAbyss : IJaneObject
    {
        public bool init;
        public JaneAbyss(bool init = true)
        {
            this.init = init;
        }
        public readonly ObjectType Type() => ObjectType.Abyss;
        public readonly string Inspect() => "abyss";
    }
    #endregion

    internal enum ObjectType
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
        Builtin
    }
}
