namespace SHJI;

using Jane.Core;
using VM;
using Compiler;

class Program
{
    static void Main(string[] args)
    {
#if DEBUG
        new REPL(REPL.REPLLogLevel.DEBUG).Run();
#else
        new REPL().Run();
#endif
    }
}