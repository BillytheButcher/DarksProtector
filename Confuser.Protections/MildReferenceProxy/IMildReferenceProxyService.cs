namespace Confuser.Protections
{
    using Confuser.Core;
    using dnlib.DotNet;
    using System;

    public interface IMildReferenceProxyService
    {
        void ExcludeMethod(ConfuserContext context, MethodDef method);
    }
}

