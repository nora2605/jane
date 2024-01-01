using Jane.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jane.Parser
{
    public class ParserError
    {
        public string Message { get; }
        public ParserErrorType Type { get; }

        public int Line { get; }
        public int Column { get; }
        public ParserError(string Message, Token ErroneousToken, ParserErrorType Type=ParserErrorType.Unspecified)
        { 
            this.Message = Message;
            this.Type = Type;
            Line = ErroneousToken.Line;
            Column = ErroneousToken.Column;
        }
        public override string ToString()
        {
            return $"Parser Error: {Type} Error on Line {Line}, Column {Column}: {Message}";
        }
    }

    public enum ParserErrorType
    {
        Unspecified,
        UnexpectedToken,
        Ambiguous
    }
}
