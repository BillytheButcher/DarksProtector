using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper.DnlibUtils2
{
    class DnlibUtils2
    {
        public static IEnumerable<IDnlibDef> FindDefinitions(ModuleDef module)
        {
            yield return module;
            foreach (TypeDef type in module.GetTypes())
            {
                yield return type;
                foreach (MethodDef method in type.Methods)
                    yield return method;

                foreach (FieldDef field in type.Fields)
                    yield return field;
                foreach (PropertyDef prop in type.Properties)
                    yield return prop;
                foreach (EventDef evt in type.Events)
                    yield return evt;
            }
        }
        public static IEnumerable<IDnlibDef> FindDefinitions(TypeDef typeDef)
        {
            yield return typeDef;
            foreach (TypeDef nestedType in typeDef.NestedTypes)
                yield return nestedType;
            foreach (MethodDef method in typeDef.Methods)
                yield return method;
            foreach (FieldDef field in typeDef.Fields)
                yield return field;
            foreach (PropertyDef prop in typeDef.Properties)
                yield return prop;
            foreach (EventDef evt in typeDef.Events)
                yield return evt;
        }
        public static bool IsDelegate(TypeDef type)
        {
            if (type.BaseType == null)
                return false;
            string fullName = type.BaseType.FullName;
            return fullName == "System.Delegate" || fullName == "System.MulticastDelegate";
        }
        public static bool HasInstructions(MethodDef method)
        {
            if (method == null)
                new ArgumentNullException("method is null");
            if (method.Body.HasInstructions)
                return true;
            else
                return false;
        }
        public static bool HasVariables(MethodDef method)
        {
            if (method == null)
                new ArgumentNullException("method is null");
            if (method.Body.HasVariables)
                return true;
            else
                return false;
        }
        public static bool Simplify(MethodDef methodDef)
        {
            if (methodDef.Parameters == null)
                return false;
            methodDef.Body.SimplifyMacros(methodDef.Parameters);
            return true;
        }
        public static bool Optimize(MethodDef methodDef)
        {
            if (methodDef.Body == null)
                return false;
            methodDef.Body.OptimizeMacros();
            methodDef.Body.OptimizeBranches();
            return true;
        }
        public static bool hasExceptionHandlers(MethodDef methodDef)
        {
            if (methodDef.Body.HasExceptionHandlers)
                return true;
            return false;
        }
        public static void fixProxy(ModuleDef moduleDef)
        {
            AssemblyResolver assemblyResolver = new AssemblyResolver();
            ModuleContext moduleContext = new ModuleContext(assemblyResolver);
            assemblyResolver.DefaultModuleContext = moduleContext;
            assemblyResolver.EnableTypeDefCache = true;
            List<AssemblyRef> list = moduleDef.GetAssemblyRefs().ToList<AssemblyRef>();
            moduleDef.Context = moduleContext;
            foreach (AssemblyRef assemblyRef in list)
            {
                bool flag3 = assemblyRef == null;
                if (!flag3)
                {
                    AssemblyDef assemblyDef = assemblyResolver.Resolve(assemblyRef.FullName, moduleDef);
                    bool flag4 = assemblyDef == null;
                    if (!flag4)
                    {
                        moduleDef.Context.AssemblyResolver.AddToCache(assemblyDef);
                    }
                }
            }
        }
    }
}
