﻿#region

using System.Reflection;

#endregion

namespace KoiVM.VM
{
    [Obfuscation(Exclude = false, ApplyToMembers = false, Feature = "+rename(forceRen=true);")]
    public enum DarksVMCalls
    {
        EXIT = 0,
        BREAK = 1,
        ECALL = 2,
        CAST = 3,
        CKFINITE = 4,
        CKOVERFLOW = 5,
        RANGECHK = 6,
        INITOBJ = 7,
        LDFLD = 8,
        LDFTN = 9,
        TOKEN = 10,
        THROW = 11,
        SIZEOF = 12,
        STFLD = 13,
        BOX = 14,
        UNBOX = 15,
        LOCALLOC = 16,

        Max
    }
}