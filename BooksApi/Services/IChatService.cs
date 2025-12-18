namespace BooksApi.Services;

public record ChatMessage(string Role, string Content);
public record ChatResponse(string Message, List<int>? RecommendedBookIds = null);

public interface IChatService
{
    Task<ChatResponse> ChatAsync(List<ChatMessage> messages, CancellationToken ct = default);
}
