using Microsoft.Extensions.Options;
using StyleApp.Models;

namespace StyleApp.Services;

public sealed class ConfiguredStylePhotoService : IStylePhotoService
{
    private readonly IStylePhotoProvider? _provider;
    private readonly string _requestedProvider;

    public ConfiguredStylePhotoService(IEnumerable<IStylePhotoProvider> providers, IOptions<PhotoProviderOptions> options)
    {
        _requestedProvider = options.Value.Provider.Trim();
        _provider = providers.FirstOrDefault(provider =>
            string.Equals(provider.ProviderName, _requestedProvider, StringComparison.OrdinalIgnoreCase));
    }

    public string ProviderName => _provider?.ProviderName ?? _requestedProvider;
    public bool IsConfigured => _provider?.IsConfigured ?? false;
    public string SetupMessage => _provider?.SetupMessage
        ?? $"Photo provider '{_requestedProvider}' is not registered. Implement IStylePhotoProvider and register it in Program.cs.";

    public Task<IReadOnlyList<StyleLook>> SearchAsync(
        string query,
        IReadOnlyList<string> tags,
        string season,
        string style,
        CancellationToken cancellationToken = default) =>
        _provider is null
            ? Task.FromResult<IReadOnlyList<StyleLook>>([])
            : _provider.SearchAsync(query, tags, season, style, cancellationToken);
}
