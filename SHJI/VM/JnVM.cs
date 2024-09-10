using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SHJI.Bytecode;

namespace SHJI.VM
{
    public class JnVM
    {
        public JaneValue[] Constants { get; }
        public byte[] Code { get; }
        public JaneValue TempReg { get => lastPopped; } 
        public int Position { get => position - 1; }
        public JaneValue[] Globals { get; private set; }
        public Stack<JaneValue> Stack { get; private set; } = new();


        JaneValue lastPopped = JaneValue.Abyss;
        private int position = 0;

        public JnVM(JaneValue[] consts, byte[] bc, JaneValue[]? globals = null)
        {
            Constants = consts;
            Code = bc;
            if (globals is null) Globals = new JaneValue[2 << 16];
            else Globals = globals;
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
                    case OpCode.PUSH:
                        Stack.Push(Constants[operands[0]]);
                        break;
                    case OpCode.POP:
                        lastPopped = Stack.Pop();
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
                        {
                            JaneValue right = Stack.Pop();
                            JaneValue left = Stack.Pop();
                            Stack.Push(ResolveOperator(left, right, instr)(left, right));
                        }
                        break;
                    case OpCode.TRUE:
                        Stack.Push(JaneValue.True);
                        break;
                    case OpCode.FALSE:
                        Stack.Push(JaneValue.False);
                        break;
                    case OpCode.NOT:
                        if (Stack.Pop() is JaneBool b) Stack.Push(b.Value ? JaneValue.False : JaneValue.True);
                        else throw new NotImplementedException("NOT operator is only supported for booleans");
                        break;
                    case OpCode.NEGATE:
                        if (Stack.Pop() is JaneInt i) Stack.Push(new JaneInt(-i.Value));
                        else throw new NotImplementedException("NEGATION operator is only supported for numerics");
                        break;
                    case OpCode.JMP:
                        position = (int)operands[0];
                        break;
                    case OpCode.ABYSS:
                        Stack.Push(JaneValue.Abyss);
                        break;
                    case OpCode.JF:
                        var cond = Stack.Pop();
                        if (cond is JaneBool bcond)
                        {
                            if (!bcond.Value)
                                position = (int)operands[0];
                        }
                        else throw new NotImplementedException($"Type {cond.Type} is not implicitly convertible to bool");
                        break;
                    case OpCode.PUSHTMP:
                        Stack.Push(TempReg);
                        break;
                    case OpCode.SET:
                        Globals[(int)operands[0]] =  Stack.Pop();
                        break;
                    case OpCode.GET:
                        Stack.Push(Globals[(int)operands[0]]);
                        break;
                    case OpCode.DUP:
                        Stack.Push(Stack.Peek());
                        break;
                    default:
                        throw new NotImplementedException($"Unknown OpCode: {instr}");
                }
            }
        }

        delegate JaneValue BinaryOperator(JaneValue left, JaneValue right);
        BinaryOperator ResolveOperator(JaneValue left, JaneValue right, OpCode instruction)
        {
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
                _ => throw new NotImplementedException(),
            };
        }

        JaneValue Add(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
            {
                return new JaneInt(l.Value + r.Value);
            }
            throw new NotImplementedException($"Add instruction not implemented for types {left.GetType()} and {right.GetType()}");
        }
        JaneValue Sub(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
            {
                return new JaneInt(l.Value - r.Value);
            }
            throw new NotImplementedException($"Sub instruction not implemented for types {left.Type} and {right.Type}");
        }
        JaneValue Mul(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
            {
                return new JaneInt(l.Value * r.Value);
            }
            throw new NotImplementedException($"Mul instruction not implemented for types {left.Type} and {right.Type}");
        }
        JaneValue Div(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
            {
                return new JaneInt(l.Value / r.Value);
            }
            throw new NotImplementedException($"Div instruction not implemented for types {left.Type} and {right.Type}");
        }

        JaneValue Pow(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
            {
                return new JaneInt(BigInteger.Pow(l.Value, (int)r.Value));
            }
            throw new NotImplementedException($"Pow instruction not implemented for types {left.Type} and {right.Type}");
        }

        JaneValue Concat(JaneValue left, JaneValue right)
        {
            return new JaneString(ToStringRepresentation(left) + ToStringRepresentation(right));
        }

        JaneValue Equal(JaneValue left, JaneValue right)
        {
            return left.Value == right.Value ? JaneValue.True : JaneValue.False;
        }

        JaneValue Greater(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
            {
                return l.Value > r.Value ? JaneValue.True : JaneValue.False;
            }
            throw new NotImplementedException($"GT instruction not implemented for types {left.Type} and {right.Type}");
        }
        JaneValue Lesser(JaneValue left, JaneValue right)
        {
            if (left is JaneInt l && right is JaneInt r)
            {
                return l.Value < r.Value ? JaneValue.True : JaneValue.False;
            }
            throw new NotImplementedException($"LT instruction not implemented for types {left.Type} and {right.Type}");
        }

        string ToStringRepresentation(JaneValue v)
        {
            if (v is JaneString s) return s.Value;
            else return v.Inspect();
        }
    }
}
