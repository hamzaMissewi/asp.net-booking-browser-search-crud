// Controllers/GamesController.cs
using AutoMapper;
using BooksApi.Data;
using BooksApi.DTOs;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public GamesController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<GameDto>>> GetGames(
        [FromQuery] string? search,
        [FromQuery] string? genre,
        [FromQuery] string? developer,
        [FromQuery] string? sortBy = "title",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        var query = _db.Games.AsNoTracking().AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(g => 
                g.Title.ToLower().Contains(term) ||
                g.Developer.ToLower().Contains(term) ||
                (g.Tags != null && g.Tags.Any(t => t.ToLower().Contains(term))));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(g => g.Genre == genre);
        }

        if (!string.IsNullOrWhiteSpace(developer))
        {
            query = query.Where(g => g.Developer == developer);
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "title" => sortOrder?.ToLower() == "desc" 
                ? query.OrderByDescending(g => g.Title) 
                : query.OrderBy(g => g.Title),
            "releasedate" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(g => g.ReleaseDate)
                : query.OrderBy(g => g.ReleaseDate),
            "price" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(g => g.Price)
                : query.OrderBy(g => g.Price),
            "rating" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(g => g.Rating)
                : query.OrderBy(g => g.Rating),
            _ => query.OrderBy(g => g.Title)
        };

        // Pagination
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var response = new PagedResponse<GameDto>
        {
            Items = _mapper.Map<List<GameDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetGame(int id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();

        return Ok(_mapper.Map<GameDto>(game));
    }

    [HttpGet("genres")]
    public async Task<ActionResult<List<string>>> GetGenres()
    {
        var genres = await _db.Games
            .Where(g => !string.IsNullOrEmpty(g.Genre))
            .Select(g => g.Genre!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

        return Ok(genres);
    }

    [HttpGet("developers")]
    public async Task<ActionResult<List<string>>> GetDevelopers()
    {
        var developers = await _db.Games
            .Select(g => g.Developer)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        return Ok(developers);
    }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}