using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PinterestLinkSearcher.Data;
using PinterestLinkSearcher.Models;

namespace PinterestLinkSearcher.Services;

public sealed class PinterestPinStore(PinterestDbContext dbContext)
{
    public async Task<int> SaveAsync(string board, IReadOnlyList<ScrapedPin> scrapedPins, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        var saved = 0;

        foreach (var scrapedPin in scrapedPins)
        {
            var normalizedUrl = scrapedPin.Url.Trim();
            var urlHash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedUrl));
            var tags = PinTagExtractor.Extract(scrapedPin.Description);
            var tagsJson = JsonSerializer.Serialize(tags);
            var existing = await dbContext.Pins.SingleOrDefaultAsync(pin => pin.Url == normalizedUrl, cancellationToken);

            if (existing is null)
            {
                dbContext.Pins.Add(new PinterestPin
                {
                    DateTime = DateTime.UtcNow,
                    Url = normalizedUrl,
                    Board = board,
                    Tags = tagsJson,
                    UrlHash = urlHash
                });
                saved++;
            }
            else
            {
                existing.Tags = tagsJson;
                existing.Board = board;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return saved;
    }
}
