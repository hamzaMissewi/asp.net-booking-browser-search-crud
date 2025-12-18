using BooksApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Services;

/// <summary>
/// Fallback chat service when OpenAI is not configured
/// </summary>
public class SimpleChatService : IChatService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SimpleChatService> _logger;

    public SimpleChatService(AppDbContext db, ILogger<SimpleChatService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ChatResponse> ChatAsync(List<ChatMessage> messages, CancellationToken ct = default)
    {
        var lastMessage = messages.LastOrDefault()?.Content?.ToLowerInvariant() ?? "";
        
        _logger.LogInformation("Processing chat with simple service");

        // Extract keywords
        var keywords = ExtractKeywords(lastMessage);
        
        if (keywords.Count == 0)
        {
            return new ChatResponse("I can help you find books! Try asking me about genres like 'science fiction', 'mystery', or author names.");
        }

        // Search books
        var books = await _db.Books
            .AsNoTracking()
            .Where(b => keywords.Any(k => 
                (b.Title != null && b.Title.ToLower().Contains(k)) ||
                (b.Author != null && b.Author.ToLower().Contains(k)) ||
                (b.Genre != null && b.Genre.ToLower().Contains(k)) ||
                (b.Description != null && b.Description.ToLower().Contains(k))))
            .Take(5)
            .ToListAsync(ct);

        if (!books.Any())
        {
            return new ChatResponse($"I couldn't find any books matching '{string.Join(", ", keywords)}'. Try different keywords or browse the full catalog!");
        }

        var response = $"I found {books.Count} book(s) that might interest you:\n\n";
        var bookIds = new List<int>();

        foreach (var book in books)
        {
            bookIds.Add(book.Id);
            response += $"📚 **{book.Title}** by {book.Author}";
            if (!string.IsNullOrEmpty(book.Genre))
                response += $" ({book.Genre})";
            if (book.Year.HasValue)
                response += $" - {book.Year}";
            response += $"\n   {(book.Description?.Length > 100 ? book.Description.Substring(0, 100) + "..." : book.Description ?? "No description available")}\n\n";
        }

        return new ChatResponse(response, bookIds);
    }

    private List<string> ExtractKeywords(string text)
    {
        var commonWords = new HashSet<string> 
        { 
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
            "book", "books", "show", "find", "search", "looking", "want", "need",
            "me", "i", "about", "any", "some", "recommend", "recommendation"
        };

        return text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(w => w.Length > 2 && !commonWords.Contains(w))
            .Distinct()
            .ToList();
    }
}
