using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Confuser.Core;
using Confuser.Core.Helpers;
using Confuser.Core.Services;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Constants {
	internal class ReferenceReplacer {
		public static void ReplaceReference(CEContext ctx, ProtectionParameters parameters) {
            foreach (var entry in ctx.ReferenceRepl)
            {
                if (parameters.GetParameter<bool>(ctx.Context, entry.Key, "cfg"))
                    ReplaceCFG(entry.Key, entry.Value, ctx);
                else
                {
                    ReplaceNormal(entry.Key, entry.Value);
                }
                Encrypt(entry.Key);
            }
		}

		static void ReplaceNormal(MethodDef method, List<Tuple<Instruction, uint, IMethod>> instrs) {
			foreach (var instr in instrs) {
				int i = method.Body.Instructions.IndexOf(instr.Item1);
				instr.Item1.OpCode = OpCodes.Ldc_I4;
				instr.Item1.Operand = (int)instr.Item2;
                method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4, (int)instr.Item2));
                method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldc_I4, (int)instr.Item2 - new Random().Next(1, 7000)));
                method.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Call, instr.Item3));
            }
		}

		struct CFGContext {
			public CEContext Ctx;
			public ControlFlowGraph Graph;
			public BlockKey[] Keys;
			public RandomGenerator Random;
			public Dictionary<uint, CFGState> StatesMap;
			public Local StateVariable;
		}

		struct CFGState {
			public uint A;
			public uint B;
			public uint C;
			public uint D;

			public CFGState(uint seed) {
				A = seed *= 0x21412321;
				B = seed *= 0x21412321;
				C = seed *= 0x21412321;
				D = seed *= 0x21412321;
			}

			public void UpdateExplicit(int id, uint value) {
				switch (id) {
					case 0:
						A = value;
						break;
					case 1:
						B = value;
						break;
					case 2:
						C = value;
						break;
					case 3:
						D = value;
						break;
				}
			}

			public void UpdateIncremental(int id, uint value) {
				switch (id) {
					case 0:
						A *= value;
						break;
					case 1:
						B += value;
						break;
					case 2:
						C ^= value;
						break;
					case 3:
						D -= value;
						break;
				}
			}

			public uint GetIncrementalUpdate(int id, uint target) {
				switch (id) {
					case 0:
						return A ^ target;
					case 1:
						return target - B;
					case 2:
						return C ^ target;
					case 3:
						return D - target;
				}
				throw new UnreachableException();
			}

			public uint Get(int id) {
				switch (id) {
					case 0:
						return A;
					case 1:
						return B;
					case 2:
						return C;
					case 3:
						return D;
				}
				throw new UnreachableException();
			}

			public static byte EncodeFlag(bool exp, int updateId, int getId) {
				byte fl = (byte)(exp ? 0x80 : 0);
				fl |= (byte)updateId;
				fl |= (byte)(getId << 2);
				return fl;
			}
		}

		static void InjectStateType(CEContext ctx) {
			if (ctx.CfgCtxType == null) {
				var type = ctx.Context.Registry.GetService<IRuntimeService>().GetRuntimeType("Confuser.Runtime.CFGCtx");
				ctx.CfgCtxType = Core.Helpers.InjectHelper.Inject(type, ctx.Module);
				ctx.Module.Types.Add(ctx.CfgCtxType);
				ctx.CfgCtxCtor = ctx.CfgCtxType.FindMethod(".ctor");
				ctx.CfgCtxNext = ctx.CfgCtxType.FindMethod("Next");

				ctx.Name.MarkHelper(ctx.CfgCtxType, ctx.Marker, ctx.Protection);
				foreach (var def in ctx.CfgCtxType.Fields)
					ctx.Name.MarkHelper(def, ctx.Marker, ctx.Protection);
				foreach (var def in ctx.CfgCtxType.Methods)
					ctx.Name.MarkHelper(def, ctx.Marker, ctx.Protection);
			}
		}
        private static void Encrypt(MethodDef method)
        {
            Random random = new Random();
            int num = 0;
            ITypeDefOrRef type = null;
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                Instruction instruction = method.Body.Instructions[i];
                if (instruction.IsLdcI4())
                {
                    switch (random.Next(1, 8))
                    {
                        case 1:
                            type = method.Module.Import(typeof(int));
                            num = 4;
                            break;
                        case 2:
                            type = method.Module.Import(typeof(sbyte));
                            num = 1;
                            break;
                        case 3:
                            type = method.Module.Import(typeof(byte));
                            num = 1;
                            break;
                        case 4:
                            type = method.Module.Import(typeof(bool));
                            num = 1;
                            break;
                        case 5:
                            type = method.Module.Import(typeof(decimal));
                            num = 16;
                            break;
                        case 6:
                            type = method.Module.Import(typeof(short));
                            num = 2;
                            break;
                        case 7:
                            type = method.Module.Import(typeof(long));
                            num = 8;
                            break;
                    }
                    int num2 = random.Next(1, 1000);
                    bool flag = Convert.ToBoolean(random.Next(0, 2));
                    switch ((num != 0) ? ((Convert.ToInt32(instruction.Operand) % num == 0) ? random.Next(1, 5) : random.Next(1, 4)) : random.Next(1, 4))
                    {
                        case 1:
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, type));
                            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Add));
                            instruction.Operand = Convert.ToInt32(instruction.Operand) - num + (flag ? (-num2) : num2);
                            goto IL_315;
                        case 2:
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, type));
                            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Sub));
                            instruction.Operand = Convert.ToInt32(instruction.Operand) + num + (flag ? (-num2) : num2);
                            goto IL_315;
                        case 3:
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, type));
                            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Add));
                            instruction.Operand = Convert.ToInt32(instruction.Operand) - num + (flag ? (-num2) : num2);
                            goto IL_315;
                        case 4:
                            method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sizeof, type));
                            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Mul));
                            instruction.Operand = Convert.ToInt32(instruction.Operand) / num;
                            break;
                        default:
                            goto IL_315;
                    }
                    IL_30F:
                    i += 2;
                    goto IL_35D;
                    IL_315:
                    method.Body.Instructions.Insert(i + 3, Instruction.CreateLdcI4(num2));
                    method.Body.Instructions.Insert(i + 4, Instruction.Create(flag ? OpCodes.Add : OpCodes.Sub));
                    i += 2;
                    goto IL_30F;
                }
                IL_35D:;
            }
        }
        static void InsertEmptyStateUpdate(CFGContext ctx, ControlFlowBlock block) {
			var body = ctx.Graph.Body;
			var key = ctx.Keys[block.Id];
			if (key.EntryState == key.ExitState)
				return;

			Instruction first = null;

			int targetIndex = body.Instructions.IndexOf(block.Header);

			CFGState entry;
			if (!ctx.StatesMap.TryGetValue(key.EntryState, out entry)) {
				key.Type = BlockKeyType.Explicit;
			}


			if (key.Type == BlockKeyType.Incremental) {

				CFGState exit;
				if (!ctx.StatesMap.TryGetValue(key.ExitState, out exit)) {

					exit = entry;
					int updateId = ctx.Random.NextInt32(3);
					uint targetValue = ctx.Random.NextUInt32();
					exit.UpdateExplicit(updateId, targetValue);

					int getId = ctx.Random.NextInt32(3);
					var fl = CFGState.EncodeFlag(false, updateId, getId);
					var incr = entry.GetIncrementalUpdate(updateId, targetValue);

					body.Instructions.Insert(targetIndex++, first = Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
					body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)fl));
					body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldc_I4, (int)incr));
					body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxNext));
					body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Pop));

					ctx.StatesMap[key.ExitState] = exit;
				}
				else {

					var headerIndex = targetIndex;
					for (int stateId = 0; stateId < 4; stateId++) {
						if (entry.Get(stateId) == exit.Get(stateId))
							continue;

						uint targetValue = exit.Get(stateId);
						int getId = ctx.Random.NextInt32(3);
						var fl = CFGState.EncodeFlag(false, stateId, getId);
						var incr = entry.GetIncrementalUpdate(stateId, targetValue);

						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)fl));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldc_I4, (int)incr));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxNext));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Pop));
					}
					first = body.Instructions[headerIndex];
				}
			}
			else {

				CFGState exit;
				if (!ctx.StatesMap.TryGetValue(key.ExitState, out exit)) {
					var seed = ctx.Random.NextUInt32();
					exit = new CFGState(seed);
					body.Instructions.Insert(targetIndex++, first = Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
					body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldc_I4, (int)seed));
					body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxCtor));

					ctx.StatesMap[key.ExitState] = exit;
				}
				else {
					var headerIndex = targetIndex;
					for (int stateId = 0; stateId < 4; stateId++) {
						uint targetValue = exit.Get(stateId);
						int getId = ctx.Random.NextInt32(3);
						var fl = CFGState.EncodeFlag(true, stateId, getId);

						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)fl));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Ldc_I4, (int)targetValue));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxNext));
						body.Instructions.Insert(targetIndex++, Instruction.Create(OpCodes.Pop));
					}
					first = body.Instructions[headerIndex];
				}
			}

			ctx.Graph.Body.ReplaceReference(block.Header, first);
		}

		static uint InsertStateGetAndUpdate(CFGContext ctx, ref int index, BlockKeyType type, ref CFGState currentState, CFGState? targetState) {
			var body = ctx.Graph.Body;

			if (type == BlockKeyType.Incremental) {
				// Incremental

				if (targetState == null) {
					// Randomly update and get state
					int updateId = ctx.Random.NextInt32(3);
					uint targetValue = ctx.Random.NextUInt32();

					int getId = ctx.Random.NextInt32(3);
					var fl = CFGState.EncodeFlag(false, updateId, getId);
					var incr = currentState.GetIncrementalUpdate(updateId, targetValue);
					currentState.UpdateExplicit(updateId, targetValue);

					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)fl));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)incr));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxNext));

					return currentState.Get(getId);
				}
				// Scan for updated state
				int[] stateIds = { 0, 1, 2, 3 };
				ctx.Random.Shuffle(stateIds);
				int i = 0;
				uint getValue = 0;
				foreach (var stateId in stateIds) {
					// There must be at least one update&get
					if (currentState.Get(stateId) == targetState.Value.Get(stateId) &&
					    i != stateIds.Length - 1) {
						i++;
						continue;
					}

					uint targetValue = targetState.Value.Get(stateId);
					int getId = ctx.Random.NextInt32(3);
					var fl = CFGState.EncodeFlag(false, stateId, getId);
					var incr = currentState.GetIncrementalUpdate(stateId, targetValue);
					currentState.UpdateExplicit(stateId, targetValue);

					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)fl));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)incr));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxNext));

					i++;
					if (i == stateIds.Length)
						getValue = currentState.Get(getId);
					else
						body.Instructions.Insert(index++, Instruction.Create(OpCodes.Pop));
				}
				return getValue;
			}
			else {
				// Explicit

				if (targetState == null) {
					// Create new exit state from random seed
					var seed = ctx.Random.NextUInt32();
					currentState = new CFGState(seed);
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Dup));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)seed));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxCtor));

					// Randomly get state
					int updateId = ctx.Random.NextInt32(3);
					uint targetValue = ctx.Random.NextUInt32();

					int getId = ctx.Random.NextInt32(3);
					var fl = CFGState.EncodeFlag(false, updateId, getId);
					var incr = currentState.GetIncrementalUpdate(updateId, targetValue);
					currentState.UpdateExplicit(updateId, targetValue);

					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)fl));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)incr));
					body.Instructions.Insert(index++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxNext));

					return currentState.Get(getId);
				}
				else {
					// Scan for updated state
					int[] stateIds = { 0, 1, 2, 3 };
					ctx.Random.Shuffle(stateIds);
					int i = 0;
					uint getValue = 0;
					foreach (var stateId in stateIds) {
						uint targetValue = targetState.Value.Get(stateId);
						int getId = ctx.Random.NextInt32(3);
						var fl = CFGState.EncodeFlag(true, stateId, getId);
						currentState.UpdateExplicit(stateId, targetValue);

						body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldloca, ctx.StateVariable));
						body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)fl));
						body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)targetValue));
						body.Instructions.Insert(index++, Instruction.Create(OpCodes.Call, ctx.Ctx.CfgCtxNext));

						i++;
						if (i == stateIds.Length)
							getValue = targetState.Value.Get(getId);
						else
							body.Instructions.Insert(index++, Instruction.Create(OpCodes.Pop));
					}
					return getValue;
				}
			}
		}

		static void ReplaceCFG(MethodDef method, List<Tuple<Instruction, uint, IMethod>> instrs, CEContext ctx) {
			InjectStateType(ctx);

			var graph = ControlFlowGraph.Construct(method.Body);
			var sequence = KeySequence.ComputeKeys(graph, null);

			var cfgCtx = new CFGContext {
				Ctx = ctx,
				Graph = graph,
				Keys = sequence,
				StatesMap = new Dictionary<uint, CFGState>(),
				Random = ctx.Random
			};

			cfgCtx.StateVariable = new Local(ctx.CfgCtxType.ToTypeSig());
			method.Body.Variables.Add(cfgCtx.StateVariable);
			method.Body.InitLocals = true;

			var blockReferences = new Dictionary<int, SortedList<int, Tuple<Instruction, uint, IMethod>>>();
			foreach (var instr in instrs) {
				var index = graph.IndexOf(instr.Item1);
				var block = graph.GetContainingBlock(index);

				SortedList<int, Tuple<Instruction, uint, IMethod>> list;
				if (!blockReferences.TryGetValue(block.Id, out list))
					list = blockReferences[block.Id] = new SortedList<int, Tuple<Instruction, uint, IMethod>>();

				list.Add(index, instr);
			}

			// Update state for blocks not in use
			for (int i = 0; i < graph.Count; i++) {
				var block = graph[i];
				if (blockReferences.ContainsKey(block.Id))
					continue;
				InsertEmptyStateUpdate(cfgCtx, block);
			}

			// Update references
			foreach (var blockRef in blockReferences) {
				var key = sequence[blockRef.Key];
				CFGState currentState;
				if (!cfgCtx.StatesMap.TryGetValue(key.EntryState, out currentState)) {
					Debug.Assert((graph[blockRef.Key].Type & ControlFlowBlockType.Entry) != 0);
					Debug.Assert(key.Type == BlockKeyType.Explicit);

					// Create new entry state
					uint blockSeed = ctx.Random.NextUInt32();
					currentState = new CFGState(blockSeed);
					cfgCtx.StatesMap[key.EntryState] = currentState;

					var index = graph.Body.Instructions.IndexOf(graph[blockRef.Key].Header);
					Instruction newHeader;
					method.Body.Instructions.Insert(index++, newHeader = Instruction.Create(OpCodes.Ldloca, cfgCtx.StateVariable));
					method.Body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)blockSeed));
					method.Body.Instructions.Insert(index++, Instruction.Create(OpCodes.Call, ctx.CfgCtxCtor));
					method.Body.ReplaceReference(graph[blockRef.Key].Header, newHeader);
					key.Type = BlockKeyType.Incremental;
				}
				var type = key.Type;

				for (int i = 0; i < blockRef.Value.Count; i++) {
					var refEntry = blockRef.Value.Values[i];

					CFGState? targetState = null;
					if (i == blockRef.Value.Count - 1) {
						CFGState exitState;
						if (cfgCtx.StatesMap.TryGetValue(key.ExitState, out exitState))
							targetState = exitState;
					}

					var index = graph.Body.Instructions.IndexOf(refEntry.Item1) + 1;
					var value = InsertStateGetAndUpdate(cfgCtx, ref index, type, ref currentState, targetState);

					refEntry.Item1.OpCode = OpCodes.Ldc_I4;
                    refEntry.Item1.Operand = (int)(refEntry.Item2 ^ value);
                    method.Body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)refEntry.Item2 - new Random().Next(1, 7000)));
                    method.Body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, (int)(refEntry.Item2 ^ value)));
                    method.Body.Instructions.Insert(index++, Instruction.Create(OpCodes.Ldc_I4, new Random().Next(1, 7000)));

                    if (i == blockRef.Value.Count - 1 && targetState == null) {
						cfgCtx.StatesMap[key.ExitState] = currentState;
					}

					type = BlockKeyType.Incremental;
				}
			}
		}
	}
}