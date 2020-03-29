using System;
using System.Linq;
using Confuser.Core;
using Confuser.Renamer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.Constants
{
    internal class TestPhase : ProtectionPhase
    {
        public TestPhase(MutationProtection parent)
            : base(parent) { }

        public override ProtectionTargets Targets
        {
            get { return ProtectionTargets.Modules; }
        }

        public override string Name
        {
            get { return "Extra Shenannigans"; }
        }

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {

            foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
            {
                foreach (TypeDef type in module.Types)
                {

                    foreach (MethodDef method in type.Methods)
                    {
                        if (!method.HasBody) continue;
                        if (method.DeclaringType.IsGlobalModuleType) continue;

                        var instr = method.Body.Instructions;
                        for (int i = 0; i < instr.Count; i++)
                        {
                            if (instr[i].OpCode == OpCodes.Ldc_I4)
                            {
                                ProtectIntegers(method, i);
                                i += 10;
                            }
                        }

                        LdfldProtection(method);
                        CallvirtProtection(method);
                    }
                }
            }


        }
        public void ProtectIntegers(MethodDef method, int i)
        {
            InlineInteger(method, i);
            ReplaceValue(method, i);

        }
        public void InlineInteger(MethodDef method, int i)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            if (!method.HasBody) return;
            var instr = method.Body.Instructions;
            if ((i - 1) > 0)
                try
                {

                    if (instr[i - 1].OpCode == OpCodes.Callvirt)
                    {
                        if (instr[i + 1].OpCode == OpCodes.Call)
                        {
                            return;
                        }
                    }
                }
                catch { }

            //if (instr[i + 4].IsBr())
            //{
            //    return;
            //}
            bool is_valid_inline = true;
            switch (random.Next(0, 2))
            {
                //true
                case 0:
                    is_valid_inline = true;
                    break;
                //false
                case 1:
                    is_valid_inline = false;
                    break;
            }

            Local new_local = new Local(method.Module.CorLibTypes.String);
            method.Body.Variables.Add(new_local);
            Local new_local2 = new Local(method.Module.CorLibTypes.Int32);
            method.Body.Variables.Add(new_local2);
            var value = instr[i].GetLdcI4Value();
            var first_ldstr = NameService.RandomNameStatic();

            instr.Insert(i, Instruction.Create(OpCodes.Ldloc_S, new_local2));

            instr.Insert(i, Instruction.Create(OpCodes.Stloc_S, new_local2));
            if (is_valid_inline)
            {
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value));
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value + 1));
            }
            else
            {
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value + 1));
                instr.Insert(i, Instruction.Create(OpCodes.Ldc_I4, value));
            }
            instr.Insert(i,
                Instruction.Create(OpCodes.Call,
                    method.Module.Import(typeof(System.String).GetMethod("op_Equality",
                        new Type[] { typeof(string), typeof(string) }))));
            instr.Insert(i, Instruction.Create(OpCodes.Ldstr, first_ldstr));
            instr.Insert(i, Instruction.Create(OpCodes.Ldloc_S, new_local));
            instr.Insert(i, Instruction.Create(OpCodes.Stloc_S, new_local));
            if (is_valid_inline)
            {
                instr.Insert(i, Instruction.Create(OpCodes.Ldstr, first_ldstr));
            }
            else
            {
                instr.Insert(i,
                    Instruction.Create(OpCodes.Ldstr,
                       NameService.RandomNameStatic()));
            }
            instr.Insert(i + 5, Instruction.Create(OpCodes.Brtrue_S, instr[i + 6]));
            instr.Insert(i + 7, Instruction.Create(OpCodes.Br_S, instr[i + 8]));

            instr.RemoveAt(i + 10);

            //if (ProxyMethodConst.Contains(method))
            //{
            //    var last = method.Body.Instructions.Count;
            //    if (!instr[last - 2].IsLdloc()) return;
            //    instr[last - 2].OpCode = OpCodes.Ldloc_S;
            //    instr[last - 2].Operand = new_local2;
            //}
        }
        public void CtorCallProtection(MethodDef method)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            if (!method.HasBody) return;
            var instr = method.Body.Instructions;

            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Call)
                {
                    if (instr[i].Operand.ToString().ToLower().Contains("void"))
                    {
                        if ((i - 1) > 0)
                            if (instr[i - 1].IsLdarg())
                            {
                                Local new_local = new Local(method.Module.CorLibTypes.Int32);
                                method.Body.Variables.Add(new_local);

                                instr.Insert(i - 1, OpCodes.Ldc_I4.ToInstruction(random.Next()));
                                instr.Insert(i, OpCodes.Stloc_S.ToInstruction(new_local));
                                instr.Insert(i + 1, OpCodes.Ldloc_S.ToInstruction(new_local));
                                instr.Insert(i + 2, OpCodes.Ldc_I4.ToInstruction(random.Next()));
                                //---------------------------------------------------bne.un.s +3
                                instr.Insert(i + 3, OpCodes.Ldarg_0.ToInstruction());
                                //---------------------------------------------------br.s +4
                                instr.Insert(i + 4, OpCodes.Nop.ToInstruction());
                                //---------------------------------------------------br.s +1
                                instr.Insert(i + 6, OpCodes.Nop.ToInstruction());

                                instr.Insert(i + 3, new Instruction(OpCodes.Bne_Un_S, instr[i + 4]));
                                instr.Insert(i + 5, new Instruction(OpCodes.Br_S, instr[i + 8]));
                                instr.Insert(i + 8, new Instruction(OpCodes.Br_S, instr[i + 9]));
                            }
                    }
                }
            }
        }
        public void LdfldProtection(MethodDef method)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            var instr = method.Body.Instructions;

            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Ldfld)
                {
                    //if (instr[i].Operand.ToString().ToLower().Contains("class"))
                    //{
                    if ((i - 1) > 0)
                        if (instr[i - 1].IsLdarg())
                        {
                            Local new_local = new Local(method.Module.CorLibTypes.Int32);
                            method.Body.Variables.Add(new_local);

                            instr.Insert(i - 1, OpCodes.Ldc_I4.ToInstruction(random.Next()));
                            instr.Insert(i, OpCodes.Stloc_S.ToInstruction(new_local));
                            instr.Insert(i + 1, OpCodes.Ldloc_S.ToInstruction(new_local));
                            instr.Insert(i + 2, OpCodes.Ldc_I4.ToInstruction(random.Next()));
                            //---------------------------------------------------bne.un.s +3
                            instr.Insert(i + 3, OpCodes.Ldarg_0.ToInstruction());
                            //---------------------------------------------------br.s +4
                            instr.Insert(i + 4, OpCodes.Nop.ToInstruction());
                            //---------------------------------------------------br.s +1
                            instr.Insert(i + 6, OpCodes.Nop.ToInstruction());

                            instr.Insert(i + 3, new Instruction(OpCodes.Beq_S, instr[i + 4]));
                            instr.Insert(i + 5, new Instruction(OpCodes.Br_S, instr[i + 8]));
                            instr.Insert(i + 8, new Instruction(OpCodes.Br_S, instr[i + 9]));
                        }
                    //}
                }
            }
        }
        public void CallvirtProtection(MethodDef method)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            var instr = method.Body.Instructions;

            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Callvirt)
                {
                    if (instr[i].Operand.ToString().ToLower().Contains("int32"))
                    {
                        if ((i - 1) > 0)
                            if (instr[i - 1].IsLdloc())
                            {
                                Local new_local = new Local(method.Module.CorLibTypes.Int32);
                                method.Body.Variables.Add(new_local);

                                instr.Insert(i - 1, OpCodes.Ldc_I4.ToInstruction(random.Next()));
                                instr.Insert(i, OpCodes.Stloc_S.ToInstruction(new_local));
                                instr.Insert(i + 1, OpCodes.Ldloc_S.ToInstruction(new_local));
                                instr.Insert(i + 2, OpCodes.Ldc_I4.ToInstruction(random.Next()));
                                //---------------------------------------------------bne.un.s +3
                                instr.Insert(i + 3, OpCodes.Ldarg_0.ToInstruction());
                                //---------------------------------------------------br.s +4
                                instr.Insert(i + 4, OpCodes.Nop.ToInstruction());
                                //---------------------------------------------------br.s +1
                                instr.Insert(i + 6, OpCodes.Nop.ToInstruction());

                                instr.Insert(i + 3, new Instruction(OpCodes.Beq_S, instr[i + 4]));
                                instr.Insert(i + 5, new Instruction(OpCodes.Br_S, instr[i + 8]));
                                instr.Insert(i + 8, new Instruction(OpCodes.Br_S, instr[i + 9]));
                            }
                    }
                }
            }
        }
        public void ReplaceValue(MethodDef method, int i)
        {
            var instr = method.Body.Instructions;
            if (instr[i].OpCode != OpCodes.Ldc_I4) return;
            var value = instr[i].GetLdcI4Value();
            if (value == 0)
                EmptyTypes(method, i);
        }


        public void EmptyTypes(MethodDef method, int i)
        {
            if (method.DeclaringType.IsGlobalModuleType) return;
            switch (random.Next(0, 2))
            {
                case 0:
                    method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Add));
                    break;

                case 1:
                    method.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Sub));
                    break;
            }
            method.Body.Instructions.Insert(i + 1,
                Instruction.Create(OpCodes.Ldsfld,
                    method.Module.Import((typeof(Type).GetField("EmptyTypes")))));
            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldlen));
        }


        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHᅠᅠOPQRSTUVWXYZ0123456789qwertyuiopasdfghjklzxxcvbnm,./;[]*^$&@$!";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }




    }
}