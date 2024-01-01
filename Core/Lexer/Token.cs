using System.Collections.Immutable;

namespace Jane.Lexer
{
    public enum TokenType
    {
        ILLEGAL,
        EOF,
        EOL,
        IDENT,
        DOT,
        INT,
        ASSIGN,
        PLUS,
        COLON,
        MINUS,
        BANG,
        ASTERISK,
        SLASH,
        DECREMENT,
        INCREMENT,
        LTE,
        GTE,
        LT,
        GT,
        EQ,
        NOT_EQ,
        COMMA,
        SEMICOLON,
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        FUNCTION,
        LET,
        TRUE,
        FALSE,
        IF,
        ELSE,
        RETURN,
        ABYSS,
        DOUBLEARROW,
        SINGLEARROW,
        LSQB,
        RSQB,
        HAT,
        FOR,
        IN,
        ACCESSOR,
        AT,
        VERBATIM_STRING,
        DOUBLEQUOTE,
        SINGLEQUOTE,
        COERCER,
        CHAR_CONTENT,
        FLOAT,
        RANGE,
        RAW_DOUBLEQUOTE,
        STRING_CONTENT,
        VERBATIM_INTERPOLATED_STRING,
        LINTERPOLATE,
        CONCAT
    }

    public struct Token(TokenType Type, string Literal)
    {
        public TokenType Type { get; set; } = Type;
        public string Literal { get; set; } = Literal;
        public int Line { get; set; }
        public int Column { get; set; }

        public Token(TokenType Type, string Literal, int Line, int Column) : this(Type, Literal)
        {
            this.Line = Line;
            this.Column = Column;
        }

        public override readonly string ToString() => TokenConstants.HumanTokenType[Type];
        public static string ToString(TokenType type) => TokenConstants.HumanTokenType[type];
    }

    public static class TokenConstants
    {
        public readonly static ImmutableDictionary<TokenType, string> HumanTokenType = new Dictionary<TokenType, string>() {
            { TokenType.ILLEGAL, "ILLEGAL" },
            { TokenType.EOF, "End of File" },
            { TokenType.IDENT, "Identifier" },
            { TokenType.INT, "Integer" },
            { TokenType.ASSIGN, "=" },
            { TokenType.PLUS, "+" },
            { TokenType.MINUS, "-" },
            { TokenType.LBRACE, "{" },
            { TokenType.RBRACE, "}" },
            { TokenType.FUNCTION, "fn" },
            { TokenType.BANG, "!" },
            { TokenType.ASTERISK, "*" },
            { TokenType.COLON, ":" },
            { TokenType.COMMA, "," },
            { TokenType.SLASH, "/" },
            { TokenType.SEMICOLON, ";" },
            { TokenType.LT, "<" },
            { TokenType.GT, ">" },
            { TokenType.EQ, "==" },
            { TokenType.NOT_EQ, "!=" },
            { TokenType.LPAREN, "(" },
            { TokenType.RPAREN, ")" },
            { TokenType.LET, "let" },
            { TokenType.TRUE, "true" },
            { TokenType.FALSE, "false" },
            { TokenType.IF, "if" },
            { TokenType.ELSE, "else" },
            { TokenType.RETURN, "ret" },
            { TokenType.ABYSS, "abyss" },
            { TokenType.EOL, "Newline" }
        
        }.ToImmutableDictionary();
    }

    public static class TokenLookup
    {
        private static readonly Dictionary<string, TokenType> keywords = new()
        {
            { "fn", TokenType.FUNCTION },
            { "let", TokenType.LET },
            { "true", TokenType.TRUE },
            { "false", TokenType.FALSE },
            { "if", TokenType.IF },
            { "else", TokenType.ELSE },
            { "ret", TokenType.RETURN },
            { "return", TokenType.RETURN },
            { "void", TokenType.ABYSS },
            { "abyss", TokenType.ABYSS },
            { "null", TokenType.ABYSS },
            { "for", TokenType.FOR },
            { "in", TokenType.IN },
            { "raw\"", TokenType.RAW_DOUBLEQUOTE },
            { "r\"", TokenType.RAW_DOUBLEQUOTE }
        };

        public static TokenType LookupIdent(string ident)
        {
            if (keywords.TryGetValue(ident, out TokenType tok))
            {
                return tok;
            }
            return TokenType.IDENT;
        }
    }
}