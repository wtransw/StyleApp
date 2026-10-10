using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StyleApp.Models;
using StyleApp.Services;

namespace StyleApp.Controllers;

public class HomeController(IStyleStateStore stateStore, IStylePhotoService photoService) : Controller
{
    private static readonly IReadOnlyList<TrendCard> TrendsFor2026 =
    [
        new("Soft tailoring", "Relaxed blazers and fluid trousers bring polish without feeling formal.", "Silhouette", "Stone · ink · warm grey", "https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=900&q=85"),
        new("Modern utility", "Clean cargo details and practical layers make everyday outfits feel considered.", "Details", "Olive · khaki · cream", "https://images.unsplash.com/photo-1529139574466-a303027c1d8b?auto=format&fit=crop&w=900&q=85"),
        new("Elevated everyday denim", "Dark, straight-leg denim pairs easily with refined knits and simple accessories.", "Fabric", "Indigo · ecru · cognac", "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?auto=format&fit=crop&w=900&q=85"),
        new("Expressive color", "A focused pop of saturated color adds energy to a wardrobe of useful neutrals.", "Color", "Cobalt · butter yellow · burgundy", "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=900&q=85"),
        new("Texture mixing", "Lightweight knits, suede, and crisp cotton make simple layers more dimensional.", "Styling", "Chocolate · oat · soft blue", "https://images.unsplash.com/photo-1485230895905-ec40ba36b9bc?auto=format&fit=crop&w=900&q=85"),
        new("Thoughtful accessories", "A strong belt, practical bag, or classic eyewear gives a look a personal finish.", "Accessories", "Leather · silver · tortoiseshell", "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?auto=format&fit=crop&w=900&q=85")
    ];

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        ViewData["Gender"] = state.Gender;
        return View(state);
    }

    public async Task<IActionResult> Wardrobe(CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        ViewData["Gender"] = state.Gender;
        return View(state);
    }

    public async Task<IActionResult> Trends(CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        ViewData["Gender"] = state.Gender;
        return View(new TrendsViewModel { Gender = state.Gender, Trends = TrendsFor2026 });
    }

    public async Task<IActionResult> Looks(
        string? query,
        string? season,
        string? style,
        CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        var searchQuery = BuildSearchQuery(query, state.Gender, season, style);
        var model = await BuildExploreModel(state, searchQuery, season, style, false, [], cancellationToken);
        model.Query = query ?? string.Empty;
        return View("Looks", model);
    }

    public async Task<IActionResult> Suggestions(
        List<Guid>? itemIds,
        CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        var selectedIds = itemIds ?? [];
        var selectedItems = state.Wardrobe.Where(item => selectedIds.Contains(item.Id)).ToList();
        ExploreViewModel model;
        if (selectedItems.Count == 0)
        {
            ViewData["Gender"] = state.Gender;
            model = new ExploreViewModel { State = state, IsConfigured = photoService.IsConfigured, IsSuggestion = true };
        }
        else
        {
            var query = $"{state.Gender} outfit featuring {string.Join(" ", selectedItems.Select(item => item.Name))}";
            model = await BuildExploreModel(state, query, null, null, true, selectedItems.Select(item => item.Id).ToList(), cancellationToken);
        }
        return View(model);
    }

    public async Task<IActionResult> Favorites(CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        ViewData["Gender"] = state.Gender;
        return View("SavedLooks", new SavedLooksViewModel
        {
            State = state,
            Looks = state.Favorites,
            Heading = "Your favorites",
            EmptyMessage = "Favorite looks you love and they will be waiting here."
        });
    }

    public async Task<IActionResult> Wishlist(CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        ViewData["Gender"] = state.Gender;
        return View("SavedLooks", new SavedLooksViewModel
        {
            State = state,
            Looks = state.Wishlist,
            Heading = "Your wishlist",
            EmptyMessage = "Save pieces and outfit ideas you would like to try."
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetGender(string gender, string? returnUrl, CancellationToken cancellationToken)
    {
        if (gender is "man" or "woman")
        {
            var state = await stateStore.GetAsync(cancellationToken);
            state.Gender = gender;
            await stateStore.SaveAsync(state, cancellationToken);
        }

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWardrobeItem(WardrobeItem item, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return RedirectToAction(nameof(Wardrobe));
        }

        item.Id = Guid.NewGuid();
        item.Name = item.Name.Trim();
        item.Category = string.IsNullOrWhiteSpace(item.Category) ? "Other" : item.Category.Trim();
        item.Color = item.Color.Trim();
        item.ImageUrl = string.IsNullOrWhiteSpace(item.ImageUrl) ? null : item.ImageUrl.Trim();

        var state = await stateStore.GetAsync(cancellationToken);
        state.Wardrobe.Insert(0, item);
        await stateStore.SaveAsync(state, cancellationToken);
        return RedirectToAction(nameof(Wardrobe));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveWardrobeItem(Guid id, CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        state.Wardrobe.RemoveAll(item => item.Id == id);
        await stateStore.SaveAsync(state, cancellationToken);
        return RedirectToAction(nameof(Wardrobe));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(LookFormInput look, CancellationToken cancellationToken) =>
        await ToggleSavedLook(look, favorite: true, cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleWishlist(LookFormInput look, CancellationToken cancellationToken) =>
        await ToggleSavedLook(look, favorite: false, cancellationToken);

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private async Task<ExploreViewModel> BuildExploreModel(
        PersonalStyleState state,
        string query,
        string? season,
        string? style,
        bool isSuggestion,
        List<Guid> selectedItemIds,
        CancellationToken cancellationToken)
    {
        var currentSeason = string.IsNullOrWhiteSpace(season) ? "All season" : season;
        var currentStyle = string.IsNullOrWhiteSpace(style) ? "Any style" : style;
        ViewData["Gender"] = state.Gender;
        var tags = query.Split([',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => tag.Length > 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();
        var model = new ExploreViewModel
        {
            State = state,
            Query = query,
            Season = currentSeason,
            Style = currentStyle,
            IsConfigured = photoService.IsConfigured,
            IsSuggestion = isSuggestion,
            SelectedItemIds = selectedItemIds
        };

        if (!model.IsConfigured)
        {
            model.Error = photoService.SetupMessage;
            return model;
        }

        try
        {
            var searchTerms = string.Join(' ', new[] { query  }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            model.Looks = await photoService.SearchAsync(searchTerms, tags, currentSeason, currentStyle, cancellationToken);
        }
        catch (HttpRequestException)
        {
            model.Error = "Unsplash could not be reached. Check your connection and access key, then try again.";
        }

        return model;
    }

    private async Task<IActionResult> ToggleSavedLook(LookFormInput input, bool favorite, CancellationToken cancellationToken)
    {
        var state = await stateStore.GetAsync(cancellationToken);
        var collection = favorite ? state.Favorites : state.Wishlist;
        var existing = collection.FindIndex(saved => saved.Id == input.Id);
        if (existing >= 0)
        {
            collection.RemoveAt(existing);
        }
        else if (!string.IsNullOrWhiteSpace(input.Id) && Uri.TryCreate(input.ImageUrl, UriKind.Absolute, out _))
        {
            collection.Insert(0, input.ToStyleLook());
        }

        await stateStore.SaveAsync(state, cancellationToken);
        return RedirectToLocal(input.ReturnUrl);
    }

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));

    private static string BuildSearchQuery(string? query, string gender, string? season, string? style)
    {
        var terms = string.IsNullOrWhiteSpace(query) ? $"{gender} street style outfit fashion" : query.Trim();
        if (!string.IsNullOrWhiteSpace(season) && season != "All season")
        {
            terms += $" {season}";
        }

        if (!string.IsNullOrWhiteSpace(style) && style != "Any style")
        {
            terms += $" {style}";
        }

        return terms;
    }
}