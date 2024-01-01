using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.Interpreter.Builtins
{
    internal static class Builtins
    {
        public static Dictionary<string, JaneBuiltinFunction> Standard = new() {
            { "WriteLn", Tty.WriteLn },
            { "Len", Len }
        };

        public static IJaneObject Len(params IJaneObject[] args)
        {
            if (args.Length != 1) throw new ArgumentException("Incorrect number of arguments for \"Len\" function");
            return new JaneInt { Value = args[0] is JaneString s ? s.Value.Length : args[0] is JaneArray a ? a.Value.Length : throw new RuntimeError($"Builtin \"Len\" not usable with type {args[0].Type()}") };
        }
    }
}
