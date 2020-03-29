﻿#region

using System.Reflection;

#endregion

namespace KoiVM.VM
{
    [Obfuscation(Exclude = false, ApplyToMembers = false, Feature = "+rename(forceRen=true);")]
    public enum DarksVMFlags
    {
        OVERFLOW = 0,
        CARRY = 1,
        ZERO = 2,
        SIGN = 3,
        UNSIGNED = 4,
        BEHAV1 = 5,
        BEHAV2 = 6,
        BEHAV3 = 7,

        Max
    }
}