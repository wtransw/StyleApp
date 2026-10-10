using Microsoft.Playwright;

namespace PinterestLinkSearcher.Services;

public sealed class PinterestBoardScraper
{
    private const int StableScrollsBeforeStop = 7;
    private const int MaximumScrolls = 300;
    private readonly string _profilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PinterestLinkSearcher",
        "BrowserProfile");

    public async Task<IReadOnlyList<ScrapedPin>> ScrapeAsync(string boardUrl, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_profilePath);
        using var playwright = await Playwright.CreateAsync();
        await using var context = await playwright.Chromium.LaunchPersistentContextAsync(_profilePath, new BrowserTypeLaunchPersistentContextOptions
        {
            Headless = false,
            ViewportSize = new ViewportSize { Width = 1440, Height = 1000 }
        });

        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();
        await page.GotoAsync(boardUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });
        await page.WaitForTimeoutAsync(2500);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsLoginPage(page.Url) ||
            await page.Locator("input[name='id'], input[name='password']").CountAsync() > 0 ||
            await page.Locator("div[data-test-id='login-modal-redesign']").CountAsync() > 0)
        {
            Console.WriteLine("Login and then hit enter in the console beyotch.");
            var ready = Console.ReadLine();

            Console.WriteLine("Pinterest sign-in detected. Sign in manually in the browser; searching will start in 30 seconds.");
            await page.GotoAsync(boardUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60_000 });
            await page.WaitForTimeoutAsync(2500);
        }

        var pins = new Dictionary<string, ScrapedPin>(StringComparer.OrdinalIgnoreCase);
        var stableScrolls = 0;

        for (var scroll = 0; scroll < MaximumScrolls && stableScrolls < StableScrollsBeforeStop; scroll++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pagePins = await ReadPinsAsync(page);
            var newPins = 0;
            foreach (var pin in pagePins)
            {
                var normalizedUrl = NormalizeImageUrl(pin.Url);
                if (normalizedUrl is null)
                {
                    continue;
                }

                if (pins.TryGetValue(normalizedUrl, out var existing))
                {
                    if (string.IsNullOrWhiteSpace(existing.Description))
                    {
                        existing.Description = pin.Description;
                    }
                }
                else
                {
                    pins[normalizedUrl] = new ScrapedPin
                    {
                        Url = normalizedUrl,
                        Description = pin.Description
                    };
                    newPins++;
                }
            }

            Console.Write($"\rFound {pins.Count} unique pins (scroll {scroll + 1})   ");
            if (await page.GetByText("Zoek meer ideeën", new() { Exact = false }).CountAsync() > 0)
            {
                break;
            }

            stableScrolls = newPins == 0 ? stableScrolls + 1 : 0;
            await page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(1100);
        }

        Console.WriteLine();
        return pins.Values.ToList();
    }

    private static async Task<List<ScrapedPin>> ReadPinsAsync(IPage page)
    {
        var json = await page.EvaluateAsync<string>(
            """
            () => JSON.stringify(
                Array.from(document.querySelectorAll('a[href*="/pin/"]'))
                    .map(anchor => {
                        const image = anchor.querySelector('img');
                        if (!image) return null;

                        const candidates = (image.getAttribute('srcset') || '')
                            .split(',')
                            .map(candidate => {
                                const [url, descriptor] = candidate.trim().split(/\s+/);
                                return { url, width: Number.parseInt(descriptor, 10) || 0 };
                            })
                            .filter(candidate => candidate.url)
                            .sort((left, right) => right.width - left.width);
                        const imageUrl = candidates[0]?.url || image.currentSrc || image.src;
                        if (!imageUrl) return null;

                        return {
                            url: imageUrl,
                            description: anchor.getAttribute('aria-label') || ''
                        };
                    })
                    .filter(pin => pin && pin.url)
            )
            """);

        return System.Text.Json.JsonSerializer.Deserialize<List<ScrapedPin>>(
            json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<ScrapedPin>();
    }

    private static string? NormalizeImageUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            !uri.Host.Equals("i.pinimg.com", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments.Length < 2)
        {
            return null;
        }

        var size = pathSegments[0];
        if (size.EndsWith('x') && int.TryParse(size[..^1], out _))
        {
            pathSegments[0] = "1200x";
        }

        var uriBuilder = new UriBuilder(uri)
        {
            Path = "/" + string.Join('/', pathSegments)
        };
        return uriBuilder.Uri.AbsoluteUri;
    }

    private static bool IsLoginPage(string url) =>
        url.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
        url.Contains("/auth", StringComparison.OrdinalIgnoreCase);
}

public sealed class ScrapedPin
{
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
