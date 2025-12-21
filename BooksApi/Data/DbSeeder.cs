using BooksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Books.AnyAsync()) return;

        var books = new List<Book>
        {
            new() { Title = "The Pragmatic Programmer", Author = "Andrew Hunt, David Thomas", Year = 1999, Genre = "Software", Isbn = "9780201616224", Description = "Classic book on pragmatic approaches to software development." },
            new() { Title = "Clean Code", Author = "Robert C. Martin", Year = 2008, Genre = "Software", Isbn = "9780132350884", Description = "A handbook of agile software craftsmanship." },
            new() { Title = "Design Patterns", Author = "Erich Gamma et al.", Year = 1994, Genre = "Software", Isbn = "9780201633610", Description = "Elements of reusable object-oriented software." },
            new() { Title = "Deep Learning", Author = "Ian Goodfellow, Yoshua Bengio, Aaron Courville", Year = 2016, Genre = "AI", Isbn = "9780262035613", Description = "Comprehensive textbook on deep learning." },
            new() { Title = "The Lord of the Rings", Author = "J.R.R. Tolkien", Year = 1954, Genre = "Fantasy", Isbn = "9780544003415", Description = "Epic high-fantasy novel." }
            new() { 
                Title = "Elden Ring", 
                Developer = "FromSoftware", 
                Publisher = "Bandai Namco",
                ReleaseDate = "2022-02-25",
                Genre = "Action RPG",
                Price = 59.99m,
                Rating = 4.8f,
                Description = "A new fantasy action RPG where you'll face fearsome enemies in a world full of danger and discovery.",
                ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/1245620/header.jpg",
                StorePageUrl = "https://store.steampowered.com/app/1245620/ELDEN_RING/",
                Tags = new() { "Souls-like", "Open World", "RPG", "Difficult" },
                SupportedPlatforms = new() { "Windows", "PlayStation 4", "PlayStation 5", "Xbox One", "Xbox Series X|S" }
            },
        };

        db.Books.AddRange(books);

        if (await db.Games.AnyAsync()) return;
        var games = new List<Game>
        {
            // Add more sample games...
        };
        await db.Games.AddRangeAsync(games);
        await db.SaveChangesAsync();
    }
}
