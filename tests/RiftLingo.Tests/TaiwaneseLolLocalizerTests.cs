using RiftLingo.Services;

namespace RiftLingo.Tests;

public sealed class TaiwaneseLolLocalizerTests
{
    [Theory]
    [InlineData("這是打野差異", "這是打野差距")]
    [InlineData("中路失蹤，不要戰鬥", "中路不見，別打")]
    [InlineData("去擊殺納什男爵", "去擊殺巴龍")]
    [InlineData("報告上路", "檢舉上路")]
    public void Localize_UsesTaiwanLolVocabulary(string source, string expected)
    {
        var localizer = new TaiwaneseLolLocalizer();

        Assert.Equal(expected, localizer.Localize(source));
    }

    [Fact]
    public void Localize_DoesNotCensorProfanity()
    {
        var localizer = new TaiwaneseLolLocalizer();

        Assert.Equal("這打野他媽的完全沒用", localizer.Localize("這打野他媽的完全沒用"));
    }
}
