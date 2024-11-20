using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jane.Core;

namespace SHJI.Compiler
{
    public class CompilerError(string message, Token ErroneousToken, CompilerErrorType Type = CompilerErrorType.Unspecified) : Exception(message)
    {
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
