using Jane.AST;
using SHJI.VM;

namespace SHJI.VMCompiler
{
    internal class Compiler
    {
        private readonly List<byte> instructions = [];
        private readonly List<IJaneObject> constants = [];

        public void Compile(IASTNode node)
        {
            switch (node)
            {
                case ASTRoot a:
                    foreach (IStatement s in a.statements)
                    {
                        Compile(s);
                    }
                    break;
                case ExpressionStatement e:
                    Compile(e.Expression);
                    break;
                case InfixExpression i:
                    Compile(i.Left);
                    Compile(i.Right);

                    switch (i.Operator)
                    {
                        case "+":
                            Emit(OpCode.Add);
                            break;
                        default:
                            throw new CompilerException("Unknown Infix Operator");
                    }
                    break;
                case IntegerLiteral i:
                    IJaneObject integer = new JaneInt() { Value = i.Value };
                    Emit(OpCode.Constant, AddConstant(integer));
                    break;
                default:
                    throw new CompilerException("Not implemented.");
                    break;
            }
        }

        int AddConstant(IJaneObject obj)
        {
            constants.Add(obj);
            return constants.Count - 1;
        }

        int Emit(OpCode op, params int[] operands)
        {
            byte[] instr = ByteCode.Make(op, operands);
            return AddInstruction(instr);
        }

        int AddInstruction(byte[] instr)
        {
            int pos = instructions.Count;
            instructions.AddRange(instr);
            return pos;
        }

        public ByteCode GetByteCode()
        {
            return new ByteCode([.. instructions], [.. constants]);
        }
    }
}
