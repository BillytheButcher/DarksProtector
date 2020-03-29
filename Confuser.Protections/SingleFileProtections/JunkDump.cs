using System;
using System.Linq;
using Confuser.Core;
using Confuser.Renamer;
using dnlib.DotNet;
using Microsoft.VisualBasic;

namespace Confuser.Protections
{
    // Token: 0x02000030 RID: 48
    internal class JunkDump : Protection
    {
        // Token: 0x060000E2 RID: 226 RVA: 0x00004A34 File Offset: 0x00002C34
        protected override void Initialize(ConfuserContext context)
        {
        }

        // Token: 0x060000E3 RID: 227 RVA: 0x00004E1E File Offset: 0x0000301E
        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.WriteModule, new JunkDump.JunkDumpPhase(this));
        }

        // Token: 0x1700005E RID: 94
        // (get) Token: 0x060000E4 RID: 228 RVA: 0x0000A0DC File Offset: 0x000082DC
        public override string Description
        {
            get
            {
                return "This protection adds junk into the output.";
            }
        }

        // Token: 0x1700005F RID: 95
        // (get) Token: 0x060000E5 RID: 229 RVA: 0x0000A0F4 File Offset: 0x000082F4
        public override string FullId
        {
            get
            {
                return "Ki.JunkDump";
            }
        }

        // Token: 0x17000060 RID: 96
        // (get) Token: 0x060000E6 RID: 230 RVA: 0x0000A10C File Offset: 0x0000830C
        public override string Id
        {
            get
            {
                return "Junk";
            }
        }

        // Token: 0x17000061 RID: 97
        // (get) Token: 0x060000E7 RID: 231 RVA: 0x0000A124 File Offset: 0x00008324
        public override string Name
        {
            get
            {
                return "Add Junk";
            }
        }

        // Token: 0x17000062 RID: 98
        // (get) Token: 0x060000E8 RID: 232 RVA: 0x00005CA4 File Offset: 0x00003EA4
        public override ProtectionPreset Preset
        {
            get
            {
                return ProtectionPreset.Minimum;
            }
        }

        // Token: 0x04000055 RID: 85
        public const string _FullId = "Ki.JunkDump";

        // Token: 0x04000056 RID: 86
        public const string _Id = "Junk";

        // Token: 0x02000031 RID: 49
        private class JunkDumpPhase : ProtectionPhase
        {
            // Token: 0x060000EB RID: 235 RVA: 0x00004E2F File Offset: 0x0000302F
            public JunkDumpPhase(JunkDump parent) : base(parent)
            {
            }

            // Token: 0x060000EC RID: 236 RVA: 0x0000A13C File Offset: 0x0000833C

            private static Random random = new Random();

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                foreach (ModuleDef moduleDef in parameters.Targets.OfType<ModuleDef>())
                {
                    ModuleDefMD moduleDefMD = (ModuleDefMD)moduleDef;
                    if (moduleDefMD.FullName.Contains(".exe"))
                    {
                        int num = context.Modules.Count - 1;
                        for (int i = 0; i <= num; i++)
                        {
                            for (int j = 0; j <= 50; j++)
                            {
                                Random rnd = new Random();
                                new TypeDefUser(NameService.RandomNameStatic(), moduleDefMD.CorLibTypes.Object.TypeDefOrRef).Attributes = TypeAttributes.Public;
                                TypeDef item = new TypeDefUser(NameService.RandomNameStatic(), NameService.RandomNameStatic(), moduleDefMD.CorLibTypes.Object.TypeDefOrRef)
                                {
                                    Attributes = TypeAttributes.Public
                                };
                                TypeDef item2 = new TypeDefUser(NameService.RandomNameStatic(), NameService.RandomNameStatic(), moduleDefMD.CorLibTypes.Object.TypeDefOrRef)
                                {
                                    Attributes = TypeAttributes.Public
                                };
                                moduleDefMD.Types.Add(item);
                                moduleDefMD.Types.Add(item2);
                            }
                            Random rnd1 = new Random();                        }
                    }
                    else
                    {
                        context.Logger.Log("WARN: Junk cannot be used on a dll");
                    }
                }
            }

            // Token: 0x17000063 RID: 99
            // (get) Token: 0x060000ED RID: 237 RVA: 0x0000A41C File Offset: 0x0000861C
            public override string Name
            {
                get
                {
                    return "Junk dumping";
                }
            }

            // Token: 0x17000064 RID: 100
            // (get) Token: 0x060000EE RID: 238 RVA: 0x00006058 File Offset: 0x00004258
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
