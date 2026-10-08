using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Text;
using RiftLingo.Services;

namespace RiftLingo.Tests;

public sealed class GeminiScreenshotTranslationServiceTests
{
    [Fact]
    public async Task TranslateAsync_SendsScreenshotAndParsesStructuredMessages()
    {
        const string payload = """{"messages":[{"original":"[ALL] Faker: mid mia","translation":"[所有人] Faker：中路不見","language":"en"}]}""";
        var apiResponse = System.Text.Json.JsonSerializer.Serialize(new
        {
            candidates = new[] { new { content = new { parts = new[] { new { text = payload } } } } }
        });
        var handler = new StubHandler(apiResponse);
        var service = new GeminiScreenshotTranslationService(new HttpClient(handler));
        using var bitmap = new Bitmap(80, 40);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(Color.Black);

        var result = await service.TranslateAsync(bitmap, "gemini-key", "gemini-test-model", true, CancellationToken.None);

        var message = Assert.Single(result.Messages);
        Assert.Equal("[ALL] Faker: mid mia", message.Original);
        Assert.Equal("[所有人] Faker：中路不見", message.Translation);
        Assert.Equal("en", message.Language);
        Assert.NotEmpty(result.ScreenshotPng!);
        Assert.Equal("gemini-key", handler.ApiKey);
        Assert.Contains("gemini-test-model:generateContent", handler.RequestUri?.AbsoluteUri);
        Assert.Contains("image/png", handler.RequestBody);
        Assert.Contains("responseMimeType", handler.RequestBody);
    }

    [Fact]
    public async Task TranslateAsync_ReturnsFriendlyApiError()
    {
        var handler = new StubHandler("""{"error":{"message":"API key not valid"}}""", HttpStatusCode.BadRequest);
        var service = new GeminiScreenshotTranslationService(new HttpClient(handler));
        using var bitmap = new Bitmap(20, 20);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.TranslateAsync(bitmap, "bad-key", "gemini-test-model", false, CancellationToken.None));

        Assert.Contains("API key not valid", exception.Message);
    }

    private sealed class StubHandler(string json, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string RequestBody { get; private set; } = string.Empty;
        public string? ApiKey { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            ApiKey = request.Headers.TryGetValues("x-goog-api-key", out var values) ? values.SingleOrDefault() : null;
            RequestBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(statusCode) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }
}
