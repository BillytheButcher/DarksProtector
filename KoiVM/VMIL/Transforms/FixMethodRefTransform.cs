﻿#region

using System.Collections.Generic;
using KoiVM.AST.IL;
using KoiVM.VM;

#endregion

namespace KoiVM.VMIL.Transforms
{
    public class FixMethodRefTransform : IPostTransform
    {
        private HashSet<DarksVMRegisters> saveRegs;

        public void Initialize(ILPostTransformer tr)
        {
            saveRegs = tr.Runtime.Descriptor.Data.LookupInfo(tr.Method).UsedRegister;
        }

        public void Transform(ILPostTransformer tr)
        {
            tr.Instructions.VisitInstrs(VisitInstr, tr);
        }

        private void VisitInstr(ILInstrList instrs, ILInstruction instr, ref int index, ILPostTransformer tr)
        {
            var rel = instr.Operand as ILRelReference;
            if(rel == null)
                return;

            var methodRef = rel.Target as ILMethodTarget;
            if(methodRef == null)
                return;

            methodRef.Resolve(tr.Runtime);
        }
    }
}