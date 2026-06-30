namespace LTools.Core.Services;

public static class StringExtensions
{
    public static string StripAnsi(this string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        int idx;
        while ((idx = text.IndexOf('\x1b', StringComparison.Ordinal)) >= 0)
        {
            var end = text.IndexOf('m', idx);
            if (end < 0) end = text.IndexOf('K', idx);
            if (end < 0) end = text.IndexOf('H', idx);
            if (end < 0) break;
            text = text[..idx] + text[(end + 1)..];
        }
        return text;
    }
}
