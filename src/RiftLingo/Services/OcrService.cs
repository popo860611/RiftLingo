using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;

namespace RiftLingo.Services;

public sealed class OcrService : IDisposable
{
    private readonly string _dataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
    private TesseractEngine? _engine;
    private string _loadedLanguages = string.Empty;

    public string Recognize(Bitmap bitmap, string languages) => RecognizeDetailed(bitmap, languages).Text;

    public OcrRecognition RecognizeDetailed(Bitmap bitmap, string languages, bool includePreview = false)
    {
        EnsureEngine(languages);
        using var highContrast = OcrImagePreprocessor.CreateHighContrast(bitmap);
        var candidates = new[]
        {
            RecognizeCandidate(bitmap, "原始畫面"),
            RecognizeCandidate(highContrast, "高對比文字")
        };
        var best = candidates
            .OrderByDescending(candidate => HasUsefulText(candidate.Text))
            .ThenByDescending(candidate => candidate.Confidence)
            .First();

        return includePreview
            ? best with { PreviewPng = EncodePng(best.PreprocessingMode == "高對比文字" ? highContrast : bitmap) }
            : best;
    }

    private OcrRecognition RecognizeCandidate(Bitmap bitmap, string mode)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        using var pix = Pix.LoadFromMemory(memory.ToArray());
        using var page = _engine!.Process(pix, PageSegMode.SparseText);
        return new OcrRecognition(page.GetText() ?? string.Empty, page.GetMeanConfidence(), mode);
    }

    private static bool HasUsefulText(string text) => text.Any(char.IsLetterOrDigit);

    private static byte[] EncodePng(Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        return memory.ToArray();
    }

    public IReadOnlyList<string> MissingLanguages(string languages) =>
        languages.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(language => !File.Exists(Path.Combine(_dataPath, $"{language}.traineddata")))
            .ToArray();

    private void EnsureEngine(string languages)
    {
        var missing = MissingLanguages(languages);
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"缺少 OCR 語言模型：{string.Join(", ", missing)}。請執行 scripts\\download-models.ps1。");
        }

        if (_engine is not null && string.Equals(_loadedLanguages, languages, StringComparison.Ordinal))
        {
            return;
        }

        _engine?.Dispose();
        _engine = new TesseractEngine(_dataPath, languages, EngineMode.LstmOnly);
        _engine.SetVariable("preserve_interword_spaces", "1");
        _loadedLanguages = languages;
    }

    public void Dispose() => _engine?.Dispose();
}
