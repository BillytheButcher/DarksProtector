namespace Confuser.Protections.MildReferenceProxy
{
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using System;

    internal interface IRPEncoding
    {
        Instruction[] EmitDecode(MethodDef init, RPContext ctx, Instruction[] arg);
        int Encode(MethodDef init, RPContext ctx, int value);
    }
}

