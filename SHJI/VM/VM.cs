using System;
using System.Linq;
using System.Security.AccessControl;

namespace SHJI.VM
{
    internal class VM(ByteCode bc)
    {
        const int STACK_SIZE = 2048;

        readonly IJaneObject[] constants = bc.Constants;
        readonly byte[] instructions = bc.Instructions;

        readonly IJaneObject[] stack = new IJaneObject[STACK_SIZE];
        int sp = 0; // how 64 bit :(

        public IJaneObject StackTop() => sp > 0 ? stack[sp - 1] : IJaneObject.JANE_ABYSS;

        public void Run()
        {
            for (int ip = 0; ip < instructions.Length; ip++)
            {
                OpCode co = (OpCode)instructions[ip];
                switch (co)
                {
                    case OpCode.Halt:
                        return;
                    case OpCode.Constant:
                        int i = ByteCode.ReadLittleEndian(instructions[(ip+1)..], 4); // Read int32
                        ip += 4;

                        Push(constants[i]);
                        break;
                    case OpCode.Add: // Seems like in the future this will have to be Addition on 64 bit to support 64 bit integers at all
                        // Smaller integers are treated the same but they would be larger on the stack, which is not ideal
                        IJaneObject right = Pop();
                        IJaneObject left = Pop();
                        int l = ((JaneInt)left).Value;
                        int r = ((JaneInt)right).Value;

                        int result = l + r;

                        Push(new JaneInt() { Value = result });
                        break;
                }
            }
        }

        void Push(IJaneObject obj)
        {
            if (sp >= STACK_SIZE) throw new StackOverflowException();

            stack[sp] = obj;
            sp++;
        }

        IJaneObject Pop()
        {
            return stack[--sp];
        }
    }
}
