using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confuser.Protections
{
    public class ArithmeticVT
    {
        private Value value;
        private Token token;
        private ArithmeticTypes arithmeticTypes;
        public ArithmeticVT(Value value, Token token, ArithmeticTypes arithmeticTypes)
        {
            this.value = value;
            this.token = token;
            this.arithmeticTypes = arithmeticTypes;
        }
        public Value GetValue() => value;
        public Token GetToken() => token;
        public ArithmeticTypes GetArithmetic() => arithmeticTypes;
    }
}
