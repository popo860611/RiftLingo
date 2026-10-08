using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using RiftLingo.Models;
using RiftLingo.Services;
using RiftLingo.Windows;

namespace RiftLingo;

public partial class MainWindow : Window
{
    private readonly SettingsStore _settingsStore = new();
    private readonly ScreenCaptureService _captureService = new();
    private readonly OcrService _ocrService = new();
    private readonly ChatLineTracker _lineTracker = new();
    private readonly FrameChangeDetector _frameChangeDetector = new();
    private readonly TaiwaneseLolLocalizer _localizer = new();
    private readonly GoogleTranslationService _translationService = new(new HttpClient { Timeout = TimeSpan.FromSeconds(12) });
    private readonly GlobalHotkeyService _hotkeys = new();
    private readonly OverlayWindow _overlay = new();
    private AppSettings _settings;
    private CancellationTokenSource? _captureCancellation;
    private bool _isPaused;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsStore.Load();
        LoadSettingsIntoUi();
        SourceInitialized += OnSourceInitialized;
        Closing += OnClosing;
        _hotkeys.ToggleOverlayRequested += (_, _) => ToggleOverlay();
        _hotkeys.TogglePauseRequested += (_, _) => TogglePause();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        try { _hotkeys.Register(new WindowInteropHelper(this).Handle); }
        catch (Exception exception) { SetStatus("快捷鍵無法使用", false, exception.Message); }
    }

    private async void SelectRegionButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        await Task.Delay(250);
        var selector = new RegionSelectorWindow();
        selector.ShowDialog();
        Show();
        Activate();
        if (selector.SelectedRegion is { IsValid: true } region)
        {
            _settings.CaptureRegion = region;
            RegionText.Text = region.ToString();
            _overlay.PositionNear(region);
            SaveSettingsFromUi();
            SetStatus("聊天區已設定", false);
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_settings.CaptureRegion is not { IsValid: true })
        {
            MessageBox.Show(this, "請先框選遊戲內的聊天區。", "RiftLingo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        SaveSettingsFromUi();
        var missing = _ocrService.MissingLanguages(_settings.OcrLanguages);
        if (missing.Count > 0)
        {
            MessageBox.Show(this, $"缺少 OCR 模型：{string.Join(", ", missing)}\n\n請先執行 scripts\\download-models.ps1。", "尚未安裝 OCR 模型", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ApiKeyBox.Password))
        {
            MessageBox.Show(this, "請輸入 Google Cloud Translation API 金鑰。", "RiftLingo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        _lineTracker.Reset();
        _frameChangeDetector.Reset();
        _captureCancellation = new CancellationTokenSource();
        _isPaused = false;
        StartButton.IsEnabled = false;
        PreviewButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        _overlay.ApplySettings(_settings);
        _overlay.PositionNear(_settings.CaptureRegion);
        _overlay.Show();
        SetStatus("翻譯中", true);
        await RunCaptureLoopAsync(_captureCancellation.Token);
    }

    private void StopButton_Click(object sender, RoutedEventArgs e) => StopTranslation();

    private async Task RunCaptureLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isPaused && _settings.CaptureRegion is { } region)
                {
                    var text = await Task.Run(() =>
                    {
                        using var bitmap = _captureService.Capture(region);
                        return _frameChangeDetector.HasMeaningfulChange(bitmap)
                            ? _ocrService.Recognize(bitmap, _settings.OcrLanguages)
                            : string.Empty;
                    }, cancellationToken);
                    foreach (var line in _lineTracker.FindNewLines(text).TakeLast(4))
                    {
                        var result = await _translationService.TranslateAsync(line, ApiKeyBox.Password, cancellationToken);
                        _overlay.AddMessage(new ChatMessage(line, _localizer.Localize(result.Text), result.DetectedLanguage));
                    }
                }
                await Task.Delay(_settings.CaptureIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            SetStatus("翻譯已停止", false, exception.Message);
            MessageBox.Show(this, exception.Message, "RiftLingo 發生問題", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            _captureCancellation?.Dispose();
            _captureCancellation = null;
            StartButton.IsEnabled = true;
            PreviewButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            if (!_isPaused) SetStatus("尚未啟動", false);
        }
    }

    private async void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (_settings.CaptureRegion is not { IsValid: true } region)
        {
            MessageBox.Show(this, "請先框選遊戲內的聊天區。", "RiftLingo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveSettingsFromUi();
        var missing = _ocrService.MissingLanguages(_settings.OcrLanguages);
        if (missing.Count > 0)
        {
            MessageBox.Show(this, $"缺少 OCR 模型：{string.Join(", ", missing)}", "尚未安裝 OCR 模型", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        PreviewButton.IsEnabled = false;
        SetStatus("正在測試辨識", true);
        try
        {
            var recognition = await Task.Run(() =>
            {
                using var bitmap = _captureService.Capture(region);
                return _ocrService.RecognizeDetailed(bitmap, _settings.OcrLanguages, includePreview: true);
            });
            new OcrPreviewWindow(recognition) { Owner = this }.ShowDialog();
            SetStatus("辨識測試完成", false);
        }
        catch (Exception exception)
        {
            SetStatus("辨識測試失敗", false, exception.Message);
            MessageBox.Show(this, exception.Message, "無法測試辨識", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            PreviewButton.IsEnabled = true;
        }
    }

    private void ToggleOverlay() { if (_overlay.IsVisible) _overlay.Hide(); else if (_captureCancellation is not null) _overlay.Show(); }
    private void TogglePause() { if (_captureCancellation is null) return; _isPaused = !_isPaused; SetStatus(_isPaused ? "已暫停" : "翻譯中", !_isPaused); }
    private void StopTranslation() { _captureCancellation?.Cancel(); _overlay.Hide(); _isPaused = false; }

    private void LoadSettingsIntoUi()
    {
        RegionText.Text = _settings.CaptureRegion?.ToString() ?? "尚未選擇";
        OcrLanguagesBox.Text = _settings.OcrLanguages;
        ApiKeyBox.Password = _settingsStore.GetApiKey(_settings);
        foreach (var item in IntervalBox.Items.OfType<ComboBoxItem>())
            if (int.TryParse(item.Tag?.ToString(), out var interval) && interval == _settings.CaptureIntervalMs) { IntervalBox.SelectedItem = item; break; }
    }

    private void SaveSettingsFromUi()
    {
        _settings.OcrLanguages = string.IsNullOrWhiteSpace(OcrLanguagesBox.Text) ? "eng+jpn+kor+vie+tha+ind" : OcrLanguagesBox.Text.Trim();
        if (IntervalBox.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag?.ToString(), out var interval)) _settings.CaptureIntervalMs = interval;
        _settingsStore.Save(_settings, ApiKeyBox.Password);
    }

    private void SetStatus(string text, bool active, string? tooltip = null)
    {
        StatusText.Text = text; StatusText.ToolTip = tooltip;
        StatusDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(active ? "#22C55E" : "#94A3B8"));
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        StopTranslation(); SaveSettingsFromUi(); _hotkeys.Dispose(); _ocrService.Dispose(); _overlay.Close();
    }
}
