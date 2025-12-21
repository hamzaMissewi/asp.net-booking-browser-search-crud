// DTOs/GameDto.cs
namespace BooksApi.DTOs;

public class GameDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice => 
        DiscountPercentage.HasValue ? Price * (100 - DiscountPercentage.Value) / 100 : null;
    public decimal? DiscountPercentage { get; set; }
    public float? Rating { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? SupportedPlatforms { get; set; }
}