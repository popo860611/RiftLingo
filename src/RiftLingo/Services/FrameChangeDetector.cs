using System.Drawing;

namespace RiftLingo.Services;

public sealed class FrameChangeDetector
{
    private byte[]? _previousSignature;

    public bool HasMeaningfulChange(Bitmap bitmap, double threshold = 3.0)
    {
        var signature = CreateSignature(bitmap);
        if (_previousSignature is null)
        {
            _previousSignature = signature;
            return true;
        }

        var totalDifference = 0;
        for (var i = 0; i < signature.Length; i++)
        {
            totalDifference += Math.Abs(signature[i] - _previousSignature[i]);
        }

        _previousSignature = signature;
        return totalDifference / (double)signature.Length >= threshold;
    }

    public void Reset() => _previousSignature = null;

    private static byte[] CreateSignature(Bitmap bitmap)
    {
        const int width = 16;
        const int height = 8;
        using var sample = new Bitmap(bitmap, new Size(width, height));
        var signature = new byte[width * height];

        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var color = sample.GetPixel(x, y);
                signature[(y * width) + x] = (byte)((color.R * 299 + color.G * 587 + color.B * 114) / 1000);
            }

        return signature;
    }
}
