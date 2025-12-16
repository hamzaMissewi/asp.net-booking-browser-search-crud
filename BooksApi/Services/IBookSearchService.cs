using BooksApi.Models;

namespace BooksApi.Services;

public record AiSearchResult(Book Book, double Score, string? Reason = null);

public interface IBookSearchService
{
    Task<IReadOnlyList<AiSearchResult>> SearchAsync(string query, CancellationToken ct = default);
}
