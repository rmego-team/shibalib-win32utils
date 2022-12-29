using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMEGo.ShibaLib.Win32Utils;

public static class Win32Constants
{
    public static class GWL
    {
        public const int EXSTYLE = -20;
        public const int STYLE = -16;
    }

    public static class WS
    {
        public const int EX_TRANSPARENT = 0x00000020;
        public const int EX_TOOLWINDOW = 0x00000080;
        public const int SYSMENU = 0x00080000;
    }

    public static class WM
    {
        public const int SYSKEYDOWN = 0x0104;
    }

    public static class VK { 
        public const int F4 = 0x73;
    }
}
