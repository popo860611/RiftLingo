using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace RiftLingo.Services;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint ModAlt = 0x0001;
    private const int ToggleOverlayId = 0x5401;
    private const int TogglePauseId = 0x5402;
    private HwndSource? _source;
    private IntPtr _handle;

    public event EventHandler? ToggleOverlayRequested;
    public event EventHandler? TogglePauseRequested;

    public void Register(IntPtr windowHandle)
    {
        _handle = windowHandle;
        _source = HwndSource.FromHwnd(windowHandle);
        _source.AddHook(WndProc);

        if (!RegisterHotKey(windowHandle, ToggleOverlayId, ModControl | ModAlt, (uint)'T') ||
            !RegisterHotKey(windowHandle, TogglePauseId, ModControl | ModAlt, (uint)'P'))
        {
            Dispose();
            throw new InvalidOperationException("無法註冊快捷鍵，可能已被其他程式使用。");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != WmHotkey)
        {
            return IntPtr.Zero;
        }

        handled = true;
        if (wParam.ToInt32() == ToggleOverlayId)
        {
            ToggleOverlayRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (wParam.ToInt32() == TogglePauseId)
        {
            TogglePauseRequested?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            UnregisterHotKey(_handle, ToggleOverlayId);
            UnregisterHotKey(_handle, TogglePauseId);
        }

        _source?.RemoveHook(WndProc);
        _source = null;
        _handle = IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
