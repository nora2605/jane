using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jane.Core;

namespace SHJI.Compiler
{
    public class CompilerError(string Message, Token ErroneousToken, CompilerErrorType Type = CompilerErrorType.Unspecified)
    {
        public string Message { get; } = Message;
        public CompilerErrorType Type { get; } = Type;

        public int Line { get; } = ErroneousToken.Line;
        public int Column { get; } = ErroneousToken.Column;

        public override string ToString()
        {
            return $"Compiler Error: {Type} Error on Line {Line}, Column {Column}: {Message}";
        }
    }

    public enum CompilerErrorType
    {
        Unspecified
    }
}
