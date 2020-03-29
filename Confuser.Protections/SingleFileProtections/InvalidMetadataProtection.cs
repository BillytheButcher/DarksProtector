using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Confuser.Core;
using Confuser.Core.Services;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace Confuser.Protections {
	internal class InvalidMetadataProtection : Protection {
		public const string _Id = "invalid metadata";
		public const string _FullId = "Ki.InvalidMD";

		public override string Name {
			get { return "Invalid Metadata Protection"; }
		}

		public override string Description {
			get { return "This protection adds invalid metadata to modules to prevent disassembler/decompiler from opening them."; }
		}

		public override string Id {
			get { return _Id; }
		}

		public override string FullId {
			get { return _FullId; }
		}

		public override ProtectionPreset Preset {
			get { return ProtectionPreset.None; }
		}

		protected override void Initialize(ConfuserContext context) {
			//
		}

		protected override void PopulatePipeline(ProtectionPipeline pipeline) {
			pipeline.InsertPostStage(PipelineStage.BeginModule, new InvalidMDPhase(this));
		}

		class InvalidMDPhase : ProtectionPhase {
			RandomGenerator random;

			public InvalidMDPhase(InvalidMetadataProtection parent)
				: base(parent) { }

			public override ProtectionTargets Targets {
				get { return ProtectionTargets.Modules; }
			}

			public override string Name {
				get { return "Invalid metadata addition"; }
			}

			protected override void Execute(ConfuserContext context, ProtectionParameters parameters) {
				if (parameters.Targets.Contains(context.CurrentModule)) {
					random = context.Registry.GetService<IRandomService>().GetRandomGenerator(_FullId);
					context.CurrentModuleWriterListener.OnWriterEvent += OnWriterEvent;
				}
			}

			void Randomize<T>(MDTable<T> table) where T : IRawRow {
				List<T> rows = table.ToList();
				random.Shuffle(rows);
				table.Reset();
				foreach (T row in rows)
					table.Add(row);
			}

			void OnWriterEvent(object sender, ModuleWriterListenerEventArgs e) {
				var writer = (ModuleWriterBase)sender;
				if (e.WriterEvent == ModuleWriterEvent.MDEndCreateTables) {

					writer.MetaData.TablesHeap.ModuleTable.Add(new RawModuleRow(0, 0x7fff7fff, 0, 0, 0));
					writer.MetaData.TablesHeap.AssemblyTable.Add(new RawAssemblyRow(0, 0, 0, 0, 0, 0, 0, 0x7fff7fff, 0));

					int r = random.NextInt32(8, 16);
					for (int i = 0; i < r; i++)
						writer.MetaData.TablesHeap.ENCLogTable.Add(new RawENCLogRow(random.NextUInt32(), random.NextUInt32()));
					r = random.NextInt32(8, 16);
					for (int i = 0; i < r; i++)
						writer.MetaData.TablesHeap.ENCMapTable.Add(new RawENCMapRow(random.NextUInt32()));

					Randomize(writer.MetaData.TablesHeap.ManifestResourceTable);

					writer.TheOptions.MetaDataOptions.TablesHeapOptions.ExtraData = random.NextUInt32();
					writer.TheOptions.MetaDataOptions.TablesHeapOptions.UseENC = false;
					writer.TheOptions.MetaDataOptions.MetaDataHeaderOptions.VersionString += "\0\0\0\0";
                    writer.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new RawHeap("#GUID", Guid.NewGuid().ToByteArray()));
					writer.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new RawHeap("#Strings", new byte[1]));
					writer.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new RawHeap("#Blob", new byte[1]));
					writer.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new RawHeap("#Schema", new byte[1]));
                    writer.TheOptions.MetaDataOptions.OtherHeapsEnd.Add(new RawHeap("#DarksProtector", new byte[1]));
                }
				else if (e.WriterEvent == ModuleWriterEvent.MDOnAllTablesSorted) {
					writer.MetaData.TablesHeap.DeclSecurityTable.Add(new RawDeclSecurityRow(
						                                                 unchecked(0x7fff), 0xffff7fff, 0xffff7fff));
				}
			}
		}

		class RawHeap : HeapBase {
			readonly byte[] content;
			readonly string name;

			public RawHeap(string name, byte[] content) {
				this.name = name;
				this.content = content;
			}

			public override string Name {
				get { return name; }
			}

			public override uint GetRawLength() {
				return (uint)content.Length;
			}

			protected override void WriteToImpl(BinaryWriter writer) {
				writer.Write(content);
			}
		}
	}
}
