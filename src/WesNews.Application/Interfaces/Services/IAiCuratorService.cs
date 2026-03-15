namespace WesNews.Application.Interfaces.Services;

public interface IAiCuratorService
{
    Task CurateAsync(CancellationToken cancellationToken = default);
}
