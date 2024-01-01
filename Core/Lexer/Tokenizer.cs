using System.Collections;

namespace Jane.Lexer
{
    public class Tokenizer : IEnumerable<Token>, IEnumerator<Token>
    {
        private readonly string input;
        private int position;
        private int readPosition;
        private char ch;

        private enum State
        {
            None = 0,
            ReadingString = 1,
            Interpolated = 2,
            Verbatim = 4,
            ReadingChar = 8,
            InsideInterpolation = 16
        }

        private bool ReadingString { get => state.HasFlag(State.ReadingString); set => state = value ? state | State.ReadingString : state & ~State.ReadingString; }
        private bool Interpolated { get => state.HasFlag(State.Interpolated); set => state = value ? state | State.Interpolated : state & ~State.Interpolated; }
        private bool Verbatim { get => state.HasFlag(State.Verbatim); set => state = value ? state | State.Verbatim : state & ~State.Verbatim; }
        private bool ReadingChar { get => state.HasFlag(State.ReadingChar); set => state = value ? state | State.ReadingChar : state & ~State.ReadingChar; }
        private bool InsideInterpolation { get => state.HasFlag(State.InsideInterpolation); set => state = value ? state | State.InsideInterpolation : state & ~State.InsideInterpolation; }

        private State state;
        public int Depth;

        private bool evaluatingNestedLexer = false;
        Tokenizer? nestedLexer = null;

        private int line = 1;
        private int column = 0;

        private Token current;

        public Tokenizer(string input)
        {
            this.input = input;
            state = State.None;
            Depth = 0;
            ReadChar();
        }
        protected void ReadChar()
        {
            if (readPosition >= input.Length)
            {
                ch = '\0';
            }
            else
            {
                ch = input[readPosition];
            }
            column++;
            position = readPosition++;
        }

        public Token NextToken()
        {
            if (!ReadingString) SkipWhitespace();
            Token tok = new();
            if (evaluatingNestedLexer)
            {
                Token t = nestedLexer?.NextToken() ?? tok;
                if (nestedLexer?.Depth < 0 || nestedLexer?.Current.Type == TokenType.EOF)
                {
                    evaluatingNestedLexer = false;
                    readPosition += nestedLexer.position - 2;
                    ReadChar();
                    nestedLexer.Dispose();
                    nestedLexer = null;
                }
                else
                {
                    current = t;
                    return t;
                }
            }
            // Reading a string, char or interpolation
            if (ch == '\0')
            {
                state = State.None;
                tok = NewTokenLC(TokenType.EOF, "");
                current = tok;
                return tok;
            }
            if (ReadingChar)
            {
                if (ch == '\'')
                {
                    ReadingChar = false;
                    tok = NewTokenLC(TokenType.SINGLEQUOTE, ch);
                    current = tok;
                    return tok;
                }
                else if (ch == '\\')
                {
                    ReadChar();
                    if (ch == 'u' || ch == 'x')
                    {
                        tok = NewTokenLC(TokenType.CHAR_CONTENT, "\\" + ReadWhile((ch) => ch != '\''));
                        ReadingChar = false;
                        current = tok;
                        return tok;
                    }
                    else tok = NewTokenLC(TokenType.CHAR_CONTENT, "\\" + ch);
                }
                else
                {
                    tok = NewTokenLC(TokenType.CHAR_CONTENT, ch);
                }
            }
            else if (ReadingString && !InsideInterpolation)
            {
                if (ch == '"' || (!Verbatim && ch == '\n'))
                {
                    ReadingString = false;
                    tok = ch == '"' ? NewTokenLC(TokenType.DOUBLEQUOTE, ch) : NewTokenLC(TokenType.EOL, ch);
                }
                else if (Interpolated)
                {
                    string result = "";
                    while (ch != '"' && ch != '\0' && (Verbatim || ch != '\n'))
                    {
                        if (ch == '\\')
                        {
                            ReadChar();
                            result += "\\" + ch;
                        }
                        else if (ch == '$')
                        {
                            if (PeekChar() == '{')
                            {
                                InsideInterpolation = true;
                                tok = NewTokenLC(TokenType.STRING_CONTENT, result);
                                current = tok;
                                return tok;
                            }
                        }
                        else result += ch;
                        ReadChar();
                    }
                    tok = NewTokenLC(TokenType.STRING_CONTENT, result);
                    current = tok;
                    return tok;
                }
                else
                {
                    string sc = "";
                    sc += ReadWhile((ch) => ch != '"' && (Verbatim || ch != '\n') && ch != '\0');
                    while (sc.EndsWith("\\") && !sc.EndsWith("\\\\") && ch == '"')
                    {
                        sc += '\"';
                        ReadChar();
                        sc += ReadWhile((ch) => ch != '"' && (Verbatim || ch != '\n') && ch != '\0');
                    }
                    tok = NewTokenLC(TokenType.STRING_CONTENT, sc);
                    current = tok;
                    return tok;
                }
            }
            else
            {
                switch (ch)
                {
                    case '\n':
                        line++; column = 0;
                        tok = NewTokenLC(TokenType.EOL, "\\n");
                        break;
                    case '~':
                        tok = NewTokenLC(TokenType.CONCAT, ch);
                        break;
                    case '$':
                        if (PeekChar() == '{')
                        {
                            if (InsideInterpolation)
                            {
                                ReadChar();
                                evaluatingNestedLexer = true;
                                nestedLexer = new(input[readPosition..]);
                                Depth++;
                                tok = NewTokenLC(TokenType.LINTERPOLATE, "${");
                            }
                            else tok = NewTokenLC(TokenType.ILLEGAL, "${");
                        }
                        else tok = NewTokenLC(TokenType.ILLEGAL, "$");
                        break;
                    case '=':
                        if (PeekChar() == '=')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.EQ, "==");
                        }
                        else if (PeekChar() == '>')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.DOUBLEARROW, "=>");
                        }
                        else
                        {
                            tok = NewTokenLC(TokenType.ASSIGN, ch);
                        }
                        break;
                    case '+':
                        if (PeekChar() == '+')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.INCREMENT, "++");
                        }
                        else tok = NewTokenLC(TokenType.PLUS, ch);
                        break;
                    case ':':
                        if (PeekChar() == ':')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.COERCER, "::");
                        }
                        else tok = NewTokenLC(TokenType.COLON, ch); break;
                    case '-':
                        if (PeekChar() == '-')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.DECREMENT, "--");
                        }
                        else if (PeekChar() == '>')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.SINGLEARROW, "->");
                        }
                        else tok = NewTokenLC(TokenType.MINUS, ch);
                        break;
                    case '!':
                        if (PeekChar() == '=')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.NOT_EQ, "!=");
                        }
                        else tok = NewTokenLC(TokenType.BANG, ch);
                        break;
                    case '/':
                        tok = NewTokenLC(TokenType.SLASH, ch);
                        break;
                    case '^':
                        tok = NewTokenLC(TokenType.HAT, ch);
                        break;
                    case '*':
                        tok = NewTokenLC(TokenType.ASTERISK, ch);
                        break;
                    case '<':
                        if (PeekChar() == '=')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.LTE, "<=");
                        }
                        else tok = NewTokenLC(TokenType.LT, ch);
                        break;
                    case '>':
                        if (PeekChar() == '=')
                        {
                            ReadChar();
                            tok = NewTokenLC(TokenType.GTE, ">=");
                        }
                        else tok = NewTokenLC(TokenType.GT, ch);
                        break;
                    case ';':
                        tok = NewTokenLC(TokenType.SEMICOLON, ch);
                        break;
                    case ',':
                        tok = NewTokenLC(TokenType.COMMA, ch);
                        break;
                    case '{':
                        Depth++;
                        tok = NewTokenLC(TokenType.LBRACE, ch);
                        break;
                    case '}':
                        InsideInterpolation = false;
                        Depth--;
                        tok = NewTokenLC(TokenType.RBRACE, ch);
                        break;
                    case '(':
                        tok = NewTokenLC(TokenType.LPAREN, ch);
                        break;
                    case ')':
                        tok = NewTokenLC(TokenType.RPAREN, ch);
                        break;
                    case '[':
                        tok = NewTokenLC(TokenType.LSQB, ch);
                        break;
                    case ']':
                        tok = NewTokenLC(TokenType.RSQB, ch);
                        break;
                    case '"':
                        ReadingString = true;
                        Interpolated = true;
                        InsideInterpolation = false;
                        tok = NewTokenLC(TokenType.DOUBLEQUOTE, ch);
                        break;
                    case '\'':
                        ReadingChar = true;
                        InsideInterpolation = false;
                        tok = NewTokenLC(TokenType.SINGLEQUOTE, ch);
                        break;
                    case '@':
                        if (PeekChar() == '"')
                        {
                            ReadChar();
                            ReadingString = true;
                            InsideInterpolation = false;
                            Verbatim = true;
                            tok = NewTokenLC(TokenType.VERBATIM_STRING, "@\"");
                        }
                        else if (PeekChar() == '$')
                        {
                            ReadChar();
                            if (PeekChar() == '"')
                            {
                                ReadChar();
                                ReadingString = Verbatim = Interpolated = true;
                                InsideInterpolation = false;
                                tok = NewTokenLC(TokenType.VERBATIM_INTERPOLATED_STRING, "@$\"");
                            }
                            else
                            {
                                tok = NewTokenLC(TokenType.ILLEGAL, "@$");
                            }
                        }
                        else
                        {
                            tok = NewTokenLC(TokenType.AT, '@');
                        }
                        break;
                    case '.':
                        if (PeekChar() == '.')
                        {
                            tok = NewTokenLC(TokenType.RANGE, "..");
                        }
                        else if (IsDigit(PeekChar()))
                        {
                            ReadChar();
                            string flt = '.' + ReadWhile(IsDigit);
                            tok = NewTokenLC(TokenType.FLOAT, flt);
                        }
                        else
                        {
                            tok = NewTokenLC(TokenType.ACCESSOR, '.');
                        }
                        break;
                    default:
                        if (IsLetter(ch))
                        {
                            string lit = ReadWhile((ch) => IsLetter(ch) || IsDigit(ch) || ch == '_' || ch == '-');
                            if (ch == '"' && (lit == "raw" || lit == "r")) { ReadChar(); lit += "\""; }
                            tok = new Token(TokenLookup.LookupIdent(lit), lit, line, column - lit.Length);
                            current = tok;
                            return tok;
                        }
                        else if (IsDigit(ch))
                        {
                            string lit = ReadWhile(IsDigit);
                            TokenType tt = TokenType.INT;
                            if (ch == '.')
                            {
                                ReadChar();
                                tt = TokenType.FLOAT;
                                lit += '.' + ReadWhile(IsDigit);
                            }
                            tok = new Token(tt, lit, line, column - lit.Length);
                            current = tok;
                            return tok;
                        }
                        else
                        {
                            tok = NewTokenLC(TokenType.ILLEGAL, ch);
                        }
                        break;
                }
            }
            ReadChar();
            current = tok;
            return tok;
        }

        private void SkipWhitespace()
        {
            while (ch == ' ' || ch == '\t' || ch == '\r')
            {
                ReadChar();
            }
        }

        private char PeekChar()
        {
            if (readPosition >= input.Length)
            {
                return '\0';
            }
            else
            {
                return input[readPosition];
            }
        }

        private string ReadWhile(Func<char, bool> Condition)
        {
            int initPosition = position;
            while (Condition(ch))
            {
                ReadChar();
            }
            return input[initPosition..position];
        }

        private static bool IsLetter(char ch) { return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch == '_'; }
        private static bool IsDigit(char ch) { return ch >= '0' && ch <= '9'; }

        private Token NewTokenLC(TokenType Type, char ident)
        {
            return new Token(Type, ident.ToString(), line, column - 1);
        }
        private Token NewTokenLC(TokenType Type, string ident)
        {
            return new Token(Type, ident, line, column - ident.Length);
        }

        public Token Current { get => current; }
        object IEnumerator.Current => current;
        public bool MoveNext()
        {
            NextToken();
            if (current.Type != TokenType.EOF) return true;
            return false;
        }

        public void Reset()
        {
            state = State.None;
            position = 0;
            current = new Token();
            readPosition = 0;
            ch = '\0';
            line = 1;
            column = 0;
            Depth = 0;
            evaluatingNestedLexer = false;
            ReadChar();
        }

        public IEnumerator<Token> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
