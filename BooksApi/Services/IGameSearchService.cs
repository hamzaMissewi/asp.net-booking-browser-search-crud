// Services/IGameSearchService.cs
using BooksApi.DTOs;

namespace BooksApi.Services;

public interface IGameSearchService
{
    Task<IReadOnlyList<GameSearchResult>> SearchAsync(string query, CancellationToken ct = default);
}

public record GameSearchResult(GameDto Game, double Score, string? Reason = null);