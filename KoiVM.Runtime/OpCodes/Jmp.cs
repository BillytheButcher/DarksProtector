﻿#region

using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.OpCodes
{
    internal class Jmp : IOpCode
    {
        public byte Code => DarksVMConstants.OP_JMP;

        public void Load(DarksVMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[DarksVMConstants.REG_SP].U4;
            var slot = ctx.Stack[sp];
            ctx.Stack.SetTopPosition(--sp);
            ctx.Registers[DarksVMConstants.REG_SP].U4 = sp;

            ctx.Registers[DarksVMConstants.REG_IP].U8 = slot.U8;
            state = ExecutionState.Next;
        }
    }

    internal class Jz : IOpCode
    {
        public byte Code => DarksVMConstants.OP_JZ;

        public void Load(DarksVMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[DarksVMConstants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp];
            var valSlot = ctx.Stack[sp - 1];
            sp -= 2;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[DarksVMConstants.REG_SP].U4 = sp;

            if(valSlot.U8 == 0)
                ctx.Registers[DarksVMConstants.REG_IP].U8 = adrSlot.U8;
            state = ExecutionState.Next;
        }
    }

    internal class Jnz : IOpCode
    {
        public byte Code => DarksVMConstants.OP_JNZ;

        public void Load(DarksVMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[DarksVMConstants.REG_SP].U4;
            var adrSlot = ctx.Stack[sp];
            var valSlot = ctx.Stack[sp - 1];
            sp -= 2;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[DarksVMConstants.REG_SP].U4 = sp;

            if(valSlot.U8 != 0)
                ctx.Registers[DarksVMConstants.REG_IP].U8 = adrSlot.U8;
            state = ExecutionState.Next;
        }
    }

    internal class Swt : IOpCode
    {
        public byte Code => DarksVMConstants.OP_SWT;

        public unsafe void Load(DarksVMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[DarksVMConstants.REG_SP].U4;
            var tblSlot = ctx.Stack[sp];
            var valSlot = ctx.Stack[sp - 1];
            sp -= 2;
            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[DarksVMConstants.REG_SP].U4 = sp;

            var index = valSlot.U4;
            var len = *(ushort*) (tblSlot.U8 - 2);
            if(index < len)
                ctx.Registers[DarksVMConstants.REG_IP].U8 += (ulong) (int) ((uint*) tblSlot.U8)[index];
            state = ExecutionState.Next;
        }
    }
}