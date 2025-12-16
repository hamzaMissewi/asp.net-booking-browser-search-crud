using BooksApi.Data;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? genre,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        var query = db.Books.AsQueryable();

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
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> GetById(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book == null) return NotFound();
        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create([FromBody] Book input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        db.Books.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Book>> Update(int id, [FromBody] Book input)
    {
        if (id != input.Id) return BadRequest("ID mismatch");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var exists = await db.Books.AnyAsync(b => b.Id == id);
        if (!exists) return NotFound();

        db.Entry(input).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return Ok(input);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book == null) return NotFound();
        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
