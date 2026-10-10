namespace StyleApp.Models;

public sealed class WardrobeItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Season { get; set; } = "All season";
    public string Style { get; set; } = "Casual";
    public string? ImageUrl { get; set; }
}
