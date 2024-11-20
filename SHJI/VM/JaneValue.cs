using System.Numerics;

namespace SHJI.VM
{
    public interface JaneValue
    {
        public object? Value { get; }
        public virtual string Inspect() => Value?.ToString() ?? "abyss";
        public abstract string Type { get; }


        public static readonly JaneValue Abyss = new JaneAbyss();
        public static readonly JaneValue True = new JaneBool(true);
        public static readonly JaneValue False = new JaneBool(false);
    }

    public interface JaneValue<T> : JaneValue
    {
        new public T Value { get; }
        object? JaneValue.Value { get => Value; }
    }

    public readonly struct JaneInt(BigInteger value) : JaneValue<BigInteger>
    {
        public BigInteger Value { get; } = value;
        public string Type { get; } = "int";
    }

    public readonly struct JaneAbyss() : JaneValue<object?>
    {
        public object? Value { get; } = null;
        public string Type { get; } = "abyss";
    }

    public readonly struct JaneBool(bool b) : JaneValue<bool>
    {
        public bool Value { get; } = b;
        public string Type { get; } = "bool";

        public string Inspect() => Value ? "true" : "false";
    }

    public readonly struct JaneString(string s) : JaneValue<string>
    {
        public string Value { get; } = s;
        public string Type { get; } = "str";

        public string Inspect() => $"\"{Value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }

    public readonly struct JaneFloat(double f) : JaneValue<double>
    {
        public double Value { get; } = f;
        public string Type { get; } = "float";
        public string Inspect() => $"{Value}";
    }

    public readonly struct JaneChar(char c) : JaneValue<char>
    {
        public char Value { get; } = c;
        public string Type { get; } = "char";
        public string Inspect() => $"'{Value}'";
    }

    public readonly struct JaneArray(JaneValue[] arr) : JaneValue<JaneValue[]>
    {
        public JaneValue[] Value { get; } = arr;
        public string Type { get; } = "obj[]";
        public string Inspect() => $"[{string.Join(" ", Value.Select(x => x.Inspect()))}]";
    }

    public readonly struct JaneFunction(byte[] instructions, int numLocals, int numParams, JaneValue[] captures) : JaneValue<byte[]>
    {
        public byte[] Value { get; } = instructions;
        // Stack space to allocate
        public int NumLocals { get; } = numLocals;
        public int NumParams { get; } = numParams;
        public JaneValue[] Captures { get; } = captures;
        public string Type { get; } = "Fn<tup -> obj>";
        public string Inspect() =>
            $"<Function 0x{GetHashCode():X}>";
    }

    public readonly struct JaneBuiltin(Lib.Lib.Builtin fn) : JaneValue<Lib.Lib.Builtin>
    {
        public Lib.Lib.Builtin Value { get; } = fn;
        public string Type { get; } = "Fn<tup -> obj>";
        public string Inspect() => $"<Builtin Function 0x{Value.Method.GetHashCode():X}>";
    }
}
