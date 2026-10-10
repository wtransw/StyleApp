namespace StyleApp.Models;

public sealed class StyleLook
{
    public string Id { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PhotographerName { get; set; } = string.Empty;
    public string PhotographerUrl { get; set; } = string.Empty;
    public string PhotoPageUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string Season { get; set; } = "All season";
    public string Style { get; set; } = "Everyday";
}
