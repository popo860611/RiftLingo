using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using RiftLingo.Models;

namespace RiftLingo.Services;

public sealed class ScreenCaptureService
{
    public Bitmap Capture(CaptureRegion region)
    {
        if (!region.IsValid)
        {
            throw new ArgumentOutOfRangeException(nameof(region), "擷取範圍太小。");
        }

        using var raw = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(raw))
        {
            graphics.CopyFromScreen(region.X, region.Y, 0, 0, raw.Size, CopyPixelOperation.SourceCopy);
        }

        var scaled = new Bitmap(region.Width * 2, region.Height * 2, PixelFormat.Format24bppRgb);
        using var output = Graphics.FromImage(scaled);
        output.InterpolationMode = InterpolationMode.HighQualityBicubic;
        output.PixelOffsetMode = PixelOffsetMode.HighQuality;
        output.DrawImage(raw, 0, 0, scaled.Width, scaled.Height);
        return scaled;
    }
}
