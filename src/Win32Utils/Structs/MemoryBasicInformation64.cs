using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMEGo.ShibaLib.Win32Utils.Structs
{
    public struct MemoryBasicInformation64
    {
        public ulong BaseAddress;
        public ulong AllocationBase;
        public int AllocationProtect;
        int __alignment1;
        public ulong RegionSize;
        public int State;
        public int Protect;
        public int Type;
        int __alignment2;
    }
}
