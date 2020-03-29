﻿#region

using KoiVM.AST.IR;
using KoiVM.VMIR.RegAlloc;

#endregion

namespace KoiVM.VMIR.Transforms
{
    public class StackFrameTransform : ITransform
    {
        private RegisterAllocator allocator;
        private bool doneEntry, doneExit;

        public void Initialize(IRTransformer tr)
        {
            allocator = (RegisterAllocator) tr.Annotations[RegisterAllocationTransform.RegAllocatorKey];
        }

        public void Transform(IRTransformer tr)
        {
            tr.Instructions.VisitInstrs(VisitInstr, tr);
        }

        private void VisitInstr(IRInstrList instrs, IRInstruction instr, ref int index, IRTransformer tr)
        {
            if(instr.OpCode == IROpCode.__ENTRY && !doneEntry)
            {
                instrs.Replace(index, new[]
                {
                    instr,
                    new IRInstruction(IROpCode.PUSH, IRRegister.BP),
                    new IRInstruction(IROpCode.MOV, IRRegister.BP, IRRegister.SP),
                    new IRInstruction(IROpCode.ADD, IRRegister.SP, IRConstant.FromI4(allocator.LocalSize))
                });
                doneEntry = true;
            }
            else if(instr.OpCode == IROpCode.__EXIT && !doneExit)
            {
                instrs.Replace(index, new[]
                {
                    new IRInstruction(IROpCode.MOV, IRRegister.SP, IRRegister.BP),
                    new IRInstruction(IROpCode.POP, IRRegister.BP),
                    instr
                });
                doneExit = true;
            }
        }
    }
}