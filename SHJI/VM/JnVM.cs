using System.Numerics;

using SHJI.Bytecode;
using SHJI.Util;

namespace SHJI.VM
{
    public class JnVM
    {
        public JaneValue[] Constants { get; }
        public JaneValue TempReg { get => lastPopped; }
        public byte[] Code { get => CallStack.Peek().Function.Value; }
        public int Position { get => position - 1; }
        public JaneValue[] Store { get; private set; }
        public RAStack<JaneValue> ValueStack { get; private set; } = new();
        public Stack<Frame> CallStack { get; private set; } = new();

        JaneValue lastPopped = JaneValue.Abyss;
        private int position = 0;

        public JnVM(JaneValue[] consts, byte[] bc, JaneValue[]? globals = null)
        {
            Constants = consts;
            globals ??= new JaneValue[2 << 16];
            Store = globals;
            JaneFunction main = new(bc, 0, 0);
            CallStack.Push(new Frame(main, 0));
        }

        public void Run()
        {
            while (position < Code.Length)
            {
                var (instr, operands) = JnBytecode.ReadInstruction(Code, ref position);
                switch (instr)
                {
                    case OpCode.HALT:
                        position = Code.Length;
                        break;
                    case OpCode.LOAD:
                        ValueStack.Push(Constants[operands[0]]);
                        break;
                    case OpCode.POP:
                        lastPopped = ValueStack.Pop();
                        break;
                    case OpCode.ADD:
                    case OpCode.SUB:
                    case OpCode.MUL:
                    case OpCode.DIV:
                    case OpCode.CONCAT:
                    case OpCode.EQUAL:
                    case OpCode.POW:
                    case OpCode.LT:
                    case OpCode.GT:
                    case OpCode.INDEX:
                        {
                            JaneValue right = ValueStack.Pop();
                            JaneValue left = ValueStack.Pop();
                            if ((left is JaneInt && right is JaneFloat) || (left is JaneFloat && right is JaneInt))
                            {
                                left = new JaneFloat(double.Parse(left.Inspect()));
                                right = new JaneFloat(double.Parse(right.Inspect()));
                            }
                            ValueStack.Push(ResolveOperator(left, right, instr)(left, right));
                        }
                        break;
                    case OpCode.TRUE:
                        ValueStack.Push(JaneValue.True);
                        break;
                    case OpCode.FALSE:
                        ValueStack.Push(JaneValue.False);
                        break;
                    case OpCode.NOT:
                        if (ValueStack.Pop() is JaneBool b) ValueStack.Push(b.Value ? JaneValue.False : JaneValue.True);
                        else throw new NotImplementedException("NOT operator is only supported for booleans");
                        break;
                    case OpCode.NEGATE:
                        if (ValueStack.Pop() is JaneInt i) ValueStack.Push(new JaneInt(-i.Value));
                        else throw new NotImplementedException("NEGATION operator is only supported for numerics");
                        break;
                    case OpCode.JMP:
                        position = (int)operands[0];
                        break;
                    case OpCode.ABYSS:
                        ValueStack.Push(JaneValue.Abyss);
                        break;
                    case OpCode.JF:
                        var cond = ValueStack.Pop();
                        if (cond is JaneBool bcond)
                        {
                            if (!bcond.Value)
                                position = (int)operands[0];
                        }
                        else throw new NotImplementedException($"Type {cond.Type} is not implicitly convertible to bool");
                        break;
                    case OpCode.PUSHTMP:
                        ValueStack.Push(TempReg);
                        break;
                    case OpCode.SET_GLOBAL:
                        Store[(int)operands[0]] = ValueStack.Pop();
                        break;
                    case OpCode.GET_GLOBAL:
                        ValueStack.Push(Store[(int)operands[0]]);
                        break;
                    case OpCode.SET:
                        ValueStack[CallStack.Peek().BasePointer + (int)operands[0]] = ValueStack.Pop();
                        break;
                    case OpCode.GET:
                        ValueStack.Push(ValueStack[CallStack.Peek().BasePointer + (int)operands[0]]);
                        break;
                    case OpCode.DUP:
                        ValueStack.Push(ValueStack.Peek());
                        break;
                    case OpCode.CONSTR_ARR:
                        var len = (int)operands[0];
                        List<JaneValue> arr_elems = [];
                        for (int idx = 0; idx < len; idx++)
                            arr_elems.Add(ValueStack.Pop());
                        ValueStack.Push(new JaneArray([..arr_elems]));
                        break;
                    case OpCode.RET:
                        var retval = ValueStack.Pop();
                        var cs = CallStack.Pop();
                        position = cs.InstructionPointer;
                        ValueStack.StackPointer = cs.BasePointer;
                        ValueStack.Push(retval);
                        break;
                    case OpCode.CALL:
                        var f = ValueStack.Pop();
                        if (f is not JaneFunction)
                            throw new NotImplementedException($"Object of type {f.Type} is not callable");
                        JaneFunction jff = (JaneFunction)f;
                        if (jff.NumParams != (int)operands[0])
                            throw new ArgumentException($"Function expected {jff.NumParams} Arguments but got {operands[0]}");
                        Frame frame = new(
                            jff,
                            ValueStack.StackPointer - (int)operands[0]
                        ) { InstructionPointer = position };
                        CallStack.Push(frame);
                        ValueStack.StackPointer = frame.BasePointer + ((JaneFunction)f).NumLocals;
                        position = 0;
                        break;
                    default:
                        throw new NotImplementedException($"Unknown OpCode: {instr}");
                }
            }
        }

        delegate JaneValue BinaryOperator(JaneValue left, JaneValue right);
        BinaryOperator ResolveOperator(JaneValue left, JaneValue right, OpCode instruction)
        {
            _ = left;
            _ = right;
            return instruction switch
            {
                OpCode.ADD => Add,
                OpCode.SUB => Sub,
                OpCode.MUL => Mul,
                OpCode.DIV => Div,
                OpCode.CONCAT => Concat,
                OpCode.EQUAL => Equal,
                OpCode.POW => Pow,
                OpCode.GT => Greater,
                OpCode.LT => Lesser,
                OpCode.INDEX => Index,
                _ => throw new NotImplementedException(),
            };
        }

        JaneValue Add(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
                return new JaneInt(l.Value + r.Value);
            else if (left is JaneFloat fl && right is JaneFloat fr)
                return new JaneFloat(fl.Value + fr.Value);
            throw new NotImplementedException($"Add instruction not implemented for types {left.GetType()} and {right.GetType()}");
        }
        JaneValue Sub(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
                return new JaneInt(l.Value - r.Value);
            else if (left is JaneFloat fl && right is JaneFloat fr)
                return new JaneFloat(fl.Value - fr.Value);
            throw new NotImplementedException($"Sub instruction not implemented for types {left.Type} and {right.Type}");
        }
        JaneValue Mul(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
                return new JaneInt(l.Value * r.Value);
            else if (left is JaneFloat fl && right is JaneFloat fr)
                return new JaneFloat(fl.Value * fr.Value);
            throw new NotImplementedException($"Mul instruction not implemented for types {left.Type} and {right.Type}");
        }
        JaneValue Div(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
                return new JaneInt(l.Value / r.Value);
            else if (left is JaneFloat fl && right is JaneFloat fr)
                return new JaneFloat(fl.Value / fr.Value);
            throw new NotImplementedException($"Div instruction not implemented for types {left.Type} and {right.Type}");
        }

        JaneValue Pow(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
                return new JaneInt(BigInteger.Pow(l.Value, (int)r.Value));
            else if (left is JaneFloat fl && right is JaneFloat fr)
                return new JaneFloat(Math.Pow(fl.Value, fr.Value));
            throw new NotImplementedException($"Pow instruction not implemented for types {left.Type} and {right.Type}");
        }

        JaneValue Concat(JaneValue left, JaneValue right)
        {
            return new JaneString(ToStringRepresentation(left) + ToStringRepresentation(right));
        }

        JaneValue Equal(JaneValue left, JaneValue right)
        {
            return left.Inspect() == right.Inspect() ? JaneValue.True : JaneValue.False;
        }

        JaneValue Greater(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
                return l.Value > r.Value ? JaneValue.True : JaneValue.False;
            else if (left is JaneFloat fl && right is JaneFloat fr)
                return fl.Value > fr.Value ? JaneValue.True : JaneValue.False;
            throw new NotImplementedException($"GT instruction not implemented for types {left.Type} and {right.Type}");
        }
        JaneValue Lesser(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
                return l.Value < r.Value ? JaneValue.True : JaneValue.False;
            else if (left is JaneFloat fl && right is JaneFloat fr)
                return fl.Value < fr.Value ? JaneValue.True : JaneValue.False;
            throw new NotImplementedException($"LT instruction not implemented for types {left.Type} and {right.Type}");
        }

        JaneValue Index(JaneValue left, JaneValue right)
        {
            if (left is JaneArray arr && right is JaneInt i)
                return arr.Value[(int)i.Value];
            else if (left is JaneString str && right is JaneInt col)
                return new JaneChar(str.Value[(int)col.Value]);
            throw new NotImplementedException($"INDEX instruction not implemented for types {left.Type} and {right.Type}");
        }

        private static string ToStringRepresentation(JaneValue v)
        {
            if (v is JaneString s) return s.Value;
            else if (v is JaneChar c) return c.Value.ToString();
            else return v.Inspect();
        }
    }
}
