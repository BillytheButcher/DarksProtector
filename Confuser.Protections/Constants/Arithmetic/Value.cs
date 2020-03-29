using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confuser.Protections
{
    public class Value
    {
        private double x;
        private double y;
        public Value(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double GetX() => x;
        public double GetY() => y;
    }
}
