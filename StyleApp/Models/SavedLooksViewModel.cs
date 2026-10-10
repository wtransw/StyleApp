namespace StyleApp.Models;

public sealed class SavedLooksViewModel
{
    public PersonalStyleState State { get; set; } = new();
    public IReadOnlyList<StyleLook> Looks { get; set; } = [];
    public string Heading { get; set; } = string.Empty;
    public string EmptyMessage { get; set; } = string.Empty;
}
