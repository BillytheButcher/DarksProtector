using System;
using System.IO;
using System.Linq;
using Confuser.Core;
using dnlib.DotNet;
using Microsoft.VisualBasic;

namespace Confuser.Protections
{
	internal class ModuleRenamer : Protection
	{
		protected override void Initialize(ConfuserContext context)
		{
		}
        
		protected override void PopulatePipeline(ProtectionPipeline pipeline)
		{
			pipeline.InsertPreStage(PipelineStage.WriteModule, new ModuleRenamer.ModuleRenamerPhase(this));
		}
        
		public override string Description
		{
			get
			{
				return "Renames the module and assembly.";
			}
		}
        
		public override string FullId
		{
			get
			{
				return "Ki.modulerenamer";
			}
		}
        
		public override string Id
		{
			get
			{
				return "Rename Module";
			}
		}
        
		public override string Name
		{
			get
			{
				return "Rename Module";
			}
		}
        
		public override ProtectionPreset Preset
		{
			get
			{
				return ProtectionPreset.Minimum;
			}
		}
        
        
		public const string _FullId = "Ki.modulerenamer";
        
		public const string _Id = "Module Renamer";
        
		private class ModuleRenamerPhase : ProtectionPhase
		{
			public ModuleRenamerPhase(ModuleRenamer parent) : base(parent)
			{
			}
            
			protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
			{
				foreach (ModuleDef moduleDef in parameters.Targets.OfType<ModuleDef>())
				{
					ModuleDefMD moduleDefMD = (ModuleDefMD)moduleDef;
                    if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt")))
                    {
                        string text = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt"));
                        if (!string.IsNullOrEmpty(text) && text.Contains(":"))
                        {
                            string module = text.Split(new char[]
                            {
                        ':'
                            })[0].Replace(" ", "");
                            string assembly = text.Split(new char[]
                            {
                        ':'
                            })[1].Replace(" ", "");
                            if(module != "")
                            {
                                moduleDefMD.Name = module;
                            }
                            if (assembly != "")
                            {
                                moduleDefMD.Assembly.Name = assembly;
                            }
                        }
                    }
                    else
                    {
                        moduleDefMD.Name = "∂αякsρяσтεcтσя";
                        moduleDefMD.Assembly.Name = "∂αякsρяσтεcтσя";
                    }
				}
			}
            
			public override string Name
			{
				get
				{
					return "Module Renaming";
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
