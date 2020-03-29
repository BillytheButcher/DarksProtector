using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;

namespace Confuser.Protections
{
    public class Sin : iFunction
    {
        public override ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Sin;
        public override ArithmeticVT Arithmetic(Instruction instruction, ModuleDef module)
        {
            Generator generator = new Generator();
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null;
            List<ArithmeticTypes> arithmeticTypes = new List<ArithmeticTypes>() { ArithmeticTypes.Add, ArithmeticTypes.Sub };
            ArithmeticEmulator arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), ArithmeticUtils.GetY(instruction.GetLdcI4Value()), ArithmeticTypes);
            return (new ArithmeticVT(new Value(arithmeticEmulator.GetValue(arithmeticTypes), arithmeticEmulator.GetY()), new Token(ArithmeticUtils.GetOpCode(arithmeticEmulator.GetType), module.Import(ArithmeticUtils.GetMethod(ArithmeticTypes))), ArithmeticTypes));
        }
    }
}
