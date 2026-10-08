using System.Drawing;
using RiftLingo.Services;

namespace RiftLingo.Tests;

public sealed class OcrIntegrationTests
{
    [Fact]
    public void Recognize_ReadsRenderedEnglishChatLine()
    {
        using var bitmap = new Bitmap(640, 120);
        using (var graphics = Graphics.FromImage(bitmap))
        using (var font = new Font("Arial", 46, FontStyle.Bold, GraphicsUnit.Pixel))
        {
            graphics.Clear(Color.White);
            graphics.DrawString("mid mia", font, Brushes.Black, new PointF(20, 20));
        }

        using var ocr = new OcrService();
        var result = ocr.Recognize(bitmap, "eng").ToLowerInvariant();

        Assert.Contains("mid", result);
        Assert.Contains("mia", result);
    }

    [Fact]
    public void RecognizeDetailed_ReadsLightTextOnDarkBackground()
    {
        using var bitmap = new Bitmap(640, 120);
        using (var graphics = Graphics.FromImage(bitmap))
        using (var font = new Font("Arial", 46, FontStyle.Bold, GraphicsUnit.Pixel))
        {
            graphics.Clear(Color.FromArgb(18, 24, 34));
            graphics.DrawString("go baron", font, Brushes.White, new PointF(20, 20));
        }

        using var ocr = new OcrService();
        var result = ocr.RecognizeDetailed(bitmap, "eng", includePreview: true);

        Assert.Contains("baron", result.Text, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(result.PreviewPng!);
    }
}
