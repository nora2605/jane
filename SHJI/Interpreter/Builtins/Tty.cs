using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.Interpreter.Builtins
{
    internal static class Tty
    {
        public static IJaneObject WriteLn(params IJaneObject[] args)
        {
            if (args.Length == 0) return IJaneObject.JANE_ABYSS;
            foreach (IJaneObject arg in args) 
                Console.WriteLine(arg.Inspect());
            return IJaneObject.JANE_ABYSS;
        }
    }
}
