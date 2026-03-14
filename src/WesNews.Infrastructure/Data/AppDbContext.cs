using Microsoft.EntityFrameworkCore;
using WesNews.Domain.Entities;

namespace WesNews.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<FeedSource> FeedSources => Set<FeedSource>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.FullName).HasMaxLength(200);
            entity.Property(u => u.PasswordHash).IsRequired();
        });
        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Url).IsUnique();
            entity.HasIndex(a => new { a.IsRead, a.PublishedAt });
            entity.Property(a => a.Title).IsRequired().HasMaxLength(500);
            entity.Property(a => a.Url).IsRequired().HasMaxLength(2000);
            entity.Property(a => a.Summary).HasMaxLength(2000);
            entity.HasOne(a => a.FeedSource)
                  .WithMany(f => f.Articles)
                  .HasForeignKey(a => a.FeedSourceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeedSource>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => f.Url).IsUnique();
            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            entity.Property(f => f.Url).IsRequired().HasMaxLength(2000);
        });
    }
}
