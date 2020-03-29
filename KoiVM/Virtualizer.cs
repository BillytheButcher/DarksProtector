﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using KoiVM.RT;
using KoiVM.VMIL;

#endregion

namespace KoiVM
{
    [Obfuscation(Exclude = false, Feature = "+koi;-ref proxy")]
    public class Virtualizer : IDarksVMSettings
    {
        private readonly bool debug;
        private readonly HashSet<MethodDef> doInstantiation = new HashSet<MethodDef>();
        private readonly GenericInstantiation instantiation = new GenericInstantiation();
        private readonly Dictionary<MethodDef, bool> methodList = new Dictionary<MethodDef, bool>();
        private readonly HashSet<ModuleDef> processed = new HashSet<ModuleDef>();
        private string runtimeName;
        private readonly int seed;
        private MethodVirtualizer vr;

        public Virtualizer(int seed, bool debug)
        {
            Runtime = null;
            this.seed = seed;
            this.debug = debug;

            instantiation.ShouldInstantiate += spec => doInstantiation.Contains(spec.Method.ResolveMethodDefThrow());
        }

        public ModuleDef RuntimeModule => Runtime.Module;

        public DarksVMRuntime Runtime
        {
            get;
            set;
        }

        bool IDarksVMSettings.IsExported(MethodDef method)
        {
            bool ret;
            if(!methodList.TryGetValue(method, out ret))
                return false;
            return ret;
        }

        bool IDarksVMSettings.IsVirtualized(MethodDef method)
        {
            return methodList.ContainsKey(method);
        }

        int IDarksVMSettings.Seed => seed;

        bool IDarksVMSettings.IsDebug => debug;

        public bool ExportDbgInfo
        {
            get;
            set;
        }

        public bool DoStackWalk
        {
            get;
            set;
        }

        public void Initialize(ModuleDef runtimeLib)
        {
            Runtime = new DarksVMRuntime(this, runtimeLib);
            runtimeName = runtimeLib.Assembly.Name;
            vr = new MethodVirtualizer(Runtime);
        }

        public void AddModule(ModuleDef module)
        {
            foreach(var method in new Scanner(module).Scan())
                AddMethod(method.Item1, method.Item2);
        }

        public void AddMethod(MethodDef method, bool isExport)
        {
            //TODO : Here is the place where method are being added to the
            //koiVM protection queue. So you can decide which one must be protected or not.
            if(!method.HasBody)
                return;
            if(method.HasGenericParameters) return;
            methodList.Add(method, isExport);

            if(!isExport)
            {
                // Need to force set declaring type because will be used in VM compilation
                var thisParam = method.HasThis ? method.Parameters[0].Type : null;

                var declType = method.DeclaringType;
                declType.Methods.Remove(method);
                if(method.SemanticsAttributes != 0)
                {
                    foreach(var prop in declType.Properties)
                    {
                        if(prop.GetMethod == method)
                            prop.GetMethod = null;
                        if(prop.SetMethod == method)
                            prop.SetMethod = null;
                    }
                    foreach(var evt in declType.Events)
                    {
                        if(evt.AddMethod == method)
                            evt.AddMethod = null;
                        if(evt.RemoveMethod == method)
                            evt.RemoveMethod = null;
                        if(evt.InvokeMethod == method)
                            evt.InvokeMethod = null;
                    }
                }
                method.DeclaringType2 = declType;

                if(thisParam != null)
                    method.Parameters[0].Type = thisParam;
            }
        }

        public IEnumerable<MethodDef> GetMethods()
        {
            return methodList.Keys;
        }

        public void ProcessMethods(ModuleDef module, Action<int, int> progress = null)
        {
            if(processed.Contains(module))
                throw new InvalidOperationException("Module already processed.");

            if(progress == null)
                progress = (num, total) => { };

            var targets = methodList.Keys.Where(method => method.Module == module).ToList();

            for(var i = 0; i < targets.Count; i++)
            {
                var method = targets[i];
                instantiation.EnsureInstantiation(method, (spec, instantation) =>
                {
                    if(instantation.Module == module || processed.Contains(instantation.Module))
                        targets.Add(instantation);
                    methodList[instantation] = false;
                });
                try
                {
                    ProcessMethod(method, methodList[method]);
                }
                catch(Exception)
                {
                    Console.WriteLine("! error on process method : " + method.FullName);
                }
                progress(i, targets.Count);
            }
            progress(targets.Count, targets.Count);
            processed.Add(module);
        }

        public IModuleWriterListener CommitModule(ModuleDefMD module, Action<int, int> progress = null)
        {
            if(progress == null)
                progress = (num, total) => { };

            var methods = methodList.Keys.Where(method => method.Module == module).ToArray();
            for(var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                PostProcessMethod(method, methodList[method]);
                progress(i, methodList.Count);
            }
            progress(methods.Length, methods.Length);

            return Runtime.CommitModule(module);
        }

        public void CommitRuntime(ModuleDef targetModule = null)
        {
            Runtime.CommitRuntime(targetModule);
        }

        private void ProcessMethod(MethodDef method, bool isExport)
        {
            vr.Load(method, isExport);
        }

        private void PostProcessMethod(MethodDef method, bool isExport)
        {
            var scope = Runtime.LookupMethod(method);

            var ilTransformer = new ILPostTransformer(method, scope, Runtime);
            ilTransformer.Transform();
        }

        public string SaveRuntime(string directory)
        {
            var rtPath = Path.Combine(directory, runtimeName + ".dll");

            File.WriteAllBytes(rtPath, Runtime.RuntimeLibrary);
            if(Runtime.RuntimeSymbols.Length > 0)
                File.WriteAllBytes(Path.ChangeExtension(rtPath, "pdb"), Runtime.RuntimeSymbols);
            return rtPath;
        }
    }
}