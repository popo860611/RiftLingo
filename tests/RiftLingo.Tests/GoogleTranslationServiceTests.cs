using System.Net;
using System.Net.Http;
using System.Text;
using RiftLingo.Services;

namespace RiftLingo.Tests;

public sealed class GoogleTranslationServiceTests
{
    [Fact]
    public async Task TranslateAsync_ParsesLanguageAndDecodesHtml()
    {
        var handler = new StubHandler("""{"data":{"translations":[{"translatedText":"你&amp;我","detectedSourceLanguage":"ja"}]}}""");
        var service = new GoogleTranslationService(new HttpClient(handler));

        var result = await service.TranslateAsync("あなたと私", "test-key", CancellationToken.None);

        Assert.Equal("你&我", result.Text);
        Assert.Equal("ja", result.DetectedLanguage);
        Assert.Contains("key=test-key", handler.RequestUri?.Query);
        Assert.Contains("zh-TW", handler.RequestBody);
    }

    private sealed class StubHandler(string json) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            RequestBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
