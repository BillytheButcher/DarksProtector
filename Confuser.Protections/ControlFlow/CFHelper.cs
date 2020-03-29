using Core.Helper.Generator.Context;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Confuser.Protections
{
    public class CFHelper
    {
        private GeneratorCtrl generator = new GeneratorCtrl();
        public bool HasUnsafeInstructions(MethodDef methodDef)
        {
            if (methodDef.HasBody)
            {
                if (methodDef.Body.HasVariables)
                    return methodDef.Body.Variables.Any(x => x.Type.IsPointer);
            }
            return false;
        }
        public Blocks GetBlocks(MethodDef method)
        {
            Blocks blocks = new Blocks();
            Block block = new Block();
            int Id = 0;
            int usage = 0;
            block.ID = Id;
            Id++;
            block.nextBlock = block.ID + 1;
            block.instructions.Add(Instruction.Create(OpCodes.Nop));
            blocks.blocks.Add(block);
            block = new Block();
            foreach (Instruction instruction in method.Body.Instructions)
            {
                int pops = 0;
                int stacks;
                instruction.CalculateStackUsage(out stacks, out pops);
                block.instructions.Add(instruction);
                usage += stacks - pops;
                if (stacks == 0)
                {
                    if (instruction.OpCode != OpCodes.Nop)
                    {
                        if (usage == 0 || instruction.OpCode == OpCodes.Ret)
                        {
                            
                            block.ID = Id;
                            Id++;
                            block.nextBlock = block.ID + 1;
                            blocks.blocks.Add(block);
                            block = new Block();
                        }
                    }
                }
            }
            return blocks;
        }
        public List<Instruction> Calc(int value)
        {
            List<Instruction> instructions = new List<Instruction>();
            int num = generator.Generate<int>(GeneratorType.Integer, 100000);
            bool once = Convert.ToBoolean(generator.Generate<int>(GeneratorType.Integer, 2));
            int num1 = generator.Generate<int>(GeneratorType.Integer, 100000);
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, value - num + (once ? (0 - num1) : num1)));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, num));
            instructions.Add(Instruction.Create(OpCodes.Add));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, num1));
            instructions.Add(Instruction.Create(once ? OpCodes.Add : OpCodes.Sub));
            return instructions;
        }
    }
}
