using Jane.Lexer;
#if DEBUG
using System.Reflection;
#endif

namespace Jane.AST
{
    public interface IStatement : IASTNode
    {
    }
    public interface IExpression : IASTNode
    {
    }

    public struct BlockStatement(IStatement single) : IASTNode
    {
        public IStatement[] Statements = [single];
        public Token Token { get; set; }

        public readonly string TokenLiteral() => Token.Literal;
        public override readonly string ToString() => $"{{{(Statements.Length == 0 ? "" : Statements.Select(s => "\n\t" + s.ToString()).Aggregate((a, b) => a + b))}\n}}";
    }

    public interface INumberLiteral : IExpression
    {
        public string? ImmediateCoalescing { get; set; }
    }

    public interface IASTNode
    {
        public Token Token { get; set; }
        public string ToString();

        public string JOHNSerialize() {
            string output = "{";
#if DEBUG
            Type t = GetType();
            var fs = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var f in fs)
            {
                string? repr = (f.GetValue(this) as IASTNode)?.JOHNSerialize();
                string? arrrepr = null;
                if (repr is not null) goto Outputter;
                IASTNode[]? arr = f.GetValue(this) as IASTNode[];
                if (arr is not null && arr.Length != 0)
                    arrrepr = "[" + 
                        (f.GetValue(this) as IASTNode[])?
                            .Select(x => " " + x.JOHNSerialize())
                            .Aggregate((a, b) => a + b)
                        + "]";
                Outputter:
                output += $"{f.Name} {repr ?? arrrepr ?? $"\"{f.GetValue(this)}\""} ";
            }
#endif
            return output[..^1] + "}";
        }
    }
}
