using SHJI.VM;

namespace SHJI.Lib
{
    public static partial class Lib
    {
        public delegate JaneValue Builtin(params JaneValue[] args);
        public static Dictionary<string, Builtin> Builtins = new() {
            { "print", (params JaneValue[] args) => { Console.Write(string.Join("", args.Select(JnVM.ToStringRepresentation))); return JaneValue.Abyss; } },
            { "println", (params JaneValue[] args) => { Console.WriteLine(string.Join("", args.Select(JnVM.ToStringRepresentation))); return JaneValue.Abyss; } }
        };
    }
}
