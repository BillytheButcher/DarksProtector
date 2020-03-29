namespace Confuser.Protections.MildReferenceProxy
{
    using Confuser.DynCipher.AST;
    using Confuser.DynCipher.Generation;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class ExpressionEncoding : IRPEncoding
    {
        private readonly Dictionary<MethodDef, Tuple<Expression, Func<int, int>>> keys = new Dictionary<MethodDef, Tuple<Expression, Func<int, int>>>();

        private void Compile(RPContext ctx, CilBody body, out Func<int, int> expCompiled, out Expression inverse)
        {
            Expression expression;
            Variable variable = new Variable("{VAR}");
            Variable variable2 = new Variable("{RESULT}");
            VariableExpression var = new VariableExpression {
                Variable = variable
            };
            VariableExpression result = new VariableExpression {
                Variable = variable2
            };
            ctx.DynCipher.GenerateExpressionPair(ctx.Random, var, result, ctx.Depth, out expression, out inverse);
            expCompiled = new DMCodeGen(typeof(int), new Tuple<string, Type>[] { Tuple.Create<string, Type>("{VAR}", typeof(int)) }).GenerateCIL(expression).Compile<Func<int, int>>();
        }

        public Instruction[] EmitDecode(MethodDef init, RPContext ctx, Instruction[] arg)
        {
            Tuple<Expression, Func<int, int>> key = this.GetKey(ctx, init);
            List<Instruction> instrs = new List<Instruction>();
            new CodeGen(arg, ctx.Method, instrs).GenerateCIL(key.Item1);
            CilBody body = init.Body;
            body.MaxStack = (ushort) (body.MaxStack + ((ushort) ctx.Depth));
            return instrs.ToArray();
        }

        public int Encode(MethodDef init, RPContext ctx, int value) => 
            this.GetKey(ctx, init).Item2(value);

        private Tuple<Expression, Func<int, int>> GetKey(RPContext ctx, MethodDef init)
        {
            Tuple<Expression, Func<int, int>> tuple;
            if (!this.keys.TryGetValue(init, out tuple))
            {
                Func<int, int> func;
                Expression expression;
                this.Compile(ctx, init.Body, out func, out expression);
                this.keys[init] = tuple = Tuple.Create<Expression, Func<int, int>>(expression, func);
            }
            return tuple;
        }

        private class CodeGen : CILCodeGen
        {
            private readonly Instruction[] arg;

            public CodeGen(Instruction[] arg, MethodDef method, IList<Instruction> instrs) : base(method, instrs)
            {
                this.arg = arg;
            }

            protected override void LoadVar(Variable var)
            {
                if (var.Name == "{RESULT}")
                {
                    foreach (Instruction instruction in this.arg)
                    {
                        base.Emit(instruction);
                    }
                }
                else
                {
                    base.LoadVar(var);
                }
            }
        }
    }
}

