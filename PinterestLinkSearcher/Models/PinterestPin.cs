namespace PinterestLinkSearcher.Models;

public sealed class PinterestPin
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Board { get; set; } = string.Empty;
    public string Tags { get; set; } = "[]";
    public byte[] UrlHash { get; set; } = [];
}
