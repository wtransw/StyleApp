using StyleApp.Models;

namespace StyleApp.Services;

public interface IStylePhotoService
{
    string ProviderName { get; }
    bool IsConfigured { get; }
    string SetupMessage { get; }
    Task<IReadOnlyList<StyleLook>> SearchAsync(string query, IReadOnlyList<string> tags, string season, string style, CancellationToken cancellationToken = default);
}
