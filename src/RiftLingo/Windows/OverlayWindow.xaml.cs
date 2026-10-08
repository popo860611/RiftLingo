using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using RiftLingo.Models;

namespace RiftLingo.Windows;

public partial class OverlayWindow : Window
{
    private const int GwlExstyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolwindow = 0x00000080;
    private const int WsExNoactivate = 0x08000000;

    public ObservableCollection<ChatMessage> Messages { get; } = new();
    public double OverlayFontSize { get; private set; } = 18;

    public OverlayWindow()
    {
        InitializeComponent();
        DataContext = this;
        SourceInitialized += (_, _) => EnableClickThrough();
    }

    public void AddMessage(ChatMessage message)
    {
        Dispatcher.Invoke(() =>
        {
            Messages.Add(message);
            while (Messages.Count > 3) Messages.RemoveAt(0);
        });
    }

    public void ApplySettings(AppSettings settings)
    {
        OverlaySurface.Opacity = Math.Clamp(settings.OverlayOpacity, 0.55, 1.0);
        OverlayFontSize = Math.Clamp(settings.OverlayFontSize, 14, 30);
    }

    public void PositionNear(CaptureRegion region)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        Left = region.X / dpi.DpiScaleX;
        Top = Math.Max(SystemParameters.VirtualScreenTop + 16, (region.Y - 210) / dpi.DpiScaleY);
        Width = Math.Clamp(region.Width / dpi.DpiScaleX, 420, 720);
    }

    private void EnableClickThrough()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var style = GetWindowLongPtr(handle, GwlExstyle).ToInt64();
        SetWindowLongPtr(handle, GwlExstyle, new IntPtr(style | WsExTransparent | WsExToolwindow | WsExNoactivate));
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr windowHandle, int index);
    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern IntPtr GetWindowLong32(IntPtr windowHandle, int index);
    private static IntPtr GetWindowLongPtr(IntPtr windowHandle, int index) => IntPtr.Size == 8 ? GetWindowLongPtr64(windowHandle, index) : GetWindowLong32(windowHandle, index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr windowHandle, int index, IntPtr newLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern IntPtr SetWindowLong32(IntPtr windowHandle, int index, IntPtr newLong);
    private static IntPtr SetWindowLongPtr(IntPtr windowHandle, int index, IntPtr newLong) => IntPtr.Size == 8 ? SetWindowLongPtr64(windowHandle, index, newLong) : SetWindowLong32(windowHandle, index, newLong);
}
