using RiftLingo.Services;

namespace RiftLingo.Tests;

public sealed class ChatLineTrackerTests
{
    [Fact]
    public void FindNewLines_NormalizesAndDeduplicatesAcrossFrames()
    {
        var tracker = new ChatLineTracker();

        var first = tracker.FindNewLines("mid   mia\ncome drake");
        var second = tracker.FindNewLines("mid mia\ncome drake\nbot no flash");

        Assert.Equal(new[] { "mid mia", "come drake" }, first);
        Assert.Equal(new[] { "bot no flash" }, second);
    }

    [Fact]
    public void Reset_AllowsPreviouslySeenLineAgain()
    {
        var tracker = new ChatLineTracker();
        tracker.FindNewLines("gg go next");

        tracker.Reset();

        Assert.Single(tracker.FindNewLines("gg go next"));
    }
}
