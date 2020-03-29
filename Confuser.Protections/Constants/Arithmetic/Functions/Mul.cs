using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections
{
    public class Mul : iFunction
    {
        public override ArithmeticTypes ArithmeticTypes => ArithmeticTypes.Mul;
        public override ArithmeticVT Arithmetic(Instruction instruction, ModuleDef module)
        {
            Generator generator = new Generator();
            if (!ArithmeticUtils.CheckArithmetic(instruction)) return null;
            ArithmeticEmulator arithmeticEmulator = new ArithmeticEmulator(instruction.GetLdcI4Value(), ArithmeticUtils.GetY(instruction.GetLdcI4Value()), ArithmeticTypes);
            return (new ArithmeticVT(new Value(arithmeticEmulator.GetValue(), arithmeticEmulator.GetY()), new Token(OpCodes.Mul), ArithmeticTypes));
        }
    }
}
