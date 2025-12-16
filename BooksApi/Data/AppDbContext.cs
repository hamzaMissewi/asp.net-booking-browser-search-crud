using BooksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(b =>
        {
            b.Property(p => p.Title).IsRequired().HasMaxLength(200);
            b.Property(p => p.Author).IsRequired().HasMaxLength(150);
            b.Property(p => p.Isbn).HasMaxLength(13);
            b.Property(p => p.Description).HasMaxLength(1000);
            b.Property(p => p.Genre).HasMaxLength(100);
            b.HasIndex(p => new { p.Title, p.Author });
        });
    }
}