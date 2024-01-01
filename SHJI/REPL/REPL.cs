using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jane.Lexer;
using Jane.Parser;
using System.ANSIConsole;
using Jane.AST;
using SHJI.VM;

namespace SHJI
{
    internal static partial class REPL
    {
        static readonly string HEADER = @$"SHJI Version {Assembly.GetExecutingAssembly().GetName().Version}";
        const string PROMPT = "jn> ";
        const string CONTINUE = "..> ";

        static bool exiting = false;
        static bool parserDebug = false;
        static int nestingLevel = 0;
        static bool controlKeyPressed = false;

        static string prevInput = "";
        static JaneEnvironment env = new();

        public static void Start(bool parser_debug = false)
        {
            parserDebug = parser_debug;
            Console.WriteLine(HEADER);
            Console.CancelKeyPress += (sender, e) =>
            {
                exiting = true;
                Console.WriteLine("\nPress Ctrl-C again or Enter to exit the REPL.");
                Console.Write(PROMPT);
                e.Cancel = true;
            };
            while (!exiting)
            {
                REP();
            }
        }

        static void REP()
        {
            if (!controlKeyPressed) Console.Write(PROMPT);
            string? line = Console.ReadLine();
            string input = "";
            if (line is null or "")
            {
                controlKeyPressed = line is null;
                return;
            }
            if (line.StartsWith('.'))
            {
                switch (line[1..])
                {
                    case "exit":
                        exiting = true;
                        return;
                    case "repeat":
                        input = prevInput;
                        goto Parse;
                    case "clear":
                        env.Store.Clear();
                        return;
                    default:
                        Console.WriteLine("Invalid REPL Command. To exit use .exit");
                        return;
                }
            }
            exiting = false;
            input = RegexReplEndBS().Match(line).Groups["line"].Value;
            Tokenizer nestChecker = new(input);
            nestingLevel = CountNestLevel(nestChecker);
            while (RegexIncompleteLine().IsMatch(line) || nestingLevel > 0)
            {
                Console.Write(CONTINUE);
                line = Console.ReadLine();
                if (line is null or "") break;
                input += $"\n{RegexReplEndBS().Match(line).Groups["line"].Value}";
                nestChecker = new(input);
                nestingLevel = CountNestLevel(nestChecker);
            }

            prevInput = input;
            Parse:
            Tokenizer lx = new(input);
            Parser ps = new(lx);
            ASTRoot AST = ps.ParseProgram();
#if DEBUG
            if (parserDebug)
            {
                lx.Reset();
                Console.WriteLine("Lexer Output: ".Bold().Yellow());
                foreach (Token token in lx)
                    Console.Write($"{{{token.Type}, {token.Literal}, {token.Line}, {token.Column}}} ".Yellow());
                Console.WriteLine();
                Console.WriteLine("Parser Output: ".Bold().Green());
                Console.WriteLine(AST.JOHNSerialize().Green());
                Console.WriteLine("Reconstructed AST: ".Bold().Blue());
                Console.WriteLine(AST.ToString().Blue());
                if (ps.Errors.Length > 0)
                {
                    string a = ps.Errors.Select(e => e.ToString().Red()).Aggregate((a, b) => a + "\n" + b);
                    Console.WriteLine("Errors Encountered: ".Bold().Red());
                    Console.WriteLine(a);
                }
            }
            Console.WriteLine("Interpreter Output: ".Cyan().Bold());
#else
                if (ps.Errors.Length > 0) {
                    Console.WriteLine(ps.Errors.Select(e => e.ToString()).Aggregate((a, b) => a + "\n" + b));
                    return;
                }
#endif
            try
            {
                IJaneObject output = IJaneObject.JANE_UNINITIALIZED;
                Exception? e = null;
                // 256 MB of stack size so i don't have to deal with anything and i'll leave it to the OS like a true evil person
                Thread it = new(new ThreadStart(() => {
                    try
                    {
                        output = IJaneObject.JANE_ABYSS;
                    }
                    catch (Exception ex)
                    {
                        e = ex;
                    }
                }), 0b1000000000000000000000000000);

                it.Start();
                it.Join(2000);
                if (it.IsAlive)
                {
                    Console.Write($"Welp you seem to have given it some tough prompt... ");
#if DEBUG
                    Console.WriteLine();
                    Console.WriteLine($"{it.GetApartmentState()}");
#endif
                    it.Join();
                }
                if (output == IJaneObject.JANE_UNINITIALIZED) throw e ?? new Exception("uh...");

                if (output != IJaneObject.JANE_ABYSS) Console.WriteLine(output.Inspect()
#if DEBUG
                    .Cyan()
#endif
                );
            }
            catch (NotImplementedException e)
            {
                Console.Error.WriteLine(e.Message.Red());
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        static int CountNestLevel(Tokenizer t)
        {
            try
            {
                int level = 0;
                foreach (Token token in t)
                {
                    if (token.Type == TokenType.LBRACE) level++;
                    else if (token.Type == TokenType.RBRACE) level--;
                }
                return level;
            }
            catch
            {
                return 0;
            }
        }

        [GeneratedRegex(@".*[\\]\s*$")]
        private static partial Regex RegexIncompleteLine();
        [GeneratedRegex(@"(?<line>.*)[\\]\s*$|(?<line>.+$)")]
        private static partial Regex RegexReplEndBS();
    }
}
