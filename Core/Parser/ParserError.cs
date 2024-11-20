using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jane.Core
{
    public class ParserError(string message, Token ErroneousToken, ParserErrorType Type = ParserErrorType.Unspecified) : Exception(message)
    {
        public ParserErrorType Type { get; } = Type;

        public int Line { get; } = ErroneousToken.Line;
        public int Column { get; } = ErroneousToken.Column;

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
