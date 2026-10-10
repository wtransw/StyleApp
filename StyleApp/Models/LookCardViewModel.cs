namespace StyleApp.Models;

public sealed class LookCardViewModel
{
    public StyleLook Look { get; set; } = new();
    public bool IsFavorite { get; set; }
    public bool IsWishlisted { get; set; }
    public string ReturnUrl { get; set; } = "/Home/Looks";
}
