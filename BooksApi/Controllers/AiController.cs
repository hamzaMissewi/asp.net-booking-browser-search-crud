using BooksApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BooksApi.Controllers;

public record AiSearchRequest(string Query);

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IBookSearchService _search;

    public AiController(IBookSearchService search)
    {
        _search = search;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] AiSearchRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query is required");

        var results = await _search.SearchAsync(request.Query, ct);
        return Ok(results);
    }
}
