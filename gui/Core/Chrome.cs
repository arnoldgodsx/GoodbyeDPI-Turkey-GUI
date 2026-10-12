using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Hub;

/// <summary>Pencere çerçevesi: Windows 11'in yuvarlak köşesi, koyu kenarlığı ve gölgesi.</summary>
static class Dwm
{
    const int DarkMode = 20, CornerPreference = 33, BorderColor = 34;

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    public static void Apply(Window w)
    {
        var hwnd = new WindowInteropHelper(w).Handle;
        int on = 1, round = 2, border = 0x00222222; // COLORREF: 0x00BBGGRR
        DwmSetWindowAttribute(hwnd, DarkMode, ref on, sizeof(int));
        DwmSetWindowAttribute(hwnd, CornerPreference, ref round, sizeof(int));
        DwmSetWindowAttribute(hwnd, BorderColor, ref border, sizeof(int));
    }
}
