namespace RiftLingo.Models;

public sealed class AppSettings
{
    public CaptureRegion? CaptureRegion { get; set; }
    public int CaptureIntervalMs { get; set; } = 800;
    public string GeminiModel { get; set; } = "gemini-3.5-flash-lite";
    public double OverlayOpacity { get; set; } = 0.92;
    public double OverlayFontSize { get; set; } = 18;
    public bool ShowOriginal { get; set; } = true;
    public string EncryptedGeminiApiKey { get; set; } = string.Empty;
}
