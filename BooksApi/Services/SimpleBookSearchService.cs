using BooksApi.Data;
using BooksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Services;

public class SimpleBookSearchService : IBookSearchService
{
    private readonly AppDbContext _db;

    public SimpleBookSearchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AiSearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<AiSearchResult>();

        var terms = query
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();

        // Fetch candidate set (simple: all, could be optimized)
        var all = await _db.Books.AsNoTracking().ToListAsync(ct);

        double Score(Book b)
        {
            double s = 0;
            var title = b.Title?.ToLowerInvariant() ?? string.Empty;
            var author = b.Author?.ToLowerInvariant() ?? string.Empty;
            var desc = b.Description?.ToLowerInvariant() ?? string.Empty;
            var genre = b.Genre?.ToLowerInvariant() ?? string.Empty;

            foreach (var t in terms)
            {
                if (title.Contains(t)) s += 3;
                if (author.Contains(t)) s += 2;
                if (genre.Contains(t)) s += 1.5;
                if (desc.Contains(t)) s += 1;
                if (!string.IsNullOrEmpty(b.Isbn) && b.Isbn.Contains(t, StringComparison.OrdinalIgnoreCase)) s += 2.5;
            }

            return s;
        }

        var ranked = all
            .Select(b => new AiSearchResult(b, Score(b)))
            .Where(r => r.Score > 0)
            .OrderByDescending(r => r.Score)
            .Take(50)
            .ToList();

        // Add naive reason text
        for (var i = 0; i < ranked.Count; i++)
        {
            var b = ranked[i].Book;
            var matchedTerms = terms.Where(t =>
                (b.Title?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.Author?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.Genre?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.Description?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (!string.IsNullOrEmpty(b.Isbn) && b.Isbn.Contains(t, StringComparison.OrdinalIgnoreCase))).ToList();

            ranked[i] = ranked[i] with
            {
                Reason = $"Matched on: {string.Join(", ", matchedTerms)}"
            };
        }

        return ranked;
    }
}
