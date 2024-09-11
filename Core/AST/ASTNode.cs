namespace Jane.Core
{
    public interface IStatement : IASTNode
    {
    }
    public interface IExpression : IASTNode
    {
    }

    public interface IASTNode
    {
        public Token Token { get; set; }
        public string ToString();
    }
}
