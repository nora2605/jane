namespace Jane.Core
{
    public interface IStatement : IASTNode
    {
    }
    public interface IExpression : IASTNode
    {
    }

    public struct BlockStatement(IStatement single) : IStatement
    {
        public IStatement[] Statements = [single];
        public Token Token { get; set; }

        public readonly string TokenLiteral() => Token.Literal;
        public override readonly string ToString() => $"{{\n{string.Join<IStatement>("\n", Statements)}\n}}";
    }

    public interface IASTNode
    {
        public Token Token { get; set; }
        public string ToString();
    }
}
