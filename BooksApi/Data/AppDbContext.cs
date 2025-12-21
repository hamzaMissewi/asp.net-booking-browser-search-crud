using BooksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

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


      public DbSet<Game> Games => Set<Game>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(g =>
        {
            g.Property(p => p.Title).IsRequired().HasMaxLength(200);
            g.Property(p => p.Developer).IsRequired().HasMaxLength(150);
            g.Property(p => p.Publisher).HasMaxLength(150);
            g.Property(p => p.Description).HasMaxLength(1000);
            g.Property(p => p.Genre).HasMaxLength(100);
            g.Property(p => p.Price).HasColumnType("decimal(18,2)");
            g.Property(p => p.DiscountPercentage).HasColumnType("decimal(5,2)");
            
            // Indexes for common queries
            g.HasIndex(p => p.Title);
            g.HasIndex(p => p.Developer);
            g.HasIndex(p => p.ReleaseDate);
            g.HasIndex(p => p.Price);
            g.HasIndex(p => p.Rating);
            g.HasIndex(p => p.Genre);
        });
    }

}