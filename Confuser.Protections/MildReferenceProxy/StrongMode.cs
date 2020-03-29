namespace Confuser.Protections.MildReferenceProxy
{
    using Confuser.Core;
    using Confuser.Core.Helpers;
    using Confuser.DynCipher;
    using Confuser.DynCipher.AST;
    using Confuser.DynCipher.Generation;
    using Core.Services;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using dnlib.DotNet.Writer;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class StrongMode : RPMode
    {
        private RPContext encodeCtx;
        private readonly List<FieldDesc> fieldDescs = new List<FieldDesc>();
        private readonly Dictionary<Tuple<Code, IMethod, IRPEncoding>, Tuple<FieldDef, MethodDef>> fields = new Dictionary<Tuple<Code, IMethod, IRPEncoding>, Tuple<FieldDef, MethodDef>>();
        private readonly Dictionary<IRPEncoding, InitMethodDesc[]> inits = new Dictionary<IRPEncoding, InitMethodDesc[]>();
        private Tuple<TypeDef, Func<int, int>>[] keyAttrs;

        private MethodDef CreateBridge(RPContext ctx, TypeDef delegateType, FieldDef field, MethodSig sig)
        {
            MethodDefUser item = new MethodDefUser(ctx.Name.RandomName(), sig) {
                Attributes = MethodAttributes.CompilerControlled | MethodAttributes.Static,
                ImplAttributes = MethodImplAttributes.IL,
                Body = new CilBody()
            };
            item.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, (IField) field));
            for (int i = 0; i < item.Parameters.Count; i++)
            {
                item.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, item.Parameters[i]));
            }
            item.Body.Instructions.Add(Instruction.Create(OpCodes.Call, (IMethod) delegateType.FindMethod("Invoke")));
            item.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            delegateType.Methods.Add(item);
            ctx.Context.Registry.GetService<IMarkerService>().Mark(item, ctx.Protection);
            ctx.Name.SetCanRename(item, false);
            return item;
        }

        private FieldDef CreateField(RPContext ctx, TypeDef delegateType)
        {
            TypeDef def;
            do
            {
                def = ctx.Module.Types[ctx.Random.NextInt32(ctx.Module.Types.Count)];
            }
            while ((def.HasGenericParameters || def.IsGlobalModuleType) || def.IsDelegate());
            TypeSig type = new CModOptSig(def, delegateType.ToTypeSig());
            FieldDefUser item = new FieldDefUser("", new FieldSig(type), FieldAttributes.Assembly | FieldAttributes.Static);
            item.CustomAttributes.Add(new CustomAttribute(this.GetKeyAttr(ctx).FindInstanceConstructors().First<MethodDef>()));
            delegateType.Fields.Add(item);
            ctx.Marker.Mark(item, ctx.Protection);
            ctx.Name.SetCanRename(item, false);
            return item;
        }

        private void EncodeField(object sender, ModuleWriterListenerEventArgs e)
        {
            ModuleWriterBase base2 = (ModuleWriterBase) sender;
            if (e.WriterEvent == ModuleWriterEvent.MDMemberDefRidsAllocated)
            {
                Dictionary<TypeDef, Func<int, int>> dictionary = (from entry in this.keyAttrs
                    where entry != null
                    select entry).ToDictionary<Tuple<TypeDef, Func<int, int>>, TypeDef, Func<int, int>>(entry => entry.Item1, entry => entry.Item2);
                foreach (FieldDesc desc in this.fieldDescs)
                {
                    uint raw = base2.MetaData.GetToken(desc.Method).Raw;
                    uint num = this.encodeCtx.Random.NextUInt32() | 1;
                    CustomAttribute attribute = desc.Field.CustomAttributes[0];
                    int num3 = dictionary[(TypeDef) attribute.AttributeType]((int) MathsUtils.modInv(num));
                    attribute.ConstructorArguments.Add(new CAArgument(this.encodeCtx.Module.CorLibTypes.Int32, num3));
                    raw *= num;
                    raw = (uint) desc.InitDesc.Encoding.Encode(desc.InitDesc.Method, this.encodeCtx, (int) raw);
                    char[] chArray = new char[5];
                    chArray[desc.InitDesc.OpCodeIndex] = (char) (((byte) desc.OpCode) ^ desc.OpKey);
                    byte[] buffer = this.encodeCtx.Random.NextBytes(4);
                    uint num4 = 0;
                    int index = 0;
                    goto Label_01E1;
                Label_018D:
                    buffer[index] = this.encodeCtx.Random.NextByte();
                Label_01A2:
                    if (buffer[index] == 0)
                    {
                        goto Label_018D;
                    }
                    chArray[desc.InitDesc.TokenNameOrder[index]] = (char) buffer[index];
                    num4 |= (uint) (buffer[index] << desc.InitDesc.TokenByteOrder[index]);
                    index++;
                Label_01E1:
                    if (index < 4)
                    {
                        goto Label_01A2;
                    }
                    desc.Field.Name = new string(chArray);
                    FieldSig fieldSig = desc.Field.FieldSig;
                    uint num6 = (raw - base2.MetaData.GetToken(((CModOptSig) fieldSig.Type).Modifier).Raw) ^ num4;
                    byte[] buffer2 = new byte[8];
                    buffer2[0] = 0xc0;
                    buffer2[3] = (byte) (num6 >> desc.InitDesc.TokenByteOrder[3]);
                    buffer2[4] = 0xc0;
                    buffer2[5] = (byte) (num6 >> desc.InitDesc.TokenByteOrder[2]);
                    buffer2[6] = (byte) (num6 >> desc.InitDesc.TokenByteOrder[1]);
                    buffer2[7] = (byte) (num6 >> desc.InitDesc.TokenByteOrder[0]);
                    fieldSig.ExtraData = buffer2;
                }
            }
        }

        public override void Finalize(RPContext ctx)
        {
            foreach (KeyValuePair<Tuple<Code, IMethod, IRPEncoding>, Tuple<FieldDef, MethodDef>> pair in this.fields)
            {
                byte num;
                InitMethodDesc initMethod = this.GetInitMethod(ctx, pair.Key.Item3);
                do
                {
                    num = ctx.Random.NextByte();
                }
                while (num == ((byte) pair.Key.Item1));
                MethodDef def2 = pair.Value.Item1.DeclaringType.FindOrCreateStaticConstructor();
                def2.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, (IMethod) initMethod.Method));
                def2.Body.Instructions.Insert(0, Instruction.CreateLdcI4(num));
                def2.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldtoken, pair.Value.Item1));
                FieldDesc item = new FieldDesc {
                    Field = pair.Value.Item1,
                    OpCode = pair.Key.Item1,
                    Method = pair.Key.Item2,
                    OpKey = num,
                    InitDesc = initMethod
                };
                this.fieldDescs.Add(item);
            }
            foreach (TypeDef def3 in ctx.Delegates.Values)
            {
                MethodDef member = def3.FindOrCreateStaticConstructor();
                ctx.Marker.Mark(member, ctx.Protection);
                ctx.Name.SetCanRename(member, false);
            }
            MetaDataOptions metaDataOptions = ctx.Context.CurrentModuleWriterOptions.MetaDataOptions;
            metaDataOptions.Flags |= MetaDataFlags.PreserveExtraSignatureData;
            ctx.Context.CurrentModuleWriterListener.OnWriterEvent += new EventHandler<ModuleWriterListenerEventArgs>(this.EncodeField);
            this.encodeCtx = ctx;
        }

        private InitMethodDesc GetInitMethod(RPContext ctx, IRPEncoding encoding)
        {
            InitMethodDesc[] descArray;
            if (!this.inits.TryGetValue(encoding, out descArray))
            {
                this.inits[encoding] = descArray = new InitMethodDesc[ctx.InitCount];
            }
            int index = ctx.Random.NextInt32(descArray.Length);
            if (descArray[index] == null)
            {
                TypeDef runtimeType = ctx.Context.Registry.GetService<IRuntimeService>().GetRuntimeType("Confuser.Runtime.RefProxyStrong");
                MethodDef injectedMethod = InjectHelper.Inject(runtimeType.FindMethod("Initialize"), ctx.Module);
                ctx.Module.GlobalType.Methods.Add(injectedMethod);
                injectedMethod.Access = MethodAttributes.CompilerControlled;
                injectedMethod.Name = ctx.Name.RandomName();
                ctx.Name.SetCanRename(injectedMethod, false);
                ctx.Marker.Mark(injectedMethod, ctx.Protection);
                InitMethodDesc desc = new InitMethodDesc {
                    Method = injectedMethod
                };
                int[] list = Enumerable.Range(0, 5).ToArray<int>();
                ctx.Random.Shuffle<int>(list);
                desc.OpCodeIndex = list[4];
                desc.TokenNameOrder = new int[4];
                Array.Copy(list, 0, desc.TokenNameOrder, 0, 4);
                desc.TokenByteOrder = (from x in Enumerable.Range(0, 4) select x * 8).ToArray<int>();
                ctx.Random.Shuffle<int>(desc.TokenByteOrder);
                int[] destinationArray = new int[9];
                Array.Copy(desc.TokenNameOrder, 0, destinationArray, 0, 4);
                Array.Copy(desc.TokenByteOrder, 0, destinationArray, 4, 4);
                destinationArray[8] = desc.OpCodeIndex;
                MutationHelper.InjectKeys(injectedMethod, Enumerable.Range(0, 9).ToArray<int>(), destinationArray);
                MutationHelper.ReplacePlaceholder(injectedMethod, arg => encoding.EmitDecode(injectedMethod, ctx, arg));
                desc.Encoding = encoding;
                descArray[index] = desc;
            }
            return descArray[index];
        }

        private TypeDef GetKeyAttr(RPContext ctx)
        {
            if (this.keyAttrs == null)
            {
                this.keyAttrs = new Tuple<TypeDef, Func<int, int>>[0x10];
            }
            int index = ctx.Random.NextInt32(this.keyAttrs.Length);
            return this.keyAttrs[index]?.Item1;
        }

        private void ProcessBridge(RPContext ctx, int instrIndex)
        {
            Instruction instruction = ctx.Body.Instructions[instrIndex];
            IMethod operand = (IMethod) instruction.Operand;
            TypeDef def = operand.DeclaringType.ResolveTypeDefThrow();
            if (def.Module.IsILOnly && !def.IsGlobalModuleType)
            {
                Tuple<FieldDef, MethodDef> tuple2;
                Tuple<Code, IMethod, IRPEncoding> key = Tuple.Create<Code, IMethod, IRPEncoding>(instruction.OpCode.Code, operand, ctx.EncodingHandler);
                if (this.fields.TryGetValue(key, out tuple2))
                {
                    if (tuple2.Item2 != null)
                    {
                        instruction.OpCode = OpCodes.Call;
                        instruction.Operand = tuple2.Item2;
                        return;
                    }
                }
                else
                {
                    tuple2 = new Tuple<FieldDef, MethodDef>(null, null);
                }
                MethodSig sig = RPMode.CreateProxySignature(ctx, operand, instruction.OpCode.Code == Code.Newobj);
                TypeDef delegateType = RPMode.GetDelegateType(ctx, sig);
                if (tuple2.Item1 == null)
                {
                    tuple2 = new Tuple<FieldDef, MethodDef>(this.CreateField(ctx, delegateType), tuple2.Item2);
                }
                tuple2 = new Tuple<FieldDef, MethodDef>(tuple2.Item1, this.CreateBridge(ctx, delegateType, tuple2.Item1, sig));
                this.fields[key] = tuple2;
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = tuple2.Item2;
            }
        }

        public override void ProcessCall(RPContext ctx, int instrIndex)
        {
            Instruction instruction = ctx.Body.Instructions[instrIndex];
            TypeDef def = ((IMethod) instruction.Operand).DeclaringType.ResolveTypeDefThrow();
            if (def.Module.IsILOnly && !def.IsGlobalModuleType)
            {
                int num;
                int num2;
                instruction.CalculateStackUsage(out num, out num2);
                int? nullable = TraceBeginning(ctx, instrIndex, num2);
                if (!nullable.HasValue)
                {
                    this.ProcessBridge(ctx, instrIndex);
                }
                else
                {
                    this.ProcessInvoke(ctx, instrIndex, nullable.Value);
                }
            }
        }

        private void ProcessInvoke(RPContext ctx, int instrIndex, int argBeginIndex)
        {
            Tuple<FieldDef, MethodDef> tuple2;
            Instruction instruction = ctx.Body.Instructions[instrIndex];
            IMethod operand = (IMethod) instruction.Operand;
            MethodSig sig = RPMode.CreateProxySignature(ctx, operand, instruction.OpCode.Code == Code.Newobj);
            TypeDef delegateType = RPMode.GetDelegateType(ctx, sig);
            Tuple<Code, IMethod, IRPEncoding> key = Tuple.Create<Code, IMethod, IRPEncoding>(instruction.OpCode.Code, operand, ctx.EncodingHandler);
            if (!this.fields.TryGetValue(key, out tuple2))
            {
                tuple2 = new Tuple<FieldDef, MethodDef>(this.CreateField(ctx, delegateType), null);
                this.fields[key] = tuple2;
            }
            if (argBeginIndex == instrIndex)
            {
                ctx.Body.Instructions.Insert(instrIndex + 1, new Instruction(OpCodes.Call, delegateType.FindMethod("Invoke")));
                instruction.OpCode = OpCodes.Ldsfld;
                instruction.Operand = tuple2.Item1;
            }
            else
            {
                Instruction instruction2 = ctx.Body.Instructions[argBeginIndex];
                ctx.Body.Instructions.Insert(argBeginIndex + 1, new Instruction(instruction2.OpCode, instruction2.Operand));
                instruction2.OpCode = OpCodes.Ldsfld;
                instruction2.Operand = tuple2.Item1;
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = delegateType.FindMethod("Invoke");
            }
        }

        private static int? TraceBeginning(RPContext ctx, int index, int argCount)
        {
            if (ctx.BranchTargets.Contains(ctx.Body.Instructions[index]))
            {
                return null;
            }
            int num = argCount;
            int num2 = index;
            while (num > 0)
            {
                num2--;
                Instruction item = ctx.Body.Instructions[num2];
                if ((item.OpCode != OpCodes.Pop) && (item.OpCode != OpCodes.Dup))
                {
                    switch (item.OpCode.FlowControl)
                    {
                        case FlowControl.Break:
                        case FlowControl.Call:
                        case FlowControl.Meta:
                        case FlowControl.Next:
                            int num3;
                            int num4;
                            item.CalculateStackUsage(out num3, out num4);
                            num += num4;
                            num -= num3;
                            if (!ctx.BranchTargets.Contains(item) || (num == 0))
                            {
                                continue;
                            }
                            return null;
                    }
                }
                return null;
            }
            if (num < 0)
            {
                return null;
            }
            return new int?(num2);
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

        private class FieldDesc
        {
            public FieldDef Field;
            public StrongMode.InitMethodDesc InitDesc;
            public IMethod Method;
            public Code OpCode;
            public byte OpKey;
        }

        private class InitMethodDesc
        {
            public IRPEncoding Encoding;
            public MethodDef Method;
            public int OpCodeIndex;
            public int[] TokenByteOrder;
            public int[] TokenNameOrder;
        }
    }
}

