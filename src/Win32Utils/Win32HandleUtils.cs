using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Win32Utils
{
    public static class Win32HandleUtils
    {
        [DllImport("kernal32.dll")]
        internal static extern bool CloseHandle(IntPtr handle);
    }
}
