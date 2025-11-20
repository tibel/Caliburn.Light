using System.Runtime.InteropServices;

namespace Caliburn.Light.WinUI;

internal static partial class User32
{
    public const int GWLP_HWNDPARENT = -8;
    public const int GWL_STYLE = -16;

    public const nint WS_DISABLED = (nint)0x08000000L;

    [LibraryImport("user32.dll", EntryPoint = "EnableWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EnableWindow(nint hWnd, [MarshalAs(UnmanagedType.Bool)] bool bEnable);

    public static nint GetWindowLongPtr(nint hWnd, int nIndex) => nint.Size == 8
        ? GetWindowLongPtrW(hWnd, nIndex)
        : GetWindowLongA(hWnd, nIndex);

    public static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong) => nint.Size == 8
        ? SetWindowLongPtrW(hWnd, nIndex, dwNewLong)
        : SetWindowLongA(hWnd, nIndex, dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongA")]
    private static partial nint GetWindowLongA(nint hWnd, int nIndex);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static partial nint GetWindowLongPtrW(nint hWnd, int nIndex);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongA")]
    private static partial nint SetWindowLongA(nint hWnd, int nIndex, nint dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static partial nint SetWindowLongPtrW(nint hWnd, int nIndex, nint dwNewLong);
}
