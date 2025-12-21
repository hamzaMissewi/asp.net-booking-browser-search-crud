// Models/Game.cs
using System.ComponentModel.DataAnnotations;

namespace BooksApi.Models;

public class Game
{
    public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [Required, MaxLength(150)]
    public string Developer { get; set; } = string.Empty;
    [MaxLength(150)]
    public string Publisher { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? ReleaseDate { get; set; } // YYYY-MM-DD format
    [MaxLength(1000)]
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? Genre { get; set; }
    [Range(0, 1000)]
    public decimal Price { get; set; }
    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }
    [Range(0, 5)]
    public float? Rating { get; set; }
    [Url]
    public string? ImageUrl { get; set; }
    [Url]
    public string? StorePageUrl { get; set; }
    public List<string>? Tags { get; set; } = new();
    public List<string>? SupportedPlatforms { get; set; } = new();
}