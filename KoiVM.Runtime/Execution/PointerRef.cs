﻿#region

using System;
using KoiVM.Runtime.Execution.Internal;

#endregion

namespace KoiVM.Runtime.Execution
{
    internal unsafe class PointerRef : IReference
    {
        // Only for typed reference use

        private readonly void* ptr;

        public PointerRef(void* ptr)
        {
            this.ptr = ptr;
        }

        public DarksVMSlot GetValue(DarksVMContext ctx, PointerType type)
        {
            throw new NotSupportedException();
        }

        public void SetValue(DarksVMContext ctx, DarksVMSlot slot, PointerType type)
        {
            throw new NotSupportedException();
        }

        public IReference Add(uint value)
        {
            throw new NotSupportedException();
        }

        public IReference Add(ulong value)
        {
            throw new NotSupportedException();
        }

        public void ToTypedReference(DarksVMContext ctx, TypedRefPtr typedRef, Type type)
        {
            TypedReferenceHelpers.MakeTypedRef(ptr, typedRef, type);
        }
    }
}