namespace Confuser.Protections.MildReferenceProxy
{
    using Confuser.Core;
    using Confuser.DynCipher;
    using Confuser.DynCipher.AST;
    using Confuser.DynCipher.Generation;
    using Core.Services;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using dnlib.DotNet.Writer;
    using Renamer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class x86Encoding : IRPEncoding
    {
        private bool addedHandler;
        private readonly Dictionary<MethodDef, Tuple<MethodDef, Func<int, int>>> keys = new Dictionary<MethodDef, Tuple<MethodDef, Func<int, int>>>();
        private readonly List<Tuple<MethodDef, byte[], dnlib.DotNet.Writer.MethodBody>> nativeCodes = new List<Tuple<MethodDef, byte[], dnlib.DotNet.Writer.MethodBody>>();

        private void Compile(RPContext ctx, out Func<int, int> expCompiled, out MethodDef native)
        {
            x86Register? nullable;
            Expression expression;
            Variable variable = new Variable("{VAR}");
            Variable variable2 = new Variable("{RESULT}");
            CorLibTypeSig retType = ctx.Module.CorLibTypes.Int32;
            native = new MethodDefUser(ctx.Context.Registry.GetService<NameService>().RandomName(), MethodSig.CreateStatic(retType, retType), MethodAttributes.CompilerControlled | MethodAttributes.PinvokeImpl | MethodAttributes.Static);
            native.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.ManagedMask | MethodImplAttributes.Native | MethodImplAttributes.PreserveSig;
            ctx.Module.GlobalType.Methods.Add(native);
            ctx.Context.Registry.GetService<IMarkerService>().Mark(native, ctx.Protection);
            ctx.Context.Registry.GetService<NameService>().SetCanRename(native, false);
            x86CodeGen codeGen = new x86CodeGen();
            do
            {
                Expression expression2;
                VariableExpression var = new VariableExpression {
                    Variable = variable
                };
                VariableExpression result = new VariableExpression {
                    Variable = variable2
                };
                ctx.DynCipher.GenerateExpressionPair(ctx.Random, var, result, ctx.Depth, out expression, out expression2);
                nullable = codeGen.GenerateX86(expression2, (v, r) => new x86Instruction[] { x86Instruction.Create(x86OpCode.POP, new Ix86Operand[] { new x86RegisterOperand(r) }) });
            }
            while (!nullable.HasValue);
            byte[] buffer = CodeGenUtils.AssembleCode(codeGen, nullable.Value);
            expCompiled = new DMCodeGen(typeof(int), new Tuple<string, Type>[] { Tuple.Create<string, Type>("{VAR}", typeof(int)) }).GenerateCIL(expression).Compile<Func<int, int>>();
            this.nativeCodes.Add(Tuple.Create<MethodDef, byte[], dnlib.DotNet.Writer.MethodBody>(native, buffer, null));
            if (!this.addedHandler)
            {
                ctx.Context.CurrentModuleWriterListener.OnWriterEvent += new EventHandler<ModuleWriterListenerEventArgs>(this.InjectNativeCode);
                this.addedHandler = true;
            }
        }

        public Instruction[] EmitDecode(MethodDef init, RPContext ctx, Instruction[] arg)
        {
            Tuple<MethodDef, Func<int, int>> key = this.GetKey(ctx, init);
            List<Instruction> list = new List<Instruction>();
            list.AddRange(arg);
            list.Add(Instruction.Create(OpCodes.Call, key.Item1));
            return list.ToArray();
        }

        public int Encode(MethodDef init, RPContext ctx, int value) => 
            this.GetKey(ctx, init).Item2(value);

        private Tuple<MethodDef, Func<int, int>> GetKey(RPContext ctx, MethodDef init)
        {
            Tuple<MethodDef, Func<int, int>> tuple;
            if (!this.keys.TryGetValue(init, out tuple))
            {
                Func<int, int> func;
                MethodDef def;
                this.Compile(ctx, out func, out def);
                this.keys[init] = tuple = Tuple.Create<MethodDef, Func<int, int>>(def, func);
            }
            return tuple;
        }

        private void InjectNativeCode(object sender, ModuleWriterListenerEventArgs e)
        {
            ModuleWriterBase base2 = (ModuleWriterBase) sender;
            if (e.WriterEvent == ModuleWriterEvent.MDEndWriteMethodBodies)
            {
                for (int i = 0; i < this.nativeCodes.Count; i++)
                {
                    this.nativeCodes[i] = new Tuple<MethodDef, byte[], dnlib.DotNet.Writer.MethodBody>(this.nativeCodes[i].Item1, this.nativeCodes[i].Item2, base2.MethodBodies.Add(new dnlib.DotNet.Writer.MethodBody(this.nativeCodes[i].Item2)));
                }
            }
            else if (e.WriterEvent == ModuleWriterEvent.EndCalculateRvasAndFileOffsets)
            {
                foreach (Tuple<MethodDef, byte[], dnlib.DotNet.Writer.MethodBody> tuple in this.nativeCodes)
                {
                    uint rid = base2.MetaData.GetRid(tuple.Item1);
                    base2.MetaData.TablesHeap.MethodTable[rid].RVA = (uint) tuple.Item3.RVA;
                }
            }
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

