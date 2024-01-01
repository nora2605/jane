namespace SHJI
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Dictionary<string, string> params = ParseArguments(args);
            // if (params.HasKey("files")) params["files"].Select(x => Interpreter.ExecuteFile(x));
            // if (params.HasKey("debug"))
            REPL.Start(parser_debug: true);
            // else REPL.Start();
        }
    }
}