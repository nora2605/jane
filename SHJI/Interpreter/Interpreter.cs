using Jane.AST;
using Jane.Lexer;
using static SHJI.IJaneObject;

namespace SHJI.Interpreter
{
    internal static class Interpreter
    {
        internal delegate IJaneObject JaneBinaryOperator(IJaneObject left, IJaneObject right);

        internal static bool returning = false;
        internal static Token processingToken;

        internal static IJaneObject Eval(IASTNode node, JaneEnvironment env)
        {
            processingToken = node.Token;
            IJaneObject result = node switch
            {
                // Statements
                ASTRoot r => EvalProgram(r.statements, env),
                ExpressionStatement es => Eval(es.Expression, env),
                ReturnStatement rs => rs.ReturnValue is null ? JANE_ABYSS : Eval(rs.ReturnValue, env),
                // Expressions

                // TODO: Immediate coalescions for integer literals
                // INumberLiteral l => EvalNumber(),
                IntegerLiteral il => new JaneInt() { Value = il.Value },
                LongLiteral il => new JaneLong() { Value = il.Value },
                Int128Literal il => new JaneInt128() { Value = il.Value },
                UInt128Literal il => new JaneUInt128() { Value = il.Value },
                FloatLiteral fl => new JaneDouble() { Value = fl.Value },

                Jane.AST.Boolean b => b.Value ? JANE_TRUE : JANE_FALSE,
                RawStringLiteral s => new JaneString { Value = System.Text.RegularExpressions.Regex.Unescape(s.EscapedValue) },
                VerbatimStringLiteral s => new JaneString { Value = System.Text.RegularExpressions.Regex.Unescape(s.EscapedValue) },
                InterpolatedStringLiteral s => EvalInterpolatedString(s, env),
                InterpolatedVerbatimStringLiteral s => EvalIntVerbString(s, env),
                Interpolation e => Eval(e.Content, env),
                Jane.AST.StringContent c => new JaneString { Value = System.Text.RegularExpressions.Regex.Unescape(c.EscapedValue) },
                CharLiteral c => new JaneChar { Value = System.Text.RegularExpressions.Regex.Unescape(c.Value).Length == 1 ? System.Text.RegularExpressions.Regex.Unescape(c.Value)[0] : '\0' },

                ArrayLiteral a => EvalArray(a, env),
                IndexingExpression ie => EvalIndexing(ie, env),

                PrefixExpression pex => EvalPrefixExpression(pex, env),
                InfixExpression inf => EvalInfixExpression(inf, env),
                BlockStatement s => EvalStatements(s.Statements, env),
                IfExpression ife => EvalIfExpression(ife, env),
                LetExpression le => EvalLetExpression(le, env),
                Identifier id => EvalIdentifier(id, env),
                Assignment ass => EvalAssignment(ass, env),
                FunctionLiteral fl => EvalFunctionLiteral(fl, env),
                CallExpression ce => EvalCallExpression(ce, env),
                _ => JANE_ABYSS,
            };
            return result;
        }

        internal static IJaneObject EvalArray(ArrayLiteral a, JaneEnvironment env)
        {
            var elements = EvalExpressions(a.Elements, env);
            return new JaneArray() { Value = elements };
        }

        internal static IJaneObject EvalIndexing(IndexingExpression ie, JaneEnvironment env)
        {
            var indexed = Eval(ie.Indexed, env);
            var index = Eval(ie.Index, env);
            if (indexed is JaneArray a && index is JaneInt i)
            {
                return a.Value[i.Value];
            }
            throw new RuntimeError($"Can't Index type {indexed.Type()} with type {index.Type()}", processingToken);
        }

        internal static IJaneObject EvalInterpolatedString(InterpolatedStringLiteral s, JaneEnvironment env)
        {
            return new JaneString() { Value = s.Expressions.Select(x => Eval(x, env).Inspect()).Aggregate((a, b) => a + b) };
        }
        internal static IJaneObject EvalIntVerbString(InterpolatedVerbatimStringLiteral s, JaneEnvironment env)
        {
            return new JaneString() { Value = s.Expressions.Select(x => Eval(x, env).Inspect()).Aggregate((a, b) => a + b) };
        }

        internal static IJaneObject EvalCallExpression(CallExpression ce, JaneEnvironment env)
        {
            var function = Eval(ce.Function, env);
            if (function is not JaneFunction and not JaneBuiltIn) throw new RuntimeError($"Cannot perform call on Object of Type {function.Type()}", processingToken);
            var args = EvalExpressions(ce.Arguments, env);
            return function is JaneFunction f ? ApplyFunction(f, args) : ApplyBuiltIn((JaneBuiltIn)function, args);
        }

        internal static IJaneObject ApplyBuiltIn(JaneBuiltIn func, IJaneObject[] args)
        {
            return func.Fn(args);
        }

        internal static IJaneObject ApplyFunction(JaneFunction func, IJaneObject[] args)
        {
            var extendedEnv = ExtendFunctionEnv(func, args);
            var evaluated = EvalFunction(func.Body.Statements, extendedEnv);
            return evaluated;
        }

        internal static JaneEnvironment ExtendFunctionEnv(JaneFunction func, IJaneObject[] args)
        {
            JaneEnvironment env = new(func.Environment);
            for (int i = 0; i < func.Parameters.Length; i++)
            {
                var param = func.Parameters[i];
                env.Set(param.Value, args[i]);
            }
            return env;
        }

        internal static IJaneObject[] EvalExpressions(IExpression[] exprs, JaneEnvironment env)
        {
            List<IJaneObject> result = [];
            for (var i = 0; i < exprs.Length; i++)
            {
                result.Add(Eval(exprs[i], env));
            }
            return [.. result];
        }

        internal static IJaneObject EvalFunctionLiteral(FunctionLiteral fl, JaneEnvironment env)
        {
            env[fl.Name] = new JaneFunction { Flags = fl.Flags, Parameters = fl.Parameters, Body = fl.Body, Environment = env, ReturnType = fl.ReturnType, Name = fl.Name };
            return env[fl.Name];
        }

        internal static IJaneObject EvalAssignment(Assignment ass, JaneEnvironment env)
        {
            var expr = Eval(ass.Value, env);
            env[ass.Name.Value] = expr;
            return expr;
        }

        internal static IJaneObject EvalLetExpression(LetExpression le, JaneEnvironment env)
        {
            if (env.Has(le.Name.Value)) throw new RuntimeError($"Variable name already in use. Use .clear to clear the environment");
            IJaneObject val = le.Value is null ? JANE_UNINITIALIZED : Eval(le.Value, env);
            env.Set(le.Name.Value, val);
            return val;
        }

        internal static IJaneObject EvalIdentifier(Identifier id, JaneEnvironment env)
        {
            var e = env.Get(id.Value);
            if (e == JANE_UNINITIALIZED)
                throw new RuntimeError($"Variable \"{id.Value}\" was uninitialized or not found at access time", processingToken);
            return e;
        }

        internal static IJaneObject EvalProgram(IStatement[] stmts, JaneEnvironment env)
        {
            IJaneObject result = JANE_ABYSS;
            foreach (IStatement stmt in stmts)
            {
                result = Eval(stmt, env);
                if (stmt is ReturnStatement || returning)
                {
                    returning = false;
                    break;
                }
            }

            // If there is no top level statement, find and call the main method

            return result;
        }

        internal static IJaneObject EvalFunction(IStatement[] stmts, JaneEnvironment env)
        {
            IJaneObject result = JANE_ABYSS;
            foreach (IStatement stmt in stmts)
            {
                result = Eval(stmt, env);
                if (stmt is ReturnStatement || returning)
                {
                    returning = false;
                    break;
                }
            }
            return result;
        }

        internal static IJaneObject EvalStatements(IStatement[] stmts, JaneEnvironment env)
        {
            IJaneObject result = JANE_ABYSS;
            foreach (IStatement stmt in stmts)
            {
                result = Eval(stmt, env);
                if (stmt is ReturnStatement)
                {
                    returning = true;
                    break;
                }
                /* if (stmt is BreakStatement)
                {
                    break;
                } */
            }
            return result;
        }

        internal static IJaneObject EvalIfExpression(IfExpression ife, JaneEnvironment env)
        {
            IJaneObject cond = Eval(ife.Condition, env);
            if (cond is JaneBool jb)
            {
                if (jb.Value) return Eval(ife.Cons, new(env));
                else return ife.Alt is null ? JANE_ABYSS : Eval(ife.Alt, new(env));
            }
            else throw new RuntimeError("Condition is not a Boolean", processingToken);
        }

        internal static IJaneObject EvalPrefixExpression(PrefixExpression prefixExpression, JaneEnvironment env)
        {
            IJaneObject right = Eval(prefixExpression.Right, env);
            return prefixExpression.Operator switch
            {
                "!" => EvalBangPrefixOperator(right),
                "-" => EvalMinusPrefixOperator(right),
                _ => JANE_ABYSS,
            };
        }

        internal static IJaneObject EvalInfixExpression(InfixExpression e, JaneEnvironment env)
        {
            IJaneObject left = Eval(e.Left, env);
            IJaneObject right = Eval(e.Right, env);

            // Insert type magic

            // make this.... general
            // I don't want to limit the operators perse but parsing new operators would require a dynamic parser and HONESTLY i can't be arsed
            // I think with basic arithmetic, rational, binary and boolean and even a special concatenate operator that you can use for lists, paths and all that
            // The language is already stacked pretty well.

            // Full list of (infix) operators TO BE IMPLEMENTED as of July 2023:
            // +, -, *, /, ^, %, >, <, >=, <=, ==, !=, &&, ||, xor
            // ~, &, |, x0r (binary xor), .. (range operator), :: (type coercion), :. (pufferfish operator, chains outer function calls)

            // Except for the type coercion and pufferfish operator, the operator behavior can be individually adjusted for every user defined type

            // An expression of type a <operator>= b will be evaluated as a = a <operator> b
            // Meaning it is valid to write var === true (technically, you would never use this probably)
            // A special case is var ?= a : b (Flipflop operator) which just switches between values so you don't have to modulo stuff
            // although a circular list or special type for that would PROBABLY be better IDK

            // Note on postfixes:
            // Implement the switcheroo-operator: !! postfix to change a boolean from true to false or from false to true
            return e.Operator switch
            {
                "+" => EvalPlusInfix(left, right),
                "-" => EvalMinusInfix(left, right),
                "/" => EvalSlashInfix(left, right),
                "*" => EvalAsteriskInfix(left, right),
                "^" => EvalHatInfix(left, right),
                ">" => EvalGTInfix(left, right),
                ">=" => EvalGTEInfix(left, right),
                "<" => EvalLTInfix(left, right),
                "<=" => EvalLTEInfix(left, right),
                "==" => EvalEQInfix(left, right),
                "!=" => EvalNOT_EQInfix(left, right),
                "~" => EvalConcatInfix(left, right),
                _ => throw new RuntimeError($"Operator {e.Operator} is unknown or not implemented", processingToken),
            } ?? throw new RuntimeError($"Operator {e.Operator} not implemented for operands of type {left.Type()} and {right.Type()}", processingToken);
        }
        #region Operator hell
        internal static IJaneObject EvalPlusInfix(IJaneObject left, IJaneObject right)
        {
            // Nasty
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return new JaneInt { Value = oLeftInt.Value + oRightInt.Value };
                // Soon to be added more? Maybe I'll find a better way
                // or just use like a static Jane definition file for all operators
                // probably not here but for the compiler definitely
                // because like
                // one will be able to declare custom operators eventually
                // and if those are stored in some way i might as well store my shit someway
            }
            throw new RuntimeError($"Operator + not implemented for operands of type {left.Type()} and {right.Type()}", processingToken);
        }
        internal static IJaneObject? EvalMinusInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return new JaneInt { Value = oLeftInt.Value - oRightInt.Value };
            }
            return null;
        }
        internal static IJaneObject? EvalSlashInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return new JaneInt { Value = oLeftInt.Value / oRightInt.Value };
            }
            return null;
        }
        internal static IJaneObject? EvalAsteriskInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return new JaneInt { Value = oLeftInt.Value * oRightInt.Value };
            }
            return null;
        }

        internal static IJaneObject? EvalHatInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return new JaneInt { Value = IntPow(oLeftInt.Value, oRightInt.Value) };
            }
            return null;
        }

        internal static IJaneObject? EvalGTInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return oLeftInt.Value > oRightInt.Value ? JANE_TRUE : JANE_FALSE;
            }
            return null;
        }

        internal static IJaneObject? EvalGTEInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return oLeftInt.Value >= oRightInt.Value ? JANE_TRUE : JANE_FALSE;
            }
            return null;
        }

        internal static IJaneObject? EvalLTInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return oLeftInt.Value < oRightInt.Value ? JANE_TRUE : JANE_FALSE;
            }
            return null;
        }

        internal static IJaneObject? EvalLTEInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return oLeftInt.Value <= oRightInt.Value ? JANE_TRUE : JANE_FALSE;
            }
            return null;
        }

        internal static IJaneObject? EvalEQInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneInt oLeftInt)
            {
                if (right is JaneInt oRightInt) return oLeftInt.Value == oRightInt.Value ? JANE_TRUE : JANE_FALSE;
            }
            if (left is JaneBool oLeftBool)
            {
                if (right is JaneBool oRightBool) return oLeftBool.Value == oRightBool.Value ? JANE_TRUE : JANE_FALSE;
            }
            return left == right ? JANE_TRUE : null;
        }

        internal static IJaneObject? EvalNOT_EQInfix(IJaneObject left, IJaneObject right)
        {
            var eq = EvalEQInfix(left, right);
            return eq == null ? null : eq == JANE_TRUE ? JANE_FALSE : JANE_TRUE;
        }

        internal static IJaneObject? EvalConcatInfix(IJaneObject left, IJaneObject right)
        {
            if (left is JaneString oLeftString)
            {
                if (right is JaneString oRightString) return new JaneString { Value = oLeftString.Value + oRightString.Value };
            }
            if (left is JaneChar oLeftChar)
            {
                if (right is JaneChar oRightChar) return new JaneString { Value = oLeftChar.Value.ToString() + oRightChar.Value.ToString() };
            }
            return null;
        }

        internal static int IntPow(int @base, int exponent)
        {
            if (exponent == 0) return 1;
            if (@base == 0) return 0;
            if (@base == 1) return 1;
            if (exponent < 0) return 0;
            else return @base * IntPow(@base, exponent - 1);
        }

        internal static IJaneObject EvalBangPrefixOperator(IJaneObject e)
        {
            if (e == JANE_TRUE) return JANE_FALSE;
            else if (e == JANE_FALSE) return JANE_TRUE;
            else if (e == JANE_ABYSS) return JANE_TRUE;
            throw new RuntimeError($"Cannot implicitly convert {e.Type()} into Boolean", processingToken);
        }

        internal static IJaneObject EvalMinusPrefixOperator(IJaneObject e) => e switch
        {
            JaneDouble @double => new JaneDouble { Value = -@double.Value },
            JaneFloat @float => new JaneFloat { Value = -@float.Value },
            JaneSByte @sbyte => new JaneSByte { Value = (sbyte)(-@sbyte.Value) },
            JaneShort @short => new JaneShort { Value = (short)(-@short.Value) },
            JaneInt @int => new JaneInt { Value = -@int.Value },
            JaneLong @long => new JaneLong { Value = -@long.Value },
            JaneInt128 @int128 => new JaneInt128 { Value = -@int128.Value },
            _ => throw new RuntimeError($"{e.Type()} is not a signed number type", processingToken)
        };
        #endregion
    }
}