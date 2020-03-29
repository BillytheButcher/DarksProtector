using Confuser.Core;
using Core.Helper.DnlibUtils2;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;

namespace Confuser.Protections.ControlFlow
{
    internal class ControlFlowPhase : ProtectionPhase
    {


        public ControlFlowPhase(ControlFlowProtection parent)
            : base(parent) { }

        public override ProtectionTargets Targets
        {
            get { return ProtectionTargets.Modules; }
        }

        public override string Name
        {
            get { return "Control Flow"; }
        }





        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {

            foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
            {
                CFHelper cFHelper = new CFHelper();
                foreach (TypeDef type in module.Types)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.Instructions.Count > 0 && !method.IsConstructor)
                        {
                            if (!cFHelper.HasUnsafeInstructions(method))
                            {
                                if (DnlibUtils2.Simplify(method))
                                {
                                    Blocks blocks = cFHelper.GetBlocks(method);
                                    if (blocks.blocks.Count != 1)
                                    {
                                        Run(cFHelper, method, blocks, context);
                                    }
                                    DnlibUtils2.Optimize(method);
                                }
                            }
                        }

                    }
                }
            }
        }
        public void Run(CFHelper cFHelper, MethodDef method, Blocks blocks, ConfuserContext context)
        {
            blocks.Scramble(out blocks);
            method.Body.Instructions.Clear();
            Local local = new Local(context.CurrentModule.CorLibTypes.Int32);
            method.Body.Variables.Add(local);
            Instruction target = Instruction.Create(OpCodes.Nop);
            Instruction instr = Instruction.Create(OpCodes.Br, target);
            foreach (Instruction instruction in cFHelper.Calc(0))
            {
                method.Body.Instructions.Add(instruction);
            }

            method.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, instr));
            method.Body.Instructions.Add(target);
            foreach (Block block in blocks.blocks)
            {
                if (block != blocks.getBlock((blocks.blocks.Count - 1)))
                {
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
                    foreach (Instruction instruction in cFHelper.Calc(block.ID))
                    {
                        method.Body.Instructions.Add(instruction);
                    }

                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
                    Instruction instruction4 = Instruction.Create(OpCodes.Nop);
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instruction4));
                    foreach (Instruction instruction in block.instructions)
                    {
                        method.Body.Instructions.Add(instruction);
                    }

                    foreach (Instruction instruction in cFHelper.Calc(block.nextBlock))
                    {
                        method.Body.Instructions.Add(instruction);
                    }

                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc, local));
                    method.Body.Instructions.Add(instruction4);
                }
            }
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, local));
            foreach (Instruction instruction in cFHelper.Calc(blocks.blocks.Count - 1))
            {
                method.Body.Instructions.Add(instruction);
            }

            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse, instr));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Br, blocks.getBlock((blocks.blocks.Count - 1)).instructions[0]));
            method.Body.Instructions.Add(instr);
            foreach (Instruction lastBlock in blocks.getBlock((blocks.blocks.Count - 1)).instructions)
            {
                method.Body.Instructions.Add(lastBlock);
            }
        }

    }
}