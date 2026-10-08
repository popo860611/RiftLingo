using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using RiftLingo.Services;

namespace RiftLingo.Windows;

public partial class OcrPreviewWindow : Window
{
    public OcrPreviewWindow(OcrRecognition recognition)
    {
        InitializeComponent();
        SummaryText.Text = $"處理模式：{recognition.PreprocessingMode}　信心度：{recognition.Confidence:P0}";
        RecognizedText.Text = string.IsNullOrWhiteSpace(recognition.Text) ? "（沒有辨識到文字，請重新框選或調整聊天字體大小）" : recognition.Text.Trim();
        if (recognition.PreviewPng is { Length: > 0 })
        {
            using var memory = new MemoryStream(recognition.PreviewPng);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = memory;
            image.EndInit();
            image.Freeze();
            PreviewImage.Source = image;
        }
    }
}
