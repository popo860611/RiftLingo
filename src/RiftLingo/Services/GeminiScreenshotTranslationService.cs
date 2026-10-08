using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using RiftLingo.Models;

namespace RiftLingo.Services;

public sealed record GeminiScreenshotTranslation(IReadOnlyList<ChatMessage> Messages, byte[]? ScreenshotPng = null);

public sealed class GeminiScreenshotTranslationService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<GeminiScreenshotTranslation> TranslateAsync(
        Bitmap screenshot,
        string apiKey,
        string model,
        bool includeScreenshot,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentException("請輸入 Gemini API Key。", nameof(apiKey));

        var png = EncodePng(screenshot);
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = TranslationPrompt },
                        new { inline_data = new { mime_type = "image/png", data = Convert.ToBase64String(png) } }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 1200,
                responseMimeType = "application/json",
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        messages = new
                        {
                            type = "ARRAY",
                            items = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    original = new { type = "STRING" },
                                    translation = new { type = "STRING" },
                                    language = new { type = "STRING" }
                                },
                                required = new[] { "original", "translation", "language" }
                            }
                        }
                    },
                    required = new[] { "messages" }
                }
            }
        };

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("x-goog-api-key", apiKey.Trim());
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(ParseApiError(responseBody, response.StatusCode.ToString()), null, response.StatusCode);
        }

        var payloadJson = ExtractResponseText(responseBody);
        var payload = JsonSerializer.Deserialize<TranslationPayload>(payloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Gemini 沒有回傳可讀取的翻譯結果。");
        var messages = payload.Messages
            .Where(message => !string.IsNullOrWhiteSpace(message.Original) && !string.IsNullOrWhiteSpace(message.Translation))
            .Select(message => new ChatMessage(message.Original.Trim(), message.Translation.Trim(), message.Language.Trim()))
            .ToArray();
        return new GeminiScreenshotTranslation(messages, includeScreenshot ? png : null);
    }

    private static byte[] EncodePng(Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        return memory.ToArray();
    }

    private static string ExtractResponseText(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        if (document.RootElement.TryGetProperty("candidates", out var candidates)
            && candidates.GetArrayLength() > 0
            && candidates[0].TryGetProperty("content", out var content)
            && content.TryGetProperty("parts", out var parts)
            && parts.GetArrayLength() > 0
            && parts[0].TryGetProperty("text", out var text))
        {
            return text.GetString() ?? string.Empty;
        }

        throw new InvalidOperationException("Gemini 回應中沒有翻譯文字，可能是圖片被安全政策攔截或額度已用完。");
    }

    private static string ParseApiError(string responseBody, string fallback)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            return document.RootElement.GetProperty("error").GetProperty("message").GetString() ?? fallback;
        }
        catch
        {
            return $"Gemini API 請求失敗：{fallback}";
        }
    }

    private const string TranslationPrompt = """
        你是《英雄聯盟》對局聊天室翻譯器。請只讀取圖片內真正由玩家輸入的聊天訊息，依畫面由上到下排列。
        忽略系統提示、擊殺資訊、購買資訊、技能文字、小地圖、Ping 與其他遊戲介面文字；不要猜測看不清楚的內容。
        original 必須保留圖片中可見的玩家名稱、頻道與原文；translation 翻成自然的台灣繁體中文英雄聯盟口語。
        保留髒話、嘲諷與原本攻擊語氣，不消音、不審查，也不要自行加重或新增原意。LoL 縮寫與術語要依對局語境翻譯。
        language 使用簡短語言代碼，例如 en、ja、ko、vi、th、id。若沒有玩家聊天，messages 回傳空陣列。
        """;

    private sealed record TranslationPayload(IReadOnlyList<TranslationMessage> Messages);
    private sealed record TranslationMessage(string Original, string Translation, string Language);
}
