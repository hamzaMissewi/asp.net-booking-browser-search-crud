using BooksApi.Data;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System.Text.Json;

namespace BooksApi.Services;

public class OpenAIChatService : IChatService
{
    private readonly AppDbContext _db;
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAIChatService> _logger;

    public OpenAIChatService(AppDbContext db, IConfiguration config, ILogger<OpenAIChatService> logger)
    {
        _db = db;
        _logger = logger;
        
        var apiKey = config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured. Set OpenAI:ApiKey in appsettings.json or OPENAI_API_KEY environment variable.");
        }
        
        var model = config["OpenAI:Model"] ?? "gpt-4o-mini";
        _chatClient = new ChatClient(model, apiKey);
    }

    public async Task<ChatResponse> ChatAsync(List<ChatMessage> messages, CancellationToken ct = default)
    {
        try
        {
            // Get all books for context
            var allBooks = await _db.Books
                .AsNoTracking()
                .Select(b => new { b.Id, b.Title, b.Author, b.Genre, b.Year, b.Description })
                .ToListAsync(ct);

            var booksContext = JsonSerializer.Serialize(allBooks, new JsonSerializerOptions 
            { 
                WriteIndented = false 
            });

            // Build system prompt with book catalog
            var systemPrompt = $@"You are a helpful book recommendation assistant. You have access to a catalog of books.
Your role is to:
1. Help users find books based on their preferences
2. Provide book recommendations
3. Answer questions about books in the catalog
4. Be conversational and friendly

Here is the current book catalog:
{booksContext}

When recommending books, reference them by their ID numbers. Keep responses concise but informative.
If users ask about books not in the catalog, politely explain what's available instead.";

            // Convert messages to OpenAI format
            var chatMessages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage(systemPrompt)
            };

            foreach (var msg in messages)
            {
                chatMessages.Add(msg.Role.ToLower() switch
                {
                    "user" => new UserChatMessage(msg.Content),
                    "assistant" => new AssistantChatMessage(msg.Content),
                    _ => new UserChatMessage(msg.Content)
                });
            }

            // Call OpenAI
            var completion = await _chatClient.CompleteChatAsync(chatMessages, cancellationToken: ct);
            var responseText = completion.Value.Content[0].Text;

            // Extract book IDs from response (simple regex pattern)
            var bookIds = ExtractBookIds(responseText, allBooks.Select(b => b.Id).ToHashSet());

            _logger.LogInformation("Chat completed. Recommended {Count} books", bookIds.Count);

            return new ChatResponse(responseText, bookIds.Any() ? bookIds : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chat completion");
            throw;
        }
    }

    private List<int> ExtractBookIds(string text, HashSet<int> validIds)
    {
        var ids = new HashSet<int>();
        
        // Look for patterns like "ID: 5", "book #3", "book 7", "[5]", etc.
        var patterns = new[]
        {
            @"ID:\s*(\d+)",
            @"book\s+#?(\d+)",
            @"\[(\d+)\]",
            @"ID\s+(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(text, pattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out var id) && validIds.Contains(id))
                {
                    ids.Add(id);
                }
            }
        }

        return ids.OrderBy(x => x).ToList();
    }
}
