using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using RiftLingo.Services;

namespace RiftLingo.Windows;

public partial class GeminiPreviewWindow : Window
{
    public GeminiPreviewWindow(GeminiScreenshotTranslation result)
    {
        InitializeComponent();
        SummaryText.Text = $"Gemini 找到 {result.Messages.Count} 則玩家聊天";
        TranslationText.Text = result.Messages.Count == 0
            ? "（沒有找到玩家聊天，請確認框選範圍及聊天框是否已展開）"
            : string.Join(Environment.NewLine + Environment.NewLine, result.Messages.Select(message => $"{message.Original}\n→ {message.Translation}"));

        if (result.ScreenshotPng is not { Length: > 0 }) return;
        using var memory = new MemoryStream(result.ScreenshotPng);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = memory;
        image.EndInit();
        image.Freeze();
        PreviewImage.Source = image;
    }
}
