using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using RMEGo.ShibaLib.Win32Utils;

namespace RMEGo.ShibaLib.Win32Utils.WPF;

public static class WindowExtensions
{
    private static IntPtr GetHwnd(this Window wnd) => new WindowInteropHelper(wnd).Handle;

    public static void SetWindowClickThrough(this Window wnd, bool enabled)
    {
        Win32WindowUtils.SetWindowClickThrough(wnd.GetHwnd(), enabled);
    }

    public static void SetSystemContextMenuEnabled(this Window wnd, bool enabled)
    {
        Win32WindowUtils.SetSystemContextMenuEnabled(wnd.GetHwnd(), enabled);
    }

    public static void SetAltTabDisplayHidden(this Window wnd, bool hidden)
    {
        Win32WindowUtils.SetAltTabDisplayHidden(wnd.GetHwnd(), hidden);
    }

    public static void SetAltF4Enabled(this Window wnd, bool enabled)
    {
        var hwndSource = HwndSource.FromHwnd(wnd.GetHwnd());
        if (enabled)
        {
            hwndSource.RemoveHook(AltF4Hook);
        }
        else
        {
            hwndSource.AddHook(AltF4Hook);
        }
    }

    private static IntPtr AltF4Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr IParam, ref bool handled)
    {
        if (msg == Win32Constants.WM.SYSKEYDOWN && wParam.ToInt32() == Win32Constants.VK.F4)
        {
            handled = true;
        }
        return IntPtr.Zero;
    }
}
