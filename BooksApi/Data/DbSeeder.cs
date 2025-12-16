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
        };

        db.Books.AddRange(books);
        await db.SaveChangesAsync();
    }
}
