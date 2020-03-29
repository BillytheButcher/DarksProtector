﻿#region

using KoiVM.CFG;

#endregion

namespace KoiVM.AST.IR
{
    public class IRBlockTarget : IIROperand
    {
        public IRBlockTarget(IBasicBlock target)
        {
            Target = target;
        }

        public IBasicBlock Target
        {
            get;
            set;
        }

        public ASTType Type => ASTType.Ptr;

        public override string ToString()
        {
            return string.Format("Block_{0:x2}", Target.Id);
        }
    }
}