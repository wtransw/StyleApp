namespace StyleApp.Models;

public sealed class LookFormInput
{
    public string Id { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PhotographerName { get; set; } = string.Empty;
    public string PhotographerUrl { get; set; } = string.Empty;
    public string PhotoPageUrl { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string Season { get; set; } = "All season";
    public string Style { get; set; } = "Everyday";
    public string ReturnUrl { get; set; } = "/Home/Looks";

    public StyleLook ToStyleLook() => new()
    {
        Id = Id,
        ImageUrl = ImageUrl,
        ThumbnailUrl = ThumbnailUrl,
        Description = Description,
        PhotographerName = PhotographerName,
        PhotographerUrl = PhotographerUrl,
        PhotoPageUrl = PhotoPageUrl,
        Tags = Tags.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
        Season = Season,
        Style = Style
    };
}
