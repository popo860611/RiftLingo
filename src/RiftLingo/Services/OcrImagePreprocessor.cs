using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RiftLingo.Services;

public static class OcrImagePreprocessor
{
    public static Bitmap CreateHighContrast(Bitmap source)
    {
        using var normalized = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(normalized))
        {
            graphics.DrawImageUnscaled(source, 0, 0);
        }

        var histogram = new int[256];
        long brightnessTotal = 0;
        var bounds = new Rectangle(0, 0, normalized.Width, normalized.Height);
        var sourceData = normalized.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var sourceBytes = new byte[Math.Abs(sourceData.Stride) * normalized.Height];
        Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytes.Length);
        normalized.UnlockBits(sourceData);

        for (var y = 0; y < normalized.Height; y++)
            for (var x = 0; x < normalized.Width; x++)
            {
                var offset = (y * Math.Abs(sourceData.Stride)) + (x * 3);
                var value = Luminance(sourceBytes[offset + 2], sourceBytes[offset + 1], sourceBytes[offset]);
                histogram[value]++;
                brightnessTotal += value;
            }

        var threshold = CalculateOtsuThreshold(histogram, normalized.Width * normalized.Height);
        var darkBackground = brightnessTotal / (double)(normalized.Width * normalized.Height) < 128;
        var result = new Bitmap(normalized.Width, normalized.Height, PixelFormat.Format24bppRgb);
        var resultData = result.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        var resultBytes = new byte[Math.Abs(resultData.Stride) * normalized.Height];

        for (var y = 0; y < normalized.Height; y++)
            for (var x = 0; x < normalized.Width; x++)
            {
                var sourceOffset = (y * Math.Abs(sourceData.Stride)) + (x * 3);
                var resultOffset = (y * Math.Abs(resultData.Stride)) + (x * 3);
                var value = Luminance(sourceBytes[sourceOffset + 2], sourceBytes[sourceOffset + 1], sourceBytes[sourceOffset]);
                var isText = darkBackground ? value > threshold : value <= threshold;
                var output = isText ? byte.MinValue : byte.MaxValue;
                resultBytes[resultOffset] = output;
                resultBytes[resultOffset + 1] = output;
                resultBytes[resultOffset + 2] = output;
            }

        Marshal.Copy(resultBytes, 0, resultData.Scan0, resultBytes.Length);
        result.UnlockBits(resultData);

        return result;
    }

    private static int Luminance(byte red, byte green, byte blue) => (red * 299 + green * 587 + blue * 114) / 1000;

    private static int CalculateOtsuThreshold(IReadOnlyList<int> histogram, int totalPixels)
    {
        long weightedTotal = 0;
        for (var i = 0; i < histogram.Count; i++) weightedTotal += (long)i * histogram[i];

        long backgroundWeighted = 0;
        var backgroundCount = 0;
        var bestVariance = -1d;
        var bestThreshold = 127;

        for (var threshold = 0; threshold < histogram.Count; threshold++)
        {
            backgroundCount += histogram[threshold];
            if (backgroundCount == 0) continue;
            var foregroundCount = totalPixels - backgroundCount;
            if (foregroundCount == 0) break;

            backgroundWeighted += (long)threshold * histogram[threshold];
            var backgroundMean = backgroundWeighted / (double)backgroundCount;
            var foregroundMean = (weightedTotal - backgroundWeighted) / (double)foregroundCount;
            var difference = backgroundMean - foregroundMean;
            var variance = (double)backgroundCount * foregroundCount * difference * difference;
            if (variance <= bestVariance) continue;
            bestVariance = variance;
            bestThreshold = threshold;
        }

        return bestThreshold;
    }
}
