using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;
using WesNews.Infrastructure.Data;

namespace WesNews.Infrastructure.Seed;

public class FeedSeeder(AppDbContext context, ILogger<FeedSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(cancellationToken);
        await SeedFeedsAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        bool hasAdmin = await context.Users.AnyAsync(u => u.Username == "wes_admin", cancellationToken);
        if (!hasAdmin)
        {
            User admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "wes_admin",
                FullName = "Wesley Admin",
                Email = "admin@wesnews.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("%Dn42jdT@8X8Zn"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(admin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded admin user: wes_admin");
        }
    }

    private async Task SeedFeedsAsync(CancellationToken cancellationToken)
    {
        bool hasFeeds = await context.FeedSources.AnyAsync(cancellationToken);

        if (hasFeeds)
        {
            return;
        }

        List<FeedSource> seeds = GetDefaultFeeds();
        await context.FeedSources.AddRangeAsync(seeds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} default feed sources", seeds.Count);
    }

    private static List<FeedSource> GetDefaultFeeds()
    {
        return new List<FeedSource>
        {
            new FeedSource { Id = Guid.NewGuid(), Name = ".NET Blog", Url = "https://devblogs.microsoft.com/dotnet/feed/", Category = Category.DotNet, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Scott Hanselman", Url = "https://www.hanselman.com/blog/feed", Category = Category.DotNet, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Andrew Lock", Url = "https://andrewlock.net/rss.xml", Category = Category.DotNet, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Khalid Abuhakmeh", Url = "https://khalidabuhakmeh.com/feed.xml", Category = Category.DotNet, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Steven Giesel", Url = "https://steven-giesel.com/feed.rss", Category = Category.DotNet, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "OpenAI Blog", Url = "https://openai.com/blog/rss.xml", Category = Category.AI, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Google AI Blog", Url = "https://blog.google/technology/ai/rss/", Category = Category.AI, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "HuggingFace Blog", Url = "https://huggingface.co/blog/feed.xml", Category = Category.AI, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Microsoft AI Blog", Url = "https://blogs.microsoft.com/ai/feed/", Category = Category.AI, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "DeepLearning.AI", Url = "https://www.deeplearning.ai/feed/", Category = Category.AI, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Martin Fowler", Url = "https://martinfowler.com/feed.atom", Category = Category.Architecture, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "InfoQ Architecture", Url = "https://www.infoq.com/architecture-design/rss/", Category = Category.Architecture, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Pragmatic Engineer", Url = "https://newsletter.pragmaticengineer.com/feed", Category = Category.Architecture, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "GitHub Blog", Url = "https://github.blog/feed/", Category = Category.DevOps, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Azure Blog", Url = "https://azure.microsoft.com/en-us/blog/feed/", Category = Category.DevOps, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Kubernetes Blog", Url = "https://kubernetes.io/feed.xml", Category = Category.DevOps, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Hacker News", Url = "https://hnrss.org/frontpage", Category = Category.General, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "The New Stack", Url = "https://thenewstack.io/feed/", Category = Category.General, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "Stack Overflow Blog", Url = "https://stackoverflow.blog/feed/", Category = Category.General, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "InfoQ", Url = "https://www.infoq.com/rss/", Category = Category.General, IsActive = true },
            new FeedSource { Id = Guid.NewGuid(), Name = "GitHub Trending (All)", Url = "https://mshibanami.github.io/GitHubTrendingRSS/daily/unknown.xml", Category = Category.GitHubTrends, IsActive = true, MaxItemsPerFetch = 10 },
            new FeedSource { Id = Guid.NewGuid(), Name = "GitHub Trending (.NET / C#)", Url = "https://mshibanami.github.io/GitHubTrendingRSS/daily/c%23.xml", Category = Category.GitHubTrends, IsActive = true, MaxItemsPerFetch = 10 },
            new FeedSource { Id = Guid.NewGuid(), Name = "GitHub Trending (Hacking / Security)", Url = "https://github.com/topics/hacking.atom", Category = Category.GitHubTrends, IsActive = true, MaxItemsPerFetch = 10 },
        };
    }
}
