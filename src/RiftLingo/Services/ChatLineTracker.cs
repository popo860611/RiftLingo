using System.Text.RegularExpressions;

namespace RiftLingo.Services;

public sealed partial class ChatLineTracker
{
    private readonly Queue<string> _recent = new();
    private readonly HashSet<string> _known = new(StringComparer.OrdinalIgnoreCase);
    private const int MaxRememberedLines = 80;

    public IReadOnlyList<string> FindNewLines(string ocrText)
    {
        var result = new List<string>();
        foreach (var rawLine in ocrText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = WhitespaceRegex().Replace(rawLine, " ").Trim(' ', '|', '[', ']');
            if (line.Length < 2 || !_known.Add(line))
            {
                continue;
            }

            _recent.Enqueue(line);
            result.Add(line);
            while (_recent.Count > MaxRememberedLines)
            {
                _known.Remove(_recent.Dequeue());
            }
        }

        return result;
    }

    public void Reset()
    {
        _recent.Clear();
        _known.Clear();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
