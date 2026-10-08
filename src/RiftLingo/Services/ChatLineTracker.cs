using System.Text.RegularExpressions;

namespace RiftLingo.Services;

public sealed partial class ChatLineTracker
{
    private readonly Queue<string> _recent = new();
    private readonly HashSet<string> _known = new(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<string> _recentComparable = new();
    private const int MaxRememberedLines = 80;

    public IReadOnlyList<string> FindNewLines(string ocrText)
    {
        var result = new List<string>();
        foreach (var rawLine in ocrText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = WhitespaceRegex().Replace(rawLine, " ").Trim(' ', '|', '[', ']');
            var comparable = Comparable(line);
            if (line.Length < 2 || _known.Contains(line) || IsNearDuplicate(comparable))
            {
                continue;
            }

            _known.Add(line);
            _recent.Enqueue(line);
            _recentComparable.Enqueue(comparable);
            result.Add(line);
            while (_recent.Count > MaxRememberedLines)
            {
                _known.Remove(_recent.Dequeue());
                _recentComparable.Dequeue();
            }
        }

        return result;
    }

    public void Reset()
    {
        _recent.Clear();
        _known.Clear();
        _recentComparable.Clear();
    }

    private bool IsNearDuplicate(string candidate)
    {
        if (candidate.Length < 6) return false;
        return _recentComparable.Any(previous => Similarity(previous, candidate) >= 0.9);
    }

    private static string Comparable(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static double Similarity(string left, string right)
    {
        var longest = Math.Max(left.Length, right.Length);
        if (longest == 0) return 1;
        var previous = Enumerable.Range(0, right.Length + 1).ToArray();
        for (var i = 1; i <= left.Length; i++)
        {
            var current = new int[right.Length + 1];
            current[0] = i;
            for (var j = 1; j <= right.Length; j++)
            {
                var substitution = previous[j - 1] + (left[i - 1] == right[j - 1] ? 0 : 1);
                current[j] = Math.Min(Math.Min(previous[j] + 1, current[j - 1] + 1), substitution);
            }
            previous = current;
        }
        return 1 - previous[^1] / (double)longest;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
