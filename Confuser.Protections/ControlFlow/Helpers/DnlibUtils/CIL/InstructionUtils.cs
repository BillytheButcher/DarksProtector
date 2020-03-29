using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper.DnlibUtils2.CIL
{
    static class InstructionUtils
    {

        public static void InsertBefore(this CilBody body, Instruction target, Instruction instruction)
        {
            if (target == null)
                throw new ArgumentNullException("target is null");
            if (instruction == null)
                throw new ArgumentNullException("instruction is null");
            var index = body.Instructions.IndexOf(target);
            if (index == -1)
                throw new ArgumentOutOfRangeException("target");
            body.Instructions.Insert(index, instruction);
        }

        public static void InsertAfter(this CilBody body, Instruction target, Instruction instruction)
        {
            if (target == null)
                throw new ArgumentNullException("target is null");
            if (instruction == null)
                throw new ArgumentNullException("instruction is null");
            var index = body.Instructions.IndexOf(target);
            if (index == -1)
                throw new ArgumentOutOfRangeException("target");
            body.Instructions.Insert(index + 1, instruction);
        }

        public static void Replace(this CilBody body, Instruction target, Instruction instruction)
        {
            if (target == null)
                throw new ArgumentNullException("target is null");
            if (instruction == null)
                throw new ArgumentNullException("instruction");
            InsertAfter(body, target, instruction);
            Remove(body, target);
        }
        public static void Remove(this CilBody body, Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");
            if (!body.Instructions.Remove(instruction))
                throw new ArgumentOutOfRangeException("cannot remove instruction");

        }
    }
}
