using Jane.Core;
using Microsoft.VisualBasic;
using SHJI.Bytecode;
using SHJI.Compiler;
using SHJI.VM;
using System.Collections.Immutable;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace SHJI
{
    internal class REPL
    {
        private bool ShouldEnd { get; set; }
        public REPLLogLevel LogLevel { get; set; }

        private Colorify.Format _colorify;

        private JaneValue[] Constants = [];
        private SymbolTable SymbolTable = new();
        private JaneValue[] Globals = new JaneValue[2<<16];

        public REPL() : this(REPLLogLevel.WARNING) { }
        public REPL(REPLLogLevel logLevel)
        {
            LogLevel = logLevel;
            _colorify = new(Colorify.UI.Theme.Dark);
        }

        public void Run()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            _colorify.WriteLine(@$"SHJI v0.1");

            while (true)
            {
                REP();
                if (ShouldEnd) break;
            }
        }
        void REP()
        {
            _colorify.Write("jn> ", Colorify.Colors.txtInfo);
            string? input = Console.ReadLine();
            if (input == null)
            {
                _colorify.WriteLine("");
                return;
            }
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
            while (!t.Finished)
            {
                _colorify.Write("..> ", Colorify.Colors.txtInfo);
                string? addit = Console.ReadLine();
                if (addit == null)
                {
                    _colorify.WriteLine("");
                    break;
                }
                input += addit;
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
            SymbolTable = c.SymbolTable;
            Constants = c.Constants;
            Log("Bytecode", REPLLogLevel.DEBUG, JnBytecode.BCToString(c.Instructions));
            Log("Heap", REPLLogLevel.DEBUG, string.Join("\n", c.Constants.Select((a, i) => $"{i}: {a.Inspect()}::{a.Type}")));

            if (c.Errors.Length > 0)
            {
                Log(
                    "Compiler Errors",
                    REPLLogLevel.ERROR,
                    string.Join<CompilerError>("\n", c.Errors)
                );
                return;
            }

            startStage = DateTime.Now;
            JnVM vm = new(c.Constants, c.Instructions, Globals);
            try
            {
                vm.Run();
                Globals = vm.Globals;
            }
            catch (Exception e)
            {
                Log(e.Message, REPLLogLevel.ERROR);
                Log($"Errored at Instruction: {vm.Position:0000}", REPLLogLevel.DEBUG);
                return;
            }
            stageTimes.Add("Run", DateTime.Now - startStage);
            Log("VM Stack", REPLLogLevel.DEBUG, string.Join("\n", vm.Stack.Select(a => a.Inspect())));
            v = vm.TempReg;

            if (v != JaneValue.Abyss)
                Log(v.Inspect());
            else
                Log("Output was abyss", REPLLogLevel.INFO);

            stageTimes.Add("Total", DateTime.Now - start);
            Log("Times", REPLLogLevel.DEBUG, string.Join("\n", stageTimes.Select(entry => $"{entry.Key}: {entry.Value}")));
        }

        void Log(string message, REPLLogLevel logLevel = REPLLogLevel.RESULT, string description = "")
        {
            if (logLevel >= LogLevel)
            {
                if (logLevel == REPLLogLevel.RESULT)
                {
                    _colorify.WriteLine(message, LogLevelColorify[logLevel]);
                    if (!string.IsNullOrEmpty(description))
                        _colorify.WriteLine(description, LogLevelColorify[logLevel]);
                    return;
                }
                _colorify.WriteLine($"[{logLevel}] ({DateTime.Now:HH:mm:ss}) {message}", LogLevelColorify[logLevel]);
                if (!string.IsNullOrEmpty(description))
                    _colorify.WriteLine(description, LogLevelColorify[logLevel]);
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
                    _colorify.WriteLine("Help yourself bro :skull emoji:");
                    break;
                case "load":
                    if (args == null || args.Length < 1)
                        _colorify.WriteLine($"Load command requires a filename");
                    else
                    {
                        try
                        {
                            string input = File.ReadAllText(string.Join(" ", args));
                            Eval(input);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException)
                        {
                            _colorify.WriteLine("File not found or inaccessible ':(");
                        }
                    }
                    break;
                case "log":
                    if (args == null || args.Length < 1)
                        _colorify.WriteLine("Log command requires a Loglevel");
                    else
                    {
                        if (Enum.TryParse<REPLLogLevel>(args[0], true, out REPLLogLevel result))
                        {
                            LogLevel = result;
                        }
                        else
                        {
                            _colorify.WriteLine("Unrecognized Log Level. Choices are: " + string.Join(", ", Enum.GetNames<REPLLogLevel>()));
                        }
                    }
                    break;
                case "reset":
                    Constants = [];
                    SymbolTable = new SymbolTable();
                    break;
                default:
                    _colorify.WriteLine($"Command \"{command}\" not recognized");
                    break;
            }
        }

        static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
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
        private static ImmutableDictionary<REPLLogLevel, string> LogLevelColorify = new Dictionary<REPLLogLevel, string>() {
            { REPLLogLevel.DEBUG, Colorify.Colors.txtMuted },
            { REPLLogLevel.INFO, Colorify.Colors.txtInfo },
            { REPLLogLevel.WARNING, Colorify.Colors.txtWarning },
            { REPLLogLevel.ERROR, Colorify.Colors.txtDanger },
            { REPLLogLevel.RESULT, Colorify.Colors.txtSuccess }
        }.ToImmutableDictionary();
    }
}
