﻿#region

using System;
using System.Reflection;
using KoiVM.Runtime.Dynamic;
using KoiVM.Runtime.Execution;

#endregion

namespace KoiVM.Runtime.VCalls
{
    internal class Ldfld : IVCall
    {
        public byte Code => DarksVMConstants.VCALL_LDFLD;

        public void Load(DarksVMContext ctx, out ExecutionState state)
        {
            var sp = ctx.Registers[DarksVMConstants.REG_SP].U4;
            var fieldSlot = ctx.Stack[sp--];
            var objSlot = ctx.Stack[sp];

            var addr = (fieldSlot.U4 & 0x80000000) != 0;
            var field = (FieldInfo) ctx.Instance.Data.LookupReference(fieldSlot.U4 & 0x7fffffff);
            if(!field.IsStatic && objSlot.O == null)
                throw new NullReferenceException();

            if(addr)
            {
                ctx.Stack[sp] = new DarksVMSlot {O = new FieldRef(objSlot.O, field)};
            }
            else
            {
                object instance;
                if(field.DeclaringType.IsValueType && objSlot.O is IReference)
                    instance = ((IReference) objSlot.O).GetValue(ctx, PointerType.OBJECT).ToObject(field.DeclaringType);
                else
                    instance = objSlot.ToObject(field.DeclaringType);
                ctx.Stack[sp] = DarksVMSlot.FromObject(field.GetValue(instance), field.FieldType);
            }

            ctx.Stack.SetTopPosition(sp);
            ctx.Registers[DarksVMConstants.REG_SP].U4 = sp;
            state = ExecutionState.Next;
        }
    }
}