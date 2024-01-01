using Jane.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.Interpreter
{
    internal class RuntimeError : Exception
    {
        public Token Token { get; set; }

        public RuntimeError() { }
        public RuntimeError(string message) : base(message) { }
        public RuntimeError(string message, Token token) : base(message)
        {
            this.Token = token;
        }
    }
}
