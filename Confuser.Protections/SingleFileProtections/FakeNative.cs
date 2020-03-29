using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Confuser.Core;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using dnlib.IO;

namespace Confuser.Protections
{
    // Token: 0x0200007F RID: 127
    [AfterProtection(new string[]
    {
        "Ki.InvalidMD",
        "Ki.AntiTamper"
    })]
    internal class FakeNative : Protection
    {
        // Token: 0x17000073 RID: 115
        // (get) Token: 0x06000226 RID: 550 RVA: 0x000132ED File Offset: 0x000114ED
        public override string Name
        {
            get
            {
                return "Fake Native Protection";
            }
        }

        // Token: 0x17000074 RID: 116
        // (get) Token: 0x06000227 RID: 551 RVA: 0x000132F4 File Offset: 0x000114F4
        public override string Description
        {
            get
            {
                return "This protection destroy the metadata and display fake native assembly.";
            }
        }

        // Token: 0x17000075 RID: 117
        // (get) Token: 0x06000228 RID: 552 RVA: 0x000132FB File Offset: 0x000114FB
        public override string Id
        {
            get
            {
                return "Fake Native";
            }
        }

        // Token: 0x17000076 RID: 118
        // (get) Token: 0x06000229 RID: 553 RVA: 0x00013302 File Offset: 0x00011502
        public override string FullId
        {
            get
            {
                return "Ki.FakeNative";
            }
        }

        // Token: 0x17000077 RID: 119
        // (get) Token: 0x0600022A RID: 554 RVA: 0x00013309 File Offset: 0x00011509
        public override ProtectionPreset Preset
        {
            get
            {
                return ProtectionPreset.Maximum;
            }
        }

        // Token: 0x0600022B RID: 555 RVA: 0x0001330C File Offset: 0x0001150C
        protected override void Initialize(ConfuserContext context)
        {
        }

        // Token: 0x0600022C RID: 556 RVA: 0x0001330E File Offset: 0x0001150E
        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPostStage(PipelineStage.BeginModule, new FakeNative.FakeNativePhase(this));
        }

        // Token: 0x04000188 RID: 392
        public const string _Id = "Fake Native";

        // Token: 0x04000189 RID: 393
        public const string _FullId = "Ki.FakeNative";

        // Token: 0x0400018A RID: 394
        private static Random R = new Random();

        // Token: 0x0400018B RID: 395
        public static string SectionName;

        // Token: 0x02000080 RID: 128
        private class FakeNativePhase : ProtectionPhase
        {
            // Token: 0x17000078 RID: 120
            // (get) Token: 0x0600022F RID: 559 RVA: 0x00013331 File Offset: 0x00011531
            public override ProtectionTargets Targets
            {
                get
                {
                    return ProtectionTargets.Modules;
                }
            }

            // Token: 0x17000079 RID: 121
            // (get) Token: 0x06000230 RID: 560 RVA: 0x00013335 File Offset: 0x00011535
            public override string Name
            {
                get
                {
                    return "Fake Native MD addition";
                }
            }

            // Token: 0x06000231 RID: 561 RVA: 0x0001333C File Offset: 0x0001153C
            public FakeNativePhase(FakeNative parent) : base(parent)
            {
            }

            // Token: 0x06000232 RID: 562 RVA: 0x00013348 File Offset: 0x00011548
            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                if (parameters.Targets.Contains(context.CurrentModule))
                {
                    this.random = context.Registry.GetService<IRandomService>().GetRandomGenerator("Ki.FakeNative");
                    context.CurrentModuleWriterListener.OnWriterEvent += new EventHandler<ModuleWriterListenerEventArgs>(this.OnWriterEvent);
                }
            }

            // Token: 0x06000233 RID: 563 RVA: 0x0001339C File Offset: 0x0001159C
            private void Randomize<T>(MDTable<T> table) where T : IRawRow
            {
                List<T> list = table.ToList<T>();
                this.random.Shuffle<T>(list);
                table.Reset();
                foreach (T current in list)
                {
                    table.Add(current);
                }
            }

            // Token: 0x06000234 RID: 564 RVA: 0x00013404 File Offset: 0x00011604
            public static string GetRandomString()
            {
                string randomFileName = Path.GetRandomFileName();
                return randomFileName.Replace(".", "");
            }

            // Token: 0x06000235 RID: 565 RVA: 0x00015DF8 File Offset: 0x00013FF8
            private void OnWriterEvent(object sender, ModuleWriterListenerEventArgs e)
            {
                ModuleWriterBase moduleWriterBase = (ModuleWriterBase)sender;
                if (e.WriterEvent == ModuleWriterEvent.MDEndCreateTables)
                {
                    PESection pESection = new PESection(".dark", 1073741888u);
                    moduleWriterBase.Sections.Add(pESection);
                    pESection.Add(new ByteArrayChunk(new byte[123]), 4u);
                    pESection.Add(new ByteArrayChunk(new byte[10]), 4u);
                    string text = ".dark";
                    string s = null;
                    for (int i = 0; i < 80; i++)
                    {
                        text += FakeNative.FakeNativePhase.GetRandomString();
                    }
                    for (int j = 0; j < 80; j++)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(text);
                        s = Utils.EncodeString(bytes, FakeNative.FakeNativePhase.asciiCharset);
                    }
                    byte[] bytes2 = Encoding.ASCII.GetBytes(s);
                    moduleWriterBase.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new FakeNative.RawHeap("#DarksProtector", bytes2));
                    pESection.Add(new ByteArrayChunk(bytes2), 4u);

                    var writer = (ModuleWriterBase)sender;

                    uint signature = (uint)(moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Rows + 1);
                    List<uint> list = (from row in moduleWriterBase.MetaData.TablesHeap.TypeDefTable
                                       select row.Namespace).Distinct<uint>().ToList<uint>();
                    List<uint> list2 = (from row in moduleWriterBase.MetaData.TablesHeap.MethodTable
                                        select row.Name).Distinct<uint>().ToList<uint>();
                    uint num2 = Convert.ToUInt32(FakeNative.R.Next(15, 3546));
                    using (List<uint>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            uint current = enumerator.Current;
                            if (current != 0u)
                            {
                                foreach (uint current2 in list2)
                                {
                                    if (current2 != 0u)
                                    {
                                        moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(signature));
                                        moduleWriterBase.MetaData.TablesHeap.ModuleTable.Add(new RawModuleRow(65535, 0u, 4294967295u, 4294967295u, 4294967295u));
                                        moduleWriterBase.MetaData.TablesHeap.ParamTable.Add(new RawParamRow(254, 254, moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow(this.random.NextUInt32()))));
                                        moduleWriterBase.MetaData.TablesHeap.FieldTable.Add(new RawFieldRow((ushort)(num2 * 4u + 77u), 31u + num2 / 2u * 3u, this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.MemberRefTable.Add(new RawMemberRefRow(num2 + 18u, num2 * 4u + 77u, 31u + num2 / 2u * 3u));
                                        moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(3391u + num2 / 2u * 3u));
                                        moduleWriterBase.MetaData.TablesHeap.PropertyTable.Add(new RawPropertyRow((ushort)(num2 + 44u - 1332u), num2 / 2u + 2u, this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.TypeSpecTable.Add(new RawTypeSpecRow(3391u + num2 / 2u * 3u));
                                        moduleWriterBase.MetaData.TablesHeap.PropertyPtrTable.Add(new RawPropertyPtrRow(this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.AssemblyRefTable.Add(new RawAssemblyRefRow(55, 44, 66, 500, this.random.NextUInt32(), this.random.NextUInt32(), moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow(this.random.NextUInt32())), this.random.NextUInt32(), this.random.NextUInt32()));
                                        moduleWriterBase.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow(this.random.NextUInt32(), moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow(this.random.NextUInt32()))));
                                        moduleWriterBase.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow(this.random.NextUInt32(), (uint)(moduleWriterBase.MetaData.TablesHeap.ENCMapTable.Rows - 1)));
                                        moduleWriterBase.MetaData.TablesHeap.ImplMapTable.Add(new RawImplMapRow(18, num2 * 4u + 77u, 31u + num2 / 2u * 3u, num2 * 4u + 77u));
                                    }
                                }
                            }
                        }
                    }
                }
                if (e.WriterEvent == ModuleWriterEvent.MDOnAllTablesSorted)
                {
                    moduleWriterBase.MetaData.TablesHeap.DeclSecurityTable.Add(new RawDeclSecurityRow(32767, 4294934527u, 4294934527u));
                }



            }

            // Token: 0x0400018C RID: 396
            private RandomGenerator random;

            // Token: 0x0400018D RID: 397
            private static readonly char[] asciiCharset = (from ord in Enumerable.Range(32, 95)
                                                           select (char)ord).Except(new char[]
            {
                '.'
            }).ToArray<char>();
        }

        // Token: 0x02000081 RID: 129
        private class RawHeap : HeapBase
        {
            // Token: 0x1700007A RID: 122
            // (get) Token: 0x0600023A RID: 570 RVA: 0x00017178 File Offset: 0x00015378
            public override string Name
            {
                get
                {
                    return this.name;
                }
            }

            // Token: 0x0600023B RID: 571 RVA: 0x00017180 File Offset: 0x00015380
            public RawHeap(string name, byte[] content)
            {
                this.name = name;
                this.content = content;
            }

            // Token: 0x0600023C RID: 572 RVA: 0x00017196 File Offset: 0x00015396
            public override uint GetRawLength()
            {
                return (uint)this.content.Length;
            }

            // Token: 0x0600023D RID: 573 RVA: 0x000171A0 File Offset: 0x000153A0
            protected override void WriteToImpl(BinaryWriter writer)
            {
                writer.Write(this.content);
            }

            // Token: 0x04000191 RID: 401
            private readonly byte[] content;

            // Token: 0x04000192 RID: 402
            private readonly string name;
        }
    }
}