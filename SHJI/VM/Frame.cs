using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.VM
{
    public class Frame(JaneFunction fn, int basePointer)
    {
        public JaneFunction Function = fn;
        public int InstructionPointer = -1;
        public int BasePointer = basePointer;
        public byte[] Instructions { get => Function.Value; }
    }
}
