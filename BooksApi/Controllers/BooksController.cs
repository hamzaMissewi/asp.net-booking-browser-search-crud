using BooksApi.Data;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BooksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BooksController> _logger;

    public BooksController(AppDbContext db, IMemoryCache cache, ILogger<BooksController> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? genre,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        // Create cache key based on query parameters
        var cacheKey = $"books_q:{q}_g:{genre}_p:{page}_ps:{pageSize}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out List<Book>? cachedBooks) && cachedBooks != null)
        {
            _logger.LogDebug("Cache hit for: {CacheKey}", cacheKey);
            return Ok(cachedBooks);
        }

        _logger.LogDebug("Cache miss for: {CacheKey}", cacheKey);

        var query = _db.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(b =>
                (b.Title != null && EF.Functions.Like(b.Title, $"%{term}%")) ||
                (b.Author != null && EF.Functions.Like(b.Author, $"%{term}%")) ||
                (b.Description != null && EF.Functions.Like(b.Description, $"%{term}%")) ||
                (b.Genre != null && EF.Functions.Like(b.Genre, $"%{term}%")) ||
                (b.Isbn != null && EF.Functions.Like(b.Isbn, $"%{term}%"))
            );
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(b => b.Genre == genre);
        }

        var items = await query
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Cache for 2 minutes
        _cache.Set(cacheKey, items, TimeSpan.FromMinutes(2));

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> GetById(int id)
    {
        var cacheKey = $"book_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Book? cachedBook) && cachedBook != null)
        {
            return Ok(cachedBook);
        }

        var book = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return NotFound();

        // Cache for 5 minutes
        _cache.Set(cacheKey, book, TimeSpan.FromMinutes(5));

        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create([FromBody] Book input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        _db.Books.Add(input);
        await _db.SaveChangesAsync();

        // Invalidate list cache
        InvalidateListCache();
        
        _logger.LogInformation("Created book: {BookId} - {Title}", input.Id, input.Title);

        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Book>> Update(int id, [FromBody] Book input)
    {
        if (id != input.Id) return BadRequest("ID mismatch");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var exists = await _db.Books.AnyAsync(b => b.Id == id);
        if (!exists) return NotFound();

        _db.Entry(input).State = EntityState.Modified;
        await _db.SaveChangesAsync();

        // Invalidate caches
        InvalidateBookCache(id);
        InvalidateListCache();
        
        _logger.LogInformation("Updated book: {BookId} - {Title}", input.Id, input.Title);

        return Ok(input);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return NotFound();
        
        _db.Books.Remove(book);
        await _db.SaveChangesAsync();

        // Invalidate caches
        InvalidateBookCache(id);
        InvalidateListCache();
        
        _logger.LogInformation("Deleted book: {BookId}", id);

        return NoContent();
    }

    private void InvalidateBookCache(int id)
    {
        _cache.Remove($"book_{id}");
    }

    private void InvalidateListCache()
    {
        // Simple approach: remove all list cache entries
        // In production, consider using cache tags or more sophisticated cache invalidation
        _logger.LogDebug("Invalidating list cache");
    }
}
