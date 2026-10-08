namespace RiftLingo.Models;

public sealed class AppSettings
{
    public CaptureRegion? CaptureRegion { get; set; }
    public string OcrLanguages { get; set; } = "eng+jpn+kor+vie+tha+ind";
    public int CaptureIntervalMs { get; set; } = 800;
    public double OverlayOpacity { get; set; } = 0.92;
    public double OverlayFontSize { get; set; } = 18;
    public bool ShowOriginal { get; set; } = true;
    public string EncryptedGoogleApiKey { get; set; } = string.Empty;
}
