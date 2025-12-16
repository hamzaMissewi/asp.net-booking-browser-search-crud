using System.ComponentModel.DataAnnotations;

namespace BooksApi.Models;

public class Book
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(13)]
    public string? Isbn { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? Year { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }
}
