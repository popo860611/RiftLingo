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

    public string Recognize(Bitmap bitmap, string languages)
    {
        EnsureEngine(languages);
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        using var pix = Pix.LoadFromMemory(memory.ToArray());
        using var page = _engine!.Process(pix, PageSegMode.SparseText);
        return page.GetText() ?? string.Empty;
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
