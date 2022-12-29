using System.Runtime.InteropServices;

namespace RMEGo.ShibaLib.Win32Utils;

public static class Win32WindowUtils
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x00080000;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int VK_F4 = 0x73;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwLong);

    private static void SwitchWindowLongFlag(IntPtr hWnd, int nIndex, int dwLong, bool turnOn)
    {
        var extendStyle = GetWindowLong(hWnd, nIndex);
        if (turnOn)
        {
            extendStyle |= dwLong;
        }
        else
        {
            extendStyle &= ~dwLong;
        }
        SetWindowLong(hWnd, nIndex, extendStyle);
    }

    public static void SetWindowClickThrough(IntPtr hWnd, bool enabled)
    {
        SwitchWindowLongFlag(hWnd, GWL_EXSTYLE, WS_EX_TRANSPARENT, enabled);
    }

    public static void SetSystemContextMenuEnabled(IntPtr hWnd, bool enabled)
    {
        SwitchWindowLongFlag(hWnd, GWL_STYLE, WS_SYSMENU, enabled);
    }

    public static void SetAltTabDisplayHidden(IntPtr hWnd, bool hidden)
    {
        SwitchWindowLongFlag(hWnd, GWL_EXSTYLE, WS_EX_TOOLWINDOW, hidden);
    }

}
