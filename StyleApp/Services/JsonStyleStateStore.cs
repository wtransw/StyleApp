using System.Text.Json;
using StyleApp.Models;

namespace StyleApp.Services;

public sealed class JsonStyleStateStore : IStyleStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonStyleStateStore(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "style-state.json");
    }

    public async Task<PersonalStyleState> GetAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_filePath))
            {
                return new PersonalStyleState();
            }

            await using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<PersonalStyleState>(stream, JsonOptions, cancellationToken)
                ?? new PersonalStyleState();
        }
        catch (JsonException)
        {
            return new PersonalStyleState();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(PersonalStyleState state, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
