using BooksApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BooksApi.Controllers;

public record AiSearchRequest(string Query);

[ApiController]
[Route("api/[controller]")]
public class AiController(IBookSearchService search) : ControllerBase
{
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] AiSearchRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query is required");

        var results = await search.SearchAsync(request.Query, ct);
        return Ok(results);
    }
}
