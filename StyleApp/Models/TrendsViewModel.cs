namespace StyleApp.Models;

public sealed class TrendsViewModel
{
    public string Gender { get; set; } = "woman";
    public IReadOnlyList<TrendCard> Trends { get; set; } = [];
}
