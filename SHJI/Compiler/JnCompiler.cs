using Jane.Core;
using SHJI.Bytecode;
using SHJI.VM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SHJI.Compiler
{
    public class JnCompiler
    {
        public byte[] Instructions { get => [.. instructions]; }
        public JaneValue[] Constants { get => [.. consts.OrderBy(x => x.Value).Select(x => x.Key)]; }
        public CompilerError[] Errors { get => [.. errors]; }

        private List<byte> instructions = [];
        private Dictionary<JaneValue, int> consts = [];
        public SymbolTable SymbolTable { get; private set; }

        private List<CompilerError> errors = [];

        public JnCompiler() : this(new(), []) { }
        public JnCompiler(SymbolTable st, JaneValue[] constants)
        {
            SymbolTable = st;
            consts = constants.Select((c, i) => new KeyValuePair<JaneValue, int>(c, i)).ToDictionary();
        }

        private static ImmutableDictionary<string, OpCode> OperatorToInstruction = new Dictionary<string, OpCode>()
        {
            { "+", OpCode.ADD },
            { "-", OpCode.SUB },
            { "*", OpCode.MUL },
            { "/", OpCode.DIV },
            { "~", OpCode.CONCAT },
            { "==", OpCode.EQUAL },
            { "^", OpCode.POW },
            { ">", OpCode.GT },
            { "<", OpCode.LT },
        }.ToImmutableDictionary();

        private IASTNode? currentNode;

        public void Compile(IASTNode node)
        {
            currentNode = node;
            switch (node)
            {
                case ASTRoot a:
                    foreach (IStatement s in a.Statements)
                        Compile(s);
                    break;
                case BlockStatement a:
                    foreach (IStatement s in a.Statements)
                        Compile(s);
                    break;
                case ExpressionStatement a:
                    Compile(a.Expression);
                    Emit(OpCode.POP);
                    break;
                case InfixExpression a:
                    Compile(a.Left);
                    Compile(a.Right);

                    if (a.Operator == "!=")
                    {
                        Emit(OpCode.EQUAL);
                        Emit(OpCode.NOT);
                    }
                    else if (a.Operator == "<=")
                    {
                        Emit(OpCode.GT);
                        Emit(OpCode.NOT);
                    }
                    else if (a.Operator == ">=")
                    {
                        Emit(OpCode.LT);
                        Emit(OpCode.NOT);
                    }
                    else if (!OperatorToInstruction.TryGetValue(a.Operator, out var opc))
                        AddError(a, CompilerErrorType.Unspecified, "Unsupported Operator");
                    else Emit(opc);
                    break;
                case BooleanLiteral a:
                    if (a.Value)
                        Emit(OpCode.TRUE);
                    else
                        Emit(OpCode.FALSE);
                    break;
                case PrefixExpression a:
                    Compile(a.Right);
                    Emit(a.Operator switch
                    {
                        "!" => OpCode.NOT,
                        "-" => OpCode.NEGATE,
                        _ => AddErrorNOP(node, CompilerErrorType.Unspecified, "Unsupported Operator")
                    });
                    break;
                case StringLiteral a:
                    if (a.Expressions.Length == 0)
                        Emit(OpCode.PUSH, (ulong)MakeConst(new JaneString("")));
                    else
                    {
                        Compile(a.Expressions[0]);
                        if (a.Expressions.Length > 1)
                        {
                            foreach (var e in a.Expressions[1..])
                            {
                                Compile(e);
                                Emit(OpCode.CONCAT);
                            }
                        }
                    }
                    break;
                case Interpolation a:
                    Compile(a.Content);
                    break;
                case Jane.Core.StringContent a:
                    Emit(OpCode.PUSH, (ulong)MakeConst(new JaneString(Regex.Unescape(a.EscapedValue))));
                    break;
                case IntegerLiteral a:
                    Emit(OpCode.PUSH, (ulong)MakeConst(new JaneInt(a.Value)));
                    break;
                case IfExpression a:
                    Compile(a.Condition);
                    int jinstr = Emit(OpCode.JF, 0);
                    Compile(a.Cons);
                    int finstr = Emit(OpCode.JMP, 0);
                    ReplaceInstr(jinstr, JnBytecode.Make(OpCode.JF, (ulong)instructions.Count));
                    if (a.Alt != null)
                        Compile(a.Alt);
                    else
                    {
                        Emit(OpCode.ABYSS);
                        Emit(OpCode.POP);
                    }
                    ReplaceInstr(finstr, JnBytecode.Make(OpCode.JMP, (ulong)instructions.Count));
                    Emit(OpCode.PUSHTMP);
                    break;
                case Abyss a:
                    Emit(OpCode.ABYSS);
                    break;
                case LetExpression a:
                    if (a.Value is null)
                        Emit(OpCode.ABYSS);
                    else
                        Compile(a.Value);
                    Emit(OpCode.DUP);
                    var ls = SymbolTable.Define(a.Name.Value);
                    if (ls is null)
                        AddError(a, CompilerErrorType.Unspecified, $"Symbol {a.Name} has already been defined.");
                    else
                        Emit(OpCode.SET, (ulong)ls.Value.Index);
                    break;
                case Identifier a:
                    var ids = SymbolTable.Resolve(a.Value);
                    if (ids is null)
                        AddError(a, CompilerErrorType.Unspecified, $"Variable {a.Value} not declared in this scope.");
                    else
                        Emit(OpCode.GET, (ulong)ids.Value.Index);
                    break;
                case Assignment a:
                    Compile(a.Value);
                    Emit(OpCode.DUP);
                    var ass = SymbolTable.Resolve(a.Name.Value);
                    if (ass is null) AddError(a, CompilerErrorType.Unspecified, $"Variable {a.Value} not declared in this scope.");
                    else Emit(OpCode.SET, (ulong)ass.Value.Index);
                    break;
                default:
                    AddError(node, CompilerErrorType.Unspecified, $"AST Node Type not implemented");
                    break;
            }
            return;
        }

        private int Emit(OpCode opCode, params ulong[] operands)
        {
            var instr = JnBytecode.Make(opCode, operands);
            if (instr.Length == 0 && currentNode != null) AddError(currentNode, CompilerErrorType.Unspecified, "Binary conversion failed");
            return AddInstruction(instr);
        }

        private int AddInstruction(byte[] instr)
        {
            int pos = instructions.Count;
            instructions.AddRange(instr);
            return pos;
        }

        /// <summary>
        /// Adds a constant to the heap and returns its index
        /// </summary>
        /// <param name="constant">The constant to be added</param>
        /// <returns>The index in the heap</returns>
        private int MakeConst(JaneValue constant)
        {
            if (consts.TryGetValue(constant, out int val))
                return val;
            int currentCount = consts.Count;
            consts.Add(constant, currentCount);
            return currentCount;
        }

        private void ReplaceInstr(int instr_p, byte[] ninstr)
        {
            for (int i = 0; i < ninstr.Length; i++)
            {
                instructions[instr_p + i] = ninstr[i];
            }
        }

        private void AddError(IASTNode node, CompilerErrorType compilerErrorType = CompilerErrorType.Unspecified, string message = "")
        {
            CompilerError e = new(message, node.Token, compilerErrorType);
            errors.Add(e);
        }
        private OpCode AddErrorNOP(IASTNode node, CompilerErrorType compilerErrorType = CompilerErrorType.Unspecified, string message = "")
        {
            AddError(node, compilerErrorType, message);
            return OpCode.NOP;
        }
    }
}
