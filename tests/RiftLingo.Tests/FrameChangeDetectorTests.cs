using System.Drawing;
using RiftLingo.Services;

namespace RiftLingo.Tests;

public sealed class FrameChangeDetectorTests
{
    [Fact]
    public void HasMeaningfulChange_SkipsIdenticalFramesAndDetectsNewText()
    {
        var detector = new FrameChangeDetector();
        using var first = CreateFrame("mid mia");
        using var identical = CreateFrame("mid mia");
        using var changed = CreateFrame("go drake now");

        Assert.True(detector.HasMeaningfulChange(first));
        Assert.False(detector.HasMeaningfulChange(identical));
        Assert.True(detector.HasMeaningfulChange(changed));
    }

    private static Bitmap CreateFrame(string text)
    {
        var bitmap = new Bitmap(320, 80);
        using var graphics = Graphics.FromImage(bitmap);
        using var font = new Font("Arial", 26, FontStyle.Bold, GraphicsUnit.Pixel);
        graphics.Clear(Color.FromArgb(20, 24, 32));
        graphics.DrawString(text, font, Brushes.White, 8, 18);
        return bitmap;
    }
}
