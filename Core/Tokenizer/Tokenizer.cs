using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Unicode;
using System.Transactions;

namespace Jane.Core
{
    public class Tokenizer(string input) : IEnumerable<Token>, IEnumerator<Token>
    {
        private readonly string input = input;
        private int readPosition = 0;
        private char currentChar = string.IsNullOrEmpty(input) ? '\0' : input[0];

        private bool inChar;
        private bool inString;
        private readonly Stack<int> nestingIndices = new();

        private Token current;
        private int column = 0;
        private int line = 1;

        public int NestingIndex { get; private set; } = 0;

        private char Peek()
        {
            if (readPosition + 1 < input.Length)
                return input[readPosition + 1];
            return '\0';
        }
        private char NextChar()
        {
            column++;
            if (readPosition + 1 < input.Length)
            {
                if (currentChar == '\n') { line++; column = 1; }
                currentChar = input[++readPosition];
            }
            else currentChar = '\0';
            return currentChar;
        }

        /// <summary>
        /// Reads in an identifier until whitespace or EOF
        /// </summary>
        /// <returns>The identifier</returns>
        private string ReadIdent()
        {
            string ident = "";
            while (IsAlphaNumeric(currentChar) || IsOutsideASCII(currentChar))
            {
                ident += currentChar;
                NextChar();
            }
            return ident;
        }

        private string ReadNumeric()
        {
            string num = "";
            while (IsNumeric(currentChar))
            {
                if (currentChar == '.' && Peek() == '.')
                {
                    break;
                }
                num += currentChar;
                NextChar();
            }
            return num;
        }

        private string ReadCharContent()
        {
            string ch = "";
            while (currentChar != '\'' && currentChar != '\0')
            {
                if (currentChar == '\\')
                {
                    ch += Peek() switch
                    {
                        '\\' => '\\',
                        '0' => '\0',
                        'n' => '\n',
                        'a' => '\a',
                        'b' => '\b',
                        'e' => '\e',
                        'f' => '\f',
                        'r' => '\r',
                        't' => '\t',
                        'v' => '\v',
                        '"' => '"',
                        '\'' => '\'',
                        'u' => ReadUnicodeEscapeSequence(),
                        _ => throw new ArgumentException($"Unrecognized String Escape Sequence: \\{Peek()}")
                    };
                    NextChar();
                }
                else ch += currentChar;
                NextChar();
            }
            return ch;
        }

        /// <summary>
        /// Reads string content until an unescaped double quote or an interpolation
        /// </summary>
        /// <returns></returns>
        private string ReadStringContent()
        {
            string content = "";
            while (currentChar != '"' && (Peek() != '{' || currentChar != '$') && currentChar != '\0')
            {
                if (currentChar == '\\')
                {
                    content += Peek() switch
                    {
                        '\\' => '\\',
                        '0' => '\0',
                        'n' => '\n',
                        'a' => '\a',
                        'b' => '\b',
                        'e' => '\e',
                        'f' => '\f',
                        'r' => '\r',
                        't' => '\t',
                        'v' => '\v',
                        '"' => '"',
                        '\'' => '\'',
                        // only supported code point specification
                        'u' => ReadUnicodeEscapeSequence(),
                        _ => throw new ArgumentException($"Unrecognized String Escape Sequence: \\{Peek()}")
                    };
                    NextChar();
                } 
                else content += currentChar;
                NextChar();
            }
            return content;
        }

        private char ReadUnicodeEscapeSequence()
        {
            string seq = "";
            int codePoint = 0;
            for (int i = 0; i < 4; i++)
            {
                NextChar();
                seq += Peek();
            }
            codePoint = (int)Parser.ParseInt(seq, 16);
            return (char)codePoint;
        }

        /// <summary>
        /// Reads in whitespace and discards it
        /// </summary>
        private void SkipWhitespace()
        {
            while (IsWhitespace(currentChar) && currentChar != '\0')
                NextChar();
        }

        private void SkipLineComment()
        {
            while (currentChar != '\n' && currentChar != '\0')
                NextChar();
        }

        private void SkipBlockComment()
        {
            while ((currentChar != '*' || Peek() != '/') && currentChar != '\0')
                NextChar();
            NextChar();
        }

        private void NextToken()
        {
            if (currentChar == '\0')
            {
                current = T(TokenType.EOF, "");
                return;
            }
            if (!inChar && !inString) SkipWhitespace();


            if (currentChar == '"')
            {
                inString = !inString;
                current = T(TokenType.DOUBLE_QUOTE);
                NextChar();
                return;
            }
            if (currentChar == '\'')
            {
                inChar = !inChar;
                current = T(TokenType.SINGLE_QUOTE);
                NextChar();
                return;
            }
            if (inChar)
            {
                current = T(TokenType.CHAR_CONTENT, ReadCharContent());
                return;
            }
            if (inString && currentChar == '$' && Peek() == '{')
            {
                nestingIndices.Push(NestingIndex);
                NestingIndex++;
                inString = false;
                NextChar();
                current = T(TokenType.STRING_INTERPOLATION_START, "${");
                NextChar();
                return;
            }
            if (inString)
            {
                current = T(TokenType.STRING_CONTENT, ReadStringContent());
                return;
            }

            if (IsDigit(currentChar)) {
                current = T(TokenType.NUMERIC, ReadNumeric());
                return;
            }
            
            if (currentChar == '/' && Peek() == '/')
                SkipLineComment();
            if (currentChar == '/' && Peek() == '*')
                SkipBlockComment();

            TokenType tokenType;
            // Identifier
            if (IsAlpha(currentChar))
            {
                string ident = ReadIdent();
                if (Token.Keywords.TryGetValue(ident, out tokenType))
                    current = T(tokenType, ident);
                else current = T(TokenType.IDENTIFIER, ident);
                return;
            }
            // Prefix Free Tokens
            if (Token.PrefixFree.TryGetValue(currentChar, out tokenType))
            {
                current = T(tokenType);
                NestingIndex += current.Type switch
                {
                    TokenType.LPAREN
                    or TokenType.LSQB
                    or TokenType.LCRB => 1,
                    TokenType.RCRB
                    or TokenType.RSQB
                    or TokenType.RPAREN => -1,
                    _ => 0,
                };
                if (
                    current.Type == TokenType.RCRB &&
                    nestingIndices.Count != 0 &&
                    nestingIndices.Peek() == NestingIndex
                ) {
                    _ = nestingIndices.Pop();
                    inString = true;
                }
                NextChar();
                return;
            }
            // tokens are max 2 characters
            string guessToken = $"{currentChar}{Peek()}";
            if (Token.NotPrefixFree.TryGetValue(guessToken, out tokenType))
            {
                NextChar();
                current = T(tokenType, guessToken);
                NextChar();
                return;
            }
            else if (Token.NotPrefixFree.TryGetValue(currentChar.ToString(), out tokenType))
            {
                current = T(tokenType);
                NextChar();
                return;
            }
            
            // Otherwise
            current = T(TokenType.ILLEGAL);
            NextChar();
        }

        /// <summary>
        /// Create a token from the current Character
        /// </summary>
        /// <param name="type">The TokenType that should be created</param>
        /// <returns>A Token with current Line and Column</returns>
        private Token T(TokenType type) => new(type, currentChar.ToString(), line, column);
        /// <summary>
        /// Create a token from a given literal
        /// </summary>
        /// <param name="type">The TokenType that should be created</param>
        /// <param name="literal">The literal to assign</param>
        /// <returns>A Token with current Line and Column</returns>
        private Token T(TokenType type, string literal) => new(type, literal, line, column - literal.Length);

        public bool Finished
        {
            get => NestingIndex == 0;
        }

        public Token Current { get => current; }
        object IEnumerator.Current => current;
        public bool MoveNext()
        {
            NextToken();
            return current.Type != TokenType.EOF;
        }

        public void Reset()
        {
            readPosition = 0;
            currentChar = string.IsNullOrEmpty(input) ? '\0' : input[0];

            current = default;
            NestingIndex = 0;
            column = 0;
            line = 1;

            inString = false;
            inChar = false;
        }

        public IEnumerator<Token> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        public void Dispose() => GC.SuppressFinalize(this);

        public static bool IsAlphaNumeric(char c)
        {
            return (c >= '0' && c <= '9') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                c == '_';
        }
        public static bool IsAlpha(char c)
        {
            return (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                c == '_';
        }
        public static bool IsDigit(char c) => c >= '0' && c <= '9';
        public static bool IsNumeric(char c) =>
            (c >= '0' && c <= '9') ||
            c == '.' ||
            c == 'x' || // 0x00
            c == 'o' || // 0o00
            (c >= 'a' && c <= 'f') || // 0b00, 0xabcdef, 1f
            (c >= 'A' && c <= 'F') || // 0xABCDEF
            c == '_';   // 1_000_000

        public static bool IsOutsideASCII(char c) => c > 0x7f;

        public static bool IsWhitespace(char c) => c == ' ' || c == '\t' || c == '\r';
    }
}
