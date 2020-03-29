using System;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    [AfterProtection(new string[]
    {
        "Ki.Constants"
    })]
    internal class DisConstConfusion : Protection
    {
        protected override void Initialize(ConfuserContext context)
        {
        }


        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new DisConstConfusion.DisConstConfusionPhase(this));
        }
        
        public override string Description
        {
            get
            {
                return "This protection disintegrate the constants in the code into expressions.";
            }
        }
        
        public override string FullId
        {
            get
            {
                return "Ki.disconstant";
            }
        }
        
        public override string Id
        {
            get
            {
                return "Const disint";
            }
        }
        
        public override string Name
        {
            get
            {
                return "Constant Disintegration";
            }
        }
        
        public override ProtectionPreset Preset
        {
            get
            {
                return ProtectionPreset.Normal;
            }
        }
        
        public const string _FullId = "Ki.disconstant";
        
        public const string _Id = "Const disint";
        
        private class DisConstConfusionPhase : ProtectionPhase
        {
            public DisConstConfusionPhase(DisConstConfusion parent) : base(parent)
            {
            }
            
            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                for (int i = 1; i < 1; i++)
                {
                    foreach (IDnlibDef dnlibDef in parameters.Targets)
                    {
                        MethodDef def = (MethodDef)dnlibDef;
                        CilBody body = def.Body;
                        body.SimplifyBranches();
                        Random random = new Random();
                        int num2 = 0;
                        while (num2 < body.Instructions.Count)
                        {
                            if (body.Instructions[num2].IsLdcI4())
                            {
                                int num3 = body.Instructions[num2].GetLdcI4Value();
                                int num4 = random.Next(5, 40);
                                body.Instructions[num2].OpCode = OpCodes.Ldc_I4;
                                body.Instructions[num2].Operand = num4 * num3;
                                body.Instructions.Insert(num2 + 1, Instruction.Create(OpCodes.Ldc_I4, num4));
                                body.Instructions.Insert(num2 + 2, Instruction.Create(OpCodes.Div));
                                num2 += 3;
                            }
                            else
                            {
                                num2++;
                            }
                        }
                        Random random2 = new Random();
                        int num5 = 0;
                        ITypeDefOrRef type = null;
                        for (int j = 0; j < def.Body.Instructions.Count; j++)
                        {
                            Instruction instruction = def.Body.Instructions[j];
                            if (instruction.IsLdcI4())
                            {
                                switch (random2.Next(1, 8))
                                {
                                    case 1:
                                        type = def.Module.Import(typeof(int));
                                        num5 = 4;
                                        break;
                                    case 2:
                                        type = def.Module.Import(typeof(sbyte));
                                        num5 = 1;
                                        break;
                                    case 3:
                                        type = def.Module.Import(typeof(byte));
                                        num5 = 1;
                                        break;
                                    case 4:
                                        type = def.Module.Import(typeof(bool));
                                        num5 = 1;
                                        break;
                                    case 5:
                                        type = def.Module.Import(typeof(decimal));
                                        num5 = 16;
                                        break;
                                    case 6:
                                        type = def.Module.Import(typeof(short));
                                        num5 = 2;
                                        break;
                                    case 7:
                                        type = def.Module.Import(typeof(long));
                                        num5 = 8;
                                        break;
                                }
                                int num6 = random2.Next(1, 1000);
                                bool flag = Convert.ToBoolean(random2.Next(0, 2));
                                switch ((num5 != 0) ? ((Convert.ToInt32(instruction.Operand) % num5 == 0) ? random2.Next(1, 5) : random2.Next(1, 4)) : random2.Next(1, 4))
                                {
                                    case 1:
                                        def.Body.Instructions.Insert(j + 1, Instruction.Create(OpCodes.Sizeof, type));
                                        def.Body.Instructions.Insert(j + 2, Instruction.Create(OpCodes.Add));
                                        instruction.Operand = Convert.ToInt32(instruction.Operand) - num5 + (flag ? (-num6) : num6);
                                        goto IL_43A;
                                    case 2:
                                        def.Body.Instructions.Insert(j + 1, Instruction.Create(OpCodes.Sizeof, type));
                                        def.Body.Instructions.Insert(j + 2, Instruction.Create(OpCodes.Sub));
                                        instruction.Operand = Convert.ToInt32(instruction.Operand) + num5 + (flag ? (-num6) : num6);
                                        goto IL_43A;
                                    case 3:
                                        def.Body.Instructions.Insert(j + 1, Instruction.Create(OpCodes.Sizeof, type));
                                        def.Body.Instructions.Insert(j + 2, Instruction.Create(OpCodes.Add));
                                        instruction.Operand = Convert.ToInt32(instruction.Operand) - num5 + (flag ? (-num6) : num6);
                                        goto IL_43A;
                                    case 4:
                                        def.Body.Instructions.Insert(j + 1, Instruction.Create(OpCodes.Sizeof, type));
                                        def.Body.Instructions.Insert(j + 2, Instruction.Create(OpCodes.Mul));
                                        instruction.Operand = Convert.ToInt32(instruction.Operand) / num5;
                                        break;
                                    default:
                                        goto IL_43A;
                                }
                            IL_484:
                                j += 2;
                                goto IL_48A;
                            IL_43A:
                                def.Body.Instructions.Insert(j + 3, Instruction.CreateLdcI4(num6));
                                def.Body.Instructions.Insert(j + 4, Instruction.Create(flag ? OpCodes.Add : OpCodes.Sub));
                                j += 2;
                                goto IL_484;
                            }
                        IL_48A:;
                        }
                        body.OptimizeBranches();
                    }
                }
            }
            
            public override string Name
            {
                get
                {
                    return "Constant Disintegration";
                }
            }
            
            public override ProtectionTargets Targets
            {
                get
                {
                    return ProtectionTargets.Methods;
                }
            }
        }
    }
}
