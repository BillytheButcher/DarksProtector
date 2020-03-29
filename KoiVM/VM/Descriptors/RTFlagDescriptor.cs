﻿#region

using System;
using System.Linq;

#endregion

namespace KoiVM.VM
{
    public class RTFlagDescriptor
    {
        private readonly byte[] ehOrder = Enumerable.Range(0, 4).Select(x => (byte) x).ToArray();
        private readonly byte[] flagOrder = Enumerable.Range(1, 7).Select(x => (byte) x).ToArray();

        public RTFlagDescriptor(Random random)
        {
            random.Shuffle(flagOrder);
            random.Shuffle(ehOrder);
        }

        public byte INSTANCE => flagOrder[0];

        public byte EH_CATCH => ehOrder[0];

        public byte EH_FILTER => ehOrder[1];

        public byte EH_FAULT => ehOrder[2];

        public byte EH_FINALLY => ehOrder[3];
    }
}