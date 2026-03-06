using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;
using WesNews.Infrastructure.Configuration;

namespace WesNews.Infrastructure.Services;

public class DigestEmailService : IDigestEmailService
{
    private readonly IResend _resend;
    private readonly DigestEmailOptions _options;
    private readonly ILogger<DigestEmailService> _logger;

    public DigestEmailService(IResend resend, IOptions<DigestEmailOptions> options, ILogger<DigestEmailService> logger)
    {
        _resend = resend;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default)
    {
        string html = BuildPreviewHtml(articles);

        EmailMessage message = new EmailMessage
        {
            From = _options.FromEmail,
            Subject = $"WesNews Digest — {DateTime.UtcNow:dd/MM/yyyy}",
            HtmlBody = html
        };

        message.To.Add(_options.ToEmail);

        await _resend.EmailSendAsync(message, cancellationToken);

        _logger.LogInformation("Digest email sent to {ToEmail}", _options.ToEmail);
    }

    public string BuildPreviewHtml(IEnumerable<NewsArticle> articles)
    {
        IReadOnlyList<NewsArticle> articleList = articles.ToList().AsReadOnly();

        if (articleList.Count == 0)
        {
            return "<p>No unread articles found for today's digest.</p>";
        }

        var grouped = articleList
            .GroupBy(a => a.FeedSource.Category)
            .OrderBy(g => g.Key);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(BuildHtmlHeader());

        foreach (var group in grouped)
        {
            string categoryName = group.Key.ToString();
            sb.Append($"<h2 style=\"color:#2563eb;border-bottom:2px solid #e5e7eb;padding-bottom:8px;\">{categoryName}</h2>");

            foreach (NewsArticle article in group)
            {
                sb.Append($"""
                    <div style="margin-bottom:20px;padding:16px;background:#f9fafb;border-radius:8px;border-left:4px solid #2563eb;">
                        <h3 style="margin:0 0 8px;font-size:16px;">
                            <a href="{article.Url}" style="color:#1e40af;text-decoration:none;">{article.Title}</a>
                        </h3>
                        <p style="margin:0 0 8px;color:#6b7280;font-size:13px;">{article.FeedSource.Name} · {article.PublishedAt:dd/MM/yyyy HH:mm}</p>
                        <p style="margin:0;color:#374151;font-size:14px;">{TruncateSummary(article.Summary, 200)}</p>
                    </div>
                    """);
            }
        }

        sb.Append(BuildHtmlFooter());
        return sb.ToString();
    }

    private static string BuildHtmlHeader()
    {
        string date = DateTime.UtcNow.ToString("dddd, MMMM dd, yyyy");
        return $"""
            <!DOCTYPE html>
            <html><head><meta charset="utf-8"></head>
            <body style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;max-width:700px;margin:0 auto;padding:24px;color:#111827;">
            <h1 style="color:#111827;font-size:24px;margin-bottom:4px;">WesNews Daily Digest</h1>
            <p style="color:#6b7280;margin-bottom:32px;">{date}</p>
            """;
    }

    private static string BuildHtmlFooter()
    {
        return """
            <hr style="border:none;border-top:1px solid #e5e7eb;margin:32px 0;">
            <p style="color:#9ca3af;font-size:12px;text-align:center;">WesNews — Your personal tech news aggregator</p>
            </body></html>
            """;
    }

    private static string TruncateSummary(string summary, int maxLength)
    {
        return summary.Length <= maxLength ? summary : summary[..maxLength] + "...";
    }
}
