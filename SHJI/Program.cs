namespace SHJI;

using Jane.Core;
using VM;
using Compiler;
using Spectre.Console;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 0)
        {
            try
            {
                string input = File.ReadAllText(args[0]);
                Tokenizer t = new(input);
                Parser p = new(t);
                ASTRoot r = p.ParseProgram();
                if (p.Errors.Length > 0)
                {
                    foreach (var pe in p.Errors)
                        AnsiConsole.MarkupLineInterpolated($"[red]{pe}[/]");
                    return;
                }
                JnCompiler c = new();
                c.Compile(r);
                if (c.Errors.Length > 0)
                {
                    foreach (var ce in c.Errors)
                        AnsiConsole.MarkupLineInterpolated($"[red]{ce}[/]");
                    return;
                }
                JnVM vm = new(c.Constants, c.CurrentInstructions);
                vm.Run();
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
        }
        else
        {
#if DEBUG
            new REPL(REPL.REPLLogLevel.INFO).Run();
#else
        new REPL().Run();
#endif
        }
    }
}