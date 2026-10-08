using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RiftLingo.Models;

namespace RiftLingo.Windows;

public partial class RegionSelectorWindow : Window
{
    private Point _start;
    private bool _selecting;
    public CaptureRegion? SelectedRegion { get; private set; }

    public RegionSelectorWindow()
    {
        InitializeComponent();
        Left = SystemParameters.VirtualScreenLeft; Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth; Height = SystemParameters.VirtualScreenHeight;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _start = e.GetPosition(this); _selecting = true; SelectionRectangle.Visibility = Visibility.Visible; CaptureMouse();
    }

    private void Window_MouseMove(object sender, MouseEventArgs e) { if (_selecting) UpdateRectangle(e.GetPosition(this)); }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_selecting) return;
        var end = e.GetPosition(this); UpdateRectangle(end); _selecting = false; ReleaseMouseCapture();
        var firstPixel = PointToScreen(_start); var lastPixel = PointToScreen(end);
        var region = new CaptureRegion((int)Math.Round(Math.Min(firstPixel.X, lastPixel.X)), (int)Math.Round(Math.Min(firstPixel.Y, lastPixel.Y)), (int)Math.Round(Math.Abs(lastPixel.X - firstPixel.X)), (int)Math.Round(Math.Abs(lastPixel.Y - firstPixel.Y)));
        if (region.IsValid) { SelectedRegion = region; DialogResult = true; } else SelectionRectangle.Visibility = Visibility.Collapsed;
    }

    private void UpdateRectangle(Point current)
    {
        Canvas.SetLeft(SelectionRectangle, Math.Min(_start.X, current.X)); Canvas.SetTop(SelectionRectangle, Math.Min(_start.Y, current.Y));
        SelectionRectangle.Width = Math.Abs(current.X - _start.X); SelectionRectangle.Height = Math.Abs(current.Y - _start.Y);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }
}
