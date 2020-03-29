﻿#region

using KoiVM.AST.IL;
using KoiVM.AST.IR;
using KoiVM.VMIR;

#endregion

namespace KoiVM.VMIL.Translation
{
    public class VcallHandler : ITranslationHandler
    {
        public IROpCode IRCode => IROpCode.VCALL;

        public void Translate(IRInstruction instr, ILTranslator tr)
        {
            if(instr.Operand2 != null)
                tr.PushOperand(instr.Operand2);
            tr.PushOperand(instr.Operand1);
            tr.Instructions.Add(new ILInstruction(ILOpCode.VCALL));
        }
    }

    public class NopHandler : ITranslationHandler
    {
        public IROpCode IRCode => IROpCode.NOP;

        public void Translate(IRInstruction instr, ILTranslator tr)
        {
            tr.Instructions.Add(new ILInstruction(ILOpCode.NOP));
        }
    }
}