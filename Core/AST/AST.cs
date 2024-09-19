using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Jane.Core
{
    public struct ASTRoot : IASTNode
    {
        public IStatement[] Statements;
        public Token Token { get; set; }

        public override readonly string ToString()
        {
            string output = "";
            foreach (var s in Statements)
            {
                output += s.ToString() + "\n";
            }

            return output;
        }
    }

    public struct FunctionDecl : IStatement
    {
        public Token Token { get; set; }
        public Identifier Name;
        public Identifier[] Args;
        public Identifier? Type;
        public BlockStatement Body;

        public override readonly string ToString() => $"fn {Name}({string.Join(", ", Args)}) {(Type == null ? "" : $"-> {Type}")}{Body}";
    }

    public struct LetExpression : IExpression
    {
        public Token Token { get; set; }
        public Identifier Name;
        public IExpression? Value;
        public readonly override string ToString() => $"{Token.Literal} {Name}{(Value is null ? "" : $" = {Value}")}";
    }

    public struct TernaryExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Condition;
        public IExpression If;
        public IExpression Else;
        public readonly override string ToString() => $"{Condition} ? {If} : {Else}";
    }

    public struct ArrayLiteral : IExpression
    {
        public Token Token { get; set; }
        public IExpression[] Elements;
        public readonly override string ToString() => $"[{string.Join<IExpression>(" ", Elements)}]";
    }

    public struct IndexingExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Indexed;
        public IExpression Index;

        public readonly override string ToString() => $"{Indexed}[{Index}]";
    }

    public struct ReturnStatement : IStatement
    {
        public Token Token { get; set; }
        public IExpression? ReturnValue;

        public override readonly string ToString() => $"{Token.Literal} {(ReturnValue is null ? "" : ReturnValue)}";
    }

    public struct ExpressionStatement : IStatement
    {
        public Token Token { get; set; }
        public IExpression Expression;

        public override readonly string ToString() => $"{Expression}";
    }

    public struct Identifier : IExpression
    {
        public Token Token { get; set; }
        public string Value;
        public string? Type;
        public override readonly string ToString() => $"{Value}{(Type is null ? "" : $": {Type}")}";
    }

    public struct CallExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Function;
        public IExpression[] Arguments;

        public override readonly string ToString() => $"{Function}({string.Join<IExpression>(", ", Arguments)})";
    }

    public struct LambdaExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Arguments; // Tuple or Identifier
        public IStatement Body;
        public override readonly string ToString() => $"{Arguments} => {Body}";
    }

    public struct TupleLiteral : IExpression
    {
        public Token Token { get; set; }
        public IExpression[] Elements;

        public override readonly string ToString() => $"({string.Join<IExpression>(", ", Elements)})";
    }

    public struct IntegerLiteral : IExpression
    {
        public Token Token { get; set; }
        public BigInteger Value;
        public override readonly string ToString() => $"{Value}";
    }

    public struct FloatLiteral : IExpression
    {
        public Token Token { get; set; }
        public double Value;

        public override readonly string ToString() => $"{Value}";
    }

    public struct PrefixExpression : IExpression
    {
        public Token Token { get; set; }
        public string Operator;
        public IExpression Right;
        public override readonly string ToString() => $"({Operator}{Right})";
    }

    public struct InfixExpression : IExpression
    {
        public Token Token { get; set; }
        public string Operator;
        public IExpression Left;
        public IExpression Right;
        public override readonly string ToString() => $"({Left} {Operator} {Right})";
    }

    public struct PostfixExpression : IExpression
    {
        public Token Token { get; set; }
        public string Operator;
        public IExpression Left;
        public override readonly string ToString() => $"({Left}{Operator})";
    }

    public struct BooleanLiteral : IExpression
    {
        public Token Token { get; set; }
        public bool Value;
        public override readonly string ToString() => Token.Literal;
    }

    public struct IfExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Condition;
        public IStatement Cons; // If
        public IStatement? Alt; // Else

        public override readonly string ToString() => $"if {Condition} {Cons} {(Alt is null ? "" : $"else {Alt}")}";
    }

    public struct Assignment : IExpression
    {
        public Token Token { get; set; }
        public Identifier Name;
        public IExpression Value;
        public override readonly string ToString() => $"{Name} = {Value}";
    }

    public struct OperatorAssignment : IExpression
    {
        public Token Token { get; set; }
        public Identifier Name;
        public string Operator;
        public IExpression Value;

        public override readonly string ToString() => $"{Name} {Operator}= {Value}";
    }

    public struct CharLiteral : IExpression
    {
        public Token Token { get; set; }
        public string Value; // Needs to be able to contain escape sequences until Interpretation
        public override readonly string ToString() => $"'{Value}'";
    }

    public struct StringLiteral : IExpression
    {
        public Token Token { get; set; }
        public IExpression[] Expressions;
        public override readonly string ToString() => $"\"{string.Join<IExpression>("", Expressions)}\"";
    }

    public struct StringContent : IExpression
    {
        public Token Token { get; set; }
        public string Value;
        public override readonly string ToString() => Value;
    }

    public struct Interpolation : IExpression
    {
        public Token Token { get; set; }
        public IExpression Content;
        public override readonly string ToString() => "${" + Content.ToString() + "}";
    }

    public struct Abyss : IExpression
    {
        public Token Token { get; set; }
        public override readonly string ToString() => "abyss";
    }

    public struct ClassDecl : IStatement
    {
        public Token Token { get; set; }
        public Identifier Name;
        public BlockStatement Body;
        public override readonly string ToString() => $"class {Name} {Body}";
    }
    public struct BlockStatement(IStatement single) : IStatement
    {
        public IStatement[] Statements = [single];
        public Token Token { get; set; }

        public readonly string TokenLiteral() => Token.Literal;
        public override readonly string ToString() => $"{{\n{string.Join<IStatement>("\n", Statements)}\n}}";
    }
}
