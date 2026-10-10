namespace StyleApp.Models;

public sealed class ExploreViewModel
{
    public PersonalStyleState State { get; set; } = new();
    public IReadOnlyList<StyleLook> Looks { get; set; } = [];
    public string Query { get; set; } = string.Empty;
    public string Season { get; set; } = "All season";
    public string Style { get; set; } = "Any style";
    public bool IsConfigured { get; set; }
    public bool IsSuggestion { get; set; }
    public string? Error { get; set; }
    public List<Guid> SelectedItemIds { get; set; } = [];
}
