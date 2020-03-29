﻿using System;
using System.Collections.Generic;
using dnlib.DotNet;

namespace Confuser.Renamer {
	/// <summary>
	///     Resolves generic arguments
	/// </summary>
	public struct GenericArgumentResolver {
		GenericArguments genericArguments;
		RecursionCounter recursionCounter;

		/// <summary>
		///     Resolves the type signature with the specified generic arguments.
		/// </summary>
		/// <param name="typeSig">The type signature.</param>
		/// <param name="typeGenArgs">The type generic arguments.</param>
		/// <returns>Resolved type signature.</returns>
		/// <exception cref="System.ArgumentException">No generic arguments to resolve.</exception>
		public static TypeSig Resolve(TypeSig typeSig, IList<TypeSig> typeGenArgs) {
			if (typeGenArgs == null)
				throw new ArgumentException("No generic arguments to resolve.");

			var resolver = new GenericArgumentResolver();
			resolver.genericArguments = new GenericArguments();
			resolver.recursionCounter = new RecursionCounter();

			if (typeGenArgs != null)
				resolver.genericArguments.PushTypeArgs(typeGenArgs);

			return resolver.ResolveGenericArgs(typeSig);
		}

		/// <summary>
		///     Resolves the method signature with the specified generic arguments.
		/// </summary>
		/// <param name="methodSig">The method signature.</param>
		/// <param name="typeGenArgs">The type generic arguments.</param>
		/// <returns>Resolved method signature.</returns>
		/// <exception cref="System.ArgumentException">No generic arguments to resolve.</exception>
		public static MethodSig Resolve(MethodSig methodSig, IList<TypeSig> typeGenArgs) {
			if (typeGenArgs == null)
				throw new ArgumentException("No generic arguments to resolve.");

			var resolver = new GenericArgumentResolver();
			resolver.genericArguments = new GenericArguments();
			resolver.recursionCounter = new RecursionCounter();

			if (typeGenArgs != null)
				resolver.genericArguments.PushTypeArgs(typeGenArgs);

			return resolver.ResolveGenericArgs(methodSig);
		}

		bool ReplaceGenericArg(ref TypeSig typeSig) {
			if (genericArguments == null)
				return false;
			TypeSig newTypeSig = genericArguments.Resolve(typeSig);
			if (newTypeSig != typeSig) {
				typeSig = newTypeSig;
				return true;
			}
			return false;
		}

		MethodSig ResolveGenericArgs(MethodSig sig) {
			if (sig == null)
				return null;
			if (!recursionCounter.Increment())
				return null;

			MethodSig result = ResolveGenericArgs(new MethodSig(sig.GetCallingConvention()), sig);

			recursionCounter.Decrement();
			return result;
		}

		MethodSig ResolveGenericArgs(MethodSig sig, MethodSig old) {
			sig.RetType = ResolveGenericArgs(old.RetType);
			foreach (TypeSig p in old.Params)
				sig.Params.Add(ResolveGenericArgs(p));
			sig.GenParamCount = old.GenParamCount;
			if (sig.ParamsAfterSentinel != null) {
				foreach (TypeSig p in old.ParamsAfterSentinel)
					sig.ParamsAfterSentinel.Add(ResolveGenericArgs(p));
			}
			return sig;
		}

		TypeSig ResolveGenericArgs(TypeSig typeSig) {
			if (!recursionCounter.Increment())
				return null;

			if (ReplaceGenericArg(ref typeSig)) {
				recursionCounter.Decrement();
				return typeSig;
			}

			TypeSig result;
			switch (typeSig.ElementType) {
				case ElementType.Ptr:
					result = new PtrSig(ResolveGenericArgs(typeSig.Next));
					break;
				case ElementType.ByRef:
					result = new ByRefSig(ResolveGenericArgs(typeSig.Next));
					break;
				case ElementType.Var:
					result = new GenericVar((typeSig as GenericVar).Number);
					break;
				case ElementType.ValueArray:
					result = new ValueArraySig(ResolveGenericArgs(typeSig.Next), (typeSig as ValueArraySig).Size);
					break;
				case ElementType.SZArray:
					result = new SZArraySig(ResolveGenericArgs(typeSig.Next));
					break;
				case ElementType.MVar:
					result = new GenericMVar((typeSig as GenericMVar).Number);
					break;
				case ElementType.CModReqd:
					result = new CModReqdSig((typeSig as ModifierSig).Modifier, ResolveGenericArgs(typeSig.Next));
					break;
				case ElementType.CModOpt:
					result = new CModOptSig((typeSig as ModifierSig).Modifier, ResolveGenericArgs(typeSig.Next));
					break;
				case ElementType.Module:
					result = new ModuleSig((typeSig as ModuleSig).Index, ResolveGenericArgs(typeSig.Next));
					break;
				case ElementType.Pinned:
					result = new PinnedSig(ResolveGenericArgs(typeSig.Next));
					break;
				case ElementType.FnPtr:
					throw new NotSupportedException("FnPtr is not supported.");

				case ElementType.Array:
					var arraySig = (ArraySig)typeSig;
					var sizes = new List<uint>(arraySig.Sizes);
					var lbounds = new List<int>(arraySig.LowerBounds);
					result = new ArraySig(ResolveGenericArgs(typeSig.Next), arraySig.Rank, sizes, lbounds);
					break;
				case ElementType.GenericInst:
					var gis = (GenericInstSig)typeSig;
					var genArgs = new List<TypeSig>(gis.GenericArguments.Count);
					foreach (TypeSig ga in gis.GenericArguments) {
						genArgs.Add(ResolveGenericArgs(ga));
					}
					result = new GenericInstSig(ResolveGenericArgs(gis.GenericType) as ClassOrValueTypeSig, genArgs);
					break;

				default:
					result = typeSig;
					break;
			}

			recursionCounter.Decrement();

			return result;
		}
	}
}