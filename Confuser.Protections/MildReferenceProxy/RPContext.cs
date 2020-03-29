namespace Confuser.Protections.MildReferenceProxy
{
    using Confuser.Core;
    using Confuser.Core.Services;
    using Confuser.DynCipher;
    using Confuser.Protections;
    using Confuser.Renamer;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using System;
    using System.Collections.Generic;

    internal class RPContext
    {
        public CilBody Body;
        public HashSet<Instruction> BranchTargets;
        public ConfuserContext Context;
        public Dictionary<MethodSig, TypeDef> Delegates;
        public int Depth;
        public IDynCipherService DynCipher;
        public EncodingType Encoding;
        public IRPEncoding EncodingHandler;
        public int InitCount;
        public bool InternalAlso;
        public IMarkerService Marker;
        public MethodDef Method;
        public Confuser.Protections.MildReferenceProxy.Mode Mode;
        public RPMode ModeHandler;
        public ModuleDef Module;
        public NameService Name;
        public MildReferenceProxyProtection Protection;
        public RandomGenerator Random;
        public bool TypeErasure;
    }
}

