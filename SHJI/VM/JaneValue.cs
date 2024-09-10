using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

    public struct JaneInt(BigInteger value) : JaneValue<BigInteger>
    {
        public BigInteger Value { get; } = value;
        public string Type { get; } = "int";
    }

    public struct JaneAbyss() : JaneValue<object?>
    {
        public object? Value { get; } = null;
        public string Type { get; } = "abyss";
    }

    public struct JaneBool(bool b) : JaneValue<bool>
    {
        public bool Value { get; } = b;
        public string Type { get; } = "bool";

        public string Inspect() => Value ? "true" : "false";
    }

    public struct JaneString(string s) : JaneValue<string>
    {
        public string Value { get; } = s;
        public string Type { get; } = "str";

        public string Inspect() => $"\"{Value}\"";
    }
}
