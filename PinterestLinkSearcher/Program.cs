using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PinterestLinkSearcher.Data;
using PinterestLinkSearcher.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Set ConnectionStrings:DefaultConnection in PinterestLinkSearcher/appsettings.json, then restart the app.");
    return;
}

var scraper = new PinterestBoardScraper();
var keepRunning = true;

while (keepRunning)
{
    Console.WriteLine();
    Console.WriteLine("Pinterest Link Searcher");
    Console.WriteLine("1. Collect pins from a Pinterest board");
    Console.WriteLine("2. Exit");
    Console.Write("Select an option: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            await CollectBoardAsync(connectionString, scraper);
            break;
        case "2":
            keepRunning = false;
            break;
        default:
            Console.WriteLine("Choose 1 or 2.");
            break;
    }
}

static async Task CollectBoardAsync(string connectionString, PinterestBoardScraper scraper)
{
    Console.Write("Pinterest board URL: ");
    var input = Console.ReadLine()?.Trim();
    if (!TryGetBoard(input, out var boardUrl, out var board))
    {
        Console.WriteLine("Enter a Pinterest board URL, for example https://www.pinterest.com/lookastic/celeb-style/.");
        return;
    }

    try
    {
        Console.WriteLine($"Opening board '{board}'. Sign in manually in the browser if Pinterest asks.");
        var pins = await scraper.ScrapeAsync(boardUrl);
        Console.WriteLine($"Collected {pins.Count} unique pin links.");
        if (pins.Count == 0)
        {
            Console.WriteLine("No pin links were found. The board may require sign-in or Pinterest may have changed its page markup.");
            return;
        }

        var options = new DbContextOptionsBuilder<PinterestDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new PinterestDbContext(options);
        var store = new PinterestPinStore(context);
        var added = await store.SaveAsync(board, pins);
        Console.WriteLine($"Saved {added} new pins to SQL Server. Existing matching URLs were not duplicated.");
        Console.WriteLine("Tags were inferred from Pinterest text and image alt text where available.");
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        Console.WriteLine($"Collection failed: {exception.Message}");
    }
}

static bool TryGetBoard(string? input, out string boardUrl, out string board)
{
    boardUrl = string.Empty;
    board = string.Empty;
    if (!Uri.TryCreate(input, UriKind.Absolute, out var uri) ||
        uri.Scheme != Uri.UriSchemeHttps ||
        !(uri.Host.Equals("pinterest.com", StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith(".pinterest.com", StringComparison.OrdinalIgnoreCase)))
    {
        return false;
    }

    var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
    if (segments.Length < 2 || segments.Any(segment => segment.Equals("pin", StringComparison.OrdinalIgnoreCase)))
    {
        return false;
    }

    board = Uri.UnescapeDataString(segments[^1]);
    boardUrl = uri.GetLeftPart(UriPartial.Path).TrimEnd('/') + "/";
    return true;
}