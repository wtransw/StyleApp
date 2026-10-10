using StyleApp.Models;

namespace StyleApp.Services;

public interface IStyleStateStore
{
    Task<PersonalStyleState> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(PersonalStyleState state, CancellationToken cancellationToken = default);
}
