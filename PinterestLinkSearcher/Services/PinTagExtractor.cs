using System.Text.RegularExpressions;

namespace PinterestLinkSearcher.Services;

public static partial class PinTagExtractor
{
    public static IReadOnlyList<string> Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var description = Regex.Replace(
            text.Trim(),
            @"^.*?\s+in\s+(?:a|an)\s+",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        return new List<string>();

        //return Regex.Split(description, @"\s+and\s+|[,;]")
        //    .Select(tag => Regex.Replace(tag.Trim(), @"^(?:a|an|the)\s+", string.Empty, RegexOptions.IgnoreCase))
        //    .Select(tag => tag.Trim().Trim('.', '!', '?'))
        //    .Where(tag => tag.Length > 0)
        //    .Distinct(StringComparer.OrdinalIgnoreCase)
        //    .Take(20)
        //    .ToList();
    }
}
