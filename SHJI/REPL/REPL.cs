using Jane.Core;
using SHJI.Bytecode;
using SHJI.Compiler;
using SHJI.VM;
using Spectre.Console;

namespace SHJI
{
    internal class REPL(REPL.REPLLogLevel logLevel)
    {
        private bool ShouldEnd { get; set; }
        public REPLLogLevel LogLevel { get; set; } = logLevel;

        private JaneValue[] Constants = [];
        private SymbolTable SymbolTable = new();
        private JaneValue[] Globals = new JaneValue[2<<16];

        private bool cancelVm = false;

        public REPL() : this(REPLLogLevel.WARNING) { }

        public void Run()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            if (AnsiConsole.Profile.Capabilities.ColorSystem != ColorSystem.TrueColor)
            {
                AnsiConsole.WriteLine("Your Terminal does not support TrueColor :(");
            }

            AnsiConsole.Foreground = Color.Turquoise4;
            AnsiConsole.WriteLine(@$"SHJI v0.1");

            while (true)
            {
                REP();
                if (ShouldEnd) break;
            }
        }
        void REP()
        {
            AnsiConsole.Markup("[turquoise4]jn>[/] ");
            string? input = Console.ReadLine();
            if (input == null || input == "") return;
            if (input.StartsWith('.'))
            {
                string[] args = input[1..].Split(" ");
                REPLCommand(args[0], args.Length > 1 ? args[1..] : null);
                return;
            }

            Eval(input);
        }

        void Eval(string input)
        {
            Dictionary<string, TimeSpan> stageTimes = [];
            var startStage = DateTime.Now;
            var start = startStage;
            Tokenizer t = new(input);
            Token[] tokens = [.. t];
            while (!t.Finished || input[^1] == '\\')
            {
                AnsiConsole.Markup("[turquoise4]..>[/] ");
                string? addit = Console.ReadLine();
                if (addit == null) return;
                input = input.TrimEnd('\\');
                input += "\n" + addit;
                t = new(input);
                tokens = [.. t];
            }
            Log(
                "Lexer Output",
                REPLLogLevel.DEBUG,
                string.Join(" ", tokens)
            );

            startStage = DateTime.Now;
            Parser p = new(tokens);
            ASTRoot r = p.ParseProgram();
            stageTimes.Add("Parse", DateTime.Now - startStage);
            Log(
                "Parser Output",
                REPLLogLevel.DEBUG,
                r.ToString()
            );

            if (p.Errors.Length > 0)
            {
                Log(
                    "Parser Errors",
                    REPLLogLevel.ERROR,
                    string.Join<ParserError>("\n", p.Errors)
                );
                return;
            }

            startStage = DateTime.Now;
            JaneValue v = JaneValue.Abyss;
            JnCompiler c = new(SymbolTable, Constants);
            c.Compile(r);
            stageTimes.Add("Compile", DateTime.Now - startStage);

            if (c.Errors.Length > 0)
            {
                Log(
                    "Compiler Errors",
                    REPLLogLevel.ERROR,
                    string.Join<CompilerError>("\n", c.Errors)
                );
                return;
            }
            SymbolTable = c.SymbolTable;
            Constants = c.Constants;
            Log("Bytecode", REPLLogLevel.DEBUG, JnBytecode.BCToString(c.CurrentInstructions));
            Log("Constants", REPLLogLevel.DEBUG, string.Join("\n", c.Constants.Select((a, i) => $"{i}: {a.Inspect()}::{a.Type}")));

            startStage = DateTime.Now;
            JnVM vm = new(c.Constants, c.CurrentInstructions, Globals);
            try
            {
                Thread vmT = new(new ThreadStart(vm.Run));
                vmT.Start();
                while (vmT.IsAlive && !cancelVm) {
#if DEBUG
                    Thread.Sleep(500);
                    if (vmT.IsAlive) Log("VM Stack", REPLLogLevel.DEBUG, string.Join("\n", vm.ValueStack.Select(a => a.Inspect())));
#endif
                }
                Globals = vm.Store;
            }
            catch (Exception e)
            {
                Log(e.Message, REPLLogLevel.ERROR);
                Log($"Errored at Instruction: {vm.Position:0000}", REPLLogLevel.DEBUG);
                return;
            }
            stageTimes.Add("Run", DateTime.Now - startStage);
            v = vm.TempReg;

            if (v != JaneValue.Abyss && !cancelVm)
                AnsiConsole.MarkupLineInterpolated($"[{(
                    TypeColors.TryGetValue(v.Type, out Color cl) ?
                        cl.ToMarkup() :
                        LogLevelColors[REPLLogLevel.RESULT].ToMarkup()
                )}]{v.Inspect()}[/]");
            else
                Log("Output was abyss", REPLLogLevel.INFO);
            cancelVm = false;
            stageTimes.Add("Total", DateTime.Now - start);
            Log("Times", REPLLogLevel.DEBUG, string.Join("\n", stageTimes.Select(entry => $"{entry.Key}: {entry.Value}")));
        }

        void Log(string message, REPLLogLevel logLevel = REPLLogLevel.RESULT, string description = "")
        {
            if (logLevel >= LogLevel)
            {
                AnsiConsole.Foreground = LogLevelColors[logLevel];
                if (logLevel == REPLLogLevel.RESULT)
                {
                    AnsiConsole.WriteLine(message);
                    if (!string.IsNullOrEmpty(description))
                        AnsiConsole.WriteLine(description);
                    AnsiConsole.ResetColors();
                    return;
                }
                AnsiConsole.WriteLine($"[{logLevel}] ({DateTime.Now:HH:mm:ss}) {message}");
                if (!string.IsNullOrEmpty(description))
                    AnsiConsole.WriteLine(description);
            }
        }

        void REPLCommand(string command, string[]? args)
        {
            switch (command)
            {
                case "exit":
                case "quit":
                    ShouldEnd = true;
                    break;
                case "help":
                    // TODO
                    AnsiConsole.WriteLine("Help yourself bro :skull emoji:");
                    break;
                case "rload":
                    Reset();
                    goto load; // haha im falling through switch cases the way i want c#
                case "load":
                    load:
                    if (args == null || args.Length < 1)
                        AnsiConsole.WriteLine($"Load command requires a filename");
                    else
                    {
                        try
                        {
                            string input = File.ReadAllText(string.Join(" ", args));
                            Eval(input);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException)
                        {
                            AnsiConsole.WriteLine("File not found or inaccessible ':(");
                        }
                    }
                    break;
                case "log":
                    if (args == null || args.Length < 1)
                        AnsiConsole.WriteLine("Log command requires a Loglevel");
                    else
                    {
                        if (Enum.TryParse(args[0], true, out REPLLogLevel result))
                        {
                            LogLevel = result;
                        }
                        else
                        {
                            AnsiConsole.WriteLine("Unrecognized Log Level. Choices are: " + string.Join(", ", Enum.GetNames<REPLLogLevel>()));
                        }
                    }
                    break;
                case "reset":
                    Reset();
                    break;
                case "clear":
                    Console.Clear();
                    break;
                default:
                    AnsiConsole.WriteLine($"Command \"{command}\" not recognized");
                    break;
            }
        }

        public void Reset()
        {
            Constants = [];
            SymbolTable = new SymbolTable();
        }
        void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            cancelVm = true;
            e.Cancel = true;
        }
        public enum REPLLogLevel
        {
            DEBUG,
            INFO,
            WARNING,
            ERROR,
            RESULT
        }
        private static readonly Dictionary<REPLLogLevel, Color> LogLevelColors = new() {
            { REPLLogLevel.DEBUG, Color.Grey42 },
            { REPLLogLevel.INFO, Color.Grey42 },
            { REPLLogLevel.WARNING, Color.Yellow },
            { REPLLogLevel.ERROR, Color.Red },
            { REPLLogLevel.RESULT, Color.Green }
        };

        private static readonly Dictionary<string, Color> TypeColors = new()
        {
            { "int", Color.Green },
            { "bool", Color.Blue },
            { "str", Color.Salmon1 },
            { "char", Color.Orange3 },
            { "float", Color.Olive }
        };
    }
}
