namespace StyleApp.Models;

public sealed class PersonalStyleState
{
    public string Gender { get; set; } = "woman";
    public List<WardrobeItem> Wardrobe { get; set; } = [];
    public List<StyleLook> Favorites { get; set; } = [];
    public List<StyleLook> Wishlist { get; set; } = [];
}
