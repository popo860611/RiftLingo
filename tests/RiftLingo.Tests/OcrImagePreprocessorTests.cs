using System.Drawing;
using RiftLingo.Services;

namespace RiftLingo.Tests;

public sealed class OcrImagePreprocessorTests
{
    [Fact]
    public void CreateHighContrast_ConvertsLightTextOnDarkBackgroundToBlackOnWhite()
    {
        using var source = new Bitmap(40, 20);
        using (var graphics = Graphics.FromImage(source))
        {
            graphics.Clear(Color.FromArgb(20, 20, 20));
            graphics.FillRectangle(Brushes.White, 10, 5, 20, 10);
        }

        using var result = OcrImagePreprocessor.CreateHighContrast(source);

        Assert.Equal(Color.White.ToArgb(), result.GetPixel(0, 0).ToArgb());
        Assert.Equal(Color.Black.ToArgb(), result.GetPixel(20, 10).ToArgb());
    }
}
