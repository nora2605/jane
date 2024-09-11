using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.VM
{
    public class Frame
    {
        public JaneFunction Function;
        public int InstructionPointer;
        public Frame(JaneFunction fn) {
            Function = fn;
            InstructionPointer = -1;
        }

        public byte[] Instructions { get => Function.Value; }
    }
}
