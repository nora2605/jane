using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.Compiler
{
    internal class Scope
    {
        public List<byte> Instructions = [];
        public Scope() { }
    }
}
