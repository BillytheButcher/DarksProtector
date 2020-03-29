using System;
using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    [BeforeProtection(new string[]
    {
        "Ki.ControlFlow"
    })]
    internal class StackUnfConfusion : Protection
    {
        protected override void Initialize(ConfuserContext context)
        {

        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new StackUnfConfusion.StackUnfConfusionPhase(this));
        }
        public override string Description
        {
            get
            {
                return "This confusion will add a piece of code in the front of the methods.";
            }
        }
        
        public override string FullId
        {
            get
            {
                return "Ki.StackUn";
            }
        }
        public override string Id
        {
            get
            {
                return "stack underflow";
            }
        }
        
        public override string Name
        {
            get
            {
                return "Stack Underflow Confusion";
            }
        }
        
        public override ProtectionPreset Preset
        {
            get
            {
                return ProtectionPreset.Minimum;
            }
        }
        
        public const string _FullId = "Ki.StackUn";

        public const string _Id = "stack underflow";

       
        private class StackUnfConfusionPhase : ProtectionPhase
        {
            public StackUnfConfusionPhase(StackUnfConfusion parent) : base(parent)
            {
            }
            
            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                foreach (IDnlibDef dnlibDef in parameters.Targets)
                {
                    MethodDef def = (MethodDef)dnlibDef;
                    if (def != null && !def.HasBody)
                    {
                        break;
                    }
                    CilBody body = def.Body;
                    Instruction target = body.Instructions[0];
                    Instruction item = Instruction.Create(OpCodes.Br_S, target);
                    Instruction instruction3 = Instruction.Create(OpCodes.Pop);
                    Random random = new Random();
                    Instruction instruction4;
                    switch (random.Next(0, 5))
                    {
                        case 0:
                            instruction4 = Instruction.Create(OpCodes.Ldnull);
                            break;
                        case 1:
                            instruction4 = Instruction.Create(OpCodes.Ldc_I4_0);
                            break;
                        case 2:
                            instruction4 = Instruction.Create(OpCodes.Ldstr, "calli");
                            break;
                        case 3:
                            instruction4 = Instruction.Create(OpCodes.Ldc_I8, (uint)random.Next());
                            break;
                        default:
                            instruction4 = Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
                            break;
                    }
                    body.Instructions.Insert(0, instruction4);
                    body.Instructions.Insert(1, instruction3);
                    body.Instructions.Insert(2, item);
                    foreach (ExceptionHandler handler in body.ExceptionHandlers)
                    {
                        if (handler.TryStart == target)
                        {
                            handler.TryStart = item;
                        }
                        else if (handler.HandlerStart == target)
                        {
                            handler.HandlerStart = item;
                        }
                        else if (handler.FilterStart == target)
                        {
                            handler.FilterStart = item;
                        }
                    }
                }
            }
            
            public override string Name
            {
                get
                {
                    return "Stack Underflow Confusion";
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
