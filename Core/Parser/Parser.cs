using Jane.AST;
using Jane.Lexer;
using System.ComponentModel.Design;
using System.Globalization;

namespace Jane.Parser
{
    public class Parser
    {
        #region statics
        private delegate IExpression? PrefixParseFn();
        private delegate IExpression? InfixParseFn(IExpression? left);
        private delegate IExpression? PostfixParseFn(IExpression? left);

        private static readonly Dictionary<TokenType, OperatorPrecedence> priorities = new()
        {
            { TokenType.EQ, OperatorPrecedence.EQUALS },
            { TokenType.NOT_EQ, OperatorPrecedence.EQUALS },
            { TokenType.ASSIGN, OperatorPrecedence.EQUALS },
            { TokenType.COMMA, OperatorPrecedence.TUPLE },
            { TokenType.LT, OperatorPrecedence.COMPARE },
            { TokenType.GT, OperatorPrecedence.COMPARE },
            { TokenType.PLUS, OperatorPrecedence.SUM },
            { TokenType.MINUS, OperatorPrecedence.SUM },
            { TokenType.CONCAT, OperatorPrecedence.SUM },
            { TokenType.ASTERISK, OperatorPrecedence.PRODUCT },
            { TokenType.SLASH, OperatorPrecedence.PRODUCT },
            { TokenType.HAT, OperatorPrecedence.POWER },
            //{ TokenType.AND, OperatorPrecedence.COMPARE },
            //{ TokenType.OR, OperatorPrecedence.COMPARE },
            { TokenType.LTE, OperatorPrecedence.COMPARE },
            { TokenType.GTE, OperatorPrecedence.COMPARE },
            { TokenType.INCREMENT, OperatorPrecedence.PREFIX },
            { TokenType.DECREMENT, OperatorPrecedence.PREFIX },
            { TokenType.LPAREN, OperatorPrecedence.CALL },
            { TokenType.LSQB, OperatorPrecedence.CALL },
            //{ TokenType.IN, OperatorPrecedence.COMPARE },
            //{ TokenType.LEFT_SHIFT, OperatorPrecedence.BITWISE },
            //{ TokenType.RIGHT_SHIFT, OperatorPrecedence.BITWISE },
            //{ TokenType.BITWISE_AND, OperatorPrecedence.BITWISE },
            //{ TokenType.BITWISE_OR, OperatorPrecedence.BITWISE },
            //{ TokenType.XOR, OperatorPrecedence.BITWISE }
            // BITWISE NAND => "nand"
            // BITWISE NOR => "nor"
            // BITWISE NXOR => "nxor"
        };


        private readonly Dictionary<TokenType, PrefixParseFn> UnaryParseFns;
        private readonly Dictionary<TokenType, InfixParseFn> BinaryParseFns;
        private readonly Dictionary<TokenType, PostfixParseFn> PostfixParseFns;

        internal readonly Tokenizer Tokenizer;
        internal static readonly IFormatProvider FormatProvider = new CultureInfo("en-US");

        Token curToken;
        Token peekToken;

        public ParserError[] Errors { get => errors.ToArray(); }
        private readonly List<ParserError> errors;
        #endregion

        public Parser(Tokenizer tokenizer)
        {
            Tokenizer = tokenizer;
            NextToken();
            NextToken();
            errors = new();

            UnaryParseFns = new()
            { 
                { TokenType.IDENT, ParseIdentifier },
                { TokenType.ABYSS, ParseAbyss },
                { TokenType.INT, ParseIntegerLiteral },
                { TokenType.TRUE, ParseBoolean },
                { TokenType.FALSE, ParseBoolean },
                { TokenType.BANG, ParsePrefixExpression },
                { TokenType.MINUS, ParsePrefixExpression },
                { TokenType.LPAREN, ParseGroupedExpression },
                { TokenType.IF, ParseIfExpression },
                { TokenType.LET, ParseLetExpression },
                //{ TokenType.BITWISE_NEGATE, ParsePrefixExpression },
                { TokenType.INCREMENT, ParsePrefixExpression },
                { TokenType.DECREMENT, ParsePrefixExpression },
                { TokenType.DOUBLEQUOTE, ParseInterpolatedString },
                { TokenType.RAW_DOUBLEQUOTE, ParseString },
                { TokenType.VERBATIM_INTERPOLATED_STRING, ParseVerbIntString },
                { TokenType.VERBATIM_STRING, ParseVerbString },
                { TokenType.FLOAT, ParseFloatingPoint },
                { TokenType.SINGLEQUOTE, ParseChar },
                { TokenType.LSQB, ParseArray }
            };

            BinaryParseFns = new()
            {
                { TokenType.PLUS, ParseInfixExpression },
                { TokenType.MINUS, ParseInfixExpression },
                { TokenType.SLASH, ParseInfixExpression },
                { TokenType.ASTERISK, ParseInfixExpression },
                { TokenType.HAT, ParseInfixExpression },
                { TokenType.EQ, ParseInfixExpression },
                { TokenType.NOT_EQ, ParseInfixExpression },
                { TokenType.LT, ParseInfixExpression },
                { TokenType.GT, ParseInfixExpression },
                { TokenType.GTE, ParseInfixExpression },
                { TokenType.LTE, ParseInfixExpression },
                { TokenType.LPAREN, ParseCallExpression },
                { TokenType.ASSIGN, ParseAssignment },
                { TokenType.CONCAT, ParseInfixExpression },
                { TokenType.LSQB, ParseIndexingExpression },
                // { TokenType.COMMA, ParseTuple }
            };

            PostfixParseFns = new()
            {
                { TokenType.INCREMENT, ParsePostfixExpression },
                { TokenType.DECREMENT, ParsePostfixExpression }
            };
        }

        #region Parse Statements
        /// <summary>
        /// Parses the AST for the Program from the Tokens supplied by the Tokenizer of the Instance
        /// </summary>
        /// <returns>The AST of the Program</returns>
        public ASTRoot ParseProgram()
        {
            ASTRoot program = new() { Token = curToken };
            List<IStatement> statements = new();
            program.statements = Array.Empty<IStatement>();
            while (curToken.Type != TokenType.EOF)
            {
                IStatement? statement = ParseStatement();
                if (statement is not null)
                statements.Add(statement);
                NextToken();
            }
            program.statements = statements.ToArray();
            return program;
        }

        IStatement? ParseStatement()
        {
            while (CurTokenIs(TokenType.SEMICOLON) || CurTokenIs(TokenType.EOL)) NextToken();
            var statement =  curToken.Type switch
            {
                TokenType.RETURN => ParseReturnStatement(),
                TokenType.FUNCTION => ParseFunctionLiteral(),
                TokenType.FOR => ParseForLoop(),
                _ => ParseExpressionStatement()
            };
            return statement;
        }

        IExpression? ParseAssignment(IExpression? left)
        {
            if (left is null or not Identifier) return null;
            Assignment ass = new() { Token = curToken, Name = (Identifier)left };
            NextToken();
            IExpression? e = ParseExpression();
            if (e is not null) ass.Value = e;
            else return null;
            return ass;
        }

        IStatement? ParseForLoop()
        {
            ForLoop loop = new() { Token = curToken };
            if (!ExpectPeek(TokenType.LET))
            {
                return null;
            }
            NextToken();
            var e = ParseIdentifier();
            if (e is null) return null;
            loop.Iterator = (Identifier)e;
            if (!ExpectPeek(TokenType.IN))
            {
                return null;
            }
            NextToken();
            e = ParseExpression();
            if (e is null) return null;
            loop.Enumerable = e;
            if (!ExpectPeek(TokenType.LBRACE))
            {
                var s = ParseStatement();
                if (s is null) return null;
                loop.LoopContent = new BlockStatement(s);
            }
            else
            {
                loop.LoopContent = ParseBlockStatement();
            }

            return loop;
        }

        IStatement? ParseReturnStatement()
        {
            ReturnStatement statement = new() { Token = curToken };
            NextToken();
            if (!UnaryParseFns.ContainsKey(curToken.Type)) return statement;
            var e = ParseExpression();
            if (e is null) return null;
            statement.ReturnValue = e;
            return statement;
        }

        IStatement? ParseExpressionStatement()
        {
            ExpressionStatement statement = new() { Token = curToken };
            IExpression? e = ParseExpression(OperatorPrecedence.LOWEST);
            if (e != null) statement.Expression = e;
            else return null;
            if (PeekTokenIs(TokenType.SEMICOLON) || PeekTokenIs(TokenType.EOL))
                NextToken(false);
            return statement;
        }

        IStatement? ParseFunctionLiteral()
        {
            FunctionLiteral literal = new() { Token = curToken };

            List<string> flags = new();
            while (ExpectPeek(TokenType.MINUS) || ExpectPeek(TokenType.DECREMENT))
            {
                if (CurTokenIs(TokenType.MINUS))
                {
                    if (ExpectPeek(TokenType.IDENT)) flags.AddRange(curToken.Literal.ToCharArray().Select(x => x.ToString()));
                }
                else
                {
                    if (ExpectPeek(TokenType.IDENT)) flags.Add(curToken.Literal);
                }
            }
            literal.Flags = flags.ToArray();
            if (!ExpectPeek(TokenType.IDENT))
            {
                return null;
            }
            literal.Name = curToken.Literal;
            if (!ExpectPeek(TokenType.LPAREN))
            {
                return null;
            }
            var par = ParseFunctionParameters();
            if (par is null) return null;
            literal.Parameters = par;

            if (ExpectPeek(TokenType.SINGLEARROW))
            {
                NextToken();
                literal.ReturnType = curToken.Literal;
            }

            if (ExpectPeek(TokenType.DOUBLEARROW))
            {
                NextToken();
                var exprStatement = ParseExpressionStatement();
                if (exprStatement is null) return null;
                literal.Body = new BlockStatement(exprStatement) { Token = curToken };
            }
            else if (ExpectPeek(TokenType.LBRACE))
            {
                literal.Body = ParseBlockStatement();
            }
            else return null;

            return literal;
        }

        Identifier[]? ParseFunctionParameters()
        {
            List<Identifier> parameters = new();
            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return Array.Empty<Identifier>();
            }
            do {
                NextToken();
                Identifier? ident = ParseIdentifier() as Identifier?;
                if (ident is null) return null;
                parameters.Add((Identifier)ident);
            } while (ExpectPeek(TokenType.COMMA));
            if (!ExpectPeek(TokenType.RPAREN)) return null;
            return parameters.ToArray();
        }

        BlockStatement ParseBlockStatement()
        {
            BlockStatement block = new() { Token = curToken };
            List<IStatement> statements = new();
            NextToken();
            while (!CurTokenIs(TokenType.RBRACE) && !CurTokenIs(TokenType.EOF))
            {
                IStatement? statement = ParseStatement();
                if (statement is null)
                {
                    NextToken();
                    continue;
                }
                statements.Add(statement);
                NextToken();
            }
            block.Statements = statements.ToArray();
            return block;
        }
        #endregion

        #region Parse general Expressions
        IExpression? ParseExpression(OperatorPrecedence precedence=OperatorPrecedence.LOWEST)
        {
            if (!UnaryParseFns.TryGetValue(curToken.Type, out PrefixParseFn? prefix))
            {
                NoPrefixParseFnError(curToken.Type);
                return null;
            }
            else
            {
                IExpression? left = prefix();
                while (!PeekTokenIs(TokenType.EOL) && !PeekTokenIs(TokenType.SEMICOLON) && precedence < PeekPrecedence())
                {
                    if (!BinaryParseFns.TryGetValue(peekToken.Type, out InfixParseFn? infix))
                    {
                        if (PostfixParseFns.TryGetValue(peekToken.Type, out PostfixParseFn? postfix))
                        {
                            NextToken(false);
                            left = postfix(left);
                        }
                        return left;
                    }
                    else
                    {
                        NextToken(false);
                        left = infix(left);
                    }
                }
                return left;
            }
        }

        IExpression? ParseArray()
        {
            ArrayLiteral al = new() { Token = curToken };
            var e = ParseExpressionList(TokenType.RSQB);
            if (e is null) return null;
            al.Elements = e;
            return al;
        }

        IExpression? ParseIndexingExpression(IExpression? left)
        {
            if (left is null) return null;
            IndexingExpression i = new() { Token = curToken, Indexee = left };
            NextToken();
            var e = ParseExpression();
            if (!ExpectPeek(TokenType.RSQB)) return null;
            if (e is null) return null;
            i.Index = e;
            return i;
        }

        IExpression? ParseAbyss()
        {
            AST.Abyss abyss = new() { Token = curToken };
            NextToken();
            return abyss;
        }

        IExpression? ParseLetExpression()
        {
            LetExpression expression = new() { Token = curToken };
            if (!ExpectPeek(TokenType.IDENT))
            {
                PeekError(TokenType.IDENT, ParserErrorType.UnexpectedToken);
                return null;
            }
            Identifier? ident = ParseIdentifier() as Identifier?;
            if (ident is null) return null;
            expression.Name = (Identifier)ident;

            if (!ExpectPeek(TokenType.ASSIGN))
                return expression;
            NextToken();

            IExpression? e = ParseExpression();
            if (e is null) return null;
            expression.Value = e;

            return expression;
        }

        IExpression? ParseGroupedExpression()
        {
            NextToken();
            var expr = ParseExpression(OperatorPrecedence.LOWEST);
            if (!ExpectPeek(TokenType.RPAREN)) return null;
            return expr;
        }

        IExpression? ParseIfExpression()
        {
            IfExpression expr = new() { Token = curToken };
            bool inParens = ExpectPeek(TokenType.LPAREN);
            NextToken();
            IExpression? cond = ParseExpression(OperatorPrecedence.LOWEST);
            if (cond is null) return null;
            expr.Condition = cond;
            if (inParens && !ExpectPeek(TokenType.RPAREN)) return null;
            bool inBraces = ExpectPeek(TokenType.LBRACE);
            if (!inParens && !inBraces) errors.Add(new ParserError("Neither providing parens () or braces {} for an if expression is potentially ambiguous. Please enclose either the condition or the block statement", curToken, ParserErrorType.Ambiguous));
            if (inBraces)
                expr.Cons = ParseBlockStatement();
            else
            {
                NextToken();
                var s = ParseStatement();
                if (s is null) return null;
                expr.Cons = new BlockStatement(s) { Token = s.Token };
            }
            if (ExpectPeek(TokenType.ELSE))
            {
                inBraces = ExpectPeek(TokenType.LBRACE);
                if (inBraces) expr.Alt = ParseBlockStatement();
                else
                {
                    NextToken();
                    var s = ParseStatement();
                    if (s is null) expr.Alt = null;
                    else expr.Alt = new BlockStatement(s) { Token = curToken };
                }
            }
            return expr;
        }

        IExpression? ParseIdentifier()
        {
            Identifier ident = new() { Token = curToken, Value = curToken.Literal };
            if (ExpectPeek(TokenType.COLON))
            {
                ident.Type = peekToken.Literal;
                NextToken();
            }
            return ident;
        }

        IExpression? ParseCallExpression(IExpression? left)
        {
            if (left is null) return null;
            CallExpression ce = new()
            {
                Token = curToken,
                Function = left
            };
            var args = ParseArguments();
            if (args is null) return null;
            ce.Arguments = args;
            return ce;
        }

        IExpression[]? ParseArguments()
        {
            List<IExpression> args = new();
            if (ExpectPeek(TokenType.RPAREN))
            {
                return args.ToArray();
            }
            do
            {
                NextToken();
                var e = ParseExpression();
                if (e is null) return null;
                args.Add(e);
            } while (ExpectPeek(TokenType.COMMA));
            if (!ExpectPeek(TokenType.RPAREN)) return null;
            return args.ToArray();
        }

        // comma seperated
        IExpression[]? ParseExpressionList(TokenType end)
        {
            List<IExpression> l = new();
            if (ExpectPeek(end)) return l.ToArray();
            do
            {
                ExpectPeek(TokenType.COMMA);
                NextToken();
                var e = ParseExpression();
                if (e is null) return null;
                l.Add(e);
            } while (!ExpectPeek(end));
            return l.ToArray();
        }
        #endregion

        #region Parse Types
        IExpression? ParseChar()
        {
            CharLiteral lit = new() { Token = curToken };
            if (!ExpectPeek(TokenType.CHAR_CONTENT)) return null;
            lit.Value = curToken.Literal;
            if (!ExpectPeek(TokenType.SINGLEQUOTE)) return null;
            return lit;
        }

        IExpression? ParseInterpolatedString()
        {
            InterpolatedStringLiteral lit = new() { Token = curToken };
            List<IExpression> contents = new();
            while (ExpectPeek(TokenType.STRING_CONTENT) || ExpectPeek(TokenType.LINTERPOLATE))
            {
                if (CurTokenIs(TokenType.STRING_CONTENT))
                {
                    contents.Add(new AST.StringContent() { Token = curToken, EscapedValue = curToken.Literal });
                }
                else
                {
                    var e = ParseInterpolation();
                    if (e is null) return null;
                    contents.Add(e);
                }
            }
            lit.Expressions = contents.ToArray();
            if (!ExpectPeek(TokenType.DOUBLEQUOTE)) return null;
            return lit;
        }
        IExpression? ParseInterpolation()
        {
            Interpolation i = new() { Token = curToken };
            NextToken();
            var e = ParseExpression();
            if (e is null) return null;
            i.Content = e;
            if (!ExpectPeek(TokenType.RBRACE)) return null;
            return i;
        }

        IExpression? ParseVerbIntString()
        {
            InterpolatedVerbatimStringLiteral lit = new() { Token = curToken };
            List<IExpression> contents = new();
            while (ExpectPeek(TokenType.STRING_CONTENT) || ExpectPeek(TokenType.LINTERPOLATE))
            {
                if (CurTokenIs(TokenType.STRING_CONTENT))
                {
                    contents.Add(new AST.StringContent() { Token = curToken, EscapedValue = curToken.Literal });
                }
                else
                {
                    var e = ParseInterpolation();
                    if (e is null) return null;
                    contents.Add(e);
                }
            }
            lit.Expressions = contents.ToArray();
            if (!ExpectPeek(TokenType.DOUBLEQUOTE)) return null;
            return lit;
        }

        IExpression? ParseVerbString()
        {
            VerbatimStringLiteral lit = new() { Token = curToken };
            if (!ExpectPeek(TokenType.STRING_CONTENT)) return null;
            lit.EscapedValue = curToken.Literal;
            if (!ExpectPeek(TokenType.DOUBLEQUOTE)) return null;
            return lit;
        }

        IExpression? ParseString()
        {
            RawStringLiteral lit = new() { Token = curToken };
            if (!ExpectPeek(TokenType.STRING_CONTENT)) return null;
            lit.EscapedValue = curToken.Literal;
            if (!ExpectPeek(TokenType.DOUBLEQUOTE)) return null;
            return lit;
        }

        IExpression? ParseIntegerLiteral()
        {
            INumberLiteral lit;
            if (!int.TryParse(curToken.Literal, FormatProvider, out int i))
                if (!long.TryParse(curToken.Literal, FormatProvider, out long l))
                    if (!Int128.TryParse(curToken.Literal, FormatProvider, out Int128 o))
                        if (!UInt128.TryParse(curToken.Literal, FormatProvider, out UInt128 u))
                            return null;
                        else lit = new UInt128Literal() { Token = curToken, Value = u };
                    else lit = new Int128Literal() { Token = curToken, Value = o };
                else lit = new LongLiteral() { Token = curToken, Value = l };
            else lit = new IntegerLiteral() { Token = curToken, Value = i };
            if (ExpectPeek(TokenType.IDENT))
            {
                lit.ImmediateCoalescion = curToken.Literal;
            }
            return lit;
        }
        IExpression? ParseFloatingPoint()
        {
            FloatLiteral lit = new() { Token = curToken };
            if (!double.TryParse(curToken.Literal, FormatProvider, out lit.Value))
                return null;
            if (ExpectPeek(TokenType.IDENT))
            {
                lit.ImmediateCoalescion = curToken.Literal;
            }
            return lit;
        }

        IExpression? ParseBoolean()
        {
            AST.Boolean lit = new() { Token = curToken };
            if (!bool.TryParse(curToken.Literal, out lit.Value))
                return null;
            return lit;
        }
        #endregion

        #region Parse Operators
        IExpression? ParsePrefixExpression()
        {
            PrefixExpression prefix = new() { Token = curToken, Operator = curToken.Literal };
            NextToken();
            IExpression? e = ParseExpression(OperatorPrecedence.PREFIX);
            if (e is not null) prefix.Right = e;
            else return null;
            return prefix;
        }
        IExpression? ParseInfixExpression(IExpression? left)
        {
            if (left is null) return null;
            InfixExpression infix = new() { Token = curToken, Operator = curToken.Literal, Left = left };
            OperatorPrecedence prec = CurPrecedence();
            NextToken();
            // Something something var += ...
            IExpression? e = ParseExpression(prec);
            if (e is not null) infix.Right = e;
            else return null;
            return infix;
        }

        IExpression? ParsePostfixExpression(IExpression? left)
        {
            if (left is null) return null;
            PostfixExpression postfix = new() { Token = curToken, Operator = curToken.Literal, Left = left };
            return postfix;
        }
        #endregion

        #region Helpers
        void NextToken(bool skipEOL = true)
        {
            do
            {
                curToken = peekToken;
                peekToken = Tokenizer.NextToken();
            } while (skipEOL && CurTokenIs(TokenType.EOL));
        }
        bool CurTokenIs(TokenType type) => curToken.Type == type;
    
        bool PeekTokenIs(TokenType type) => peekToken.Type == type;
        bool ExpectPeek(TokenType type)
        {
            if (PeekTokenIs(type))
            {
                NextToken(); return true;
            }
            else return false;
        }
        
        OperatorPrecedence PeekPrecedence()
        {
            if (priorities.TryGetValue(peekToken.Type, out OperatorPrecedence prec))
            {
                return prec;
            }
            return OperatorPrecedence.LOWEST;
        }

        OperatorPrecedence CurPrecedence()
        {
            if (priorities.TryGetValue(curToken.Type, out OperatorPrecedence prec))
            {
                return prec;
            }
            return OperatorPrecedence.LOWEST;
        }

        void PeekError(TokenType expected, ParserErrorType type=ParserErrorType.Unspecified)
        {
            ParserError pe = new($"Expected next token to be {Token.ToString(expected)}, got {Token.ToString(peekToken.Type)} instead.", peekToken, type);
            errors.Add(pe);
        }

        void NoPrefixParseFnError(TokenType type)
        {
            ParserError pe = new($"Unexpected {type} found", curToken, ParserErrorType.UnexpectedToken);
            errors.Add(pe);
        }
        #endregion
    }
    internal enum OperatorPrecedence
    {
        LOWEST,
        TUPLE,
        EQUALS,
        COMPARE,
        BITWISE,
        SUM,
        PRODUCT,
        PREFIX,
        POWER,
        CALL
    }
}
