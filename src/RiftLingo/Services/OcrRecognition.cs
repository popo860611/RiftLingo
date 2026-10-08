namespace RiftLingo.Services;

public sealed record OcrRecognition(string Text, float Confidence, string PreprocessingMode, byte[]? PreviewPng = null);
