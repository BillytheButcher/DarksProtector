namespace Confuser.Protections.MildReferenceProxy
{
    using Confuser.Core;
    using Confuser.Renamer;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using System;
    using System.Collections.Generic;

    internal class MildMode : RPMode
    {
        private readonly Dictionary<Tuple<Code, TypeDef, IMethod>, MethodDef> proxies = new Dictionary<Tuple<Code, TypeDef, IMethod>, MethodDef>();

        public override void Finalize(RPContext ctx)
        {
        }

        public override void ProcessCall(RPContext ctx, int instrIndex)
        {
            Instruction instruction = ctx.Body.Instructions[instrIndex];
            IMethod operand = (IMethod) instruction.Operand;
            if (!operand.DeclaringType.ResolveTypeDefThrow().IsValueType && (operand.ResolveThrow().IsPublic || operand.ResolveThrow().IsAssembly))
            {
                MethodDef def;
                Tuple<Code, TypeDef, IMethod> key = Tuple.Create<Code, TypeDef, IMethod>(instruction.OpCode.Code, ctx.Method.DeclaringType, operand);
                if (!this.proxies.TryGetValue(key, out def))
                {
                    MethodSig methodSig = RPMode.CreateProxySignature(ctx, operand, instruction.OpCode.Code == Code.Newobj);
                    def = new MethodDefUser(NameService.RandomNameStatic(), methodSig) {
                        Attributes = MethodAttributes.CompilerControlled | MethodAttributes.Static,
                        ImplAttributes = MethodImplAttributes.IL
                    };
                    ctx.Method.DeclaringType.Methods.Add(def);
                    if ((instruction.OpCode.Code == Code.Call) && operand.ResolveThrow().IsVirtual)
                    {
                        def.IsStatic = false;
                        methodSig.HasThis = true;
                        methodSig.Params.RemoveAt(0);
                    }
                    ctx.Marker.Mark(def, ctx.Protection);
                    /*ctx.Name.Analyze(def);
                    ctx.Name.SetCanRename(def, false);*/
                    def.Body = new CilBody();
                    for (int i = 0; i < def.Parameters.Count; i++)
                    {
                        def.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, def.Parameters[i]));
                    }
                    def.Body.Instructions.Add(Instruction.Create(instruction.OpCode, operand));
                    def.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    this.proxies[key] = def;
                }
                instruction.OpCode = OpCodes.Call;
                if (ctx.Method.DeclaringType.HasGenericParameters)
                {
                    GenericVar[] genArgs = new GenericVar[ctx.Method.DeclaringType.GenericParameters.Count];
                    for (int j = 0; j < genArgs.Length; j++)
                    {
                        genArgs[j] = new GenericVar(j);
                    }
                    instruction.Operand = new MemberRefUser(ctx.Module, def.Name, def.MethodSig, new GenericInstSig((ClassOrValueTypeSig) ctx.Method.DeclaringType.ToTypeSig(), genArgs).ToTypeDefOrRef());
                }
                else
                {
                    instruction.Operand = def;
                }
            }
        }
    }
}

