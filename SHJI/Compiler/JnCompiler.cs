using Jane.Core;
using SHJI.Bytecode;
using SHJI.VM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SHJI.Compiler
{
    public class JnCompiler
    {
        public byte[] CurrentInstructions {
            get => [.. scopes.Last().Instructions];
        }
        public JaneValue[] Constants { get => [.. consts.OrderBy(x => x.Value).Select(x => x.Key)]; }
        public CompilerError[] Errors { get => [.. errors]; }

        private readonly List<Scope> scopes = [];
        private readonly Dictionary<JaneValue, int> consts;
        public SymbolTable SymbolTable { get; private set; }

        private readonly List<CompilerError> errors = [];

        public JnCompiler(SymbolTable st, JaneValue[] constants)
        {
            consts = constants.Select((c, i) => new KeyValuePair<JaneValue, int>(c, i)).ToDictionary();
            SymbolTable = st;

            Scope topLevel = new();
            scopes = [topLevel];
        }

        public JnCompiler() : this(new(), []) { }

        private static readonly ImmutableDictionary<string, OpCode> OperatorToInstruction = new Dictionary<string, OpCode>()
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
                    EnterScope();
                    foreach (IStatement s in a.Statements)
                        Compile(s);
                    var block = LeaveScope();
                    scopes.Last().Instructions.AddRange(block);
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
                    Emit(OpCode.LOAD, (ulong)GetOrCreateConst(new JaneString("")));
                    foreach (var e in a.Expressions)
                    {
                        Compile(e);
                        Emit(OpCode.CONCAT);
                    }
                    break;
                case Interpolation a:
                    Compile(a.Content);
                    break;
                case Jane.Core.StringContent a:
                    Emit(OpCode.LOAD, (ulong)GetOrCreateConst(new JaneString(a.Value)));
                    break;
                case IntegerLiteral a:
                    Emit(OpCode.LOAD, (ulong)GetOrCreateConst(new JaneInt(a.Value)));
                    break;
                case IfExpression a:
                    Compile(a.Condition);
                    int jinstr = Emit(OpCode.JF, 0);
                    Compile(a.Cons);
                    int finstr = Emit(OpCode.JMP, 0);
                    ReplaceInstr(jinstr, JnBytecode.Make(OpCode.JF, (ulong)CurrentInstructions.Length));
                    if (a.Alt != null)
                        Compile(a.Alt);
                    else
                    {
                        Emit(OpCode.ABYSS);
                        Emit(OpCode.POP);
                    }
                    ReplaceInstr(finstr, JnBytecode.Make(OpCode.JMP, (ulong)CurrentInstructions.Length));
                    Emit(OpCode.PUSHTMP);
                    break;
                case Abyss:
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
                        Emit(ls.Value.Scope == "local" ? OpCode.SET : OpCode.SET_GLOBAL, (ulong)ls.Value.Index);
                    break;
                case Identifier a:
                    var ids = SymbolTable.Resolve(a.Value);
                    if (ids is null)
                        AddError(a, CompilerErrorType.Unspecified, $"Variable {a.Value} not declared in this scope.");
                    else
                        Emit(ids.Value.Scope == "local" ? OpCode.GET : OpCode.GET_GLOBAL, (ulong)ids.Value.Index);
                    break;
                case Assignment a:
                    Compile(a.Value);
                    Emit(OpCode.DUP);
                    var ass = SymbolTable.Resolve(a.Name.Value);
                    if (ass is null) AddError(a, CompilerErrorType.Unspecified, $"Variable {a.Value} not declared in this scope.");
                    else Emit(ass.Value.Scope == "local" ? OpCode.SET : OpCode.SET_GLOBAL, (ulong)ass.Value.Index);
                    break;
                case OperatorAssignment a:
                    Compile(new Assignment() { 
                        Token = a.Token, 
                        Name = a.Name, 
                        Value = new InfixExpression() {
                            Token = a.Token,
                            Operator = a.Operator,
                            Left = a.Name,
                            Right = a.Value
                        }
                    });
                    break;
                case PostfixExpression a:
                    Compile(a.Left);
                    if (a.Operator == "++")
                    {
                        Emit(OpCode.LOAD, (ulong)GetOrCreateConst(new JaneInt(1)));
                        Emit(OpCode.ADD);
                    }
                    else if (a.Operator == "--")
                    {
                        Emit(OpCode.LOAD, (ulong)GetOrCreateConst(new JaneInt(1)));
                        Emit(OpCode.ADD);
                    }
                    break;
                case FloatLiteral a:
                    Emit(OpCode.LOAD, (ulong)GetOrCreateConst(new JaneFloat(a.Value)));
                    break;
                case ArrayLiteral a:
                    foreach (IExpression arr_expr in a.Elements.Reverse())
                        Compile(arr_expr);
                    Emit(OpCode.CONSTR_ARR, (ulong)a.Elements.Length);
                    break;
                case IndexingExpression a:
                    Compile(a.Indexed);
                    Compile(a.Index);
                    Emit(OpCode.INDEX);
                    break;
                case CharLiteral a:
                    if (a.Value.Length != 1) AddError(a, CompilerErrorType.Unspecified, $"Char Literal contains not exactly one character.");
                    else Emit(OpCode.LOAD, (ulong)GetOrCreateConst(new JaneChar(a.Value[0])));
                    break;
                case LambdaExpression a:
                    EnterScope();
                    Compile(a.Body);
                    if (a.Body is BlockStatement body)
                    {
                        if (body.Statements.Length == 0)
                            Emit(OpCode.ABYSS);
                        else if (body.Statements.Last() is ExpressionStatement)
                            Emit(OpCode.PUSHTMP);
                    }
                    else if (a.Body is ExpressionStatement)
                        Emit(OpCode.PUSHTMP);
                    else Emit(OpCode.ABYSS);

                    Emit(OpCode.RET);
                    Optimize();
                    int numLocals = SymbolTable.Store.Count;
                    var instrs = LeaveScope();
                    var lambda = new JaneFunction(instrs, numLocals);
                    Emit(OpCode.LOAD, (ulong)GetOrCreateConst(lambda));
                    break;
                case FunctionDecl a:
                    EnterScope();
                    Compile(a.Body);
                    if (a.Body.Statements.Length == 0)
                        Emit(OpCode.ABYSS);
                    else if (a.Body.Statements.Last() is ExpressionStatement)
                        Emit(OpCode.PUSHTMP);
                    Emit(OpCode.RET);
                    Optimize();
                    int fnumLocals = SymbolTable.Store.Count;
                    var finstrs = LeaveScope();
                    var function = new JaneFunction(finstrs, fnumLocals);
                    Emit(OpCode.LOAD, (ulong)GetOrCreateConst(function));
                    var sym = SymbolTable.Define(a.Name.Value);
                    if (sym is null) AddError(currentNode, CompilerErrorType.Unspecified, "Function name already present in scope");
                    else Emit(sym.Value.Scope == "local" ? OpCode.SET : OpCode.SET_GLOBAL, (ulong)sym.Value.Index);
                    break;
                case ReturnStatement a:
                    if (a.ReturnValue != null) Compile(a.ReturnValue);
                    else Emit(OpCode.ABYSS);
                    Emit(OpCode.RET);
                    break;
                case CallExpression a:
                    Compile(a.Function);
                    //foreach (IExpression e in a.Arguments)
                    //    Compile(e);
                    Emit(OpCode.CALL);
                    break;
                default:
                    AddError(node, CompilerErrorType.Unspecified, $"AST Node Type not implemented");
                    break;
            }
            return;
        }

        // as if lol
        public void Optimize()
        {
            if (CurrentInstructions.Length == 0) return;
            OptimizePeepholePopPush();
            RemoveNOP();
        }

        private void OptimizePeepholePopPush()
        {
            int position = 0;
            byte[] cur = CurrentInstructions;
            var (opFirst, _) = JnBytecode.ReadInstruction(cur, ref position);
            while (position < cur.Length)
            {
                var (opSecond, _) = JnBytecode.ReadInstruction(cur, ref position);
                if (opFirst == OpCode.POP && opSecond == OpCode.PUSHTMP)
                {
                    byte[] nop = JnBytecode.Make(OpCode.NOP);
                    ReplaceInstr(position - 2, nop);
                    ReplaceInstr(position - 1, nop);
                }
                opFirst = opSecond;
            }
        }

        private void RemoveNOP()
        {
            int position = 0;
            int removed = 0;
            byte[] cur = CurrentInstructions;
            List<byte> newInstrs = [.. cur];
            while (position < cur.Length)
            {
                var (op, _) = JnBytecode.ReadInstruction(cur, ref position);
                if (op == OpCode.NOP)
                    newInstrs.RemoveAt(position - 1 - (removed++));
            }
            scopes[^1].Instructions = newInstrs;
        }

        private void EnterScope()
        {
            var scope = new Scope();
            scopes.Add(scope);
            SymbolTable = SymbolTable.MakeNested();
        }

        private byte[] LeaveScope()
        {
            var instrs = scopes.Last().Instructions;
            scopes.RemoveAt(scopes.Count - 1);
            SymbolTable = SymbolTable.Outer ?? throw new NullReferenceException("Outer Symbol Table not defined when leaving scope");
            return [..instrs];
        }

        private int Emit(OpCode opCode, params ulong[] operands)
        {
            var instr = JnBytecode.Make(opCode, operands);
            if (instr.Length == 0 && currentNode != null) AddError(currentNode, CompilerErrorType.Unspecified, "Binary conversion failed");
            return AddInstruction(instr);
        }

        private int AddInstruction(byte[] instr)
        {
            int pos = CurrentInstructions.Length;
            scopes.Last().Instructions.AddRange(instr);
            return pos;
        }

        /// <summary>
        /// Adds a constant to the Pile and returns its index
        /// </summary>
        /// <param name="constant">The constant to be added</param>
        /// <returns>The index in the Pile</returns>
        private int GetOrCreateConst(JaneValue constant)
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
                scopes.Last().Instructions[instr_p + i] = ninstr[i];
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
