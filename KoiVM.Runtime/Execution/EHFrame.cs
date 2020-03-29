﻿#region

using System;

#endregion

namespace KoiVM.Runtime.Execution
{
    internal struct EHFrame
    {
        public byte EHType;
        public ulong FilterAddr;
        public ulong HandlerAddr;
        public Type CatchType;

        public DarksVMSlot BP;
        public DarksVMSlot SP;
    }
}