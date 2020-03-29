using System;
using System.Collections.Generic;
using System.Linq;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    // Token: 0x02000074 RID: 116


    internal class HideMethodsProc : Protection
    {
        // Token: 0x060001E9 RID: 489 RVA: 0x00011C8C File Offset: 0x0000FE8C
        protected override void Initialize(ConfuserContext context)
        {
        }

        // Token: 0x060001EA RID: 490 RVA: 0x00011C8E File Offset: 0x0000FE8E
        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {

            pipeline.InsertPreStage(PipelineStage.EndModule, new HideMethodsPhase(this));



        }

        // Token: 0x17000053 RID: 83
        public override string Description
        {
            // Token: 0x060001E5 RID: 485 RVA: 0x00011C74 File Offset: 0x0000FE74
            get
            {
                return "This protection hides methods.";
            }
        }

        // Token: 0x17000055 RID: 85
        public override string FullId
        {
            // Token: 0x060001E7 RID: 487 RVA: 0x00011C82 File Offset: 0x0000FE82
            get
            {
                return "Ki.HideMethods";
            }
        }

        // Token: 0x17000054 RID: 84
        public override string Id
        {
            // Token: 0x060001E6 RID: 486 RVA: 0x00011C7B File Offset: 0x0000FE7B
            get
            {
                return "Hide Methods";
            }
        }

        // Token: 0x17000052 RID: 82
        public override string Name
        {
            // Token: 0x060001E4 RID: 484 RVA: 0x00011C6D File Offset: 0x0000FE6D
            get
            {
                return "Hide Methods";
            }
        }

        // Token: 0x17000056 RID: 86
        public override ProtectionPreset Preset
        {
            // Token: 0x060001E8 RID: 488 RVA: 0x00011C89 File Offset: 0x0000FE89
            get
            {
                return ProtectionPreset.Normal;
            }
        }

        // Token: 0x04000174 RID: 372
        public const string _FullId = "Ki.HideMethods";

        // Token: 0x04000173 RID: 371
        public const string _Id = "Hide Methods";

        // Token: 0x02000075 RID: 117
        private class HideMethodsPhase : ProtectionPhase
        {
            // Token: 0x060001EC RID: 492 RVA: 0x00011CA5 File Offset: 0x0000FEA5
            public HideMethodsPhase(HideMethodsProc parent) : base(parent)
            {
            }

            // Token: 0x060001EF RID: 495 RVA: 0x00011CCC File Offset: 0x0000FECC
            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {

                foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
                {
                    foreach (var type in module.Types)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (method == module.EntryPoint)
                            {
                                Local local1 = new Local(module.Import(typeof(int)).ToTypeSig());
                                Local local2 = new Local(module.Import(typeof(bool)).ToTypeSig());

                                method.Body.Variables.Add(local1);
                                method.Body.Variables.Add(local2);

                                Instruction operand = null;
                                Instruction instruction = new Instruction(OpCodes.Ret);
                                Instruction instruction2 = new Instruction(OpCodes.Ldc_I4_1);

                                method.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_0));
                                method.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, local1));
                                method.Body.Instructions.Insert(2, new Instruction(OpCodes.Br, instruction2));

                                Instruction instruction3 = new Instruction(OpCodes.Ldloc, local1);

                                method.Body.Instructions.Insert(3, instruction3);
                                method.Body.Instructions.Insert(4, new Instruction(OpCodes.Ldc_I4_0));
                                method.Body.Instructions.Insert(5, new Instruction(OpCodes.Ceq));
                                method.Body.Instructions.Insert(6, new Instruction(OpCodes.Ldc_I4_1));
                                method.Body.Instructions.Insert(7, new Instruction(OpCodes.Ceq));
                                method.Body.Instructions.Insert(8, new Instruction(OpCodes.Stloc, local2));
                                method.Body.Instructions.Insert(9, new Instruction(OpCodes.Ldloc, local2));
                                method.Body.Instructions.Insert(10, new Instruction(OpCodes.Brtrue, method.Body.Instructions[sizeof(Decimal) - 6]));
                                method.Body.Instructions.Insert(11, new Instruction(OpCodes.Ret));
                                method.Body.Instructions.Insert(12, new Instruction(OpCodes.Calli));
                                method.Body.Instructions.Insert(13, new Instruction(OpCodes.Sizeof, operand));
                                method.Body.Instructions.Insert(method.Body.Instructions.Count, instruction2);
                                method.Body.Instructions.Insert(method.Body.Instructions.Count, new Instruction(OpCodes.Stloc, local2));
                                method.Body.Instructions.Insert(method.Body.Instructions.Count, new Instruction(OpCodes.Br, instruction3));
                                method.Body.Instructions.Insert(method.Body.Instructions.Count, instruction);

                                ExceptionHandler item2 = new ExceptionHandler(ExceptionHandlerType.Finally)
                                {
                                    HandlerStart = method.Body.Instructions[10],
                                    HandlerEnd = method.Body.Instructions[11],
                                    TryEnd = method.Body.Instructions[14],
                                    TryStart = method.Body.Instructions[12]
                                };

                                bool flag3 = !method.Body.HasExceptionHandlers;

                                if (flag3)
                                {
                                    method.Body.ExceptionHandlers.Add(item2);
                                }

                                operand = new Instruction(OpCodes.Br, instruction);
                                method.Body.OptimizeBranches();
                                method.Body.OptimizeMacros();
                            }
                        }

                    }
                }
            }

            // Token: 0x17000058 RID: 88
            public override string Name
            {
                // Token: 0x060001EE RID: 494 RVA: 0x00011CB2 File Offset: 0x0000FEB2
                get
                {
                    return "Hiding Methods";
                }
            }

            // Token: 0x17000057 RID: 87
            public override ProtectionTargets Targets
            {
                // Token: 0x060001ED RID: 493 RVA: 0x00011CAE File Offset: 0x0000FEAE
                get
                {
                    return ProtectionTargets.Modules;
                }
            }
        }
    }
}
