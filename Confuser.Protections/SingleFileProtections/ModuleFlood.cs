using System;
using System.Collections.Generic;
using System.Linq;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    [BeforeProtection(new string[]
    {
        "Ki.ControlFlow",
        "Ki.AntiTamper"
    })]
    internal class ModuleFlood : Protection
    {
        protected override void Initialize(ConfuserContext context)
        {
        }
        
        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new ModuleFlood.ModuleFloodPhase(this));
        }
        
        public override string Description
        {
            get
            {
                return "This protection flood the module.cctor.";
            }
        }
        
        public override string FullId
        {
            get
            {
                return "Ki.ModuleFlood";
            }
        }
        
        public override string Id
        {
            get
            {
                return "module flood";
            }
        }
        
        public override string Name
        {
            get
            {
                return "Module Flood Protection";
            }
        }
        
        public override ProtectionPreset Preset
        {
            get
            {
                return ProtectionPreset.Maximum;
            }
        }
        
        public const string _FullId = "Ki.ModuleFlood";
        
        public const string _Id = "module flood";
        
        private class ModuleFloodPhase : ProtectionPhase
        {
            public ModuleFloodPhase(ModuleFlood parent) : base(parent)
            {
            }
            
            
            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                TypeDef runtimeType = context.Registry.GetService<IRuntimeService>().GetRuntimeType("Confuser.Runtime.ModuleFlood");
                IMarkerService service = context.Registry.GetService<IMarkerService>();
                INameService service2 = context.Registry.GetService<INameService>();
                foreach (ModuleDef moduleDef in parameters.Targets.OfType<ModuleDef>())
                {
                    var r = new Random(DateTime.Now.Millisecond);
                    IEnumerable<IDnlibDef> enumerable = Core.Helpers.InjectHelper.Inject(runtimeType, moduleDef.GlobalType, moduleDef);
                    MethodDef methodDef = moduleDef.GlobalType.FindStaticConstructor();
                    for (int i = 0; i < r.Next(100, 200); i++)
                    {
                        methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, (MethodDef)enumerable.Single((IDnlibDef method) => method.Name == "Initialize")));
                    }
                    foreach (IDnlibDef def in enumerable)
                    {
                        service2.MarkHelper(def, service, (Protection)base.Parent);
                    }
                }
            }
            
            public override string Name
            {
                get
                {
                    return "Module Flooding";
                }
            }
            
            public override ProtectionTargets Targets
            {
                get
                {
                    return ProtectionTargets.Modules;
                }
            }
        }
    }
}
