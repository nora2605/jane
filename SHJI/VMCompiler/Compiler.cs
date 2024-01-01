using Jane.AST;
using SHJI.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.VMCompiler
{
    internal class Compiler
    {
        private byte[] instructions = [];
        private IJaneObject[] constants = [];

        public void Compile(IASTNode node)
        {
            switch (node)
            {
                case ASTRoot:

                    break;
            }
        }

        public ByteCode ByteCode()
        {
            return new ByteCode(instructions, constants);
        }
    }
}
