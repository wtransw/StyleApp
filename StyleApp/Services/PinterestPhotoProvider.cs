using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using StyleApp.Models;

namespace StyleApp.Services;

public sealed class PinterestPhotoProvider(IConfiguration configuration) : IStylePhotoProvider
{
    private static readonly Regex GenderPattern = new(
        @"\b(women|woman|men|man|female|male)(?:'s)?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly string _connectionString = configuration.GetConnectionString("PinterestPins")?.Trim() ?? string.Empty;

    public string ProviderName => "Pinterest";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);
    public string SetupMessage => "Set ConnectionStrings:PinterestPins to the SQL Server database used by PinterestLinkSearcher.";

    public async Task<IReadOnlyList<StyleLook>> SearchAsync(
        string query,
        IReadOnlyList<string> tags,
        string season,
        string style,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return [];
        }

        var gender = ResolveGender(query, tags);
        if (gender is null)
        {
            //return [];
        }

        var otherTags = tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => GenderPattern.Replace(tag, string.Empty).Trim())
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var looks = new List<StyleLook>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Url], [Tags] FROM [dbo].[PinterestPins] ORDER BY [datetime] DESC";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var imageUrl = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var tagsJson = reader.IsDBNull(2) ? "[]" : reader.GetString(2);
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                continue;
            }

            var pinTags = DeserializeTags(tagsJson);
            //if (!pinTags.Any(tag => IsGenderTag(tag, gender)))
            //{
            //    continue;
            //}

            if (pinTags.Any())
            {
                ;
            }

            if (otherTags.Length > 0 && !pinTags.Any(pinTag => otherTags.Any(filterTag =>
                    pinTag.Contains(filterTag, StringComparison.OrdinalIgnoreCase) ||
                    filterTag.Contains(pinTag, StringComparison.OrdinalIgnoreCase))))
            {
                continue;
            }

            var id = reader.GetInt32(0);
            looks.Add(new StyleLook
            {
                Id = id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ImageUrl = imageUrl,
                ThumbnailUrl = imageUrl,
                Description = string.Join(", ", pinTags),
                PhotographerName = "Pinterest",
                PhotographerUrl = imageUrl,
                PhotoPageUrl = imageUrl,
                Tags = pinTags,
                Season = season,
                Style = style
            });
        }

        return looks;
    }

    private static string? ResolveGender(string query, IReadOnlyList<string> tags)
    {
        var matches = tags
            .Append(query)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => GenderPattern.Matches(value).Cast<Match>().Select(match => NormalizeGender(match.Groups[1].Value)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return matches.Length == 1 ? matches[0] : null;
    }

    private static string NormalizeGender(string value) => value.ToLowerInvariant() switch
    {
        "man" or "men" or "male" => "men",
        _ => "women"
    };

    private static bool IsGenderTag(string tag, string gender)
    {
        var normalized = tag.Trim().Trim('\'', '"', '.', ',', ';', ':').ToLowerInvariant();
        return gender == "men"
            ? normalized is "man" or "men" or "male" or "mens" or "men's"
            : normalized is "woman" or "women" or "female" or "womens" or "women's";
    }

    private static List<string> DeserializeTags(string json)
    {
        try
        {
            return (JsonSerializer.Deserialize<List<string?>>(json) ?? [])
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag!.Trim())
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
