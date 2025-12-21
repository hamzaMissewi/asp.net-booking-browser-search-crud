// Services/GameSearchService.cs
using BooksApi.Data;
using BooksApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Services;

public class GameSearchService : IGameSearchService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public GameSearchService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<GameSearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<GameSearchResult>();

        var games = await _db.Games
            .AsNoTracking()
            .Where(g => 
                EF.Functions.Like(g.Title, $"%{query}%") ||
                EF.Functions.Like(g.Developer, $"%{query}%") ||
                EF.Functions.Like(g.Description, $"%{query}%") ||
                EF.Functions.Like(g.Genre, $"%{query}%") ||
                (g.Tags != null && g.Tags.Any(t => EF.Functions.Like(t, $"%{query}%")))
            )
            .ToListAsync(ct);

        var results = new List<GameSearchResult>();
        foreach (var game in games)
        {
            var score = CalculateRelevanceScore(game, query);
            if (score > 0)
            {
                results.Add(new GameSearchResult(_mapper.Map<GameDto>(game), score));
            }
        }

        return results
            .OrderByDescending(r => r.Score)
            .ToList();
    }

    private static double CalculateRelevanceScore(Game game, string query)
    {
        var queryTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(term => term.ToLower())
            .ToList();

        double score = 0;

        // Title matches are most important
        if (!string.IsNullOrEmpty(game.Title))
        {
            var title = game.Title.ToLower();
            score += queryTerms.Count(term => title.Contains(term)) * 10;
        }

        // Developer matches are also important
        if (!string.IsNullOrEmpty(game.Developer))
        {
            var developer = game.Developer.ToLower();
            score += queryTerms.Count(term => developer.Contains(term)) * 8;
        }

        // Tag matches are moderately important
        if (game.Tags != null)
        {
            score += game.Tags
                .SelectMany(tag => queryTerms
                    .Where(term => tag.ToLower().Contains(term)))
                .Count() * 5;
        }

        // Description matches are less important
        if (!string.IsNullOrEmpty(game.Description))
        {
            var description = game.Description.ToLower();
            score += queryTerms.Count(term => description.Contains(term)) * 2;
        }

        return score;
    }
}