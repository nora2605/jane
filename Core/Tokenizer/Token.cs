using System.Collections.Immutable;

namespace Jane.Core
{
    public struct Token(TokenType Type, string Literal)
    {
        /// <summary>
        /// Dictionary of alphanumeric keywords to corresponding TokenType
        /// </summary>
        public static ImmutableDictionary<string, TokenType> Keywords { get; } = new Dictionary<string, TokenType>() {
            { "abyss",      TokenType.ABYSS     },
            { "true",       TokenType.TRUE      },
            { "false",      TokenType.FALSE     },

            { "class",      TokenType.CLASS     },
            { "struct",     TokenType.STRUCT    },
            { "trait",      TokenType.TRAIT     },
            { "ext",        TokenType.EXT       },
            { "type",       TokenType.TYPE      },
            { "realm",      TokenType.REALM     },
            { "file",       TokenType.FILE      },
            { "use",        TokenType.USE       },
            { "fn",         TokenType.FN        },
            { "let",        TokenType.LET       },
            { "new",        TokenType.NEW       },
            { "ret",        TokenType.RETURN    },
            { "match",      TokenType.MATCH     },
            { "if",         TokenType.IF        },
            { "else",       TokenType.ELSE      },
            { "while",      TokenType.WHILE     },
            { "loop",       TokenType.LOOP      },
            { "for",        TokenType.FOR       },
            { "break",      TokenType.BREAK     },
            { "continue",   TokenType.CONTINUE  },

            { "xor",        TokenType.L_XOR     },
            { "in",         TokenType.IN        },
            { "enum",       TokenType.ENUM      },
        }.ToImmutableDictionary();

        /// <summary>
        /// Dictionary of Single Character Tokens which can't be altered in meaning by any chars after
        /// </summary>
        public static ImmutableDictionary<char, TokenType> PrefixFree { get; } = new Dictionary<char, TokenType>() {
            { '*', TokenType.MUL },
            { '/', TokenType.DIV },
            { '(', TokenType.LPAREN },
            { ')', TokenType.RPAREN },
            { '[', TokenType.LSQB },
            { ']', TokenType.RSQB },
            { '{', TokenType.LCRB },
            { '}', TokenType.RCRB },
            { ';', TokenType.SEMICOLON },
            { '%', TokenType.REMAINDER },
            { '\0', TokenType.EOF },
            { '\n', TokenType.EOL },
#if DEBUG
            { '\\', TokenType.EOL },
#endif
            { ',', TokenType.COMMA },
            { '_', TokenType.DISCARD },
            { '$', TokenType.CURRY }
        }.ToImmutableDictionary();

        /// <summary>
        /// Dictionary of constant tokens which are not prefix-free
        /// </summary>
        public static ImmutableDictionary<string, TokenType> NotPrefixFree { get; } = new Dictionary<string, TokenType>()
        {
            { "+", TokenType.PLUS },
            { "-", TokenType.MINUS },
            { "++", TokenType.INCREMENT },
            { "--", TokenType.MINUS },
            { "->", TokenType.FN_TYPE },
            { "^", TokenType.POWER },
            { "^^", TokenType.B_XOR },
            { "=", TokenType.ASSIGN },
            { "==", TokenType.EQUAL },
            { "=>", TokenType.LAMBDA },
            { "!", TokenType.EXCLAM },
            { "!=", TokenType.NOT_EQUAL },
            { ">", TokenType.GT },
            { "<", TokenType.LT },
            { ">=", TokenType.GTE },
            { "<=", TokenType.LTE },
            { ">>", TokenType.RIGHT_SHIFT },
            { "<<", TokenType.LEFT_SHIFT },
            { "&", TokenType.B_AND },
            { "&&", TokenType.L_AND },
            { "|", TokenType.B_OR },
            { "||", TokenType.L_OR },
            { "~", TokenType.CONCAT },
            { "~~", TokenType.B_NEG },
            { "?", TokenType.INTERRO },
            { "?.", TokenType.ABYSS_ACCESSOR },
            { "??", TokenType.ABYSS_COALESC },
            { ".", TokenType.ACCESSOR },
            { "..", TokenType.RANGE },
            { ":", TokenType.COLON },
            { "::", TokenType.COERCE }
        }.ToImmutableDictionary();

        private static TokenType[] UnreadableTokenTypes { get; } = [
            TokenType.EOF,
            TokenType.EOL
        ];
        private static TokenType[] VariableTokenTypes { get; } = [
            TokenType.ILLEGAL,
            TokenType.IDENTIFIER,
            TokenType.NUMERIC,
            TokenType.STRING_CONTENT,
            TokenType.CHAR_CONTENT,
            TokenType.FLAGS
        ];

        public TokenType Type { get; set; } = Type;
        public string Literal { get; set; } = Literal;
        public int Line { get; set; }
        public int Column { get; set; }

        public Token(TokenType Type, string Literal, int Line, int Column) : this(Type, Literal)
        {
            this.Line = Line;
            this.Column = Column;
        }

        public override readonly string ToString() {
            if (UnreadableTokenTypes.Contains(Type))
                return $"<{Type} at L{Line}C{Column}>";
            else if (VariableTokenTypes.Contains(Type))
                return $"<{Type} \"{Literal}\" at L{Line}C{Column}>";
            else
                return $"<\"{Literal}\" at L{Line}C{Column}>";
        }
    }
}