﻿#region

using System.Reflection;

#endregion

namespace KoiVM.Runtime.Data
{
    internal struct DarksVMExportInfo
    {
        public unsafe DarksVMExportInfo(ref byte* ptr, Module module)
        {
            CodeOffset = *(uint*) ptr;
            ptr += 4;
            if(CodeOffset != 0)
            {
                EntryKey = *(uint*) ptr;
                ptr += 4;
            }
            else
            {
                EntryKey = 0;
            }
            Signature = new DarksVMFuncSig(ref ptr, module);
        }

        public readonly uint CodeOffset;
        public readonly uint EntryKey;
        public readonly DarksVMFuncSig Signature;
    }
}