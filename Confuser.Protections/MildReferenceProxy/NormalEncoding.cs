namespace Confuser.Protections.MildReferenceProxy
{
    using Confuser.Core.Services;
    using Confuser.DynCipher;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using System;
    using System.Collections.Generic;

    internal class NormalEncoding : IRPEncoding
    {
        private readonly Dictionary<MethodDef, Tuple<int, int>> keys = new Dictionary<MethodDef, Tuple<int, int>>();

        public Instruction[] EmitDecode(MethodDef init, RPContext ctx, Instruction[] arg)
        {
            Tuple<int, int> key = this.GetKey(ctx.Random, init);
            List<Instruction> list = new List<Instruction>();
            if (ctx.Random.NextBoolean())
            {
                list.Add(Instruction.Create(OpCodes.Ldc_I4, key.Item1));
                list.AddRange(arg);
            }
            else
            {
                list.AddRange(arg);
                list.Add(Instruction.Create(OpCodes.Ldc_I4, key.Item1));
            }
            list.Add(Instruction.Create(OpCodes.Mul));
            return list.ToArray();
        }

        public int Encode(MethodDef init, RPContext ctx, int value)
        {
            Tuple<int, int> key = this.GetKey(ctx.Random, init);
            return (value * key.Item2);
        }

        private Tuple<int, int> GetKey(RandomGenerator random, MethodDef init)
        {
            Tuple<int, int> tuple;
            if (!this.keys.TryGetValue(init, out tuple))
            {
                int num = random.NextInt32() | 1;
                this.keys[init] = tuple = Tuple.Create<int, int>(num, (int) MathsUtils.modInv((uint) num));
            }
            return tuple;
        }
    }
}

