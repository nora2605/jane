using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.Bytecode
{
    public static class JnBytecode
    {
        public static ImmutableDictionary<OpCode, OpDefinition> Definitions { get; } = new Dictionary<OpCode, OpDefinition>()
        {
            { OpCode.HALT, new OpDefinition("Halt", []) },
            { OpCode.PUSH, new OpDefinition("Push Constant", [2]) },
            { OpCode.GET, new OpDefinition("Get Variable", [2]) },
            { OpCode.SET, new OpDefinition("Set Variable", [2]) },
            { OpCode.DUP, new OpDefinition("Duplicate", []) },
            { OpCode.POP, new OpDefinition("Pop", []) },
            { OpCode.ADD, new OpDefinition("Add", []) },
            { OpCode.SUB, new OpDefinition("Sub", []) },
            { OpCode.MUL, new OpDefinition("Mul", []) },
            { OpCode.DIV, new OpDefinition("Div", []) },
            { OpCode.CONCAT, new OpDefinition("Concat", []) },
            { OpCode.TRUE, new OpDefinition("True", []) },
            { OpCode.FALSE, new OpDefinition("False", []) },
            { OpCode.NOT, new OpDefinition("Not", []) },
            { OpCode.NEGATE, new OpDefinition("Negate", []) },
            { OpCode.JF, new OpDefinition("Jump if false", [8]) },
            { OpCode.JMP, new OpDefinition("Jump", [8]) },
            { OpCode.ABYSS, new OpDefinition("Abyss", []) },
            { OpCode.EQUAL, new OpDefinition("Equal", []) },
            { OpCode.PUSHTMP, new OpDefinition("Push TmpReg", []) }
        }.ToImmutableDictionary();

        public static byte[] Make(OpCode opCode, params ulong[] operands)
        {
            if (!Definitions.TryGetValue(opCode, out var definition))
                definition = new OpDefinition(opCode.ToString(), []);
            int instrLen = 1 + definition.OperandWidths.Sum();

            byte[] instruction = new byte[instrLen];
            instruction[0] = (byte)opCode;

            int offset = 1;
            for (int i = 0; i < operands.Length; i++)
            {
                int width = definition.OperandWidths[i];
                WriteLE(instruction, offset, operands[i], width);
                offset += width;
            }
            return instruction;
        }

        public static void WriteLE(byte[] bytes, int offset, ulong value, int width)
        {
            for (int i = 0; i < width; i++)
                bytes[offset + i] = (byte)(value >> (8*i) & 0xFF);
        }

        public static ulong ReadLE(byte[] bytes, int offset, int width)
        {
            ulong result = 0;
            for (int i = 0; i < width; i++)
                result += (ulong)(bytes[offset + i] << (8*i));
            return result;
        }

        public static string BCToString(byte[] bc)
        {
            string acc = "";
            int pos = 0;
            while (pos < bc.Length)
            {
                int curPos = pos;
                var (opCode, operands) = ReadInstruction(bc, ref pos);
                acc += $"{curPos:0000} {(Definitions.ContainsKey(opCode) ? Definitions[opCode].Name : opCode.ToString())} {string.Join(" ", operands)}\n";
            }
            return acc.Length == 0 ? "" : acc[..^1];
        }

        public static (OpCode, ulong[]) ReadInstruction(byte[] bc, ref int pos)
        {
            var op = (OpCode)bc[pos++];
            if (!Definitions.TryGetValue(op, out var def))
                def = new OpDefinition(op.ToString(), []);
            ulong[] operands = new ulong[def.OperandWidths.Length];
            for (int i = 0; i < operands.Length; i++)
            {
                operands[i] = ReadLE(bc, pos, def.OperandWidths[i]);
                pos += def.OperandWidths[i];
            }
            return (op, operands);
        }

        
    }
    public struct OpDefinition(string name, int[] operandWidths)
    {
        public string Name { get; } = name;
        public int[] OperandWidths { get; } = operandWidths;
    }

    public enum OpCode
    {
        NOP,
        HALT,
        PUSH,
        GET,
        SET,
        DUP,
        POP,
        ADD,
        SUB,
        MUL,
        DIV,
        CONCAT,
        TRUE,
        FALSE,
        ABYSS,
        NOT,
        NEGATE,
        EQUAL,
        GT,
        LT,
        JT,
        JF,
        JMP,
        POW,
        PUSHTMP, // Lifts the TempReg back into the Stack to convert the result of an expressionstatement into an expressionresult
    }
}
