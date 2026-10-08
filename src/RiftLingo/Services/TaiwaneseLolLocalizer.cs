using System.Text.RegularExpressions;

namespace RiftLingo.Services;

public sealed partial class TaiwaneseLolLocalizer
{
    private static readonly (Regex Pattern, string Replacement)[] Rules =
    {
        (Word("打野差異"), "打野差距"),
        (Word("中路失蹤"), "中路不見"),
        (Word("下路失蹤"), "下路不見"),
        (Word("上路失蹤"), "上路不見"),
        (Word("終極技能"), "大招"),
        (Word("大招技能"), "大招"),
        (Word("召喚師閃現"), "閃現"),
        (Word("視野守衛"), "眼"),
        (Word("撤退吧"), "先退"),
        (Word("不要戰鬥"), "別打"),
        (Word("下一場比賽"), "下一把"),
        (Word("報告上路"), "檢舉上路"),
        (Word("報告中路"), "檢舉中路"),
        (Word("報告打野"), "檢舉打野"),
        (Word("報告下路"), "檢舉下路"),
        (Word("報告輔助"), "檢舉輔助"),
        (Word("小龍"), "龍"),
        (Word("納什男爵"), "巴龍"),
        (Word("峽谷先鋒"), "預示者")
    };

    public string Localize(string translatedText)
    {
        var result = translatedText.Trim();
        foreach (var (pattern, replacement) in Rules)
        {
            result = pattern.Replace(result, replacement);
        }

        return MultipleSpacesRegex().Replace(result, " ");
    }

    private static Regex Word(string value) => new(Regex.Escape(value), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [GeneratedRegex(@"[ \t]{2,}")]
    private static partial Regex MultipleSpacesRegex();
}
