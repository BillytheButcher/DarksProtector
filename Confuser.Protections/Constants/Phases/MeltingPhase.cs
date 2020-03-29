using System;
using System.Linq;
using Confuser.Core;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Constants
{
    internal class MeltingPhase : ProtectionPhase
    {
        public MeltingPhase(ConstantProtection parent)
            : base(parent) { }

        public override ProtectionTargets Targets
        {
            get { return ProtectionTargets.Modules; }
        }

        public override string Name
        {
            get { return "Constants Melter"; }
        }

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {

            foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
            {
                foreach (TypeDef type in module.Types)
                {
                    if (type.IsGlobalModuleType) continue;
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.FullName.Contains("My.")) continue;
                        if (method.FullName.Contains("InitializeCompnent")) continue;
                        if (method.IsConstructor) continue;
                        if (method.DeclaringType.IsGlobalModuleType) continue;
                        if (!method.HasBody) continue;
                        var instr = method.Body.Instructions;
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].ToString().Contains("ResourceManager"))
                            {
                                i = method.Body.Instructions.Count;
                                continue;
                            }
                            if (method.Body.Instructions[i].ToString().Contains("GetObject")) continue;
                            if (instr[i].OpCode == OpCodes.Ldstr)
                            {
                                Random rn = new Random();
                                for (int j = 1; j < 2; j++)
                                {
                                    if (j != 1) j += 1;
                                    Local new_local = new Local(module.CorLibTypes.String);
                                    Local new_local2 = new Local(module.CorLibTypes.String);

                                    method.Body.Variables.Add(new_local);
                                    method.Body.Variables.Add(new_local2);

                                    instr.Insert(i + j, Instruction.Create(OpCodes.Stloc_S, new_local));
                                    instr.Insert(i + (j + 1), Instruction.Create(OpCodes.Ldloc_S, new_local));
                                }
                            }
                            if (method.Body.Instructions[i].ToString().Contains("ResourceManager")) continue;
                            if (method.Body.Instructions[i].ToString().Contains("GetObject")) continue;
                            if (instr[i].IsLdcI4())
                            {
                                Random rn = new Random();
                                for (int j = 1; j < 2; j++)
                                {
                                    if (j != 1) j += 1;
                                    Local new_local = new Local(module.CorLibTypes.Int32);
                                    Local new_local2 = new Local(module.CorLibTypes.Int32);
                                    method.Body.Variables.Add(new_local);
                                    method.Body.Variables.Add(new_local2);
                                    instr.Insert(i + j, Instruction.Create(OpCodes.Stloc_S, new_local));
                                    instr.Insert(i + (j + 1), Instruction.Create(OpCodes.Ldloc_S, new_local));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void HideStrings(MethodDef methodDef)
        {
            if (canObfuscate(methodDef))
            {
                foreach (Instruction instruction in methodDef.Body.Instructions)
                {
                    if (instruction.OpCode != OpCodes.Ldstr) continue;
                    MethodDef meth = new MethodDefUser(NameService.RandomNameStatic(), MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.String), MethodImplAttributes.IL | MethodImplAttributes.Managed, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig) { Body = new CilBody() };
                    meth.Body.Instructions.Add(new Instruction(OpCodes.Ldstr, instruction.Operand.ToString()));
                    meth.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                    methodDef.DeclaringType.Methods.Add(meth);
                    instruction.OpCode = OpCodes.Call;
                    instruction.Operand = meth;
                }
            }
        }
        public static bool canObfuscate(MethodDef methodDef)
        {
            if (!methodDef.HasBody)
                return false;
            if (!methodDef.Body.HasInstructions)
                return false;
            if (methodDef.DeclaringType.IsGlobalModuleType)
                return false;

            return true;

        }
        private void HideInts(MethodDef methodDef)
        {
            if (canObfuscate(methodDef))
            {
                foreach (Instruction instruction in methodDef.Body.Instructions)
                {
                    if (instruction.OpCode != OpCodes.Ldc_I4) continue;
                    MethodDef meth = new MethodDefUser(NameService.RandomNameStatic(), MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.Int32), MethodImplAttributes.IL | MethodImplAttributes.Managed, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig) { Body = new CilBody() };
                    meth.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, instruction.GetLdcI4Value()));
                    meth.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                    methodDef.DeclaringType.Methods.Add(meth);
                    instruction.OpCode = OpCodes.Call;
                    instruction.Operand = meth;
                }
            }
        }


    }
}