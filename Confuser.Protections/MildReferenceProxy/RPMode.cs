namespace Confuser.Protections.MildReferenceProxy
{
    using Confuser.Core;
    using Confuser.Renamer;
    using Confuser.Renamer.References;
    using dnlib.DotNet;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class RPMode
    {
        protected RPMode()
        {
        }

        protected static MethodSig CreateProxySignature(RPContext ctx, IMethod method, bool newObj)
        {
            Func<TypeSig, TypeSig> selector = null;
            Func<TypeSig, TypeSig> func2 = null;
            ModuleDef module = ctx.Module;
            if (newObj)
            {
                TypeSig sig;
                if (selector == null)
                {
                    selector = delegate (TypeSig type) {
                        if ((ctx.TypeErasure && type.IsClassSig) && method.MethodSig.HasThis)
                        {
                            return module.CorLibTypes.Object;
                        }
                        return type;
                    };
                }
                TypeSig[] argTypes = method.MethodSig.Params.Select<TypeSig, TypeSig>(selector).ToArray<TypeSig>();
                if (ctx.TypeErasure)
                {
                    sig = module.CorLibTypes.Object;
                }
                else
                {
                    TypeDef typeDef = method.DeclaringType.ResolveTypeDefThrow();
                    sig = Import(ctx, typeDef).ToTypeSig();
                }
                return MethodSig.CreateStatic(sig, argTypes);
            }
            if (func2 == null)
            {
                func2 = delegate (TypeSig type) {
                    if ((ctx.TypeErasure && type.IsClassSig) && method.MethodSig.HasThis)
                    {
                        return module.CorLibTypes.Object;
                    }
                    return type;
                };
            }
            IEnumerable<TypeSig> second = method.MethodSig.Params.Select<TypeSig, TypeSig>(func2);
            if (method.MethodSig.HasThis && !method.MethodSig.ExplicitThis)
            {
                TypeDef def2 = method.DeclaringType.ResolveTypeDefThrow();
                if (ctx.TypeErasure && !def2.IsValueType)
                {
                    second = new CorLibTypeSig[] { module.CorLibTypes.Object }.Concat<TypeSig>(second);
                }
                else
                {
                    second = new TypeSig[] { Import(ctx, def2).ToTypeSig() }.Concat<TypeSig>(second);
                }
            }
            TypeSig retType = method.MethodSig.RetType;
            if (ctx.TypeErasure && retType.IsClassSig)
            {
                retType = module.CorLibTypes.Object;
            }
            return MethodSig.CreateStatic(retType, second.ToArray<TypeSig>());
        }

        public abstract void Finalize(RPContext ctx);
        protected static TypeDef GetDelegateType(RPContext ctx, MethodSig sig)
        {
            TypeDef def;
            if (!ctx.Delegates.TryGetValue(sig, out def))
            {
                def = new TypeDefUser(ctx.Name.RandomName(), ctx.Name.RandomName(), ctx.Module.CorLibTypes.GetTypeRef("System", "MulticastDelegate")) {
                    Attributes = TypeAttributes.AnsiClass | TypeAttributes.Sealed
                };
                MethodDefUser item = new MethodDefUser(".ctor", MethodSig.CreateInstance(ctx.Module.CorLibTypes.Void, ctx.Module.CorLibTypes.Object, ctx.Module.CorLibTypes.IntPtr)) {
                    Attributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName,
                    ImplAttributes = MethodImplAttributes.CodeTypeMask
                };
                def.Methods.Add(item);
                MethodDefUser user2 = new MethodDefUser("Invoke", sig.Clone()) {
                    MethodSig = { HasThis = true },
                    Attributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                    ImplAttributes = MethodImplAttributes.CodeTypeMask
                };
                def.Methods.Add(user2);
                ctx.Module.Types.Add(def);
                foreach (IDnlibDef def2 in def.FindDefinitions())
                {
                    ctx.Marker.Mark(def2, ctx.Protection);
                    ctx.Name.SetCanRename(def2, false);
                }
                ctx.Delegates[sig] = def;
            }
            return def;
        }

        private static ITypeDefOrRef Import(RPContext ctx, TypeDef typeDef)
        {
            ITypeDefOrRef ref2 = new Importer(ctx.Module, ImporterOptions.TryToUseTypeDefs).Import(typeDef);
            if ((typeDef.Module != ctx.Module) && ctx.Context.Modules.Contains((ModuleDefMD) typeDef.Module))
            {
                ctx.Name.AddReference<TypeDef>(typeDef, new TypeRefReference((TypeRef) ref2, typeDef));
            }
            return ref2;
        }

        public abstract void ProcessCall(RPContext ctx, int instrIndex);
    }
}

