using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RiftLingo.Services;

public sealed class GoogleTranslationService(HttpClient httpClient)
{
    public async Task<TranslationResult> TranslateAsync(string text, string apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("請先輸入 Google Cloud Translation API 金鑰。");
        }

        var endpoint = $"https://translation.googleapis.com/language/translate/v2?key={Uri.EscapeDataString(apiKey)}";
        using var response = await httpClient.PostAsJsonAsync(endpoint, new
        {
            q = text,
            target = "zh-TW",
            format = "text"
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Google 翻譯失敗（{(int)response.StatusCode}）：{Trim(detail, 180)}");
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleResponse>(cancellationToken: cancellationToken);
        var item = payload?.Data?.Translations?.FirstOrDefault()
            ?? throw new InvalidOperationException("Google 翻譯沒有回傳內容。");

        return new TranslationResult(
            WebUtility.HtmlDecode(item.TranslatedText ?? string.Empty),
            item.DetectedSourceLanguage ?? "auto");
    }

    private static string Trim(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength] + "…";

    private sealed class GoogleResponse
    {
        [JsonPropertyName("data")]
        public GoogleData? Data { get; init; }
    }

    private sealed class GoogleData
    {
        [JsonPropertyName("translations")]
        public List<GoogleTranslation>? Translations { get; init; }
    }

    private sealed class GoogleTranslation
    {
        [JsonPropertyName("translatedText")]
        public string? TranslatedText { get; init; }

        [JsonPropertyName("detectedSourceLanguage")]
        public string? DetectedSourceLanguage { get; init; }
    }
}

public sealed record TranslationResult(string Text, string DetectedLanguage);
