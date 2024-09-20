using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Jane.Core
{
    public class Parser
    {
        private delegate IExpression? PrefixParseFn();
        private delegate IExpression? InfixParseFn(IExpression? left);
        private delegate IExpression? PostfixParseFn(IExpression? left);
        private readonly Dictionary<TokenType, PrefixParseFn> UnaryParseFns;
        private readonly Dictionary<TokenType, InfixParseFn> BinaryParseFns;
        private readonly Dictionary<TokenType, PostfixParseFn> PostfixParseFns;
        private readonly TokenType[] RightAssociativeOperators;
        private readonly TokenType[] AllowedOperators;

        private readonly Token[] tokens;
        private int readPosition;
        private Token Current { get; set; }

        public ParserError[] Errors => [.. errors];
        private readonly List<ParserError> errors;

        public Parser(IEnumerable<Token> tokens)
        {
            this.tokens = [.. tokens];
            errors = [];

            UnaryParseFns = new()
            {
                { TokenType.IDENTIFIER, ParseIdentifier },
                { TokenType.ABYSS, ParseAbyss },
                { TokenType.NUMERIC, ParseNumeric },
                { TokenType.TRUE, ParseBoolean },
                { TokenType.FALSE, ParseBoolean },
                { TokenType.EXCLAM, ParsePrefixExpression },
                { TokenType.MINUS, ParsePrefixExpression },
                { TokenType.LPAREN, ParseGroupedExpression },
                { TokenType.DOUBLE_QUOTE, ParseString },
                { TokenType.SINGLE_QUOTE, ParseChar },
                { TokenType.LET, ParseLetExpression },
                { TokenType.IF, ParseIfExpression },
                { TokenType.INCREMENT, ParsePrefixExpression },
                { TokenType.DECREMENT, ParsePrefixExpression },
                { TokenType.B_NEG, ParsePrefixExpression },
                { TokenType.LSQB, ParseArray }
            };

            // Gets parsed regularly by ParseInfixExpression
            AllowedOperators = [
                TokenType.PLUS,
                TokenType.MINUS,
                TokenType.DIV,
                TokenType.MUL,
                TokenType.REMAINDER,
                TokenType.POWER,
                TokenType.EQUAL,
                TokenType.NOT_EQUAL,
                TokenType.LT,
                TokenType.GT,
                TokenType.GTE,
                TokenType.LTE,
                TokenType.CONCAT,
                TokenType.B_AND,
                TokenType.B_OR,
                TokenType.B_XOR,
                TokenType.L_AND,
                TokenType.L_OR,
                TokenType.L_XOR,
                TokenType.LEFT_SHIFT,
                TokenType.RIGHT_SHIFT,
                TokenType.ABYSS_ACCESSOR,
                TokenType.ACCESSOR,
                TokenType.ABYSS_COALESC,
                TokenType.COERCE,
                TokenType.CURRY,
                TokenType.IN,
                TokenType.RANGE,
            ];
            RightAssociativeOperators = [
                TokenType.ASSIGN,
                TokenType.POWER
            ];

            BinaryParseFns = new()
            {
                { TokenType.LPAREN, ParseCallExpression },
                { TokenType.ASSIGN, ParseAssignment },
                { TokenType.INTERRO, ParseTernary },
                { TokenType.LSQB, ParseIndexingExpression },
                { TokenType.LAMBDA, ParseLambdaExpression },
                { TokenType.COMMA, ParseTuple },
            };

            PostfixParseFns = new()
            {
                { TokenType.INCREMENT, ParsePostfixExpression },
                { TokenType.DECREMENT, ParsePostfixExpression }
            };

            Current = this.tokens.Length > 0 ? this.tokens[0] : new Token(TokenType.EOF, "");
        }

        /// <summary>
        /// Parses the AST for the Program from the Tokens supplied by the Tokenizer of the Instance
        /// </summary>
        /// <returns>The AST of the Program</returns>
        public ASTRoot ParseProgram()
        {
            ASTRoot program = new() { Token = Current };
            List<IStatement> statements = [];
            program.Statements = [];
            while (Current.Type != TokenType.EOF)
            {
                IStatement? statement = ParseStatement();
                if (statement is not null)
                    statements.Add(statement);
                _ = ExpectStatementEnd();
                SkipEOL_S();
            }
            program.Statements = [.. statements];
            return program;
        }

        IStatement? ParseStatement()
        {
            IStatement? s = Current.Type switch
            {
                TokenType.RETURN => ParseReturnStatement(),
                TokenType.FN => ParseFunctionDecl(),
                TokenType.CLASS => ParseClassDecl(),
                TokenType.LCRB => ParseBlockStatement(),
                _ => ParseExpressionStatement()
            };
            return s;
        }

        ReturnStatement? ParseReturnStatement()
        {
            ReturnStatement statement = new() { Token = Current };
            Consume(TokenType.RETURN);
            var e = ParseExpression();
            if (e is null) return statement;
            statement.ReturnValue = e;
            return statement;
        }

        IExpression? ParseLetExpression()
        {
            LetExpression le = new() { Token = Current };
            Consume(TokenType.LET);
            // ParseFlags();
            IExpression? ident = ParseIdentifier();
            if (ident == null) return null;
            le.Name = (Identifier)ident;
            Consume(TokenType.ASSIGN);
            IExpression? right = ParseExpression();
            if (right == null) return null;
            le.Value = right;
            return le;
        }

        ExpressionStatement? ParseExpressionStatement()
        {
            ExpressionStatement statement = new() { Token = Current };
            IExpression? e = ParseExpression(OperatorPrecedence.LOWEST);
            if (e == null) return null;
            statement.Expression = e;
            return statement;
        }

        BlockStatement ParseBlockStatement()
        {
            BlockStatement block = new() { Token = Current };
            List<IStatement> statements = [];
            Consume(TokenType.LCRB);
            SkipEOL_S();
            while (Current.Type != TokenType.RCRB && Current.Type != TokenType.EOF)
            {
                IStatement? statement = ParseStatement();
                if (statement is not null)
                    statements.Add(statement);
                _ = ExpectStatementEnd();
                SkipEOL_S();
            }
            block.Statements = [.. statements];
            Consume(TokenType.RCRB);
            return block;
        }

        FunctionDecl? ParseFunctionDecl()
        {
            FunctionDecl fn = new() { Token = Current };
            Consume(TokenType.FN);
            // Parse Flags
            IExpression? ident = ParseIdentifier();
            if (ident == null) return null;
            fn.Name = (Identifier)ident;
            Consume(TokenType.LPAREN);
            if (Current.Type == TokenType.RPAREN)
            {
                fn.Args = [];
                Consume(TokenType.RPAREN);
            }
            else
            {
                Identifier[]? args = ParseDeclArgumentList();
                if (args == null) return null;
                fn.Args = args;
            }
            if (Current.Type == TokenType.FN_TYPE)
            {
                Consume();
                IExpression? fntype = ParseIdentifier();
                if (fntype == null) return null;
                fn.Type = (Identifier)fntype;
            }
            fn.Body = ParseBlockStatement();
            return fn;
        }

        IExpression? ParseLambdaExpression(IExpression? left)
        {
            LambdaExpression e = new() { Token = Current };
            if (left is null) return null;
            e.Arguments = left;
            Consume(TokenType.LAMBDA);
            var body = ParseStatement();
            if (body is null) return null;
            e.Body = body;
            return e;
        }

        IStatement? ParseClassDecl()
        {
            Consume(TokenType.CLASS);
            ClassDecl c = new() { Token = Current };
            var name = ParseIdentifier();
            if (name is null) return null;
            c.Name = (Identifier)name;
            c.Body = ParseBlockStatement();
            return c;
        }

        IExpression? ParseTuple(IExpression? fst)
        {
            TupleLiteral e = new() { Token = Current };
            if (fst is null) return null;
            List<IExpression> elems = [];
            elems.Add(fst);
            while (Current.Type == TokenType.COMMA)
            {
                Consume(TokenType.COMMA);
                var elem = ParseExpression(OperatorPrecedence.TUPLE);
                if (elem is null) return null;
                elems.Add(elem);
            }
            e.Elements = [.. elems];
            return e;
        }

        IExpression? ParseExpression(OperatorPrecedence precedence = OperatorPrecedence.LOWEST)
        {
            if (!UnaryParseFns.TryGetValue(Current.Type, out PrefixParseFn? prefix))
            {
                ParserError(ParserErrorType.UnexpectedToken, $"Unexpected Token {Current}");
                Consume();
                return null;
            }
            IExpression? left = prefix();
            while (Current.Type != TokenType.SEMICOLON && (precedence < CurPrecedence() || precedence == CurPrecedence() && RightAssociativeOperators.Contains(Current.Type)))
            {
                InfixParseFn? infix = ParseInfixExpression;
                if (AllowedOperators.Contains(Current.Type) || BinaryParseFns.TryGetValue(Current.Type, out infix))
                    left = infix(left);
                else
                {
                    if (PostfixParseFns.TryGetValue(Current.Type, out PostfixParseFn? postfix))
                        left = postfix(left);
                    return left;
                }
            }
            return left;
        }

        IExpression? ParseAbyss()
        {
            Abyss abyss = new() { Token = Current };
            Consume(TokenType.ABYSS);
            return abyss;
        }

        IExpression? ParseGroupedExpression()
        {
            var open = Current;
            Consume(TokenType.LPAREN);
            if (Current.Type == TokenType.RPAREN)
            {
                Consume(TokenType.RPAREN);
                return new TupleLiteral() { Token = open, Elements = [] };
            }
            var expr = ParseExpression(OperatorPrecedence.LOWEST);
            Consume(TokenType.RPAREN);
            return expr;
        }

        IExpression? ParseArray()
        {
            ArrayLiteral a = new() { Token = Current };
            List<IExpression> elems = [];
            Consume(TokenType.LSQB);
            while (Current.Type != TokenType.RSQB && Current.Type != TokenType.EOF)
            {
                var elem = ParseExpression(OperatorPrecedence.TUPLE);
                if (elem is null) return null;
                elems.Add(elem);
                if (Current.Type == TokenType.COMMA)
                    Consume(TokenType.COMMA);
            }
            Consume(TokenType.RSQB);
            a.Elements = [.. elems];
            return a;
        }

        IExpression? ParseIndexingExpression(IExpression? left)
        {
            IndexingExpression e = new() { Token = Current };
            if (left is null) return null;
            e.Indexed = left;
            Consume(TokenType.LSQB);
            var arg = ParseExpression();
            if (arg is null) return null;
            e.Index = arg;
            Consume(TokenType.RSQB);
            return e;
        }

        IExpression? ParseIdentifier() {
            Identifier i = new() { Token = Current, Value = Current.Literal };
            Consume(TokenType.IDENTIFIER);
            return i;
        }

        IExpression? ParseChar()
        {
            CharLiteral c = new() { Token = Current };
            Consume(TokenType.SINGLE_QUOTE);
            c.Value = Current.Literal;
            if (!Consume(TokenType.CHAR_CONTENT)) return null;
            Consume(TokenType.SINGLE_QUOTE);
            return c;
        }

        IExpression? ParseString()
        {
            StringLiteral s = new() { Token = Current };
            List<IExpression> components = [];
            Consume(TokenType.DOUBLE_QUOTE);
            while (Current.Type != TokenType.DOUBLE_QUOTE && Current.Type != TokenType.EOF)
            {
                if (Current.Type == TokenType.STRING_CONTENT)
                {
                    components.Add(new StringContent() { Value = Current.Literal, Token = Current });
                    Consume(TokenType.STRING_CONTENT);
                }
                else if (Current.Type == TokenType.STRING_INTERPOLATION_START)
                {
                    Interpolation i = new() { Token = Current };
                    Consume(TokenType.STRING_INTERPOLATION_START);
                    var e = ParseExpression();
                    if (e is null) continue;
                    i.Content = e;
                    components.Add(i);
                    Consume(TokenType.RCRB);
                }
                else return null;
            }
            Consume(TokenType.DOUBLE_QUOTE);
            s.Expressions = [.. components];
            return s;
        }

        IExpression? ParseNumeric()
        {
            string lit = Current.Literal;
            BigInteger? intValue = null;
            double? floatValue = null;
            if (lit.StartsWith("0x"))
                intValue = ParseInt(lit[2..], 16);
            else if (lit.StartsWith("0o"))
                intValue = ParseInt(lit[2..], 8);
            else if (lit.StartsWith("0b"))
                intValue = ParseInt(lit[2..], 2);
            else if (BigInteger.TryParse(lit, out BigInteger iVal))
                    intValue = iVal;
            else if (double.TryParse(lit, out double fVal))
                    floatValue = fVal;

            Token numberToken = Current;
            Consume(TokenType.NUMERIC);

            if (intValue.HasValue)
                return new IntegerLiteral() { Value = intValue.Value, Token = numberToken };
            else if (floatValue.HasValue)
                return new FloatLiteral() { Value = floatValue.Value, Token = numberToken };
            else return null;
        }

        /// <summary>
        /// Parses a string literal to a BigInteger in arbitrary base (until base 36)
        /// </summary>
        /// <param name="literal">The string literal to parse</param>
        /// <param name="iBase">the base</param>
        /// <returns></returns>
        public static BigInteger ParseInt(string literal, byte iBase = 10)
        {
            literal = literal.ToLower();
            return literal.Aggregate(new BigInteger(0), (b, c) => b * iBase + (c > '9' ? 10 + c - 'a' : c - '0'));
        }

        private IExpression? ParseCallExpression(IExpression? left)
        {
            CallExpression e = new() { Token = Current };
            Consume(TokenType.LPAREN);
            if (left == null) return null;
            if (Current.Type == TokenType.RPAREN)
            {
                e.Arguments = [];
                Consume(TokenType.RPAREN);
            }
            else
            {
                IExpression[]? args = ParseArgumentList();
                if (args == null) return null;
                e.Arguments = args;
            }
            e.Function = left;
            return e;
        }

        private IExpression? ParseTernary(IExpression? left)
        {
            TernaryExpression e = new() { Token = Current };
            Consume(TokenType.INTERRO);
            if (left is null) return null;
            e.Condition = left;
            IExpression? el = ParseExpression();
            if (el is null) return null;
            Consume(TokenType.COLON);
            IExpression? er = ParseExpression();
            if (er is null) return null;
            e.If = el;
            e.Else = er;
            return e;
        }

        private IExpression? ParseIfExpression()
        {
            IfExpression e = new() { Token = Current };
            Consume(TokenType.IF);
            var cond = ParseExpression();
            if (cond is null) return null;
            e.Condition = cond;
            SkipEOL();
            var cons = ParseStatement();
            if (cons is null) return null;
            e.Cons = cons;
            SkipEOL(TokenType.ELSE);
            if (Current.Type == TokenType.ELSE)
            {
                Consume(TokenType.ELSE);
                SkipEOL();
                e.Alt = ParseStatement();
            }
            return e;
        }

        private IExpression[]? ParseArgumentList()
        {
            List<IExpression> args = [];
            do
            {
                var e = ParseExpression(OperatorPrecedence.TUPLE);
                if (e is null) return null;
                args.Add(e);
            }
            while (Current.Type == TokenType.COMMA && Consume(TokenType.COMMA));
            Consume(TokenType.RPAREN);
            return [.. args];
        }

        private Identifier[]? ParseDeclArgumentList()
        {
            List<Identifier> args = [];
            do
            {
                var e = ParseIdentifier();
                if (e is null) return null;
                args.Add((Identifier)e);
            }
            while (Current.Type == TokenType.COMMA && Consume(TokenType.COMMA));
            Consume(TokenType.RPAREN);
            return [.. args];
        }

        IExpression? ParseBoolean()
        {
            BooleanLiteral lit = new() { Token = Current };
            if (!bool.TryParse(Current.Literal, out lit.Value))
                return null;
            Consume(); // true or false
            return lit;
        }
        IExpression? ParsePrefixExpression()
        {
            PrefixExpression prefix = new() { Token = Current, Operator = Current.Literal };
            Consume();
            IExpression? e = ParseExpression(OperatorPrecedence.PREFIX);
            if (e is not null) prefix.Right = e;
            else return null;
            return prefix;
        }
        IExpression? ParseInfixExpression(IExpression? left)
        {
            SkipEOL();
            InfixExpression infix = new() { Token = Current, Operator = Current.Literal };
            OperatorAssignment oa = new() { Token = Current, Operator = Current.Literal };
            OperatorPrecedence prec = CurPrecedence();
            Consume();
            if (left is null) return null;
            infix.Left = left;
            // General Operator Assignment: <pattern> <op> = <expr> is generally equivalent to <pattern> = <pattern> <op> <expr>
            // Doesn't work for some operators but that's for the compiler to decide
            if (Current.Type == TokenType.ASSIGN)
            {
                Consume(TokenType.ASSIGN);
                IExpression? offs = ParseExpression(OperatorPrecedence.EQUALS);
                if (offs is null) return null;
                if (left is not Identifier) return null;
                oa.Value = offs;
                oa.Name = (Identifier)left;
                return oa;
            }
            IExpression? e = ParseExpression(prec);
            if (e is null) return null;
            infix.Right = e;
            return infix;
        }

        IExpression? ParseAssignment(IExpression? left)
        {
            Assignment a = new() { Token = Current };
            Consume(TokenType.ASSIGN);
            if (left is null or not Identifier)
            {
                ParserError(ParserErrorType.Unspecified, "Assignment to Patterns is not yet implemented");
                return null;
            }
            a.Name = (Identifier)left;
            IExpression? value = ParseExpression();
            if (value is null) return null;
            a.Value = value;
            return a;
        }

        IExpression? ParsePostfixExpression(IExpression? left)
        {
            if (left is null) return null;
            PostfixExpression postfix = new() { Token = Current, Operator = Current.Literal, Left = left };
            Consume();
            return postfix;
        }

        Token NextToken()
        {
            if (readPosition + 1 < tokens.Length)
                Current = tokens[++readPosition];
            else Current = new Token(TokenType.EOF, "", Current.Line, Current.Column + Current.Literal.Length);
            return Current;
        }

        bool Consume(TokenType? type = null)
        {
            if (type != null && Current.Type != type)
            {
                ParserError(ParserErrorType.UnexpectedToken, $"Expected {type}");
                NextToken();
                return false;
            }
            NextToken();
            return true;
        }

        /// <summary>
        /// Expects either EOL, a semicolon or EOF as end of statement/expression
        /// </summary>
        /// <returns><c>true</c> if success, <c>false</c> if errored</returns>
        bool ExpectStatementEnd()
        {
            if (
                Current.Type == TokenType.EOL ||
                Current.Type == TokenType.SEMICOLON ||
                Current.Type == TokenType.EOF ||
                Current.Type == TokenType.RCRB
            ) return true;
            ParserError(ParserErrorType.UnexpectedToken, $"Expected EOL or SEMICOLON");
            return false;
        }

        /// <summary>
        /// Skips EOL Tokens. If <paramref name="after"/> is specified,
        /// it only skips the newlines if the first token after the newlines is of specified type.
        /// </summary>
        /// <param name="after">Optional Parameter specifying the first TokenType after newlines</param>
        void SkipEOL(TokenType after = TokenType.EOL)
        {
            if (after == TokenType.EOL || tokens[readPosition..].First(x => x.Type != TokenType.EOL).Type == after)
            {
                while (Current.Type == TokenType.EOL)
                    NextToken();
            }
        }

        void SkipEOL_S()
        {
            while (Current.Type == TokenType.SEMICOLON || Current.Type == TokenType.EOL)
                NextToken();
        }

        private Token Peek
        {
            get {
                if (readPosition + 1 < tokens.Length)
                    return tokens[readPosition + 1];
                return new Token(TokenType.EOF, "");
            }
        }

        static OperatorPrecedence GetPrecedence(Token t)
        {
            if (priorities.TryGetValue(t.Type, out OperatorPrecedence prec))
                return prec;
            return OperatorPrecedence.LOWEST;
        }

        OperatorPrecedence CurPrecedence()
        {
            return GetPrecedence(Current);
        }

        void ParserError(ParserErrorType type = ParserErrorType.Unspecified, string message = "")
        {
            ParserError pe = new(message, Current, type);
            errors.Add(pe);
        }

        private static readonly Dictionary<TokenType, OperatorPrecedence> priorities = new()
        {
            { TokenType.COMMA, OperatorPrecedence.TUPLE },
            { TokenType.ASSIGN, OperatorPrecedence.EQUALS },
            { TokenType.INTERRO, OperatorPrecedence.COMPARE },
            { TokenType.LT, OperatorPrecedence.COMPARE },
            { TokenType.GT, OperatorPrecedence.COMPARE },
            { TokenType.L_AND, OperatorPrecedence.COMPARE },
            { TokenType.L_OR, OperatorPrecedence.COMPARE },
            { TokenType.L_XOR, OperatorPrecedence.COMPARE },
            { TokenType.LTE, OperatorPrecedence.COMPARE },
            { TokenType.GTE, OperatorPrecedence.COMPARE },
            { TokenType.IN, OperatorPrecedence.COMPARE },
            { TokenType.ABYSS_COALESC, OperatorPrecedence.COMPARE },
            { TokenType.EQUAL, OperatorPrecedence.COMPARE },
            { TokenType.NOT_EQUAL, OperatorPrecedence.COMPARE },
            { TokenType.LEFT_SHIFT, OperatorPrecedence.BITWISE },
            { TokenType.RIGHT_SHIFT, OperatorPrecedence.BITWISE },
            { TokenType.B_AND, OperatorPrecedence.BITWISE },
            { TokenType.B_OR, OperatorPrecedence.BITWISE },
            { TokenType.B_XOR, OperatorPrecedence.BITWISE },
            { TokenType.PLUS, OperatorPrecedence.SUM },
            { TokenType.MINUS, OperatorPrecedence.SUM },
            { TokenType.CONCAT, OperatorPrecedence.SUM },
            { TokenType.RANGE, OperatorPrecedence.SUM },
            { TokenType.MUL, OperatorPrecedence.PRODUCT },
            { TokenType.DIV, OperatorPrecedence.PRODUCT },
            { TokenType.REMAINDER, OperatorPrecedence.PRODUCT },
            { TokenType.LAMBDA, OperatorPrecedence.PREFIX },
            { TokenType.INCREMENT, OperatorPrecedence.PREFIX },
            { TokenType.DECREMENT, OperatorPrecedence.PREFIX },
            { TokenType.B_NEG, OperatorPrecedence.PREFIX },
            { TokenType.EXCLAM, OperatorPrecedence.PREFIX },
            { TokenType.POWER, OperatorPrecedence.POWER },
            { TokenType.LPAREN, OperatorPrecedence.CALL },
            { TokenType.LSQB, OperatorPrecedence.CALL },
            { TokenType.CURRY, OperatorPrecedence.CALL },
            { TokenType.COERCE, OperatorPrecedence.ACCESS },
            { TokenType.ABYSS_ACCESSOR, OperatorPrecedence.ACCESS },
            { TokenType.ACCESSOR, OperatorPrecedence.ACCESS },
        };
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
        CALL,
        ACCESS
    }
}