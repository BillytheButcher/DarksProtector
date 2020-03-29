using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confuser.Protections
{
    public class Block
    {
        public int ID = 0;
        public int nextBlock = 0;
        public List<Instruction> instructions = new List<Instruction>();
    }
}
