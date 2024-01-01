using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHJI.VM
{
    internal class ByteCode(byte[] instructions, IJaneObject[] constants)
    {
        static Dictionary<OpCode, Definition> definitions = [];

        public static Definition? Lookup(OpCode opCode)
        {
            if (definitions.TryGetValue(opCode, out Definition value))
            {
                return value;
            }
            return null;
        }

        public static byte[] Make(OpCode op, params int[] operands)
        {
            Definition? definition = Lookup(op);
            if (definition is null) return [];
            Definition def = definition.Value;
            int instructionLength = 1 + def.OperandWidths.Sum();
            byte[] instruction = new byte[instructionLength];
            instruction[0] = (byte)op;

            int offset = 1;
            for (int i = 0; i < operands.Length; i++)
            {
                int width = def.OperandWidths[i];
                WriteLittleEndian(instruction, offset, operands[i], width);
                offset += width;
            }

            return instruction;
        }

        public static (int[], int) ReadOperands(Definition def, byte[] instr)
        {
            int[] operands = new int[def.OperandWidths.Length];
            int offset = 0;
            for (int i = 0; i < def.OperandWidths.Length; i++)
            {
                int width = def.OperandWidths[i];
                operands[i] = ReadLittleEndian(instr[offset..], width);
                offset += width;
            }

            return (operands, offset);
        }

        private static void WriteLittleEndian(byte[] pointer, int offset, int operand, int width)
        {
            for (int i = 0; i < width; i++)
            {
                pointer[offset + i] = (byte)((operand >> (i * 8)) & 0xFF);
            }
        }

        private static int ReadLittleEndian(byte[] pointer, int width)
        {
            int result = 0;
            for (int i = 0; i < width; i++)
            {
                result |= pointer[i] << (i * 8);
            }
            return result;
        }

        public static string Stringify(byte[] instructions)
        {
            string result = "";
            int i = 0;
            foreach (byte b in instructions)
            {
                Definition? d = Lookup((OpCode)b);
                if (d is null)
                {
                    result += $"ERROR: Unknown Opcode {b}\n";
                    continue;
                }
                Definition def = d.Value;
                (int[] operands, int offset) = ReadOperands(def, instructions[(i+1)..]);
                result += $"{i:04} {(
                    def.OperandWidths.Length != operands.Length
                        ? ($"ERROR: operand len {operands.Length} does not match defined {def.OperandWidths.Length}\n")
                        : $"{def.Name} {operands.Select(s => s.ToString()).Aggregate((a, b) => a + b)}")}\n";
                result += "\n";
                i++;
            }
            return result;
        }

        public byte[] Instructions { get; } = instructions;
        public IJaneObject[] Constants { get; } = constants;
    }

    internal struct Definition
    {
        public string Name;
        public int[] OperandWidths;
    }

    internal enum OpCode
    {
        OpConstant
    }
}
